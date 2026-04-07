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
    /// ???¨Ì??¬∑ÎØ∏Ï?? 4Ï¢?¬∑Ïπ¥??Î°?Í∑??ùÏ?± ??CampaignMenu ?¨Ï?ê ?∞Í≤∞.
    /// ?¥Ï?©?? <c>Assets/Campaign/Content</c> ???§Ï†? ?êÏ??(????∑Î???YAML)Í≥?Îß?Ï∂???
    /// </summary>
    public static class CampaignContentBuilder
    {
        private const string ContentFolder = "Assets/Campaign/Content";
        private const string DialoguePath = ContentFolder + "/CampaignDialogue_KR.asset";
        private const string CatalogPath = ContentFolder + "/CampaignMissionCatalog.asset";
        private const string ScenePath = "Assets/Scenes/CampaignMenu.unity";

        /// <summary>CampaignDialogue_KR.asset Í≥???Ïùº Î¨∏Íµ¨ ???†Í∑? ?ùÏ?± ??Ï?êÎß??¨Ï?©.</summary>
        private static class DialogueKr
        {
            public const string M01Brief =
                "??Ï†?Î™? ?±Ï?§?¨Ï?¥ ?§Ïª§ÎØ∏Ï??\n\n?¥Î?® ÍµêÎ?®????Ïß? ÏΩ?Ï?¥Î•???Í¥¥??Ï?≠??Ï?§. ÎØ∏Î??Îß?Î∂?Ï? ??Ï?ù??Î™©Ì????Î????\n??Íµ∞ ÏΩ?Ï?¥Í∞? Î®ºÏ? Î∂?Í¥¥??Î©¥ ??Ï†??? ?§Ì?®Î°?Ï¢?Î£??©Î????";
            public const string M01Win =
                "?¥Î?® ÍµêÎ?® ÏΩ?Ï?¥Í∞? Ïπ®Î¨µ??Ï?µ??Î?§. ??Íµ¨Ï?≠???†Ï???? ?§Ï?? ÍµêÎ?®???êÏ?ºÎ°???Ï????Ï?µ??Î?§.";
            public const string M01Lose =
                "??Íµ∞ ÏΩ?Ï?¥Í∞? Î∂?Í¥¥??Ï?µ??Î?§. ???§Ì?®???§Ïù? ?±Ï†???Í∏∞Îè?Î°??¥Ï?¥Ïß?Î????";

            public const string M02Brief =
                "??Ï†?Î™? ?±Ï?≠ Î∞©Ï?¥\n\n??Ì?? ??Í∞? ??Ï?? ??Íµ∞ ÏΩ?Ï?¥Î•?Ïß??§Ï?≠??Ï?§. ??Í∞????§Ì??Î©?Íµ¨Ï?ê????Ï∞©?©Î????\n?®Ï? ??Í∞??? ?ÅÎ?® HUD????Ï???©Î????\n??Íµ∞ ÏΩ?Ï?¥Í∞? Î®ºÏ? Î∂?Í¥¥??Î©¥ ??Ï†??? ?§Ì?®Î°?Ï¢?Î£??©Î????";
            public const string M02Win = "?±Ï?≠??Ïß?Ïº?Ï°??µÎ???? ?†Ï?ê?§Ïù? ?∏Î??Í∞? ?§Ï?? ?∏Î¶Ω??Î?§.";
            public const string M02Lose = "?±Ï?≠??Î¨¥Î??Ï°?Ï?µ??Î?§. Í∑∏Î?¨???†Ï???? Í∫ºÏ?Ïß? ??Ï?µ??Î?§.";

            public const string M03Brief =
                "??Ï†?Î™? ?±Ï?†Î¨??∏Ï??\n\n?©Í∏? Íµ¨Ï≤¥(?±Ï?†Î¨?Î•??πÏ?? Î™©Ì?? Íµ¨Ï?≠Íπ?Ï? ?∏Ï????Ï?≠??Ï?§. ÎØ∏Î??ÎßµÏ?ê??Í∏?Ï??¬∑?πÏ?? ??Ï?ù????Ïù∏??????Ï?µ??Î?§.\n?±Ï?†Î¨ºÏù¥ Î®ºÏ? ??Í¥¥??Î©¥ ??Ï†??? ?§Ì?®Î°?Ï¢?Î£??©Î????";
            public const string M03Win = "?±Ï?†Î¨ºÏù¥ ??Ï†? Íµ¨Ï?≠????Î?¨??Ï?µ??Î?§. ?§Ïù? ??Íµ∞??Ï§?Îπ?Ì????????";
            public const string M03Lose = "?±Ï?†Î¨ºÏù¥ ??Í¥¥??Ï???µÎ???? ???êÏ?§?? ÍµêÎ?® ?∞Í∞ê??Í∏∞Î°ù?©Î????";

            public const string M04Brief =
                "??Ï†?Î™? ?¥Î?® Î≥∏Í±∞Ïß? ?¨Î©∏\n\n??ÏΩ?Ï?¥????Î≥?Í∞?Î°? Î∂?Ì?ç ??Ï?ù???¥Î?® Î≥∏Í±∞Ïß?(Í∞?Ì?? Íµ¨Ï°∞Î¨?Î•???Í±∞??Ï?≠??Ï?§. ÎØ∏Î??ÎßµÏ?ê????Ïπ?Î•???Ïù∏??????Ï?µ??Î?§.\n??Íµ∞ ÏΩ?Ï?¥Í∞? Î®ºÏ? Î∂?Í¥¥??Î©¥ ??Ï†??? ?§Ì?®Î°?Ï¢?Î£??©Î????";
            public const string M04Win = "?¥Î?® Î≥∏Í±∞Ïß?Í∞? Î¨¥Î??Ï°?Ï?µ??Î?§. ??Íµ¨Ï?≠???¥Î?® ?§Íµê????Í≤º?µÎ????";
            public const string M04Lose = "??Íµ∞ ÏΩ?Ï?¥Í∞? Î∂?Í¥¥??Ï?µ??Î?§. ?±Ï?§?¨Ï?¥ Î∞?Í≤©??Ï§?Îπ?Ì????????";

            public const string M05Brief =
                "??Ï†?Î™? Í±∞Ï†ê ?êÎ†π(?§Ì?Å)\n\nÎ≥¥Îùº???êÎ†π Íµ¨Ï?≠???ºÏ†? ??Í∞? ?†Ï???Î©¥ ?πÎ¶¨?©Î????\n??Íµ∞ ÏΩ?Ï?¥Í∞? Î®ºÏ? Î∂?Í¥¥??Î©¥ ?§Ì?®??Î????";
            public const string M05Win = "Í±∞Ï†ê??ÍµêÎ?®???êÏ?ê ??Ï?¥??Ï?µ??Î?§. ?§Ì?Å ÎØ∏Ï?? ?¥Î¶¨??";
            public const string M05Lose = "??Íµ∞ ÏΩ?Ï?¥Í∞? Î∂?Í¥¥??Ï?µ??Î?§. ?¨Ï†?Îπ????§Ï?? ??Ï†???Ï?≠??Ï?§.";
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
                "?ùÏ?±¬∑?∞Í≤∞ ??Î£?.\n" + DialoguePath + "\nÎØ∏Ï?? 5Ï¢?+ Ïπ¥Ì??Î°?Í∑∏ ??CampaignMenu",
                "??Ïù∏");
        }

        /// <summary>Í∏∞Ï°¥ ???¨Ì????m04 ????ÎùΩ IDÎß?Ï∂?Í?(????Î°??ùÌ?∏ ??Í∑∏??Ïù¥??Ï?©).</summary>
        [MenuItem("Game/Campaign/Append Missing KR Dialogue Entries")]
        private static void AppendMissingKrDialogue()
        {
            DialogueTable d = AssetDatabase.LoadAssetAtPath<DialogueTable>(DialoguePath);
            if (d == null)
            {
                EditorUtility.DisplayDialog(
                    "Campaign",
                    "???¨Ì??Í∞? ??Ï?µ??Î?§.\nGame/Campaign/Generate Campaign Demo Content (KR) Î•?Î®ºÏ? ?§Ì????Ï?∏??",
                    "??Ïù∏");
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
            EditorUtility.DisplayDialog("Campaign", "??ÎùΩ??KR ??????™©??Ï∂?Í???Ï?µ??Î?§ (??Ïù? ??Îß?).", "??Ïù∏");
        }

        [MenuItem("Game/Campaign/Set Play Mode Start Scene ??CampaignMenu")]
        private static void SetPlayModeStart()
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                EditorUtility.DisplayDialog("?§Î•?", "?¨Ïù? Ï∞æÏù? ????Ï?µ??Î?§: " + ScenePath, "??Ïù∏");
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
            EditorUtility.DisplayDialog("Play Mode", "Play Î≤?Ì?º ??CampaignMenu Î∂?????Ï???©Î????", "??Ïù∏");
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
                "1. ?±Ï?§?¨Ï?¥ ?§Ïª§ÎØ∏Ï?? ???¥Î?® ÏΩ?Ï?¥ ?¨Î©∏",
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
                "2. ?±Ï?≠ Î∞©Ï?¥ ????Ì?? ??Í∞? ?ùÏ°¥",
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
                "3. ?±Ï?†Î¨??∏Ï?? ???πÏ?? Î™©Ì?? Íµ¨Ï?≠",
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
                "4. Destroy the fortified heresy stronghold",
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
                "5. Í±∞Ï†ê ?êÎ†π ????Î? ?•Ï?† Íµ¨Ï?≠",
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
