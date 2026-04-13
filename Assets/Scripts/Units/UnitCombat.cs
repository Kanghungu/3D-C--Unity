using Game.Audio;
using Game.BattleAces;
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
    public partial class UnitCombat : MonoBehaviour
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

        /// <summary>Battle Aces — 저체력 후퇴 재시도 간격</summary>
        private float battleAcesRetreatThrottle;

        /// <summary>아군 공격 휘두르기/발사음 스팸 방지 (언스케일)</summary>
        private float nextAttackWindupSoundUnscaled = -999f;

        /// <summary>애니 선딜에서 이미 휘두르기음 낸 뒤 실제 타격에서 중복 방지</summary>
        private bool suppressAttackWindupOnce;

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

            TryBattleAcesLowHealthRetreat();

            if (currentTarget != null && currentTarget.IsAlive)
            {
                pursuitDestination = GetEngagementPosition(currentTarget);
                hasPursuitDestination = true;
            }

            // V 토글 — 가까운 적 우선: 교전 중 더 가까운 적이 있으면 표적 교체 시도
            if (BattleAcesCombatSettings.AutoAcquireMode == AutoAcquireMode.PreferNearest &&
                currentTarget != null &&
                currentTarget.IsAlive &&
                retargetTimer <= 0f)
            {
                CombatTarget candidate = FindClosestEnemyTarget();
                if (candidate != null && candidate != currentTarget)
                {
                    float dCur = Vector3.Distance(transform.position, currentTarget.transform.position);
                    float dNew = Vector3.Distance(transform.position, candidate.transform.position);
                    if (dNew + 0.4f < dCur)
                    {
                        currentTarget = candidate;
                        pursuitDestination = GetEngagementPosition(currentTarget);
                        hasPursuitDestination = true;
                    }
                }

                retargetTimer = retargetInterval;
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

            float effectiveAttackRange = attackRange + (roleController != null ? roleController.GetPassiveAttackRangeBonus() : 0f);
            if (distance > effectiveAttackRange)
            {
                mover.SetDestination(pursuitDestination);
                return;
            }

            mover.Stop();
            FaceTarget(targetPosition);

            if (awaitingAnimStrike && distance > effectiveAttackRange * 1.28f)
            {
                awaitingAnimStrike = false;
                animStrikeVictim = null;
                animStrikeTimeoutRemainingUnscaled = 0f;
                mover?.SetExternalSpeedMultiplier(1f);
            }

            TickAnimStrikeFallback();
            if (awaitingAnimStrike)
            {
                return;
            }

            if (cooldownTimer > 0f)
            {
                return;
            }

            TryPerformAttackWhenReady();
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
            engagementBeamTimer = 2.35f;
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

        public static void SpawnImpactEffect(
            Vector3 position,
            float scale,
            Color color,
            float scaleMul = 1f,
            float lifeMul = 1f,
            bool directMelee = false)
        {
            float meleeBoost = directMelee ? 1.12f : 1f;
            float effectScale = Mathf.Max(0.2f, scale) * Mathf.Max(0.5f, scaleMul) * meleeBoost;

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

            int sparkCount = directMelee ? 7 : 4;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = (i / (float)sparkCount) * 360f;
                float radians = angle * Mathf.Deg2Rad;
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spark.name = $"Spark {i + 1}";
                spark.transform.SetParent(root.transform);
                spark.transform.localPosition = new Vector3(Mathf.Cos(radians) * effectScale * 0.24f, 0f, Mathf.Sin(radians) * effectScale * 0.24f);
                spark.transform.localRotation = Quaternion.Euler(0f, angle, 24f);
                spark.transform.localScale = new Vector3(0.08f, 0.08f, effectScale * (directMelee ? 0.78f : 0.65f));
                Collider sparkCollider = spark.GetComponent<Collider>();
                if (sparkCollider != null)
                {
                    sparkCollider.enabled = false;
                }

                Renderer sparkRenderer = spark.GetComponent<Renderer>();
                sparkRenderer.material.color = Color.Lerp(color, Color.white, 0.18f);
            }

            if (directMelee)
            {
                GameObject ring2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring2.name = "Shock Ring Outer";
                ring2.transform.SetParent(root.transform);
                ring2.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                ring2.transform.localScale = new Vector3(effectScale * 1.85f, 0.02f, effectScale * 1.85f);
                Collider ring2c = ring2.GetComponent<Collider>();
                if (ring2c != null)
                {
                    ring2c.enabled = false;
                }

                Renderer ring2r = ring2.GetComponent<Renderer>();
                ring2r.material.color = new Color(color.r, color.g, color.b, 0.45f);
            }

            Destroy(root, 0.24f * Mathf.Max(0.4f, lifeMul));
        }

        /// <summary>Battle Aces 아군만 — 공격 직전 짧은 음으로 히트와 레이어 분리</summary>
        private void TryPlayAttackWindupFeedback()
        {
            if (owner == null || owner.Team != UnitTeam.Player)
            {
                return;
            }

            if (!BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match) || match.IsFinished)
            {
                return;
            }

            if (suppressAttackWindupOnce)
            {
                suppressAttackWindupOnce = false;
                return;
            }

            if (Time.unscaledTime < nextAttackWindupSoundUnscaled)
            {
                return;
            }

            nextAttackWindupSoundUnscaled = Time.unscaledTime + 0.052f;
            ProceduralAudioUtility.PlayWeaponAttackWindup(usesProjectile, 1f);
        }

        private void ApplyDirectDamage(CombatTarget target)
        {
            if (target == null || !target.IsAlive)
            {
                return;
            }

            TryPlayAttackWindupFeedback();
            float damageMultiplier = abilityState != null ? abilityState.GetAttackDamageMultiplier() : 1f;
            damageMultiplier *= roleController != null ? roleController.GetPassiveAttackDamageMultiplier() : 1f;
            float resolvedDamage = CombatTriangleRules.ResolveDamage(GetArchetype(), target, attackDamage * damageMultiplier, false);
            target.Health.ApplyDamage(resolvedDamage, transform.position, fromProjectile: false);
            bool battleAcesActive = BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController baHit) && !baHit.IsFinished;
            float impactScaleMul = battleAcesActive ? 1.28f : 1f;
            float impactLifeMul = battleAcesActive ? 1.22f : 1f;
            Vector3 impactPos = target.transform.position + Vector3.up * 0.6f;
            SpawnImpactEffect(impactPos, impactEffectScale, GetAttackColor(), impactScaleMul, impactLifeMul, directMelee: true);
            if (battleAcesActive)
            {
                BattleAcesCombatJuice.NotifyImpactAccentRing(impactPos, owner.Team, 1.08f);
            }
        }

        private void LaunchProjectile(CombatTarget target)
        {
            if (target == null)
            {
                return;
            }

            TryPlayAttackWindupFeedback();
            float damageMultiplier = abilityState != null ? abilityState.GetAttackDamageMultiplier() : 1f;
            damageMultiplier *= roleController != null ? roleController.GetPassiveAttackDamageMultiplier() : 1f;
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

        /// <summary>
        /// Battle Aces — 아군·선택 해제·홀드/가드/공격이동 아님·체력 낮을 때 코어 쪽으로 후퇴 시도.
        /// </summary>
        private void TryBattleAcesLowHealthRetreat()
        {
            if (owner.Team != UnitTeam.Player)
            {
                return;
            }

            if (health.Normalized >= 0.26f)
            {
                return;
            }

            if (selectableUnit != null && selectableUnit.IsSelected)
            {
                return;
            }

            if (hasHoldPosition || hasGuardPoint || hasAttackMoveDestination)
            {
                return;
            }

            if (!BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match) || match.IsFinished)
            {
                return;
            }

            BattleAcesCore core = match.PlayerCore;
            if (core == null || core.Health == null || !core.Health.IsAlive)
            {
                return;
            }

            battleAcesRetreatThrottle -= Time.deltaTime;
            if (battleAcesRetreatThrottle > 0f)
            {
                return;
            }

            if (Vector3.Distance(transform.position, core.transform.position) < 9f)
            {
                return;
            }

            battleAcesRetreatThrottle = 2.4f;
            currentTarget = null;
            hasPursuitDestination = false;
            Vector3 flat = core.transform.position;
            flat.y = transform.position.y;
            mover.SetDestination(flat);
        }
    }
}
