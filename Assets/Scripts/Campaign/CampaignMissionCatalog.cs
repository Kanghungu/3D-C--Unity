using Game.Campaign.Data;
using UnityEngine;

namespace Game.Campaign
{
    /// <summary>
    /// 캠페인에 등장하는 미션 목록(순서 = 해금 순서와 동일하게 배치).
    /// </summary>
    [CreateAssetMenu(fileName = "CampaignMissionCatalog", menuName = "Game/Campaign/Mission Catalog", order = 3)]
    public sealed class CampaignMissionCatalog : ScriptableObject
    {
        [SerializeField] private MissionDefinition[] missionsInOrder;

        public MissionDefinition[] MissionsInOrder => missionsInOrder;

        public int Count => missionsInOrder != null ? missionsInOrder.Length : 0;

        public MissionDefinition GetMissionAt(int index)
        {
            if (missionsInOrder == null || index < 0 || index >= missionsInOrder.Length)
            {
                return null;
            }

            return missionsInOrder[index];
        }
    }
}
