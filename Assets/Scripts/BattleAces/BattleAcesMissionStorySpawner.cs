using Game.Campaign.Data;
using Game.Campaign.Scene;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 캠페인 미션 중 전장에 스토리 트리거 박스를 런타임 배치한다(씬 YAML 수동 편집 없이 검증 가능).
    /// </summary>
    public static class BattleAcesMissionStorySpawner
    {
        /// <summary>아군·적 코어 사이에 최대 2개 트리거 — 대사 ID는 DialogueTable 에 있어야 함</summary>
        public static void SpawnForMission(MissionDefinition mission, Transform parent, Vector3 playerCorePos, Vector3 enemyCorePos)
        {
            if (mission == null || parent == null)
            {
                return;
            }

            (string id1, string id2) = ResolveFieldDialogueIds(mission);
            if (string.IsNullOrEmpty(id1))
            {
                return;
            }

            Vector3 mid = Vector3.Lerp(playerCorePos, enemyCorePos, 0.38f);
            Vector3 nearEnemy = Vector3.Lerp(playerCorePos, enemyCorePos, 0.72f);
            mid.y = 2.5f;
            nearEnemy.y = 2.5f;

            SpawnTrigger(parent, mid, new Vector3(14f, 5f, 14f), id1);
            if (!string.IsNullOrEmpty(id2))
            {
                SpawnTrigger(parent, nearEnemy, new Vector3(12f, 5f, 12f), id2);
            }
        }

        private static (string, string) ResolveFieldDialogueIds(MissionDefinition mission)
        {
            if (mission == null || string.IsNullOrEmpty(mission.MissionId))
            {
                return (null, null);
            }

            switch (mission.MissionId)
            {
                case "mission_01_skirmish":
                    return ("m01_field_1", "m01_field_2");
                case "mission_02_sanctuary":
                    return ("m02_field_1", "m02_field_2");
                case "mission_03_escort":
                    return ("m03_field_1", "m03_field_2");
                case "mission_04_heresy":
                    return ("m04_field_1", "m04_field_2");
                case "mission_05_stub":
                    return ("m05_field_1", "m05_field_2");
                case "mission_06_fortress":
                    return mission.AirborneCitadelFocus
                        ? ("m06_field_1", "m06_field_2_air")
                        : ("m06_field_1", "m06_field_2");
                case "mission_07_counter_rush":
                    return ("m07_field_1", "m07_field_2");
                default:
                    return (null, null);
            }
        }

        private static void SpawnTrigger(Transform parent, Vector3 center, Vector3 boxSize, string dialogueId)
        {
            GameObject go = new GameObject("MissionStoryZone_" + dialogueId);
            go.transform.SetParent(parent, false);
            go.transform.position = center;

            BoxCollider box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = boxSize;
            box.center = Vector3.zero;

            MissionStoryTrigger trigger = go.AddComponent<MissionStoryTrigger>();
            trigger.SetDialogueIdForRuntime(dialogueId);
        }
    }
}
