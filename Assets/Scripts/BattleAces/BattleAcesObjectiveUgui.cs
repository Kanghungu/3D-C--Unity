using Game.Campaign.Data;
using Game.Campaign.Scene;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleAces
{
    public sealed class BattleAcesObjectiveUgui : MonoBehaviour
    {
        // 데모 제목 · 목표 한 줄 · 보조 설명(2줄까지)
        private const float StripHeight = 124f;
        private const float TopMargin = 10f;
        private const float SideMargin = 16f;

        private MissionDefinition mission;
        private BattleMissionFlow flow;
        private Canvas rootCanvas;
        private Text textDemoTitle;
        private Text textObjective;
        private Text textAuxiliary;
        private bool built;

        public bool HasObjectiveUi => built && mission != null;
        public float TopReservePixels => HasObjectiveUi ? TopMargin + StripHeight + 6f : 0f;

        public void Initialize(MissionDefinition m, BattleMissionFlow battleFlow)
        {
            mission = m;
            flow = battleFlow;
            if (mission == null)
            {
                enabled = false;
                return;
            }

            BuildUi();
            built = true;
        }

        private void LateUpdate()
        {
            if (!built || mission == null || rootCanvas == null)
            {
                return;
            }

            bool show = flow == null || flow.IsGameplayStarted;
            // 승패 결과 IMGUI와 겹치지 않도록 전투 종료 시 상단 목표 바 숨김
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController matchCtrl) && matchCtrl.IsFinished)
            {
                show = false;
            }

            rootCanvas.gameObject.SetActive(show);
            if (!show)
            {
                return;
            }

            RefreshTexts();
        }

        private void OnDestroy()
        {
            if (rootCanvas != null)
            {
                Destroy(rootCanvas.gameObject);
                rootCanvas = null;
            }
        }

        private void BuildUi()
        {
            Font font = GetUiFont();

            GameObject root = new GameObject("BA_ObjectiveUgui");
            root.transform.SetParent(transform, false);

            rootCanvas = root.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 24;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("ObjectiveStrip");
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(1f, 1f);
            panelRt.pivot = new Vector2(0.5f, 1f);
            panelRt.sizeDelta = new Vector2(-SideMargin * 2f, StripHeight);
            panelRt.anchoredPosition = new Vector2(0f, -TopMargin);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(ImGuiGameUi.PanelBgHud.r, ImGuiGameUi.PanelBgHud.g, ImGuiGameUi.PanelBgHud.b, 0.96f);
            bg.raycastTarget = false;

            GameObject leftStripe = new GameObject("LeftStripe");
            leftStripe.transform.SetParent(panel.transform, false);
            RectTransform stripeRt = leftStripe.AddComponent<RectTransform>();
            stripeRt.anchorMin = new Vector2(0f, 0f);
            stripeRt.anchorMax = new Vector2(0f, 1f);
            stripeRt.pivot = new Vector2(0f, 0.5f);
            stripeRt.sizeDelta = new Vector2(4f, 0f);
            stripeRt.anchoredPosition = Vector2.zero;
            Image stripeImg = leftStripe.AddComponent<Image>();
            stripeImg.color = ImGuiGameUi.HudStripeTactical;
            stripeImg.raycastTarget = false;

            GameObject accent = new GameObject("AccentLine");
            accent.transform.SetParent(panel.transform, false);
            RectTransform accentRt = accent.AddComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(1f, 0f);
            accentRt.pivot = new Vector2(0.5f, 0f);
            accentRt.sizeDelta = new Vector2(0f, 1f);
            accentRt.anchoredPosition = Vector2.zero;
            Image accentImg = accent.AddComponent<Image>();
            accentImg.color = new Color(ImGuiGameUi.BorderCool.r, ImGuiGameUi.BorderCool.g, ImGuiGameUi.BorderCool.b, 0.85f);
            accentImg.raycastTarget = false;

            textDemoTitle = CreateStretchedTopText(panel.transform, "DemoTitle", font, 15, TextAnchor.MiddleLeft, 10f, 22f, 10f);
            textDemoTitle.color = ImGuiGameUi.AccentGold;
            AddOutline(textDemoTitle);

            textObjective = CreateStretchedTopText(panel.transform, "Objective", font, 16, TextAnchor.MiddleLeft, 34f, 26f, 10f);
            textObjective.color = ImGuiGameUi.TextTitle;
            AddOutline(textObjective);

            textAuxiliary = CreateStretchedTopText(panel.transform, "Auxiliary", font, 12, TextAnchor.UpperLeft, 62f, 56f, 10f);
            textAuxiliary.color = ImGuiGameUi.TextMuted;
            textAuxiliary.horizontalOverflow = HorizontalWrapMode.Wrap;
            textAuxiliary.verticalOverflow = VerticalWrapMode.Truncate;
            AddOutline(textAuxiliary);

            RefreshTexts();
        }

        private static Text CreateStretchedTopText(
            Transform parent,
            string name,
            Font font,
            int fontSize,
            TextAnchor align,
            float yFromTop,
            float height,
            float padLeftForStripe = 0f)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            float side = 18f + padLeftForStripe;
            rt.sizeDelta = new Vector2(-side - 18f, height);
            rt.anchoredPosition = new Vector2(padLeftForStripe * 0.5f, -yFromTop);

            Text t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        private static void AddOutline(Text t)
        {
            Outline o = t.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.78f);
            o.effectDistance = new Vector2(1f, -1f);
        }

        private static Font GetUiFont()
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null)
            {
                f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return f;
        }

        /// <summary>상단 바·F1 등에서 동일 규칙 — 캠페인은 「데모 ·」 접두, 연습/스커미시는 표시명만</summary>
        public static string FormatTopBarDemoTitle(MissionDefinition m)
        {
            if (m == null)
            {
                return string.Empty;
            }

            // 챕터6 등 번들 표시명은 ResolveMissionDisplayName 에서 통일
            string name = MissionObjectiveDisplayText.ResolveMissionDisplayName(m);
            if (m.IsPracticeStyleOneMatch)
            {
                return name;
            }

            return string.IsNullOrEmpty(name) ? "데모" : $"데모 · {name}";
        }

        private void RefreshTexts()
        {
            if (textDemoTitle == null || textObjective == null || textAuxiliary == null || mission == null)
            {
                return;
            }

            textDemoTitle.text = FormatTopBarDemoTitle(mission);
            textObjective.text = MissionObjectiveDisplayText.GetPrimaryLine(mission);

            string hint = MissionObjectiveDisplayText.GetGameplayHint(mission);
            string extra = GetDefenseExtraLine();
            if (string.IsNullOrEmpty(extra))
            {
                extra = GetSeizeExtraLine();
            }

            textAuxiliary.text = string.IsNullOrEmpty(extra) ? hint : $"{hint}\n{extra}";
        }

        private string GetDefenseExtraLine()
        {
            if (mission.ObjectiveKind != MissionObjectiveKind.SanctuaryDefense)
            {
                return string.Empty;
            }

            if (flow == null || !flow.IsGameplayStarted)
            {
                return "작전 시작 후 방어 타이머가 진행됩니다.";
            }

            float elapsed = Time.time - flow.GameplayStartTime;
            float remain = Mathf.Max(0f, mission.DefenseDurationSeconds - elapsed);
            return $"남은 시간 {remain:0}초 / 목표 {mission.DefenseDurationSeconds:0}초 생존";
        }

        private string GetSeizeExtraLine()
        {
            if (mission.ObjectiveKind != MissionObjectiveKind.SeizeRelicOrNode)
            {
                return string.Empty;
            }

            MissionCaptureZone zone = MissionCaptureZone.Instance;
            if (zone == null)
            {
                return "보라 구역 안에 아군 병력을 넣어 점령을 시작하십시오.";
            }

            int pct = Mathf.RoundToInt(zone.HoldProgress01 * 100f);
            string healNote = zone.OccupantHealPerSecond > 0.01f
                ? $" 거점 정령: 구역 안 아군 체력이 초당 약 {zone.OccupantHealPerSecond:0.#} 회복됩니다."
                : string.Empty;

            if (zone.IsCompleted)
            {
                return "점령 완료. 승리 처리 중입니다.";
            }

            if (!zone.IsPlayerInside)
            {
                return
                    $"점령 대기 중 · 진행 {pct}% / 목표 {zone.HoldSecondsRequired:0}초 유지 — " +
                    "아군이 보라 구역 안에 있을 때만 타이머가 차오릅니다." +
                    healNote;
            }

            return
                $"점령 진행 중 · 아군 {zone.OccupyingPlayerUnitCount}기 배치 · 진행 {pct}% — " +
                "구역을 비우면 타이머가 초기화됩니다." +
                healNote;
        }

    }
}
