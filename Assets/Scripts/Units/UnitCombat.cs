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
        private float engagementBeamTimer;
        private bool hasAttackMoveDestination;
        private Vector3 attackMoveDestination;
        private bool hasPursuitDestination;
        private Vector3 pursuitDestination;
        private bool hasHoldPosition;
        private Vector3 holdPosition;
        private bool hasGuardPoint;
        private Vector3 guardPoint;
        private float guardRadius = 18f;
        private Transform engagementAnchor;
        private Transform engagementBeam;
        private Renderer engagementBeamRenderer;
        private Transform engagementTip;
        private Renderer engagementTipRenderer;
        private Transform orderAnchorVisual;
        private Transform orderAnchorBeam;
        private Renderer orderAnchorBeamRenderer;
        private Transform orderAnchorMarker;
        private Renderer orderAnchorMarkerRenderer;
        private float recentAttackPulse;

        public CombatTarget CurrentTarget => currentTarget;
        public bool HasAttackMoveDestination => hasAttackMoveDestination;
        public string OrderLabel => BuildOrderLabel();
        public float RecentAttackPulse => recentAttackPulse;

        private void Awake()
        {
            mover = GetComponent<SimpleUnitMover>();
            owner = GetComponent<CombatTarget>();
            health = GetComponent<UnitHealth>();
            abilityState = GetComponent<UnitAbilityState>();
            selectableUnit = GetComponent<SelectableUnit>();
            roleController = GetComponent<AdvancedUnitRoleController>();
            EnsureEngagementVisuals();
            EnsureOrderAnchorVisuals();
            UpdateEngagementVisuals();
            UpdateOrderAnchorVisuals();
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

            if (engagementBeamTimer > 0f)
            {
                engagementBeamTimer -= Time.deltaTime;
            }

            if (recentAttackPulse > 0f)
            {
                recentAttackPulse = Mathf.Max(0f, recentAttackPulse - Time.deltaTime * 3.6f);
            }

            if (retargetTimer > 0f)
            {
                retargetTimer -= Time.deltaTime;
            }

            if (!health.IsAlive)
            {
                UpdateEngagementVisuals();
                UpdateOrderAnchorVisuals();
                return;
            }

            if (currentTarget != null && currentTarget.IsAlive)
            {
                pursuitDestination = GetEngagementPosition(currentTarget);
                hasPursuitDestination = true;
            }

            if ((currentTarget == null || !currentTarget.IsAlive) && retargetTimer <= 0f)
            {
                currentTarget = FindClosestEnemyTarget();
                retargetTimer = retargetInterval;
            }

            if (currentTarget == null)
            {
                UpdateEngagementVisuals();
                UpdateOrderAnchorVisuals();
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
                UpdateEngagementVisuals();
                UpdateOrderAnchorVisuals();
                return;
            }

            Vector3 targetPosition = currentTarget.transform.position;
            float distance = Vector3.Distance(transform.position, targetPosition);
            UpdateEngagementVisuals();
            UpdateOrderAnchorVisuals();

            if (distance > attackRange)
            {
                mover.SetDestination(pursuitDestination);
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

            recentAttackPulse = 1f;
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
            EnsureEngagementVisuals();
            EnsureOrderAnchorVisuals();
            UpdateEngagementVisuals();
            UpdateOrderAnchorVisuals();
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
            engagementBeamTimer = 1.8f;
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
            float effectScale = Mathf.Max(0.2f, scale);

            GameObject root = new("Impact Effect");
            root.transform.position = position;

            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "Flash";
            flash.transform.SetParent(root.transform);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localScale = Vector3.one * effectScale;
            Collider flashCollider = flash.GetComponent<Collider>();
            if (flashCollider != null)
            {
                flashCollider.enabled = false;
            }

            Renderer flashRenderer = flash.GetComponent<Renderer>();
            flashRenderer.material.color = Color.Lerp(color, Color.white, 0.35f);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Shock Ring";
            ring.transform.SetParent(root.transform);
            ring.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            ring.transform.localScale = new Vector3(effectScale * 1.35f, 0.03f, effectScale * 1.35f);
            Collider ringCollider = ring.GetComponent<Collider>();
            if (ringCollider != null)
            {
                ringCollider.enabled = false;
            }

            Renderer ringRenderer = ring.GetComponent<Renderer>();
            ringRenderer.material.color = new Color(color.r, color.g, color.b, 0.85f);

            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                float radians = angle * Mathf.Deg2Rad;
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spark.name = $"Spark {i + 1}";
                spark.transform.SetParent(root.transform);
                spark.transform.localPosition = new Vector3(Mathf.Cos(radians) * effectScale * 0.24f, 0f, Mathf.Sin(radians) * effectScale * 0.24f);
                spark.transform.localRotation = Quaternion.Euler(0f, angle, 24f);
                spark.transform.localScale = new Vector3(0.08f, 0.08f, effectScale * 0.65f);
                Collider sparkCollider = spark.GetComponent<Collider>();
                if (sparkCollider != null)
                {
                    sparkCollider.enabled = false;
                }

                Renderer sparkRenderer = spark.GetComponent<Renderer>();
                sparkRenderer.material.color = Color.Lerp(color, Color.white, 0.18f);
            }

            Destroy(root, 0.24f);
        }

        private Vector3 GetEngagementPosition(CombatTarget target)
        {
            // 같은 팀에서 같은 타겟을 공격하는 유닛들 수집
            int mySlot = 0;
            int totalAttackers = 0;

            foreach (UnitCombat other in PrototypeRuntimeRegistry.GetUnitCombats())
            {
                if (other == null || other.owner == null || other.owner.Team != owner.Team)
                {
                    continue;
                }

                if (other.currentTarget != target)
                {
                    continue;
                }

                if (other == this)
                {
                    mySlot = totalAttackers;
                }

                totalAttackers++;
            }

            if (totalAttackers <= 1)
            {
                return target.transform.position;
            }

            // 공격 반지름: attackRange의 75% 지점에 원형 배치
            float radius = attackRange * 0.75f;
            float angle = mySlot * (Mathf.PI * 2f / totalAttackers);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 pos = target.transform.position + offset;
            pos.y = transform.position.y;
            return pos;
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

        private void EnsureEngagementVisuals()
        {
            if (engagementAnchor != null)
            {
                return;
            }

            engagementAnchor = new GameObject("Engagement Anchor").transform;
            engagementAnchor.SetParent(transform);
            engagementAnchor.localPosition = new Vector3(0f, 0.82f, 0f);
            engagementAnchor.localRotation = Quaternion.identity;
            engagementAnchor.localScale = Vector3.one;

            engagementBeam = CreateEngagementPrimitive(
                engagementAnchor,
                PrimitiveType.Cube,
                "Engagement Beam",
                new Vector3(0f, 0f, 0.4f),
                new Vector3(0.05f, 0.05f, 0.8f),
                new Color(1f, 0.42f, 0.24f));
            engagementBeamRenderer = engagementBeam.GetComponent<Renderer>();

            engagementTip = CreateEngagementPrimitive(
                engagementAnchor,
                PrimitiveType.Sphere,
                "Engagement Tip",
                new Vector3(0f, 0f, 0.82f),
                new Vector3(0.12f, 0.12f, 0.12f),
                new Color(1f, 0.42f, 0.24f));
            engagementTipRenderer = engagementTip.GetComponent<Renderer>();
        }

        private void UpdateEngagementVisuals()
        {
            if (engagementAnchor == null)
            {
                return;
            }

            bool show = engagementBeamTimer > 0f
                && health != null && health.IsAlive
                && currentTarget != null && currentTarget.IsAlive;
            engagementAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            Vector3 targetPosition = currentTarget.transform.position + Vector3.up * 0.72f;
            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.35f, 0.65f), localTarget.z);
            float distance = Mathf.Max(0.12f, planarTarget.magnitude);
            Vector3 direction = planarTarget / distance;
            float pulse = 0.88f + Mathf.PingPong(Time.time * 4.4f, 0.18f);
            Color linkColor = usesProjectile ? new Color(1f, 0.56f, 0.24f) : new Color(1f, 0.34f, 0.22f);

            if (engagementAnchor != null)
            {
                engagementAnchor.localPosition = new Vector3(0f, 0.82f + Mathf.PingPong(Time.time * 1.2f, 0.06f), 0f);
                engagementAnchor.localRotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (engagementBeam != null)
            {
                engagementBeam.localPosition = new Vector3(0f, 0f, distance * 0.5f);
                engagementBeam.localScale = new Vector3(0.045f, 0.045f, distance);
            }

            if (engagementBeamRenderer != null)
            {
                engagementBeamRenderer.material.color = linkColor * pulse;
            }

            if (engagementTip != null)
            {
                engagementTip.localPosition = new Vector3(0f, 0f, distance);
                engagementTip.localScale = Vector3.one * (0.1f + Mathf.PingPong(Time.time * 2.8f, 0.03f));
            }

            if (engagementTipRenderer != null)
            {
                engagementTipRenderer.material.color = Color.Lerp(linkColor, Color.white, 0.18f) * pulse;
            }
        }

        private static Transform CreateEngagementPrimitive(Transform parent, PrimitiveType primitiveType, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = objectName;
            child.transform.SetParent(parent);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = localScale;

            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer rendererComponent = child.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            return child.transform;
        }

        private void EnsureOrderAnchorVisuals()
        {
            if (orderAnchorVisual != null)
            {
                return;
            }

            orderAnchorVisual = new GameObject("Order Anchor Visual").transform;
            orderAnchorVisual.SetParent(transform);
            orderAnchorVisual.localPosition = new Vector3(0f, 0.42f, 0f);
            orderAnchorVisual.localRotation = Quaternion.identity;
            orderAnchorVisual.localScale = Vector3.one;

            orderAnchorBeam = CreateEngagementPrimitive(
                orderAnchorVisual,
                PrimitiveType.Cube,
                "Order Anchor Beam",
                new Vector3(0f, 0f, 0.5f),
                new Vector3(0.04f, 0.04f, 1f),
                new Color(0.34f, 0.95f, 1f));
            orderAnchorBeamRenderer = orderAnchorBeam.GetComponent<Renderer>();

            orderAnchorMarker = CreateEngagementPrimitive(
                orderAnchorVisual,
                PrimitiveType.Cylinder,
                "Order Anchor Marker",
                new Vector3(0f, -0.1f, 1f),
                new Vector3(0.12f, 0.04f, 0.12f),
                new Color(0.34f, 0.95f, 1f));
            orderAnchorMarkerRenderer = orderAnchorMarker.GetComponent<Renderer>();
        }

        private void UpdateOrderAnchorVisuals()
        {
            if (orderAnchorVisual == null)
            {
                return;
            }

            bool canShow = health != null
                && health.IsAlive
                && selectableUnit != null
                && selectableUnit.IsSelected
                && currentTarget == null;

            bool hasAnchor = canShow && (hasHoldPosition || hasGuardPoint || hasAttackMoveDestination);
            orderAnchorVisual.gameObject.SetActive(hasAnchor);

            if (!hasAnchor)
            {
                return;
            }

            Vector3 anchorWorld = hasGuardPoint
                ? guardPoint
                : hasHoldPosition
                    ? holdPosition
                    : attackMoveDestination;
            Vector3 targetPoint = new Vector3(anchorWorld.x, transform.position.y + 0.08f, anchorWorld.z);
            Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.25f, 0.35f), localTarget.z);
            float distance = Mathf.Max(0.2f, planarTarget.magnitude);
            Vector3 direction = planarTarget / distance;
            float pulse = 0.86f + Mathf.PingPong(Time.time * 3.2f, 0.16f);

            Color anchorColor = hasGuardPoint
                ? new Color(1f, 0.86f, 0.34f)
                : hasHoldPosition
                    ? new Color(0.52f, 0.76f, 1f)
                    : new Color(0.34f, 0.95f, 1f);

            orderAnchorVisual.localPosition = new Vector3(0f, 0.42f + Mathf.PingPong(Time.time * 1.2f, 0.04f), 0f);
            orderAnchorVisual.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (orderAnchorBeam != null)
            {
                orderAnchorBeam.localPosition = new Vector3(0f, 0f, distance * 0.5f);
                orderAnchorBeam.localScale = new Vector3(0.035f, 0.035f, distance);
            }

            if (orderAnchorBeamRenderer != null)
            {
                orderAnchorBeamRenderer.material.color = anchorColor * pulse;
            }

            if (orderAnchorMarker != null)
            {
                orderAnchorMarker.localPosition = new Vector3(0f, -0.08f, distance);
                orderAnchorMarker.localScale = new Vector3(0.12f + Mathf.PingPong(Time.time * 1.5f, 0.03f), 0.04f, 0.12f + Mathf.PingPong(Time.time * 1.5f, 0.03f));
            }

            if (orderAnchorMarkerRenderer != null)
            {
                orderAnchorMarkerRenderer.material.color = Color.Lerp(anchorColor, Color.white, 0.14f) * pulse;
            }
        }
    }
}
