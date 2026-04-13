using Game.BattleAces;
using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Represents a unit that can be selected and commanded by the prototype controller.
    /// </summary>
    [RequireComponent(typeof(SimpleUnitMover))]
    [RequireComponent(typeof(CombatTarget))]
    public class SelectableUnit : MonoBehaviour
    {
        private Renderer[] cachedRenderers;
        private SimpleUnitMover unitMover;
        private UnitCombat combat;
        private CombatTarget combatTarget;
        private UnitAbilityState abilityState;
        private GameObject selectionRing;
        private Transform statusAnchor;
        private Transform statusCore;
        private Renderer statusCoreRenderer;
        private Transform statusBanner;
        private Renderer statusBannerRenderer;
        private Transform abilityHalo;
        private Renderer abilityHaloRenderer;
        private Transform roleBadgePrimary;
        private Renderer roleBadgePrimaryRenderer;
        private Transform roleBadgeSecondary;
        private Renderer roleBadgeSecondaryRenderer;
        private Transform directivePointerRoot;
        private Transform directivePointerBeam;
        private Renderer directivePointerBeamRenderer;
        private Transform directivePointerTip;
        private Renderer directivePointerTipRenderer;
        private Transform[] animatedVisualParts = System.Array.Empty<Transform>();
        private Vector3[] animatedVisualBasePositions = System.Array.Empty<Vector3>();
        private Quaternion[] animatedVisualBaseRotations = System.Array.Empty<Quaternion>();
        private Vector3[] animatedVisualBaseScales = System.Array.Empty<Vector3>();
        private string[] animatedVisualNames = System.Array.Empty<string>();
        private float[] animatedVisualPhases = System.Array.Empty<float>();
        private int animatedVisualChildCount = -1;
        private UnitTeam team;
        private UnitDefinition definition;
        private Color defaultColor;
        private Color selectedColor;

        /// <summary>선택 링 직경·두께 — 바닥에서 윤곽이 더 잘 보이게</summary>
        private const float SelectionRingDiameter = 1.38f;

        private const float SelectionRingHeight = 0.046f;

        public UnitTeam Team => team;
        public UnitArchetype Archetype => definition != null ? definition.Archetype : UnitArchetype.Spearman;
        public string DisplayName => definition != null ? definition.DisplayName : "Unit";
        public UnitDefinition Definition => definition;
        public bool IsSelected { get; private set; }
        public string AbilityStatus => abilityState != null ? abilityState.StatusLabel : "None";
        public string OrderLabel => combat != null ? combat.OrderLabel : "Idle";

        private void Awake()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            unitMover = GetComponent<SimpleUnitMover>();
            combat = GetComponent<UnitCombat>();
            combatTarget = GetComponent<CombatTarget>();
            abilityState = GetComponent<UnitAbilityState>();
            CreateSelectionRing();
            CreateStatusIndicator();
            ApplyTeamColors();
            UpdateStatusIndicator();
        }

        private void OnEnable()
        {
            PrototypeRuntimeRegistry.Register(this);
        }

        private void OnDisable()
        {
            PrototypeRuntimeRegistry.Unregister(this);
        }

        private void LateUpdate()
        {
            RefreshAnimatedVisualsIfNeeded();
            UpdateUnitMotion();
            UpdateStatusIndicator();
        }

        public void Initialize(UnitTeam assignedTeam, UnitDefinition assignedDefinition, SimpleUnitMover mover, UnitCombat unitCombat)
        {
            team = assignedTeam;
            definition = assignedDefinition;
            unitMover = mover;
            combat = unitCombat;
            abilityState = GetComponent<UnitAbilityState>();
            ApplyTeamColors();
            UpdateStatusIndicator();
        }

        public void MoveTo(Vector3 destination)
        {
            combat?.ClearOrders();
            unitMover.SetDestination(destination);
        }

        public void AttackMoveTo(Vector3 destination)
        {
            combat?.SetAttackMoveDestination(destination);
        }

        public void Attack(CombatTarget target)
        {
            combat?.SetTarget(target);
        }

        public void HoldPosition()
        {
            combat?.SetHoldPosition(transform.position);
        }

        public void GuardPoint(Vector3 point, float radius)
        {
            combat?.SetGuardPoint(point, radius);
        }

        public bool TryActivateAbility()
        {
            return abilityState != null && abilityState.TryActivateRoleAbility();
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            if (selectionRing != null)
            {
                selectionRing.SetActive(isSelected);
            }

            ApplyColor(isSelected ? selectedColor : defaultColor);
            if (isSelected)
            {
                ApplySelectionRingHighlight();
            }
            else
            {
                ApplySelectionRingTeamIdleTint();
            }

            UpdateStatusIndicator();
        }

        private void ApplyColor(Color color)
        {
            foreach (Renderer cachedRenderer in cachedRenderers)
            {
                if (cachedRenderer != null)
                {
                    // 실루엣 이미터는 유지하고 알베도만 — 선택 시 몸통만 밝게
                    ReadablePrimitiveMaterialUtility.ApplyAlbedoOnly(cachedRenderer, color);
                }
            }
        }

        private void ApplyTeamColors()
        {
            team = combatTarget != null ? combatTarget.Team : team;

            if (definition == null)
            {
                defaultColor = team == UnitTeam.Player ? new Color(0.7f, 0.8f, 1f) : new Color(0.9f, 0.35f, 0.35f);
            }
            else
            {
                defaultColor = team == UnitTeam.Player ? definition.PlayerColor : definition.EnemyColor;
            }

            defaultColor = BattleAcesReadability.EnhanceFactionUnitTint(defaultColor, team);

            // 선택 링은 티얼/앰버 — 본체 틴트는 살짝 채도만 올린 티얼·앰버 계열
            selectedColor = team == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.2f, 0.95f, 0.55f, 1f), 0.35f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.72f, 0.28f, 1f), 0.28f);

            if (cachedRenderers != null && cachedRenderers.Length > 0)
            {
                ApplyColor(defaultColor);
            }

            ApplySelectionRingTeamIdleTint();
            UpdateStatusIndicator();
        }

        /// <summary>비선택 상태 링 틴트 — 다음에 선택될 때까지 유지</summary>
        private void ApplySelectionRingTeamIdleTint()
        {
            if (selectionRing == null)
            {
                return;
            }

            Renderer ringRenderer = selectionRing.GetComponent<Renderer>();
            if (ringRenderer == null)
            {
                return;
            }

            Color ringColor = team == UnitTeam.Player
                ? BattleAcesArtDirection.PointTeal
                : BattleAcesArtDirection.EnemyEmber;
            ReadablePrimitiveMaterialUtility.Apply(ringRenderer, ringColor, 0.82f);
        }

        /// <summary>선택됨 — 링만 강한 시안·금색 이미터로 분리</summary>
        private void ApplySelectionRingHighlight()
        {
            if (selectionRing == null)
            {
                return;
            }

            Renderer ringRenderer = selectionRing.GetComponent<Renderer>();
            if (ringRenderer == null)
            {
                return;
            }

            Color ringColor = team == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.12f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.1f);
            ReadablePrimitiveMaterialUtility.Apply(ringRenderer, ringColor, 1.15f);
        }

        private void CreateSelectionRing()
        {
            if (selectionRing != null)
            {
                return;
            }

            selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionRing.name = "Selection Ring";
            selectionRing.transform.SetParent(transform);
            selectionRing.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            selectionRing.transform.localRotation = Quaternion.identity;
            selectionRing.transform.localScale = new Vector3(SelectionRingDiameter, SelectionRingHeight, SelectionRingDiameter);
            selectionRing.GetComponent<Collider>().enabled = false;
            selectionRing.SetActive(false);
        }

        private void CreateStatusIndicator()
        {
            if (statusAnchor != null)
            {
                return;
            }

            statusAnchor = new GameObject("Status Anchor").transform;
            statusAnchor.SetParent(transform);
            statusAnchor.localRotation = Quaternion.identity;
            statusAnchor.localScale = Vector3.one;

            CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cylinder,
                "Status Mast",
                new Vector3(0f, -0.1f, 0f),
                new Vector3(0.03f, 0.1f, 0.03f),
                new Color(0.16f, 0.18f, 0.22f));

            statusCore = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Sphere,
                "Status Core",
                new Vector3(0f, 0.04f, 0f),
                new Vector3(0.16f, 0.16f, 0.16f),
                new Color(0.7f, 0.72f, 0.76f));
            statusCoreRenderer = statusCore.GetComponent<Renderer>();

            statusBanner = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Status Banner",
                new Vector3(0f, 0.04f, 0.18f),
                new Vector3(0.08f, 0.04f, 0.28f),
                new Color(0.7f, 0.72f, 0.76f));
            statusBannerRenderer = statusBanner.GetComponent<Renderer>();

            abilityHalo = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cylinder,
                "Ability Halo",
                new Vector3(0f, 0.24f, 0f),
                new Vector3(0.12f, 0.025f, 0.12f),
                Color.Lerp(BattleAcesArtDirection.EnemyEmber, BattleAcesArtDirection.PointTeal, 0.15f));
            abilityHaloRenderer = abilityHalo.GetComponent<Renderer>();

            roleBadgePrimary = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Role Badge Primary",
                new Vector3(0f, -0.12f, -0.1f),
                new Vector3(0.12f, 0.04f, 0.12f),
                new Color(0.82f, 0.86f, 0.92f));
            roleBadgePrimaryRenderer = roleBadgePrimary.GetComponent<Renderer>();

            roleBadgeSecondary = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Role Badge Secondary",
                new Vector3(0f, -0.12f, -0.24f),
                new Vector3(0.08f, 0.04f, 0.08f),
                new Color(0.82f, 0.86f, 0.92f));
            roleBadgeSecondaryRenderer = roleBadgeSecondary.GetComponent<Renderer>();

            directivePointerRoot = new GameObject("Directive Pointer Root").transform;
            directivePointerRoot.SetParent(statusAnchor);
            directivePointerRoot.localPosition = new Vector3(0f, 0.26f, 0f);
            directivePointerRoot.localRotation = Quaternion.identity;
            directivePointerRoot.localScale = Vector3.one;

            directivePointerBeam = CreateStatusPrimitive(
                directivePointerRoot,
                PrimitiveType.Cube,
                "Directive Pointer Beam",
                new Vector3(0f, 0f, 0.32f),
                new Vector3(0.03f, 0.03f, 0.64f),
                BattleAcesArtDirection.PointTeal);
            directivePointerBeamRenderer = directivePointerBeam.GetComponent<Renderer>();

            directivePointerTip = CreateStatusPrimitive(
                directivePointerRoot,
                PrimitiveType.Sphere,
                "Directive Pointer Tip",
                new Vector3(0f, 0f, 0.66f),
                new Vector3(0.1f, 0.1f, 0.1f),
                BattleAcesArtDirection.PointTeal);
            directivePointerTipRenderer = directivePointerTip.GetComponent<Renderer>();
        }

        private void UpdateStatusIndicator()
        {
            if (statusAnchor == null)
            {
                return;
            }

            float anchorHeight = Mathf.Clamp(transform.localScale.y * 1.7f, 1.45f, 3.25f);
            if (definition != null && definition.IsFlying)
            {
                anchorHeight += 0.4f;
            }

            statusAnchor.localPosition = new Vector3(0f, anchorHeight, 0f);

            string orderLabel = OrderLabel;
            bool directional = false;
            bool lateral = false;
            Color orderColor = Team == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.08f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.06f);
            float bannerWidth = 0.08f;
            float bannerDepth = 0.18f;
            float coreScale = IsSelected ? 0.18f : 0.15f;
            float pulse = 0.88f + Mathf.PingPong(Time.time * 2.2f, 0.16f);

            switch (orderLabel)
            {
                case "Engage":
                    directional = true;
                    orderColor = BattleAcesArtDirection.EnemyEmber;
                    bannerDepth = 0.36f;
                    coreScale = 0.18f;
                    pulse = 0.9f + Mathf.PingPong(Time.time * 5.2f, 0.22f);
                    break;
                case "Advance":
                    directional = true;
                    orderColor = Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.5f, 0.95f, 1f, 1f), 0.25f);
                    bannerDepth = 0.34f;
                    coreScale = 0.17f;
                    pulse = 0.88f + Mathf.PingPong(Time.time * 3.8f, 0.18f);
                    break;
                case "Move":
                    directional = true;
                    orderColor = Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.35f, 0.92f, 0.55f, 1f), 0.4f);
                    bannerDepth = 0.28f;
                    pulse = 0.88f + Mathf.PingPong(Time.time * 3f, 0.14f);
                    break;
                case "Guard":
                    lateral = true;
                    orderColor = Color.Lerp(BattleAcesArtDirection.GunmetalLift, BattleAcesArtDirection.PointTeal, 0.45f);
                    bannerWidth = 0.34f;
                    bannerDepth = 0.08f;
                    coreScale = 0.17f;
                    break;
                case "Hold":
                    orderColor = new Color(0.52f, 0.76f, 1f);
                    bannerWidth = 0.18f;
                    bannerDepth = 0.18f;
                    coreScale = 0.18f;
                    break;
                default:
                    orderColor = IsSelected
                        ? (Team == UnitTeam.Player
                            ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.22f)
                            : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.18f))
                        : new Color(0.48f, 0.5f, 0.54f);
                    bannerWidth = 0.08f;
                    bannerDepth = 0.12f;
                    coreScale = IsSelected ? 0.16f : 0.12f;
                    pulse = IsSelected ? 0.9f + Mathf.PingPong(Time.time * 2.5f, 0.15f) : 1f;
                    break;
            }

            if (statusCore != null)
            {
                statusCore.localScale = Vector3.one * coreScale;
            }

            if (statusCoreRenderer != null)
            {
                statusCoreRenderer.material.color = orderColor * pulse;
            }

            if (statusBanner != null)
            {
                if (directional)
                {
                    statusBanner.localPosition = new Vector3(0f, 0.04f, 0.08f + bannerDepth * 0.5f);
                    statusBanner.localScale = new Vector3(bannerWidth, 0.04f, bannerDepth);
                }
                else if (lateral)
                {
                    statusBanner.localPosition = new Vector3(0f, 0.04f, 0f);
                    statusBanner.localScale = new Vector3(bannerWidth, 0.04f, bannerDepth);
                }
                else
                {
                    statusBanner.localPosition = new Vector3(0f, 0.04f, 0f);
                    statusBanner.localScale = new Vector3(bannerWidth, 0.04f, bannerDepth);
                }
            }

            if (statusBannerRenderer != null)
            {
                statusBannerRenderer.material.color = Color.Lerp(orderColor, Color.white, IsSelected ? 0.18f : 0.06f) * pulse;
            }

            UpdateRoleBadge(orderColor, pulse);
            UpdateAbilityHalo(orderColor);
            UpdateDirectivePointer();
        }

        private void RefreshAnimatedVisualsIfNeeded()
        {
            if (transform.childCount == animatedVisualChildCount && animatedVisualParts.Length > 0)
            {
                return;
            }

            animatedVisualChildCount = transform.childCount;

            int visualPartCount = 0;
            foreach (Transform child in transform)
            {
                if (ShouldAnimateVisualPart(child))
                {
                    visualPartCount++;
                }
            }

            animatedVisualParts = new Transform[visualPartCount];
            animatedVisualBasePositions = new Vector3[visualPartCount];
            animatedVisualBaseRotations = new Quaternion[visualPartCount];
            animatedVisualBaseScales = new Vector3[visualPartCount];
            animatedVisualNames = new string[visualPartCount];
            animatedVisualPhases = new float[visualPartCount];

            int index = 0;
            foreach (Transform child in transform)
            {
                if (!ShouldAnimateVisualPart(child))
                {
                    continue;
                }

                animatedVisualParts[index] = child;
                animatedVisualBasePositions[index] = child.localPosition;
                animatedVisualBaseRotations[index] = child.localRotation;
                animatedVisualBaseScales[index] = child.localScale;
                animatedVisualNames[index] = child.name;
                animatedVisualPhases[index] = Random.value * Mathf.PI * 2f;
                index++;
            }
        }

        private void UpdateUnitMotion()
        {
            if (animatedVisualParts.Length == 0)
            {
                return;
            }

            bool isMoving = unitMover != null && unitMover.IsMoving;
            bool isEngaging = combat != null && combat.CurrentTarget != null;
            bool isFlying = definition != null && definition.IsFlying;
            float attackPulse = combat != null ? combat.RecentAttackPulse : 0f;
            float teamBias = team == UnitTeam.Player ? 0.82f : 1.14f;
            float moveSpeedFactor = isMoving ? 8.6f : 2.2f;
            float bodyLift = isFlying ? 0.08f : 0.02f;
            float hoverWave = Mathf.Sin(Time.time * (isFlying ? 3.8f : 1.4f)) * bodyLift;

            for (int index = 0; index < animatedVisualParts.Length; index++)
            {
                Transform part = animatedVisualParts[index];
                if (part == null)
                {
                    continue;
                }

                string partName = animatedVisualNames[index];
                Vector3 basePosition = animatedVisualBasePositions[index];
                Quaternion baseRotation = animatedVisualBaseRotations[index];
                Vector3 baseScale = animatedVisualBaseScales[index];
                float phase = animatedVisualPhases[index];
                float wave = Mathf.Sin(Time.time * moveSpeedFactor + phase);
                float sway = Mathf.Cos(Time.time * (moveSpeedFactor * 0.66f) + phase);
                float motion = isMoving ? 1f : 0.22f;
                float pulse = 1f + attackPulse * 0.08f;
                Vector3 animatedPosition = basePosition;
                Quaternion animatedRotation = baseRotation;
                Vector3 animatedScale = baseScale;

                animatedPosition.y += hoverWave + wave * 0.015f * motion * teamBias;

                if (partName.Contains("Head") || partName.Contains("Visor") || partName.Contains("Crest"))
                {
                    animatedPosition.y += wave * 0.025f * motion;
                    animatedRotation *= Quaternion.Euler(wave * 3.5f * motion, sway * 3.2f * motion, -wave * 2.2f * motion);
                }
                else if (partName.Contains("Arm"))
                {
                    float armSwing = isMoving ? wave * 18f : wave * 4f;
                    animatedRotation *= Quaternion.Euler(armSwing, 0f, 0f);
                }
                else if (partName.Contains("Leg"))
                {
                    float legSwing = isMoving ? -wave * 16f : wave * 2f;
                    animatedRotation *= Quaternion.Euler(legSwing, 0f, 0f);
                }
                else if (partName.Contains("Rifle") || partName.Contains("Pike") || partName.Contains("Blade") || partName.Contains("Cannon") || partName.Contains("Barrel"))
                {
                    float weaponKick = attackPulse * (team == UnitTeam.Player ? 9f : 12f);
                    animatedPosition.z -= attackPulse * 0.08f;
                    animatedRotation *= Quaternion.Euler(-weaponKick - wave * 3f * motion, sway * 3f, 0f);
                    animatedScale *= pulse;
                }
                else if (partName.Contains("Shield"))
                {
                    animatedPosition.z += isEngaging ? 0.04f : 0f;
                    animatedRotation *= Quaternion.Euler(isEngaging ? -8f : 0f, 0f, wave * 2f * motion);
                }
                else if (partName.Contains("Wing"))
                {
                    float wingTilt = isFlying ? wave * 10f : wave * 3f;
                    animatedPosition.y += isFlying ? Mathf.Abs(wave) * 0.03f : 0f;
                    animatedRotation *= Quaternion.Euler(wingTilt, 0f, sway * 6f);
                }
                else if (partName.Contains("Thruster") || partName.Contains("Engine"))
                {
                    animatedPosition.y += Mathf.Abs(wave) * 0.028f;
                    animatedPosition.z -= attackPulse * 0.04f;
                    animatedRotation *= Quaternion.Euler(wave * 6f, sway * 6f, 0f);
                    animatedScale *= 1f + Mathf.Abs(wave) * 0.08f + attackPulse * 0.06f;
                }
                else if (partName.Contains("Halo") || partName.Contains("Core"))
                {
                    animatedPosition.y += Mathf.Abs(wave) * 0.024f;
                    animatedScale *= 1f + Mathf.Abs(wave) * 0.06f + attackPulse * 0.08f;
                }
                else if (partName.Contains("Bike") || partName.Contains("Lance") || partName.Contains("Banner"))
                {
                    animatedPosition.y += wave * 0.02f * motion;
                    animatedRotation *= Quaternion.Euler(wave * 3f, 0f, sway * 4f);
                }
                else if (partName.Contains("Hull") || partName.Contains("Deck") || partName.Contains("Citadel") || partName.Contains("Flight"))
                {
                    animatedPosition.y += isFlying ? Mathf.Abs(wave) * 0.04f : 0f;
                    animatedRotation *= Quaternion.Euler(isMoving ? wave * 2f : 0f, 0f, isFlying ? sway * 4f : 0f);
                }
                else
                {
                    animatedRotation *= Quaternion.Euler(wave * 1.8f * motion, 0f, sway * 1.4f * motion);
                }

                if (isEngaging && !partName.Contains("Wing") && !partName.Contains("Leg"))
                {
                    animatedPosition.z -= 0.02f * teamBias;
                    animatedRotation *= Quaternion.Euler(-3.5f * teamBias, 0f, 0f);
                }

                part.localPosition = animatedPosition;
                part.localRotation = animatedRotation;
                part.localScale = animatedScale;
            }

            if (selectionRing != null && selectionRing.activeSelf)
            {
                float attackBoost = combat != null ? combat.RecentAttackPulse * 0.22f : 0f;
                float ringPulse = 1f + Mathf.PingPong(Time.time * 2.8f, 0.12f) + attackBoost;
                selectionRing.transform.localScale = new Vector3(
                    SelectionRingDiameter * ringPulse,
                    SelectionRingHeight,
                    SelectionRingDiameter * ringPulse);
            }
            else if (selectionRing != null)
            {
                selectionRing.transform.localScale = new Vector3(SelectionRingDiameter, SelectionRingHeight, SelectionRingDiameter);
            }
        }

        private void UpdateRoleBadge(Color orderColor, float pulse)
        {
            if (roleBadgePrimary == null || roleBadgeSecondary == null)
            {
                return;
            }

            UnitArchetype archetype = Archetype;
            bool showSecondary = true;
            Vector3 primaryPosition = new Vector3(0f, -0.12f, -0.1f);
            Vector3 secondaryPosition = new Vector3(0f, -0.12f, -0.24f);
            Vector3 primaryScale = new Vector3(0.12f, 0.04f, 0.12f);
            Vector3 secondaryScale = new Vector3(0.08f, 0.04f, 0.08f);
            Color badgeColor = GetRoleBadgeColor(archetype, orderColor);

            switch (archetype)
            {
                case UnitArchetype.Spearman:
                    primaryScale = new Vector3(0.04f, 0.12f, 0.04f);
                    secondaryScale = new Vector3(0.1f, 0.03f, 0.1f);
                    secondaryPosition = new Vector3(0f, -0.2f, -0.1f);
                    break;
                case UnitArchetype.ShieldInfantry:
                    primaryScale = new Vector3(0.18f, 0.06f, 0.08f);
                    secondaryScale = new Vector3(0.12f, 0.06f, 0.06f);
                    secondaryPosition = new Vector3(0f, -0.04f, -0.22f);
                    break;
                case UnitArchetype.Rifleman:
                    primaryScale = new Vector3(0.04f, 0.04f, 0.24f);
                    secondaryScale = new Vector3(0.08f, 0.04f, 0.08f);
                    secondaryPosition = new Vector3(0f, -0.12f, -0.32f);
                    break;
                case UnitArchetype.Fighter:
                    primaryScale = new Vector3(0.24f, 0.03f, 0.08f);
                    secondaryScale = new Vector3(0.08f, 0.03f, 0.22f);
                    break;
                case UnitArchetype.SpecialWarrior:
                    primaryScale = new Vector3(0.04f, 0.12f, 0.04f);
                    secondaryScale = new Vector3(0.04f, 0.12f, 0.04f);
                    primaryPosition = new Vector3(-0.05f, -0.12f, -0.12f);
                    secondaryPosition = new Vector3(0.05f, -0.12f, -0.12f);
                    break;
                case UnitArchetype.RoyalGuard:
                    primaryScale = new Vector3(0.12f, 0.12f, 0.08f);
                    secondaryScale = new Vector3(0.18f, 0.03f, 0.18f);
                    secondaryPosition = new Vector3(0f, -0.2f, -0.1f);
                    break;
                case UnitArchetype.Outrider:
                    primaryScale = new Vector3(0.24f, 0.04f, 0.1f);
                    secondaryScale = new Vector3(0.08f, 0.08f, 0.08f);
                    secondaryPosition = new Vector3(0f, -0.08f, -0.24f);
                    break;
                case UnitArchetype.Artillery:
                    primaryScale = new Vector3(0.2f, 0.05f, 0.12f);
                    secondaryScale = new Vector3(0.06f, 0.06f, 0.06f);
                    secondaryPosition = new Vector3(0f, -0.04f, -0.24f);
                    break;
                case UnitArchetype.MobileFortress:
                    primaryScale = new Vector3(0.22f, 0.08f, 0.16f);
                    secondaryScale = new Vector3(0.18f, 0.04f, 0.1f);
                    secondaryPosition = new Vector3(0f, -0.22f, -0.1f);
                    break;
                case UnitArchetype.AirborneCitadel:
                    primaryScale = new Vector3(0.28f, 0.04f, 0.12f);
                    secondaryScale = new Vector3(0.12f, 0.08f, 0.12f);
                    secondaryPosition = new Vector3(0f, -0.02f, -0.24f);
                    break;
                default:
                    showSecondary = false;
                    break;
            }

            roleBadgePrimary.localPosition = primaryPosition;
            roleBadgePrimary.localScale = primaryScale;
            roleBadgePrimary.gameObject.SetActive(true);

            roleBadgeSecondary.localPosition = secondaryPosition;
            roleBadgeSecondary.localScale = secondaryScale;
            roleBadgeSecondary.gameObject.SetActive(showSecondary);

            if (roleBadgePrimaryRenderer != null)
            {
                roleBadgePrimaryRenderer.material.color = badgeColor * pulse;
            }

            if (showSecondary && roleBadgeSecondaryRenderer != null)
            {
                roleBadgeSecondaryRenderer.material.color = Color.Lerp(badgeColor, Color.white, 0.12f) * pulse;
            }
        }

        private Color GetRoleBadgeColor(UnitArchetype archetype, Color fallbackColor)
        {
            return archetype switch
            {
                UnitArchetype.Spearman => Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.3f, 0.95f, 0.55f, 1f), 0.35f),
                UnitArchetype.ShieldInfantry => Color.Lerp(BattleAcesArtDirection.PointTeal, BattleAcesArtDirection.AmbientSky, 0.28f),
                UnitArchetype.Rifleman => Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.75f, 0.38f, 1f), 0.32f),
                UnitArchetype.SpecialWarrior => Color.Lerp(BattleAcesArtDirection.EnemyEmber, BattleAcesArtDirection.PointTeal, 0.22f),
                UnitArchetype.RoyalGuard => Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.82f, 0.5f, 1f), 0.28f),
                UnitArchetype.Outrider => Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.55f, 0.95f, 1f, 1f), 0.34f),
                UnitArchetype.Artillery => Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.5f, 0.3f, 1f), 0.2f),
                UnitArchetype.Fighter => Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.55f, 0.88f, 1f, 1f), 0.3f),
                UnitArchetype.MobileFortress => Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.75f, 0.7f, 0.92f, 1f), 0.25f),
                UnitArchetype.AirborneCitadel => Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.2f),
                _ => Color.Lerp(fallbackColor, Color.white, 0.24f)
            };
        }

        private void UpdateAbilityHalo(Color orderColor)
        {
            if (abilityHalo == null)
            {
                return;
            }

            bool abilityActive = abilityState != null && abilityState.IsActive;
            bool abilityReady = abilityState == null || abilityState.IsReady;
            bool showHalo = abilityActive || IsSelected || !abilityReady;
            abilityHalo.gameObject.SetActive(showHalo);

            if (!showHalo)
            {
                return;
            }

            float haloPulse = abilityActive
                ? 0.92f + Mathf.PingPong(Time.time * 4.6f, 0.22f)
                : IsSelected
                    ? 0.9f + Mathf.PingPong(Time.time * 2.2f, 0.14f)
                    : 0.84f + Mathf.PingPong(Time.time * 1.4f, 0.08f);
            Color haloColor = abilityActive
                ? Color.Lerp(BattleAcesArtDirection.EnemyEmber, BattleAcesArtDirection.PointTeal, 0.2f)
                : abilityReady
                    ? Color.Lerp(orderColor, Color.white, 0.2f)
                    : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.12f);

            abilityHalo.localScale = new Vector3(0.12f * haloPulse, 0.025f, 0.12f * haloPulse);

            if (abilityHaloRenderer != null)
            {
                abilityHaloRenderer.material.color = haloColor * haloPulse;
            }
        }

        private void UpdateDirectivePointer()
        {
            if (directivePointerRoot == null)
            {
                return;
            }

            bool hasObjective = TryGetStrategicObjective(out Vector3 objectivePosition, out Color objectiveColor, out bool baseAssault);
            bool showPointer = hasObjective && IsSelected;
            directivePointerRoot.gameObject.SetActive(showPointer);

            if (!showPointer)
            {
                return;
            }

            Vector3 pointerTarget = new Vector3(objectivePosition.x, transform.position.y + 0.12f, objectivePosition.z);
            Vector3 localTarget = transform.InverseTransformPoint(pointerTarget);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.3f, 0.3f), localTarget.z);
            if (planarTarget.sqrMagnitude <= 0.0001f)
            {
                planarTarget = Vector3.forward * 0.22f;
            }

            float distance = Mathf.Max(0.22f, planarTarget.magnitude);
            Vector3 direction = planarTarget.normalized;
            float pulse = baseAssault
                ? 0.92f + Mathf.PingPong(Time.time * 4.2f, 0.22f)
                : 0.86f + Mathf.PingPong(Time.time * 2.8f, 0.14f);
            float beamThickness = baseAssault ? 0.04f : 0.03f;
            float tipScale = baseAssault ? 0.12f : 0.1f;

            directivePointerRoot.localPosition = new Vector3(0f, 0.26f + Mathf.PingPong(Time.time * 1.4f, 0.05f), 0f);
            directivePointerRoot.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (directivePointerBeam != null)
            {
                directivePointerBeam.localPosition = new Vector3(0f, 0f, distance * 0.5f);
                directivePointerBeam.localScale = new Vector3(beamThickness, beamThickness, distance);
            }

            if (directivePointerBeamRenderer != null)
            {
                directivePointerBeamRenderer.material.color = objectiveColor * pulse;
            }

            if (directivePointerTip != null)
            {
                directivePointerTip.localPosition = new Vector3(0f, 0f, distance);
                directivePointerTip.localScale = Vector3.one * (tipScale + Mathf.PingPong(Time.time * 2f, 0.025f));
            }

            if (directivePointerTipRenderer != null)
            {
                directivePointerTipRenderer.material.color = Color.Lerp(objectiveColor, Color.white, baseAssault ? 0.26f : 0.14f) * pulse;
            }
        }

        private bool TryGetStrategicObjective(out Vector3 objectivePosition, out Color objectiveColor, out bool baseAssault)
        {
            objectivePosition = transform.position;
            objectiveColor = team == UnitTeam.Player
                ? BattleAcesArtDirection.PointTeal
                : BattleAcesArtDirection.EnemyEmber;
            baseAssault = false;

            BattleDirectiveController directiveController = BattleDirectiveController.Instance;
            if (directiveController == null)
            {
                return false;
            }

            if (directiveController.CanTargetEnemyBase(team))
            {
                UnitTeam enemyTeam = team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player;
                BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(enemyTeam);
                if (enemyBase == null)
                {
                    return false;
                }

                objectivePosition = enemyBase.transform.position;
                objectiveColor = team == UnitTeam.Player
                    ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.08f)
                    : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.06f);
                baseAssault = true;
                return true;
            }

            ControlNode priorityNode = directiveController.FindPriorityNodeFor(team);
            if (priorityNode == null)
            {
                return false;
            }

            objectivePosition = priorityNode.transform.position;
            objectiveColor = priorityNode.Tier switch
            {
                ControlNodeTier.Grand => Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.95f, 0.78f, 0.38f, 1f), 0.35f),
                ControlNodeTier.Major => Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.85f, 0.88f, 0.5f, 1f), 0.28f),
                _ => objectiveColor
            };
            return true;
        }

        private static Transform CreateStatusPrimitive(Transform parent, PrimitiveType primitiveType, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
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

        private bool ShouldAnimateVisualPart(Transform child)
        {
            if (child == null || child == statusAnchor || child == selectionRing?.transform || child == directivePointerRoot)
            {
                return false;
            }

            string childName = child.name;
            if (childName == "Status Anchor"
                || childName == "Selection Ring"
                || childName == "Engagement Anchor"
                || childName == "Order Anchor Visual")
            {
                return false;
            }

            return true;
        }
    }
}


