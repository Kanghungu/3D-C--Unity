using Game.Campaign.Data;
using Game.Campaign.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleAces
{
    public sealed class BattleAcesObjectiveUgui : MonoBehaviour
    {
        // 보조 힌트 2줄 + 방어 미션 시 타이머 줄 수용
        private const float StripHeight = 136f;
        private const float TopMargin = 12f;
        private const float SideMargin = 18f;

        private MissionDefinition mission;
        private CampaignBattleFlow flow;
        private Canvas rootCanvas;
        private Text textPrimary;
        private Text textDetail;
        private bool built;

        public bool HasObjectiveUi => built && mission != null;
        public float TopReservePixels => HasObjectiveUi ? TopMargin + StripHeight + 6f : 0f;

        public void Initialize(MissionDefinition m, CampaignBattleFlow battleFlow)
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
            bg.color = new Color(0.07f, 0.09f, 0.13f, 0.94f);
            bg.raycastTarget = false;

            GameObject accent = new GameObject("AccentLine");
            accent.transform.SetParent(panel.transform, false);
            RectTransform accentRt = accent.AddComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(1f, 0f);
            accentRt.pivot = new Vector2(0.5f, 0f);
            accentRt.sizeDelta = new Vector2(0f, 3f);
            accentRt.anchoredPosition = Vector2.zero;
            Image accentImg = accent.AddComponent<Image>();
            accentImg.color = new Color(0.72f, 0.62f, 0.28f, 0.95f);
            accentImg.raycastTarget = false;

            textPrimary = CreateStretchedTopText(panel.transform, "Primary", font, 18, TextAnchor.MiddleLeft, 18f, 32f);
            textPrimary.color = new Color(0.96f, 0.86f, 0.42f, 1f);
            AddOutline(textPrimary);

            textDetail = CreateStretchedTopText(panel.transform, "Detail", font, 14, TextAnchor.UpperLeft, 52f, 78f);
            textDetail.color = new Color(0.65f, 0.72f, 0.8f, 1f);
            textDetail.horizontalOverflow = HorizontalWrapMode.Wrap;
            textDetail.verticalOverflow = VerticalWrapMode.Truncate;
            AddOutline(textDetail);

            RefreshTexts();
        }

        private static Text CreateStretchedTopText(
            Transform parent,
            string name,
            Font font,
            int fontSize,
            TextAnchor align,
            float yFromTop,
            float height)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-36f, height);
            rt.anchoredPosition = new Vector2(0f, -yFromTop);

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

        private void RefreshTexts()
        {
            if (textPrimary == null || textDetail == null || mission == null)
            {
                return;
            }

            string primary = MissionObjectiveDisplayText.GetPrimaryLine(mission.ObjectiveKind);
            textPrimary.text = $"《 {mission.DisplayName} 》  ·  {primary}";

            string hint = MissionObjectiveDisplayText.GetGameplayHint(mission.ObjectiveKind);
            string extra = GetDefenseExtraLine();
            if (string.IsNullOrEmpty(extra))
            {
                extra = GetSeizeExtraLine();
            }
            textDetail.text = string.IsNullOrEmpty(extra) ? hint : $"{hint}\n{extra}";
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
            if (zone.IsCompleted)
            {
                return "점령 완료. 승리 처리 중입니다.";
            }

            if (!zone.IsPlayerInside)
            {
                return $"점령 대기 중 · 진행 {pct}% / 목표 {zone.HoldSecondsRequired:0}초 유지";
            }

            return $"점령 진행 중 · 아군 {zone.OccupyingPlayerUnitCount}기 배치 · 진행 {pct}%";
        }

    }
}
