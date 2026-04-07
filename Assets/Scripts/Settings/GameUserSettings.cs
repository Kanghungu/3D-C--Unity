using Game.Audio;
using UnityEngine;

namespace Game.Settings
{
    /// <summary>
    /// 마스터 볼륨·카메라 감도·전체화면 — PlayerPrefs 저장.
    /// </summary>
    public static class GameUserSettings
    {
        private const string KeyMasterVol = "gs_master_vol";
        private const string KeyCamSens = "gs_cam_sens";
        private const string KeyFullscreen = "gs_fullscreen";
        private const string KeyMinimapScale = "gs_minimap_scale";
        private const string KeyUiScale = "gs_ui_scale";
        private const string KeyBattleAmbientVol = "gs_battle_ambient_vol";
        private const string KeyResultStingVol = "gs_result_sting_vol";

        public const float DefaultMasterVolume = 0.82f;
        public const float DefaultCameraSensitivity = 1f;
        public const float DefaultMinimapScale = 1f;
        public const float DefaultUiScale = 1f;

        public const float DefaultBattleAmbientVolume = 0.9f;

        public const float DefaultResultStingVolume = 0.95f;

        public static float MasterVolume01 { get; private set; } = DefaultMasterVolume;

        /// <summary>AudioMixer Expose 이름 BattleAmbientVol — 전투 앰비언트 루프</summary>
        public static float BattleAmbientVolume01 { get; private set; } = DefaultBattleAmbientVolume;

        /// <summary>AudioMixer Expose 이름 ResultStingVol — 승·패 스팅</summary>
        public static float ResultStingVolume01 { get; private set; } = DefaultResultStingVolume;

        public static float CameraSensitivityMultiplier { get; private set; } = DefaultCameraSensitivity;

        /// <summary>미니맵 한 변 픽셀 배율(0.75~1.35) — BattleAcesMinimap 에서 사용</summary>
        public static float MinimapScale01 { get; private set; } = DefaultMinimapScale;

        /// <summary>IMGUI 전체 스케일(0.75~1.35) — ImGuiGameUi.BeginScaledGui</summary>
        public static float UiScale01 { get; private set; } = DefaultUiScale;

        static GameUserSettings()
        {
            Load();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyOnBoot()
        {
            Load();
            AudioListener.volume = MasterVolume01;
            int fs = PlayerPrefs.GetInt(KeyFullscreen, 0);
            Screen.fullScreen = fs == 1;
            ProceduralAudioUtility.PushMixerVolumesFromUserSettings();
        }

        public static void Load()
        {
            MasterVolume01 = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMasterVol, DefaultMasterVolume));
            CameraSensitivityMultiplier = Mathf.Clamp(
                PlayerPrefs.GetFloat(KeyCamSens, DefaultCameraSensitivity),
                0.35f,
                2.5f);
            MinimapScale01 = Mathf.Clamp(
                PlayerPrefs.GetFloat(KeyMinimapScale, DefaultMinimapScale),
                0.75f,
                1.35f);
            UiScale01 = Mathf.Clamp(
                PlayerPrefs.GetFloat(KeyUiScale, DefaultUiScale),
                0.75f,
                1.35f);
            BattleAmbientVolume01 = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyBattleAmbientVol, DefaultBattleAmbientVolume));
            ResultStingVolume01 = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyResultStingVol, DefaultResultStingVolume));
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(KeyMasterVol, MasterVolume01);
            PlayerPrefs.SetFloat(KeyCamSens, CameraSensitivityMultiplier);
            PlayerPrefs.SetFloat(KeyMinimapScale, MinimapScale01);
            PlayerPrefs.SetFloat(KeyUiScale, UiScale01);
            PlayerPrefs.SetFloat(KeyBattleAmbientVol, BattleAmbientVolume01);
            PlayerPrefs.SetFloat(KeyResultStingVol, ResultStingVolume01);
            PlayerPrefs.SetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = MasterVolume01;
            ProceduralAudioUtility.PushMixerVolumesFromUserSettings();
        }

        public static void SetMasterVolume01(float value)
        {
            MasterVolume01 = Mathf.Clamp01(value);
            AudioListener.volume = MasterVolume01;
        }

        public static void SetCameraSensitivity(float multiplier)
        {
            CameraSensitivityMultiplier = Mathf.Clamp(multiplier, 0.35f, 2.5f);
        }

        public static void SetMinimapScale(float scale01)
        {
            MinimapScale01 = Mathf.Clamp(scale01, 0.75f, 1.35f);
        }

        public static void SetUiScale01(float scale01)
        {
            UiScale01 = Mathf.Clamp(scale01, 0.75f, 1.35f);
        }

        public static void SetBattleAmbientVolume01(float value01)
        {
            BattleAmbientVolume01 = Mathf.Clamp01(value01);
            ProceduralAudioUtility.PushMixerVolumesFromUserSettings();
        }

        public static void SetResultStingVolume01(float value01)
        {
            ResultStingVolume01 = Mathf.Clamp01(value01);
            ProceduralAudioUtility.PushMixerVolumesFromUserSettings();
        }

        public static void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }
    }
}
