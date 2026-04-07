#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>빌드 전 씬 경로·존재 여부 스모크 검사(A-6 보조).</summary>
    public static class ProjectSmokeChecks
    {
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
