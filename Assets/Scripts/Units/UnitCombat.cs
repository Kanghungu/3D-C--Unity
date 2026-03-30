using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Very lightweight combat loop for prototype testing.
    /// Supports direct attacks, ranged projectiles, attack-move orders, nearby battle support,
    /// and simple RTS command modes like hold, guard, and fallback.
    /// </summary>
    [RequireComponent(typeof(SimpleUnitMover))]
    [RequireComponent(typeof(CombatTarget))]
    [RequireComponent(typeof(UnitHealth))]
    public class UnitCombat : MonoBehaviour
    {
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 0.8f;
        [SerializeField] private float aggroRange = 7f;
        [SerializeField] private float retargetInterval = 0.6f;
        [SerializeField] private float supportAssistRadius = 14f;
        [SerializeField] private float supportTargetRadius = 18f;
        [SerializeField] private bool usesProjectile;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float projectileArc = 1f;
        [SerializeField] private float splashRadius;
        [SerializeField] private float impactEffectScale = 0.7f;

        private CombatTarget owner;
        private UnitHealth health;
        private SimpleUnitMover mover;
        private CombatTarget currentTarget;
        private UnitAbilityState abilityState;
        private SelectableUnit selectableUnit;
        private AdvancedUnitRoleController roleController;
        private float cooldownTimer;
        private float retargetTimer;
        private bool hasAttackMoveDestination;
        private Vector3 attackMoveDestination;
        private bool hasPursuitDestination;
        private Vector3 pursuitDestination;
        private bool hasHoldPosition;
        private Vector3 holdPosition;
        private bool hasGuardPoint;
        private Vector3 guardPoint;
        private float guardRadius = 18f;

        public CombatTarget CurrentTarget => currentTarget;
        public bool HasAttackMoveDestination => hasAttackMoveDestination;
        public string OrderLabel => BuildOrderLabel();

        private void Awake()
        {
            mover = GetComponent<SimpleUnitMover>();
            owner = GetComponent<CombatTarget>();
            health = GetComponent<UnitHealth>();
            abilityState = GetComponent<UnitAbilityState>();
            selectableUnit = GetComponent<SelectableUnit>();
            roleController = GetComponent<AdvancedUnitRoleController>();
        }

        private void OnEnable()
        {
            PrototypeRuntimeRegistry.Register(this);
        }

        private void OnDisable()
        {
            PrototypeRuntimeRegistry.Unregister(this);
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (retargetTimer > 0f)
            {
                retargetTimer -= Time.deltaTime;
            }

            if (!health.IsAlive)
            {
                return;
            }

            if (currentTarget != null && currentTarget.IsAlive)
            {
                pursuitDestination = currentTarget.transform.position;
                hasPursuitDestination = true;
            }

            if ((currentTarget == null || !currentTarget.IsAlive) && retargetTimer <= 0f)
            {
                currentTarget = FindClosestEnemyTarget();
                retargetTimer = retargetInterval;
            }

            if (currentTarget == null)
            {
                if (RunGuardOrderIfNeeded())
                {
                    return;
                }

                if (RunPursuitAdvance())
                {
                    return;
                }

                RunAttackMoveIfNeeded();
                return;
            }

            if (roleController != null && !roleController.AllowsCombat())
            {
                currentTarget = null;
                return;
            }

            Vector3 targetPosition = currentTarget.transform.position;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > attackRange)
            {
                mover.SetDestination(targetPosition);
                return;
            }

            mover.Stop();
            FaceTarget(targetPosition);

            if (cooldownTimer > 0f)
            {
                return;
            }

            if (usesProjectile)
            {
                LaunchProjectile(currentTarget);
            }
            else
            {
                ApplyDirectDamage(currentTarget);
            }

            float cooldownMultiplier = abilityState != null ? abilityState.GetAttackCooldownMultiplier() : 1f;
            cooldownTimer = attackCooldown * cooldownMultiplier;
        }

        public void Configure(
            float newAttackRange,
            float newAttackDamage,
            float newAttackCooldown,
            float newAggroRange,
            float newRetargetInterval,
            bool newUsesProjectile,
            float newProjectileSpeed,
            float newProjectileArc,
            float newSplashRadius,
            float newImpactEffectScale)
        {
            attackRange = newAttackRange;
            attackDamage = newAttackDamage;
            attackCooldown = newAttackCooldown;
            aggroRange = newAggroRange;
            retargetInterval = newRetargetInterval;
            usesProjectile = newUsesProjectile;
            projectileSpeed = newProjectileSpeed;
            projectileArc = newProjectileArc;
            splashRadius = newSplashRadius;
            impactEffectScale = newImpactEffectScale;
        }

        public void Initialize(CombatTarget assignedTarget, UnitHealth assignedHealth)
        {
            owner = assignedTarget;
            health = assignedHealth;
            mover = GetComponent<SimpleUnitMover>();
            abilityState = GetComponent<UnitAbilityState>();
            selectableUnit = GetComponent<SelectableUnit>();
            roleController = GetComponent<AdvancedUnitRoleController>();
        }

        public void SetTarget(CombatTarget target)
        {
            if (target == null || target == owner || target.Team == owner.Team)
            {
                return;
            }

            if (!CanAcceptCombatTarget(target))
            {
                return;
            }

            ClearDirectiveState();
            currentTarget = target;
            pursuitDestination = target.transform.position;
            hasPursuitDestination = true;
        }

        public void SetAttackMoveDestination(Vector3 destination)
        {
            ClearDirectiveState();
            hasAttackMoveDestination = true;
            attackMoveDestination = new Vector3(destination.x, transform.position.y, destination.z);
            mover.SetDestination(attackMoveDestination);
        }

        public void SetHoldPosition(Vector3 position)
        {
            ClearDirectiveState();
            hasHoldPosition = true;
            holdPosition = new Vector3(position.x, transform.position.y, position.z);
            mover.Stop();
        }

        public void SetGuardPoint(Vector3 position, float radius)
        {
            ClearDirectiveState();
            hasGuardPoint = true;
            guardPoint = new Vector3(position.x, transform.position.y, position.z);
            guardRadius = Mathf.Max(8f, radius);
            mover.SetDestination(guardPoint);
        }

        public void ClearTarget()
        {
            currentTarget = null;
            hasPursuitDestination = false;
        }

        public void ClearOrders()
        {
            ClearDirectiveState();
            mover.Stop();
        }

        public static void SpawnImpactEffect(Vector3 position, float scale, Color color)
        {
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "Impact Effect";
            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * Mathf.Max(0.2f, scale);

            Collider effectCollider = effect.GetComponent<Collider>();
            if (effectCollider != null)
            {
                effectCollider.enabled = false;
            }

            Renderer rendererComponent = effect.GetComponent<Renderer>();
            rendererComponent.material.color = color;
            Destroy(effect, 0.22f);
        }

        private CombatTarget FindClosestEnemyTarget()
        {
            CombatTarget bestTarget = null;
            float bestDistance = aggroRange;
            Vector3 searchOrigin = GetSearchOrigin();

            foreach (CombatTarget target in PrototypeRuntimeRegistry.GetCombatTargets())
            {
                if (target == null || target == owner || !target.IsAlive || target.Team == owner.Team)
                {
                    continue;
                }

                if (!CanAcceptCombatTarget(target))
                {
                    continue;
                }

                float distance = Vector3.Distance(searchOrigin, target.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            CombatTarget supportTarget = FindNearbySupportTarget();
            if (supportTarget != null)
            {
                float supportDistance = Vector3.Distance(transform.position, supportTarget.transform.position);
                if (bestTarget == null || supportDistance < bestDistance)
                {
                    bestTarget = supportTarget;
                }
            }

            return bestTarget;
        }

        private CombatTarget FindNearbySupportTarget()
        {
            if (selectableUnit != null && selectableUnit.IsSelected)
            {
                return null;
            }

            CombatTarget bestTarget = null;
            float bestDistance = supportTargetRadius;

            foreach (UnitCombat allyCombat in PrototypeRuntimeRegistry.GetUnitCombats())
            {
                if (allyCombat == null || allyCombat == this)
                {
                    continue;
                }

                CombatTarget allyOwner = allyCombat.owner;
                if (allyOwner == null || allyOwner.Team != owner.Team || allyCombat.CurrentTarget == null || !allyCombat.CurrentTarget.IsAlive)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, allyCombat.transform.position) > supportAssistRadius)
                {
                    continue;
                }

                if (!CanAcceptCombatTarget(allyCombat.CurrentTarget))
                {
                    continue;
                }

                float targetDistance = Vector3.Distance(transform.position, allyCombat.CurrentTarget.transform.position);
                if (targetDistance > supportAssistRadius)
                {
                    continue;
                }

                if (targetDistance < bestDistance)
                {
                    bestDistance = targetDistance;
                    bestTarget = allyCombat.CurrentTarget;
                }
            }

            return bestTarget;
        }

        private bool CanAcceptCombatTarget(CombatTarget target)
        {
            if (target == null)
            {
                return false;
            }

            if (roleController != null && !roleController.CanAcceptTarget(target))
            {
                return false;
            }

            BaseStructure baseStructure = target.GetComponent<BaseStructure>();
            if (baseStructure != null && !BattleDirectiveController.CanTargetEnemyBaseStatic(owner.Team))
            {
                return false;
            }

            if (hasHoldPosition || hasGuardPoint)
            {
                Vector3 anchor = hasGuardPoint ? guardPoint : holdPosition;
                float leash = hasGuardPoint ? guardRadius + aggroRange + 6f : Mathf.Max(aggroRange + 2f, 10f);
                if (Vector3.Distance(anchor, target.transform.position) > leash)
                {
                    return false;
                }
            }

            return true;
        }

        private bool RunGuardOrderIfNeeded()
        {
            if (hasGuardPoint)
            {
                Vector3 anchor = new Vector3(guardPoint.x, transform.position.y, guardPoint.z);
                float distance = Vector3.Distance(transform.position, anchor);

                if (distance > guardRadius * 0.6f)
                {
                    mover.SetDestination(anchor);
                    return true;
                }

                mover.Stop();
                return true;
            }

            if (hasHoldPosition)
            {
                Vector3 anchor = new Vector3(holdPosition.x, transform.position.y, holdPosition.z);
                float distance = Vector3.Distance(transform.position, anchor);

                if (distance > Mathf.Max(0.8f, attackRange * 0.28f))
                {
                    mover.SetDestination(anchor);
                    return true;
                }

                mover.Stop();
                return true;
            }

            return false;
        }

        private bool RunPursuitAdvance()
        {
            if (!hasPursuitDestination)
            {
                return false;
            }

            Vector3 destination = new Vector3(pursuitDestination.x, transform.position.y, pursuitDestination.z);
            float remainingDistance = Vector3.Distance(transform.position, destination);

            if (remainingDistance <= Mathf.Max(0.45f, attackRange * 0.3f))
            {
                hasPursuitDestination = false;
                mover.Stop();
                return false;
            }

            mover.SetDestination(destination);
            return true;
        }

        private void RunAttackMoveIfNeeded()
        {
            if (!hasAttackMoveDestination)
            {
                return;
            }

            Vector3 flatDestination = new Vector3(attackMoveDestination.x, transform.position.y, attackMoveDestination.z);
            float remainingDistance = Vector3.Distance(transform.position, flatDestination);

            if (remainingDistance <= Mathf.Max(0.35f, attackRange * 0.2f))
            {
                hasAttackMoveDestination = false;
                mover.Stop();
                return;
            }

            mover.SetDestination(flatDestination);
        }

        private void ApplyDirectDamage(CombatTarget target)
        {
            if (target == null || !target.IsAlive)
            {
                return;
            }

            float damageMultiplier = abilityState != null ? abilityState.GetAttackDamageMultiplier() : 1f;
            float resolvedDamage = CombatTriangleRules.ResolveDamage(GetArchetype(), target, attackDamage * damageMultiplier, false);
            target.Health.ApplyDamage(resolvedDamage);
            SpawnImpactEffect(target.transform.position + Vector3.up * 0.6f, impactEffectScale, GetAttackColor());
        }

        private void LaunchProjectile(CombatTarget target)
        {
            if (target == null)
            {
                return;
            }

            float damageMultiplier = abilityState != null ? abilityState.GetAttackDamageMultiplier() : 1f;
            float distance = Vector3.Distance(transform.position, target.transform.position);
            int burstCount = roleController != null ? roleController.GetProjectileBurstCount(distance) : 1;

            for (int index = 0; index < burstCount; index++)
            {
                GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projectileObject.name = usesProjectile && splashRadius > 0.01f ? "Shell" : "Projectile";
                projectileObject.transform.position = transform.position + Vector3.up * 0.8f;
                projectileObject.transform.localScale = Vector3.one * Mathf.Clamp(0.22f + splashRadius * 0.08f, 0.2f, 0.55f);

                Collider projectileCollider = projectileObject.GetComponent<Collider>();
                if (projectileCollider != null)
                {
                    projectileCollider.enabled = false;
                }

                Renderer rendererComponent = projectileObject.GetComponent<Renderer>();
                rendererComponent.material.color = GetAttackColor();

                Vector3 aimOffset = roleController != null ? roleController.GetProjectileAimOffset(distance) : Vector3.zero;
                float burstDamage = attackDamage * damageMultiplier / Mathf.Max(1, burstCount);
                UnitProjectile projectile = projectileObject.AddComponent<UnitProjectile>();
                projectile.Initialize(target, owner.Team, GetArchetype(), burstDamage, projectileSpeed, projectileArc, splashRadius, impactEffectScale, rendererComponent.material.color, aimOffset);
            }
        }

        private UnitArchetype GetArchetype()
        {
            return selectableUnit != null ? selectableUnit.Archetype : UnitArchetype.Spearman;
        }

        private Color GetAttackColor()
        {
            Renderer rendererComponent = GetComponentInChildren<Renderer>();
            return rendererComponent != null ? rendererComponent.material.color : Color.white;
        }

        private Vector3 GetSearchOrigin()
        {
            if (hasGuardPoint)
            {
                return guardPoint;
            }

            if (hasHoldPosition)
            {
                return holdPosition;
            }

            return transform.position;
        }

        private string BuildOrderLabel()
        {
            if (currentTarget != null)
            {
                return "Engage";
            }

            if (hasAttackMoveDestination)
            {
                return "Advance";
            }

            if (hasGuardPoint)
            {
                return "Guard";
            }

            if (hasHoldPosition)
            {
                return "Hold";
            }

            return mover != null && mover.IsMoving ? "Move" : "Idle";
        }

        private void ClearDirectiveState()
        {
            currentTarget = null;
            hasAttackMoveDestination = false;
            hasPursuitDestination = false;
            hasHoldPosition = false;
            hasGuardPoint = false;
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized, Vector3.up),
                540f * Time.deltaTime);
        }
    }
}
