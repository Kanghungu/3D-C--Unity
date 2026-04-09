#if UNITY_EDITOR
using System.IO;
using Game.EditorTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>빌드 전 씬 경로·존재 여부 스모크 검사(A-6 보조).</summary>
    public static class ProjectSmokeChecks
    {
        /// <summary>Game/Campaign/Verify Chapter 1 과 동일 목록 — 회귀 시 한 번에 확인.</summary>
        [MenuItem("Tools/프로젝트/챕터1 씬 (CampaignBuildMenu 와 동일)")]
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
                    Debug.LogError($"[Chapter1·스모크] 파일 없음: {rel}");
                    allOk = false;
                }
                else
                {
                    Debug.Log($"[Chapter1·스모크 OK] {rel}");
                }
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("[Chapter1·스모크] Build Settings 씬 목록 비어 있음 — Register Campaign Scenes In Build 권장.");
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
                        Debug.LogError($"[Chapter1·스모크] 빌드에 없거나 비활성: {required}");
                        allOk = false;
                    }
                }
            }

            Debug.Log(allOk
                ? "[Chapter1·스모크] CampaignChapter1BuildPaths 기준 검사 통과."
                : "[Chapter1·스모크] 위 오류를 해결한 뒤 Game/Campaign/Verify Chapter 1 Build Scenes 로 재확인하세요.");
        }

        [MenuItem("Tools/프로젝트/빌드 씬 목록 검사")]
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
