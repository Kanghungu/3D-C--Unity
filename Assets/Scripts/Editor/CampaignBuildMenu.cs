#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// 빌드 설정에 캠페인 메뉴 + 전투 씬 등록(한 번 실행).
    /// </summary>
    public static class CampaignBuildMenu
    {
        private const string MenuPath = "Game/Campaign/Register Campaign Scenes In Build";

        [MenuItem(MenuPath)]
        private static void RegisterScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/CampaignMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/NewSampleScene.unity", true),
            };

            Debug.Log("[CampaignBuildMenu] Build Settings 에 CampaignMenu, NewSampleScene 이 등록되었습니다.");
        }
    }
}
#endif
