using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>F1 로 간단 조작 도움말(IMGUI).</summary>
    public sealed class BattleAcesInGameHelp : MonoBehaviour
    {
        private bool visible;

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            // 승패 오버레이 중에는 F1 토글로 UI가 겹치지 않게 함
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController baMatch) && baMatch.IsFinished)
            {
                visible = false;
                return;
            }

            // 작전 브리핑 전체화면 중에는 도움말 비활성
            if (CampaignBattleFlow.Instance != null && CampaignBattleFlow.Instance.IsBriefingBlocking)
            {
                visible = false;
                return;
            }

            if (kb.f1Key.wasPressedThisFrame)
            {
                visible = !visible;
            }
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            float w = Mathf.Min(520f, Screen.width - 40f);
            float h = Mathf.Min(420f, Screen.height - 80f);
            Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);
            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);

            GUI.skin.label.fontSize = 20;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(card.x + 18f, card.y + 14f, card.width - 36f, 32f), "조작 도움말");
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextTitle;

            const string body =
                "• 우클릭 — 선택 유닛 이동 / 적·건물 공격\n" +
                "• 1~8 — 코어 생산 슬롯\n" +
                "• T / Y / U — 코어 업그레이드 (생산·장갑·자원)\n" +
                "• P — 일시정지 · 1/2/3 — 배속\n" +
                "• Space — 카메라 초점(위협/거점 우선)\n" +
                "• Home — 아군 본거지로 시야\n" +
                "• 미니맵 좌·우클릭 — 시야 점프 · 드래그 — 지도 패닝\n" +
                "• V — 자동 표적(가까운 적 우선 ↔ 현재 표적 고정)\n" +
                "• F1 — 이 창 닫기";

            GUI.Label(new Rect(card.x + 18f, card.y + 52f, card.width - 36f, card.height - 100f), body);
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + 18f, card.yMax - 44f, card.width - 36f, 28f), "F1 또는 아무 키 없이… 창 밖 클릭은 없음 — F1 로 토글");
            GUI.color = Color.white;

            Rect closeBtn = new Rect(card.xMax - 120f, card.y + 12f, 100f, 32f);
            if (ImGuiGameUi.GameMenuButton(closeBtn, "닫기 (F1)"))
            {
                visible = false;
            }
        }
    }
}
