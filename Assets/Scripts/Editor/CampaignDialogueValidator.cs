#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Campaign.Data;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// CampaignDialogue_KR ??ÎØ∏ÏÖò¬∑?ÑÎìú ?Ä??IDÍ∞Ä Îπ†Ï°å?îÏ? Í≤Ä???ÑÎ°ú???†Ï?Î≥¥Ïàò??.
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
                EditorUtility.DisplayDialog("Dialogue", "CampaignDialogue_KR.asset ??Ï∞æÏùÑ ???ÜÏäµ?àÎã§.", "?ïÏù∏");
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
                        missing.Add($"{m.MissionId} ??{label}: [{id}]");
                    }
                }

                Check(m.BriefingDialogueId, "brief");
                Check(m.VictoryDialogueId, "win");
                Check(m.DefeatDialogueId, "lose");
            }

            string[] fieldIds =
            {
                "m01_field_1", "m01_field_2", "m02_field_1", "m03_field_1", "m03_field_2",
                "m04_field_1", "m04_field_2", "m05_field_1"
            };

            foreach (string fid in fieldIds)
            {
                if (!ids.Contains(fid))
                {
                    missing.Add($"?ÑÎìú ?Ä???§ÌÜ†Î¶?Ï°? ??[{fid}]");
                }
            }

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Dialogue Check", "No missing dialogue IDs found.", "OK");
                return;
            }

            Debug.LogWarning("[CampaignDialogueValidator] ?ÑÎùΩ:\n" + string.Join("\n", missing));
            EditorUtility.DisplayDialog(
                "Dialogue Check",
                "Found " + missing.Count + " missing dialogue IDs. Check the console warning.",
                "OK");
        }
    }
}
#endif
