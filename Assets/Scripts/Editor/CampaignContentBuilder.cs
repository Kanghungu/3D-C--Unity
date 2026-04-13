#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Campaign;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.Settings;
using Game.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools
{
        /// <summary>
        /// Generates a compact Korean campaign content set and wires it into CampaignMenu.
        /// Assets are created under <c>Assets/Campaign/Content</c> as editable ScriptableObjects.
        /// </summary>
    public static class CampaignContentBuilder
    {
        private const string ContentFolder = "Assets/Campaign/Content";
        private const string DialoguePath = ContentFolder + "/CampaignDialogue_KR.asset";
        private const string CatalogPath = ContentFolder + "/CampaignMissionCatalog.asset";
        private const string ScenePath = "Assets/Scenes/CampaignMenu.unity";

        /// <summary>Seed dialogue strings used when generating CampaignDialogue_KR.asset.</summary>
        private static class DialogueKr
        {
            public const string M01Brief =
                "미션 1: 초전 교전\n\n적 코어를 파괴하세요. 숫자 키로 덱 생산, Alt+지면 우클릭으로 랠리를 설정하세요.\n아군 코어가 먼저 파괴되면 패배합니다. 브리핑을 끝낸 뒤 작전을 시작하세요.";
            public const string M01Win =
                "적 핵심이 무너졌습니다. Battle Aces 데모의 한 판 루프를 통과했습니다.";
            public const string M01Lose =
                "아군 코어가 파괴되었습니다. 생산·랠리·교전 밸런스를 다시 조정해 보세요.";

            public const string M02Brief =
                "미션 2: 성역 방어\n\n성소 목표를 지키며 적의 공세를 막으세요. 코어와 목표 HP를 HUD에서 확인하세요.\n필요하면 T/Y/U 강화를 활용하세요.\n아군 코어가 먼저 파괴되면 패배입니다.";
            public const string M02Win = "성소가 지켜졌습니다. 다음 작전으로 이어질 여지가 있습니다.";
            public const string M02Lose = "방어선이 무너졌습니다. 유닛 배치와 생산 타이밍을 조정해 보세요.";

            public const string M03Brief =
                "미션 3: 유물 호위\n\n지정 유물을 안전 구역으로 호위하세요. 적의 견제와 교전을 감안한 기동이 필요합니다.\n유물 HP를 먼저 살피고, 코어는 뒤에서 보호하세요.";
            public const string M03Win = "유물이 목표 지점에 도달했습니다. 호위 임무 완수입니다.";
            public const string M03Lose = "유물이 파괴되었거나 코어가 무너졌습니다. 전진 속도와 호위대 구성을 바꿔 보세요.";

            public const string M04Brief =
                "미션 4: 이단 요새\n\n중장갑 요새와 방어축을 뚫고 목표를 제거하세요. 포병·항공·특수 유닛을 섞는 것이 유리합니다.\n아군 코어가 먼저 파괴되면 패배입니다.";
            public const string M04Win = "이단 요새가 무너졌습니다. 주력 방어가 꺾였습니다.";
            public const string M04Lose = "아군 코어가 파괴되었습니다. 화력 배분과 돌입 타이밍을 다시 짜 보세요.";

            public const string M05Brief =
                "미션 5: 거점 점령(스텁)\n\n노드·유물 거점을 일정 시간 점유하세요. 적의 역공에 대비해 방어 유닛을 남겨 두세요.\n아군 코어가 먼저 파괴되면 패배입니다.";
            public const string M05Win = "거점을 장악했습니다. 짧은 캠페인 데모 구간을 마쳤습니다.";
            public const string M05Lose = "아군 코어가 파괴되었거나 점령에 실패했습니다. 다시 시도해 보세요.";
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
                "캠페인 데모 콘텐츠 생성을 완료했습니다.\n" + DialoguePath + "\n미션 5종과 카탈로그가 CampaignMenu에 연결됩니다.",
                "확인");
        }

        /// <summary>Adds missing late-mission dialogue ids without overwriting existing text.</summary>
        [MenuItem("Game/Campaign/Append Missing KR Dialogue Entries")]
        private static void AppendMissingKrDialogue()
        {
            DialogueTable d = AssetDatabase.LoadAssetAtPath<DialogueTable>(DialoguePath);
            if (d == null)
            {
                EditorUtility.DisplayDialog(
                    "Campaign",
                    "대화 자산이 없습니다.\nGame/Campaign/Generate Campaign Demo Content (KR)를 먼저 실행하세요.",
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
            EditorUtility.DisplayDialog("Campaign", "누락된 KR 대화 항목을 추가했습니다. (기존 값 유지)", "확인");
        }

        [MenuItem("Game/Campaign/Set Play Mode Start Scene -> CampaignMenu")]
        private static void SetPlayModeStart()
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                EditorUtility.DisplayDialog("씬 누락", "시작 씬을 찾을 수 없습니다: " + ScenePath, "확인");
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
            EditorUtility.DisplayDialog("Play Mode", "Play 모드 시작 씬을 CampaignMenu로 설정했습니다.", "확인");
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
                "1. 초전 교전 — 적 코어 격파",
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
                "2. 성역 방어",
                1,
                MissionObjectiveKind.SanctuaryDefense,
                "m02_brief",
                "m02_win",
                "m02_lose",
                132f,
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
                "3. 호위 임무",
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
                "4. 이단 요새 격파",
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
                "5. 거점 점령",
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
            PersistentGameCore core = Object.FindAnyObjectByType<PersistentGameCore>();
            CampaignMenuController menu = Object.FindAnyObjectByType<CampaignMenuController>();
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
