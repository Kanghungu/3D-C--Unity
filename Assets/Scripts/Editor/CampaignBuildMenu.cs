#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// 빌드 설정에 캠페인 메뉴 + 전투 씬 등록(한 번 실행).
    /// </summary>
    public static class CampaignBuildMenu
    {
        private const string MenuRegister = "Game/Campaign/Register Campaign Scenes In Build";
        private const string MenuVerify = "Game/Campaign/Verify Chapter 1 Build Scenes";

        [MenuItem(MenuRegister)]
        private static void RegisterScenes()
        {
            string[] required = CampaignChapter1BuildPaths.RequiredScenes;
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(required[0], true),
                new EditorBuildSettingsScene(required[1], true),
            };

            Debug.Log("[CampaignBuildMenu] Build Settings 에 CampaignMenu, NewSampleScene 이 등록되었습니다.");
        }

        /// <summary>챕터 1 — 빌드 목록·파일 존재·활성화 검사(로그만, 자동 수정 없음).</summary>
        [MenuItem(MenuVerify)]
        private static void VerifyBuildScenes()
        {
            bool allOk = true;
            foreach (string path in CampaignChapter1BuildPaths.RequiredScenes)
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    Debug.LogError($"[Chapter1] 씬 파일 없음: {path}");
                    allOk = false;
                    continue;
                }
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("[Chapter1] Build Settings 씬 목록이 비어 있습니다. 「Register Campaign Scenes In Build」 실행 권장.");
                allOk = false;
            }
            else
            {
                string[] chapter1 = CampaignChapter1BuildPaths.RequiredScenes;
                for (int i = 0; i < chapter1.Length; i++)
                {
                    string required = chapter1[i];
                    bool foundEnabled = false;
                    for (int s = 0; s < scenes.Length; s++)
                    {
                        if (scenes[s].path != required)
                        {
                            continue;
                        }

                        if (!scenes[s].enabled)
                        {
                            Debug.LogWarning($"[Chapter1] 씬이 빌드에서 꺼져 있음(체크 해제): {required}");
                            allOk = false;
                        }
                        else
                        {
                            foundEnabled = true;
                        }

                        break;
                    }

                    if (!foundEnabled)
                    {
                        Debug.LogError($"[Chapter1] Build Settings 에 없거나 비활성: {required} — Register 메뉴로 추가하세요.");
                        allOk = false;
                    }
                }
            }

            if (allOk)
            {
                Debug.Log("[Chapter1] Build Scenes 검사 통과: CampaignMenu, NewSampleScene 경로 존재·목록 포함·활성화됨.");
            }
        }
    }
}
#endif
