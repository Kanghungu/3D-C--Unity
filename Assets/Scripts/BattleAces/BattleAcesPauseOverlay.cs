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
            float panelH = Mathf.Min(260f, Screen.height * 0.38f);
            Rect box = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.34f, w, panelH);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.HudStripeTactical, 2f);

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 16f, box.y + 12f, box.width - 32f, 26f), "일시정지 (P로 재개)");
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            // 결과 카드와 동일 순서: R → Esc → 전투 중 조작
            string body =
                DemoPresentationCopy.BuildPauseAndResultInputBlockKo() + "\n\n" +
                "전투 조작\n" +
                "· 이동: 지면 우클릭   · 공격: 적 우클릭\n" +
                "· 집결: Alt + 우클릭   · 전술 지도: 우하단 클릭/드래그\n" +
                "· 시간: [ ] 배속   · 도움: F1   · 설정: O"
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                + "\n· 진단 패널: F10 (에디터·개발 빌드만)"
#endif
                ;
            GUI.Label(new Rect(box.x + 16f, box.y + 40f, box.width - 32f, panelH - 48f), body);
            GUI.color = Color.white;

            ImGuiGameUi.EndScaledGui();
        }
    }
}
