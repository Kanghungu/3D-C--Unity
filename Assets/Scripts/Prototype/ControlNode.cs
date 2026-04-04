using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Strategic battlefield objective that can change production tempo, local sustain,
    /// and overall frontline pressure depending on its tier.
    /// </summary>
    public class ControlNode : MonoBehaviour
    {
        private static float nextProductionRefreshTime;

        [SerializeField] private float controlRadius = 18f;
        [SerializeField] private float captureDuration = 4.6f;
        [SerializeField] private float captureCheckInterval = 0.2f;
        [SerializeField] private float baseCaptureStep = 0.28f;
        [SerializeField] private float neutralCaptureSpeedMultiplier = 1.3f;
        [SerializeField] private float productionBonusMultiplier = 1.12f;
        [SerializeField] private float healingRadius = 18f;
        [SerializeField] private float healingPerSecond = 4.5f;
        [SerializeField] private string nodeLabel = "Holy Node";
        [SerializeField] private ControlNodeTier nodeTier = ControlNodeTier.Minor;

        private Renderer cachedRenderer;
        private UnitTeam? ownerTeam;
        private UnitTeam? capturingTeam;
        private float captureProgress;
        private float captureCheckTimer;
        private float healingTickTimer;
        private Transform statusAnchor;
        private Transform captureBarFill;
        private Renderer captureBarFillRenderer;
        private Renderer captureBarFrameRenderer;
        private Transform ownerBeacon;
        private Renderer ownerBeaconRenderer;
        private Transform contestBeacon;
        private Renderer contestBeaconRenderer;
        private Transform playerStrengthColumn;
        private Renderer playerStrengthColumnRenderer;
        private Transform enemyStrengthColumn;
        private Renderer enemyStrengthColumnRenderer;
        private Transform pressureBalanceBar;
        private Renderer pressureBalanceBarRenderer;
        private Transform pressureBalanceNeedle;
        private Renderer pressureBalanceNeedleRenderer;
        private Transform flipWarningRoot;
        private Transform flipWarningRing;
        private Renderer flipWarningRingRenderer;
        private Transform flipWarningSpire;
        private Renderer flipWarningSpireRenderer;
        private Transform stabilityRoot;
        private Transform stabilityRing;
        private Renderer stabilityRingRenderer;
        private Transform stabilityCore;
        private Renderer stabilityCoreRenderer;
        private Transform garrisonWarningRoot;
        private Transform garrisonWarningRing;
        private Renderer garrisonWarningRingRenderer;
        private Transform garrisonWarningCore;
        private Renderer garrisonWarningCoreRenderer;
        private Transform playerPressureRoot;
        private Transform playerPressureBeam;
        private Renderer playerPressureBeamRenderer;
        private Transform playerPressureTip;
        private Renderer playerPressureTipRenderer;
        private Transform enemyPressureRoot;
        private Transform enemyPressureBeam;
        private Renderer enemyPressureBeamRenderer;
        private Transform enemyPressureTip;
        private Renderer enemyPressureTipRenderer;
        private readonly Transform[] tierPips = new Transform[3];
        private readonly Renderer[] tierPipRenderers = new Renderer[3];
        private Transform zoneAnchor;
        private readonly Transform[] controlPosts = new Transform[8];
        private readonly Renderer[] controlPostRenderers = new Renderer[8];
        private readonly Transform[] healingBeacons = new Transform[4];
        private readonly Renderer[] healingBeaconRenderers = new Renderer[4];
        private Transform objectiveAnchor;
        private Transform playerObjectiveMarker;
        private Renderer playerObjectiveRenderer;
        private Transform enemyObjectiveMarker;
        private Renderer enemyObjectiveRenderer;
        private UnitTeam? previousOwnerTeam;
        private int lastPlayerCount;
        private int lastEnemyCount;

        public string OwnerLabel => ownerTeam.HasValue ? ownerTeam.Value.ToString() : "Neutral";
        public float BonusMultiplier => productionBonusMultiplier;
        public float HealingPerSecond => healingPerSecond;
        public string NodeLabel => string.IsNullOrWhiteSpace(nodeLabel) ? name : nodeLabel;
        public UnitTeam? OwnerTeam => ownerTeam;
        public UnitTeam? CapturingTeam => capturingTeam;
        public float ControlRadius => controlRadius;
        public ControlNodeTier Tier => nodeTier;
        public bool IsGrand => nodeTier == ControlNodeTier.Grand;
        public bool IsMajor => nodeTier == ControlNodeTier.Major;
        public bool IsBeingCaptured => capturingTeam.HasValue && ownerTeam != capturingTeam;
        public float CaptureProgressNormalized => Mathf.Clamp01(captureProgress / Mathf.Max(0.01f, GetCaptureThreshold()));
        public int StrategicWeight => nodeTier switch
        {
            ControlNodeTier.Grand => 4,
            ControlNodeTier.Major => 2,
            _ => 1
        };
        public int DesiredGarrison => nodeTier switch
        {
            ControlNodeTier.Grand => 26,
            ControlNodeTier.Major => 16,
            _ => 10
        };
        public int DesiredStrikeForce => nodeTier switch
        {
            ControlNodeTier.Grand => 36,
            ControlNodeTier.Major => 24,
            _ => 14
        };
        public string TierLabel => nodeTier switch
        {
            ControlNodeTier.Grand => "Grand",
            ControlNodeTier.Major => "Major",
            _ => "Minor"
        };
        public string TierShortLabel => nodeTier switch
        {
            ControlNodeTier.Grand => "대",
            ControlNodeTier.Major => "중",
            _ => "소"
        };
        public string CaptureSummaryLabel
        {
            get
            {
                if (IsBeingCaptured)
                {
                    string capturingLabel = capturingTeam == UnitTeam.Player ? "아군 점령" : "적 점령";
                    return $"{capturingLabel} {Mathf.RoundToInt(CaptureProgressNormalized * 100f)}%";
                }

                if (ownerTeam == UnitTeam.Player)
                {
                    return "아군 확보";
                }

                if (ownerTeam == UnitTeam.Enemy)
                {
                    return "적 확보";
                }

                return "중립";
            }
        }
        public int PlayerPresenceCount => lastPlayerCount;
        public int EnemyPresenceCount => lastEnemyCount;
        public bool IsPlayerRecaptureEmergency => ownerTeam == UnitTeam.Player
            && capturingTeam == UnitTeam.Enemy
            && CaptureProgressNormalized >= 0.55f;
        public string RecaptureAlertLabel
        {
            get
            {
                if (IsPlayerRecaptureEmergency)
                {
                    return $"탈환 경고 적 {EnemyPresenceCount} / 아군 {PlayerPresenceCount}";
                }

                if (ownerTeam == UnitTeam.Player)
                {
                    return "아군 유지";
                }

                if (capturingTeam == UnitTeam.Player)
                {
                    return $"아군 탈환 {Mathf.RoundToInt(CaptureProgressNormalized * 100f)}%";
                }

                return CaptureSummaryLabel;
            }
        }

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();

            if (string.IsNullOrWhiteSpace(nodeLabel))
            {
                nodeLabel = name;
            }

            ApplyVisuals();
            EnsureStatusVisuals();
            EnsureZoneVisuals();
            EnsureObjectiveVisuals();
            UpdateStatusVisuals();
            UpdateZoneVisuals();
            UpdateObjectiveVisuals();
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
            captureCheckTimer -= Time.deltaTime;
            healingTickTimer -= Time.deltaTime;
            UpdateStatusVisuals();
            UpdateZoneVisuals();
            UpdateObjectiveVisuals();

            if (captureCheckTimer <= 0f)
            {
                captureCheckTimer = captureCheckInterval;
                UpdateCaptureState();
            }

            if (healingTickTimer <= 0f)
            {
                healingTickTimer = 0.25f;
                ApplyHealingAura(0.25f);
            }

            if (Time.time >= nextProductionRefreshTime)
            {
                nextProductionRefreshTime = Time.time + 0.4f;
                ApplyProductionBonus();
            }
        }

        public void ConfigureNode(string label, float newProductionBonusMultiplier, float newHealingPerSecond, ControlNodeTier tier)
        {
            nodeLabel = label;
            productionBonusMultiplier = newProductionBonusMultiplier;
            healingPerSecond = newHealingPerSecond;
            nodeTier = tier;
            ApplyVisuals();
            UpdateStatusVisuals();
            UpdateZoneVisuals();
            UpdateObjectiveVisuals();
        }

        private void UpdateCaptureState()
        {
            int playerCount = 0;
            int enemyCount = 0;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, unit.transform.position) > controlRadius)
                {
                    continue;
                }

                if (unit.Team == UnitTeam.Player)
                {
                    playerCount++;
                }
                else if (unit.Team == UnitTeam.Enemy)
                {
                    enemyCount++;
                }
            }

            lastPlayerCount = playerCount;
            lastEnemyCount = enemyCount;

            UnitTeam? nextCapturingTeam = null;

            if (playerCount > 0 && enemyCount == 0)
            {
                nextCapturingTeam = UnitTeam.Player;
            }
            else if (enemyCount > 0 && playerCount == 0)
            {
                nextCapturingTeam = UnitTeam.Enemy;
            }

            if (!nextCapturingTeam.HasValue)
            {
                captureProgress = Mathf.Max(0f, captureProgress - baseCaptureStep * 0.65f);
                if (captureProgress <= 0.01f)
                {
                    capturingTeam = null;
                }
                return;
            }

            if (capturingTeam != nextCapturingTeam)
            {
                capturingTeam = nextCapturingTeam;
                captureProgress = 0f;
            }

            if (ownerTeam == capturingTeam)
            {
                captureProgress = GetCaptureThreshold();
                return;
            }

            int pressureCount = Mathf.Max(playerCount, enemyCount);
            float pressureBonus = Mathf.Min(0.16f, Mathf.Max(0, pressureCount - 1) * 0.04f);
            float captureStep = baseCaptureStep + pressureBonus;
            if (!ownerTeam.HasValue)
            {
                captureStep *= neutralCaptureSpeedMultiplier;
            }

            captureProgress += captureStep;

            if (captureProgress >= GetCaptureThreshold())
            {
                previousOwnerTeam = ownerTeam;
                ownerTeam = capturingTeam;
                captureProgress = GetCaptureThreshold();
                ApplyVisuals();
                ApplyProductionBonus();
                nextProductionRefreshTime = Time.time + 0.4f;
                BroadcastOwnershipChange();
            }
        }

        private void BroadcastOwnershipChange()
        {
            if (ownerTeam == UnitTeam.Player)
            {
                string recoveredLabel = previousOwnerTeam == UnitTeam.Enemy ? "재탈환" : "점령";
                BattleDirectiveController.BroadcastNewsStatic($"{NodeLabel} {recoveredLabel}. 아군 전선이 복구됩니다.");
                return;
            }

            if (ownerTeam == UnitTeam.Enemy)
            {
                string lostLabel = previousOwnerTeam == UnitTeam.Player ? "함락" : "적 점령";
                BattleDirectiveController.BroadcastNewsStatic($"{NodeLabel} {lostLabel}. 전방 탈환 대응이 필요합니다.");
            }
        }

        private float GetCaptureThreshold()
        {
            return captureDuration * (nodeTier switch
            {
                ControlNodeTier.Grand => 1.25f,
                ControlNodeTier.Major => 1.08f,
                _ => 1f
            });
        }

        private void ApplyProductionBonus()
        {
            float playerMultiplier = CalculateTeamProductionMultiplier(UnitTeam.Player);
            float enemyMultiplier = CalculateTeamProductionMultiplier(UnitTeam.Enemy);

            foreach (ProductionStructure structure in PrototypeRuntimeRegistry.GetProductionStructures())
            {
                if (structure == null)
                {
                    continue;
                }

                if (structure.Team == UnitTeam.Player)
                {
                    structure.SetProductionSpeedMultiplier(playerMultiplier);
                }
                else if (structure.Team == UnitTeam.Enemy)
                {
                    structure.SetProductionSpeedMultiplier(enemyMultiplier);
                }
            }

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);

            if (playerBase != null)
            {
                playerBase.SetProductionSpeedMultiplier(playerMultiplier);
            }

            if (enemyBase != null)
            {
                enemyBase.SetProductionSpeedMultiplier(enemyMultiplier);
            }
        }

        private float CalculateTeamProductionMultiplier(UnitTeam team)
        {
            float multiplier = 1f;

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node != null && node.OwnerTeam == team)
                {
                    multiplier *= node.BonusMultiplier;
                }
            }

            return multiplier;
        }

        private void ApplyHealingAura(float tickDuration)
        {
            if (!ownerTeam.HasValue)
            {
                return;
            }

            float healAmount = healingPerSecond * tickDuration;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || unit.Team != ownerTeam.Value)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, unit.transform.position) > healingRadius)
                {
                    continue;
                }

                UnitHealth health = unit.GetComponent<UnitHealth>();
                health?.Heal(healAmount);
            }
        }

        private void ApplyVisuals()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            Color baseColor = ownerTeam switch
            {
                UnitTeam.Player => new Color(0.28f, 0.9f, 1f),
                UnitTeam.Enemy => new Color(1f, 0.42f, 0.22f),
                _ => new Color(0.75f, 0.75f, 0.78f)
            };

            Color tierAccent = nodeTier switch
            {
                ControlNodeTier.Grand => new Color(1f, 0.92f, 0.52f),
                ControlNodeTier.Major => new Color(0.92f, 0.84f, 0.64f),
                _ => baseColor
            };

            cachedRenderer.material.color = Color.Lerp(baseColor, tierAccent, nodeTier == ControlNodeTier.Minor ? 0f : 0.25f);

            float scale = nodeTier switch
            {
                ControlNodeTier.Grand => 1.35f,
                ControlNodeTier.Major => 1.16f,
                _ => 1f
            };
            transform.localScale = new Vector3(scale, transform.localScale.y, scale);
            UpdateStatusVisuals();
            UpdateZoneVisuals();
            UpdateObjectiveVisuals();
        }

        private void EnsureStatusVisuals()
        {
            if (statusAnchor != null)
            {
                return;
            }

            statusAnchor = new GameObject("Node Status Anchor").transform;
            statusAnchor.SetParent(transform);
            statusAnchor.localPosition = new Vector3(0f, 1.72f, 0f);
            statusAnchor.localRotation = Quaternion.identity;
            statusAnchor.localScale = Vector3.one;

            Transform captureFrame = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Capture Frame",
                new Vector3(0f, 0f, 0f),
                new Vector3(1.14f, 0.08f, 0.12f),
                new Color(0.1f, 0.1f, 0.12f));
            captureBarFrameRenderer = captureFrame.GetComponent<Renderer>();

            captureBarFill = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Capture Fill",
                new Vector3(-0.5f, 0f, 0f),
                new Vector3(0.01f, 0.05f, 0.06f),
                new Color(0.72f, 0.72f, 0.76f));
            captureBarFillRenderer = captureBarFill.GetComponent<Renderer>();

            ownerBeacon = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Sphere,
                "Owner Beacon",
                new Vector3(0f, 0.28f, 0f),
                new Vector3(0.18f, 0.18f, 0.18f),
                new Color(0.72f, 0.72f, 0.76f));
            ownerBeaconRenderer = ownerBeacon.GetComponent<Renderer>();

            contestBeacon = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cylinder,
                "Contest Beacon",
                new Vector3(0f, 0.5f, 0f),
                new Vector3(0.05f, 0.18f, 0.05f),
                new Color(1f, 0.9f, 0.52f));
            contestBeaconRenderer = contestBeacon.GetComponent<Renderer>();

            playerStrengthColumn = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Player Strength Column",
                new Vector3(-0.74f, 0.14f, 0f),
                new Vector3(0.1f, 0.28f, 0.1f),
                new Color(0.32f, 0.92f, 1f));
            playerStrengthColumnRenderer = playerStrengthColumn.GetComponent<Renderer>();

            enemyStrengthColumn = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Enemy Strength Column",
                new Vector3(0.74f, 0.14f, 0f),
                new Vector3(0.1f, 0.28f, 0.1f),
                new Color(1f, 0.46f, 0.24f));
            enemyStrengthColumnRenderer = enemyStrengthColumn.GetComponent<Renderer>();

            pressureBalanceBar = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Pressure Balance Bar",
                new Vector3(0f, -0.2f, 0f),
                new Vector3(0.92f, 0.04f, 0.06f),
                new Color(0.16f, 0.16f, 0.2f));
            pressureBalanceBarRenderer = pressureBalanceBar.GetComponent<Renderer>();

            pressureBalanceNeedle = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Pressure Balance Needle",
                new Vector3(0f, -0.16f, 0f),
                new Vector3(0.08f, 0.12f, 0.08f),
                new Color(0.86f, 0.86f, 0.9f));
            pressureBalanceNeedleRenderer = pressureBalanceNeedle.GetComponent<Renderer>();

            flipWarningRoot = new GameObject("Flip Warning Root").transform;
            flipWarningRoot.SetParent(statusAnchor);
            flipWarningRoot.localPosition = new Vector3(0f, 0.92f, 0f);
            flipWarningRoot.localRotation = Quaternion.identity;
            flipWarningRoot.localScale = Vector3.one;

            flipWarningRing = CreateStatusPrimitive(
                flipWarningRoot,
                PrimitiveType.Cylinder,
                "Flip Warning Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.2f, 0.03f, 0.2f),
                new Color(1f, 0.9f, 0.4f));
            flipWarningRingRenderer = flipWarningRing.GetComponent<Renderer>();

            flipWarningSpire = CreateStatusPrimitive(
                flipWarningRoot,
                PrimitiveType.Cube,
                "Flip Warning Spire",
                new Vector3(0f, 0.16f, 0f),
                new Vector3(0.1f, 0.32f, 0.1f),
                new Color(1f, 0.9f, 0.4f));
            flipWarningSpireRenderer = flipWarningSpire.GetComponent<Renderer>();

            stabilityRoot = new GameObject("Stability Root").transform;
            stabilityRoot.SetParent(statusAnchor);
            stabilityRoot.localPosition = new Vector3(0f, 0.78f, 0f);
            stabilityRoot.localRotation = Quaternion.identity;
            stabilityRoot.localScale = Vector3.one;

            stabilityRing = CreateStatusPrimitive(
                stabilityRoot,
                PrimitiveType.Cylinder,
                "Stability Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.18f, 0.025f, 0.18f),
                new Color(0.32f, 0.92f, 1f));
            stabilityRingRenderer = stabilityRing.GetComponent<Renderer>();

            stabilityCore = CreateStatusPrimitive(
                stabilityRoot,
                PrimitiveType.Sphere,
                "Stability Core",
                new Vector3(0f, 0.12f, 0f),
                new Vector3(0.14f, 0.14f, 0.14f),
                new Color(0.32f, 0.92f, 1f));
            stabilityCoreRenderer = stabilityCore.GetComponent<Renderer>();

            garrisonWarningRoot = new GameObject("Garrison Warning Root").transform;
            garrisonWarningRoot.SetParent(statusAnchor);
            garrisonWarningRoot.localPosition = new Vector3(0f, 1.12f, 0f);
            garrisonWarningRoot.localRotation = Quaternion.identity;
            garrisonWarningRoot.localScale = Vector3.one;

            garrisonWarningRing = CreateStatusPrimitive(
                garrisonWarningRoot,
                PrimitiveType.Cylinder,
                "Garrison Warning Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.18f, 0.025f, 0.18f),
                new Color(1f, 0.62f, 0.24f));
            garrisonWarningRingRenderer = garrisonWarningRing.GetComponent<Renderer>();

            garrisonWarningCore = CreateStatusPrimitive(
                garrisonWarningRoot,
                PrimitiveType.Cube,
                "Garrison Warning Core",
                new Vector3(0f, 0.14f, 0f),
                new Vector3(0.12f, 0.24f, 0.12f),
                new Color(1f, 0.62f, 0.24f));
            garrisonWarningCoreRenderer = garrisonWarningCore.GetComponent<Renderer>();

            playerPressureRoot = new GameObject("Player Pressure Root").transform;
            playerPressureRoot.SetParent(statusAnchor);
            playerPressureRoot.localPosition = new Vector3(0f, 0.62f, 0f);
            playerPressureRoot.localRotation = Quaternion.identity;
            playerPressureRoot.localScale = Vector3.one;

            playerPressureBeam = CreateStatusPrimitive(
                playerPressureRoot,
                PrimitiveType.Cube,
                "Player Pressure Beam",
                new Vector3(0f, 0f, 0.26f),
                new Vector3(0.03f, 0.03f, 0.52f),
                new Color(0.32f, 0.92f, 1f));
            playerPressureBeamRenderer = playerPressureBeam.GetComponent<Renderer>();

            playerPressureTip = CreateStatusPrimitive(
                playerPressureRoot,
                PrimitiveType.Cube,
                "Player Pressure Tip",
                new Vector3(0f, 0f, 0.56f),
                new Vector3(0.1f, 0.08f, 0.1f),
                new Color(0.32f, 0.92f, 1f));
            playerPressureTipRenderer = playerPressureTip.GetComponent<Renderer>();

            enemyPressureRoot = new GameObject("Enemy Pressure Root").transform;
            enemyPressureRoot.SetParent(statusAnchor);
            enemyPressureRoot.localPosition = new Vector3(0f, 0.62f, 0f);
            enemyPressureRoot.localRotation = Quaternion.identity;
            enemyPressureRoot.localScale = Vector3.one;

            enemyPressureBeam = CreateStatusPrimitive(
                enemyPressureRoot,
                PrimitiveType.Cube,
                "Enemy Pressure Beam",
                new Vector3(0f, 0f, 0.26f),
                new Vector3(0.03f, 0.03f, 0.52f),
                new Color(1f, 0.46f, 0.24f));
            enemyPressureBeamRenderer = enemyPressureBeam.GetComponent<Renderer>();

            enemyPressureTip = CreateStatusPrimitive(
                enemyPressureRoot,
                PrimitiveType.Cube,
                "Enemy Pressure Tip",
                new Vector3(0f, 0f, 0.56f),
                new Vector3(0.1f, 0.08f, 0.1f),
                new Color(1f, 0.46f, 0.24f));
            enemyPressureTipRenderer = enemyPressureTip.GetComponent<Renderer>();

            for (int i = 0; i < tierPips.Length; i++)
            {
                float x = (i - 1) * 0.3f;
                tierPips[i] = CreateStatusPrimitive(
                    statusAnchor,
                    PrimitiveType.Cube,
                    $"Tier Pip {i + 1}",
                    new Vector3(x, 0.28f, -0.18f),
                    new Vector3(0.12f, 0.06f, 0.06f),
                    new Color(0.9f, 0.84f, 0.64f));
                tierPipRenderers[i] = tierPips[i].GetComponent<Renderer>();
            }
        }

        private void UpdateStatusVisuals()
        {
            if (statusAnchor == null)
            {
                return;
            }

            statusAnchor.gameObject.SetActive(isActiveAndEnabled);

            float captureThreshold = Mathf.Max(0.01f, GetCaptureThreshold());
            float progressNormalized = Mathf.Clamp01(captureProgress / captureThreshold);
            UnitTeam? displayTeam = capturingTeam ?? ownerTeam;
            Color neutralColor = new Color(0.74f, 0.74f, 0.78f);
            Color ownerColor = GetTeamColor(ownerTeam, neutralColor);
            Color captureColor = GetTeamColor(displayTeam, neutralColor);
            bool contested = capturingTeam.HasValue && ownerTeam != capturingTeam;
            float pulse = 0.84f + Mathf.PingPong(Time.time * (contested ? 2.6f : 1.2f), 0.16f);

            if (captureBarFill != null)
            {
                float fillWidth = Mathf.Lerp(0.01f, 1.02f, contested || ownerTeam.HasValue ? Mathf.Max(progressNormalized, ownerTeam.HasValue && !contested ? 1f : 0f) : 0f);
                captureBarFill.localScale = new Vector3(fillWidth, 0.05f, 0.06f);
                captureBarFill.localPosition = new Vector3(-0.51f + fillWidth * 0.5f, 0f, 0f);
            }

            if (captureBarFillRenderer != null)
            {
                captureBarFillRenderer.material.color = contested ? captureColor * pulse : Color.Lerp(neutralColor, captureColor, ownerTeam.HasValue ? 0.92f : progressNormalized);
            }

            if (captureBarFrameRenderer != null)
            {
                captureBarFrameRenderer.material.color = contested
                    ? new Color(captureColor.r, captureColor.g, captureColor.b, 0.7f)
                    : new Color(0.1f, 0.1f, 0.12f, 0.95f);
            }

            if (ownerBeacon != null)
            {
                float ownerScale = contested ? 0.2f : 0.16f + GetTierPipCount() * 0.03f;
                ownerBeacon.localScale = new Vector3(ownerScale, ownerScale, ownerScale);
            }

            if (ownerBeaconRenderer != null)
            {
                ownerBeaconRenderer.material.color = contested ? Color.Lerp(ownerColor, captureColor, 0.5f) * pulse : ownerColor;
            }

            if (contestBeacon != null)
            {
                contestBeacon.gameObject.SetActive(contested);
                contestBeacon.localScale = contested
                    ? new Vector3(0.06f, 0.12f + Mathf.PingPong(Time.time * 1.8f, 0.12f), 0.06f)
                    : new Vector3(0.05f, 0.12f, 0.05f);
            }

            if (contested && contestBeaconRenderer != null)
            {
                contestBeaconRenderer.material.color = captureColor * pulse;
            }

            int tierPipCount = GetTierPipCount();
            for (int i = 0; i < tierPips.Length; i++)
            {
                bool active = i < tierPipCount;
                if (tierPips[i] != null)
                {
                    tierPips[i].gameObject.SetActive(active);
                }

                if (active && tierPipRenderers[i] != null)
                {
                    Color tierColor = nodeTier switch
                    {
                        ControlNodeTier.Grand => new Color(1f, 0.92f, 0.52f),
                        ControlNodeTier.Major => new Color(0.92f, 0.84f, 0.64f),
                        _ => ownerColor
                    };
                    tierPipRenderers[i].material.color = contested ? Color.Lerp(tierColor, captureColor, 0.35f) : tierColor;
                }
            }

            UpdateFlipWarningVisual(progressNormalized, contested, captureColor);
            UpdateStabilityVisual(progressNormalized, ownerColor, contested);
            UpdateGarrisonWarningVisual(contested);
            UpdatePressureBalanceVisual();
            UpdateStrengthColumn(playerStrengthColumn, playerStrengthColumnRenderer, UnitTeam.Player);
            UpdateStrengthColumn(enemyStrengthColumn, enemyStrengthColumnRenderer, UnitTeam.Enemy);
            UpdatePressureVisual(playerPressureRoot, playerPressureBeam, playerPressureBeamRenderer, playerPressureTip, playerPressureTipRenderer, UnitTeam.Player);
            UpdatePressureVisual(enemyPressureRoot, enemyPressureBeam, enemyPressureBeamRenderer, enemyPressureTip, enemyPressureTipRenderer, UnitTeam.Enemy);
        }

        private static Color GetTeamColor(UnitTeam? team, Color fallback)
        {
            return team switch
            {
                UnitTeam.Player => new Color(0.28f, 0.9f, 1f),
                UnitTeam.Enemy => new Color(1f, 0.42f, 0.22f),
                _ => fallback
            };
        }

        private int GetTierPipCount()
        {
            return nodeTier switch
            {
                ControlNodeTier.Grand => 3,
                ControlNodeTier.Major => 2,
                _ => 1
            };
        }

        private void UpdatePressureBalanceVisual()
        {
            if (pressureBalanceBar == null || pressureBalanceNeedle == null)
            {
                return;
            }

            int playerPresence = CountTeamPresence(UnitTeam.Player, out _, out float playerPressure);
            int enemyPresence = CountTeamPresence(UnitTeam.Enemy, out _, out float enemyPressure);
            bool show = isActiveAndEnabled && (playerPresence > 0 || enemyPresence > 0);
            pressureBalanceBar.gameObject.SetActive(show);
            pressureBalanceNeedle.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float playerScore = playerPresence + playerPressure * 2f;
            float enemyScore = enemyPresence + enemyPressure * 2f;
            float totalScore = Mathf.Max(0.001f, playerScore + enemyScore);
            float balance = Mathf.Clamp((enemyScore - playerScore) / totalScore, -1f, 1f);
            float pulse = 0.88f + Mathf.PingPong(Time.time * 2f, 0.08f);
            Color playerColor = new Color(0.32f, 0.92f, 1f);
            Color enemyColor = new Color(1f, 0.46f, 0.24f);
            Color centerColor = new Color(0.84f, 0.84f, 0.88f);
            Color balanceColor = balance < 0f
                ? Color.Lerp(centerColor, playerColor, Mathf.Abs(balance))
                : Color.Lerp(centerColor, enemyColor, Mathf.Abs(balance));

            if (pressureBalanceBarRenderer != null)
            {
                pressureBalanceBarRenderer.material.color = Color.Lerp(playerColor, enemyColor, (balance + 1f) * 0.5f) * pulse;
            }

            float needleX = balance * 0.4f;
            pressureBalanceNeedle.localPosition = new Vector3(needleX, -0.16f + Mathf.PingPong(Time.time * 1.4f, 0.03f), 0f);
            pressureBalanceNeedle.localScale = new Vector3(0.08f, 0.1f + Mathf.Abs(balance) * 0.06f, 0.08f);

            if (pressureBalanceNeedleRenderer != null)
            {
                pressureBalanceNeedleRenderer.material.color = balanceColor * pulse;
            }
        }

        private void UpdateFlipWarningVisual(float progressNormalized, bool contested, Color captureColor)
        {
            if (flipWarningRoot == null)
            {
                return;
            }

            float urgency = contested ? Mathf.InverseLerp(0.58f, 1f, progressNormalized) : 0f;
            bool show = contested && urgency > 0f;
            flipWarningRoot.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float pulse = 0.92f + Mathf.PingPong(Time.time * (3.4f + urgency * 4.2f), 0.18f + urgency * 0.12f);
            Color warningColor = Color.Lerp(new Color(1f, 0.9f, 0.42f), captureColor, 0.55f + urgency * 0.25f);
            float ringRadius = 0.18f + urgency * 0.18f;
            float spireHeight = 0.18f + urgency * 0.38f;

            flipWarningRoot.localPosition = new Vector3(0f, 0.92f + Mathf.PingPong(Time.time * 1.8f, 0.08f + urgency * 0.06f), 0f);

            if (flipWarningRing != null)
            {
                flipWarningRing.localScale = new Vector3(ringRadius, 0.03f, ringRadius);
            }

            if (flipWarningRingRenderer != null)
            {
                flipWarningRingRenderer.material.color = warningColor * pulse;
            }

            if (flipWarningSpire != null)
            {
                flipWarningSpire.localPosition = new Vector3(0f, 0.1f + spireHeight * 0.5f, 0f);
                flipWarningSpire.localScale = new Vector3(0.08f + urgency * 0.06f, spireHeight, 0.08f + urgency * 0.06f);
            }

            if (flipWarningSpireRenderer != null)
            {
                flipWarningSpireRenderer.material.color = Color.Lerp(warningColor, Color.white, 0.18f + urgency * 0.12f) * pulse;
            }
        }

        private void UpdateStabilityVisual(float progressNormalized, Color ownerColor, bool contested)
        {
            if (stabilityRoot == null)
            {
                return;
            }

            bool show = ownerTeam.HasValue && !contested;
            stabilityRoot.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            int ownerPresence = CountTeamPresence(ownerTeam.Value, out _, out float ownerPressure);
            float presenceFactor = Mathf.Clamp01(ownerPresence / 6f);
            float stability = Mathf.Clamp01(Mathf.Max(progressNormalized, 0.38f + presenceFactor * 0.32f + ownerPressure * 0.3f));
            float pulse = 0.86f + Mathf.PingPong(Time.time * (1.1f + stability * 1.4f), 0.08f + stability * 0.06f);
            float ringRadius = 0.16f + stability * 0.18f;
            float coreScale = 0.12f + stability * 0.12f;

            stabilityRoot.localPosition = new Vector3(0f, 0.78f + Mathf.PingPong(Time.time * 1.2f, 0.04f), 0f);

            if (stabilityRing != null)
            {
                stabilityRing.localScale = new Vector3(ringRadius, 0.025f, ringRadius);
            }

            if (stabilityRingRenderer != null)
            {
                stabilityRingRenderer.material.color = Color.Lerp(ownerColor, Color.white, 0.08f + stability * 0.1f) * pulse;
            }

            if (stabilityCore != null)
            {
                stabilityCore.localScale = Vector3.one * coreScale;
            }

            if (stabilityCoreRenderer != null)
            {
                stabilityCoreRenderer.material.color = Color.Lerp(ownerColor, Color.white, 0.16f + stability * 0.12f) * pulse;
            }
        }

        private void UpdateGarrisonWarningVisual(bool contested)
        {
            if (garrisonWarningRoot == null)
            {
                return;
            }

            bool show = ownerTeam.HasValue && !contested;
            float deficit = 0f;
            Color warningColor = new Color(1f, 0.62f, 0.24f);

            if (show)
            {
                int ownerPresence = CountTeamPresence(ownerTeam.Value, out _, out _);
                deficit = 1f - Mathf.Clamp01(ownerPresence / (float)Mathf.Max(1, DesiredGarrison));
                show = deficit > 0.35f;
                warningColor = ownerTeam == UnitTeam.Player
                    ? new Color(1f, 0.62f, 0.24f)
                    : new Color(0.44f, 0.92f, 1f);
            }

            garrisonWarningRoot.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float pulse = 0.9f + Mathf.PingPong(Time.time * (2.4f + deficit * 3f), 0.12f + deficit * 0.14f);
            float ringRadius = 0.16f + deficit * 0.18f;
            float coreHeight = 0.14f + deficit * 0.28f;

            garrisonWarningRoot.localPosition = new Vector3(0f, 1.12f + Mathf.PingPong(Time.time * 1.6f, 0.06f), 0f);

            if (garrisonWarningRing != null)
            {
                garrisonWarningRing.localScale = new Vector3(ringRadius, 0.025f, ringRadius);
            }

            if (garrisonWarningRingRenderer != null)
            {
                garrisonWarningRingRenderer.material.color = warningColor * pulse;
            }

            if (garrisonWarningCore != null)
            {
                garrisonWarningCore.localPosition = new Vector3(0f, 0.08f + coreHeight * 0.5f, 0f);
                garrisonWarningCore.localScale = new Vector3(0.1f + deficit * 0.04f, coreHeight, 0.1f + deficit * 0.04f);
            }

            if (garrisonWarningCoreRenderer != null)
            {
                garrisonWarningCoreRenderer.material.color = Color.Lerp(warningColor, Color.white, 0.16f + deficit * 0.12f) * pulse;
            }
        }

        private void UpdateStrengthColumn(Transform column, Renderer columnRenderer, UnitTeam team)
        {
            if (column == null)
            {
                return;
            }

            int presence = CountTeamPresence(team, out _, out float pressure);
            bool show = isActiveAndEnabled && presence > 0;
            column.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            float height = 0.16f + pressure * 0.5f;
            float pulse = 0.88f + Mathf.PingPong(Time.time * (1.8f + pressure * 2.2f), 0.12f);
            Color strengthColor = team == UnitTeam.Player
                ? new Color(0.32f, 0.92f, 1f)
                : new Color(1f, 0.46f, 0.24f);
            float xPosition = team == UnitTeam.Player ? -0.74f : 0.74f;

            column.localPosition = new Vector3(xPosition, -0.02f + height * 0.5f, 0f);
            column.localScale = new Vector3(0.1f, height, 0.1f);

            if (columnRenderer != null)
            {
                columnRenderer.material.color = Color.Lerp(strengthColor, Color.white, 0.14f + pressure * 0.12f) * pulse;
            }
        }

        private void UpdatePressureVisual(
            Transform root,
            Transform beam,
            Renderer beamRenderer,
            Transform tip,
            Renderer tipRenderer,
            UnitTeam team)
        {
            if (root == null)
            {
                return;
            }

            int presence = CountTeamPresence(team, out Vector3 direction, out float pressure);
            bool show = isActiveAndEnabled && presence > 0;
            root.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            Color pressureColor = team == UnitTeam.Player
                ? new Color(0.32f, 0.92f, 1f)
                : new Color(1f, 0.46f, 0.24f);
            float pulse = 0.86f + Mathf.PingPong(Time.time * (2f + pressure * 2.6f), 0.12f + pressure * 0.1f);
            float length = 0.34f + pressure * 0.24f;

            root.localPosition = new Vector3(0f, 0.62f + Mathf.PingPong(Time.time * 1.2f + (team == UnitTeam.Player ? 0f : 0.4f), 0.04f), 0f);
            root.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            if (beam != null)
            {
                beam.localPosition = new Vector3(0f, 0f, length * 0.5f);
                beam.localScale = new Vector3(0.03f, 0.03f, length);
            }

            if (beamRenderer != null)
            {
                beamRenderer.material.color = pressureColor * pulse;
            }

            if (tip != null)
            {
                tip.localPosition = new Vector3(0f, 0f, length + 0.06f);
                tip.localScale = new Vector3(0.08f + pressure * 0.04f, 0.08f, 0.08f + pressure * 0.04f);
            }

            if (tipRenderer != null)
            {
                tipRenderer.material.color = Color.Lerp(pressureColor, Color.white, 0.18f) * pulse;
            }
        }

        private int CountTeamPresence(UnitTeam team, out Vector3 direction, out float pressure)
        {
            direction = Vector3.forward;
            pressure = 0f;
            int count = 0;
            Vector3 centroid = Vector3.zero;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || unit.Team != team)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance > controlRadius)
                {
                    continue;
                }

                count++;
                centroid += unit.transform.position;
                pressure = Mathf.Max(pressure, 1f - distance / Mathf.Max(0.01f, controlRadius));
            }

            pressure = Mathf.Clamp01(Mathf.Max(pressure, count / 6f));
            if (count > 0)
            {
                direction = centroid / count - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    direction = Vector3.forward;
                }
            }

            return count;
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

        private void EnsureZoneVisuals()
        {
            if (zoneAnchor != null)
            {
                return;
            }

            zoneAnchor = new GameObject("Zone Anchor").transform;
            zoneAnchor.SetParent(transform);
            zoneAnchor.localPosition = new Vector3(0f, 0.06f, 0f);
            zoneAnchor.localRotation = Quaternion.identity;
            zoneAnchor.localScale = Vector3.one;

            for (int i = 0; i < controlPosts.Length; i++)
            {
                controlPosts[i] = CreateStatusPrimitive(
                    zoneAnchor,
                    PrimitiveType.Cylinder,
                    $"Control Post {i + 1}",
                    Vector3.zero,
                    new Vector3(0.18f, 0.16f, 0.18f),
                    new Color(0.72f, 0.72f, 0.76f));
                controlPostRenderers[i] = controlPosts[i].GetComponent<Renderer>();
            }

            for (int i = 0; i < healingBeacons.Length; i++)
            {
                healingBeacons[i] = CreateStatusPrimitive(
                    zoneAnchor,
                    PrimitiveType.Sphere,
                    $"Healing Beacon {i + 1}",
                    Vector3.zero,
                    new Vector3(0.22f, 0.22f, 0.22f),
                    new Color(0.44f, 1f, 0.84f));
                healingBeaconRenderers[i] = healingBeacons[i].GetComponent<Renderer>();
            }
        }

        private void UpdateZoneVisuals()
        {
            if (zoneAnchor == null)
            {
                return;
            }

            float safeScaleX = Mathf.Approximately(transform.localScale.x, 0f) ? 1f : transform.localScale.x;
            float safeScaleZ = Mathf.Approximately(transform.localScale.z, 0f) ? 1f : transform.localScale.z;
            zoneAnchor.localScale = new Vector3(1f / safeScaleX, 1f, 1f / safeScaleZ);

            UnitTeam? displayTeam = capturingTeam ?? ownerTeam;
            Color neutralColor = new Color(0.62f, 0.62f, 0.66f);
            Color controlColor = GetTeamColor(displayTeam, neutralColor);
            bool contested = capturingTeam.HasValue && ownerTeam != capturingTeam;
            float pulse = 0.82f + Mathf.PingPong(Time.time * (contested ? 2.6f : 1.2f), 0.18f);

            for (int i = 0; i < controlPosts.Length; i++)
            {
                if (controlPosts[i] == null)
                {
                    continue;
                }

                float angle = i / (float)controlPosts.Length * Mathf.PI * 2f;
                Vector3 ringPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * controlRadius;
                controlPosts[i].localPosition = ringPosition;
                controlPosts[i].localScale = new Vector3(0.18f, 0.12f + (i % 2 == 0 ? 0.06f : 0f), 0.18f);

                if (controlPostRenderers[i] != null)
                {
                    controlPostRenderers[i].material.color = Color.Lerp(neutralColor, controlColor, displayTeam.HasValue ? 0.9f : 0.25f) * pulse;
                }
            }

            bool showHealing = ownerTeam.HasValue;
            for (int i = 0; i < healingBeacons.Length; i++)
            {
                if (healingBeacons[i] == null)
                {
                    continue;
                }

                healingBeacons[i].gameObject.SetActive(showHealing);
                if (!showHealing)
                {
                    continue;
                }

                float angle = i / (float)healingBeacons.Length * Mathf.PI * 2f + Time.time * 0.22f;
                Vector3 ringPosition = new Vector3(Mathf.Cos(angle), 0.24f + Mathf.Sin(Time.time * 1.8f + i) * 0.05f, Mathf.Sin(angle)) * healingRadius;
                healingBeacons[i].localPosition = ringPosition;
                healingBeacons[i].localScale = Vector3.one * (0.18f + nodeTier switch
                {
                    ControlNodeTier.Grand => 0.08f,
                    ControlNodeTier.Major => 0.04f,
                    _ => 0f
                });

                if (healingBeaconRenderers[i] != null)
                {
                    Color healingColor = ownerTeam == UnitTeam.Player
                        ? new Color(0.42f, 1f, 0.88f)
                        : new Color(1f, 0.62f, 0.34f);
                    healingBeaconRenderers[i].material.color = healingColor * pulse;
                }
            }
        }

        private void EnsureObjectiveVisuals()
        {
            if (objectiveAnchor != null)
            {
                return;
            }

            objectiveAnchor = new GameObject("Objective Anchor").transform;
            objectiveAnchor.SetParent(transform);
            objectiveAnchor.localPosition = new Vector3(0f, 2.46f, 0f);
            objectiveAnchor.localRotation = Quaternion.identity;
            objectiveAnchor.localScale = Vector3.one;

            playerObjectiveMarker = CreateStatusPrimitive(
                objectiveAnchor,
                PrimitiveType.Cube,
                "Player Objective Marker",
                new Vector3(-0.34f, 0f, 0f),
                new Vector3(0.12f, 0.24f, 0.12f),
                new Color(0.32f, 0.92f, 1f));
            playerObjectiveRenderer = playerObjectiveMarker.GetComponent<Renderer>();

            enemyObjectiveMarker = CreateStatusPrimitive(
                objectiveAnchor,
                PrimitiveType.Cube,
                "Enemy Objective Marker",
                new Vector3(0.34f, 0f, 0f),
                new Vector3(0.12f, 0.24f, 0.12f),
                new Color(1f, 0.46f, 0.24f));
            enemyObjectiveRenderer = enemyObjectiveMarker.GetComponent<Renderer>();
        }

        private void UpdateObjectiveVisuals()
        {
            if (objectiveAnchor == null)
            {
                return;
            }

            BattleDirectiveController directiveController = BattleDirectiveController.Instance;
            bool playerPriority = directiveController != null && directiveController.FindPriorityNodeFor(UnitTeam.Player) == this;
            bool enemyPriority = directiveController != null && directiveController.FindPriorityNodeFor(UnitTeam.Enemy) == this;
            bool hasAnyPriority = playerPriority || enemyPriority;
            objectiveAnchor.gameObject.SetActive(hasAnyPriority);

            if (!hasAnyPriority)
            {
                return;
            }

            float pulse = 0.86f + Mathf.PingPong(Time.time * 3.2f, 0.18f);

            if (playerObjectiveMarker != null)
            {
                playerObjectiveMarker.gameObject.SetActive(playerPriority);
                if (playerPriority)
                {
                    playerObjectiveMarker.localPosition = new Vector3(-0.34f, 0.12f + Mathf.PingPong(Time.time * 1.6f, 0.08f), 0f);
                    playerObjectiveMarker.localScale = new Vector3(0.12f, 0.22f * pulse, 0.12f);
                }
            }

            if (playerObjectiveRenderer != null && playerPriority)
            {
                playerObjectiveRenderer.material.color = new Color(0.32f, 0.92f, 1f) * pulse;
            }

            if (enemyObjectiveMarker != null)
            {
                enemyObjectiveMarker.gameObject.SetActive(enemyPriority);
                if (enemyPriority)
                {
                    enemyObjectiveMarker.localPosition = new Vector3(0.34f, 0.12f + Mathf.PingPong(Time.time * 1.9f, 0.08f), 0f);
                    enemyObjectiveMarker.localScale = new Vector3(0.12f, 0.22f * pulse, 0.12f);
                }
            }

            if (enemyObjectiveRenderer != null && enemyPriority)
            {
                enemyObjectiveRenderer.material.color = new Color(1f, 0.46f, 0.24f) * pulse;
            }
        }
    }
}
