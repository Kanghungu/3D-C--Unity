using Game.Campaign.Data;
using UnityEngine;

namespace Game.Campaign
{
    /// <summary>
    /// 캠페인 진행 — 어디까지 해금됐는지만 저장(완전 세이브 아님).
    /// </summary>
    public static class CampaignProgressStorage
    {
        private const string KeyHighestUnlockedIndex = "campaign_highest_unlocked_mission_index";
        private const string KeyCompletedPrefix = "campaign_completed_";

        public static int GetHighestUnlockedMissionIndex()
        {
            return PlayerPrefs.GetInt(KeyHighestUnlockedIndex, 0);
        }

        public static void SetHighestUnlockedMissionIndex(int index)
        {
            int cur = GetHighestUnlockedMissionIndex();
            if (index > cur)
            {
                PlayerPrefs.SetInt(KeyHighestUnlockedIndex, index);
                PlayerPrefs.Save();
            }
        }

        public static bool IsMissionCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
            {
                return false;
            }

            return PlayerPrefs.GetInt(KeyCompletedPrefix + missionId, 0) == 1;
        }

        public static void MarkMissionCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
            {
                return;
            }

            PlayerPrefs.SetInt(KeyCompletedPrefix + missionId, 1);
            PlayerPrefs.Save();
        }

        /// <summary>미션 클리어 시 호출 — 해금 인덱스 + 완료 플래그</summary>
        public static void RegisterMissionWin(int missionOrderIndex, string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
            {
                Debug.LogWarning("[Campaign] missionId 가 비어 있어 진행 저장을 건너뜁니다.");
                return;
            }

            if (missionOrderIndex < 0)
            {
                Debug.LogWarning("[Campaign] missionOrderIndex 가 음수입니다. 진행 저장을 건너뜁니다.");
                return;
            }

            MarkMissionCompleted(missionId);
            SetHighestUnlockedMissionIndex(missionOrderIndex + 1);
        }

        /// <summary>해금 인덱스 초기화. catalog 가 있으면 미션별 완료 플래그도 함께 삭제.</summary>
        public static void ClearAllProgress(CampaignMissionCatalog catalog = null)
        {
            PlayerPrefs.DeleteKey(KeyHighestUnlockedIndex);
            if (catalog != null)
            {
                for (int i = 0; i < catalog.Count; i++)
                {
                    MissionDefinition m = catalog.GetMissionAt(i);
                    if (m != null && !string.IsNullOrEmpty(m.MissionId))
                    {
                        PlayerPrefs.DeleteKey(KeyCompletedPrefix + m.MissionId);
                    }
                }
            }

            PlayerPrefs.Save();
        }
    }
}
