using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Spawns reinforcements for each team and acts as a team objective.
    /// </summary>
    public class BaseStructure : MonoBehaviour
    {
        [SerializeField] private float autoSpawnInterval = 12f;
        [SerializeField] private float playerProductionTime = 6f;
        [SerializeField] private int maxTeamUnits = 14;
        [SerializeField] private float spawnRadius = 5f;

        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private float autoSpawnTimer;
        private float productionTimer;
        private bool hasQueuedProduction;
        private UnitArchetype queuedArchetype;
        private int reinforcementIndex;

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public float HealthNormalized => health != null ? health.Normalized : 0f;
        public bool HasQueuedProduction => hasQueuedProduction;
        public float ProductionProgressNormalized => hasQueuedProduction ? Mathf.Clamp01(1f - (productionTimer / playerProductionTime)) : 0f;
        public string QueueLabel => hasQueuedProduction ? queuedArchetype.ToString() : "Idle";

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
            autoSpawnTimer = autoSpawnInterval;
            ApplyVisuals();
        }

        public bool TryQueueProduction(UnitArchetype archetype)
        {
            if (!IsAlive || Team != UnitTeam.Player || hasQueuedProduction || CountLivingUnitsForTeam() >= maxTeamUnits)
            {
                return false;
            }

            queuedArchetype = archetype;
            hasQueuedProduction = true;
            productionTimer = playerProductionTime + (archetype == UnitArchetype.Skirmisher ? 1f : 0f);
            return true;
        }

        private void RunEnemyAutoProduction()
        {
            autoSpawnTimer -= Time.deltaTime;

            if (autoSpawnTimer > 0f)
            {
                return;
            }

            if (CountLivingUnitsForTeam() >= maxTeamUnits)
            {
                autoSpawnTimer = 1.5f;
                return;
            }

            UnitArchetype archetype = reinforcementIndex++ % 2 == 0 ? UnitArchetype.Vanguard : UnitArchetype.Skirmisher;
            SpawnUnit(archetype);
            autoSpawnTimer = autoSpawnInterval;
        }

        private void RunPlayerProduction()
        {
            if (!hasQueuedProduction)
            {
                return;
            }

            productionTimer -= Time.deltaTime;

            if (productionTimer > 0f)
            {
                return;
            }

            if (CountLivingUnitsForTeam() < maxTeamUnits)
            {
                SpawnUnit(queuedArchetype);
            }

            hasQueuedProduction = false;
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

            foreach (SelectableUnit unit in FindObjectsByType<SelectableUnit>(FindObjectsSortMode.None))
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
    }
}
