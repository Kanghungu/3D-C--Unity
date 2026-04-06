// =============================================================================
// [Scripts 레이어: Battle Aces]
// - NewSampleScene 전용 — 단일 코어 RTS 전투 루프(자원·생산·승패·미니맵).
// - Campaign이 선택한 MissionDefinition이 있으면 덱·목표·팩션 배율을 읽어 적용한다.
// - PrototypeBootstrapper와 별개 — 전장 생성은 이 클래스가 담당한다.
// =============================================================================
using Game.CameraSystem;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.Campaign.Scene;
using Game.Prototype;
using Game.Selection;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.BattleAces
{
    /// <summary>
    /// NewSampleScene 진입: 단일 코어×2, 덱 8슬롯, 자동 자원, 미션(선택 시 PersistentGameCore).
    /// </summary>
    public class BattleAcesSceneBootstrapper : MonoBehaviour
    {
        [Header("Arena")]
        [SerializeField] private bool createArenaOnPlay = true;
        [SerializeField] private Vector3 groundScale = new(24f, 1f, 24f);
        [SerializeField] private Color groundTint = new(0.22f, 0.24f, 0.28f);
        [SerializeField] private string groundObjectName = "Battle Arena Ground";

        [Header("Spawns")]
        [SerializeField] private Vector3 playerCorePosition = new(-22f, 1.6f, -18f);
        [SerializeField] private Vector3 enemyCorePosition = new(22f, 1.6f, 18f);
        [SerializeField] private Vector3 playerRallyPoint = new(-12f, 1f, -8f);

        private void Awake()
        {
            EnsureRtsCameraControllerOnMainCamera();

            MissionDefinition mission = PersistentGameCore.Instance != null
                ? PersistentGameCore.Instance.ActiveMission
                : null;

            Vector3 pPos = playerCorePosition;
            Vector3 ePos = enemyCorePosition;
            Vector3 rally = playerRallyPoint;
            if (mission != null && mission.MirroredLayoutVariant)
            {
                pPos.x *= -1f;
                ePos.x *= -1f;
                rally.x *= -1f;
            }

            GameObject systems = new("BA_Systems");
            Transform structuresRoot = new GameObject("BA_Structures").transform;
            Transform unitsRoot = new GameObject("BA_Units").transform;

            if (createArenaOnPlay)
            {
                SetupMinimalArena();
            }

            GameObject ground = GameObject.Find(groundObjectName);
            if (ground != null)
            {
                BakeNavMeshAroundGround(ground.transform.position, groundScale);
            }

            systems.AddComponent<RtsTimeControl>();
            PrototypeGameDatabase database = systems.AddComponent<PrototypeGameDatabase>();
            BattleAcesEconomy economy = systems.AddComponent<BattleAcesEconomy>();
            BattleAcesMatchController match = systems.AddComponent<BattleAcesMatchController>();
            BattleAcesHudOverlay hud = systems.AddComponent<BattleAcesHudOverlay>();
            systems.AddComponent<PrototypeSelectionController>();
            BattleAcesEnemyBrain enemyBrain = systems.AddComponent<BattleAcesEnemyBrain>();

            UnitArchetype[] deck = BuildDeckArray(mission);

            float playerHpMul = mission != null && mission.PlayerFactionRules != null
                ? mission.PlayerFactionRules.UnitMaxHealthMultiplier
                : 1f;
            float enemyHpMul = mission != null && mission.EnemyFactionRules != null
                ? mission.EnemyFactionRules.UnitMaxHealthMultiplier
                : 1f;

            BattleAcesCore playerCore = CreateCoreStructure(
                "Player Core",
                pPos,
                UnitTeam.Player,
                new Color(0.55f, 0.72f, 0.95f),
                structuresRoot,
                playerHpMul);
            playerCore.gameObject.AddComponent<CoreStructureHitSound>();

            BattleAcesCore enemyCore = CreateCoreStructure(
                "Enemy Core",
                ePos,
                UnitTeam.Enemy,
                new Color(0.95f, 0.42f, 0.38f),
                structuresRoot,
                enemyHpMul);

            float playerProdMul = mission != null && mission.PlayerFactionRules != null
                ? mission.PlayerFactionRules.ProductionDurationMultiplier
                : 1f;
            float enemyProdMul = mission != null && mission.EnemyFactionRules != null
                ? mission.EnemyFactionRules.ProductionDurationMultiplier
                : 1f;

            playerCore.SetFactionProductionDurationMultiplier(playerProdMul);
            enemyCore.SetFactionProductionDurationMultiplier(enemyProdMul);

            playerCore.Initialize(
                UnitTeam.Player,
                deck,
                database,
                economy,
                unitsRoot,
                rally,
                enemyCore.transform);

            enemyCore.Initialize(
                UnitTeam.Enemy,
                deck,
                database,
                economy,
                unitsRoot,
                ePos + new Vector3(0f, 0f, -6f),
                playerCore.transform);

            enemyCore.gameObject.AddComponent<CoreStructureHitSound>();

            match.BindCores(playerCore, enemyCore);

            CampaignBattleFlow campaignFlow = null;
            if (mission != null)
            {
                campaignFlow = systems.AddComponent<CampaignBattleFlow>();
                campaignFlow.Initialize(mission, match, economy);
            }

            BattleAcesObjectiveUgui objectiveUgui = systems.AddComponent<BattleAcesObjectiveUgui>();
            objectiveUgui.Initialize(mission, campaignFlow);
            hud.Bind(economy, playerCore, match, mission, campaignFlow, objectiveUgui);
            enemyBrain.Bind(enemyCore, match);

            float thinkBase = 19f;
            if (mission != null && mission.EnemyFactionRules != null)
            {
                thinkBase *= Mathf.Clamp(mission.EnemyFactionRules.ProductionDurationMultiplier, 0.5f, 2f);
            }

            if (mission != null && mission.EnemyBrainThinkIntervalOverride > 0f)
            {
                thinkBase = mission.EnemyBrainThinkIntervalOverride;
            }

            if (mission != null)
            {
                thinkBase *= mission.EnemyBrainThinkIntervalMultiplier;
            }

            enemyBrain.ApplyEnemyPattern(
                mission != null ? mission.EnemyPatternId : "default_skirmish",
                thinkBase);

            float halfX = groundScale.x * 5f;
            float halfZ = groundScale.z * 5f;
            BattleAcesMinimap minimap = systems.AddComponent<BattleAcesMinimap>();
            minimap.Bind(new Vector2(-halfX, -halfZ), new Vector2(halfX, halfZ), playerCore, enemyCore, match);

            systems.AddComponent<BattleAcesStoryBanner>();
            systems.AddComponent<BattleAcesCombatAudio>();
            systems.AddComponent<BattleAcesInGameHelp>();
            systems.AddComponent<BattleAcesInputToggles>();
            BattleAcesSelectionInfoHud selectionInfo = systems.AddComponent<BattleAcesSelectionInfoHud>();
            selectionInfo.Bind(playerCore, economy, database);

            RelicMissionObject relic = null;
            MissionCaptureZone capture = null;
            CombatTarget heresy = null;

            if (mission != null)
            {
                switch (mission.ObjectiveKind)
                {
                    case MissionObjectiveKind.EscortRelic:
                    case MissionObjectiveKind.RecoverRelicAndEvacuate:
                        relic = SpawnRelicMission(structuresRoot, rally, mission);
                        break;
                    case MissionObjectiveKind.SeizeRelicOrNode:
                        capture = SpawnCaptureZone(structuresRoot, mission);
                        if (mission != null && mission.MissionId == "mission_05_stub" && capture != null)
                        {
                            SpawnMission05VariantProps(structuresRoot, capture.transform.position);
                        }

                        break;
                    case MissionObjectiveKind.DestroyHeresyStronghold:
                        heresy = SpawnHeresyStronghold(structuresRoot, ePos, mission);
                        break;
                }

                AddMissionMarkersToMinimap(minimap, relic, capture, heresy, rally, ePos);
                MissionObjectiveRuntime mor = systems.AddComponent<MissionObjectiveRuntime>();
                mor.Initialize(mission, match, playerCore, relic, capture, heresy, campaignFlow);

                BattleAcesMissionStorySpawner.SpawnForMission(mission, structuresRoot, pPos, ePos);
            }
        }

        private static void AddMissionMarkersToMinimap(
            BattleAcesMinimap minimap,
            RelicMissionObject relic,
            MissionCaptureZone capture,
            CombatTarget heresy,
            Vector3 rally,
            Vector3 enemyCorePos)
        {
            if (minimap == null)
            {
                return;
            }

            minimap.ClearExtraMarkers();
            if (relic != null)
            {
                minimap.AddExtraMarker(relic.transform.position, new Color(0.95f, 0.85f, 0.35f, 1f), 7f);
                if (relic.EscortTargetZone != null)
                {
                    minimap.AddExtraMarker(relic.EscortTargetZone.bounds.center, new Color(0.4f, 0.95f, 0.5f, 1f), 5f);
                }

                if (relic.EvacuationZone != null)
                {
                    minimap.AddExtraMarker(relic.EvacuationZone.bounds.center, new Color(0.45f, 0.55f, 1f, 1f), 5f);
                }
            }

            if (capture != null)
            {
                minimap.AddExtraMarker(capture.transform.position, new Color(0.9f, 0.5f, 1f, 1f), 7f);
            }

            if (heresy != null && heresy.IsAlive)
            {
                minimap.AddExtraMarker(heresy.transform.position, new Color(1f, 0.35f, 0.5f, 1f), 7f);
            }

            minimap.AddExtraMarker(rally, new Color(0.5f, 0.85f, 1f, 1f), 4f);
            minimap.AddExtraMarker(enemyCorePos, new Color(1f, 0.4f, 0.3f, 1f), 5f);
        }

        private static RelicMissionObject SpawnRelicMission(Transform root, Vector3 rally, MissionDefinition mission)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Mission Relic";
            go.transform.SetParent(root);
            go.transform.position = rally + new Vector3(6f, 1.1f, 4f);
            go.transform.localScale = Vector3.one * 2.2f;

            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = new Color(0.95f, 0.88f, 0.4f, 1f);
            }

            UnitHealth uh = go.AddComponent<UnitHealth>();
            float relicHp = mission != null ? mission.RelicMaxHealth : 520f;
            uh.Configure(relicHp, true, Vector3.zero);
            CombatTarget ct = go.AddComponent<CombatTarget>();
            ct.Initialize(UnitTeam.Player, uh);

            RelicMissionObject relic = go.AddComponent<RelicMissionObject>();

            GameObject escort = new GameObject("EscortZone");
            escort.transform.SetParent(root);
            escort.transform.position = new Vector3(8f, 2f, 10f);
            BoxCollider escortBox = escort.AddComponent<BoxCollider>();
            escortBox.isTrigger = true;
            escortBox.size = new Vector3(14f, 5f, 14f);

            GameObject evac = new GameObject("EvacZone");
            evac.transform.SetParent(root);
            evac.transform.position = new Vector3(-16f, 2f, 20f);
            BoxCollider evacBox = evac.AddComponent<BoxCollider>();
            evacBox.isTrigger = true;
            evacBox.size = new Vector3(12f, 5f, 12f);

            if (mission.ObjectiveKind == MissionObjectiveKind.EscortRelic)
            {
                relic.AssignZones(escortBox, null);
            }
            else
            {
                relic.AssignZones(escortBox, evacBox);
            }

            return relic;
        }

        private static MissionCaptureZone SpawnCaptureZone(Transform root, MissionDefinition mission)
        {
            GameObject z = new GameObject("SeizeZone");
            z.transform.SetParent(root);
            Vector3 pos = new Vector3(0f, 2.5f, 2f);
            Vector3 boxSize = new Vector3(18f, 6f, 18f);

            // 미션 5 — 동일 점령 목표이나 구역·장애물 배치만 변주(스텁과 체감 분리)
            if (mission != null && mission.MissionId == "mission_05_stub")
            {
                pos = new Vector3(-7f, 2.5f, 15f);
                boxSize = new Vector3(22f, 6f, 20f);
                if (mission.MirroredLayoutVariant)
                {
                    pos.x *= -1f;
                }
            }

            z.transform.position = pos;
            BoxCollider box = z.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = boxSize;
            MissionCaptureZone cap = z.AddComponent<MissionCaptureZone>();
            cap.ConfigureHoldSeconds(mission != null ? mission.SeizeHoldSeconds : 12f);
            return cap;
        }

        /// <summary>미션 5 전용 — 점령 구역 주변 장애물(프로토용 프리미티브).</summary>
        private static void SpawnMission05VariantProps(Transform root, Vector3 seizeCenter)
        {
            for (int i = 0; i < 10; i++)
            {
                float ang = (i / 10f) * Mathf.PI * 2f;
                float rad = 14f + (i % 3) * 2.2f;
                GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cyl.name = "M05_RuinsPillar_" + i;
                cyl.transform.SetParent(root);
                cyl.transform.position = seizeCenter + new Vector3(Mathf.Cos(ang) * rad, 1f, Mathf.Sin(ang) * rad);
                cyl.transform.localScale = new Vector3(1.5f, Random.Range(1.1f, 2.2f), 1.5f);
                Renderer ren = cyl.GetComponent<Renderer>();
                if (ren != null)
                {
                    ren.material.color = new Color(0.38f + Random.value * 0.08f, 0.33f, 0.4f, 1f);
                }
            }

            for (int j = 0; j < 5; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "M05_Debris_" + j;
                cube.transform.SetParent(root);
                cube.transform.position = seizeCenter + new Vector3(Random.Range(-11f, 11f), 0.55f, Random.Range(-9f, 9f));
                cube.transform.localScale = new Vector3(2.2f, 1.1f, 3f);
                cube.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 180f), 0f);
                Renderer r2 = cube.GetComponent<Renderer>();
                if (r2 != null)
                {
                    r2.material.color = new Color(0.28f, 0.3f, 0.34f, 1f);
                }
            }
        }

        private static CombatTarget SpawnHeresyStronghold(Transform root, Vector3 enemyCorePos, MissionDefinition mission)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Heresy Stronghold";
            go.transform.SetParent(root);
            go.transform.position = enemyCorePos + new Vector3(-14f, 1.6f, -10f);
            go.transform.localScale = new Vector3(5f, 3.5f, 5f);

            Renderer ren = go.GetComponent<Renderer>();
            if (ren != null)
            {
                ren.material.color = new Color(0.55f, 0.15f, 0.35f, 1f);
            }

            float hp = mission != null ? mission.HeresyStrongholdMaxHealth : 1280f;
            UnitHealth h = go.AddComponent<UnitHealth>();
            h.Configure(hp, true, new Vector3(0f, 2f, 0f));
            CombatTarget ct = go.AddComponent<CombatTarget>();
            ct.Initialize(UnitTeam.Enemy, h);
            go.AddComponent<MissionStructureHitSound>();
            return ct;
        }

        private static UnitArchetype[] BuildDeckArray(MissionDefinition mission)
        {
            if (mission == null)
            {
                return BuildDefaultDeck();
            }

            UnitArchetype[] src = mission.GetPlayerDeckCopy();
            if (src.Length == 0)
            {
                return BuildDefaultDeck();
            }

            UnitArchetype[] eight = new UnitArchetype[8];
            for (int i = 0; i < 8; i++)
            {
                eight[i] = i < src.Length ? src[i] : UnitArchetype.Spearman;
            }

            return eight;
        }

        /// <summary>NewSampleScene 메인 카메라에 RTS 조작이 없으면 붙인다(미니맵 클릭 이동용).</summary>
        private static void EnsureRtsCameraControllerOnMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            if (cam.GetComponent<RTSCameraController>() == null)
            {
                cam.gameObject.AddComponent<RTSCameraController>();
            }
        }

        private static UnitArchetype[] BuildDefaultDeck()
        {
            return new[]
            {
                UnitArchetype.Spearman,
                UnitArchetype.ShieldInfantry,
                UnitArchetype.Rifleman,
                UnitArchetype.Artillery,
                UnitArchetype.Fighter,
                UnitArchetype.SpecialWarrior,
                UnitArchetype.RoyalGuard,
                UnitArchetype.Outrider
            };
        }

        private void SetupMinimalArena()
        {
            if (GameObject.Find(groundObjectName) != null)
            {
                return;
            }

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = groundObjectName;
            ground.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            ground.transform.localScale = groundScale;

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = groundTint;
            }

            RenderSettings.ambientSkyColor = new Color(0.18f, 0.2f, 0.24f);
            RenderSettings.fog = false;
        }

        private static void BakeNavMeshAroundGround(Vector3 groundCenter, Vector3 planeScale)
        {
            float extentX = planeScale.x * 5f + 40f;
            float extentZ = planeScale.z * 5f + 40f;
            Bounds bounds = new Bounds(groundCenter, new Vector3(extentX * 2f, 80f, extentZ * 2f));

            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(bounds, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);

            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(
                settings, sources, bounds, Vector3.zero, Quaternion.identity);

            if (navMeshData != null)
            {
                NavMesh.AddNavMeshData(navMeshData);
            }
        }

        private static BattleAcesCore CreateCoreStructure(
            string objectName,
            Vector3 position,
            UnitTeam team,
            Color color,
            Transform parent,
            float maxHpMultiplier = 1f)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            go.transform.SetPositionAndRotation(position, Quaternion.identity);
            go.transform.localScale = new Vector3(5.6f, 3.2f, 5.6f);
            go.transform.SetParent(parent);

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            float baseHp = team == UnitTeam.Player ? 3000f : 2050f;
            float maxHp = baseHp * Mathf.Max(0.25f, maxHpMultiplier);
            UnitHealth health = go.AddComponent<UnitHealth>();
            health.Configure(maxHp, true, new Vector3(0f, 2.4f, 0f));

            CombatTarget combatTarget = go.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);

            return go.AddComponent<BattleAcesCore>();
        }
    }
}
