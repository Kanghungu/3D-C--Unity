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

        public const float DefaultMasterVolume = 0.82f;
        public const float DefaultCameraSensitivity = 1f;

        public static float MasterVolume01 { get; private set; } = DefaultMasterVolume;

        public static float CameraSensitivityMultiplier { get; private set; } = DefaultCameraSensitivity;

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
        }

        public static void Load()
        {
            MasterVolume01 = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMasterVol, DefaultMasterVolume));
            CameraSensitivityMultiplier = Mathf.Clamp(
                PlayerPrefs.GetFloat(KeyCamSens, DefaultCameraSensitivity),
                0.35f,
                2.5f);
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(KeyMasterVol, MasterVolume01);
            PlayerPrefs.SetFloat(KeyCamSens, CameraSensitivityMultiplier);
            PlayerPrefs.SetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = MasterVolume01;
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

        public static void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }
    }
}
