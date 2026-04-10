// =============================================================================
// [Scripts 위치: Battle Aces]
// - 캠페인 메뉴·전투 씬 공통: 첫 씬 로드 후 Player.log 한 줄 [DemoBoot](스모크·버전 비교).
// =============================================================================
using System.Globalization;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// Windows 빌드 스모크 시 Player.log 에 한 줄 남김 — 버전·씬·해상도·그래픽 프리셋 등.
    /// </summary>
    public static class BattleAcesBootDiagnostics
    {
        private static bool bootLineLogged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LogOnceAfterFirstSceneLoad()
        {
            if (bootLineLogged)
            {
                return;
            }

            bootLineLogged = true;
            Scene s = SceneManager.GetActiveScene();
            string missionId = string.Empty;
            PersistentGameCore core = PersistentGameCore.Instance;
            if (core != null && core.ActiveMission != null)
            {
                MissionDefinition m = core.ActiveMission;
                missionId = string.IsNullOrEmpty(m.MissionId) ? "(no id)" : m.MissionId;
            }

            bool battleAcesScene =
                Object.FindAnyObjectByType<BattleAcesSceneBootstrapper>(FindObjectsInactive.Include) != null;
            string battleFields = string.Empty;
            if (battleAcesScene)
            {
                // 집 PC·에디터 간 설정 동일 여부 빠르게 비교(Invariant — 로그 파싱 안정)
                IFormatProvider inv = CultureInfo.InvariantCulture;
                battleFields =
                    " battleAces=1" +
                    " uiScale=" + GameUserSettings.UiScale01.ToString("0.###", inv) +
                    " battleFogInt=" + GameUserSettings.BattleFogIntensity01.ToString("0.###", inv) +
                    " battleFogDist=" + GameUserSettings.BattleFogDistanceScale.ToString("0.###", inv) +
                    " minimapScale=" + GameUserSettings.MinimapScale01.ToString("0.###", inv) +
                    " lang=" + GameUserSettings.Language;
            }

            // 스모크 후 로그에서 [DemoBoot] 로 검색하면 동일 빌드 비교가 쉬움
            Debug.Log(
                "[DemoBoot] " +
                "playerVer=" + Application.version +
                " unity=" + Application.unityVersion +
                " product=\"" + Application.productName + "\"" +
                " scene=" + s.name +
                " res=" + Screen.width + "x" + Screen.height +
                " fullscreen=" + Screen.fullScreen +
                " platform=" + Application.platform +
                " devBuild=" + Debug.isDebugBuild +
                " editor=" + Application.isEditor +
                " gfxPreset=" + GameUserSettings.GraphicQualityPreset +
                " activeMissionId=" + (string.IsNullOrEmpty(missionId) ? "(none)" : missionId) +
                battleFields);
        }
    }
}
