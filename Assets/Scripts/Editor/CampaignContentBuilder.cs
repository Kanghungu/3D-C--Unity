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
    /// ?€?¬í‘œÂ·ë¯¸ì…˜ 4ì¢…Â·ì¹´?ˆë¡œê·??ì„± ??CampaignMenu ?¬ì— ?°ê²°.
    /// ?´ìš©?€ <c>Assets/Campaign/Content</c> ???¤ì œ ?ì…‹(?€??·ë???YAML)ê³?ë§ì¶˜??
    /// </summary>
    public static class CampaignContentBuilder
    {
        private const string ContentFolder = "Assets/Campaign/Content";
        private const string DialoguePath = ContentFolder + "/CampaignDialogue_KR.asset";
        private const string CatalogPath = ContentFolder + "/CampaignMissionCatalog.asset";
        private const string ScenePath = "Assets/Scenes/CampaignMenu.unity";

        /// <summary>CampaignDialogue_KR.asset ê³??™ì¼ ë¬¸êµ¬ ??? ê·œ ?ì„± ?œì—ë§??¬ìš©.</summary>
        private static class DialogueKr
        {
            public const string M01Brief =
                "?‘ì „ëª? ?±ìŠ¤?¬ìš´ ?¤ì»¤ë¯¸ì‹œ\n\n?´ë‹¨ êµë‹¨???„ì§„ ì½”ì–´ë¥??Œê´´?˜ì‹­?œì˜¤. ë¯¸ë‹ˆë§?ë¶‰ì? ?œì‹??ëª©í‘œ?…ë‹ˆ??\n?„êµ° ì½”ì–´ê°€ ë¨¼ì? ë¶•ê´´?˜ë©´ ?‘ì „?€ ?¤íŒ¨ë¡?ì¢…ë£Œ?©ë‹ˆ??";
            public const string M01Win =
                "?´ë‹¨ êµë‹¨ ì½”ì–´ê°€ ì¹¨ë¬µ?ˆìŠµ?ˆë‹¤. ??êµ¬ì—­??? ì•™?€ ?¤ì‹œ êµë‹¨???ìœ¼ë¡??Œì•„?”ìŠµ?ˆë‹¤.";
            public const string M01Lose =
                "?„êµ° ì½”ì–´ê°€ ë¶•ê´´?ˆìŠµ?ˆë‹¤. ???¤íŒ¨???¤ìŒ ?±ì „??ê¸°ë„ë¡??´ì–´ì§‘ë‹ˆ??";

            public const string M02Brief =
                "?‘ì „ëª? ?±ì—­ ë°©ì–´\n\n?œí•œ ?œê°„ ?™ì•ˆ ?„êµ° ì½”ì–´ë¥?ì§€?¤ì‹­?œì˜¤. ?œê°„???¤í•˜ë©?êµ¬ì›???„ì°©?©ë‹ˆ??\n?¨ì? ?œê°„?€ ?ë‹¨ HUD???œì‹œ?©ë‹ˆ??\n?„êµ° ì½”ì–´ê°€ ë¨¼ì? ë¶•ê´´?˜ë©´ ?‘ì „?€ ?¤íŒ¨ë¡?ì¢…ë£Œ?©ë‹ˆ??";
            public const string M02Win = "?±ì—­??ì§€ì¼œì¡Œ?µë‹ˆ?? ? ì?¤ì˜ ?¸ë˜ê°€ ?¤ì‹œ ?¸ë¦½?ˆë‹¤.";
            public const string M02Lose = "?±ì—­??ë¬´ë„ˆì¡ŒìŠµ?ˆë‹¤. ê·¸ëŸ¬??? ì•™?€ êº¼ì?ì§€ ?ŠìŠµ?ˆë‹¤.";

            public const string M03Brief =
                "?‘ì „ëª? ?±ìœ ë¬??¸ìœ„\n\n?©ê¸ˆ êµ¬ì²´(?±ìœ ë¬?ë¥??¹ìƒ‰ ëª©í‘œ êµ¬ì—­ê¹Œì? ?¸ìœ„?˜ì‹­?œì˜¤. ë¯¸ë‹ˆë§µì—??ê¸ˆìƒ‰Â·?¹ìƒ‰ ?œì‹???•ì¸?????ˆìŠµ?ˆë‹¤.\n?±ìœ ë¬¼ì´ ë¨¼ì? ?Œê´´?˜ë©´ ?‘ì „?€ ?¤íŒ¨ë¡?ì¢…ë£Œ?©ë‹ˆ??";
            public const string M03Win = "?±ìœ ë¬¼ì´ ?ˆì „ êµ¬ì—­???„ë‹¬?ˆìŠµ?ˆë‹¤. ?¤ìŒ ?‰êµ°??ì¤€ë¹„í•˜??‹œ??";
            public const string M03Lose = "?±ìœ ë¬¼ì´ ?Œê´´?˜ì—ˆ?µë‹ˆ?? ???ì‹¤?€ êµë‹¨ ?°ê°??ê¸°ë¡?©ë‹ˆ??";

            public const string M04Brief =
                "?‘ì „ëª? ?´ë‹¨ ë³¸ê±°ì§€ ?¬ë©¸\n\n??ì½”ì–´?€??ë³„ê°œë¡? ë¶„í™ ?œì‹???´ë‹¨ ë³¸ê±°ì§€(ê°•í™” êµ¬ì¡°ë¬?ë¥??œê±°?˜ì‹­?œì˜¤. ë¯¸ë‹ˆë§µì—???„ì¹˜ë¥??•ì¸?????ˆìŠµ?ˆë‹¤.\n?„êµ° ì½”ì–´ê°€ ë¨¼ì? ë¶•ê´´?˜ë©´ ?‘ì „?€ ?¤íŒ¨ë¡?ì¢…ë£Œ?©ë‹ˆ??";
            public const string M04Win = "?´ë‹¨ ë³¸ê±°ì§€ê°€ ë¬´ë„ˆì¡ŒìŠµ?ˆë‹¤. ??êµ¬ì—­???´ë‹¨ ?¤êµ???Šê²¼?µë‹ˆ??";
            public const string M04Lose = "?„êµ° ì½”ì–´ê°€ ë¶•ê´´?ˆìŠµ?ˆë‹¤. ?±ìŠ¤?¬ìš´ ë°˜ê²©??ì¤€ë¹„í•˜??‹œ??";

            public const string M05Brief =
                "?‘ì „ëª? ê±°ì  ?ë ¹(?¤í…)\n\në³´ë¼???ë ¹ êµ¬ì—­???¼ì • ?œê°„ ? ì??˜ë©´ ?¹ë¦¬?©ë‹ˆ??\n?„êµ° ì½”ì–´ê°€ ë¨¼ì? ë¶•ê´´?˜ë©´ ?¤íŒ¨?…ë‹ˆ??";
            public const string M05Win = "ê±°ì ??êµë‹¨???ì— ?˜ì–´?”ìŠµ?ˆë‹¤. ?¤í… ë¯¸ì…˜ ?´ë¦¬??";
            public const string M05Lose = "?„êµ° ì½”ì–´ê°€ ë¶•ê´´?ˆìŠµ?ˆë‹¤. ?¬ì •ë¹????¤ì‹œ ?„ì „?˜ì‹­?œì˜¤.";
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
                "?ì„±Â·?°ê²° ?„ë£Œ.\n" + DialoguePath + "\në¯¸ì…˜ 5ì¢?+ ì¹´íƒˆë¡œê·¸ ??CampaignMenu",
                "?•ì¸");
        }

        /// <summary>ê¸°ì¡´ ?€?¬í‘œ??m04 ???„ë½ IDë§?ì¶”ê?(???„ë¡œ?íŠ¸ ?…ê·¸?ˆì´?œìš©).</summary>
        [MenuItem("Game/Campaign/Append Missing KR Dialogue Entries")]
        private static void AppendMissingKrDialogue()
        {
            DialogueTable d = AssetDatabase.LoadAssetAtPath<DialogueTable>(DialoguePath);
            if (d == null)
            {
                EditorUtility.DisplayDialog(
                    "Campaign",
                    "?€?¬í‘œê°€ ?†ìŠµ?ˆë‹¤.\nGame/Campaign/Generate Campaign Demo Content (KR) ë¥?ë¨¼ì? ?¤í–‰?˜ì„¸??",
                    "?•ì¸");
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
            EditorUtility.DisplayDialog("Campaign", "?„ë½??KR ?€????ª©??ì¶”ê??ˆìŠµ?ˆë‹¤ (?†ì„ ?Œë§Œ).", "?•ì¸");
        }

        [MenuItem("Game/Campaign/Set Play Mode Start Scene ??CampaignMenu")]
        private static void SetPlayModeStart()
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                EditorUtility.DisplayDialog("?¤ë¥˜", "?¬ì„ ì°¾ì„ ???†ìŠµ?ˆë‹¤: " + ScenePath, "?•ì¸");
                return;
            }

            EditorSceneManager.playModeStartScene = scene;
            EditorUtility.DisplayDialog("Play Mode", "Play ë²„íŠ¼ ??CampaignMenu ë¶€???œì‘?©ë‹ˆ??", "?•ì¸");
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
                "1. ?±ìŠ¤?¬ìš´ ?¤ì»¤ë¯¸ì‹œ ???´ë‹¨ ì½”ì–´ ?¬ë©¸",
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
                "2. ?±ì—­ ë°©ì–´ ???œí•œ ?œê°„ ?ì¡´",
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
                "3. ?±ìœ ë¬??¸ìœ„ ???¹ìƒ‰ ëª©í‘œ êµ¬ì—­",
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
                "5. ê±°ì  ?ë ¹ ???™ë? ?¥ì•  êµ¬ì—­",
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
