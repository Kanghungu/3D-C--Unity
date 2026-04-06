#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Campaign.Data;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// CampaignDialogue_KR 에 미션·필드 대사 ID가 빠졌는지 검사(프로토 유지보수용).
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
                EditorUtility.DisplayDialog("Dialogue", "CampaignDialogue_KR.asset 을 찾을 수 없습니다.", "확인");
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
                        missing.Add($"{m.MissionId} → {label}: [{id}]");
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
                    missing.Add($"필드 대사(스토리 존) → [{fid}]");
                }
            }

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Dialogue 검사", "누락된 ID 없음.", "확인");
                return;
            }

            Debug.LogWarning("[CampaignDialogueValidator] 누락:\n" + string.Join("\n", missing));
            EditorUtility.DisplayDialog(
                "Dialogue 검사",
                "누락 " + missing.Count + "건 — 콘솔 경고를 확인하세요.",
                "확인");
        }
    }
}
#endif
