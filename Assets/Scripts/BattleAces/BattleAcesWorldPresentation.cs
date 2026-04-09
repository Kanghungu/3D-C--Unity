using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// 메뉴 ↔ 전투 씬 전환 시 <see cref="RenderSettings"/>·메인 카메라 클리어를 한곳에서 맞춤.
    /// 전투 연출은 <see cref="BattleAcesDemoStagePresentation"/>·부트스트랩이 덮어씀.
    /// </summary>
    public static class BattleAcesWorldPresentation
    {
        private const string CampaignMenuSceneName = "CampaignMenu";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadedHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == CampaignMenuSceneName)
            {
                ApplyMenuWorldPresentation();
            }
        }

        /// <summary>캠페인 메뉴 진입 시 — 전투 데모 안개·스크린 톤·트라이라이트를 걷고 UI 친화 톤으로</summary>
        public static void ApplyMenuWorldPresentation()
        {
            BattleAcesDemoStagePresentation.RemoveDemoPostEffectsFromMainCamera();
            BattleAcesDemoStagePresentation.ClearClassicDuelAtmosphereCache();

            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = BattleAcesArtDirection.AmbientFlatNeutral;
            RenderSettings.ambientIntensity = 1f;

            Camera main = Camera.main;
            if (main == null)
            {
                return;
            }

            // 메뉴는 IMGUI 풀스크린 위에 얹이므로 배경은 차분한 단색(씬에 스카이박스가 있어도 일관되게)
            main.clearFlags = CameraClearFlags.SolidColor;
            main.backgroundColor = new Color(0.1f, 0.12f, 0.16f, 1f);
        }

        /// <summary>설정 패널에서 안개 슬라이더 조작 시 — 현재 ClassicDuel 전투면 즉시 반영</summary>
        public static void RefreshBattleFogIfClassicDuelActive()
        {
            BattleAcesDemoStagePresentation.RefreshClassicDuelAtmosphereFromUserSettings();
        }
    }
}
