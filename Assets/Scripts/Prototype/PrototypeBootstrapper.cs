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
        [Header("Runtime Rebuild")]
        [SerializeField] private bool rebuildPrototypeWorldOnPlay = true;

        [Header("Battlefield Theme")]
        [SerializeField] private BattlefieldTheme battlefieldTheme = BattlefieldTheme.DesertShrine;
        [SerializeField] private bool randomizeBattlefieldThemeOnPlay;

        [Header("Ground")]
        [SerializeField] private Vector3 groundScale = new(460f, 1f, 440f);
        [SerializeField] private Vector3 groundPosition = Vector3.zero;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraPosition = new(0f, 720f, -1540f);
        [SerializeField] private Vector3 cameraRotation = new(68f, 0f, 0f);

        [Header("Army Setup")]
        [SerializeField] private Vector3 friendlyStart = new(-2060f, 1f, -840f);
        [SerializeField] private Vector2Int friendlyGrid = new(34, 18);
        [SerializeField] private Vector3 enemyStart = new(1580f, 1f, 620f);
        [SerializeField] private Vector2Int enemyGrid = new(26, 16);
        [SerializeField] private float unitSpacing = 8.8f;

        [Header("Bases")]
        [SerializeField] private Vector3 playerBasePosition = new(-2240f, 1f, -1460f);
        [SerializeField] private Vector3 enemyBasePosition = new(2240f, 1f, 1460f);
        [SerializeField] private Vector3 playerFoundryPosition = new(-2090f, 1f, -1630f);
        [SerializeField] private Vector3 playerSiegePosition = new(-2320f, 1f, -1780f);

        private Color sandColor = new(0.76f, 0.64f, 0.42f);
        private Color duneColor = new(0.68f, 0.55f, 0.34f);
        private Color stoneColor = new(0.62f, 0.56f, 0.46f);
        private Color altarColor = new(0.82f, 0.72f, 0.48f);
        private Color ruinColor = new(0.48f, 0.42f, 0.36f);

        private Transform playerUnitRoot;
        private Transform enemyUnitRoot;
        private PrototypeGameDatabase database;
        private BattlefieldMapProfile mapProfile;
        private BattlefieldVisionController visionController;
        private float rootLabelRefreshTimer;

        private void Awake()
        {
            if (rebuildPrototypeWorldOnPlay)
            {
                ClearExistingPrototypeWorld();
            }

            if (randomizeBattlefieldThemeOnPlay)
            {
                battlefieldTheme = (BattlefieldTheme)Random.Range(0, 3);
            }

            ApplyThemeLayout();
            ApplyThemePalette();
            SetupGround();
            SetupMapProfile();
            SetupCamera();
            SetupRoots();
            SetupDatabase();
            SetupTerrainFeatures();
            SetupPrototypeSystems();
            SetupBases();
            SetupUnits();
            RefreshRootLabels();
        }

        private void Update()
        {
            rootLabelRefreshTimer -= Time.deltaTime;

            if (rootLabelRefreshTimer <= 0f)
            {
                rootLabelRefreshTimer = 0.5f;
                RefreshRootLabels();
            }
        }

        private void ClearExistingPrototypeWorld()
        {
            PrototypeSceneHierarchyUtility.DestroyByType<SelectableUnit>();
            PrototypeSceneHierarchyUtility.DestroyByType<BaseStructure>();
            PrototypeSceneHierarchyUtility.DestroyByType<ProductionStructure>();
            PrototypeSceneHierarchyUtility.DestroyByType<ControlNode>();
            PrototypeSceneHierarchyUtility.DestroyByType<DefensiveTurret>();
            PrototypeSceneHierarchyUtility.DestroyByType<BattleDirectiveController>();
            PrototypeSceneHierarchyUtility.DestroyByType<BattlefieldMapProfile>();
            PrototypeSceneHierarchyUtility.DestroyByType<BattlefieldVisionController>();

            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Friendly Units");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Enemy Units");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Structures");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Terrain Features");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Move Marker");
        }

        private void ApplyThemePalette()
        {
            switch (battlefieldTheme)
            {
                case BattlefieldTheme.CrimsonBasin:
                    sandColor = new Color(0.53f, 0.31f, 0.25f);
                    duneColor = new Color(0.4f, 0.22f, 0.2f);
                    stoneColor = new Color(0.46f, 0.32f, 0.28f);
                    altarColor = new Color(0.84f, 0.38f, 0.24f);
                    ruinColor = new Color(0.28f, 0.18f, 0.16f);
                    break;
                case BattlefieldTheme.PaleSaltFlats:
                    sandColor = new Color(0.78f, 0.76f, 0.68f);
                    duneColor = new Color(0.64f, 0.64f, 0.58f);
                    stoneColor = new Color(0.7f, 0.68f, 0.62f);
                    altarColor = new Color(0.62f, 0.82f, 0.92f);
                    ruinColor = new Color(0.44f, 0.44f, 0.42f);
                    break;
                default:
                    sandColor = new Color(0.76f, 0.64f, 0.42f);
                    duneColor = new Color(0.68f, 0.55f, 0.34f);
                    stoneColor = new Color(0.62f, 0.56f, 0.46f);
                    altarColor = new Color(0.82f, 0.72f, 0.48f);
                    ruinColor = new Color(0.48f, 0.42f, 0.36f);
                    break;
            }
        }

        private void ApplyThemeLayout()
        {
            switch (battlefieldTheme)
            {
                case BattlefieldTheme.CrimsonBasin:
                    groundScale = new Vector3(500f, 1f, 460f);
                    cameraPosition = new Vector3(160f, 760f, -1660f);
                    cameraRotation = new Vector3(69f, -8f, 0f);
                    friendlyStart = new Vector3(-2240f, 1f, -920f);
                    enemyStart = new Vector3(1700f, 1f, 700f);
                    friendlyGrid = new Vector2Int(34, 18);
                    enemyGrid = new Vector2Int(30, 16);
                    playerBasePosition = new Vector3(-2460f, 1f, -1640f);
                    enemyBasePosition = new Vector3(2460f, 1f, 1600f);
                    playerFoundryPosition = new Vector3(-2310f, 1f, -1820f);
                    playerSiegePosition = new Vector3(-2520f, 1f, -1980f);
                    unitSpacing = 9.4f;
                    break;
                case BattlefieldTheme.PaleSaltFlats:
                    groundScale = new Vector3(560f, 1f, 520f);
                    cameraPosition = new Vector3(0f, 840f, -1880f);
                    cameraRotation = new Vector3(70f, 0f, 0f);
                    friendlyStart = new Vector3(-2500f, 1f, -980f);
                    enemyStart = new Vector3(1880f, 1f, 820f);
                    friendlyGrid = new Vector2Int(38, 18);
                    enemyGrid = new Vector2Int(30, 16);
                    playerBasePosition = new Vector3(-2820f, 1f, -1820f);
                    enemyBasePosition = new Vector3(2820f, 1f, 1780f);
                    playerFoundryPosition = new Vector3(-2650f, 1f, -2020f);
                    playerSiegePosition = new Vector3(-2880f, 1f, -2200f);
                    unitSpacing = 9.8f;
                    break;
            }
        }
        private void SetupMapProfile()
        {
            mapProfile = gameObject.GetComponent<BattlefieldMapProfile>();
            if (mapProfile == null)
            {
                mapProfile = gameObject.AddComponent<BattlefieldMapProfile>();
            }

            mapProfile.Configure(
                battlefieldTheme,
                GetBattlefieldLabel(battlefieldTheme),
                new Vector2(groundPosition.x, groundPosition.z),
                new Vector2(groundScale.x * 10f, groundScale.z * 10f),
                new Color(sandColor.r * 0.28f, sandColor.g * 0.28f, sandColor.b * 0.28f, 0.98f),
                new Color(0.02f, 0.02f, 0.03f, 0.96f),
                new Color(0.11f, 0.1f, 0.08f, 0.76f));

            visionController = gameObject.GetComponent<BattlefieldVisionController>();
            if (visionController == null)
            {
                visionController = gameObject.AddComponent<BattlefieldVisionController>();
            }

            visionController.Configure(mapProfile);
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
            RenderSettings.fogColor = Color.Lerp(sandColor, altarColor, 0.2f);
            RenderSettings.fogDensity = 0.00016f;
            RenderSettings.ambientSkyColor = Color.Lerp(stoneColor, sandColor, 0.4f);
            RenderSettings.ambientEquatorColor = Color.Lerp(ruinColor, stoneColor, 0.45f);
            RenderSettings.ambientGroundColor = Color.Lerp(ruinColor, Color.black, 0.5f);
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
            mainCamera.backgroundColor = Color.Lerp(sandColor, altarColor, 0.3f);
            mainCamera.fieldOfView = 52f;

            RTSCameraController cameraController = cameraTransform.GetComponent<RTSCameraController>();
            if (cameraController == null)
            {
                cameraController = cameraTransform.gameObject.AddComponent<RTSCameraController>();
            }

            cameraController.ApplyMapProfile(mapProfile);
        }

        private void SetupRoots()
        {
            playerUnitRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Friendly Units");
            enemyUnitRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Enemy Units");
            PrototypeSceneHierarchyUtility.GetOrCreateRoot("Structures");
            PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");
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
            Transform terrainRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");

            if (terrainRoot.childCount > 0)
            {
                return;
            }

            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("North Dune Wall", new Vector3(0f, 4.2f, 1780f), new Vector3(1280f, 8.4f, 64f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("South Dune Wall", new Vector3(0f, 4.2f, -1780f), new Vector3(1280f, 8.4f, 64f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("West Dune Arm", new Vector3(-2080f, 4.2f, 0f), new Vector3(64f, 8.4f, 960f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("East Dune Arm", new Vector3(2080f, 4.2f, 0f), new Vector3(64f, 8.4f, 960f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Grand Causeway", new Vector3(0f, 0.22f, 0f), new Vector3(220f, 0.44f, 1480f), terrainRoot, stoneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("North Crossway", new Vector3(0f, 0.21f, 720f), new Vector3(420f, 0.38f, 40f), terrainRoot, stoneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("South Crossway", new Vector3(0f, 0.21f, -720f), new Vector3(420f, 0.38f, 40f), terrainRoot, stoneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Sanctum Forecourt", new Vector3(-2140f, 0.16f, -1380f), new Vector3(240f, 0.28f, 240f), terrainRoot, stoneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Bastion Forecourt", new Vector3(2140f, 0.16f, 1380f), new Vector3(240f, 0.28f, 240f), terrainRoot, stoneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("West Trench", new Vector3(-760f, 0.12f, -460f), new Vector3(40f, 0.22f, 220f), terrainRoot, ruinColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("East Trench", new Vector3(760f, 0.12f, 460f), new Vector3(40f, 0.22f, 220f), terrainRoot, ruinColor);
            PrototypeTerrainPrimitiveFactory.CreateBanner("Player Sanctum Banner", new Vector3(-2120f, 11.2f, -1180f), new Color(0.78f, 0.8f, 0.92f), terrainRoot);
            PrototypeTerrainPrimitiveFactory.CreateBanner("Enemy War Banner", new Vector3(2120f, 11.2f, 1180f), new Color(0.92f, 0.42f, 0.24f), terrainRoot);
        }

        private void SetupPrototypeSystems()
        {
            if (FindAnyObjectByType<PrototypeSelectionController>() == null) gameObject.AddComponent<PrototypeSelectionController>();
            if (FindAnyObjectByType<EnemyAIController>() == null) gameObject.AddComponent<EnemyAIController>();
            if (FindAnyObjectByType<PrototypeHUD>() == null) gameObject.AddComponent<PrototypeHUD>();
            if (FindAnyObjectByType<PlayerProductionController>() == null) gameObject.AddComponent<PlayerProductionController>();
            if (FindAnyObjectByType<PlayerAbilityController>() == null) gameObject.AddComponent<PlayerAbilityController>();
            if (FindAnyObjectByType<ThreatResponseController>() == null) gameObject.AddComponent<ThreatResponseController>();
            if (FindAnyObjectByType<BattleDirectiveController>() == null) gameObject.AddComponent<BattleDirectiveController>();
            if (FindAnyObjectByType<PrototypeMatchController>() == null) gameObject.AddComponent<PrototypeMatchController>();
        }

        private void SetupBases()
        {
            Transform structuresRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Structures");

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            if (playerBase == null)
            {
                playerBase = PrototypeEntityFactory.CreateBase("Player Sanctum", playerBasePosition, UnitTeam.Player, structuresRoot);
            }

            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            if (enemyBase == null)
            {
                enemyBase = PrototypeEntityFactory.CreateBase("Enemy Bastion", enemyBasePosition, UnitTeam.Enemy, structuresRoot);
            }

            ProductionStructure playerFoundry = PrototypeRuntimeQuery.FindProductionStructure(UnitTeam.Player, UnitArchetype.Spearman);
            if (playerFoundry == null)
            {
                playerFoundry = PrototypeEntityFactory.CreateProductionStructure("Holy Foundry", playerFoundryPosition, UnitTeam.Player, structuresRoot);
            }

            ProductionStructure playerSiegeWorks = PrototypeRuntimeQuery.FindProductionStructure(UnitTeam.Player, UnitArchetype.Artillery);
            if (playerSiegeWorks == null || playerSiegeWorks == playerFoundry)
            {
                playerSiegeWorks = PrototypeEntityFactory.CreateProductionStructure("Siege Chapel", playerSiegePosition, UnitTeam.Player, structuresRoot);
            }

            playerFoundry.ConfigureStructure(
                "Holy Foundry",
                new Vector3(380f, 0f, 150f),
                new Color(0.74f, 0.7f, 0.56f),
                UnitArchetype.Spearman,
                UnitArchetype.ShieldInfantry,
                UnitArchetype.Rifleman,
                UnitArchetype.SpecialWarrior);
            playerSiegeWorks.ConfigureStructure(
                "Siege Chapel",
                new Vector3(460f, 0f, 180f),
                new Color(0.84f, 0.58f, 0.32f),
                UnitArchetype.Fighter,
                UnitArchetype.Artillery,
                UnitArchetype.MobileFortress,
                UnitArchetype.AirborneCitadel);

            EnsureTurret("Player Sentinel Left", new Vector3(-1460f, 1f, -720f), UnitTeam.Player, structuresRoot);
            EnsureTurret("Player Sentinel Right", new Vector3(-1620f, 1f, -680f), UnitTeam.Player, structuresRoot);
            EnsureTurret("Enemy Cannon Left", new Vector3(1460f, 1f, 720f), UnitTeam.Enemy, structuresRoot);
            EnsureTurret("Enemy Cannon Right", new Vector3(1620f, 1f, 680f), UnitTeam.Enemy, structuresRoot);

            RebuildBattlefieldNodes(structuresRoot);

            playerBase.Initialize(playerUnitRoot, database);
            enemyBase.Initialize(enemyUnitRoot, database);

            CombatTarget foundryTarget = playerFoundry.GetComponent<CombatTarget>();
            UnitHealth foundryHealth = playerFoundry.GetComponent<UnitHealth>();
            playerFoundry.Initialize(foundryTarget, foundryHealth, playerUnitRoot, database, playerBase);

            CombatTarget siegeTarget = playerSiegeWorks.GetComponent<CombatTarget>();
            UnitHealth siegeHealth = playerSiegeWorks.GetComponent<UnitHealth>();
            playerSiegeWorks.Initialize(siegeTarget, siegeHealth, playerUnitRoot, database, playerBase);
        }

        private void RebuildBattlefieldNodes(Transform parent)
        {
            foreach (ControlNode existingNode in FindObjectsByType<ControlNode>())
            {
                if (existingNode != null)
                {
                    DestroyImmediate(existingNode.gameObject);
                }
            }

            CreateBattlefieldNodeGrid(parent);
        }

        private static void EnsureTurret(string turretName, Vector3 position, UnitTeam team, Transform parent)
        {
            foreach (DefensiveTurret turret in FindObjectsByType<DefensiveTurret>())
            {
                if (turret != null && turret.name == turretName)
                {
                    return;
                }
            }

            PrototypeEntityFactory.CreateTurret(turretName, position, team, parent);
        }

        private void CreateBattlefieldNodeGrid(Transform parent)
        {
            float[] xPositions = BuildAxisPositions(7, groundScale.x * 4.35f, 260f);
            float[] zPositions = BuildAxisPositions(5, groundScale.z * 4.05f, 240f);
            Transform terrainRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");

            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Shrine Center Spine", new Vector3(0f, 0.16f, 0f), new Vector3(140f, 0.32f, groundScale.z * 0.78f), terrainRoot, Color.Lerp(stoneColor, altarColor, 0.18f));
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Shrine Mid Crossway", new Vector3(0f, 0.16f, 0f), new Vector3(groundScale.x * 0.62f, 0.28f, 24f), terrainRoot, Color.Lerp(stoneColor, ruinColor, 0.14f));

            for (int z = 0; z < zPositions.Length; z++)
            {
                for (int x = 0; x < xPositions.Length; x++)
                {
                    string label = $"Shrine {z * xPositions.Length + x + 1}";
                    bool isGrandNode = x == xPositions.Length / 2 && z == zPositions.Length / 2;
                    bool isMajorNode = x == xPositions.Length / 2 || z == zPositions.Length / 2;
                    float bonus = isGrandNode ? 1.2f : (isMajorNode ? 1.1f : 1.05f);
                    float healing = isGrandNode ? 8.4f : (isMajorNode ? 6.8f : 5.4f);
                    Vector3 position = new Vector3(xPositions[x], 0.35f, zPositions[z]);

                    if (isGrandNode)
                    {
                        label = "Grand Altar";
                    }
                    else if (isMajorNode)
                    {
                        label = $"Major Shrine {z * xPositions.Length + x + 1}";
                    }

                    Vector3 platformScale = isGrandNode ? new Vector3(30f, 0.4f, 30f) : (isMajorNode ? new Vector3(24f, 0.34f, 24f) : new Vector3(18f, 0.26f, 18f));
                    PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"{label} Platform", position + new Vector3(0f, 0.16f, 0f), platformScale, terrainRoot, isMajorNode ? altarColor : Color.Lerp(stoneColor, altarColor, 0.35f));

                    if (isGrandNode)
                    {
                        PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"{label} Pillar A", position + new Vector3(-12f, 4.2f, -12f), terrainRoot, ruinColor);
                        PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"{label} Pillar B", position + new Vector3(12f, 4.2f, 12f), terrainRoot, ruinColor);
                    }
                    else if (isMajorNode)
                    {
                        PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"{label} Marker", position + new Vector3(0f, 3.2f, -10f), terrainRoot, ruinColor);
                    }

                    CreateShrineNode($"{label} Node", position, label, bonus, healing, isGrandNode ? ControlNodeTier.Grand : (isMajorNode ? ControlNodeTier.Major : ControlNodeTier.Minor), parent);
                }
            }
        }
        private void SetupUnits()
        {
            if (FindAnyObjectByType<SelectableUnit>() != null)
            {
                return;
            }

            PrototypeArmyDeploymentUtility.CreateGridFormation(database, friendlyStart, friendlyGrid, unitSpacing, UnitTeam.Player, playerUnitRoot,
                row => row switch
                {
                    <= 4 => UnitArchetype.ShieldInfantry,
                    <= 8 => UnitArchetype.Spearman,
                    9 or 10 => UnitArchetype.SpecialWarrior,
                    _ => UnitArchetype.Rifleman
                });

            PrototypeArmyDeploymentUtility.CreateGridFormation(database, enemyStart, enemyGrid, unitSpacing, UnitTeam.Enemy, enemyUnitRoot,
                row => row switch
                {
                    <= 3 => UnitArchetype.Rifleman,
                    <= 7 => UnitArchetype.Spearman,
                    8 or 9 => UnitArchetype.SpecialWarrior,
                    _ => UnitArchetype.Rifleman
                });

            PrototypeArmyDeploymentUtility.CreateLine(database, friendlyStart + new Vector3(160f, 0f, 260f), UnitTeam.Player, playerUnitRoot, 28, 12f, UnitArchetype.Artillery);
            PrototypeArmyDeploymentUtility.CreateLine(database, enemyStart + new Vector3(120f, 0f, -220f), UnitTeam.Enemy, enemyUnitRoot, 16, 12f, UnitArchetype.Artillery);
            PrototypeArmyDeploymentUtility.CreateWing(database, friendlyStart + new Vector3(180f, 7.5f, 420f), UnitTeam.Player, playerUnitRoot, 24, 14f, UnitArchetype.Fighter);
            PrototypeArmyDeploymentUtility.CreateWing(database, enemyStart + new Vector3(140f, 7.5f, -320f), UnitTeam.Enemy, enemyUnitRoot, 18, 14f, UnitArchetype.Fighter);
            PrototypeArmyDeploymentUtility.CreateLine(database, playerBasePosition + new Vector3(80f, 0f, 220f), UnitTeam.Player, playerUnitRoot, 12, 12f, UnitArchetype.RoyalGuard);
            PrototypeArmyDeploymentUtility.CreateSingleUnit(database, playerBasePosition + new Vector3(220f, 0f, 280f), UnitTeam.Player, playerUnitRoot, UnitArchetype.MobileFortress);
            PrototypeArmyDeploymentUtility.CreateSingleUnit(database, playerBasePosition + new Vector3(260f, 0f, 360f), UnitTeam.Player, playerUnitRoot, UnitArchetype.AirborneCitadel);
        }
        private static void CreateShrineNode(string objectName, Vector3 position, string label, float captureMultiplier, float healingRadius, ControlNodeTier tier, Transform parent)
        {
            ControlNode node = PrototypeEntityFactory.CreateControlNode(objectName, position, parent);
            node.ConfigureNode(label, captureMultiplier, healingRadius, tier);
            PrototypeTerrainPrimitiveFactory.EnsureFogObject(node.gameObject, BattlefieldFogRequirement.Explored);
        }

        private void RefreshRootLabels()
        {
            if (playerUnitRoot != null)
            {
                PrototypeSceneHierarchyUtility.RefreshUnitRootLabel(playerUnitRoot, "Friendly Units", UnitTeam.Player);
            }

            if (enemyUnitRoot != null)
            {
                PrototypeSceneHierarchyUtility.RefreshUnitRootLabel(enemyUnitRoot, "Enemy Units", UnitTeam.Enemy);
            }
        }

        private static float[] BuildAxisPositions(int count, float halfExtent, float margin)
        {
            float usableMin = -Mathf.Max(halfExtent - margin, 120f);
            float usableMax = Mathf.Max(halfExtent - margin, 120f);
            float[] positions = new float[count];

            for (int index = 0; index < count; index++)
            {
                positions[index] = Mathf.Lerp(usableMin, usableMax, count == 1 ? 0.5f : index / (float)(count - 1));
            }

            return positions;
        }

        private static string GetBattlefieldLabel(BattlefieldTheme theme)
        {
            return theme switch
            {
                BattlefieldTheme.CrimsonBasin => "Crimson Basin Front",
                BattlefieldTheme.PaleSaltFlats => "Pale Salt Flats",
                _ => "Desert Shrine Front"
            };
        }
    }
}


