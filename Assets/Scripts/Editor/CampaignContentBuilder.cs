#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Campaign;
using Game.Campaign.Data;
using Game.Settings;
using Game.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// 대사표·미션 4종·카탈로그 생성 후 CampaignMenu 씬에 연결.
    /// 내용은 <c>Assets/Campaign/Content</c> 의 실제 에셋(대사·미션 YAML)과 맞춘다.
    /// </summary>
    public static class CampaignContentBuilder
    {
        private const string ContentFolder = "Assets/Campaign/Content";
        private const string DialoguePath = ContentFolder + "/CampaignDialogue_KR.asset";
        private const string CatalogPath = ContentFolder + "/CampaignMissionCatalog.asset";
        private const string ScenePath = "Assets/Scenes/CampaignMenu.unity";

        /// <summary>CampaignDialogue_KR.asset 과 동일 문구 — 신규 생성 시에만 사용.</summary>
        private static class DialogueKr
        {
            public const string M01Brief =
                "작전명: 성스러운 스커미시\n\n이단 교단의 전진 코어를 파괴하십시오. 미니맵 붉은 표식이 목표입니다.\n아군 코어가 먼저 붕괴하면 작전은 실패로 종료됩니다.";
            public const string M01Win =
                "이단 교단 코어가 침묵했습니다. 이 구역의 신앙은 다시 교단의 손으로 돌아왔습니다.";
            public const string M01Lose =
                "아군 코어가 붕괴했습니다. 이 실패는 다음 성전의 기도로 이어집니다.";

            public const string M02Brief =
                "작전명: 성역 방어\n\n제한 시간 동안 아군 코어를 지키십시오. 시간이 다하면 구원이 도착합니다.\n남은 시간은 상단 HUD에 표시됩니다.\n아군 코어가 먼저 붕괴하면 작전은 실패로 종료됩니다.";
            public const string M02Win = "성역이 지켜졌습니다. 신자들의 노래가 다시 울립니다.";
            public const string M02Lose = "성역이 무너졌습니다. 그러나 신앙은 꺼지지 않습니다.";

            public const string M03Brief =
                "작전명: 성유물 호위\n\n황금 구체(성유물)를 녹색 목표 구역까지 호위하십시오. 미니맵에서 금색·녹색 표식을 확인할 수 있습니다.\n성유물이 먼저 파괴되면 작전은 실패로 종료됩니다.";
            public const string M03Win = "성유물이 안전 구역에 도달했습니다. 다음 행군을 준비하십시오.";
            public const string M03Lose = "성유물이 파괴되었습니다. 이 손실은 교단 연감에 기록됩니다.";

            public const string M04Brief =
                "작전명: 이단 본거지 섬멸\n\n적 코어와는 별개로, 분홍 표식의 이단 본거지(강화 구조물)를 제거하십시오. 미니맵에서 위치를 확인할 수 있습니다.\n아군 코어가 먼저 붕괴하면 작전은 실패로 종료됩니다.";
            public const string M04Win = "이단 본거지가 무너졌습니다. 이 구역의 이단 설교는 끊겼습니다.";
            public const string M04Lose = "아군 코어가 붕괴했습니다. 성스러운 반격을 준비하십시오.";

            public const string M05Brief =
                "작전명: 거점 점령(스텁)\n\n보라색 점령 구역을 일정 시간 유지하면 승리합니다.\n아군 코어가 먼저 붕괴하면 실패입니다.";
            public const string M05Win = "거점이 교단의 손에 넘어왔습니다. 스텁 미션 클리어.";
            public const string M05Lose = "아군 코어가 붕괴했습니다. 재정비 후 다시 도전하십시오.";
        }

        [MenuItem("Game/Campaign/Generate Campaign Demo Content (KR)")]
        private static void GenerateAll()
        {
            EnsureFolder(ContentFolder);

            DialogueTable dialogue = GetOrCreateDialogue();
            MissionDefinition m1 = CreateMissionSkirmish();
            MissionDefinition m2 = CreateMissionSanctuary();
            MissionDefinition m3 = CreateMissionEscort();
            MissionDefinition m4 = CreateMissionHeresy();
            MissionDefinition m5 = CreateMissionStub();
            WriteCatalog(m1, m2, m3, m4, m5);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireScene(dialogue);
            EditorUtility.DisplayDialog(
                "Campaign Content",
                "생성·연결 완료.\n" + DialoguePath + "\n미션 5종 + 카탈로그 → CampaignMenu",
                "확인");
        }

        /// <summary>기존 대사표에 m04 등 누락 ID만 추가(옛 프로젝트 업그레이드용).</summary>
        [MenuItem("Game/Campaign/Append Missing KR Dialogue Entries")]
        private static void AppendMissingKrDialogue()
        {
            DialogueTable d = AssetDatabase.LoadAssetAtPath<DialogueTable>(DialoguePath);
            if (d == null)
            {
                EditorUtility.DisplayDialog(
                    "Campaign",
                    "대사표가 없습니다.\nGame/Campaign/Generate Campaign Demo Content (KR) 를 먼저 실행하세요.",
                    "확인");
                return;
            }

            SerializedObject so = new SerializedObject(d);
            SerializedProperty entries = so.FindProperty("entries");
            var ids = new HashSet<string>();
            for (int i = 0; i < entries.arraySize; i++)
            {
                string id = entries.GetArrayElementAtIndex(i).FindPropertyRelative("Id").stringValue;
                if (!string.IsNullOrEmpty(id))
                {
                    ids.Add(id);
                }
            }

            void AddIfMissing(string id, string text)
            {
                if (ids.Contains(id))
                {
                    return;
                }

                entries.InsertArrayElementAtIndex(entries.arraySize);
                SerializedProperty e = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                e.FindPropertyRelative("Id").stringValue = id;
                e.FindPropertyRelative("Text").stringValue = text;
                ids.Add(id);
            }

            AddIfMissing("m04_brief", DialogueKr.M04Brief);
            AddIfMissing("m04_win", DialogueKr.M04Win);
            AddIfMissing("m04_lose", DialogueKr.M04Lose);
            AddIfMissing("m05_brief", DialogueKr.M05Brief);
            AddIfMissing("m05_win", DialogueKr.M05Win);
            AddIfMissing("m05_lose", DialogueKr.M05Lose);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(d);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Campaign", "누락된 KR 대사 항목을 추가했습니다 (없을 때만).", "확인");
        }

        [MenuItem("Game/Campaign/Set Play Mode Start Scene → CampaignMenu")]
        private static void SetPlayModeStart()
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                EditorUtility.DisplayDialog("오류", "씬을 찾을 수 없습니다: " + ScenePath, "확인");
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
            EditorUtility.DisplayDialog("Play Mode", "Play 버튼 시 CampaignMenu 부터 시작합니다.", "확인");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Campaign"))
            {
                AssetDatabase.CreateFolder("Assets", "Campaign");
            }

            AssetDatabase.CreateFolder("Assets/Campaign", "Content");
        }

        private static DialogueTable GetOrCreateDialogue()
        {
            DialogueTable existing = AssetDatabase.LoadAssetAtPath<DialogueTable>(DialoguePath);
            if (existing != null)
            {
                return existing;
            }

            DialogueTable table = ScriptableObject.CreateInstance<DialogueTable>();
            AssetDatabase.CreateAsset(table, DialoguePath);
            SerializedObject so = new SerializedObject(table);
            SerializedProperty entries = so.FindProperty("entries");
            entries.ClearArray();

            void Add(string id, string text)
            {
                entries.InsertArrayElementAtIndex(entries.arraySize);
                SerializedProperty e = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                e.FindPropertyRelative("Id").stringValue = id;
                e.FindPropertyRelative("Text").stringValue = text;
            }

            Add("m01_brief", DialogueKr.M01Brief);
            Add("m01_win", DialogueKr.M01Win);
            Add("m01_lose", DialogueKr.M01Lose);
            Add("m02_brief", DialogueKr.M02Brief);
            Add("m02_win", DialogueKr.M02Win);
            Add("m02_lose", DialogueKr.M02Lose);
            Add("m03_brief", DialogueKr.M03Brief);
            Add("m03_win", DialogueKr.M03Win);
            Add("m03_lose", DialogueKr.M03Lose);
            Add("m04_brief", DialogueKr.M04Brief);
            Add("m04_win", DialogueKr.M04Win);
            Add("m04_lose", DialogueKr.M04Lose);
            Add("m05_brief", DialogueKr.M05Brief);
            Add("m05_win", DialogueKr.M05Win);
            Add("m05_lose", DialogueKr.M05Lose);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(table);
            return table;
        }

        private static MissionDefinition CreateMissionSkirmish()
        {
            const string path = ContentFolder + "/Mission_01_Skirmish.asset";
            MissionDefinition m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (m != null)
            {
                return m;
            }

            m = ScriptableObject.CreateInstance<MissionDefinition>();
            ApplyMission(
                m,
                "mission_01_skirmish",
                "1. 성스러운 스커미시 — 이단 코어 섬멸",
                0,
                MissionObjectiveKind.DestroyEnemyCore,
                "m01_brief",
                "m01_win",
                "m01_lose",
                135f,
                14f,
                480f,
                1200f,
                new[]
                {
                    UnitArchetype.Spearman,
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.Rifleman,
                    UnitArchetype.Artillery,
                    UnitArchetype.Fighter,
                    UnitArchetype.SpecialWarrior,
                    UnitArchetype.RoyalGuard,
                    UnitArchetype.Outrider
                });

            AssetDatabase.CreateAsset(m, path);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static MissionDefinition CreateMissionSanctuary()
        {
            const string path = ContentFolder + "/Mission_02_Sanctuary.asset";
            MissionDefinition m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (m != null)
            {
                return m;
            }

            m = ScriptableObject.CreateInstance<MissionDefinition>();
            ApplyMission(
                m,
                "mission_02_sanctuary",
                "2. 성역 방어 — 제한 시간 생존",
                1,
                MissionObjectiveKind.SanctuaryDefense,
                "m02_brief",
                "m02_win",
                "m02_lose",
                145f,
                14f,
                520f,
                1280f,
                new[]
                {
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.Rifleman,
                    UnitArchetype.Rifleman,
                    UnitArchetype.Artillery,
                    UnitArchetype.Fighter,
                    UnitArchetype.Spearman,
                    UnitArchetype.RoyalGuard
                });

            AssetDatabase.CreateAsset(m, path);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static MissionDefinition CreateMissionEscort()
        {
            const string path = ContentFolder + "/Mission_03_Escort.asset";
            MissionDefinition m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (m != null)
            {
                return m;
            }

            m = ScriptableObject.CreateInstance<MissionDefinition>();
            ApplyMission(
                m,
                "mission_03_escort",
                "3. 성유물 호위 — 녹색 목표 구역",
                2,
                MissionObjectiveKind.EscortRelic,
                "m03_brief",
                "m03_win",
                "m03_lose",
                120f,
                12f,
                500f,
                1280f,
                new[]
                {
                    UnitArchetype.Spearman,
                    UnitArchetype.Rifleman,
                    UnitArchetype.Rifleman,
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.Fighter,
                    UnitArchetype.Artillery,
                    UnitArchetype.SpecialWarrior,
                    UnitArchetype.RoyalGuard
                });

            AssetDatabase.CreateAsset(m, path);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static MissionDefinition CreateMissionHeresy()
        {
            const string path = ContentFolder + "/Mission_04_Heresy.asset";
            MissionDefinition m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (m != null)
            {
                return m;
            }

            m = ScriptableObject.CreateInstance<MissionDefinition>();
            ApplyMission(
                m,
                "mission_04_heresy",
                "4. 이단 본거지 — 강화 구조물",
                3,
                MissionObjectiveKind.DestroyHeresyStronghold,
                "m04_brief",
                "m04_win",
                "m04_lose",
                120f,
                12f,
                500f,
                1100f,
                new[]
                {
                    UnitArchetype.Fighter,
                    UnitArchetype.Rifleman,
                    UnitArchetype.Artillery,
                    UnitArchetype.SpecialWarrior,
                    UnitArchetype.Spearman,
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.RoyalGuard,
                    UnitArchetype.Rifleman
                });

            SerializedObject soHeresy = new SerializedObject(m);
            soHeresy.FindProperty("enemyBrainThinkIntervalOverride").floatValue = 15f;
            soHeresy.FindProperty("enemyBrainThinkIntervalMultiplier").floatValue = 0.88f;
            soHeresy.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(m, path);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static MissionDefinition CreateMissionStub()
        {
            const string path = ContentFolder + "/Mission_05_Stub.asset";
            MissionDefinition m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
            if (m != null)
            {
                return m;
            }

            m = ScriptableObject.CreateInstance<MissionDefinition>();
            ApplyMission(
                m,
                "mission_05_stub",
                "5. 거점 점령 — 동부 장애 구역",
                4,
                MissionObjectiveKind.SeizeRelicOrNode,
                "m05_brief",
                "m05_win",
                "m05_lose",
                120f,
                14f,
                500f,
                1200f,
                new[]
                {
                    UnitArchetype.Spearman,
                    UnitArchetype.ShieldInfantry,
                    UnitArchetype.Rifleman,
                    UnitArchetype.Artillery,
                    UnitArchetype.Fighter,
                    UnitArchetype.SpecialWarrior,
                    UnitArchetype.RoyalGuard,
                    UnitArchetype.Outrider
                });

            SerializedObject soM05 = new SerializedObject(m);
            soM05.FindProperty("mirroredLayoutVariant").boolValue = true;
            soM05.FindProperty("enemyPatternId").stringValue = "aggressive_push";
            soM05.FindProperty("enemyBrainThinkIntervalMultiplier").floatValue = 0.92f;
            soM05.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(m, path);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void ApplyMission(
            MissionDefinition m,
            string id,
            string title,
            int sortOrder,
            MissionObjectiveKind kind,
            string brief,
            string win,
            string lose,
            float defenseSeconds,
            float seizeSeconds,
            float relicHp,
            float heresyHp,
            UnitArchetype[] deck)
        {
            SerializedObject so = new SerializedObject(m);
            so.FindProperty("missionId").stringValue = id;
            so.FindProperty("displayName").stringValue = title;
            so.FindProperty("gameplaySceneName").stringValue = "NewSampleScene";
            so.FindProperty("campaignSortOrder").intValue = sortOrder;
            so.FindProperty("objectiveKind").enumValueIndex = (int)kind;
            so.FindProperty("briefingDialogueId").stringValue = brief;
            so.FindProperty("victoryDialogueId").stringValue = win;
            so.FindProperty("defeatDialogueId").stringValue = lose;
            so.FindProperty("enemyPatternId").stringValue = "default_skirmish";

            so.FindProperty("defenseDurationSeconds").floatValue = defenseSeconds;
            so.FindProperty("seizeHoldSeconds").floatValue = seizeSeconds;

            so.FindProperty("relicMaxHealth").floatValue = relicHp;
            so.FindProperty("heresyStrongholdMaxHealth").floatValue = heresyHp;

            SerializedProperty deckProp = so.FindProperty("playerDeck");
            deckProp.ClearArray();
            for (int i = 0; i < deck.Length && i < 8; i++)
            {
                deckProp.InsertArrayElementAtIndex(i);
                deckProp.GetArrayElementAtIndex(i).enumValueIndex = (int)deck[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WriteCatalog(
            MissionDefinition m1,
            MissionDefinition m2,
            MissionDefinition m3,
            MissionDefinition m4,
            MissionDefinition m5)
        {
            CampaignMissionCatalog cat = AssetDatabase.LoadAssetAtPath<CampaignMissionCatalog>(CatalogPath);
            if (cat == null)
            {
                cat = ScriptableObject.CreateInstance<CampaignMissionCatalog>();
                AssetDatabase.CreateAsset(cat, CatalogPath);
            }

            SerializedObject so = new SerializedObject(cat);
            SerializedProperty arr = so.FindProperty("missionsInOrder");
            arr.ClearArray();
            MissionDefinition[] missions = { m1, m2, m3, m4, m5 };
            for (int i = 0; i < missions.Length; i++)
            {
                arr.InsertArrayElementAtIndex(i);
                arr.GetArrayElementAtIndex(i).objectReferenceValue = missions[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cat);
        }

        private static void WireScene(DialogueTable dialogue)
        {
            EditorSceneManager.OpenScene(ScenePath);
            PersistentGameCore core = Object.FindFirstObjectByType<PersistentGameCore>();
            CampaignMenuController menu = Object.FindFirstObjectByType<CampaignMenuController>();
            CampaignMissionCatalog catalog = AssetDatabase.LoadAssetAtPath<CampaignMissionCatalog>(CatalogPath);

            if (core != null)
            {
                SerializedObject so = new SerializedObject(core);
                so.FindProperty("dialogueTable").objectReferenceValue = dialogue;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(core);
            }

            if (menu != null && catalog != null)
            {
                SerializedObject so = new SerializedObject(menu);
                so.FindProperty("catalog").objectReferenceValue = catalog;
                so.FindProperty("useRuntimeDemoIfCatalogEmpty").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(menu);
            }

            GameObject settingsGo = GameObject.Find("Game Settings Overlay");
            if (settingsGo == null)
            {
                settingsGo = new GameObject("Game Settings Overlay");
                settingsGo.AddComponent<GameSettingsMenuOverlay>();
            }
            else if (settingsGo.GetComponent<GameSettingsMenuOverlay>() == null)
            {
                settingsGo.AddComponent<GameSettingsMenuOverlay>();
            }

            EditorSceneManager.SaveOpenScenes();
        }
    }
}
#endif
