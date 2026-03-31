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
        private Light sceneLight;
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
            SetupLighting();
            SetupMapProfile();
            SetupCamera();
            SetupRoots();
            SetupDatabase();
            SetupTerrainFeatures();
            SetupAmbientAnimator();
            SetupSkyAtmosphere();
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

        private void SetupLighting()
        {
            sceneLight = FindAnyObjectByType<Light>();
            if (sceneLight == null)
            {
                GameObject lightObject = new("Directional Light");
                sceneLight = lightObject.AddComponent<Light>();
                sceneLight.type = LightType.Directional;
            }

            sceneLight.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            sceneLight.color = Color.Lerp(altarColor, Color.white, 0.28f);
            sceneLight.intensity = battlefieldTheme == BattlefieldTheme.PaleSaltFlats ? 1.38f : 1.24f;
            sceneLight.shadows = LightShadows.Soft;
            sceneLight.shadowStrength = 0.74f;
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

            CreateFactionDistricts(terrainRoot);
            CreateProcessionalApproaches(terrainRoot);
            CreateFrontlineObstacles(terrainRoot);
            CreateFlankLandmarks(terrainRoot);
            CreateAtmosphericMonuments(terrainRoot);
            CreateRouteSignalLanes(terrainRoot);
            CreateProcessionConvoys(terrainRoot);
            CreateBattleSmokeAndBeacons(terrainRoot);
            CreateSearchlights(terrainRoot);
            CreateGrandAltarRitualSignals(terrainRoot);
        }

        private void SetupAmbientAnimator()
        {
            BattlefieldAmbientAnimator ambientAnimator = gameObject.GetComponent<BattlefieldAmbientAnimator>();
            if (ambientAnimator == null)
            {
                ambientAnimator = gameObject.AddComponent<BattlefieldAmbientAnimator>();
            }

            ambientAnimator.Configure(battlefieldTheme, PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features"), sceneLight);
        }

        private void SetupSkyAtmosphere()
        {
            BattlefieldSkyAtmosphere skyAtmosphere = gameObject.GetComponent<BattlefieldSkyAtmosphere>();
            if (skyAtmosphere == null)
            {
                skyAtmosphere = gameObject.AddComponent<BattlefieldSkyAtmosphere>();
            }

            skyAtmosphere.Configure(battlefieldTheme, mapProfile, PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features"));
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

            CreatePlayerVanguard();
            CreateEnemyGunline();
            CreateRearReserves();
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

        private void CreateFactionDistricts(Transform terrainRoot)
        {
            Color playerStone = Color.Lerp(stoneColor, new Color(0.7f, 0.76f, 0.88f), 0.42f);
            Color playerAccent = new(0.36f, 0.9f, 1f);
            Color enemyStone = Color.Lerp(ruinColor, new Color(0.44f, 0.18f, 0.16f), 0.54f);
            Color enemyAccent = new(1f, 0.48f, 0.2f);

            Vector3 playerSanctumCenter = playerBasePosition + new Vector3(180f, 0f, 260f);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Grand Forecourt", playerSanctumCenter, new Vector3(420f, 0.24f, 300f), terrainRoot, playerStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Procession Spine", playerSanctumCenter + new Vector3(120f, 0.12f, 260f), new Vector3(120f, 0.28f, 480f), terrainRoot, Color.Lerp(playerStone, altarColor, 0.32f));
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Left Terrace", playerSanctumCenter + new Vector3(-110f, 1.6f, 120f), new Vector3(96f, 3.2f, 180f), terrainRoot, playerStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Right Terrace", playerSanctumCenter + new Vector3(110f, 1.6f, 120f), new Vector3(96f, 3.2f, 180f), terrainRoot, playerStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Foundry Platform", playerFoundryPosition + new Vector3(70f, 0.18f, 70f), new Vector3(180f, 0.28f, 140f), terrainRoot, Color.Lerp(playerStone, altarColor, 0.2f));
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Siege Platform", playerSiegePosition + new Vector3(70f, 0.18f, 80f), new Vector3(220f, 0.32f, 150f), terrainRoot, Color.Lerp(playerStone, ruinColor, 0.12f));

            for (int index = 0; index < 6; index++)
            {
                float z = -1540f + index * 120f;
                PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"Player Colonnade West {index + 1}", new Vector3(-2440f, 7f, z), terrainRoot, playerStone);
                PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"Player Colonnade East {index + 1}", new Vector3(-2040f, 7f, z), terrainRoot, playerStone);
            }

            for (int index = 0; index < 4; index++)
            {
                Vector3 bannerPosition = new(-1970f + index * 110f, 11.4f, -960f + index * 46f);
                PrototypeTerrainPrimitiveFactory.CreateBanner($"Player Procession Banner {index + 1}", bannerPosition, playerAccent, terrainRoot);
            }

            Vector3 enemyWarcampCenter = enemyBasePosition + new Vector3(-220f, 0f, -240f);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy War Dais", enemyWarcampCenter, new Vector3(440f, 0.24f, 320f), terrainRoot, enemyStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy Gun Terrace", enemyWarcampCenter + new Vector3(-110f, 0.2f, -240f), new Vector3(320f, 0.36f, 140f), terrainRoot, Color.Lerp(enemyStone, ruinColor, 0.16f));
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy Battery Shelf Left", enemyWarcampCenter + new Vector3(-150f, 1.2f, -100f), new Vector3(110f, 2.4f, 160f), terrainRoot, enemyStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy Battery Shelf Right", enemyWarcampCenter + new Vector3(150f, 1.2f, -100f), new Vector3(110f, 2.4f, 160f), terrainRoot, enemyStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy Ember Trench", enemyWarcampCenter + new Vector3(0f, 0.08f, -60f), new Vector3(360f, 0.16f, 40f), terrainRoot, Color.Lerp(enemyAccent, ruinColor, 0.72f));

            for (int index = 0; index < 6; index++)
            {
                float x = 1840f + index * 110f;
                PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"Enemy War Pike {index + 1}", new Vector3(x, 7.4f, 1120f), terrainRoot, enemyStone);
            }

            for (int index = 0; index < 5; index++)
            {
                Vector3 spikePosition = enemyWarcampCenter + new Vector3(-180f + index * 90f, 2.4f, 100f);
                PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"Enemy Rampart Spike {index + 1}", spikePosition, new Vector3(16f, 4.8f, 16f), terrainRoot, enemyAccent);
            }

            for (int index = 0; index < 4; index++)
            {
                Vector3 bannerPosition = new(1940f + index * 120f, 11.4f, 980f - index * 44f);
                PrototypeTerrainPrimitiveFactory.CreateBanner($"Enemy War Banner {index + 1}", bannerPosition, enemyAccent, terrainRoot);
            }
        }

        private void CreateProcessionalApproaches(Transform terrainRoot)
        {
            Color shrineStone = Color.Lerp(stoneColor, altarColor, 0.28f);

            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Central Procession West", new Vector3(-840f, 0.16f, -320f), new Vector3(520f, 0.26f, 110f), terrainRoot, shrineStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Central Procession East", new Vector3(860f, 0.16f, 320f), new Vector3(520f, 0.26f, 110f), terrainRoot, shrineStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Northern Pilgrim Lane", new Vector3(0f, 0.15f, 1120f), new Vector3(840f, 0.24f, 90f), terrainRoot, Color.Lerp(shrineStone, sandColor, 0.28f));
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Southern Pilgrim Lane", new Vector3(0f, 0.15f, -1120f), new Vector3(840f, 0.24f, 90f), terrainRoot, Color.Lerp(shrineStone, sandColor, 0.28f));

            for (int index = 0; index < 5; index++)
            {
                float x = -920f + index * 460f;
                PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"Central Shrine Marker North {index + 1}", new Vector3(x, 4.6f, 210f), terrainRoot, Color.Lerp(altarColor, ruinColor, 0.2f));
                PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"Central Shrine Marker South {index + 1}", new Vector3(x, 4.6f, -210f), terrainRoot, Color.Lerp(altarColor, ruinColor, 0.2f));
            }
        }

        private void CreateFrontlineObstacles(Transform terrainRoot)
        {
            Color barricadeColor = Color.Lerp(ruinColor, duneColor, 0.42f);

            for (int index = 0; index < 4; index++)
            {
                float x = -980f + index * 660f;
                float z = -160f + (index % 2 == 0 ? -120f : 120f);
                PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"Midfield Barricade {index + 1}", new Vector3(x, 0.7f, z), new Vector3(120f, 1.4f, 34f), terrainRoot, barricadeColor);
            }

            for (int index = 0; index < 6; index++)
            {
                float x = -1280f + index * 500f;
                PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"Shard Cover North {index + 1}", new Vector3(x, 0.6f, 620f), new Vector3(56f, 1.2f, 28f), terrainRoot, Color.Lerp(barricadeColor, altarColor, 0.08f));
                PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"Shard Cover South {index + 1}", new Vector3(x + 120f, 0.6f, -620f), new Vector3(56f, 1.2f, 28f), terrainRoot, Color.Lerp(barricadeColor, altarColor, 0.08f));
            }
        }

        private void CreateFlankLandmarks(Transform terrainRoot)
        {
            Color ridgeColor = Color.Lerp(duneColor, ruinColor, 0.18f);

            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("West Flank Ridge North", new Vector3(-1560f, 3.2f, 980f), new Vector3(180f, 6.4f, 320f), terrainRoot, ridgeColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("West Flank Ridge South", new Vector3(-1560f, 3.2f, -980f), new Vector3(180f, 6.4f, 320f), terrainRoot, ridgeColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("East Flank Ridge North", new Vector3(1560f, 3.2f, 980f), new Vector3(180f, 6.4f, 320f), terrainRoot, ridgeColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("East Flank Ridge South", new Vector3(1560f, 3.2f, -980f), new Vector3(180f, 6.4f, 320f), terrainRoot, ridgeColor);

            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("West Scout Spur", new Vector3(-1180f, 1.4f, 1340f), new Vector3(90f, 2.8f, 180f), terrainRoot, ridgeColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("East Scout Spur", new Vector3(1180f, 1.4f, -1340f), new Vector3(90f, 2.8f, 180f), terrainRoot, ridgeColor);
        }

        private void CreateAtmosphericMonuments(Transform terrainRoot)
        {
            Color playerGlow = new(0.34f, 0.9f, 1f);
            Color enemyGlow = new(1f, 0.44f, 0.2f);
            Color shrineGlow = Color.Lerp(altarColor, Color.white, 0.18f);

            CreateAtmospherePrimitive("Grand Altar Beam", PrimitiveType.Cylinder, new Vector3(0f, 22f, 0f), new Vector3(1.8f, 18f, 1.8f), shrineGlow, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Halo", PrimitiveType.Cylinder, new Vector3(0f, 8.4f, 0f), new Vector3(7.6f, 0.06f, 7.6f), shrineGlow, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Crown", PrimitiveType.Sphere, new Vector3(0f, 42f, 0f), new Vector3(2.6f, 2.6f, 2.6f), shrineGlow, terrainRoot);

            CreateAtmospherePrimitive("Player Sanctum Beam", PrimitiveType.Cylinder, playerBasePosition + new Vector3(120f, 18f, 220f), new Vector3(1.4f, 14f, 1.4f), playerGlow, terrainRoot);
            CreateAtmospherePrimitive("Player Sanctum Halo", PrimitiveType.Cylinder, playerBasePosition + new Vector3(120f, 5.8f, 220f), new Vector3(5.8f, 0.05f, 5.8f), playerGlow, terrainRoot);
            CreateAtmospherePrimitive("Player Sanctum Beacon", PrimitiveType.Sphere, playerBasePosition + new Vector3(120f, 31f, 220f), new Vector3(1.8f, 1.8f, 1.8f), playerGlow, terrainRoot);

            CreateAtmospherePrimitive("Enemy War Pyre", PrimitiveType.Cylinder, enemyBasePosition + new Vector3(-120f, 16f, -180f), new Vector3(1.6f, 12f, 1.6f), enemyGlow, terrainRoot);
            CreateAtmospherePrimitive("Enemy War Halo", PrimitiveType.Cylinder, enemyBasePosition + new Vector3(-120f, 5.2f, -180f), new Vector3(6.4f, 0.05f, 6.4f), enemyGlow, terrainRoot);
            CreateAtmospherePrimitive("Enemy War Crown", PrimitiveType.Sphere, enemyBasePosition + new Vector3(-120f, 27f, -180f), new Vector3(2.2f, 2.2f, 2.2f), enemyGlow, terrainRoot);
        }

        private void CreateRouteSignalLanes(Transform terrainRoot)
        {
            Color playerColor = new(0.34f, 0.9f, 1f);
            Color enemyColor = new(1f, 0.44f, 0.2f);

            CreateRouteLane(
                "Player Sanctum Route",
                playerBasePosition + new Vector3(240f, 0.24f, 280f),
                new Vector3(-180f, 0.24f, -120f),
                9,
                playerColor,
                terrainRoot);

            CreateRouteLane(
                "Enemy Assault Route",
                enemyBasePosition + new Vector3(-260f, 0.24f, -240f),
                new Vector3(200f, 0.24f, 140f),
                9,
                enemyColor,
                terrainRoot);
        }

        private void CreateRouteLane(string routeName, Vector3 start, Vector3 end, int beaconCount, Color routeColor, Transform terrainRoot)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            float spacing = beaconCount <= 1 ? 0f : distance / (beaconCount - 1);

            for (int index = 0; index < beaconCount; index++)
            {
                Vector3 position = start + direction * (spacing * index);
                float beaconHeight = 0.52f + (index % 2 == 0 ? 0.04f : 0f);
                CreateAtmospherePrimitive($"{routeName} Route Beacon {index + 1}", PrimitiveType.Cylinder, position + new Vector3(0f, beaconHeight, 0f), new Vector3(0.42f, 0.36f, 0.42f), routeColor, terrainRoot);
                CreateAtmospherePrimitive($"{routeName} Route Halo {index + 1}", PrimitiveType.Cylinder, position + new Vector3(0f, 0.02f, 0f), new Vector3(2.2f, 0.02f, 2.2f), new Color(routeColor.r, routeColor.g, routeColor.b, 0.7f), terrainRoot);

                GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arrow.name = $"{routeName} Route Arrow {index + 1}";
                arrow.transform.SetParent(terrainRoot);
                arrow.transform.position = position + new Vector3(0f, 0.16f, 0f);
                arrow.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                arrow.transform.localScale = new Vector3(0.38f, 0.08f, 2.8f);
                Collider arrowCollider = arrow.GetComponent<Collider>();
                if (arrowCollider != null)
                {
                    arrowCollider.enabled = false;
                }

                Renderer arrowRenderer = arrow.GetComponent<Renderer>();
                if (arrowRenderer != null)
                {
                    arrowRenderer.material.color = Color.Lerp(routeColor, Color.white, 0.18f);
                }

                PrototypeTerrainPrimitiveFactory.EnsureFogObject(arrow, BattlefieldFogRequirement.Explored);
            }
        }

        private void CreateProcessionConvoys(Transform terrainRoot)
        {
            Color playerColor = new(0.34f, 0.9f, 1f);
            Color enemyColor = new(1f, 0.44f, 0.2f);

            CreateRouteConvoyLane(
                "Player Sanctum",
                playerBasePosition + new Vector3(240f, 0.3f, 280f),
                new Vector3(-180f, 0.3f, -120f),
                4,
                playerColor,
                terrainRoot);

            CreateRouteConvoyLane(
                "Enemy Assault",
                enemyBasePosition + new Vector3(-260f, 0.3f, -240f),
                new Vector3(200f, 0.3f, 140f),
                4,
                enemyColor,
                terrainRoot);
        }

        private void CreateRouteConvoyLane(string convoyName, Vector3 start, Vector3 end, int convoyCount, Color convoyColor, Transform terrainRoot)
        {
            Vector3 direction = (end - start).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, direction);

            for (int index = 0; index < convoyCount; index++)
            {
                float progress = convoyCount <= 1 ? 0.5f : Mathf.Lerp(0.16f, 0.84f, index / (float)(convoyCount - 1));
                float lateralOffset = (index % 2 == 0 ? -1f : 1f) * 6.4f;
                Vector3 position = Vector3.Lerp(start, end, progress) + right * lateralOffset;

                GameObject convoyGroup = new($"{convoyName} Route Convoy Group {index + 1}");
                convoyGroup.transform.SetParent(terrainRoot);
                convoyGroup.transform.position = position + new Vector3(0f, 0.54f + index * 0.04f, 0f);
                convoyGroup.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                convoyGroup.transform.localScale = Vector3.one;

                Color hullColor = Color.Lerp(convoyColor, ruinColor, 0.58f);
                Color haloColor = new Color(convoyColor.r, convoyColor.g, convoyColor.b, 0.72f);
                Color glowColor = Color.Lerp(convoyColor, Color.white, 0.18f);

                CreateAtmosphereChildPrimitive(
                    $"{convoyName} Route Convoy Halo {index + 1}",
                    PrimitiveType.Cylinder,
                    convoyGroup.transform,
                    new Vector3(0f, -0.46f, 0f),
                    new Vector3(1.8f, 0.02f, 1.8f),
                    haloColor);
                CreateAtmosphereChildPrimitive(
                    $"{convoyName} Route Convoy Cart {index + 1}",
                    PrimitiveType.Cube,
                    convoyGroup.transform,
                    new Vector3(0f, 0f, 0f),
                    new Vector3(1.6f, 0.32f, 3.8f),
                    hullColor);
                CreateAtmosphereChildPrimitive(
                    $"{convoyName} Route Convoy Canopy {index + 1}",
                    PrimitiveType.Cube,
                    convoyGroup.transform,
                    new Vector3(0f, 0.52f, 0.2f),
                    new Vector3(1.2f, 0.18f, 2.2f),
                    Color.Lerp(glowColor, hullColor, 0.44f));
                CreateAtmosphereChildPrimitive(
                    $"{convoyName} Route Convoy Core {index + 1}",
                    PrimitiveType.Sphere,
                    convoyGroup.transform,
                    new Vector3(0f, 0.7f, 0.2f),
                    Vector3.one * 0.78f,
                    glowColor);
                CreateAtmosphereChildPrimitive(
                    $"{convoyName} Route Convoy Escort Left {index + 1}",
                    PrimitiveType.Sphere,
                    convoyGroup.transform,
                    new Vector3(-1.2f, 0.26f, -0.8f),
                    Vector3.one * 0.42f,
                    glowColor);
                CreateAtmosphereChildPrimitive(
                    $"{convoyName} Route Convoy Escort Right {index + 1}",
                    PrimitiveType.Sphere,
                    convoyGroup.transform,
                    new Vector3(1.2f, 0.26f, -0.8f),
                    Vector3.one * 0.42f,
                    glowColor);
            }
        }

        private void CreateBattleSmokeAndBeacons(Transform terrainRoot)
        {
            Color playerSmoke = new(0.36f, 0.48f, 0.58f, 0.72f);
            Color enemySmoke = new(0.38f, 0.22f, 0.18f, 0.78f);
            Color emberPlayer = new(0.48f, 0.92f, 1f, 0.9f);
            Color emberEnemy = new(1f, 0.54f, 0.26f, 0.9f);

            CreateSmokeCluster("Player Watchfire", playerBasePosition + new Vector3(320f, 0f, 120f), playerSmoke, emberPlayer, terrainRoot);
            CreateSmokeCluster("Player Ridge Beacon", new Vector3(-1480f, 0f, 980f), playerSmoke, emberPlayer, terrainRoot);
            CreateSmokeCluster("Enemy Gun Pyre", enemyBasePosition + new Vector3(-360f, 0f, -80f), enemySmoke, emberEnemy, terrainRoot);
            CreateSmokeCluster("Enemy Ridge Pyre", new Vector3(1480f, 0f, -980f), enemySmoke, emberEnemy, terrainRoot);
            CreateSmokeCluster("Midfield Ruin Smoke", new Vector3(0f, 0f, -360f), Color.Lerp(playerSmoke, enemySmoke, 0.5f), Color.Lerp(emberPlayer, emberEnemy, 0.5f), terrainRoot);
            CreateSmokeCluster("Midfield Ruin Smoke East", new Vector3(420f, 0f, 280f), Color.Lerp(playerSmoke, enemySmoke, 0.5f), Color.Lerp(emberPlayer, emberEnemy, 0.5f), terrainRoot);
        }

        private void CreateSmokeCluster(string clusterName, Vector3 origin, Color smokeColor, Color emberColor, Transform terrainRoot)
        {
            for (int index = 0; index < 3; index++)
            {
                float offsetX = (index - 1) * 2.8f;
                float offsetZ = (index % 2 == 0 ? 1.6f : -1.6f);
                CreateAtmospherePrimitive(
                    $"{clusterName} Smoke Column {index + 1}",
                    PrimitiveType.Cylinder,
                    origin + new Vector3(offsetX, 8f + index * 1.8f, offsetZ),
                    new Vector3(1.8f + index * 0.34f, 7.8f + index * 1.4f, 1.8f + index * 0.34f),
                    smokeColor,
                    terrainRoot);
            }

            for (int index = 0; index < 4; index++)
            {
                float angle = index * 90f;
                float radians = angle * Mathf.Deg2Rad;
                CreateAtmospherePrimitive(
                    $"{clusterName} Ember {index + 1}",
                    PrimitiveType.Sphere,
                    origin + new Vector3(Mathf.Cos(radians) * 1.8f, 1.1f + index * 0.32f, Mathf.Sin(radians) * 1.8f),
                    Vector3.one * 0.42f,
                    emberColor,
                    terrainRoot);
            }
        }

        private void CreateSearchlights(Transform terrainRoot)
        {
            CreateSearchlightRig(
                "Player Searchlight",
                playerBasePosition + new Vector3(180f, 8f, 320f),
                new Color(0.34f, 0.9f, 1f, 0.65f),
                terrainRoot);
            CreateSearchlightRig(
                "Player Searchlight North",
                new Vector3(-1760f, 8f, 1180f),
                new Color(0.34f, 0.9f, 1f, 0.58f),
                terrainRoot);
            CreateSearchlightRig(
                "Enemy Searchlight",
                enemyBasePosition + new Vector3(-180f, 8f, -320f),
                new Color(1f, 0.5f, 0.22f, 0.65f),
                terrainRoot);
            CreateSearchlightRig(
                "Enemy Searchlight South",
                new Vector3(1760f, 8f, -1180f),
                new Color(1f, 0.5f, 0.22f, 0.58f),
                terrainRoot);
        }

        private void CreateSearchlightRig(string rigName, Vector3 position, Color beamColor, Transform terrainRoot)
        {
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = $"{rigName} Mast";
            mast.transform.SetParent(terrainRoot);
            mast.transform.position = position;
            mast.transform.localScale = new Vector3(0.64f, 6.6f, 0.64f);
            Collider mastCollider = mast.GetComponent<Collider>();
            if (mastCollider != null)
            {
                mastCollider.enabled = false;
            }

            Renderer mastRenderer = mast.GetComponent<Renderer>();
            if (mastRenderer != null)
            {
                mastRenderer.material.color = Color.Lerp(beamColor, new Color(0.14f, 0.14f, 0.16f), 0.7f);
            }
            PrototypeTerrainPrimitiveFactory.EnsureFogObject(mast, BattlefieldFogRequirement.Explored);

            CreateAtmospherePrimitive($"{rigName} Searchlight Beam", PrimitiveType.Cube, position + new Vector3(0f, 7.8f, 0f), new Vector3(1.6f, 1.6f, 42f), beamColor, terrainRoot);
            CreateAtmospherePrimitive($"{rigName} Searchlight Core", PrimitiveType.Sphere, position + new Vector3(0f, 8.2f, 0f), new Vector3(1.2f, 1.2f, 1.2f), Color.Lerp(beamColor, Color.white, 0.18f), terrainRoot);
        }

        private void CreateGrandAltarRitualSignals(Transform terrainRoot)
        {
            Color ritualColor = Color.Lerp(altarColor, Color.white, 0.24f);

            CreateAtmospherePrimitive("Grand Altar Ring 1", PrimitiveType.Cylinder, new Vector3(0f, 3.2f, 0f), new Vector3(10.6f, 0.03f, 10.6f), ritualColor, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Ring 2", PrimitiveType.Cylinder, new Vector3(0f, 6.8f, 0f), new Vector3(7.4f, 0.03f, 7.4f), ritualColor, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Ring 3", PrimitiveType.Cylinder, new Vector3(0f, 10.8f, 0f), new Vector3(4.8f, 0.03f, 4.8f), ritualColor, terrainRoot);

            CreateAtmospherePrimitive("Grand Altar Descent Beam 1", PrimitiveType.Cylinder, new Vector3(-3.8f, 16f, -3.8f), new Vector3(0.72f, 9f, 0.72f), ritualColor, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Descent Beam 2", PrimitiveType.Cylinder, new Vector3(3.8f, 18f, 3.8f), new Vector3(0.62f, 11f, 0.62f), ritualColor, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Descent Beam 3", PrimitiveType.Cylinder, new Vector3(-4.2f, 20f, 4.2f), new Vector3(0.54f, 12f, 0.54f), ritualColor, terrainRoot);
            CreateAtmospherePrimitive("Grand Altar Descent Beam 4", PrimitiveType.Cylinder, new Vector3(4.2f, 14f, -4.2f), new Vector3(0.8f, 8.5f, 0.8f), ritualColor, terrainRoot);
        }

        private static void CreateAtmospherePrimitive(string objectName, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent);
            primitive.transform.position = position;
            primitive.transform.localScale = scale;

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private static void CreateAtmosphereChildPrimitive(string objectName, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreatePlayerVanguard()
        {
            Vector3 spearAnchor = friendlyStart + new Vector3(90f, 0f, 420f);
            for (int index = 0; index < 18; index++)
            {
                float offsetX = (index - 8.5f) * 10f;
                float offsetZ = Mathf.Abs(index - 8.5f) * 5.8f;
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, spearAnchor + new Vector3(offsetX, 0f, offsetZ), UnitTeam.Player, playerUnitRoot, UnitArchetype.Spearman);
            }

            Vector3 shieldAnchor = friendlyStart + new Vector3(220f, 0f, 360f);
            for (int index = 0; index < 14; index++)
            {
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, shieldAnchor + new Vector3((index - 6.5f) * 9f, 0f, (index % 2 == 0 ? 0f : 10f)), UnitTeam.Player, playerUnitRoot, UnitArchetype.ShieldInfantry);
            }
        }

        private void CreateEnemyGunline()
        {
            Vector3 rifleAnchor = enemyStart + new Vector3(180f, 0f, -420f);
            for (int index = 0; index < 20; index++)
            {
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, rifleAnchor + new Vector3((index - 9.5f) * 9.6f, 0f, (index % 2 == 0 ? 0f : -12f)), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Rifleman);
            }

            Vector3 spearAnchor = enemyStart + new Vector3(-40f, 0f, -300f);
            for (int index = 0; index < 12; index++)
            {
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, spearAnchor + new Vector3((index - 5.5f) * 10.4f, 0f, Mathf.Abs(index - 5.5f) * -4.2f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Spearman);
            }
        }

        private void CreateRearReserves()
        {
            PrototypeArmyDeploymentUtility.CreateLine(database, enemyBasePosition + new Vector3(-240f, 0f, -220f), UnitTeam.Enemy, enemyUnitRoot, 10, 12f, UnitArchetype.RoyalGuard);
            PrototypeArmyDeploymentUtility.CreateSingleUnit(database, enemyBasePosition + new Vector3(-320f, 0f, -320f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.MobileFortress);
            PrototypeArmyDeploymentUtility.CreateSingleUnit(database, enemyBasePosition + new Vector3(-260f, 0f, -420f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.AirborneCitadel);
            PrototypeArmyDeploymentUtility.CreateLine(database, playerBasePosition + new Vector3(40f, 0f, 360f), UnitTeam.Player, playerUnitRoot, 8, 12f, UnitArchetype.SpecialWarrior);
        }
    }
}


