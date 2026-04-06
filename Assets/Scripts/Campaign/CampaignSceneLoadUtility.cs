using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Campaign
{
    /// <summary>
    /// 빌드에 없는 씬을 LoadScene 하면 런타임 예외가 나므로, 호출 전 검사(집에서 잡기 어려운 오류 예방).
    /// </summary>
    public static class CampaignSceneLoadUtility
    {
        /// <summary>씬 이름이 빌드에 있으면 로드하고 true. 아니면 로그만 남기고 false.</summary>
        public static bool TryLoadSceneByName(string sceneName, string logContext)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[Campaign] 씬 이름이 비어 있습니다. " + logContext);
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    "[Campaign] 빌드 설정에 씬이 없거나 이름이 다릅니다: '" + sceneName +
                    "'. File → Build Settings 에 추가했는지 확인하세요. " + logContext);
                return false;
            }

            SceneManager.LoadScene(sceneName);
            return true;
        }
    }
}
