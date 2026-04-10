#if UNITY_EDITOR
using System.IO;
using Game.EditorTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>빌드 씬 경로와 존재 여부를 빠르게 점검하는 보조 메뉴입니다.</summary>
    public static class ProjectSmokeChecks
    {
        /// <summary>CampaignBuildMenu와 동일한 Chapter 1 씬 목록을 검증합니다.</summary>
        [MenuItem("Tools/Project/Verify Chapter 1 Build Scenes")]
        public static void VerifyChapter1PathsMatchCampaignMenu()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            bool allOk = true;
            foreach (string relative in CampaignChapter1BuildPaths.RequiredScenes)
            {
                string rel = relative.Replace("\\", "/");
                string fullPath = Path.GetFullPath(Path.Combine(projectRoot, rel));
                if (!File.Exists(fullPath))
                {
                    Debug.LogError($"[Chapter1 Demo] Missing file: {rel}");
                    allOk = false;
                }
                else
                {
                    Debug.Log($"[Chapter1 Demo OK] {rel}");
                }
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("[Chapter1 Demo] Build Settings scene list is empty. Run Register Campaign Scenes In Build.");
                allOk = false;
            }
            else
            {
                foreach (string required in CampaignChapter1BuildPaths.RequiredScenes)
                {
                    string requiredNorm = required.Replace("\\", "/");
                    bool foundEnabled = false;
                    for (int s = 0; s < scenes.Length; s++)
                    {
                        string scenePathNorm = scenes[s].path.Replace("\\", "/");
                        if (scenePathNorm != requiredNorm)
                        {
                            continue;
                        }

                        if (scenes[s].enabled)
                        {
                            foundEnabled = true;
                        }

                        break;
                    }

                    if (!foundEnabled)
                    {
                        Debug.LogError($"[Chapter1 Demo] Scene missing or disabled in build settings: {required}");
                        allOk = false;
                    }
                }
            }

            Debug.Log(allOk
                ? "[Chapter1 Demo] CampaignChapter1BuildPaths verification passed."
                : "[Chapter1 Demo] Fix the errors above, then rerun Game/Campaign/Verify Chapter 1 Build Scenes.");
        }

        [MenuItem("Tools/Project/Verify Build Scene List")]
        public static void LogBuildScenesHealth()
        {
            int ok = 0;
            int fail = 0;
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            {
                if (!s.enabled)
                {
                    continue;
                }

                string relative = s.path.Replace("\\", "/");
                string fullPath = Path.GetFullPath(Path.Combine(projectRoot, relative));
                bool exists = File.Exists(fullPath);
                if (exists)
                {
                    Debug.Log($"[빌드 씬 OK] {relative}");
                    ok++;
                }
                else
                {
                    Debug.LogError($"[빌드 씬 없음] {relative} → {fullPath}");
                    fail++;
                }
            }

            Debug.Log($"[빌드 씬 검사] 성공 {ok}건, 실패 {fail}건. 실패 시 Build Settings 에서 경로를 확인하세요.");
        }
    }
}
#endif
