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
        [SerializeField] private Vector3 friendlyStart = new(-10f, 1f, -4f);
        [SerializeField] private Vector2Int friendlyGrid = new(3, 2);
        [SerializeField] private Vector3 enemyStart = new(8f, 1f, 6f);
        [SerializeField] private Vector2Int enemyGrid = new(3, 2);
        [SerializeField] private float unitSpacing = 3f;

        [Header("Bases")]
        [SerializeField] private Vector3 playerBasePosition = new(-18f, 1f, -14f);
        [SerializeField] private Vector3 enemyBasePosition = new(18f, 1f, 14f);

        private Transform playerUnitRoot;
        private Transform enemyUnitRoot;

        private void Awake()
        {
            SetupGround();
            SetupCamera();
            SetupRoots();
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
            if (FindObjectsByType<BaseStructure>(FindObjectsSortMode.None).Length > 0)
            {
                return;
            }

            Transform structuresRoot = GetOrCreateRoot("Structures");
            BaseStructure playerBase = PrototypeEntityFactory.CreateBase("Player Base", playerBasePosition, UnitTeam.Player, structuresRoot);
            BaseStructure enemyBase = PrototypeEntityFactory.CreateBase("Enemy Base", enemyBasePosition, UnitTeam.Enemy, structuresRoot);

            playerBase.Initialize(playerUnitRoot);
            enemyBase.Initialize(enemyUnitRoot);
        }

        private void SetupUnits()
        {
            if (FindAnyObjectByType<SelectableUnit>() != null)
            {
                return;
            }

            CreateFormation(friendlyStart, friendlyGrid, UnitTeam.Player, playerUnitRoot);
            CreateFormation(enemyStart, enemyGrid, UnitTeam.Enemy, enemyUnitRoot);
        }

        private void CreateFormation(Vector3 origin, Vector2Int grid, UnitTeam team, Transform parent)
        {
            for (int row = 0; row < grid.y; row++)
            {
                for (int column = 0; column < grid.x; column++)
                {
                    Vector3 spawnPosition = origin + new Vector3(column * unitSpacing, 0f, row * unitSpacing);
                    UnitArchetype archetype = row == 0 ? UnitArchetype.Vanguard : UnitArchetype.Skirmisher;
                    PrototypeEntityFactory.CreateUnit(team, archetype, spawnPosition, parent);
                }
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
