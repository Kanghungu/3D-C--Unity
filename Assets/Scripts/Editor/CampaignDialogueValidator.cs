#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Campaign.Data;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// CampaignDialogue_KR? ????? ?? ID? ???? ????.
    /// </summary>
    public static class CampaignDialogueValidator
    {
        private const string DialoguePath = "Assets/Campaign/Content/CampaignDialogue_KR.asset";

        [MenuItem("Game/Campaign/Validate Dialogue Keys (Missions + Fields)")]
        private static void Validate()
        {
            DialogueTable table = AssetDatabase.LoadAssetAtPath<DialogueTable>(DialoguePath);
            if (table == null)
            {
                EditorUtility.DisplayDialog("Dialogue", "CampaignDialogue_KR.asset? ?? ? ????.", "??");
                return;
            }

            var ids = new HashSet<string>();
            SerializedObject tableSo = new SerializedObject(table);
            SerializedProperty entriesProp = tableSo.FindProperty("entries");
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                string id = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Id").stringValue;
                if (!string.IsNullOrEmpty(id))
                {
                    ids.Add(id);
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:MissionDefinition", new[] { "Assets/Campaign/Content" });
            var missing = new List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MissionDefinition m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
                if (m == null)
                {
                    continue;
                }

                void Check(string id, string label)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    if (!ids.Contains(id))
                    {
                        missing.Add($"{m.MissionId} / {label}: [{id}]");
                    }
                }

                Check(m.BriefingDialogueId, "brief");
                Check(m.VictoryDialogueId, "win");
                Check(m.DefeatDialogueId, "lose");

                // ?? ??: ???? *_air ?? ??? ????? ??? ?
                if (m.AirborneCitadelFocus)
                {
                    if (!string.IsNullOrEmpty(m.BriefingDialogueId))
                    {
                        Check(m.BriefingDialogueId + "_air", "brief_air");
                    }

                    if (!string.IsNullOrEmpty(m.VictoryDialogueId))
                    {
                        Check(m.VictoryDialogueId + "_air", "win_air");
                    }

                    if (!string.IsNullOrEmpty(m.DefeatDialogueId))
                    {
                        Check(m.DefeatDialogueId + "_air", "lose_air");
                    }
                }
            }

            string[] fieldIds =
            {
                "m01_field_1", "m01_field_2", "m02_field_1", "m02_field_2",
                "m03_field_1", "m03_field_2",
                "m04_field_1", "m04_field_2",
                "m05_field_1", "m05_field_2",
                "m06_field_1", "m06_field_2", "m06_field_2_air",
                "m07_field_1", "m07_field_2"
            };

            foreach (string fid in fieldIds)
            {
                if (!ids.Contains(fid))
                {
                    missing.Add("field dialogue: [" + fid + "]");
                }
            }

            string[] hintIds =
            {
                "hint_retry_player_core",
                "hint_retry_player_core_m01",
                "hint_retry_player_core_m02",
                "hint_retry_player_core_m03",
                "hint_retry_player_core_m04",
                "hint_retry_player_core_m05",
                "hint_retry_player_core_m06",
                "hint_retry_player_core_m07",
                "hint_retry_relic_objective",
                "hint_retry_relic_objective_m03",
                "hint_retry_mission_fail",
                "hint_retry_mission_fail_m02",
                "hint_retry_mission_fail_m03",
                "hint_retry_mission_fail_m05",
                "hint_retry_generic"
            };

            foreach (string hid in hintIds)
            {
                if (!ids.Contains(hid))
                {
                    missing.Add("defeat hint: [" + hid + "]");
                }
            }

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Dialogue Check", "??? ?? ID? ????.", "OK");
                return;
            }

            Debug.LogWarning("[CampaignDialogueValidator] ??:\n" + string.Join("\n", missing));
            EditorUtility.DisplayDialog(
                "Dialogue Check",
                "?? " + missing.Count + "?. ?? ??? ?????.",
                "OK");
        }
    }
}
#endif
