using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Dedicated production building for player reinforcements.
    /// </summary>
    public class ProductionStructure : MonoBehaviour
    {
        [SerializeField] private int maxUnits = 1000;
        [SerializeField] private int maxQueueLength = 48;
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private Vector3 defaultRallyOffset = new(8f, 0f, 4f);

        private readonly List<UnitDefinition> productionQueue = new();
        private readonly List<UnitArchetype> allowedArchetypes = new();
        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private PrototypeGameDatabase database;
        private BaseStructure linkedBase;
        private float productionTimer;
        private bool isProducing;
        private bool hasRallyPoint;
        private float productionSpeedMultiplier = 1f;
        private float lastQueueCommandTime;
        private UnitDefinition currentProductionDefinition;
        private int rallyDispatchSequence;
        private Vector3 rallyPoint;
        private GameObject rallyMarker;
        private string structureLabel = "Foundry";

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public bool HasQueuedProduction => productionQueue.Count > 0 || isProducing;
        public int QueueCount => productionQueue.Count + (isProducing ? 1 : 0);
        public float LastQueueCommandTime => lastQueueCommandTime;
        public bool HasRallyPoint => hasRallyPoint;
        public Vector3 RallyPoint => rallyPoint;
        public float ProductionProgressNormalized => isProducing && currentProductionDefinition != null
            ? Mathf.Clamp01(1f - (productionTimer / Mathf.Max(0.01f, currentProductionDefinition.ProductionDuration)))
            : 0f;
        public string QueueLabel => isProducing && currentProductionDefinition != null ? currentProductionDefinition.DisplayName : "Idle";
        public string QueuePreview => BuildQueuePreview();
        public string RallyLabel => hasRallyPoint ? $"{rallyPoint.x:0.0}, {rallyPoint.z:0.0}" : "Unset";
        public float SpeedMultiplier => productionSpeedMultiplier;
        public string StructureLabel => structureLabel;
        public string AllowedUnitsLabel => BuildAllowedUnitsLabel();

        private void Awake()
        {
            EnsureRallyMarker();
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

            if (!IsAlive || unitRoot == null || database == null || Team != UnitTeam.Player)
            {
                return;
            }

            if (linkedBase != null && !linkedBase.IsAlive)
            {
                return;
            }

            RunProduction();
        }

        public void Initialize(CombatTarget assignedTarget, UnitHealth assignedHealth, Transform assignedUnitRoot, PrototypeGameDatabase assignedDatabase, BaseStructure assignedLinkedBase)
        {
            combatTarget = assignedTarget;
            health = assignedHealth;
            unitRoot = assignedUnitRoot;
            database = assignedDatabase;
            linkedBase = assignedLinkedBase;
            ApplyVisuals();
            SetRallyPoint(transform.position + defaultRallyOffset);
        }

        public void ConfigureStructure(string newLabel, Vector3 rallyOffset, Color color, params UnitArchetype[] newAllowedArchetypes)
        {
            structureLabel = newLabel;
            defaultRallyOffset = rallyOffset;
            allowedArchetypes.Clear();
            allowedArchetypes.AddRange(newAllowedArchetypes);

            Renderer rendererComponent = GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            if (rallyMarker != null)
            {
                rallyMarker.GetComponent<Renderer>().material.color = color;
            }
        }

        public bool TryQueueProduction(UnitDefinition definition)
        {
            int reservedTotal = productionQueue.Count + (isProducing ? 1 : 0);

            if (!IsAlive || Team != UnitTeam.Player || definition == null || !CanProduce(definition.Archetype) || productionQueue.Count >= maxQueueLength)
            {
                return false;
            }

            if (!PrototypeProductionRules.CanReserveUnit(Team, definition, reservedTotal, CountQueued(definition.Archetype), maxUnits))
            {
                return false;
            }

            productionQueue.Add(definition);
            lastQueueCommandTime = Time.unscaledTime;

            if (!isProducing)
            {
                BeginNextProduction();
            }

            return true;
        }

        public bool TryCancelLastQueuedProduction()
        {
            if (productionQueue.Count == 0)
            {
                return false;
            }

            productionQueue.RemoveAt(productionQueue.Count - 1);
            lastQueueCommandTime = Time.unscaledTime;
            return true;
        }

        public bool CanProduce(UnitArchetype archetype)
        {
            return allowedArchetypes.Count == 0 || allowedArchetypes.Contains(archetype);
        }

        public void SetRallyPoint(Vector3 worldPoint)
        {
            rallyPoint = new Vector3(worldPoint.x, 1f, worldPoint.z);
            hasRallyPoint = true;
            UpdateRallyMarker();
        }

        public void SetProductionSpeedMultiplier(float multiplier)
        {
            productionSpeedMultiplier = Mathf.Max(0.5f, multiplier);
        }

        private void RunProduction()
        {
            if (!isProducing)
            {
                if (productionQueue.Count > 0)
                {
                    BeginNextProduction();
                }

                return;
            }

            productionTimer -= Time.deltaTime * productionSpeedMultiplier;
            if (productionTimer > 0f)
            {
                return;
            }

            SpawnUnit(currentProductionDefinition);
            isProducing = false;
            currentProductionDefinition = null;

            if (productionQueue.Count > 0)
            {
                BeginNextProduction();
            }
        }

        private void BeginNextProduction()
        {
            if (productionQueue.Count == 0)
            {
                isProducing = false;
                currentProductionDefinition = null;
                return;
            }

            currentProductionDefinition = productionQueue[0];
            productionQueue.RemoveAt(0);
            productionTimer = currentProductionDefinition.ProductionDuration;
            isProducing = true;
        }

        private void SpawnUnit(UnitDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            Vector3 offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y = definition.IsFlying ? definition.HoverHeight : 1f;
            SelectableUnit unit = PrototypeEntityFactory.CreateUnit(Team, definition, spawnPosition, unitRoot);

            if (unit != null && hasRallyPoint)
            {
                unit.MoveTo(GetRallyDestination());
            }
        }

        private Vector3 GetRallyDestination()
        {
            rallyDispatchSequence++;

            if (rallyDispatchSequence <= 1)
            {
                return rallyPoint;
            }

            const float goldenAngle = 2.39996323f;
            float radius = Mathf.Min(spawnRadius * 1.8f, 8f + Mathf.Sqrt(rallyDispatchSequence) * 1.9f);
            float angle = rallyDispatchSequence * goldenAngle;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            return rallyPoint + offset;
        }

        private int CountQueued(UnitArchetype archetype)
        {
            int count = 0;

            if (isProducing && currentProductionDefinition != null && currentProductionDefinition.Archetype == archetype)
            {
                count++;
            }

            foreach (UnitDefinition definition in productionQueue)
            {
                if (definition != null && definition.Archetype == archetype)
                {
                    count++;
                }
            }

            return count;
        }

        private string BuildQueuePreview()
        {
            if (!isProducing && productionQueue.Count == 0)
            {
                return "Idle";
            }

            List<string> labels = new();
            if (isProducing && currentProductionDefinition != null)
            {
                labels.Add($"> {currentProductionDefinition.DisplayName}");
            }

            foreach (UnitDefinition definition in productionQueue)
            {
                if (definition != null)
                {
                    labels.Add(definition.DisplayName);
                }
            }

            return string.Join(", ", labels);
        }

        private string BuildAllowedUnitsLabel()
        {
            if (allowedArchetypes.Count == 0)
            {
                return "All";
            }

            if (database == null)
            {
                return string.Join(", ", allowedArchetypes);
            }

            List<string> labels = new();
            foreach (UnitArchetype archetype in allowedArchetypes)
            {
                UnitDefinition definition = database.GetDefinition(archetype);
                labels.Add(definition != null ? definition.DisplayName : archetype.ToString());
            }

            return string.Join(", ", labels);
        }

        private void ApplyVisuals()
        {
            Renderer rendererComponent = GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = new Color(0.32f, 0.78f, 1f);
            }
        }

        private void EnsureRallyMarker()
        {
            if (rallyMarker != null)
            {
                return;
            }

            rallyMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rallyMarker.name = $"{name} Rally Marker";
            rallyMarker.transform.localScale = new Vector3(1.12f, 0.03f, 1.12f);
            rallyMarker.GetComponent<Collider>().enabled = false;
            rallyMarker.GetComponent<Renderer>().material.color = new Color(0.25f, 1f, 0.85f, 0.9f);
            rallyMarker.SetActive(false);
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
