using Game.Settings;
using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// P 일시정지 중에만 짧은 조작 요약(배속 줄과 중복되지 않게 별도 패널).
    /// </summary>
    public sealed class BattleAcesPauseOverlay : MonoBehaviour
    {
        private void OnGUI()
        {
            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc == null || !rtc.IsPaused)
            {
                return;
            }

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m) && m.IsFinished)
            {
                return;
            }

            BattleMissionFlow flow = BattleMissionFlow.Instance;
            if (flow != null && (flow.IsBriefingBlocking || flow.IsShowingBattleResult))
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();

            float w = Mathf.Min(420f, Screen.width - 40f);
            float panelH = Mathf.Min(300f, Screen.height * 0.42f);
            Rect box = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.34f, w, panelH);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.HudStripeTactical, 2f);

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 16f, box.y + 12f, box.width - 32f, 26f), DemoPresentationCopy.PausePanelTitleKo);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            // 결과 카드와 동일 순서: 입력(R/Esc) → 전투 조작(문구는 DemoPresentationCopy)
            string body = DemoPresentationCopy.BuildPauseAndResultInputBlockKo() + "\n\n" +
                          DemoPresentationCopy.PauseBattleControlsBody + "\n\n" +
                          DemoPresentationCopy.PauseControlsFixedKeysNotice;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            body += GameUserSettings.Language == GameLanguage.Korean
                ? "\n· 진단 패널: F10 · FoW 격자 미리보기: F11 (에디터·개발 빌드만)"
                : "\n· Diagnostics: F10 · FoW overlay: F11 (editor / dev builds only)";
#endif
            GUI.Label(new Rect(box.x + 16f, box.y + 40f, box.width - 32f, panelH - 48f), body);
            GUI.color = Color.white;

            ImGuiGameUi.EndScaledGui();
        }
    }
}
