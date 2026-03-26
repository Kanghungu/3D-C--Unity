using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Spawns reinforcements for each team and acts as a team objective.
    /// </summary>
    public class BaseStructure : MonoBehaviour
    {
        [SerializeField] private float enemyAutoSpawnInterval = 24f;
        [SerializeField] private float playerProductionTime = 3.5f;
        [SerializeField] private int playerMaxUnits = 24;
        [SerializeField] private int enemyMaxUnits = 8;
        [SerializeField] private int maxQueueLength = 6;
        [SerializeField] private float spawnRadius = 5f;

        private readonly List<UnitArchetype> productionQueue = new();
        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private float autoSpawnTimer;
        private float productionTimer;
        private bool isProducing;
        private UnitArchetype currentProductionArchetype;
        private int reinforcementIndex;

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public float HealthNormalized => health != null ? health.Normalized : 0f;
        public bool HasQueuedProduction => productionQueue.Count > 0 || isProducing;
        public int QueueCount => productionQueue.Count + (isProducing ? 1 : 0);
        public float ProductionProgressNormalized => isProducing ? Mathf.Clamp01(1f - (productionTimer / CurrentProductionDuration)) : 0f;
        public string QueueLabel => isProducing ? currentProductionArchetype.ToString() : "Idle";
        public string QueuePreview => BuildQueuePreview();

        private float CurrentProductionDuration => playerProductionTime + (currentProductionArchetype == UnitArchetype.Skirmisher ? 0.6f : 0f);
        private int CurrentMaxUnits => Team == UnitTeam.Player ? playerMaxUnits : enemyMaxUnits;

        private void Awake()
        {
            ApplyVisuals();
        }

        private void Update()
        {
            if (!IsAlive || unitRoot == null)
            {
                return;
            }

            if (Team == UnitTeam.Enemy)
            {
                RunEnemyAutoProduction();
                return;
            }

            RunPlayerProduction();
        }

        public void InitializeTarget(CombatTarget assignedTarget, UnitHealth assignedHealth)
        {
            combatTarget = assignedTarget;
            health = assignedHealth;
            ApplyVisuals();
        }

        public void Initialize(Transform assignedUnitRoot)
        {
            unitRoot = assignedUnitRoot;
            autoSpawnTimer = enemyAutoSpawnInterval;
            ApplyVisuals();
        }

        public bool TryQueueProduction(UnitArchetype archetype)
        {
            int reservedSlots = CountLivingUnitsForTeam() + productionQueue.Count + (isProducing ? 1 : 0);

            if (!IsAlive || Team != UnitTeam.Player || productionQueue.Count >= maxQueueLength || reservedSlots >= CurrentMaxUnits)
            {
                return false;
            }

            productionQueue.Add(archetype);

            if (!isProducing)
            {
                BeginNextProduction();
            }

            return true;
        }

        public bool TryCancelLastQueuedProduction()
        {
            if (Team != UnitTeam.Player || productionQueue.Count == 0)
            {
                return false;
            }

            productionQueue.RemoveAt(productionQueue.Count - 1);
            return true;
        }

        private void RunEnemyAutoProduction()
        {
            autoSpawnTimer -= Time.deltaTime;

            if (autoSpawnTimer > 0f)
            {
                return;
            }

            if (CountLivingUnitsForTeam() >= CurrentMaxUnits)
            {
                autoSpawnTimer = 3f;
                return;
            }

            UnitArchetype archetype = reinforcementIndex % 4 == 0 ? UnitArchetype.Skirmisher : UnitArchetype.Vanguard;
            reinforcementIndex++;
            SpawnUnit(archetype);
            autoSpawnTimer = enemyAutoSpawnInterval;
        }

        private void RunPlayerProduction()
        {
            if (!isProducing)
            {
                if (productionQueue.Count > 0)
                {
                    BeginNextProduction();
                }

                return;
            }

            productionTimer -= Time.deltaTime;

            if (productionTimer > 0f)
            {
                return;
            }

            if (CountLivingUnitsForTeam() < CurrentMaxUnits)
            {
                SpawnUnit(currentProductionArchetype);
            }

            isProducing = false;

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
                return;
            }

            currentProductionArchetype = productionQueue[0];
            productionQueue.RemoveAt(0);
            isProducing = true;
            productionTimer = CurrentProductionDuration;
        }

        private void SpawnUnit(UnitArchetype archetype)
        {
            Vector3 offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y = 1f;
            PrototypeEntityFactory.CreateUnit(Team, archetype, spawnPosition, unitRoot);
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

        private string BuildQueuePreview()
        {
            if (!isProducing && productionQueue.Count == 0)
            {
                return "Idle";
            }

            List<string> labels = new();

            if (isProducing)
            {
                labels.Add($"> {currentProductionArchetype}");
            }

            foreach (UnitArchetype archetype in productionQueue)
            {
                labels.Add(archetype.ToString());
            }

            return string.Join(", ", labels);
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
    }
}
