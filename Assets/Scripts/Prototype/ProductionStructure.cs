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
        [SerializeField] private int maxUnits = 28;
        [SerializeField] private int maxQueueLength = 10;
        [SerializeField] private float spawnRadius = 4.5f;
        [SerializeField] private Vector3 defaultRallyOffset = new(8f, 0f, 4f);

        private readonly List<UnitDefinition> productionQueue = new();
        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private PrototypeGameDatabase database;
        private BaseStructure linkedBase;
        private float productionTimer;
        private bool isProducing;
        private bool hasRallyPoint;
        private float productionSpeedMultiplier = 1f;
        private UnitDefinition currentProductionDefinition;
        private Vector3 rallyPoint;
        private GameObject rallyMarker;

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public bool HasQueuedProduction => productionQueue.Count > 0 || isProducing;
        public float ProductionProgressNormalized => isProducing && currentProductionDefinition != null
            ? Mathf.Clamp01(1f - (productionTimer / Mathf.Max(0.01f, currentProductionDefinition.ProductionDuration)))
            : 0f;
        public string QueueLabel => isProducing && currentProductionDefinition != null ? currentProductionDefinition.DisplayName : "Idle";
        public string QueuePreview => BuildQueuePreview();
        public string RallyLabel => hasRallyPoint ? $"{rallyPoint.x:0.0}, {rallyPoint.z:0.0}" : "Unset";
        public float SpeedMultiplier => productionSpeedMultiplier;

        private void Awake()
        {
            EnsureRallyMarker();
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

        public bool TryQueueProduction(UnitDefinition definition)
        {
            int reservedSlots = PrototypeRuntimeQuery.CountUnits(Team) + productionQueue.Count + (isProducing ? 1 : 0);

            if (!IsAlive || Team != UnitTeam.Player || definition == null || productionQueue.Count >= maxQueueLength || reservedSlots >= maxUnits)
            {
                return false;
            }

            productionQueue.Add(definition);

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
            return true;
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
            spawnPosition.y = 1f;
            SelectableUnit unit = PrototypeEntityFactory.CreateUnit(Team, definition, spawnPosition, unitRoot);

            if (unit != null && hasRallyPoint)
            {
                unit.MoveTo(rallyPoint);
            }
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
            rallyMarker.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
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
