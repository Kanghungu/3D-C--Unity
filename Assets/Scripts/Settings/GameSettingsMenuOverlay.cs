using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Settings
{
    /// <summary>
    /// 캠페인 메뉴에서 O 로 간단 설정(마스터 볼륨·카메라 감도·전체화면).
    /// </summary>
    public sealed class GameSettingsMenuOverlay : MonoBehaviour
    {
        private bool panelOpen;

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.oKey.wasPressedThisFrame)
            {
                panelOpen = !panelOpen;
            }
        }

        private void OnGUI()
        {
            if (!panelOpen)
            {
                return;
            }

            GUI.Box(new Rect(24f, Screen.height - 220f, 420f, 200f), "설정 (O 닫기)");
            float y = Screen.height - 196f;

            GUI.Label(new Rect(40f, y, 200f, 24f), "마스터 볼륨");
            float vol = GUI.HorizontalSlider(new Rect(200f, y + 4f, 200f, 20f), GameUserSettings.MasterVolume01, 0f, 1f);
            if (!Mathf.Approximately(vol, GameUserSettings.MasterVolume01))
            {
                GameUserSettings.SetMasterVolume01(vol);
            }

            y += 40f;
            GUI.Label(new Rect(40f, y, 200f, 24f), "카메라 감도");
            float sens = GUI.HorizontalSlider(new Rect(200f, y + 4f, 200f, 20f), GameUserSettings.CameraSensitivityMultiplier, 0.35f, 2.5f);
            if (!Mathf.Approximately(sens, GameUserSettings.CameraSensitivityMultiplier))
            {
                GameUserSettings.SetCameraSensitivity(sens);
            }

            y += 40f;
            bool fs = Screen.fullScreen;
            bool newFs = GUI.Toggle(new Rect(40f, y, 360f, 24f), fs, "전체 화면");
            if (newFs != fs)
            {
                GameUserSettings.SetFullscreen(newFs);
            }

            y += 36f;
            if (GUI.Button(new Rect(40f, y, 160f, 28f), "설정 저장"))
            {
                GameUserSettings.Save();
            }
        }
    }
}
