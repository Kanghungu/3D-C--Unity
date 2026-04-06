using Game.CameraSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Prototype
{
    /// <summary>
    /// 지면·조명·카메라·맵 프로필·내비메시·지형 장식.
    /// </summary>
    public partial class PrototypeBootstrapper
    {
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
                    friendlyGrid = new Vector2Int(20, 10);
                    enemyGrid = new Vector2Int(16, 8);
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
                    friendlyGrid = new Vector2Int(22, 10);
                    enemyGrid = new Vector2Int(18, 8);
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

            GroundTextureStyle style = battlefieldTheme switch
            {
                BattlefieldTheme.CrimsonBasin  => GroundTextureStyle.CrackedEarth,
                BattlefieldTheme.PaleSaltFlats => GroundTextureStyle.AshWasteland,
                _                              => GroundTextureStyle.DesertStone,
            };

            Renderer groundRenderer = ground.GetComponent<Renderer>();

            if (groundTextureOverride != null)
            {
                // 외부 텍스처 사용: 색상 보정 없이 그대로
                groundRenderer.material.color = Color.white;
                groundRenderer.material.mainTexture = groundTextureOverride;
                groundRenderer.material.mainTextureScale = groundTextureTiling;
            }
            else
            {
                // 절차적 생성 텍스처 (기존 방식)
                groundRenderer.material.color = sandColor;
                Texture2D groundTex = PrototypeGroundTextureFactory.Generate(sandColor, style);
                groundRenderer.material.mainTexture = groundTex;
                groundRenderer.material.mainTextureScale = new Vector2(groundScale.x * 0.18f, groundScale.z * 0.18f);
            }

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
            cameraController.CenterViewOnWorldPoint(GetInitialCameraFocusPoint());
        }

        private Vector3 GetInitialCameraFocusPoint()
        {
            Vector3 mapCenter = new(groundPosition.x, 0f, groundPosition.z);
            Vector3 approachAnchor = Vector3.Lerp(friendlyStart, mapCenter, 0.42f);
            Vector3 logisticsAnchor = Vector3.Lerp(playerBasePosition, playerFoundryPosition, 0.35f);
            Vector3 focusPoint = Vector3.Lerp(logisticsAnchor, approachAnchor, 0.68f);
            focusPoint.y = 0f;
            return focusPoint;
        }

        private void SetupTerrainFeatures()
        {
            Transform terrainRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");

            if (terrainRoot.childCount > 0)
            {
                return;
            }

            // 지도 경계벽
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("North Dune Wall", new Vector3(0f, 4.2f, 1780f), new Vector3(1280f, 8.4f, 64f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("South Dune Wall", new Vector3(0f, 4.2f, -1780f), new Vector3(1280f, 8.4f, 64f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("West Dune Arm", new Vector3(-2080f, 4.2f, 0f), new Vector3(64f, 8.4f, 960f), terrainRoot, duneColor);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("East Dune Arm", new Vector3(2080f, 4.2f, 0f), new Vector3(64f, 8.4f, 960f), terrainRoot, duneColor);

            CreateFactionDistricts(terrainRoot);
            CreateFrontlineObstacles(terrainRoot);
            CreateFlankLandmarks(terrainRoot);
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

        private void SetupNavMesh()
        {
            // 씬의 모든 PhysicsCollider를 수집해서 런타임으로 NavMesh 베이크
            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            Bounds bounds = new Bounds(groundPosition, new Vector3(
                groundScale.x * 10f + 400f,
                200f,
                groundScale.z * 10f + 400f));

            // Terrain Features 하위 오브젝트는 "Not Walkable"(area=1)로 표시.
            var markups = new List<NavMeshBuildMarkup>();
            Transform terrainRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");
            foreach (Transform child in terrainRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == terrainRoot) continue;
                markups.Add(new NavMeshBuildMarkup
                {
                    root        = child,
                    overrideArea = true,
                    area        = 1  // 1 = Not Walkable
                });
            }

            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(
                bounds,
                ~0,
                NavMeshCollectGeometry.PhysicsColliders,
                0,
                markups,
                sources);

            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(
                settings, sources, bounds, Vector3.zero, Quaternion.identity);

            if (navMeshData != null)
            {
                NavMesh.AddNavMeshData(navMeshData);
            }
        }

        private void CreateFactionDistricts(Transform terrainRoot)
        {
            Color playerStone = Color.Lerp(stoneColor, new Color(0.7f, 0.76f, 0.88f), 0.42f);
            Color enemyStone = Color.Lerp(ruinColor, new Color(0.44f, 0.18f, 0.16f), 0.54f);
            Color enemyAccent = new(1f, 0.48f, 0.2f);

            Vector3 playerSanctumCenter = playerBasePosition + new Vector3(180f, 0f, 260f);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Left Terrace", playerSanctumCenter + new Vector3(-110f, 1.6f, 120f), new Vector3(96f, 3.2f, 180f), terrainRoot, playerStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Player Right Terrace", playerSanctumCenter + new Vector3(110f, 1.6f, 120f), new Vector3(96f, 3.2f, 180f), terrainRoot, playerStone);

            Vector3 enemyWarcampCenter = enemyBasePosition + new Vector3(-220f, 0f, -240f);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy Battery Shelf Left", enemyWarcampCenter + new Vector3(-150f, 1.2f, -100f), new Vector3(110f, 2.4f, 160f), terrainRoot, enemyStone);
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Enemy Battery Shelf Right", enemyWarcampCenter + new Vector3(150f, 1.2f, -100f), new Vector3(110f, 2.4f, 160f), terrainRoot, enemyStone);

            for (int index = 0; index < 5; index++)
            {
                Vector3 spikePosition = enemyWarcampCenter + new Vector3(-180f + index * 90f, 2.4f, 100f);
                PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"Enemy Rampart Spike {index + 1}", spikePosition, new Vector3(16f, 4.8f, 16f), terrainRoot, enemyAccent);
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
    }
}
