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
        [SerializeField] private Vector3 friendlyStart = new(-15f, 1f, -7f);
        [SerializeField] private Vector2Int friendlyGrid = new(3, 3);
        [SerializeField] private Vector3 enemyStart = new(8f, 1f, 8f);
        [SerializeField] private Vector2Int enemyGrid = new(3, 2);
        [SerializeField] private float unitSpacing = 3.1f;

        [Header("Bases")]
        [SerializeField] private Vector3 playerBasePosition = new(-20f, 1f, -15f);
        [SerializeField] private Vector3 enemyBasePosition = new(20f, 1f, 15f);
        [SerializeField] private Vector3 playerFoundryPosition = new(-12f, 1f, -17f);

        private readonly Color sandColor = new(0.76f, 0.64f, 0.42f);
        private readonly Color duneColor = new(0.68f, 0.55f, 0.34f);
        private readonly Color stoneColor = new(0.62f, 0.56f, 0.46f);
        private readonly Color altarColor = new(0.82f, 0.72f, 0.48f);
        private readonly Color ruinColor = new(0.48f, 0.42f, 0.36f);

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
            ground.GetComponent<Renderer>().material.color = sandColor;

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.78f, 0.62f, 0.42f);
            RenderSettings.fogDensity = 0.0025f;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.52f, 0.38f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.3f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.14f, 0.1f);
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
            mainCamera.backgroundColor = new Color(0.87f, 0.73f, 0.48f);

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

            CreateTerrainBlock("Dune Ridge North", new Vector3(0f, 0.5f, 14f), new Vector3(11f, 1f, 3f), terrainRoot, duneColor);
            CreateTerrainBlock("Dune Ridge South", new Vector3(0f, 0.5f, -14f), new Vector3(11f, 1f, 3f), terrainRoot, duneColor);
            CreateTerrainBlock("Blessed Causeway", new Vector3(0f, 0.15f, 0f), new Vector3(8f, 0.3f, 16f), terrainRoot, stoneColor);
            CreateTerrainBlock("Ruined Wall West", new Vector3(-11f, 1.1f, 3f), new Vector3(2.3f, 2.2f, 6f), terrainRoot, ruinColor);
            CreateTerrainBlock("Ruined Wall East", new Vector3(11f, 1.1f, -3f), new Vector3(2.3f, 2.2f, 6f), terrainRoot, ruinColor);
            CreateTerrainBlock("Collapsed Shrine A", new Vector3(-4f, 0.8f, 5f), new Vector3(3.5f, 1.6f, 2.2f), terrainRoot, stoneColor);
            CreateTerrainBlock("Collapsed Shrine B", new Vector3(5.5f, 0.8f, -5.5f), new Vector3(3.5f, 1.6f, 2.2f), terrainRoot, stoneColor);
            CreateTerrainBlock("Central Altar Base", new Vector3(0f, 0.8f, 0f), new Vector3(4.8f, 1.6f, 4.8f), terrainRoot, altarColor);
            CreateTerrainBlock("Central Altar Step", new Vector3(0f, 1.75f, 0f), new Vector3(2.8f, 0.8f, 2.8f), terrainRoot, stoneColor);
            CreateTerrainPillar("Altar Pillar NW", new Vector3(-2.6f, 2f, 2.6f), terrainRoot, altarColor);
            CreateTerrainPillar("Altar Pillar NE", new Vector3(2.6f, 2f, 2.6f), terrainRoot, altarColor);
            CreateTerrainPillar("Altar Pillar SW", new Vector3(-2.6f, 2f, -2.6f), terrainRoot, altarColor);
            CreateTerrainPillar("Altar Pillar SE", new Vector3(2.6f, 2f, -2.6f), terrainRoot, altarColor);
            CreateBanner("Player Sanctum Banner", new Vector3(-18f, 2.6f, -11f), new Color(0.78f, 0.8f, 0.92f), terrainRoot);
            CreateBanner("Enemy War Banner", new Vector3(18f, 2.6f, 11f), new Color(0.92f, 0.42f, 0.24f), terrainRoot);
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

            if (FindAnyObjectByType<PlayerAbilityController>() == null)
            {
                gameObject.AddComponent<PlayerAbilityController>();
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
            BaseStructure playerBase = PrototypeEntityFactory.CreateBase("Player Sanctum", playerBasePosition, UnitTeam.Player, structuresRoot);
            BaseStructure enemyBase = PrototypeEntityFactory.CreateBase("Enemy Bastion", enemyBasePosition, UnitTeam.Enemy, structuresRoot);
            ProductionStructure playerFoundry = PrototypeEntityFactory.CreateProductionStructure("Holy Foundry", playerFoundryPosition, UnitTeam.Player, structuresRoot);

            PrototypeEntityFactory.CreateTurret("Player Sentinel Left", new Vector3(-13f, 1f, -11f), UnitTeam.Player, structuresRoot);
            PrototypeEntityFactory.CreateTurret("Player Sentinel Right", new Vector3(-22f, 1f, -7f), UnitTeam.Player, structuresRoot);
            PrototypeEntityFactory.CreateTurret("Enemy Cannon Left", new Vector3(12f, 1f, 11f), UnitTeam.Enemy, structuresRoot);
            PrototypeEntityFactory.CreateTurret("Enemy Cannon Right", new Vector3(22f, 1f, 7f), UnitTeam.Enemy, structuresRoot);
            PrototypeEntityFactory.CreateControlNode("Central Holy Node", new Vector3(0f, 0.35f, 0f), structuresRoot);

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

            CreatePlayerFormation();
            CreateEnemyFormation();
            CreateSingleUnit(new Vector3(-18f, 1f, -9f), UnitTeam.Player, playerUnitRoot, UnitArchetype.Artillery);
            CreateSingleUnit(new Vector3(13f, 1f, 10f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Artillery);
            CreateSingleUnit(new Vector3(16f, 1f, 7f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Artillery);
        }

        private void CreatePlayerFormation()
        {
            for (int row = 0; row < friendlyGrid.y; row++)
            {
                for (int column = 0; column < friendlyGrid.x; column++)
                {
                    Vector3 spawnPosition = friendlyStart + new Vector3(column * unitSpacing, 0f, row * unitSpacing);
                    UnitArchetype archetype = row <= 1 ? UnitArchetype.Vanguard : UnitArchetype.Skirmisher;
                    CreateSingleUnit(spawnPosition, UnitTeam.Player, playerUnitRoot, archetype);
                }
            }
        }

        private void CreateEnemyFormation()
        {
            for (int row = 0; row < enemyGrid.y; row++)
            {
                for (int column = 0; column < enemyGrid.x; column++)
                {
                    Vector3 spawnPosition = enemyStart + new Vector3(column * unitSpacing, 0f, row * unitSpacing);
                    UnitArchetype archetype = row == 0 ? UnitArchetype.Skirmisher : UnitArchetype.Vanguard;
                    CreateSingleUnit(spawnPosition, UnitTeam.Enemy, enemyUnitRoot, archetype);
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

        private static void CreateTerrainPillar(string name, Vector3 position, Transform parent, Color color)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = name;
            pillar.transform.position = position;
            pillar.transform.localScale = new Vector3(0.8f, 2.2f, 0.8f);
            pillar.transform.SetParent(parent);
            pillar.GetComponent<Renderer>().material.color = color;
        }

        private static void CreateBanner(string name, Vector3 position, Color bannerColor, Transform parent)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = name + " Pole";
            pole.transform.position = position;
            pole.transform.localScale = new Vector3(0.12f, 2.4f, 0.12f);
            pole.transform.SetParent(parent);
            pole.GetComponent<Renderer>().material.color = new Color(0.34f, 0.28f, 0.2f);

            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = name;
            cloth.transform.position = position + new Vector3(0.8f, 1.2f, 0f);
            cloth.transform.localScale = new Vector3(1.5f, 1.1f, 0.1f);
            cloth.transform.SetParent(parent);
            cloth.GetComponent<Renderer>().material.color = bannerColor;
        }

        private static Transform GetOrCreateRoot(string rootName)
        {
            GameObject root = GameObject.Find(rootName);
            return root != null ? root.transform : new GameObject(rootName).transform;
        }
    }
}
