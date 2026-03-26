using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Spawns enemy reinforcements and acts as a team objective.
    /// </summary>
    public class BaseStructure : MonoBehaviour
    {
        [SerializeField] private float enemyAutoSpawnInterval = 24f;
        [SerializeField] private int enemyMaxUnits = 8;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private Vector3 playerDefaultRallyOffset = new(9f, 0f, 6f);
        [SerializeField] private Vector3 enemyDefaultRallyOffset = new(-9f, 0f, -6f);

        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private PrototypeGameDatabase database;
        private float autoSpawnTimer;
        private float productionSpeedMultiplier = 1f;
        private bool hasRallyPoint;
        private int reinforcementIndex;
        private Vector3 rallyPoint;
        private GameObject rallyMarker;

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public float HealthNormalized => health != null ? health.Normalized : 0f;
        public bool HasQueuedProduction => false;
        public int QueueCount => 0;
        public float ProductionProgressNormalized => 0f;
        public string QueueLabel => Team == UnitTeam.Enemy ? "Auto Reinforce" : "Delegated";
        public string QueuePreview => Team == UnitTeam.Enemy ? "Enemy waves" : "Use Foundry";
        public bool HasRallyPoint => hasRallyPoint;
        public Vector3 RallyPoint => rallyPoint;
        public string RallyLabel => hasRallyPoint ? $"{rallyPoint.x:0.0}, {rallyPoint.z:0.0}" : "Unset";

        private void Awake()
        {
            ApplyVisuals();
            EnsureRallyMarker();
            UpdateRallyMarkerVisuals();
        }

        private void Update()
        {
            UpdateRallyMarker();

            if (!IsAlive || unitRoot == null || database == null)
            {
                return;
            }

            if (Team == UnitTeam.Enemy)
            {
                RunEnemyAutoProduction();
            }
        }

        public void InitializeTarget(CombatTarget assignedTarget, UnitHealth assignedHealth)
        {
            combatTarget = assignedTarget;
            health = assignedHealth;
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

        private void RunEnemyAutoProduction()
        {
            autoSpawnTimer -= Time.deltaTime * productionSpeedMultiplier;

            if (autoSpawnTimer > 0f)
            {
                return;
            }

            if (CountLivingUnitsForTeam() >= enemyMaxUnits)
            {
                autoSpawnTimer = 3f;
                return;
            }

            IReadOnlyList<UnitDefinition> options = database.GetProductionOptions();
            UnitDefinition definition = options.Count == 0 ? null : options[reinforcementIndex % Mathf.Min(3, options.Count)];
            reinforcementIndex++;

            if (definition != null)
            {
                SpawnUnit(definition);
            }

            autoSpawnTimer = enemyAutoSpawnInterval;
        }

        private void SpawnUnit(UnitDefinition definition)
        {
            Vector3 offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y = 1f;
            SelectableUnit unit = PrototypeEntityFactory.CreateUnit(Team, definition, spawnPosition, unitRoot);

            if (unit != null && hasRallyPoint)
            {
                unit.MoveTo(rallyPoint);
            }
        }

        private int CountLivingUnitsForTeam()
        {
            int count = 0;

            foreach (SelectableUnit unit in FindObjectsByType<SelectableUnit>())
            {
                if (unit != null && unit.Team == Team)
                {
                    count++;
                }
            }

            return count;
        }

        private void ApplyVisuals()
        {
            Renderer rendererComponent = GetComponent<Renderer>();

            if (rendererComponent == null)
            {
                return;
            }

            if (combatTarget == null)
            {
                rendererComponent.material.color = Color.gray;
                return;
            }

            rendererComponent.material.color = Team == UnitTeam.Player ? new Color(0.2f, 0.55f, 1f) : new Color(0.9f, 0.25f, 0.2f);
        }

        private void EnsureRallyMarker()
        {
            if (rallyMarker != null)
            {
                return;
            }

            rallyMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rallyMarker.name = $"{name} Rally Marker";
            rallyMarker.transform.localScale = new Vector3(0.65f, 0.04f, 0.65f);
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
