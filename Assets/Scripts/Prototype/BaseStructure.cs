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
        [SerializeField] private float enemyAutoSpawnInterval = 12.5f;
        [SerializeField] private int enemyMaxUnits = 1000;
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private Vector3 playerDefaultRallyOffset = new(34f, 0f, 18f);
        [SerializeField] private Vector3 enemyDefaultRallyOffset = new(-28f, 0f, -16f);

        private readonly UnitArchetype[] enemyReinforcementCycle =
        {
            UnitArchetype.Rifleman,
            UnitArchetype.Rifleman,
            UnitArchetype.Spearman,
            UnitArchetype.Rifleman,
            UnitArchetype.ShieldInfantry,
            UnitArchetype.Artillery
        };

        private readonly float[] phaseThresholds = { 0.75f, 0.5f, 0.25f };
        private readonly float[] phaseScaleMultipliers = { 1f, 0.94f, 0.87f, 0.8f };
        private readonly float[] phaseProductionMultipliers = { 1f, 1.08f, 1.16f, 1.28f };

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
        public string PhaseLabel => currentPhase switch
        {
            1 => "Bulwark",
            2 => "Breach",
            3 => "Last Rite",
            _ => "Collapse"
        };

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            baseScale = transform.localScale;
            ApplyVisuals();
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();
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
        }

        public void Initialize(Transform assignedUnitRoot, PrototypeGameDatabase assignedDatabase)
        {
            unitRoot = assignedUnitRoot;
            database = assignedDatabase;
            autoSpawnTimer = enemyAutoSpawnInterval;
            ApplyVisuals();
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();

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
        }

        public void SetProductionSpeedMultiplier(float multiplier)
        {
            productionSpeedMultiplier = Mathf.Max(0.5f, multiplier);
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
            ApplyVisuals();
            SpawnPhaseReinforcements();

            string teamLabel = Team == UnitTeam.Player ? "Imperial Sanctum" : "Enemy Bastion";
            BattleDirectiveController.BroadcastNewsStatic($"War News: {teamLabel} shifted to phase {currentPhase} - {PhaseLabel}.");
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
            autoSpawnTimer = Mathf.Max(5.5f, enemyAutoSpawnInterval - (currentPhase - 1) * 1.4f);
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
            if (Team == UnitTeam.Player)
            {
                return currentPhase switch
                {
                    2 => new[]
                    {
                        UnitArchetype.ShieldInfantry, UnitArchetype.ShieldInfantry, UnitArchetype.ShieldInfantry,
                        UnitArchetype.Spearman, UnitArchetype.Spearman, UnitArchetype.Spearman,
                        UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.SpecialWarrior
                    },
                    3 => new[]
                    {
                        UnitArchetype.ShieldInfantry, UnitArchetype.ShieldInfantry, UnitArchetype.Spearman, UnitArchetype.Spearman,
                        UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.SpecialWarrior, UnitArchetype.SpecialWarrior,
                        UnitArchetype.Artillery, UnitArchetype.RoyalGuard
                    },
                    4 => new[]
                    {
                        UnitArchetype.RoyalGuard, UnitArchetype.RoyalGuard, UnitArchetype.SpecialWarrior, UnitArchetype.SpecialWarrior,
                        UnitArchetype.ShieldInfantry, UnitArchetype.ShieldInfantry, UnitArchetype.Rifleman, UnitArchetype.Rifleman,
                        UnitArchetype.Artillery, UnitArchetype.MobileFortress, UnitArchetype.AirborneCitadel
                    },
                    _ => System.Array.Empty<UnitArchetype>()
                };
            }

            return currentPhase switch
            {
                2 => new[]
                {
                    UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Rifleman,
                    UnitArchetype.Spearman, UnitArchetype.Spearman, UnitArchetype.Fighter, UnitArchetype.Fighter
                },
                3 => new[]
                {
                    UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Rifleman,
                    UnitArchetype.SpecialWarrior, UnitArchetype.SpecialWarrior,
                    UnitArchetype.Artillery, UnitArchetype.Fighter, UnitArchetype.Fighter
                },
                4 => new[]
                {
                    UnitArchetype.SpecialWarrior, UnitArchetype.SpecialWarrior, UnitArchetype.SpecialWarrior,
                    UnitArchetype.Rifleman, UnitArchetype.Rifleman, UnitArchetype.Artillery, UnitArchetype.Artillery,
                    UnitArchetype.MobileFortress, UnitArchetype.Fighter, UnitArchetype.Fighter
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
    }
}
