// =============================================================================
// [Scripts 레이어: Battle Aces]
// - NewSampleScene 전용 단일 코어 RTS 데모 루프(자원·생산·승패·미니맵).
// - Campaign에서 선택한 MissionDefinition이 있으면 전장·덱·적 패턴에 반영.
// - PrototypeBootstrapper는 구형 대형 전장 생성·DB를 담당(역할 분리).
// =============================================================================
using Game.Audio;
using Game.CameraSystem;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.Campaign.Scene;
using Game.Prototype;
using Game.Selection;
using Game.Settings;
using Game.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

namespace Game.BattleAces
{
    /// <summary>
    /// NewSampleScene 진입: 단일 코어 RTS 부트(지면·덱·경제·미션).
    /// 대기 연출은 <see cref="BattleAcesDemoStagePresentation"/> — 메뉴로 돌아갈 때는 <see cref="BattleAcesWorldPresentation"/> 가 RenderSettings/카메라를 복구.
    /// </summary>
    public class BattleAcesSceneBootstrapper : MonoBehaviour
    {
        [Header("Arena")]
        [SerializeField] private bool createArenaOnPlay = true;
        [SerializeField] private Vector3 groundScale = new(32f, 1f, 32f);
        // ClassicDuel 은 Apply 에서 그라데이션 머티리얼로 덮음 — 비클래식·폴백은 팔레트 중간 톤과 맞춤
        [SerializeField] private Color groundTint = new(0.18f, 0.2f, 0.24f);
        [SerializeField] private string groundObjectName = "Battle Arena Ground";

        /// <summary>GameObject.Find 반복 호출 줄이기 — 지면 루트 캐시</summary>
        private GameObject cachedBattleGround;

        [Header("Spawns")]
        [SerializeField] private Vector3 playerCorePosition = new(-34f, 1.6f, -28f);
        [SerializeField] private Vector3 enemyCorePosition = new(34f, 1.6f, 28f);
        [SerializeField] private Vector3 playerRallyPoint = new(-22f, 1f, -18f);

        [Header("Audio (믹서 선택)")]
        [Tooltip("BattleAces 메인 AudioMixer 에셋 — Master 아래 ResultSting 그룹 권장")]
        [SerializeField] private AudioMixer battleAcesAudioMixer;

        [Tooltip("승패 스팅 출력 그룹 이름 — 없으면 Master 로 폴백")]
        [SerializeField] private string resultStingGroupName = "ResultSting";

        [Tooltip("믹서 에셋 없이 그룹만 직접 넣을 때 — AudioMixerGroup 슬롯에 할당")]
        [SerializeField] private AudioMixerGroup resultStingMixerGroup;

        [Tooltip("전투 앰비언트 루프용 그룹 이름(믹서 내)")]
        [SerializeField] private string battleAmbientGroupName = "BattleAmbient";

        [Tooltip("Mixer 에서 그룹 Volume → Expose 한 파라미터 이름(O 설정과 동일)")]
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

            // 챕터 0: 메뉴 없이 NewSampleScene만 Play 해도 한 판 루프(브리핑·결과·R·메인)가 성립하도록 폴백 미션 주입
            if (mission == null)
            {
                PersistentGameCore coreForDemo = PersistentGameCore.FindOrCreateForBattleScene();
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                mission.AssignFallbackOneMatchDemoRuntime();
                coreForDemo.SetActiveMission(mission);
                coreForDemo.PendingMissionOrderIndex = -1;
                Debug.Log("[BattleAces] ActiveMission 없음 → Battle Aces 한 판 데모(폴백) 런타임 미션을 설정했습니다.");
            }

            Vector3 arenaScale = groundScale;
            Vector3 pPos = playerCorePosition;
            Vector3 ePos = enemyCorePosition;
            Vector3 rally = playerRallyPoint;
            BattleArenaLayoutBootstrap.ApplySpawnAndScale(
                mission,
                groundScale,
                playerCorePosition,
                enemyCorePosition,
                playerRallyPoint,
                out arenaScale,
                out pPos,
                out ePos,
                out rally);

            // 챕터6 번들: 미러 여부도 에셋이 아니라 번들 단일 소스
            bool mirrorX = mission != null &&
                           (DemoChapter6SingleMatchBundle.Matches(mission)
                               ? DemoChapter6SingleMatchBundle.MirroredLayoutVariant
                               : mission.MirroredLayoutVariant);
            if (mirrorX)
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
                SetupMinimalArena(arenaScale);
            }

            GameObject ground = ResolveBattleGroundObject();
            BattleArenaLayoutKind layoutKind = BattleArenaLayoutBootstrap.ResolveLayoutKind(mission);
            BattleArenaLayoutBootstrap.SpawnLayoutObstacles(structuresRoot, layoutKind, mirrorX);
            BattleAcesDemoStagePresentation.Apply(ground, arenaScale, structuresRoot, layoutKind, mirrorX);
            if (layoutKind != BattleArenaLayoutKind.ClassicDuel)
            {
                TrySpawnArenaLightDunes(structuresRoot, arenaScale, layoutKind, mirrorX);
            }

            if (ground != null)
            {
                BakeNavMeshAroundGround(ground.transform.position, arenaScale);
            }

            systems.AddComponent<RtsTimeControl>();
            PrototypeGameDatabase database = systems.AddComponent<PrototypeGameDatabase>();
            BattleAcesEconomy economy = systems.AddComponent<BattleAcesEconomy>();
            BattleAcesRunStats runStats = systems.AddComponent<BattleAcesRunStats>();
            ApplyMissionIncomeTuning(economy, mission);
            mission?.ApplySkirmishOpeningIncomeBoostIfNeeded(economy);
            BattleAcesClassicDuelArenaTuning.ApplyEconomyForLayout(economy, layoutKind, mission);
            // 캠페인 첫 스커미시 — 메뉴 스커미시와 별도 ID이므로 동일 톤의 개장 부스트 유지
            if (mission != null && mission.MissionId == "mission_01_skirmish")
            {
                economy.ActivateOpeningIncomeBoost(60f, 1.35f);
            }

            // 챕터 4 데모 — 초반 병력 형성이 늦으면 거점 공성이 지루해지므로 짧은 개장 부스트
            if (mission != null && mission.MissionId == "mission_04_heresy")
            {
                economy.ActivateOpeningIncomeBoost(48f, 1.18f);
            }
            BattleAcesMatchController match = systems.AddComponent<BattleAcesMatchController>();
            BattleAcesHudOverlay hud = systems.AddComponent<BattleAcesHudOverlay>();
            systems.AddComponent<PrototypeSelectionController>();
            systems.AddComponent<PlayerAbilityController>();
            BattleAcesEnemyBrain enemyBrain = systems.AddComponent<BattleAcesEnemyBrain>();

            UnitArchetype[] deckBase = DemoChapter6SingleMatchBundle.Matches(mission)
                ? DemoChapter6SingleMatchBundle.GetPlayerDeckEightCopy()
                : BuildDeckArray(mission);
            bool airborneCitadelFocus = mission != null &&
                                        (DemoChapter6SingleMatchBundle.Matches(mission)
                                            ? DemoChapter6SingleMatchBundle.AirborneCitadelFocus
                                            : mission.AirborneCitadelFocus);
            UnitArchetype[] deck = ApplyAirborneCitadelDeckVariant(airborneCitadelFocus, deckBase);

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
                new Color(0.42f, 0.66f, 0.98f),
                structuresRoot,
                playerHpMul);
            playerCore.gameObject.AddComponent<CoreStructureHitSound>();

            BattleAcesCore enemyCore = CreateCoreStructure(
                "Enemy Core",
                ePos,
                UnitTeam.Enemy,
                new Color(0.98f, 0.34f, 0.26f),
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
                rally);

            enemyCore.Initialize(
                UnitTeam.Enemy,
                deck,
                database,
                economy,
                unitsRoot,
                ePos + new Vector3(0f, 0f, -6f));

            enemyCore.gameObject.AddComponent<CoreStructureHitSound>();

            match.BindCores(playerCore, enemyCore);

            BattleMissionFlow missionFlow = null;
            if (mission != null)
            {
                missionFlow = systems.AddComponent<BattleMissionFlow>();
                systems.AddComponent<BattleAcesResultUgui>();
                missionFlow.Initialize(mission, match, economy, runStats);
            }

            BattleAcesObjectiveUgui objectiveUgui = systems.AddComponent<BattleAcesObjectiveUgui>();
            objectiveUgui.Initialize(mission, missionFlow);
            hud.Bind(economy, playerCore, match, mission, missionFlow, objectiveUgui);
            enemyBrain.Bind(enemyCore, match);
            systems.AddComponent<BattleAcesEnemyVisionHider>();

            float thinkBase = 18.25f;
            if (mission != null && mission.EnemyFactionRules != null)
            {
                thinkBase *= Mathf.Clamp(mission.EnemyFactionRules.ProductionDurationMultiplier, 0.5f, 2f);
            }

            float thinkIntervalMul = mission != null ? mission.EnemyBrainThinkIntervalMultiplier : 1f;
            float thinkOverride = mission != null ? mission.EnemyBrainThinkIntervalOverride : 0f;
            string enemyPatternId = mission != null ? mission.EnemyPatternId : "default_skirmish";

            if (DemoChapter6SingleMatchBundle.Matches(mission))
            {
                thinkIntervalMul = DemoChapter6SingleMatchBundle.EnemyBrainThinkIntervalMultiplier;
                thinkOverride = DemoChapter6SingleMatchBundle.EnemyBrainThinkIntervalOverride;
                enemyPatternId = DemoChapter6SingleMatchBundle.EnemyPatternId;
            }
            else if (mission != null && mission.AirborneCitadelFocus)
            {
                // 공중 변주 미션: 기본은 공성 패턴(챕터6은 번들에서 fortress_break 고정)
                enemyPatternId = "airborne_siege";
            }

            if (thinkOverride > 0f)
            {
                thinkBase = thinkOverride;
            }

            thinkBase *= thinkIntervalMul;

            enemyBrain.ApplyEnemyPattern(enemyPatternId, thinkBase);
            BattleAcesClassicDuelArenaTuning.ApplyEnemyBrainForLayout(enemyBrain, layoutKind, mirrorX);

            float halfX = arenaScale.x * 5f;
            float halfZ = arenaScale.z * 5f;
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
            systems.AddComponent<BattleAcesVictoryPresentation>();
            systems.AddComponent<BattleAcesInGameHelp>();
            BattleAcesFirstPlayGuide firstPlayGuide = systems.AddComponent<BattleAcesFirstPlayGuide>();
            firstPlayGuide.Initialize(mission, missionFlow, match, runStats);
            systems.AddComponent<BattleAcesPauseOverlay>();
            systems.AddComponent<GameSettingsMenuOverlay>();
            systems.AddComponent<BattleAcesCombatAmbientLoop>();
            systems.AddComponent<BattleAcesMixerParameterSync>();
            systems.AddComponent<BattleAcesScreenFlashHud>();
            TryAddBattleAcesWorldDamageNumbers(systems);
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
                mor.Initialize(mission, match, playerCore, relic, capture, heresy, missionFlow);

                // 챕터6: 필드 중간 대사 트리거 박스 생략(번들). 다른 미션은 기존과 동일.
                bool spawnFieldStory =
                    !DemoChapter6SingleMatchBundle.Matches(mission) ||
                    DemoChapter6SingleMatchBundle.SpawnStoryFieldTriggers;
                if (spawnFieldStory)
                {
                    BattleAcesMissionStorySpawner.SpawnForMission(mission, structuresRoot, pPos, ePos);
                }
            }

            // 챕터2 데모: 카메라가 기본 ±1600 바운드로 허공까지 밀리지 않도록 지면에 맞춤
            ApplyRtsCameraToBattleArena(ResolveBattleGroundObject(), arenaScale);
            // 브리핑 종료 후 1프레임 — 전장 중심·줌이 한 번에 읽히게(이후 플레이어 자유 시야)
            StartCoroutine(SnapBattleOverviewWhenGameplayReady(pPos, ePos, missionFlow));
        }

        /// <summary>작전 시작 후 코어 중점을 보도록 RTS 높이·XZ 를 1회만 맞춤.</summary>
        private IEnumerator SnapBattleOverviewWhenGameplayReady(
            Vector3 playerCoreWorld,
            Vector3 enemyCoreWorld,
            BattleMissionFlow missionFlow)
        {
            if (missionFlow != null)
            {
                while (missionFlow != null && !missionFlow.IsGameplayStarted)
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }

            yield return null;

            Camera cam = Camera.main;
            if (cam == null)
            {
                yield break;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            if (rts == null)
            {
                yield break;
            }

            Vector3 focus = (playerCoreWorld + enemyCoreWorld) * 0.5f;
            float span = Vector3.Distance(
                new Vector3(playerCoreWorld.x, 0f, playerCoreWorld.z),
                new Vector3(enemyCoreWorld.x, 0f, enemyCoreWorld.z));
            float targetHeight = Mathf.Clamp(span * 0.58f, 46f, 128f);
            rts.ApplyPresentationView(focus, targetHeight);
        }

        /// <summary>지면 Renderer 기준으로 RTS 카메라 XZ·줌 상한 설정(지면이 없으면 groundScale 폴백).</summary>
        private void ApplyRtsCameraToBattleArena(GameObject groundPlane, Vector3 planeScaleFallback)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            if (rts == null)
            {
                return;
            }

            const float paddingWorld = 38f;
            if (groundPlane != null && groundPlane.TryGetComponent(out Renderer groundRenderer))
            {
                Bounds b = groundRenderer.bounds;
                rts.SetWorldXZBounds(
                    b.min.x - paddingWorld,
                    b.max.x + paddingWorld,
                    b.min.z - paddingWorld,
                    b.max.z + paddingWorld);
                float longest = Mathf.Max(b.size.x, b.size.z);
                rts.SetHeightClamp(5f, Mathf.Clamp(longest * 0.52f, 52f, 240f));
                return;
            }

            float halfExtent = 5f * Mathf.Max(planeScaleFallback.x, planeScaleFallback.z);
            rts.SetWorldXZBounds(
                -halfExtent - paddingWorld,
                halfExtent + paddingWorld,
                -halfExtent - paddingWorld,
                halfExtent + paddingWorld);
            rts.SetHeightClamp(5f, Mathf.Max(halfExtent * 1.05f, 96f));
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
            else if (Application.isEditor || Debug.isDebugBuild)
            {
                Debug.LogWarning(
                    "[BattleAcesSceneBootstrapper] AudioMixer 에 '" + resultStingGroupName + "' 또는 Master 그룹이 없습니다.");
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

            // 미션 5 스텁 — 점령 목표는 같고 구역·대사만 변주(에셋과 스폰 위치 분리)
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

        /// <summary>미션 5 점령 구역 시각화 — 단순 실린더 링</summary>
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

        /// <summary>미션 ID별 플레이어·적 수입 배율(곱셈). ClassicDuel 은 이후 <see cref="BattleAcesClassicDuelArenaTuning"/> 가 추가 보정.</summary>
        /// <remarks>
        /// 캠페인 3~6화 등은 플레이 타임·압박감에 맞춰 미세 조정.
        /// 예: mission_03 player 약간↑, mission_04 enemy 약간↑ 등 DEVLOG 기준 반영.
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
                    // 목표 플레이 8~15분 — 적 압박은 유지하되 수입 격차가 과하면 초반이 과도하게 빡빡해짐
                    playerMul = 1.048f;
                    enemyMul = 1.038f;
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
                case "skirmish_vs_ai":
                case "skirmish_vs_ai_easy":
                case "skirmish_vs_ai_hard":
                    // 메뉴 스커미시 — missionId 미매칭 시 경제가 기본 곡선만 쓰여 한 판이 짧아지기 쉬움
                    playerMul = 1.08f;
                    enemyMul = 0.96f;
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

        /// <summary>메인 카메라에 RTS 컨트롤러가 없으면 추가 — 미니맵 클릭 이동 등</summary>
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

        /// <summary>공중 요새 미션 — 덱 슬롯 일부를 공성 병과 위주로 치환</summary>
        private static UnitArchetype[] ApplyAirborneCitadelDeckVariant(bool airborneCitadelFocus, UnitArchetype[] deck)
        {
            if (!airborneCitadelFocus || deck == null || deck.Length != 8)
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

        private void SetupMinimalArena(Vector3 planeScale)
        {
            if (ResolveBattleGroundObject() != null)
            {
                GameObject existing = ResolveBattleGroundObject();
                existing.transform.localScale = planeScale;
                return;
            }

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = groundObjectName;
            cachedBattleGround = ground;
            ground.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            ground.transform.localScale = planeScale;

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                // ClassicDuel 은 Apply 가 그라데이션으로 덮음 — 평면 단색도 팔레트·PBR 베이스 통일
                ReadablePrimitiveMaterialUtility.Apply(renderer, groundTint, 0f);
            }

            // 대기·안개·지면 그라데이션은 BattleAcesDemoStagePresentation.Apply 에서 통일 적용
        }

        private static void TrySpawnArenaLightDunes(Transform structuresParent, Vector3 arenaScale, BattleArenaLayoutKind kind, bool mirrorX)
        {
            System.Type terrainReadabilityType = System.Type.GetType("Game.BattleAces.BattleAcesArenaTerrainReadability, Assembly-CSharp");
            if (terrainReadabilityType == null)
            {
                return;
            }

            terrainReadabilityType.GetMethod("SpawnLightDunes")?.Invoke(
                null,
                new object[] { structuresParent, arenaScale, kind, mirrorX });
        }

        private static void TryAddBattleAcesWorldDamageNumbers(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            System.Type damageNumbersType = System.Type.GetType("Game.BattleAces.BattleAcesWorldDamageNumbers, Assembly-CSharp");
            if (damageNumbersType == null || target.GetComponent(damageNumbersType) != null)
            {
                return;
            }

            target.AddComponent(damageNumbersType);
        }

        private static void BakeNavMeshAroundGround(Vector3 groundCenter, Vector3 planeScale)
        {
            float extentX = planeScale.x * 5f + 40f;
            float extentZ = planeScale.z * 5f + 40f;
            Bounds bounds = new Bounds(groundCenter, new Vector3(extentX * 2f, 80f, extentZ * 2f));

            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(bounds, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);

            if (sources.Count == 0)
            {
                Debug.LogError(
                    "[BattleAces] NavMesh 수집 소스가 0개입니다. 지면 Plane·장애물에 Physics Collider 가 있는지, " +
                    "바운드가 씬 밖으로 벗어나지 않았는지 확인하세요. " +
                    $"center={groundCenter} extent=({planeScale.x * 5f + 40f:0}, {planeScale.z * 5f + 40f:0})");
                return;
            }

            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(
                settings, sources, bounds, Vector3.zero, Quaternion.identity);

            if (navMeshData != null)
            {
                NavMesh.AddNavMeshData(navMeshData);
            }
            else if (Application.isEditor || Debug.isDebugBuild)
            {
                Debug.LogWarning(
                    "[BattleAces] NavMesh bake 데이터가 null 입니다. Project Settings → Navigation 또는 " +
                    "콜라이더/레이어 설정을 확인하세요. " +
                    $"bounds center={groundCenter} sources={sources.Count}");
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
            go.transform.localScale = team == UnitTeam.Player
                ? new Vector3(5.9f, 3.38f, 5.9f)
                : new Vector3(5.42f, 3.06f, 5.42f);
            go.transform.SetParent(parent);

            BattleAcesCommandCoreVisuals.ApplyToCore(go, team, color);

            // 본진이 너무 빨리 무너지지 않도록 기본 체력 상향(팩션 배율은 그대로 곱함)
            float baseHp = team == UnitTeam.Player ? 5200f : 4800f;
            float maxHp = baseHp * Mathf.Max(0.25f, maxHpMultiplier);
            UnitHealth health = go.AddComponent<UnitHealth>();
            health.Configure(maxHp, true, new Vector3(0f, 2.4f, 0f));

            CombatTarget combatTarget = go.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);

            return go.AddComponent<BattleAcesCore>();
        }
    }
}
