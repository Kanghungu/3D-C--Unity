using Game.CameraSystem;
using Game.Selection;
using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Creates a more complete playable RTS prototype scene at runtime.
    /// Add this to an empty GameObject in the scene and press Play.
    /// </summary>
    public class PrototypeBootstrapper : MonoBehaviour
    {
        [Header("Ground")]
        [SerializeField] private Vector3 groundScale = new(10f, 1f, 10f);
        [SerializeField] private Vector3 groundPosition = Vector3.zero;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraPosition = new(0f, 28f, -24f);
        [SerializeField] private Vector3 cameraRotation = new(50f, 0f, 0f);

        [Header("Army Setup")]
        [SerializeField] private Vector3 friendlyStart = new(-12f, 1f, -5f);
        [SerializeField] private Vector2Int friendlyGrid = new(4, 2);
        [SerializeField] private Vector3 enemyStart = new(9f, 1f, 7f);
        [SerializeField] private Vector2Int enemyGrid = new(2, 2);
        [SerializeField] private float unitSpacing = 3f;

        [Header("Bases")]
        [SerializeField] private Vector3 playerBasePosition = new(-18f, 1f, -14f);
        [SerializeField] private Vector3 enemyBasePosition = new(18f, 1f, 14f);
        [SerializeField] private Vector3 playerFoundryPosition = new(-11f, 1f, -16f);

        private Transform playerUnitRoot;
        private Transform enemyUnitRoot;
        private PrototypeGameDatabase database;

        private void Awake()
        {
            SetupGround();
            SetupCamera();
            SetupRoots();
            SetupDatabase();
            SetupTerrainFeatures();
            SetupPrototypeSystems();
            SetupBases();
            SetupUnits();
        }

        private void SetupGround()
        {
            GameObject ground = GameObject.Find("Ground");

            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
            }

            ground.transform.SetPositionAndRotation(groundPosition, Quaternion.identity);
            ground.transform.localScale = groundScale;
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObject = new("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            Transform cameraTransform = mainCamera.transform;
            cameraTransform.position = cameraPosition;
            cameraTransform.rotation = Quaternion.Euler(cameraRotation);

            if (cameraTransform.GetComponent<RTSCameraController>() == null)
            {
                cameraTransform.gameObject.AddComponent<RTSCameraController>();
            }
        }

        private void SetupRoots()
        {
            playerUnitRoot = GetOrCreateRoot("Friendly Units");
            enemyUnitRoot = GetOrCreateRoot("Enemy Units");
            GetOrCreateRoot("Structures");
            GetOrCreateRoot("Terrain Features");
        }

        private void SetupDatabase()
        {
            database = FindAnyObjectByType<PrototypeGameDatabase>();

            if (database == null)
            {
                database = gameObject.AddComponent<PrototypeGameDatabase>();
            }
        }

        private void SetupTerrainFeatures()
        {
            Transform terrainRoot = GetOrCreateRoot("Terrain Features");

            if (terrainRoot.childCount > 0)
            {
                return;
            }

            CreateTerrainBlock("North Ridge", new Vector3(0f, 0.75f, 14f), new Vector3(9f, 1.5f, 2f), terrainRoot, new Color(0.28f, 0.28f, 0.32f));
            CreateTerrainBlock("South Ridge", new Vector3(0f, 0.75f, -14f), new Vector3(9f, 1.5f, 2f), terrainRoot, new Color(0.28f, 0.28f, 0.32f));
            CreateTerrainBlock("West Cover", new Vector3(-13f, 1f, 1f), new Vector3(2.2f, 2f, 2.2f), terrainRoot, new Color(0.24f, 0.24f, 0.28f));
            CreateTerrainBlock("East Cover", new Vector3(13f, 1f, -1f), new Vector3(2.2f, 2f, 2.2f), terrainRoot, new Color(0.24f, 0.24f, 0.28f));
            CreateTerrainBlock("Mid Cover A", new Vector3(-4f, 0.8f, 4f), new Vector3(3f, 1.6f, 1.8f), terrainRoot, new Color(0.3f, 0.32f, 0.36f));
            CreateTerrainBlock("Mid Cover B", new Vector3(5f, 0.8f, -5f), new Vector3(3f, 1.6f, 1.8f), terrainRoot, new Color(0.3f, 0.32f, 0.36f));
            CreateTerrainBlock("Central Battery", new Vector3(0.5f, 0.9f, 0.5f), new Vector3(2.4f, 1.8f, 2.4f), terrainRoot, new Color(0.34f, 0.34f, 0.38f));
        }

        private void SetupPrototypeSystems()
        {
            if (FindAnyObjectByType<PrototypeSelectionController>() == null)
            {
                gameObject.AddComponent<PrototypeSelectionController>();
            }

            if (FindAnyObjectByType<EnemyAIController>() == null)
            {
                gameObject.AddComponent<EnemyAIController>();
            }

            if (FindAnyObjectByType<PrototypeHUD>() == null)
            {
                gameObject.AddComponent<PrototypeHUD>();
            }

            if (FindAnyObjectByType<PlayerProductionController>() == null)
            {
                gameObject.AddComponent<PlayerProductionController>();
            }

            if (FindAnyObjectByType<PrototypeMatchController>() == null)
            {
                gameObject.AddComponent<PrototypeMatchController>();
            }
        }

        private void SetupBases()
        {
            if (FindObjectsByType<BaseStructure>().Length > 0)
            {
                return;
            }

            Transform structuresRoot = GetOrCreateRoot("Structures");
            BaseStructure playerBase = PrototypeEntityFactory.CreateBase("Player Base", playerBasePosition, UnitTeam.Player, structuresRoot);
            BaseStructure enemyBase = PrototypeEntityFactory.CreateBase("Enemy Base", enemyBasePosition, UnitTeam.Enemy, structuresRoot);
            ProductionStructure playerFoundry = PrototypeEntityFactory.CreateProductionStructure("Player Foundry", playerFoundryPosition, UnitTeam.Player, structuresRoot);

            PrototypeEntityFactory.CreateTurret("Player Turret Left", new Vector3(-12f, 1f, -10f), UnitTeam.Player, structuresRoot);
            PrototypeEntityFactory.CreateTurret("Player Turret Right", new Vector3(-21f, 1f, -6f), UnitTeam.Player, structuresRoot);
            PrototypeEntityFactory.CreateTurret("Enemy Turret Left", new Vector3(12f, 1f, 10f), UnitTeam.Enemy, structuresRoot);
            PrototypeEntityFactory.CreateTurret("Enemy Turret Right", new Vector3(21f, 1f, 6f), UnitTeam.Enemy, structuresRoot);
            PrototypeEntityFactory.CreateControlNode("Central Control Node", new Vector3(0f, 0.35f, 0f), structuresRoot);

            playerBase.Initialize(playerUnitRoot, database);
            enemyBase.Initialize(enemyUnitRoot, database);

            CombatTarget foundryTarget = playerFoundry.GetComponent<CombatTarget>();
            UnitHealth foundryHealth = playerFoundry.GetComponent<UnitHealth>();
            playerFoundry.Initialize(foundryTarget, foundryHealth, playerUnitRoot, database, playerBase);
        }

        private void SetupUnits()
        {
            if (FindAnyObjectByType<SelectableUnit>() != null)
            {
                return;
            }

            CreateFormation(friendlyStart, friendlyGrid, UnitTeam.Player, playerUnitRoot);
            CreateFormation(enemyStart, enemyGrid, UnitTeam.Enemy, enemyUnitRoot);
            CreateSingleUnit(new Vector3(-16f, 1f, -8f), UnitTeam.Player, playerUnitRoot, UnitArchetype.Artillery);
            CreateSingleUnit(new Vector3(-13f, 1f, -10f), UnitTeam.Player, playerUnitRoot, UnitArchetype.Skirmisher);
            CreateSingleUnit(new Vector3(15f, 1f, 10f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Artillery);
        }

        private void CreateFormation(Vector3 origin, Vector2Int grid, UnitTeam team, Transform parent)
        {
            for (int row = 0; row < grid.y; row++)
            {
                for (int column = 0; column < grid.x; column++)
                {
                    Vector3 spawnPosition = origin + new Vector3(column * unitSpacing, 0f, row * unitSpacing);
                    UnitArchetype archetype = row == 0 ? UnitArchetype.Vanguard : UnitArchetype.Skirmisher;
                    CreateSingleUnit(spawnPosition, team, parent, archetype);
                }
            }
        }

        private void CreateSingleUnit(Vector3 position, UnitTeam team, Transform parent, UnitArchetype archetype)
        {
            UnitDefinition definition = database.GetDefinition(archetype);

            if (definition != null)
            {
                PrototypeEntityFactory.CreateUnit(team, definition, position, parent);
            }
        }

        private static void CreateTerrainBlock(string name, Vector3 position, Vector3 scale, Transform parent, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.transform.SetParent(parent);
            block.GetComponent<Renderer>().material.color = color;
        }

        private static Transform GetOrCreateRoot(string rootName)
        {
            GameObject root = GameObject.Find(rootName);
            return root != null ? root.transform : new GameObject(rootName).transform;
        }
    }
}
