using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Very lightweight combat loop for prototype testing.
    /// Supports direct attacks, ranged projectiles, and attack-move orders.
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
        private float cooldownTimer;
        private float retargetTimer;
        private bool hasAttackMoveDestination;
        private Vector3 attackMoveDestination;

        public CombatTarget CurrentTarget => currentTarget;
        public bool HasAttackMoveDestination => hasAttackMoveDestination;

        private void Awake()
        {
            mover = GetComponent<SimpleUnitMover>();
            owner = GetComponent<CombatTarget>();
            health = GetComponent<UnitHealth>();
            abilityState = GetComponent<UnitAbilityState>();
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

            if ((currentTarget == null || !currentTarget.IsAlive) && retargetTimer <= 0f)
            {
                currentTarget = FindClosestEnemyTarget();
                retargetTimer = retargetInterval;
            }

            if (currentTarget == null)
            {
                RunAttackMoveIfNeeded();
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
        }

        public void SetTarget(CombatTarget target)
        {
            if (target == null || target == owner || target.Team == owner.Team)
            {
                return;
            }

            hasAttackMoveDestination = false;
            currentTarget = target;
        }

        public void SetAttackMoveDestination(Vector3 destination)
        {
            hasAttackMoveDestination = true;
            attackMoveDestination = new Vector3(destination.x, transform.position.y, destination.z);
            currentTarget = null;
            mover.SetDestination(attackMoveDestination);
        }

        public void ClearTarget()
        {
            currentTarget = null;
            hasAttackMoveDestination = false;
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

            foreach (CombatTarget target in FindObjectsByType<CombatTarget>())
            {
                if (target == null || target == owner || !target.IsAlive || target.Team == owner.Team)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            return bestTarget;
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
            target.Health.ApplyDamage(attackDamage * damageMultiplier);
            SpawnImpactEffect(target.transform.position + Vector3.up * 0.6f, impactEffectScale, GetAttackColor());
        }

        private void LaunchProjectile(CombatTarget target)
        {
            if (target == null)
            {
                return;
            }

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

            float damageMultiplier = abilityState != null ? abilityState.GetAttackDamageMultiplier() : 1f;
            UnitProjectile projectile = projectileObject.AddComponent<UnitProjectile>();
            projectile.Initialize(target, owner.Team, attackDamage * damageMultiplier, projectileSpeed, projectileArc, splashRadius, impactEffectScale, rendererComponent.material.color);
        }

        private Color GetAttackColor()
        {
            Renderer rendererComponent = GetComponentInChildren<Renderer>();
            return rendererComponent != null ? rendererComponent.material.color : Color.white;
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
