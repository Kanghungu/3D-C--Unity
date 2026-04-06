using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Spawns reinforcements, manages rally, and drives multi-phase base behavior.
    /// </summary>
    public class BaseStructure : MonoBehaviour
    {
        [SerializeField] private float enemyAutoSpawnInterval = 11.5f;
        [SerializeField] private int enemyMaxUnits = 1000;
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private Vector3 playerDefaultRallyOffset = new(34f, 0f, 18f);
        [SerializeField] private Vector3 enemyDefaultRallyOffset = new(-28f, 0f, -16f);
        [SerializeField] private float threatWarningRadius = 28f;

        private readonly UnitArchetype[] enemyReinforcementCycle =
        {
            UnitArchetype.Rifleman,
            UnitArchetype.Rifleman,
            UnitArchetype.Spearman,
            UnitArchetype.Rifleman,
            UnitArchetype.ShieldInfantry,
            UnitArchetype.Spearman
        };

        private readonly float[] phaseThresholds = { 0.82f, 0.62f, 0.38f };
        private readonly float[] phaseScaleMultipliers = { 1f, 0.94f, 0.87f, 0.8f };
        private readonly float[] phaseProductionMultipliers = { 1f, 1.12f, 1.24f, 1.4f };

        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private PrototypeGameDatabase database;
        private Renderer cachedRenderer;
        private Vector3 baseScale = Vector3.one;
        private float autoSpawnTimer;
        private float productionSpeedMultiplier = 1f;
        private bool hasRallyPoint;
        private int reinforcementIndex;
        private int currentPhase = 1;
        private Vector3 rallyPoint;
        private GameObject rallyMarker;
        private Transform phaseAnchor;
        private Renderer phaseCoreRenderer;
        private readonly Transform[] phasePillars = new Transform[4];
        private readonly Renderer[] phasePillarRenderers = new Renderer[4];
        private Transform phaseHalo;
        private Renderer phaseHaloRenderer;
        private Transform assaultAnchor;
        private Transform assaultMarker;
        private Renderer assaultMarkerRenderer;
        private Transform threatAnchor;
        private Transform threatRing;
        private Renderer threatRingRenderer;
        private Transform threatCore;
        private Renderer threatCoreRenderer;
        private Transform threatDirectionRoot;
        private Transform threatDirectionBeam;
        private Renderer threatDirectionBeamRenderer;
        private Transform threatDirectionTip;
        private Renderer threatDirectionTipRenderer;
        private readonly Transform[] threatSpikes = new Transform[4];
        private readonly Renderer[] threatSpikeRenderers = new Renderer[4];
        private float lastPhaseAdvanceTime = -10f;
        private int nearbyHostileCount;
        private int nearbyFriendlyCount;
        private float nearbyThreatPressure;
        private Vector3 nearbyThreatDirection = Vector3.forward;

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public float HealthNormalized => health != null ? health.Normalized : 0f;
        public bool HasQueuedProduction => false;
        public int QueueCount => 0;
        public float ProductionProgressNormalized => 0f;
        public string QueueLabel => Team == UnitTeam.Enemy ? $"Auto Reinforce P{currentPhase}" : $"Citadel Phase {currentPhase}";
        public string QueuePreview => Team == UnitTeam.Enemy ? "Enemy waves" : "Use Foundry";
        public bool HasRallyPoint => hasRallyPoint;
        public Vector3 RallyPoint => rallyPoint;
        public string RallyLabel => hasRallyPoint ? $"{rallyPoint.x:0.0}, {rallyPoint.z:0.0}" : "Unset";
        public int CurrentPhase => currentPhase;
        public int NearbyHostileCount => nearbyHostileCount;
        public int NearbyFriendlyCount => nearbyFriendlyCount;
        public float NearbyThreatPressure => nearbyThreatPressure;
        public string PhaseLabel => currentPhase switch
        {
            1 => "Bulwark",
            2 => "Breach",
            3 => "Last Rite",
            _ => "Collapse"
        };
        public string PhaseStatusLabel => currentPhase switch
        {
            1 => "諛⑹뼱???좎?",
            2 => "洹좎뿴 諛쒖깮",
            3 => "遺뺢눼 吏곸쟾",
            _ => "理쒖쥌 遺뺢눼",
        };
        public bool IsDefenseEmergency => nearbyHostileCount >= Mathf.Max(5, nearbyFriendlyCount + 4) || (CurrentPhase >= 3 && nearbyHostileCount > nearbyFriendlyCount);
        public string DefenseUrgencyLabel => GetDefenseUrgencyLabel();

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            baseScale = transform.localScale;
            ApplyVisuals();
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();
            EnsurePhaseVisuals();
            EnsureAssaultVisuals();
            EnsureThreatVisuals();
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
            UpdateThreatVisuals();
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
            UpdateRallyMarker();
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
            UpdateThreatVisuals();

            if (!IsAlive || unitRoot == null || database == null)
            {
                return;
            }

            EvaluatePhaseState();
            if (Team == UnitTeam.Enemy)
            {
                RunEnemyAutoProduction();
            }
        }

        public void InitializeTarget(CombatTarget assignedTarget, UnitHealth assignedHealth)
        {
            combatTarget = assignedTarget;
            health = assignedHealth;
            cachedRenderer = GetComponent<Renderer>();
            baseScale = transform.localScale;
            ApplyVisuals();
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();
            EnsurePhaseVisuals();
            EnsureAssaultVisuals();
            EnsureThreatVisuals();
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
            UpdateThreatVisuals();
        }

        public void Initialize(Transform assignedUnitRoot, PrototypeGameDatabase assignedDatabase)
        {
            unitRoot = assignedUnitRoot;
            database = assignedDatabase;
            autoSpawnTimer = enemyAutoSpawnInterval;
            ApplyVisuals();
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();
            EnsurePhaseVisuals();
            EnsureAssaultVisuals();
            EnsureThreatVisuals();
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
            UpdateThreatVisuals();

            Vector3 defaultOffset = Team == UnitTeam.Player ? playerDefaultRallyOffset : enemyDefaultRallyOffset;
            SetRallyPoint(transform.position + defaultOffset);
        }

        public void SetRallyPoint(Vector3 worldPoint)
        {
            rallyPoint = new Vector3(worldPoint.x, 1f, worldPoint.z);
            hasRallyPoint = true;
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();
            UpdateRallyMarker();
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
            UpdateThreatVisuals();
        }

        public void SetProductionSpeedMultiplier(float multiplier)
        {
            productionSpeedMultiplier = Mathf.Max(0.5f, multiplier);
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
            UpdateThreatVisuals();
        }

        private void EvaluatePhaseState()
        {
            while (currentPhase <= phaseThresholds.Length && HealthNormalized <= phaseThresholds[currentPhase - 1])
            {
                AdvancePhase(currentPhase + 1);
            }
        }

        private void AdvancePhase(int nextPhase)
        {
            currentPhase = Mathf.Clamp(nextPhase, 1, phaseScaleMultipliers.Length);
            lastPhaseAdvanceTime = Time.time;
            ApplyVisuals();
            SpawnPhaseReinforcements();

            string teamLabel = Team == UnitTeam.Player ? "?꾧뎔 蹂몄쭊" : "??蹂몄쭊";
            BattleDirectiveController.BroadcastNewsStatic($"{teamLabel} ?④퀎 蹂?? P{currentPhase} {PhaseStatusLabel}");
        }

        private void RunEnemyAutoProduction()
        {
            autoSpawnTimer -= Time.deltaTime * GetEffectiveProductionSpeedMultiplier();
            if (autoSpawnTimer > 0f)
            {
                return;
            }

            UnitArchetype nextArchetype = enemyReinforcementCycle[reinforcementIndex % enemyReinforcementCycle.Length];
            UnitDefinition definition = database.GetDefinition(nextArchetype);
            reinforcementIndex++;

            if (!PrototypeProductionRules.CanReserveUnit(Team, definition, 0, 0, enemyMaxUnits))
            {
                autoSpawnTimer = 4.5f;
                return;
            }

            SpawnUnit(definition);
            autoSpawnTimer = Mathf.Max(4.5f, enemyAutoSpawnInterval - (currentPhase - 1) * 1.9f);
        }

        private void SpawnPhaseReinforcements()
        {
            if (database == null || unitRoot == null)
            {
                return;
            }

            SpawnPhaseWave(GetPhaseWaveArchetypes());
        }

        private void SpawnPhaseWave(UnitArchetype[] archetypes)
        {
            if (archetypes == null)
            {
                return;
            }

            int reservedTotal = 0;
            Dictionary<UnitArchetype, int> reservedByArchetype = new();

            foreach (UnitArchetype archetype in archetypes)
            {
                UnitDefinition definition = database.GetDefinition(archetype);
                int reservedForArchetype = reservedByArchetype.TryGetValue(archetype, out int currentReserved) ? currentReserved : 0;

                if (!PrototypeProductionRules.CanReserveUnit(Team, definition, reservedTotal, reservedForArchetype, enemyMaxUnits))
                {
                    continue;
                }

                if (SpawnUnit(definition))
                {
                    reservedTotal++;
                    reservedByArchetype[archetype] = reservedForArchetype + 1;
                }
            }
        }

        private UnitArchetype[] GetPhaseWaveArchetypes()
        {
            // 페이즈 증원은 보병 삼각 + 포/전투기 중심으로 단순화 (특수·거점급 유닛 난사 방지)
            if (Team == UnitTeam.Player)
            {
                return currentPhase switch
                {
                    2 => new[]
                    {
                        UnitArchetype.ShieldInfantry, UnitArchetype.ShieldInfantry, UnitArchetype.ShieldInfantry,
                        UnitArchetype.Spearman, UnitArchetype.Spearman, UnitArchetype.Spearman,
                        UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Rifleman
                    },
                    3 => new[]
                    {
                        UnitArchetype.ShieldInfantry, UnitArchetype.Spearman, UnitArchetype.Rifleman,
                        UnitArchetype.Rifleman, UnitArchetype.Rifleman,
                        UnitArchetype.Artillery, UnitArchetype.Fighter
                    },
                    4 => new[]
                    {
                        UnitArchetype.ShieldInfantry, UnitArchetype.Spearman, UnitArchetype.Rifleman,
                        UnitArchetype.Artillery, UnitArchetype.Artillery,
                        UnitArchetype.Fighter, UnitArchetype.Fighter
                    },
                    _ => System.Array.Empty<UnitArchetype>()
                };
            }

            return currentPhase switch
            {
                2 => new[]
                {
                    UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Rifleman,
                    UnitArchetype.Spearman, UnitArchetype.Spearman, UnitArchetype.ShieldInfantry,
                    UnitArchetype.Rifleman
                },
                3 => new[]
                {
                    UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Spearman,
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.Artillery, UnitArchetype.Fighter
                },
                4 => new[]
                {
                    UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Spearman,
                    UnitArchetype.Artillery, UnitArchetype.Fighter, UnitArchetype.Fighter
                },
                _ => System.Array.Empty<UnitArchetype>()
            };
        }

        private bool SpawnUnit(UnitDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            Vector3 offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y = definition.IsFlying ? definition.HoverHeight : 1f;
            SelectableUnit unit = PrototypeEntityFactory.CreateUnit(Team, definition, spawnPosition, unitRoot);

            if (unit != null && hasRallyPoint)
            {
                unit.MoveTo(rallyPoint);
            }

            return unit != null;
        }

        private float GetEffectiveProductionSpeedMultiplier()
        {
            return productionSpeedMultiplier * phaseProductionMultipliers[Mathf.Clamp(currentPhase - 1, 0, phaseProductionMultipliers.Length - 1)];
        }

        private void ApplyVisuals()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            Color baseColor = Team == UnitTeam.Player ? new Color(0.2f, 0.55f, 1f) : new Color(0.9f, 0.25f, 0.2f);
            float phaseTint = Mathf.Clamp01((currentPhase - 1) * 0.12f);
            cachedRenderer.material.color = Color.Lerp(baseColor, new Color(1f, 0.84f, 0.48f), phaseTint);

            float scaleMultiplier = phaseScaleMultipliers[Mathf.Clamp(currentPhase - 1, 0, phaseScaleMultipliers.Length - 1)];
            transform.localScale = baseScale * scaleMultiplier;
            UpdatePhaseVisuals();
            UpdateAssaultVisuals();
        }

        private void EnsureRallyMarker()
        {
            if (rallyMarker != null)
            {
                return;
            }

            rallyMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rallyMarker.name = $"{name} Rally Marker";
            rallyMarker.transform.localScale = new Vector3(1.3f, 0.04f, 1.3f);
            rallyMarker.GetComponent<Collider>().enabled = false;
            rallyMarker.SetActive(false);
        }

        private void UpdateRallyMarkerVisuals()
        {
            if (rallyMarker == null)
            {
                return;
            }

            Renderer rendererComponent = rallyMarker.GetComponent<Renderer>();
            rendererComponent.material.color = Team == UnitTeam.Player
                ? new Color(0.25f, 0.95f, 1f, 0.9f)
                : new Color(1f, 0.45f, 0.2f, 0.9f);
        }

        private void UpdateRallyMarker()
        {
            if (rallyMarker == null)
            {
                return;
            }

            rallyMarker.transform.position = new Vector3(rallyPoint.x, 0.12f, rallyPoint.z);
            rallyMarker.SetActive(hasRallyPoint && IsAlive);
        }

        private void EnsurePhaseVisuals()
        {
            if (phaseAnchor != null)
            {
                return;
            }

            phaseAnchor = new GameObject("Phase Anchor").transform;
            phaseAnchor.SetParent(transform);
            phaseAnchor.localPosition = new Vector3(0f, 1.52f, 0f);
            phaseAnchor.localRotation = Quaternion.identity;
            phaseAnchor.localScale = Vector3.one;

            Transform crown = CreateStatusPrimitive(
                phaseAnchor,
                PrimitiveType.Cylinder,
                "Phase Crown",
                new Vector3(0f, 0.1f, 0f),
                new Vector3(0.26f, 0.08f, 0.26f),
                new Color(0.3f, 0.7f, 0.95f));
            phaseCoreRenderer = crown.GetComponent<Renderer>();

            phaseHalo = CreateStatusPrimitive(
                phaseAnchor,
                PrimitiveType.Cylinder,
                "Phase Halo",
                new Vector3(0f, -0.04f, 0f),
                new Vector3(0.62f, 0.02f, 0.62f),
                new Color(0.36f, 0.84f, 1f, 0.5f));
            phaseHaloRenderer = phaseHalo.GetComponent<Renderer>();

            for (int i = 0; i < phasePillars.Length; i++)
            {
                float angle = i * Mathf.PI * 0.5f;
                Vector3 localPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.46f + new Vector3(0f, 0.32f, 0f);
                phasePillars[i] = CreateStatusPrimitive(
                    phaseAnchor,
                    PrimitiveType.Cube,
                    $"Phase Pillar {i + 1}",
                    localPosition,
                    new Vector3(0.12f, 0.22f, 0.12f),
                    new Color(0.36f, 0.74f, 1f));
                phasePillarRenderers[i] = phasePillars[i].GetComponent<Renderer>();
            }
        }

        private void UpdatePhaseVisuals()
        {
            if (phaseAnchor == null)
            {
                return;
            }

            bool alive = IsAlive;
            phaseAnchor.gameObject.SetActive(alive);
            if (!alive)
            {
                return;
            }

            Color teamColor = Team == UnitTeam.Player ? new Color(0.34f, 0.84f, 1f) : new Color(1f, 0.46f, 0.22f);
            Color phaseColor = Color.Lerp(teamColor, new Color(1f, 0.9f, 0.54f), Mathf.InverseLerp(1f, 4f, currentPhase));
            float pulse = 0.88f + Mathf.PingPong(Time.time * (0.75f + currentPhase * 0.18f), 0.12f);
            float phaseBurst = Mathf.Clamp01(1f - (Time.time - lastPhaseAdvanceTime) / 2.4f);
            float burstScale = 1f + phaseBurst * 0.42f;

            if (phaseCoreRenderer != null)
            {
                phaseCoreRenderer.material.color = phaseColor * pulse;
            }

            if (phaseHalo != null)
            {
                float haloScale = (0.56f + currentPhase * 0.12f) * burstScale;
                phaseHalo.localScale = new Vector3(haloScale, 0.02f + phaseBurst * 0.02f, haloScale);
                phaseHalo.localPosition = new Vector3(0f, -0.04f + phaseBurst * 0.04f, 0f);
            }

            if (phaseHaloRenderer != null)
            {
                Color haloColor = Color.Lerp(teamColor, phaseColor, 0.7f) * (0.42f + phaseBurst * 0.5f);
                phaseHaloRenderer.material.color = haloColor;
            }

            for (int i = 0; i < phasePillars.Length; i++)
            {
                bool active = i < currentPhase;
                if (phasePillars[i] != null)
                {
                    phasePillars[i].gameObject.SetActive(active);
                    float pillarHeight = 0.18f + i * 0.04f + phaseBurst * 0.08f;
                    phasePillars[i].localScale = new Vector3(0.12f + phaseBurst * 0.02f, pillarHeight, 0.12f + phaseBurst * 0.02f);
                }

                if (active && phasePillarRenderers[i] != null)
                {
                    float pillarMix = 0.2f + i * 0.18f;
                    phasePillarRenderers[i].material.color = Color.Lerp(teamColor, phaseColor, pillarMix) * (pulse + phaseBurst * 0.2f);
                }
            }
        }

        private void EnsureAssaultVisuals()
        {
            if (assaultAnchor != null)
            {
                return;
            }

            assaultAnchor = new GameObject("Assault Anchor").transform;
            assaultAnchor.SetParent(transform);
            assaultAnchor.localPosition = new Vector3(0f, 2.34f, 0f);
            assaultAnchor.localRotation = Quaternion.identity;
            assaultAnchor.localScale = Vector3.one;

            assaultMarker = CreateStatusPrimitive(
                assaultAnchor,
                PrimitiveType.Cube,
                "Assault Marker",
                new Vector3(0f, 0.18f, 0f),
                new Vector3(0.18f, 0.36f, 0.18f),
                new Color(1f, 0.42f, 0.24f));
            assaultMarkerRenderer = assaultMarker.GetComponent<Renderer>();
        }

        private void UpdateAssaultVisuals()
        {
            if (assaultAnchor == null)
            {
                return;
            }

            bool alive = IsAlive;
            bool threatenedByPlayer = Team == UnitTeam.Enemy && BattleDirectiveController.CanTargetEnemyBaseStatic(UnitTeam.Player);
            bool threatenedByEnemy = Team == UnitTeam.Player && BattleDirectiveController.CanTargetEnemyBaseStatic(UnitTeam.Enemy);
            bool isObjective = alive && (threatenedByPlayer || threatenedByEnemy);
            assaultAnchor.gameObject.SetActive(isObjective);

            if (!isObjective || assaultMarker == null)
            {
                return;
            }

            Color assaultColor = threatenedByPlayer
                ? new Color(0.34f, 0.92f, 1f)
                : new Color(1f, 0.46f, 0.24f);
            bool enemyAssaultStart = threatenedByEnemy && BattleDirectiveController.Instance != null && BattleDirectiveController.Instance.HasRecentEnemyAssaultStart;
            float pulseSpeed = enemyAssaultStart ? 6.2f : 4.1f;
            float pulseRange = enemyAssaultStart ? 0.34f : 0.22f;
            float pulse = 0.9f + Mathf.PingPong(Time.time * pulseSpeed, pulseRange);
            float verticalPingPong = enemyAssaultStart ? 0.2f : 0.12f;
            float markerWidth = enemyAssaultStart ? 0.24f : 0.18f;
            float markerHeight = enemyAssaultStart ? 0.46f : 0.34f;
            assaultMarker.localPosition = new Vector3(0f, 0.18f + Mathf.PingPong(Time.time * (enemyAssaultStart ? 2.3f : 1.6f), verticalPingPong), 0f);
            assaultMarker.localScale = new Vector3(markerWidth, markerHeight * pulse, markerWidth);

            if (assaultMarkerRenderer != null)
            {
                Color markerColor = enemyAssaultStart
                    ? Color.Lerp(assaultColor, Color.white, 0.2f)
                    : assaultColor;
                assaultMarkerRenderer.material.color = markerColor * pulse;
            }
        }

        private void EnsureThreatVisuals()
        {
            if (threatAnchor != null)
            {
                return;
            }

            threatAnchor = new GameObject("Threat Anchor").transform;
            threatAnchor.SetParent(transform);
            threatAnchor.localPosition = new Vector3(0f, 2.02f, 0f);
            threatAnchor.localRotation = Quaternion.identity;
            threatAnchor.localScale = Vector3.one;

            threatRing = CreateStatusPrimitive(
                threatAnchor,
                PrimitiveType.Cylinder,
                "Threat Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.26f, 0.04f, 0.26f),
                new Color(1f, 0.46f, 0.24f));
            threatRingRenderer = threatRing.GetComponent<Renderer>();

            threatCore = CreateStatusPrimitive(
                threatAnchor,
                PrimitiveType.Sphere,
                "Threat Core",
                new Vector3(0f, 0.16f, 0f),
                new Vector3(0.18f, 0.18f, 0.18f),
                new Color(1f, 0.46f, 0.24f));
            threatCoreRenderer = threatCore.GetComponent<Renderer>();

            threatDirectionRoot = new GameObject("Threat Direction Root").transform;
            threatDirectionRoot.SetParent(threatAnchor);
            threatDirectionRoot.localPosition = new Vector3(0f, 0.06f, 0f);
            threatDirectionRoot.localRotation = Quaternion.identity;
            threatDirectionRoot.localScale = Vector3.one;

            threatDirectionBeam = CreateStatusPrimitive(
                threatDirectionRoot,
                PrimitiveType.Cube,
                "Threat Direction Beam",
                new Vector3(0f, 0f, 0.3f),
                new Vector3(0.04f, 0.04f, 0.6f),
                new Color(1f, 0.46f, 0.24f));
            threatDirectionBeamRenderer = threatDirectionBeam.GetComponent<Renderer>();

            threatDirectionTip = CreateStatusPrimitive(
                threatDirectionRoot,
                PrimitiveType.Cube,
                "Threat Direction Tip",
                new Vector3(0f, 0f, 0.64f),
                new Vector3(0.12f, 0.12f, 0.12f),
                new Color(1f, 0.46f, 0.24f));
            threatDirectionTipRenderer = threatDirectionTip.GetComponent<Renderer>();

            for (int i = 0; i < threatSpikes.Length; i++)
            {
                threatSpikes[i] = CreateStatusPrimitive(
                    threatAnchor,
                    PrimitiveType.Cube,
                    $"Threat Spike {i + 1}",
                    Vector3.zero,
                    new Vector3(0.08f, 0.22f, 0.08f),
                    new Color(1f, 0.46f, 0.24f));
                threatSpikeRenderers[i] = threatSpikes[i].GetComponent<Renderer>();
            }
        }

        private void UpdateThreatVisuals()
        {
            if (threatAnchor == null)
            {
                return;
            }

            int hostileCount = MeasureNearbyThreatState(out float pressure, out Vector3 threatDirection, out int friendlyCount);
            nearbyHostileCount = hostileCount;
            nearbyFriendlyCount = friendlyCount;
            nearbyThreatPressure = pressure;
            nearbyThreatDirection = threatDirection;
            bool showThreat = IsAlive && hostileCount > 0;
            threatAnchor.gameObject.SetActive(showThreat);

            if (!showThreat)
            {
                return;
            }

            bool emergency = IsDefenseEmergency;
            Color threatColor = Team == UnitTeam.Player
                ? (emergency ? new Color(1f, 0.22f, 0.18f) : new Color(1f, 0.46f, 0.24f))
                : new Color(0.34f, 0.92f, 1f);
            float pulse = 0.88f + Mathf.PingPong(Time.time * (2.2f + pressure * 2.8f), 0.14f + pressure * 0.16f);
            float emergencyBoost = emergency ? 0.18f : 0f;
            float ringRadius = 0.24f + pressure * 0.24f + emergencyBoost;
            float coreScale = 0.16f + pressure * 0.16f + emergencyBoost * 0.45f;

            threatAnchor.localPosition = new Vector3(0f, 2.02f + Mathf.PingPong(Time.time * (emergency ? 2.6f : 1.5f), emergency ? 0.18f : 0.1f), 0f);

            if (threatRing != null)
            {
                threatRing.localScale = new Vector3(ringRadius, 0.04f, ringRadius);
            }

            if (threatRingRenderer != null)
            {
                threatRingRenderer.material.color = threatColor * pulse;
            }

            if (threatCore != null)
            {
                threatCore.localScale = Vector3.one * coreScale;
            }

            if (threatCoreRenderer != null)
            {
                threatCoreRenderer.material.color = Color.Lerp(threatColor, Color.white, 0.22f) * pulse;
            }

            int shownSpikes = emergency
                ? threatSpikes.Length
                : Mathf.Clamp(hostileCount, 1, threatSpikes.Length);
            for (int i = 0; i < threatSpikes.Length; i++)
            {
                if (threatSpikes[i] == null)
                {
                    continue;
                }

                bool active = i < shownSpikes;
                threatSpikes[i].gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                float angle = i / (float)threatSpikes.Length * Mathf.PI * 2f + Time.time * 0.35f;
                Vector3 spikePosition = new Vector3(Mathf.Cos(angle), 0.18f + Mathf.PingPong(Time.time * 1.6f + i * 0.4f, 0.08f), Mathf.Sin(angle)) * (0.34f + pressure * 0.12f);
                threatSpikes[i].localPosition = spikePosition;
                threatSpikes[i].localScale = new Vector3(0.08f + emergencyBoost * 0.18f, 0.16f + pressure * 0.14f + emergencyBoost * 0.3f, 0.08f + emergencyBoost * 0.18f);

                if (threatSpikeRenderers[i] != null)
                {
                    threatSpikeRenderers[i].material.color = Color.Lerp(threatColor, Color.white, 0.12f + i * 0.04f) * pulse;
                }
            }

            bool showDirection = threatDirectionRoot != null && threatDirection.sqrMagnitude > 0.0001f;
            if (threatDirectionRoot != null)
            {
                threatDirectionRoot.gameObject.SetActive(showDirection);
            }

            if (!showDirection)
            {
                return;
            }

            float directionLength = 0.48f + pressure * 0.26f + emergencyBoost * 0.9f;
            threatDirectionRoot.localRotation = Quaternion.LookRotation(threatDirection.normalized, Vector3.up);

            if (threatDirectionBeam != null)
            {
                threatDirectionBeam.localPosition = new Vector3(0f, 0f, directionLength * 0.5f);
                threatDirectionBeam.localScale = new Vector3(0.04f, 0.04f, directionLength);
            }

            if (threatDirectionBeamRenderer != null)
            {
                threatDirectionBeamRenderer.material.color = threatColor * pulse;
            }

            if (threatDirectionTip != null)
            {
                threatDirectionTip.localPosition = new Vector3(0f, 0f, directionLength + 0.1f);
                threatDirectionTip.localScale = new Vector3(0.1f + pressure * 0.05f, 0.1f + pressure * 0.03f, 0.1f + pressure * 0.05f);
            }

            if (threatDirectionTipRenderer != null)
            {
                threatDirectionTipRenderer.material.color = Color.Lerp(threatColor, Color.white, 0.22f) * pulse;
            }
        }

        private int MeasureNearbyThreatState(out float pressure, out Vector3 threatDirection, out int friendlyCount)
        {
            pressure = 0f;
            threatDirection = Vector3.zero;
            friendlyCount = 0;
            int hostileCount = 0;
            Vector3 centroid = Vector3.zero;
            float friendlyRadius = threatWarningRadius * 0.9f;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                CombatTarget target = unit.GetComponent<CombatTarget>();
                if (target != null && !target.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (unit.Team == Team)
                {
                    if (distance <= friendlyRadius)
                    {
                        friendlyCount++;
                    }

                    continue;
                }

                if (distance > threatWarningRadius)
                {
                    continue;
                }

                hostileCount++;
                centroid += unit.transform.position;
                pressure = Mathf.Max(pressure, 1f - distance / Mathf.Max(0.01f, threatWarningRadius));
            }

            pressure = Mathf.Clamp01(Mathf.Max(pressure, hostileCount / 6f));
            if (hostileCount > 0)
            {
                Vector3 averagePosition = centroid / hostileCount;
                threatDirection = averagePosition - transform.position;
                threatDirection.y = 0f;
                if (threatDirection.sqrMagnitude <= 0.0001f)
                {
                    threatDirection = Vector3.forward;
                }
            }

            return hostileCount;
        }

        private string GetDefenseUrgencyLabel()
        {
            if (!IsAlive)
            {
                return "Destroyed";
            }

            if (nearbyHostileCount <= 0)
            {
                return CurrentPhase >= 3 ? "Unstable" : "Stable";
            }

            if (IsDefenseEmergency)
            {
                return "Immediate";
            }

            if (CurrentPhase >= 3 || nearbyHostileCount > nearbyFriendlyCount)
            {
                return "Urgent";
            }

            return "Engaged";
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
    }
}

