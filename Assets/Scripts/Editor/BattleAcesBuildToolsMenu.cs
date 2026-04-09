#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// 집에서 스모크 기록용 — 클립보드·BUILD_SMOKE_LOG.md 행 추가(Battle Aces 빌드 흐름과 동일 메뉴 그룹).
    /// </summary>
    public static class BattleAcesBuildToolsMenu
    {
        private const string MenuCopy = "Game/Battle Aces/스모크 로그 한 줄 복사 (콘솔+클립보드)";
        private const string MenuAppend = "Game/Battle Aces/BUILD_SMOKE_LOG.md 에 오늘 행 추가";

        private static string SmokeLogPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "Docs", "BUILD_SMOKE_LOG.md"));

        [MenuItem(MenuCopy)]
        private static void CopySmokeLine()
        {
            string line = BuildSmokeTableRow(string.Empty);
            EditorGUIUtility.systemCopyBuffer = line;
            UnityEngine.Debug.Log("[DemoSmoke] 클립보드에 복사됨 → " + line);
        }

        [MenuItem(MenuAppend)]
        private static void AppendSmokeRowToDoc()
        {
            string path = SmokeLogPath;
            string row = BuildSmokeTableRow("에디터에서 행 추가");
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, BuildSmokeDocHeader(), Encoding.UTF8);
                }

                File.AppendAllText(path, row + Environment.NewLine, Encoding.UTF8);
                UnityEngine.Debug.Log("[DemoSmoke] 추가됨: " + path + Environment.NewLine + row);
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[DemoSmoke] 파일 쓰기 실패: " + ex.Message);
            }
        }

        private static string BuildSmokeDocHeader()
        {
            return
                "# Windows 빌드 스모크 기록\r\n\r\n" +
                "Player.log 에서 `[DemoBoot]` 검색 — 동일 빌드 비교용(devBuild·editor·gfxPreset 포함).\r\n" +
                "개발 빌드: F10 진단 HUD, F11 FoW 격자(`BattleAcesFogOfWarDebug`).\r\n\r\n" +
                "| 날짜 | PlayerSettings 버전 | 짧은 메모 | 결과 | 비고 |\r\n" +
                "|------|----------------------|-----------|------|------|\r\n";
        }

        private static string BuildSmokeTableRow(string memoFallback)
        {
            string ver = PlayerSettings.bundleVersion;
            if (string.IsNullOrWhiteSpace(ver))
            {
                ver = "(no bundleVersion)";
            }

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string git = TryGetShortGitHash();
            string memo = string.IsNullOrWhiteSpace(memoFallback)
                ? (string.IsNullOrEmpty(git) ? "—" : "git " + git)
                : memoFallback;
            const string result = "PASS";
            const string note = "수동 기록";
            return "| " + date + " | " + ver + " | " + memo + " | " + result + " | " + note + " |";
        }

        /// <summary>저장소 루트에서 짧은 해시만 시도 — 실패 시 빈 문자열.</summary>
        private static string TryGetShortGitHash()
        {
            try
            {
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --short HEAD",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                Process p = Process.Start(psi);
                if (p == null)
                {
                    return string.Empty;
                }

                try
                {
                    // rev-parse 는 한 줄·즉시 종료 — ReadToEnd 가 종료까지 블록(데드락 방지: 출력 버퍼 작음)
                    string stdout = p.StandardOutput.ReadToEnd().Trim();
                    p.WaitForExit();
                    if (p.ExitCode != 0 || string.IsNullOrEmpty(stdout))
                    {
                        return string.Empty;
                    }

                    return stdout;
                }
                finally
                {
                    p.Dispose();
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
#endif
