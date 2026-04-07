using System;
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

        /// <summary>보조 목표 달성(선택) — 미션 클리어와 별도</summary>
        private const string KeyBonusPrefix = "campaign_bonus_done_";

        /// <summary>캠페인 최초 진입 시 F1 조작·목표 요약 자동 표시(1회) — 0이면 아직 안 봄</summary>
        private const string KeyCampaignF1IntroDone = "campaign_f1_intro_done";

        public static int GetHighestUnlockedMissionIndex()
        {
            return PlayerPrefs.GetInt(KeyHighestUnlockedIndex, 0);
        }

        /// <summary>캠페인에서 첫 전투 진입 시 F1 도움말을 자동으로 띄울지(아직 한 번도 완료 안 했으면 true)</summary>
        public static bool ShouldAutoShowCampaignF1Intro()
        {
            return PlayerPrefs.GetInt(KeyCampaignF1IntroDone, 0) == 0;
        }

        /// <summary>플레이어가 첫 F1 튜토리얼 패널을 닫은 뒤 호출</summary>
        public static void MarkCampaignF1IntroCompleted()
        {
            try
            {
                PlayerPrefs.SetInt(KeyCampaignF1IntroDone, 1);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Campaign] F1 안내 진행 저장 실패(무시 가능): " + e.Message);
            }
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

        public static bool IsBonusObjectiveCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
            {
                return false;
            }

            return PlayerPrefs.GetInt(KeyBonusPrefix + missionId, 0) == 1;
        }

        public static void MarkBonusObjectiveCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
            {
                return;
            }

            PlayerPrefs.SetInt(KeyBonusPrefix + missionId, 1);
            PlayerPrefs.Save();
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
            PlayerPrefs.DeleteKey(KeyCampaignF1IntroDone);
            if (catalog != null)
            {
                for (int i = 0; i < catalog.Count; i++)
                {
                    MissionDefinition m = catalog.GetMissionAt(i);
                    if (m != null && !string.IsNullOrEmpty(m.MissionId))
                    {
                        PlayerPrefs.DeleteKey(KeyCompletedPrefix + m.MissionId);
                        PlayerPrefs.DeleteKey(KeyBonusPrefix + m.MissionId);
                    }
                }
            }

            PlayerPrefs.Save();
        }
    }
}
