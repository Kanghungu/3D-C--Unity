// =============================================================================
// [Scripts ?덉씠?? Battle Aces]
// - NewSampleScene ?꾩슜 ???⑥씪 肄붿뼱 RTS ?꾪닾 猷⑦봽(?먯썝쨌?앹궛쨌?뱁뙣쨌誘몃땲留?.
// - Campaign???좏깮??MissionDefinition???덉쑝硫??굿룸ぉ?쑣룻뙥??諛곗쑉???쎌뼱 ?곸슜?쒕떎.
// - PrototypeBootstrapper? 蹂꾧컻 ???꾩옣 ?앹꽦? ???대옒?ㅺ? ?대떦?쒕떎.
// =============================================================================
using Game.Audio;
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
using UnityEngine.Audio;

namespace Game.BattleAces
{
    /// <summary>
    /// NewSampleScene 吏꾩엯: ?⑥씪 肄붿뼱횞2, ??8?щ’, ?먮룞 ?먯썝, 誘몄뀡(?좏깮 ??PersistentGameCore).
    /// </summary>
    public class BattleAcesSceneBootstrapper : MonoBehaviour
    {
        [Header("Arena")]
        [SerializeField] private bool createArenaOnPlay = true;
        [SerializeField] private Vector3 groundScale = new(24f, 1f, 24f);
        [SerializeField] private Color groundTint = new(0.22f, 0.24f, 0.28f);
        [SerializeField] private string groundObjectName = "Battle Arena Ground";

        /// <summary>GameObject.Find 諛섎났 ?몄텧 以꾩씠湲???遺?몄뒪?몃옪 1?뚯꽦 罹먯떆</summary>
        private GameObject cachedBattleGround;

        [Header("Spawns")]
        [SerializeField] private Vector3 playerCorePosition = new(-22f, 1.6f, -18f);
        [SerializeField] private Vector3 enemyCorePosition = new(22f, 1.6f, 18f);
        [SerializeField] private Vector3 playerRallyPoint = new(-12f, 1f, -8f);

        [Header("Audio (?뮤룻뙣 ?ㅽ똿)")]
        [Tooltip("Game/Audio/Create BattleAces_Main.mixer 濡?留뚮뱺 誘뱀꽌 ??Master ?꾨옒 ResultSting 沅뚯옣")]
        [SerializeField] private AudioMixer battleAcesAudioMixer;

        [Tooltip("誘뱀꽌 ??洹몃９ ?대쫫 ??ResultSting ???놁쑝硫?Master 濡??대갚")]
        [SerializeField] private string resultStingGroupName = "ResultSting";

        [Tooltip("誘뱀꽌 ?먯뀑 ?놁씠 洹몃９留?吏곸젒 ?ｌ쓣 ???덉쑝硫?AudioMixer ?좊떦蹂대떎 ?곗꽑)")]
        [SerializeField] private AudioMixerGroup resultStingMixerGroup;

        [Tooltip("?꾪닾 ?곕퉬?명듃 ??媛숈? 誘뱀꽌?먯꽌 BattleAmbient ?먯떇 洹몃９??留뚮뱾怨??대쫫 留욎땄")]
        [SerializeField] private string battleAmbientGroupName = "BattleAmbient";

        [Tooltip("Audio Mixer ?먯꽌 洹몃９ Volume ??Expose ???뚮씪誘명꽣 ?대쫫(?ㅼ젙 O ?⑤꼸怨??곌껐)")]
        [SerializeField] private string exposedBattleAmbientVolume = "BattleAmbientVol";

        [SerializeField] private string exposedResultStingVolume = "ResultStingVol";

        private void Awake()
        {
            ApplyResultStingMixerRouting();
            ApplyBattleAmbientMixerRouting();
            RegisterMixerVolumeExposes();

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

            GameObject ground = ResolveBattleGroundObject();
            if (ground != null)
            {
                BakeNavMeshAroundGround(ground.transform.position, groundScale);
            }

            systems.AddComponent<RtsTimeControl>();
            PrototypeGameDatabase database = systems.AddComponent<PrototypeGameDatabase>();
            BattleAcesEconomy economy = systems.AddComponent<BattleAcesEconomy>();
            BattleAcesRunStats runStats = systems.AddComponent<BattleAcesRunStats>();
            ApplyMissionIncomeTuning(economy, mission);
            if (mission != null && mission.MissionId == "mission_01_skirmish")
            {
                economy.ActivateOpeningIncomeBoost(60f, 1.35f);
            }
            BattleAcesMatchController match = systems.AddComponent<BattleAcesMatchController>();
            BattleAcesHudOverlay hud = systems.AddComponent<BattleAcesHudOverlay>();
            systems.AddComponent<PrototypeSelectionController>();
            BattleAcesEnemyBrain enemyBrain = systems.AddComponent<BattleAcesEnemyBrain>();

            UnitArchetype[] deck = ApplyAirborneCitadelDeckVariant(mission, BuildDeckArray(mission));

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
                campaignFlow.Initialize(mission, match, economy, runStats);
            }

            BattleAcesObjectiveUgui objectiveUgui = systems.AddComponent<BattleAcesObjectiveUgui>();
            objectiveUgui.Initialize(mission, campaignFlow);
            hud.Bind(economy, playerCore, match, mission, campaignFlow, objectiveUgui);
            enemyBrain.Bind(enemyCore, match);

            float thinkBase = 18.25f;
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

            string enemyPatternId = mission != null ? mission.EnemyPatternId : "default_skirmish";
            if (mission != null && mission.AirborneCitadelFocus)
            {
                enemyPatternId = "airborne_siege";
            }

            enemyBrain.ApplyEnemyPattern(enemyPatternId, thinkBase);

            float halfX = groundScale.x * 5f;
            float halfZ = groundScale.z * 5f;
            BattleAcesMinimap minimap = systems.AddComponent<BattleAcesMinimap>();
            Vector2 fogWorldMin = new Vector2(-halfX, -halfZ);
            Vector2 fogWorldMax = new Vector2(halfX, halfZ);
            minimap.Bind(
                fogWorldMin,
                fogWorldMax,
                playerCore,
                enemyCore,
                match,
                mission != null ? mission.ObjectiveKind : (MissionObjectiveKind?)null,
                showRallyOnLegend: mission != null);

            float fogSurfaceY = 0.08f;
            GameObject groundForFog = ResolveBattleGroundObject();
            if (groundForFog != null)
            {
                Renderer groundR = groundForFog.GetComponent<Renderer>();
                if (groundR != null)
                {
                    fogSurfaceY = groundR.bounds.max.y + 0.04f;
                }
            }

            GameObject fogWorldRoot = new GameObject("FogOfWar_WorldRoot");
            fogWorldRoot.transform.SetParent(systems.transform, false);
            BattleAcesFogWorldOverlay fogWorldOverlay = fogWorldRoot.AddComponent<BattleAcesFogWorldOverlay>();
            fogWorldOverlay.Initialize(fogWorldMin, fogWorldMax, fogSurfaceY);

            BattleAcesFogOfWarDebug fogDebug = systems.AddComponent<BattleAcesFogOfWarDebug>();
            fogDebug.Initialize(fogWorldMin, fogWorldMax, match);

            systems.AddComponent<BattleAcesStoryBanner>();
            systems.AddComponent<BattleAcesCombatAudio>();
            systems.AddComponent<BattleAcesInGameHelp>();
            systems.AddComponent<BattleAcesCombatAmbientLoop>();
            systems.AddComponent<BattleAcesMixerParameterSync>();
            systems.AddComponent<BattleAcesScreenFlashHud>();
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

        private void ApplyResultStingMixerRouting()
        {
            if (resultStingMixerGroup != null)
            {
                ProceduralAudioUtility.SetResultStingMixerGroup(resultStingMixerGroup);
                return;
            }

            if (battleAcesAudioMixer == null || string.IsNullOrEmpty(resultStingGroupName))
            {
                return;
            }

            AudioMixerGroup[] groups = battleAcesAudioMixer.FindMatchingGroups(resultStingGroupName);
            if (groups == null || groups.Length == 0)
            {
                groups = battleAcesAudioMixer.FindMatchingGroups("Master");
            }

            if (groups != null && groups.Length > 0)
            {
                ProceduralAudioUtility.SetResultStingMixerGroup(groups[0]);
            }
            else
            {
                Debug.LogWarning(
                    "[BattleAcesSceneBootstrapper] AudioMixer ??'" + resultStingGroupName + "' ?먮뒗 Master 洹몃９???놁뒿?덈떎.");
            }
        }

        private void ApplyBattleAmbientMixerRouting()
        {
            if (battleAcesAudioMixer == null || string.IsNullOrEmpty(battleAmbientGroupName))
            {
                return;
            }

            AudioMixerGroup[] ag = battleAcesAudioMixer.FindMatchingGroups(battleAmbientGroupName);
            if (ag != null && ag.Length > 0)
            {
                ProceduralAudioUtility.SetBattleAmbientMixerGroup(ag[0]);
            }
        }

        private void RegisterMixerVolumeExposes()
        {
            if (battleAcesAudioMixer == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(exposedBattleAmbientVolume) && string.IsNullOrEmpty(exposedResultStingVolume))
            {
                return;
            }

            ProceduralAudioUtility.RegisterMixerExposedVolumeParameters(
                battleAcesAudioMixer,
                exposedBattleAmbientVolume,
                exposedResultStingVolume);
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

            // 誘몄뀡 5 ???숈씪 ?먮졊 紐⑺몴?대굹 援ъ뿭쨌?μ븷臾?諛곗튂留?蹂二??ㅽ뀅怨?泥닿컧 遺꾨━)
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
            CreateCaptureZoneVisual(z.transform, boxSize);
            MissionCaptureZone cap = z.AddComponent<MissionCaptureZone>();
            cap.ConfigureHoldSeconds(mission != null ? mission.SeizeHoldSeconds : 12f);
            return cap;
        }

        /// <summary>誘몄뀡 5 ?꾩슜 ???먮졊 援ъ뿭 二쇰? ?μ븷臾??꾨줈?좎슜 ?꾨━誘명떚釉?.</summary>
        private static void CreateCaptureZoneVisual(Transform parent, Vector3 boxSize)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "SeizeZone_Visual";
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = new Vector3(0f, -2.15f, 0f);
            ring.transform.localScale = new Vector3(boxSize.x * 0.09f, 0.03f, boxSize.z * 0.09f);

            Collider ringCollider = ring.GetComponent<Collider>();
            if (ringCollider != null)
            {
                ringCollider.enabled = false;
            }

            Renderer ringRenderer = ring.GetComponent<Renderer>();
            if (ringRenderer != null)
            {
                ringRenderer.material.color = new Color(0.66f, 0.36f, 0.92f, 0.9f);
                ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ringRenderer.receiveShadows = false;
            }

            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "SeizeZone_Beacon";
            beacon.transform.SetParent(parent, false);
            beacon.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            beacon.transform.localScale = new Vector3(0.55f, 1.2f, 0.55f);

            Collider beaconCollider = beacon.GetComponent<Collider>();
            if (beaconCollider != null)
            {
                beaconCollider.enabled = false;
            }

            Renderer beaconRenderer = beacon.GetComponent<Renderer>();
            if (beaconRenderer != null)
            {
                beaconRenderer.material.color = new Color(0.82f, 0.58f, 1f, 0.96f);
                beaconRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                beaconRenderer.receiveShadows = false;
            }
        }

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
                    ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    ren.receiveShadows = false;
                }

                AddNavMeshObstacleFromCollider(cyl);
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
                    r2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r2.receiveShadows = false;
                }

                AddNavMeshObstacleFromCollider(cube);
            }
        }

        private static void AddNavMeshObstacleFromCollider(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Collider col = target.GetComponent<Collider>();
            if (col == null)
            {
                return;
            }

            NavMeshObstacle obstacle = target.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;

            if (col is CapsuleCollider capsule)
            {
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.center = capsule.center;
                obstacle.radius = capsule.radius * Mathf.Max(target.transform.lossyScale.x, target.transform.lossyScale.z);
                obstacle.height = capsule.height * target.transform.lossyScale.y;
                return;
            }

            if (col is BoxCollider box)
            {
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.center = box.center;
                obstacle.size = Vector3.Scale(box.size, target.transform.lossyScale);
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

        /// <summary>罹좏럹??誘몄뀡蹂꾨줈 ?먯썝 怨≪꽑留??댁쭩 議곗젙(A 諛몃윴???⑥뒪).</summary>
        /// <remarks>
        /// DEVLOG 짠I 쨌 誘몄뀡 3~6 ???먯뵫 ?뚮젅?????レ옄留?硫붾え???ш린??誘몄꽭 議곗젙?섎㈃ ??
        /// ?? mission_03 ??playerMul +0.01 / mission_04 ??enemyMul -0.02 ??媛먭컖쨌?ы쁽 湲곗?).
        /// </remarks>
        private static void ApplyMissionIncomeTuning(BattleAcesEconomy economy, MissionDefinition mission)
        {
            if (economy == null || mission == null || string.IsNullOrEmpty(mission.MissionId))
            {
                return;
            }

            float playerMul = 1f;
            float enemyMul = 1f;

            switch (mission.MissionId)
            {
                case "mission_01_skirmish":
                    playerMul = 1.11f;
                    enemyMul = 0.93f;
                    break;
                case "mission_02_sanctuary":
                    playerMul = 1.065f;
                    enemyMul = 0.985f;
                    break;
                case "mission_03_escort":
                    playerMul = 1.058f;
                    enemyMul = 1.032f;
                    break;
                case "mission_04_heresy":
                    playerMul = 1.032f;
                    enemyMul = 1.05f;
                    break;
                case "mission_05_stub":
                    playerMul = 1.045f;
                    enemyMul = 1.035f;
                    break;
                case "mission_06_fortress":
                    playerMul = 1.04f;
                    enemyMul = 1.025f;
                    break;
                case "mission_07_counter_rush":
                    playerMul = 1.048f;
                    enemyMul = 1.038f;
                    break;
                default:
                    return;
            }

            economy.ApplyIncomeMultipliers(playerMul, enemyMul);
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

        /// <summary>NewSampleScene 硫붿씤 移대찓?쇱뿉 RTS 議곗옉???놁쑝硫?遺숈씤??誘몃땲留??대┃ ?대룞??.</summary>
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

        /// <summary>誘몄뀡 ?듭뀡 ???대룞 ?붿깉瑜?怨듭쨷 ?붿깉濡?諛붽씀怨?怨듭꽦 ?щ’??媛뺤“</summary>
        private static UnitArchetype[] ApplyAirborneCitadelDeckVariant(MissionDefinition mission, UnitArchetype[] deck)
        {
            if (mission == null || !mission.AirborneCitadelFocus || deck == null || deck.Length != 8)
            {
                return deck;
            }

            UnitArchetype[] copy = new UnitArchetype[8];
            System.Array.Copy(deck, copy, 8);
            for (int i = 0; i < copy.Length; i++)
            {
                if (copy[i] == UnitArchetype.MobileFortress)
                {
                    copy[i] = UnitArchetype.AirborneCitadel;
                }
            }

            copy[3] = UnitArchetype.AirborneCitadel;
            return copy;
        }

        private GameObject ResolveBattleGroundObject()
        {
            if (cachedBattleGround == null)
            {
                cachedBattleGround = GameObject.Find(groundObjectName);
            }

            return cachedBattleGround;
        }

        private void SetupMinimalArena()
        {
            if (ResolveBattleGroundObject() != null)
            {
                return;
            }

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = groundObjectName;
            cachedBattleGround = ground;
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
