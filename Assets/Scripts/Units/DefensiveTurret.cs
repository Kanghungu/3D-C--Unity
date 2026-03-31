using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Simple fixed defense that protects a lane or base approach.
    /// </summary>
    [RequireComponent(typeof(CombatTarget))]
    [RequireComponent(typeof(UnitHealth))]
    public class DefensiveTurret : MonoBehaviour
    {
        [SerializeField] private float attackRange = 10f;
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private float attackCooldown = 1.15f;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileArc = 0.4f;
        [SerializeField] private float retargetInterval = 0.5f;
        [SerializeField] private float supportLinkRadius = 16f;

        private CombatTarget owner;
        private UnitHealth health;
        private CombatTarget currentTarget;
        private CombatTarget trackedVisualTarget;
        private CombatTarget trackedLockTarget;
        private float cooldownTimer;
        private float retargetTimer;
        private float retargetHeat;
        private float targetLockTime;
        private Transform rangeAnchor;
        private Transform rangeCore;
        private Renderer rangeCoreRenderer;
        private readonly Transform[] rangePosts = new Transform[6];
        private readonly Renderer[] rangePostRenderers = new Renderer[6];
        private Transform engagementAnchor;
        private Transform engagementBeam;
        private Renderer engagementBeamRenderer;
        private Transform engagementTip;
        private Renderer engagementTipRenderer;
        private Transform cooldownAnchor;
        private Transform cooldownRing;
        private Renderer cooldownRingRenderer;
        private Transform cooldownCore;
        private Renderer cooldownCoreRenderer;
        private Transform targetBadgeAnchor;
        private Transform targetBadgePrimary;
        private Renderer targetBadgePrimaryRenderer;
        private Transform targetBadgeSecondary;
        private Renderer targetBadgeSecondaryRenderer;
        private Transform overloadAnchor;
        private Transform overloadRing;
        private Renderer overloadRingRenderer;
        private Transform overloadCore;
        private Renderer overloadCoreRenderer;
        private Transform overloadDirectionRoot;
        private Transform overloadDirectionBeam;
        private Renderer overloadDirectionBeamRenderer;
        private Transform overloadDirectionTip;
        private Renderer overloadDirectionTipRenderer;
        private Transform rangeStateAnchor;
        private Transform rangeStateBar;
        private Renderer rangeStateBarRenderer;
        private Transform rangeStateNeedle;
        private Renderer rangeStateNeedleRenderer;
        private Transform watchAnchor;
        private Transform watchBeam;
        private Renderer watchBeamRenderer;
        private Transform watchTip;
        private Renderer watchTipRenderer;
        private Transform watchAlertRing;
        private Renderer watchAlertRingRenderer;
        private Transform watchAlertCore;
        private Renderer watchAlertCoreRenderer;
        private Transform supportAnchor;
        private Transform supportRing;
        private Renderer supportRingRenderer;
        private readonly Transform[] supportPips = new Transform[3];
        private readonly Renderer[] supportPipRenderers = new Renderer[3];
        private Transform isolationAnchor;
        private Transform isolationRing;
        private Renderer isolationRingRenderer;
        private Transform isolationCore;
        private Renderer isolationCoreRenderer;
        private Transform focusAnchor;
        private Transform focusRing;
        private Renderer focusRingRenderer;
        private readonly Transform[] focusPips = new Transform[3];
        private readonly Renderer[] focusPipRenderers = new Renderer[3];
        private Transform spreadAnchor;
        private Transform spreadRing;
        private Renderer spreadRingRenderer;
        private readonly Transform[] spreadPips = new Transform[3];
        private readonly Renderer[] spreadPipRenderers = new Renderer[3];
        private Transform retargetAnchor;
        private Transform retargetRing;
        private Renderer retargetRingRenderer;
        private Transform retargetCore;
        private Renderer retargetCoreRenderer;
        private Transform volleyAnchor;
        private Transform volleyRing;
        private Renderer volleyRingRenderer;
        private readonly Transform[] volleyPips = new Transform[3];
        private readonly Renderer[] volleyPipRenderers = new Renderer[3];
        private Transform gapAnchor;
        private Transform gapRing;
        private Renderer gapRingRenderer;
        private Transform gapCore;
        private Renderer gapCoreRenderer;
        private Transform lockAnchor;
        private Transform lockRing;
        private Renderer lockRingRenderer;
        private Transform lockCore;
        private Renderer lockCoreRenderer;
        private Transform finishAnchor;
        private Transform finishRing;
        private Renderer finishRingRenderer;
        private Transform finishCore;
        private Renderer finishCoreRenderer;
        private Transform overkillAnchor;
        private Transform overkillRing;
        private Renderer overkillRingRenderer;
        private Transform overkillCore;
        private Renderer overkillCoreRenderer;

        private void Awake()
        {
            owner = GetComponent<CombatTarget>();
            health = GetComponent<UnitHealth>();
            EnsureRangeVisuals();
            EnsureEngagementVisuals();
            EnsureCooldownVisuals();
            EnsureTargetBadgeVisuals();
            EnsureOverloadVisuals();
            EnsureRangeStateVisuals();
            EnsureWatchVisuals();
            EnsureSupportVisuals();
            EnsureIsolationVisuals();
            EnsureFocusVisuals();
            EnsureSpreadVisuals();
            EnsureRetargetVisuals();
            EnsureVolleyVisuals();
            EnsureGapVisuals();
            EnsureLockVisuals();
            EnsureFinishVisuals();
            EnsureOverkillVisuals();
            UpdateRangeVisuals();
            UpdateEngagementVisuals();
            UpdateCooldownVisuals();
            UpdateTargetBadgeVisuals();
            UpdateOverloadVisuals();
            UpdateRangeStateVisuals();
            UpdateWatchVisuals();
            UpdateSupportVisuals();
            UpdateIsolationVisuals();
            UpdateFocusVisuals();
            UpdateSpreadVisuals();
            UpdateRetargetVisuals();
            UpdateVolleyVisuals();
            UpdateGapVisuals();
            UpdateLockVisuals();
            UpdateFinishVisuals();
            UpdateOverkillVisuals();
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
            UpdateRetargetHeat();

            if (health == null || !health.IsAlive)
            {
                targetLockTime = 0f;
                trackedLockTarget = null;
                UpdateRangeVisuals();
                UpdateEngagementVisuals();
                UpdateCooldownVisuals();
                UpdateTargetBadgeVisuals();
                UpdateOverloadVisuals();
                UpdateRangeStateVisuals();
                UpdateWatchVisuals();
                UpdateSupportVisuals();
                UpdateIsolationVisuals();
                UpdateFocusVisuals();
                UpdateSpreadVisuals();
                UpdateRetargetVisuals();
                UpdateVolleyVisuals();
                UpdateGapVisuals();
                UpdateLockVisuals();
                UpdateFinishVisuals();
                UpdateOverkillVisuals();
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (retargetTimer > 0f)
            {
                retargetTimer -= Time.deltaTime;
            }

            if ((currentTarget == null || !currentTarget.IsAlive || Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange) && retargetTimer <= 0f)
            {
                currentTarget = FindClosestEnemy();
                retargetTimer = retargetInterval;
            }

            TrackRetargetChange();
            UpdateTargetLockState();

            if (currentTarget == null)
            {
                UpdateRangeVisuals();
                UpdateEngagementVisuals();
                UpdateCooldownVisuals();
                UpdateTargetBadgeVisuals();
                UpdateOverloadVisuals();
                UpdateRangeStateVisuals();
                UpdateWatchVisuals();
                UpdateSupportVisuals();
                UpdateIsolationVisuals();
                UpdateFocusVisuals();
                UpdateSpreadVisuals();
                UpdateRetargetVisuals();
                UpdateVolleyVisuals();
                UpdateGapVisuals();
                UpdateLockVisuals();
                UpdateFinishVisuals();
                UpdateOverkillVisuals();
                return;
            }

            FaceTarget(currentTarget.transform.position);
            UpdateRangeVisuals();
            UpdateEngagementVisuals();
            UpdateCooldownVisuals();
            UpdateTargetBadgeVisuals();
            UpdateOverloadVisuals();
            UpdateRangeStateVisuals();
            UpdateWatchVisuals();
            UpdateSupportVisuals();
            UpdateIsolationVisuals();
            UpdateFocusVisuals();
            UpdateSpreadVisuals();
            UpdateRetargetVisuals();
            UpdateVolleyVisuals();
            UpdateGapVisuals();
            UpdateLockVisuals();
            UpdateFinishVisuals();
            UpdateOverkillVisuals();

            if (cooldownTimer > 0f)
            {
                return;
            }

            FireProjectile(currentTarget);
            cooldownTimer = attackCooldown;
            UpdateCooldownVisuals();
        }

        public void Configure(float newAttackRange, float newAttackDamage, float newAttackCooldown, float newProjectileSpeed, float newProjectileArc)
        {
            attackRange = newAttackRange;
            attackDamage = newAttackDamage;
            attackCooldown = newAttackCooldown;
            projectileSpeed = newProjectileSpeed;
            projectileArc = newProjectileArc;
            UpdateRangeVisuals();
            UpdateEngagementVisuals();
            UpdateCooldownVisuals();
            UpdateTargetBadgeVisuals();
            UpdateOverloadVisuals();
            UpdateRangeStateVisuals();
            UpdateWatchVisuals();
            UpdateSupportVisuals();
            UpdateIsolationVisuals();
            UpdateFocusVisuals();
            UpdateSpreadVisuals();
            UpdateRetargetVisuals();
            UpdateVolleyVisuals();
            UpdateGapVisuals();
            UpdateLockVisuals();
            UpdateFinishVisuals();
            UpdateOverkillVisuals();
        }

        private CombatTarget FindClosestEnemy()
        {
            CombatTarget bestTarget = null;
            float bestDistance = attackRange;

            foreach (CombatTarget target in PrototypeRuntimeRegistry.GetCombatTargets())
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

        private void FireProjectile(CombatTarget target)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Turret Shot";
            projectileObject.transform.position = transform.position + Vector3.up * 1.5f;
            projectileObject.transform.localScale = Vector3.one * 0.28f;

            Collider projectileCollider = projectileObject.GetComponent<Collider>();

            if (projectileCollider != null)
            {
                projectileCollider.enabled = false;
            }

            Renderer rendererComponent = projectileObject.GetComponent<Renderer>();
            rendererComponent.material.color = owner.Team == UnitTeam.Player ? new Color(0.35f, 1f, 1f) : new Color(1f, 0.45f, 0.25f);

            float damage = attackDamage;
            SelectableUnit unit = target.GetComponent<SelectableUnit>();

            if (unit != null && unit.Archetype == UnitArchetype.Fighter)
            {
                damage *= 1.8f;
            }

            UnitProjectile projectile = projectileObject.AddComponent<UnitProjectile>();
            projectile.Initialize(target, owner.Team, UnitArchetype.Artillery, damage, projectileSpeed, projectileArc, 0f, 0.45f, rendererComponent.material.color, Vector3.zero);
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
                520f * Time.deltaTime);
        }

        private void EnsureRangeVisuals()
        {
            if (rangeAnchor != null)
            {
                return;
            }

            rangeAnchor = new GameObject("Range Anchor").transform;
            rangeAnchor.SetParent(transform);
            rangeAnchor.localPosition = new Vector3(0f, 0.08f, 0f);
            rangeAnchor.localRotation = Quaternion.identity;
            rangeAnchor.localScale = Vector3.one;

            rangeCore = CreateRangePrimitive(
                rangeAnchor,
                PrimitiveType.Cylinder,
                "Range Core",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.42f, 0.03f, 0.42f),
                new Color(0.3f, 0.84f, 1f));
            rangeCoreRenderer = rangeCore.GetComponent<Renderer>();

            for (int i = 0; i < rangePosts.Length; i++)
            {
                rangePosts[i] = CreateRangePrimitive(
                    rangeAnchor,
                    PrimitiveType.Cylinder,
                    $"Range Post {i + 1}",
                    Vector3.zero,
                    new Vector3(0.16f, 0.12f, 0.16f),
                    new Color(0.3f, 0.84f, 1f));
                rangePostRenderers[i] = rangePosts[i].GetComponent<Renderer>();
            }
        }

        private void UpdateRangeVisuals()
        {
            if (rangeAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            rangeAnchor.gameObject.SetActive(alive);
            if (!alive)
            {
                return;
            }

            Color teamColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.46f, 0.24f)
                : new Color(0.34f, 0.92f, 1f);
            bool hasTarget = currentTarget != null && currentTarget.IsAlive;
            float pulse = 0.84f + Mathf.PingPong(Time.time * (hasTarget ? 4.2f : 1.5f), hasTarget ? 0.22f : 0.1f);

            if (rangeCore != null)
            {
                float coreScale = hasTarget ? 0.56f : 0.42f;
                rangeCore.localScale = new Vector3(coreScale, 0.03f, coreScale);
            }

            if (rangeCoreRenderer != null)
            {
                rangeCoreRenderer.material.color = teamColor * pulse;
            }

            for (int i = 0; i < rangePosts.Length; i++)
            {
                if (rangePosts[i] == null)
                {
                    continue;
                }

                float angle = i / (float)rangePosts.Length * Mathf.PI * 2f;
                Vector3 localPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * attackRange;
                rangePosts[i].localPosition = localPosition;
                rangePosts[i].localScale = new Vector3(0.16f, hasTarget && i == 0 ? 0.26f : 0.16f, 0.16f);

                if (rangePostRenderers[i] != null)
                {
                    rangePostRenderers[i].material.color = Color.Lerp(teamColor, Color.white, hasTarget ? 0.12f : 0.04f) * pulse;
                }
            }
        }

        private static Transform CreateRangePrimitive(Transform parent, PrimitiveType primitiveType, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
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

        private void EnsureEngagementVisuals()
        {
            if (engagementAnchor != null)
            {
                return;
            }

            engagementAnchor = new GameObject("Turret Engagement Anchor").transform;
            engagementAnchor.SetParent(transform);
            engagementAnchor.localPosition = new Vector3(0f, 1.18f, 0f);
            engagementAnchor.localRotation = Quaternion.identity;
            engagementAnchor.localScale = Vector3.one;

            engagementBeam = CreateRangePrimitive(
                engagementAnchor,
                PrimitiveType.Cube,
                "Turret Engagement Beam",
                new Vector3(0f, 0f, 0.6f),
                new Vector3(0.06f, 0.06f, 1.2f),
                new Color(1f, 0.46f, 0.24f));
            engagementBeamRenderer = engagementBeam.GetComponent<Renderer>();

            engagementTip = CreateRangePrimitive(
                engagementAnchor,
                PrimitiveType.Sphere,
                "Turret Engagement Tip",
                new Vector3(0f, 0f, 1.24f),
                new Vector3(0.14f, 0.14f, 0.14f),
                new Color(1f, 0.46f, 0.24f));
            engagementTipRenderer = engagementTip.GetComponent<Renderer>();
        }

        private void UpdateEngagementVisuals()
        {
            if (engagementAnchor == null)
            {
                return;
            }

            bool show = health != null
                && health.IsAlive
                && currentTarget != null
                && currentTarget.IsAlive
                && owner != null
                && currentTarget.Team != owner.Team;
            engagementAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            Vector3 targetPosition = currentTarget.transform.position + Vector3.up * 0.72f;
            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.5f, 0.7f), localTarget.z);
            float distance = Mathf.Max(0.18f, planarTarget.magnitude);
            Vector3 direction = planarTarget / distance;
            float pulse = 0.9f + Mathf.PingPong(Time.time * 4.8f, 0.2f);
            Color linkColor = owner.Team == UnitTeam.Player
                ? new Color(0.34f, 0.92f, 1f)
                : new Color(1f, 0.46f, 0.24f);

            engagementAnchor.localPosition = new Vector3(0f, 1.18f + Mathf.PingPong(Time.time * 1.2f, 0.05f), 0f);
            engagementAnchor.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (engagementBeam != null)
            {
                engagementBeam.localPosition = new Vector3(0f, 0f, distance * 0.5f);
                engagementBeam.localScale = new Vector3(0.06f, 0.06f, distance);
            }

            if (engagementBeamRenderer != null)
            {
                engagementBeamRenderer.material.color = linkColor * pulse;
            }

            if (engagementTip != null)
            {
                engagementTip.localPosition = new Vector3(0f, 0f, distance);
                engagementTip.localScale = Vector3.one * (0.12f + Mathf.PingPong(Time.time * 2.4f, 0.03f));
            }

            if (engagementTipRenderer != null)
            {
                engagementTipRenderer.material.color = Color.Lerp(linkColor, Color.white, 0.2f) * pulse;
            }
        }

        private void EnsureCooldownVisuals()
        {
            if (cooldownAnchor != null)
            {
                return;
            }

            cooldownAnchor = new GameObject("Turret Cooldown Anchor").transform;
            cooldownAnchor.SetParent(transform);
            cooldownAnchor.localPosition = new Vector3(0f, 1.56f, 0f);
            cooldownAnchor.localRotation = Quaternion.identity;
            cooldownAnchor.localScale = Vector3.one;

            cooldownRing = CreateRangePrimitive(
                cooldownAnchor,
                PrimitiveType.Cylinder,
                "Turret Cooldown Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.18f, 0.025f, 0.18f),
                new Color(0.34f, 0.92f, 1f));
            cooldownRingRenderer = cooldownRing.GetComponent<Renderer>();

            cooldownCore = CreateRangePrimitive(
                cooldownAnchor,
                PrimitiveType.Sphere,
                "Turret Cooldown Core",
                new Vector3(0f, 0.12f, 0f),
                new Vector3(0.16f, 0.16f, 0.16f),
                new Color(0.34f, 0.92f, 1f));
            cooldownCoreRenderer = cooldownCore.GetComponent<Renderer>();
        }

        private void UpdateCooldownVisuals()
        {
            if (cooldownAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            cooldownAnchor.gameObject.SetActive(alive);
            if (!alive)
            {
                return;
            }

            float cooldownNormalized = attackCooldown > 0.001f
                ? 1f - Mathf.Clamp01(cooldownTimer / attackCooldown)
                : 1f;
            bool hasTarget = currentTarget != null && currentTarget.IsAlive;
            bool ready = cooldownTimer <= 0.001f;
            Color readyColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.46f, 0.24f)
                : new Color(0.34f, 0.92f, 1f);
            Color chargeColor = Color.Lerp(new Color(0.82f, 0.82f, 0.88f), readyColor, cooldownNormalized);
            float pulse = ready
                ? 0.92f + Mathf.PingPong(Time.time * (hasTarget ? 4.4f : 2.2f), hasTarget ? 0.22f : 0.12f)
                : 0.84f + Mathf.PingPong(Time.time * 1.6f, 0.08f);
            float ringRadius = 0.14f + cooldownNormalized * 0.16f;
            float coreScale = 0.1f + cooldownNormalized * 0.14f;

            cooldownAnchor.localPosition = new Vector3(0f, 1.56f + Mathf.PingPong(Time.time * 1.1f, ready ? 0.06f : 0.03f), 0f);

            if (cooldownRing != null)
            {
                cooldownRing.localScale = new Vector3(ringRadius, 0.025f, ringRadius);
            }

            if (cooldownRingRenderer != null)
            {
                cooldownRingRenderer.material.color = chargeColor * pulse;
            }

            if (cooldownCore != null)
            {
                cooldownCore.localScale = Vector3.one * coreScale;
            }

            if (cooldownCoreRenderer != null)
            {
                cooldownCoreRenderer.material.color = Color.Lerp(chargeColor, Color.white, ready ? 0.2f : 0.08f) * pulse;
            }
        }

        private void EnsureTargetBadgeVisuals()
        {
            if (targetBadgeAnchor != null)
            {
                return;
            }

            targetBadgeAnchor = new GameObject("Turret Target Badge Anchor").transform;
            targetBadgeAnchor.SetParent(transform);
            targetBadgeAnchor.localPosition = new Vector3(0f, 1.92f, 0f);
            targetBadgeAnchor.localRotation = Quaternion.identity;
            targetBadgeAnchor.localScale = Vector3.one;

            targetBadgePrimary = CreateRangePrimitive(
                targetBadgeAnchor,
                PrimitiveType.Cube,
                "Turret Target Badge Primary",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.14f, 0.08f, 0.14f),
                new Color(0.9f, 0.9f, 0.94f));
            targetBadgePrimaryRenderer = targetBadgePrimary.GetComponent<Renderer>();

            targetBadgeSecondary = CreateRangePrimitive(
                targetBadgeAnchor,
                PrimitiveType.Cube,
                "Turret Target Badge Secondary",
                new Vector3(0f, 0.12f, 0f),
                new Vector3(0.08f, 0.08f, 0.08f),
                new Color(0.9f, 0.9f, 0.94f));
            targetBadgeSecondaryRenderer = targetBadgeSecondary.GetComponent<Renderer>();
        }

        private void UpdateTargetBadgeVisuals()
        {
            if (targetBadgeAnchor == null)
            {
                return;
            }

            bool show = health != null && health.IsAlive && currentTarget != null && currentTarget.IsAlive;
            targetBadgeAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            SelectableUnit targetUnit = currentTarget.GetComponent<SelectableUnit>();
            bool isStructure = currentTarget.GetComponent<BaseStructure>() != null || currentTarget.GetComponent<ProductionStructure>() != null;
            bool isAir = targetUnit != null && targetUnit.Definition != null && targetUnit.Definition.IsFlying;
            Color badgeColor = isAir
                ? new Color(1f, 0.86f, 0.34f)
                : isStructure
                    ? new Color(0.92f, 0.64f, 0.28f)
                    : new Color(0.78f, 0.88f, 1f);
            float pulse = 0.9f + Mathf.PingPong(Time.time * (isAir ? 4.4f : 2.4f), isAir ? 0.22f : 0.12f);

            targetBadgeAnchor.localPosition = new Vector3(0f, 1.92f + Mathf.PingPong(Time.time * 1.1f, 0.04f), 0f);

            if (targetBadgePrimary != null)
            {
                targetBadgePrimary.localPosition = Vector3.zero;
                targetBadgePrimary.localScale = isAir
                    ? new Vector3(0.22f, 0.03f, 0.08f)
                    : isStructure
                        ? new Vector3(0.16f, 0.12f, 0.16f)
                        : new Vector3(0.08f, 0.14f, 0.08f);
            }

            if (targetBadgeSecondary != null)
            {
                bool showSecondary = isAir || isStructure;
                targetBadgeSecondary.gameObject.SetActive(showSecondary);
                if (showSecondary)
                {
                    targetBadgeSecondary.localPosition = isAir ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0.14f, 0f);
                    targetBadgeSecondary.localScale = isAir
                        ? new Vector3(0.08f, 0.03f, 0.22f)
                        : new Vector3(0.08f, 0.08f, 0.08f);
                }
            }

            if (targetBadgePrimaryRenderer != null)
            {
                targetBadgePrimaryRenderer.material.color = badgeColor * pulse;
            }

            if (targetBadgeSecondaryRenderer != null && targetBadgeSecondary.gameObject.activeSelf)
            {
                targetBadgeSecondaryRenderer.material.color = Color.Lerp(badgeColor, Color.white, 0.18f) * pulse;
            }
        }

        private void EnsureOverloadVisuals()
        {
            if (overloadAnchor != null)
            {
                return;
            }

            overloadAnchor = new GameObject("Turret Overload Anchor").transform;
            overloadAnchor.SetParent(transform);
            overloadAnchor.localPosition = new Vector3(0f, 2.2f, 0f);
            overloadAnchor.localRotation = Quaternion.identity;
            overloadAnchor.localScale = Vector3.one;

            overloadRing = CreateRangePrimitive(
                overloadAnchor,
                PrimitiveType.Cylinder,
                "Turret Overload Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.16f, 0.025f, 0.16f),
                new Color(1f, 0.62f, 0.24f));
            overloadRingRenderer = overloadRing.GetComponent<Renderer>();

            overloadCore = CreateRangePrimitive(
                overloadAnchor,
                PrimitiveType.Sphere,
                "Turret Overload Core",
                new Vector3(0f, 0.12f, 0f),
                new Vector3(0.12f, 0.12f, 0.12f),
                new Color(1f, 0.62f, 0.24f));
            overloadCoreRenderer = overloadCore.GetComponent<Renderer>();

            overloadDirectionRoot = new GameObject("Turret Overload Direction Root").transform;
            overloadDirectionRoot.SetParent(overloadAnchor);
            overloadDirectionRoot.localPosition = new Vector3(0f, 0.02f, 0f);
            overloadDirectionRoot.localRotation = Quaternion.identity;
            overloadDirectionRoot.localScale = Vector3.one;

            overloadDirectionBeam = CreateRangePrimitive(
                overloadDirectionRoot,
                PrimitiveType.Cube,
                "Turret Overload Direction Beam",
                new Vector3(0f, 0f, 0.22f),
                new Vector3(0.03f, 0.03f, 0.44f),
                new Color(1f, 0.62f, 0.24f));
            overloadDirectionBeamRenderer = overloadDirectionBeam.GetComponent<Renderer>();

            overloadDirectionTip = CreateRangePrimitive(
                overloadDirectionRoot,
                PrimitiveType.Cube,
                "Turret Overload Direction Tip",
                new Vector3(0f, 0f, 0.48f),
                new Vector3(0.1f, 0.08f, 0.1f),
                new Color(1f, 0.62f, 0.24f));
            overloadDirectionTipRenderer = overloadDirectionTip.GetComponent<Renderer>();
        }

        private void UpdateOverloadVisuals()
        {
            if (overloadAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            Vector3 pressureDirection = Vector3.zero;
            int hostileCount = alive ? CountHostilesInRange(out pressureDirection) : 0;
            bool show = alive && hostileCount > 1;
            overloadAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float overload = Mathf.Clamp01((hostileCount - 1f) / 4f);
            float pulse = 0.9f + Mathf.PingPong(Time.time * (2.8f + overload * 3.2f), 0.14f + overload * 0.16f);
            Color overloadColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.44f, 0.92f, 1f)
                : new Color(1f, 0.62f, 0.24f);
            float ringRadius = 0.15f + overload * 0.16f;
            float coreScale = 0.11f + overload * 0.14f;

            overloadAnchor.localPosition = new Vector3(0f, 2.2f + Mathf.PingPong(Time.time * 1.3f, 0.05f + overload * 0.04f), 0f);

            if (overloadRing != null)
            {
                overloadRing.localScale = new Vector3(ringRadius, 0.025f, ringRadius);
            }

            if (overloadRingRenderer != null)
            {
                overloadRingRenderer.material.color = overloadColor * pulse;
            }

            if (overloadCore != null)
            {
                overloadCore.localScale = Vector3.one * coreScale;
            }

            if (overloadCoreRenderer != null)
            {
                overloadCoreRenderer.material.color = Color.Lerp(overloadColor, Color.white, 0.18f + overload * 0.12f) * pulse;
            }

            bool showDirection = overloadDirectionRoot != null && pressureDirection.sqrMagnitude > 0.0001f;
            if (overloadDirectionRoot != null)
            {
                overloadDirectionRoot.gameObject.SetActive(showDirection);
            }

            if (!showDirection)
            {
                return;
            }

            float directionLength = 0.36f + overload * 0.24f;
            overloadDirectionRoot.localRotation = Quaternion.LookRotation(pressureDirection.normalized, Vector3.up);

            if (overloadDirectionBeam != null)
            {
                overloadDirectionBeam.localPosition = new Vector3(0f, 0f, directionLength * 0.5f);
                overloadDirectionBeam.localScale = new Vector3(0.03f, 0.03f, directionLength);
            }

            if (overloadDirectionBeamRenderer != null)
            {
                overloadDirectionBeamRenderer.material.color = overloadColor * pulse;
            }

            if (overloadDirectionTip != null)
            {
                overloadDirectionTip.localPosition = new Vector3(0f, 0f, directionLength + 0.08f);
                overloadDirectionTip.localScale = new Vector3(0.08f + overload * 0.04f, 0.08f, 0.08f + overload * 0.04f);
            }

            if (overloadDirectionTipRenderer != null)
            {
                overloadDirectionTipRenderer.material.color = Color.Lerp(overloadColor, Color.white, 0.18f) * pulse;
            }
        }

        private int CountHostilesInRange(out Vector3 pressureDirection)
        {
            pressureDirection = Vector3.zero;
            int count = 0;
            Vector3 centroid = Vector3.zero;

            foreach (CombatTarget target in PrototypeRuntimeRegistry.GetCombatTargets())
            {
                if (target == null || target == owner || !target.IsAlive || owner == null || target.Team == owner.Team)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
                {
                    continue;
                }

                count++;
                centroid += target.transform.position;
            }

            if (count > 0)
            {
                pressureDirection = centroid / count - transform.position;
                pressureDirection.y = 0f;
                if (pressureDirection.sqrMagnitude <= 0.0001f)
                {
                    pressureDirection = Vector3.forward;
                }
            }

            return count;
        }

        private void EnsureRangeStateVisuals()
        {
            if (rangeStateAnchor != null)
            {
                return;
            }

            rangeStateAnchor = new GameObject("Turret Range State Anchor").transform;
            rangeStateAnchor.SetParent(transform);
            rangeStateAnchor.localPosition = new Vector3(0f, 2.46f, 0f);
            rangeStateAnchor.localRotation = Quaternion.identity;
            rangeStateAnchor.localScale = Vector3.one;

            rangeStateBar = CreateRangePrimitive(
                rangeStateAnchor,
                PrimitiveType.Cube,
                "Turret Range State Bar",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.42f, 0.04f, 0.08f),
                new Color(0.18f, 0.18f, 0.22f));
            rangeStateBarRenderer = rangeStateBar.GetComponent<Renderer>();

            rangeStateNeedle = CreateRangePrimitive(
                rangeStateAnchor,
                PrimitiveType.Cube,
                "Turret Range State Needle",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(0.08f, 0.12f, 0.08f),
                new Color(0.34f, 0.92f, 1f));
            rangeStateNeedleRenderer = rangeStateNeedle.GetComponent<Renderer>();
        }

        private void UpdateRangeStateVisuals()
        {
            if (rangeStateAnchor == null)
            {
                return;
            }

            bool show = health != null && health.IsAlive && currentTarget != null && currentTarget.IsAlive;
            rangeStateAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            float normalizedDistance = attackRange > 0.001f ? Mathf.Clamp01(distance / attackRange) : 1f;
            float pulse = 0.88f + Mathf.PingPong(Time.time * (normalizedDistance > 0.72f ? 3.8f : 2f), normalizedDistance > 0.72f ? 0.18f : 0.08f);
            Color secureColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.44f, 0.92f, 1f)
                : new Color(0.34f, 0.92f, 1f);
            Color edgeColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.82f, 0.82f, 0.9f)
                : new Color(1f, 0.72f, 0.24f);
            Color distanceColor = Color.Lerp(secureColor, edgeColor, normalizedDistance);

            rangeStateAnchor.localPosition = new Vector3(0f, 2.46f + Mathf.PingPong(Time.time * 1f, 0.03f), 0f);

            if (rangeStateBarRenderer != null)
            {
                rangeStateBarRenderer.material.color = Color.Lerp(new Color(0.18f, 0.18f, 0.22f), distanceColor, 0.7f) * pulse;
            }

            if (rangeStateNeedle != null)
            {
                float needleX = Mathf.Lerp(-0.18f, 0.18f, normalizedDistance);
                rangeStateNeedle.localPosition = new Vector3(needleX, 0.08f, 0f);
                rangeStateNeedle.localScale = new Vector3(0.07f, 0.1f + normalizedDistance * 0.08f, 0.08f);
            }

            if (rangeStateNeedleRenderer != null)
            {
                rangeStateNeedleRenderer.material.color = Color.Lerp(distanceColor, Color.white, 0.18f) * pulse;
            }
        }

        private void EnsureWatchVisuals()
        {
            if (watchAnchor != null)
            {
                return;
            }

            watchAnchor = new GameObject("Turret Watch Anchor").transform;
            watchAnchor.SetParent(transform);
            watchAnchor.localPosition = new Vector3(0f, 2.72f, 0f);
            watchAnchor.localRotation = Quaternion.identity;
            watchAnchor.localScale = Vector3.one;

            watchBeam = CreateRangePrimitive(
                watchAnchor,
                PrimitiveType.Cube,
                "Turret Watch Beam",
                new Vector3(0f, 0f, 0.24f),
                new Vector3(0.025f, 0.025f, 0.48f),
                new Color(0.82f, 0.82f, 0.9f));
            watchBeamRenderer = watchBeam.GetComponent<Renderer>();

            watchTip = CreateRangePrimitive(
                watchAnchor,
                PrimitiveType.Sphere,
                "Turret Watch Tip",
                new Vector3(0f, 0f, 0.5f),
                new Vector3(0.1f, 0.1f, 0.1f),
                new Color(0.82f, 0.82f, 0.9f));
            watchTipRenderer = watchTip.GetComponent<Renderer>();

            watchAlertRing = CreateRangePrimitive(
                watchAnchor,
                PrimitiveType.Cylinder,
                "Turret Watch Alert Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.72f, 0.24f));
            watchAlertRingRenderer = watchAlertRing.GetComponent<Renderer>();

            watchAlertCore = CreateRangePrimitive(
                watchAnchor,
                PrimitiveType.Sphere,
                "Turret Watch Alert Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.1f, 0.1f, 0.1f),
                new Color(1f, 0.72f, 0.24f));
            watchAlertCoreRenderer = watchAlertCore.GetComponent<Renderer>();
        }

        private void UpdateWatchVisuals()
        {
            if (watchAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            float watchDistance = attackRange * 1.35f;
            int watchCount = 0;
            CombatTarget watchTarget = alive && (currentTarget == null || !currentTarget.IsAlive)
                ? FindClosestThreatInWatchBand(out watchDistance, out watchCount)
                : null;
            bool show = watchTarget != null;
            watchAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            Vector3 targetPosition = watchTarget.transform.position + Vector3.up * 0.72f;
            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.4f, 0.6f), localTarget.z);
            if (planarTarget.sqrMagnitude <= 0.0001f)
            {
                planarTarget = Vector3.forward * 0.24f;
            }

            float distance = Mathf.Max(0.2f, planarTarget.magnitude);
            Vector3 direction = planarTarget.normalized;
            float normalizedDistance = Mathf.Clamp01(distance / Mathf.Max(attackRange * 1.35f, 0.01f));
            float urgency = Mathf.Clamp01(1f - Mathf.InverseLerp(attackRange, attackRange * 1.35f, watchDistance));
            urgency = Mathf.Clamp01(Mathf.Max(urgency, (watchCount - 1f) / 3f));
            float pulse = 0.86f + Mathf.PingPong(Time.time * (1.8f + urgency * 2.2f), 0.08f + urgency * 0.1f);
            Color watchColor = Color.Lerp(new Color(0.84f, 0.84f, 0.9f), owner != null && owner.Team == UnitTeam.Enemy ? new Color(0.44f, 0.92f, 1f) : new Color(1f, 0.72f, 0.24f), 0.35f);

            watchAnchor.localPosition = new Vector3(0f, 2.72f + Mathf.PingPong(Time.time * 0.9f, 0.03f), 0f);
            watchAnchor.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (watchBeam != null)
            {
                float beamLength = Mathf.Lerp(0.28f, 0.54f, normalizedDistance);
                watchBeam.localPosition = new Vector3(0f, 0f, beamLength * 0.5f);
                watchBeam.localScale = new Vector3(0.025f, 0.025f, beamLength);
            }

            if (watchBeamRenderer != null)
            {
                watchBeamRenderer.material.color = watchColor * pulse;
            }

            if (watchTip != null)
            {
                watchTip.localPosition = new Vector3(0f, 0f, watchBeam != null ? watchBeam.localScale.z + 0.06f : 0.4f);
                watchTip.localScale = Vector3.one * 0.09f;
            }

            if (watchTipRenderer != null)
            {
                watchTipRenderer.material.color = Color.Lerp(watchColor, Color.white, 0.16f) * pulse;
            }

            if (watchAlertRing != null)
            {
                watchAlertRing.localScale = new Vector3(0.12f + urgency * 0.14f, 0.02f, 0.12f + urgency * 0.14f);
            }

            if (watchAlertRingRenderer != null)
            {
                watchAlertRingRenderer.material.color = Color.Lerp(watchColor, Color.white, 0.08f + urgency * 0.18f) * pulse;
            }

            if (watchAlertCore != null)
            {
                watchAlertCore.localScale = Vector3.one * (0.08f + urgency * 0.12f);
            }

            if (watchAlertCoreRenderer != null)
            {
                watchAlertCoreRenderer.material.color = Color.Lerp(watchColor, Color.white, 0.16f + urgency * 0.16f) * pulse;
            }
        }

        private CombatTarget FindClosestThreatInWatchBand(out float closestDistance, out int threatCount)
        {
            CombatTarget bestTarget = null;
            float bestDistance = attackRange * 1.35f;
            threatCount = 0;

            foreach (CombatTarget target in PrototypeRuntimeRegistry.GetCombatTargets())
            {
                if (target == null || target == owner || !target.IsAlive || owner == null || target.Team == owner.Team)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= attackRange || distance > attackRange * 1.35f)
                {
                    continue;
                }

                threatCount++;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            closestDistance = bestDistance;
            return bestTarget;
        }

        private void EnsureSupportVisuals()
        {
            if (supportAnchor != null)
            {
                return;
            }

            supportAnchor = new GameObject("Turret Support Anchor").transform;
            supportAnchor.SetParent(transform);
            supportAnchor.localPosition = new Vector3(0f, 3f, 0f);
            supportAnchor.localRotation = Quaternion.identity;
            supportAnchor.localScale = Vector3.one;

            supportRing = CreateRangePrimitive(
                supportAnchor,
                PrimitiveType.Cylinder,
                "Turret Support Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(0.44f, 1f, 0.86f));
            supportRingRenderer = supportRing.GetComponent<Renderer>();

            for (int i = 0; i < supportPips.Length; i++)
            {
                supportPips[i] = CreateRangePrimitive(
                    supportAnchor,
                    PrimitiveType.Sphere,
                    $"Turret Support Pip {i + 1}",
                    Vector3.zero,
                    new Vector3(0.08f, 0.08f, 0.08f),
                    new Color(0.44f, 1f, 0.86f));
                supportPipRenderers[i] = supportPips[i].GetComponent<Renderer>();
            }
        }

        private void UpdateSupportVisuals()
        {
            if (supportAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            int supportCount = alive ? CountNearbyAlliedTurrets() : 0;
            bool show = alive && supportCount > 0;
            supportAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float supportStrength = Mathf.Clamp01(supportCount / 3f);
            float pulse = 0.88f + Mathf.PingPong(Time.time * (1.8f + supportStrength), 0.08f + supportStrength * 0.08f);
            Color supportColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.44f, 0.92f, 1f)
                : new Color(0.44f, 1f, 0.86f);

            supportAnchor.localPosition = new Vector3(0f, 3f + Mathf.PingPong(Time.time * 0.9f, 0.03f), 0f);

            if (supportRing != null)
            {
                supportRing.localScale = new Vector3(0.12f + supportStrength * 0.12f, 0.02f, 0.12f + supportStrength * 0.12f);
            }

            if (supportRingRenderer != null)
            {
                supportRingRenderer.material.color = supportColor * pulse;
            }

            for (int i = 0; i < supportPips.Length; i++)
            {
                bool active = i < Mathf.Min(supportCount, supportPips.Length);
                if (supportPips[i] != null)
                {
                    supportPips[i].gameObject.SetActive(active);
                }

                if (!active || supportPips[i] == null || supportPipRenderers[i] == null)
                {
                    continue;
                }

                float angle = i / (float)supportPips.Length * Mathf.PI * 2f + Time.time * 0.3f;
                supportPips[i].localPosition = new Vector3(Mathf.Cos(angle), 0.1f, Mathf.Sin(angle)) * (0.16f + supportStrength * 0.08f);
                supportPipRenderers[i].material.color = Color.Lerp(supportColor, Color.white, 0.14f + i * 0.06f) * pulse;
            }
        }

        private int CountNearbyAlliedTurrets()
        {
            int count = 0;

            foreach (DefensiveTurret turret in PrototypeRuntimeRegistry.GetDefensiveTurrets())
            {
                if (turret == null || turret == this || turret.owner == null || owner == null || turret.owner.Team != owner.Team)
                {
                    continue;
                }

                if (turret.health == null || !turret.health.IsAlive)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, turret.transform.position) > supportLinkRadius)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private void EnsureIsolationVisuals()
        {
            if (isolationAnchor != null)
            {
                return;
            }

            isolationAnchor = new GameObject("Turret Isolation Anchor").transform;
            isolationAnchor.SetParent(transform);
            isolationAnchor.localPosition = new Vector3(0f, 3.26f, 0f);
            isolationAnchor.localRotation = Quaternion.identity;
            isolationAnchor.localScale = Vector3.one;

            isolationRing = CreateRangePrimitive(
                isolationAnchor,
                PrimitiveType.Cylinder,
                "Turret Isolation Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.16f, 0.02f, 0.16f),
                new Color(1f, 0.58f, 0.24f));
            isolationRingRenderer = isolationRing.GetComponent<Renderer>();

            isolationCore = CreateRangePrimitive(
                isolationAnchor,
                PrimitiveType.Cube,
                "Turret Isolation Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.12f, 0.24f, 0.12f),
                new Color(1f, 0.58f, 0.24f));
            isolationCoreRenderer = isolationCore.GetComponent<Renderer>();
        }

        private void UpdateIsolationVisuals()
        {
            if (isolationAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            int supportCount = alive ? CountNearbyAlliedTurrets() : 0;
            float watchDistance = attackRange * 1.35f;
            int watchCount = 0;
            CombatTarget watchThreat = alive && (currentTarget == null || !currentTarget.IsAlive)
                ? FindClosestThreatInWatchBand(out watchDistance, out watchCount)
                : null;
            bool activeThreat = currentTarget != null && currentTarget.IsAlive;
            bool show = alive && supportCount == 0 && (activeThreat || watchThreat != null);
            isolationAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float urgency = activeThreat
                ? 1f
                : Mathf.Clamp01(1f - Mathf.InverseLerp(attackRange, attackRange * 1.35f, watchDistance));
            urgency = Mathf.Clamp01(Mathf.Max(urgency, activeThreat ? 1f : (watchCount - 1f) / 3f));
            float pulse = 0.92f + Mathf.PingPong(Time.time * (2.8f + urgency * 3.2f), 0.14f + urgency * 0.16f);
            Color isolationColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.44f, 0.92f, 1f)
                : new Color(1f, 0.58f, 0.24f);

            isolationAnchor.localPosition = new Vector3(0f, 3.26f + Mathf.PingPong(Time.time * 1f, 0.04f + urgency * 0.04f), 0f);

            if (isolationRing != null)
            {
                isolationRing.localScale = new Vector3(0.14f + urgency * 0.16f, 0.02f, 0.14f + urgency * 0.16f);
            }

            if (isolationRingRenderer != null)
            {
                isolationRingRenderer.material.color = isolationColor * pulse;
            }

            if (isolationCore != null)
            {
                isolationCore.localScale = new Vector3(0.1f + urgency * 0.08f, 0.18f + urgency * 0.22f, 0.1f + urgency * 0.08f);
                isolationCore.localPosition = new Vector3(0f, 0.08f + isolationCore.localScale.y * 0.5f, 0f);
            }

            if (isolationCoreRenderer != null)
            {
                isolationCoreRenderer.material.color = Color.Lerp(isolationColor, Color.white, 0.18f + urgency * 0.14f) * pulse;
            }
        }

        private void EnsureFocusVisuals()
        {
            if (focusAnchor != null)
            {
                return;
            }

            focusAnchor = new GameObject("Turret Focus Anchor").transform;
            focusAnchor.SetParent(transform);
            focusAnchor.localPosition = new Vector3(0f, 3.54f, 0f);
            focusAnchor.localRotation = Quaternion.identity;
            focusAnchor.localScale = Vector3.one;

            focusRing = CreateRangePrimitive(
                focusAnchor,
                PrimitiveType.Cylinder,
                "Turret Focus Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.84f, 0.3f));
            focusRingRenderer = focusRing.GetComponent<Renderer>();

            for (int i = 0; i < focusPips.Length; i++)
            {
                focusPips[i] = CreateRangePrimitive(
                    focusAnchor,
                    PrimitiveType.Sphere,
                    $"Turret Focus Pip {i + 1}",
                    Vector3.zero,
                    new Vector3(0.08f, 0.08f, 0.08f),
                    new Color(1f, 0.84f, 0.3f));
                focusPipRenderers[i] = focusPips[i].GetComponent<Renderer>();
            }
        }

        private void UpdateFocusVisuals()
        {
            if (focusAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            int focusCount = alive && currentTarget != null && currentTarget.IsAlive ? CountAlliedTurretsOnSameTarget() : 0;
            bool show = alive && focusCount > 0;
            focusAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float focusStrength = Mathf.Clamp01(focusCount / 3f);
            float pulse = 0.9f + Mathf.PingPong(Time.time * (2.2f + focusStrength * 1.8f), 0.12f + focusStrength * 0.1f);
            Color focusColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.7f, 0.3f)
                : new Color(1f, 0.84f, 0.3f);

            focusAnchor.localPosition = new Vector3(0f, 3.54f + Mathf.PingPong(Time.time * 1f, 0.03f), 0f);

            if (focusRing != null)
            {
                focusRing.localScale = new Vector3(0.12f + focusStrength * 0.12f, 0.02f, 0.12f + focusStrength * 0.12f);
            }

            if (focusRingRenderer != null)
            {
                focusRingRenderer.material.color = focusColor * pulse;
            }

            for (int i = 0; i < focusPips.Length; i++)
            {
                bool active = i < Mathf.Min(focusCount, focusPips.Length);
                if (focusPips[i] != null)
                {
                    focusPips[i].gameObject.SetActive(active);
                }

                if (!active || focusPips[i] == null || focusPipRenderers[i] == null)
                {
                    continue;
                }

                float angle = i / (float)focusPips.Length * Mathf.PI * 2f - Time.time * 0.45f;
                focusPips[i].localPosition = new Vector3(Mathf.Cos(angle), 0.12f, Mathf.Sin(angle)) * (0.14f + focusStrength * 0.08f);
                focusPipRenderers[i].material.color = Color.Lerp(focusColor, Color.white, 0.16f + i * 0.06f) * pulse;
            }
        }

        private int CountAlliedTurretsOnSameTarget()
        {
            if (currentTarget == null)
            {
                return 0;
            }

            int count = 0;

            foreach (DefensiveTurret turret in PrototypeRuntimeRegistry.GetDefensiveTurrets())
            {
                if (turret == null || turret == this || turret.owner == null || owner == null || turret.owner.Team != owner.Team)
                {
                    continue;
                }

                if (turret.health == null || !turret.health.IsAlive)
                {
                    continue;
                }

                if (turret.currentTarget != currentTarget)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private void EnsureSpreadVisuals()
        {
            if (spreadAnchor != null)
            {
                return;
            }

            spreadAnchor = new GameObject("Turret Spread Anchor").transform;
            spreadAnchor.SetParent(transform);
            spreadAnchor.localPosition = new Vector3(0f, 3.82f, 0f);
            spreadAnchor.localRotation = Quaternion.identity;
            spreadAnchor.localScale = Vector3.one;

            spreadRing = CreateRangePrimitive(
                spreadAnchor,
                PrimitiveType.Cylinder,
                "Turret Spread Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(0.54f, 1f, 0.52f));
            spreadRingRenderer = spreadRing.GetComponent<Renderer>();

            for (int i = 0; i < spreadPips.Length; i++)
            {
                spreadPips[i] = CreateRangePrimitive(
                    spreadAnchor,
                    PrimitiveType.Cube,
                    $"Turret Spread Pip {i + 1}",
                    Vector3.zero,
                    new Vector3(0.08f, 0.08f, 0.08f),
                    new Color(0.54f, 1f, 0.52f));
                spreadPipRenderers[i] = spreadPips[i].GetComponent<Renderer>();
            }
        }

        private void UpdateSpreadVisuals()
        {
            if (spreadAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            int spreadCount = alive && currentTarget != null && currentTarget.IsAlive ? CountAlliedTurretsOnDifferentTargets() : 0;
            bool show = alive && spreadCount > 0;
            spreadAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float spreadStrength = Mathf.Clamp01(spreadCount / 3f);
            float pulse = 0.88f + Mathf.PingPong(Time.time * (1.8f + spreadStrength * 1.6f), 0.08f + spreadStrength * 0.08f);
            Color spreadColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.62f, 1f, 0.66f)
                : new Color(0.54f, 1f, 0.52f);

            spreadAnchor.localPosition = new Vector3(0f, 3.82f + Mathf.PingPong(Time.time * 0.9f, 0.03f), 0f);

            if (spreadRing != null)
            {
                spreadRing.localScale = new Vector3(0.12f + spreadStrength * 0.12f, 0.02f, 0.12f + spreadStrength * 0.12f);
            }

            if (spreadRingRenderer != null)
            {
                spreadRingRenderer.material.color = spreadColor * pulse;
            }

            for (int i = 0; i < spreadPips.Length; i++)
            {
                bool active = i < Mathf.Min(spreadCount, spreadPips.Length);
                if (spreadPips[i] != null)
                {
                    spreadPips[i].gameObject.SetActive(active);
                }

                if (!active || spreadPips[i] == null || spreadPipRenderers[i] == null)
                {
                    continue;
                }

                float angle = i / (float)spreadPips.Length * Mathf.PI * 2f + Time.time * 0.28f;
                spreadPips[i].localPosition = new Vector3(Mathf.Cos(angle), 0.12f, Mathf.Sin(angle)) * (0.14f + spreadStrength * 0.08f);
                spreadPips[i].localScale = new Vector3(0.08f, 0.08f, 0.08f + spreadStrength * 0.04f);
                spreadPipRenderers[i].material.color = Color.Lerp(spreadColor, Color.white, 0.14f + i * 0.06f) * pulse;
            }
        }

        private int CountAlliedTurretsOnDifferentTargets()
        {
            if (currentTarget == null)
            {
                return 0;
            }

            int count = 0;

            foreach (DefensiveTurret turret in PrototypeRuntimeRegistry.GetDefensiveTurrets())
            {
                if (turret == null || turret == this || turret.owner == null || owner == null || turret.owner.Team != owner.Team)
                {
                    continue;
                }

                if (turret.health == null || !turret.health.IsAlive || turret.currentTarget == null || !turret.currentTarget.IsAlive)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, turret.transform.position) > supportLinkRadius)
                {
                    continue;
                }

                if (turret.currentTarget == currentTarget)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private void EnsureRetargetVisuals()
        {
            if (retargetAnchor != null)
            {
                return;
            }

            retargetAnchor = new GameObject("Turret Retarget Anchor").transform;
            retargetAnchor.SetParent(transform);
            retargetAnchor.localPosition = new Vector3(0f, 4.1f, 0f);
            retargetAnchor.localRotation = Quaternion.identity;
            retargetAnchor.localScale = Vector3.one;

            retargetRing = CreateRangePrimitive(
                retargetAnchor,
                PrimitiveType.Cylinder,
                "Turret Retarget Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.74f, 0.28f));
            retargetRingRenderer = retargetRing.GetComponent<Renderer>();

            retargetCore = CreateRangePrimitive(
                retargetAnchor,
                PrimitiveType.Cube,
                "Turret Retarget Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.12f, 0.2f, 0.12f),
                new Color(1f, 0.74f, 0.28f));
            retargetCoreRenderer = retargetCore.GetComponent<Renderer>();
        }

        private void UpdateRetargetVisuals()
        {
            if (retargetAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            bool show = alive && retargetHeat > 0.08f;
            retargetAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float pulse = 0.9f + Mathf.PingPong(Time.time * (2f + retargetHeat * 3.2f), 0.1f + retargetHeat * 0.14f);
            Color retargetColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.78f, 0.34f)
                : new Color(1f, 0.74f, 0.28f);

            retargetAnchor.localPosition = new Vector3(0f, 4.1f + Mathf.PingPong(Time.time * 0.9f, 0.03f + retargetHeat * 0.03f), 0f);

            if (retargetRing != null)
            {
                retargetRing.localScale = new Vector3(0.12f + retargetHeat * 0.16f, 0.02f, 0.12f + retargetHeat * 0.16f);
            }

            if (retargetRingRenderer != null)
            {
                retargetRingRenderer.material.color = retargetColor * pulse;
            }

            if (retargetCore != null)
            {
                retargetCore.localScale = new Vector3(0.1f + retargetHeat * 0.08f, 0.16f + retargetHeat * 0.2f, 0.1f + retargetHeat * 0.08f);
                retargetCore.localPosition = new Vector3(0f, 0.08f + retargetCore.localScale.y * 0.5f, 0f);
            }

            if (retargetCoreRenderer != null)
            {
                retargetCoreRenderer.material.color = Color.Lerp(retargetColor, Color.white, 0.18f + retargetHeat * 0.16f) * pulse;
            }
        }

        private void UpdateRetargetHeat()
        {
            if (retargetHeat > 0f)
            {
                retargetHeat = Mathf.Max(0f, retargetHeat - Time.deltaTime * 0.28f);
            }
        }

        private void TrackRetargetChange()
        {
            if (currentTarget == trackedVisualTarget)
            {
                return;
            }

            if (trackedVisualTarget != null && currentTarget != trackedVisualTarget)
            {
                retargetHeat = Mathf.Clamp01(retargetHeat + (currentTarget != null ? 0.38f : 0.2f));
            }

            trackedVisualTarget = currentTarget;
        }

        private void EnsureVolleyVisuals()
        {
            if (volleyAnchor != null)
            {
                return;
            }

            volleyAnchor = new GameObject("Turret Volley Anchor").transform;
            volleyAnchor.SetParent(transform);
            volleyAnchor.localPosition = new Vector3(0f, 4.38f, 0f);
            volleyAnchor.localRotation = Quaternion.identity;
            volleyAnchor.localScale = Vector3.one;

            volleyRing = CreateRangePrimitive(
                volleyAnchor,
                PrimitiveType.Cylinder,
                "Turret Volley Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.9f, 0.34f));
            volleyRingRenderer = volleyRing.GetComponent<Renderer>();

            for (int i = 0; i < volleyPips.Length; i++)
            {
                volleyPips[i] = CreateRangePrimitive(
                    volleyAnchor,
                    PrimitiveType.Cube,
                    $"Turret Volley Pip {i + 1}",
                    Vector3.zero,
                    new Vector3(0.08f, 0.08f, 0.08f),
                    new Color(1f, 0.9f, 0.34f));
                volleyPipRenderers[i] = volleyPips[i].GetComponent<Renderer>();
            }
        }

        private void UpdateVolleyVisuals()
        {
            if (volleyAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            int volleyCount = alive ? CountReadyAlliedTurrets() : 0;
            bool show = alive && volleyCount > 0;
            volleyAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float volleyStrength = Mathf.Clamp01(volleyCount / 3f);
            float pulse = 0.9f + Mathf.PingPong(Time.time * (2.4f + volleyStrength * 2f), 0.12f + volleyStrength * 0.1f);
            Color volleyColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.82f, 0.34f)
                : new Color(1f, 0.9f, 0.34f);

            volleyAnchor.localPosition = new Vector3(0f, 4.38f + Mathf.PingPong(Time.time * 0.9f, 0.03f), 0f);

            if (volleyRing != null)
            {
                volleyRing.localScale = new Vector3(0.12f + volleyStrength * 0.12f, 0.02f, 0.12f + volleyStrength * 0.12f);
            }

            if (volleyRingRenderer != null)
            {
                volleyRingRenderer.material.color = volleyColor * pulse;
            }

            for (int i = 0; i < volleyPips.Length; i++)
            {
                bool active = i < Mathf.Min(volleyCount, volleyPips.Length);
                if (volleyPips[i] != null)
                {
                    volleyPips[i].gameObject.SetActive(active);
                }

                if (!active || volleyPips[i] == null || volleyPipRenderers[i] == null)
                {
                    continue;
                }

                float angle = i / (float)volleyPips.Length * Mathf.PI * 2f - Time.time * 0.35f;
                volleyPips[i].localPosition = new Vector3(Mathf.Cos(angle), 0.12f, Mathf.Sin(angle)) * (0.14f + volleyStrength * 0.08f);
                volleyPips[i].localScale = new Vector3(0.08f, 0.08f + volleyStrength * 0.04f, 0.08f);
                volleyPipRenderers[i].material.color = Color.Lerp(volleyColor, Color.white, 0.16f + i * 0.06f) * pulse;
            }
        }

        private int CountReadyAlliedTurrets()
        {
            int count = 0;

            foreach (DefensiveTurret turret in PrototypeRuntimeRegistry.GetDefensiveTurrets())
            {
                if (turret == null || turret == this || turret.owner == null || owner == null || turret.owner.Team != owner.Team)
                {
                    continue;
                }

                if (turret.health == null || !turret.health.IsAlive || turret.currentTarget == null || !turret.currentTarget.IsAlive)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, turret.transform.position) > supportLinkRadius)
                {
                    continue;
                }

                if (turret.cooldownTimer > turret.attackCooldown * 0.25f)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private void EnsureGapVisuals()
        {
            if (gapAnchor != null)
            {
                return;
            }

            gapAnchor = new GameObject("Turret Gap Anchor").transform;
            gapAnchor.SetParent(transform);
            gapAnchor.localPosition = new Vector3(0f, 4.66f, 0f);
            gapAnchor.localRotation = Quaternion.identity;
            gapAnchor.localScale = Vector3.one;

            gapRing = CreateRangePrimitive(
                gapAnchor,
                PrimitiveType.Cylinder,
                "Turret Gap Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.54f, 0.24f));
            gapRingRenderer = gapRing.GetComponent<Renderer>();

            gapCore = CreateRangePrimitive(
                gapAnchor,
                PrimitiveType.Cube,
                "Turret Gap Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.12f, 0.22f, 0.12f),
                new Color(1f, 0.54f, 0.24f));
            gapCoreRenderer = gapCore.GetComponent<Renderer>();
        }

        private void UpdateGapVisuals()
        {
            if (gapAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            GetLocalFireWindowStats(out int engagedCount, out int readyCount, out float bestReadiness);
            bool show = alive && engagedCount > 0 && readyCount == 0;
            gapAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float gapSeverity = Mathf.Clamp01(1f - bestReadiness);
            float pulse = 0.92f + Mathf.PingPong(Time.time * (2.2f + gapSeverity * 2.8f), 0.12f + gapSeverity * 0.14f);
            Color gapColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.44f, 0.92f, 1f)
                : new Color(1f, 0.54f, 0.24f);

            gapAnchor.localPosition = new Vector3(0f, 4.66f + Mathf.PingPong(Time.time * 0.95f, 0.03f + gapSeverity * 0.03f), 0f);

            if (gapRing != null)
            {
                gapRing.localScale = new Vector3(0.12f + gapSeverity * 0.14f, 0.02f, 0.12f + gapSeverity * 0.14f);
            }

            if (gapRingRenderer != null)
            {
                gapRingRenderer.material.color = gapColor * pulse;
            }

            if (gapCore != null)
            {
                gapCore.localScale = new Vector3(0.1f + gapSeverity * 0.08f, 0.16f + gapSeverity * 0.22f, 0.1f + gapSeverity * 0.08f);
                gapCore.localPosition = new Vector3(0f, 0.08f + gapCore.localScale.y * 0.5f, 0f);
            }

            if (gapCoreRenderer != null)
            {
                gapCoreRenderer.material.color = Color.Lerp(gapColor, Color.white, 0.18f + gapSeverity * 0.16f) * pulse;
            }
        }

        private void GetLocalFireWindowStats(out int engagedCount, out int readyCount, out float bestReadiness)
        {
            engagedCount = 0;
            readyCount = 0;
            bestReadiness = 0f;

            foreach (DefensiveTurret turret in PrototypeRuntimeRegistry.GetDefensiveTurrets())
            {
                if (turret == null || turret.owner == null || owner == null || turret.owner.Team != owner.Team)
                {
                    continue;
                }

                if (turret.health == null || !turret.health.IsAlive || turret.currentTarget == null || !turret.currentTarget.IsAlive)
                {
                    continue;
                }

                if (turret != this && Vector3.Distance(transform.position, turret.transform.position) > supportLinkRadius)
                {
                    continue;
                }

                engagedCount++;

                float readiness = turret.attackCooldown > 0.001f
                    ? 1f - Mathf.Clamp01(turret.cooldownTimer / turret.attackCooldown)
                    : 1f;
                bestReadiness = Mathf.Max(bestReadiness, readiness);

                if (turret.cooldownTimer <= turret.attackCooldown * 0.25f)
                {
                    readyCount++;
                }
            }
        }

        private void EnsureLockVisuals()
        {
            if (lockAnchor != null)
            {
                return;
            }

            lockAnchor = new GameObject("Turret Lock Anchor").transform;
            lockAnchor.SetParent(transform);
            lockAnchor.localPosition = new Vector3(0f, 4.94f, 0f);
            lockAnchor.localRotation = Quaternion.identity;
            lockAnchor.localScale = Vector3.one;

            lockRing = CreateRangePrimitive(
                lockAnchor,
                PrimitiveType.Cylinder,
                "Turret Lock Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(0.52f, 1f, 0.78f));
            lockRingRenderer = lockRing.GetComponent<Renderer>();

            lockCore = CreateRangePrimitive(
                lockAnchor,
                PrimitiveType.Sphere,
                "Turret Lock Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.12f, 0.12f, 0.12f),
                new Color(0.52f, 1f, 0.78f));
            lockCoreRenderer = lockCore.GetComponent<Renderer>();
        }

        private void UpdateLockVisuals()
        {
            if (lockAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            float lockStrength = Mathf.Clamp01(targetLockTime / 3f);
            bool show = alive && currentTarget != null && currentTarget.IsAlive && lockStrength > 0.12f;
            lockAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float pulse = 0.88f + Mathf.PingPong(Time.time * (1.4f + lockStrength * 1.8f), 0.08f + lockStrength * 0.08f);
            Color lockColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(0.62f, 1f, 0.86f)
                : new Color(0.52f, 1f, 0.78f);

            lockAnchor.localPosition = new Vector3(0f, 4.94f + Mathf.PingPong(Time.time * 0.8f, 0.03f), 0f);

            if (lockRing != null)
            {
                lockRing.localScale = new Vector3(0.12f + lockStrength * 0.14f, 0.02f, 0.12f + lockStrength * 0.14f);
            }

            if (lockRingRenderer != null)
            {
                lockRingRenderer.material.color = lockColor * pulse;
            }

            if (lockCore != null)
            {
                lockCore.localScale = Vector3.one * (0.1f + lockStrength * 0.14f);
            }

            if (lockCoreRenderer != null)
            {
                lockCoreRenderer.material.color = Color.Lerp(lockColor, Color.white, 0.16f + lockStrength * 0.14f) * pulse;
            }
        }

        private void UpdateTargetLockState()
        {
            if (currentTarget == null || !currentTarget.IsAlive)
            {
                trackedLockTarget = null;
                targetLockTime = Mathf.Max(0f, targetLockTime - Time.deltaTime * 1.2f);
                return;
            }

            if (currentTarget != trackedLockTarget)
            {
                trackedLockTarget = currentTarget;
                targetLockTime = 0f;
                return;
            }

            targetLockTime = Mathf.Min(4f, targetLockTime + Time.deltaTime);
        }

        private void EnsureFinishVisuals()
        {
            if (finishAnchor != null)
            {
                return;
            }

            finishAnchor = new GameObject("Turret Finish Anchor").transform;
            finishAnchor.SetParent(transform);
            finishAnchor.localPosition = new Vector3(0f, 5.22f, 0f);
            finishAnchor.localRotation = Quaternion.identity;
            finishAnchor.localScale = Vector3.one;

            finishRing = CreateRangePrimitive(
                finishAnchor,
                PrimitiveType.Cylinder,
                "Turret Finish Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.38f, 0.24f));
            finishRingRenderer = finishRing.GetComponent<Renderer>();

            finishCore = CreateRangePrimitive(
                finishAnchor,
                PrimitiveType.Sphere,
                "Turret Finish Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.1f, 0.1f, 0.1f),
                new Color(1f, 0.38f, 0.24f));
            finishCoreRenderer = finishCore.GetComponent<Renderer>();
        }

        private void UpdateFinishVisuals()
        {
            if (finishAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            float targetHealthNormalized = currentTarget != null && currentTarget.IsAlive && currentTarget.Health != null
                ? currentTarget.Health.Normalized
                : 1f;
            float finishStrength = 1f - Mathf.InverseLerp(0.18f, 0.55f, targetHealthNormalized);
            bool show = alive && currentTarget != null && currentTarget.IsAlive && finishStrength > 0f;
            finishAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float pulse = 0.92f + Mathf.PingPong(Time.time * (2.6f + finishStrength * 3.4f), 0.14f + finishStrength * 0.16f);
            Color finishColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.54f, 0.28f)
                : new Color(1f, 0.38f, 0.24f);

            finishAnchor.localPosition = new Vector3(0f, 5.22f + Mathf.PingPong(Time.time * 1f, 0.03f + finishStrength * 0.04f), 0f);

            if (finishRing != null)
            {
                finishRing.localScale = new Vector3(0.12f + finishStrength * 0.16f, 0.02f, 0.12f + finishStrength * 0.16f);
            }

            if (finishRingRenderer != null)
            {
                finishRingRenderer.material.color = finishColor * pulse;
            }

            if (finishCore != null)
            {
                finishCore.localScale = Vector3.one * (0.08f + finishStrength * 0.14f);
            }

            if (finishCoreRenderer != null)
            {
                finishCoreRenderer.material.color = Color.Lerp(finishColor, Color.white, 0.18f + finishStrength * 0.16f) * pulse;
            }
        }

        private void EnsureOverkillVisuals()
        {
            if (overkillAnchor != null)
            {
                return;
            }

            overkillAnchor = new GameObject("Turret Overkill Anchor").transform;
            overkillAnchor.SetParent(transform);
            overkillAnchor.localPosition = new Vector3(0f, 5.5f, 0f);
            overkillAnchor.localRotation = Quaternion.identity;
            overkillAnchor.localScale = Vector3.one;

            overkillRing = CreateRangePrimitive(
                overkillAnchor,
                PrimitiveType.Cylinder,
                "Turret Overkill Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.14f, 0.02f, 0.14f),
                new Color(1f, 0.3f, 0.3f));
            overkillRingRenderer = overkillRing.GetComponent<Renderer>();

            overkillCore = CreateRangePrimitive(
                overkillAnchor,
                PrimitiveType.Cube,
                "Turret Overkill Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.12f, 0.18f, 0.12f),
                new Color(1f, 0.3f, 0.3f));
            overkillCoreRenderer = overkillCore.GetComponent<Renderer>();
        }

        private void UpdateOverkillVisuals()
        {
            if (overkillAnchor == null)
            {
                return;
            }

            bool alive = health != null && health.IsAlive;
            float targetHealthNormalized = currentTarget != null && currentTarget.IsAlive && currentTarget.Health != null
                ? currentTarget.Health.Normalized
                : 1f;
            int focusCount = alive && currentTarget != null && currentTarget.IsAlive ? CountAlliedTurretsOnSameTarget() : 0;
            float finishStrength = 1f - Mathf.InverseLerp(0.18f, 0.55f, targetHealthNormalized);
            float overkillStrength = Mathf.Clamp01(finishStrength * Mathf.Clamp01(focusCount / 3f));
            bool show = alive && currentTarget != null && currentTarget.IsAlive && focusCount > 0 && overkillStrength > 0.12f;
            overkillAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float pulse = 0.92f + Mathf.PingPong(Time.time * (2.8f + overkillStrength * 3f), 0.14f + overkillStrength * 0.14f);
            Color overkillColor = owner != null && owner.Team == UnitTeam.Enemy
                ? new Color(1f, 0.46f, 0.3f)
                : new Color(1f, 0.3f, 0.3f);

            overkillAnchor.localPosition = new Vector3(0f, 5.5f + Mathf.PingPong(Time.time * 1f, 0.03f + overkillStrength * 0.04f), 0f);

            if (overkillRing != null)
            {
                overkillRing.localScale = new Vector3(0.12f + overkillStrength * 0.16f, 0.02f, 0.12f + overkillStrength * 0.16f);
            }

            if (overkillRingRenderer != null)
            {
                overkillRingRenderer.material.color = overkillColor * pulse;
            }

            if (overkillCore != null)
            {
                overkillCore.localScale = new Vector3(0.1f + overkillStrength * 0.08f, 0.14f + overkillStrength * 0.2f, 0.1f + overkillStrength * 0.08f);
                overkillCore.localPosition = new Vector3(0f, 0.08f + overkillCore.localScale.y * 0.5f, 0f);
            }

            if (overkillCoreRenderer != null)
            {
                overkillCoreRenderer.material.color = Color.Lerp(overkillColor, Color.white, 0.18f + overkillStrength * 0.16f) * pulse;
            }
        }
    }
}
