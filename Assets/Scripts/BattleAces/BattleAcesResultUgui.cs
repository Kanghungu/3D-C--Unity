using Game.Audio;
using Game.Campaign;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.BattleAces
{
    /// <summary>
    /// 승패 결과 카드 — IMGUI 팔레트·LegacyRuntime 폰트·상단 목표 바보다 높은 sortingOrder.
    /// </summary>
    public sealed class BattleAcesResultUgui : MonoBehaviour
    {
        private const int CanvasSortOrder = 40;

        private Canvas rootCanvas;
        private CanvasScaler uguiScaler;
        private RectTransform cardRt;
        private Text textTitle;
        private Text textEndReason;
        private Text textDefeatHint;
        private Text textBody;
        private Text textStats;
        private Text textBonus;
        private Text textNextStep;
        private Text textRKey;
        private Text textEsc;
        private Text textFooter;
        private Text textRetry;
        private Text textMenu;
        private GameObject endReasonRow;
        private GameObject defeatRow;
        private GameObject bonusRow;

        private string menuSceneForClick = "CampaignMenu";

        private bool built;

        /// <summary>결과 카드가 화면에 떠 있는지 — OnGUI 가 매 프레임 호출될 때 중복 Show 방지용</summary>
        public bool IsResultCardVisible => built && rootCanvas != null && rootCanvas.gameObject.activeSelf;

        private void LateUpdate()
        {
            if (!built || rootCanvas == null || !rootCanvas.gameObject.activeSelf || uguiScaler == null)
            {
                return;
            }

            BattleAcesUguiScaleUtility.ApplyUserUiScale(uguiScaler);
        }

        private void OnDestroy()
        {
            if (rootCanvas != null)
            {
                Destroy(rootCanvas.gameObject);
                rootCanvas = null;
            }

            uguiScaler = null;
        }

        public void Hide()
        {
            if (rootCanvas != null)
            {
                rootCanvas.gameObject.SetActive(false);
            }
        }

        public void Show(in BattleResultCardPresentation presentation)
        {
            if (!built)
            {
                BuildUi();
            }

            UpdateCardSize();
            ApplyPresentation(in presentation);
            rootCanvas.gameObject.SetActive(true);
        }

        private void UpdateCardSize()
        {
            if (cardRt == null)
            {
                return;
            }

            float sw = Screen.width;
            float sh = Screen.height;
            bool veryLowRes = sh < 520f || sw < 720f;
            float cardW = Mathf.Min(700f, sw - 40f);
            float cardH = Mathf.Clamp(sh * (veryLowRes ? 0.62f : 0.56f), veryLowRes ? 340f : 392f, 600f);
            cardH = Mathf.Min(cardH, Mathf.Max(veryLowRes ? 300f : 320f, sh - (veryLowRes ? 36f : 48f)));
            cardRt.sizeDelta = new Vector2(cardW, cardH);
        }

        private void ApplyPresentation(in BattleResultCardPresentation p)
        {
            menuSceneForClick = p.MenuSceneName;

            textTitle.text = p.Title;
            textTitle.color = p.Won ? ImGuiGameUi.VictoryTint : ImGuiGameUi.DefeatTint;

            bool hasEnd = !string.IsNullOrEmpty(p.EndReasonLine);
            endReasonRow.SetActive(hasEnd);
            if (hasEnd)
            {
                textEndReason.text = p.EndReasonLine;
            }

            bool hasDefeat = !string.IsNullOrEmpty(p.DefeatRetryHint);
            defeatRow.SetActive(hasDefeat);
            if (hasDefeat)
            {
                textDefeatHint.text = p.DefeatRetryHint;
            }

            textBody.text = p.Body;
            textStats.text = p.StatsOneLine;

            bonusRow.SetActive(p.HasBonusLine);
            if (p.HasBonusLine)
            {
                textBonus.text = p.BonusLine;
                textBonus.color = p.BonusLineUseGold ? ImGuiGameUi.AccentGold : ImGuiGameUi.TextMuted;
            }

            textNextStep.text = p.NextStepLine;
            textRKey.text = p.RKeyLine;
            textEsc.text = p.EscLine;
            textFooter.text = p.FooterDevNote;

            textRetry.text = p.RetryLabel;
            textMenu.text = p.MenuButtonLabel;

            LayoutRebuilder.ForceRebuildLayoutImmediate(cardRt);
        }

        private void OnRetryClicked()
        {
            ProceduralAudioUtility.PlayUiMenuAck();
            Time.timeScale = 1f;
            Hide();
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }

        private void OnMenuClicked()
        {
            ProceduralAudioUtility.PlayUiMenuAck();
            Time.timeScale = 1f;
            Hide();
            CampaignSceneLoadUtility.TryLoadSceneByName(menuSceneForClick, "결과 화면에서 메뉴 복귀");
        }

        private void BuildUi()
        {
            Font font = GetUiFont();

            GameObject root = new GameObject("BA_ResultUgui");
            root.transform.SetParent(transform, false);

            rootCanvas = root.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = CanvasSortOrder;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            uguiScaler = scaler;
            BattleAcesUguiScaleUtility.ApplyUserUiScale(uguiScaler);

            root.AddComponent<GraphicRaycaster>();

            GameObject dimGo = new GameObject("Dim");
            dimGo.transform.SetParent(root.transform, false);
            RectTransform dimRt = dimGo.AddComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero;
            dimRt.offsetMax = Vector2.zero;
            Image dimImg = dimGo.AddComponent<Image>();
            dimImg.color = ImGuiGameUi.DimFullscreen;
            dimImg.raycastTarget = true;

            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(root.transform, false);
            cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);

            Image cardBg = cardGo.AddComponent<Image>();
            cardBg.color = ImGuiGameUi.PanelBgLift;
            cardBg.raycastTarget = true;
            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = ImGuiGameUi.BorderCool;
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject innerGo = new GameObject("Inner");
            innerGo.transform.SetParent(cardGo.transform, false);
            RectTransform innerRt = innerGo.AddComponent<RectTransform>();
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = new Vector2(8f, 8f);
            innerRt.offsetMax = new Vector2(-8f, -8f);

            VerticalLayoutGroup rootVlg = innerGo.AddComponent<VerticalLayoutGroup>();
            rootVlg.padding = new RectOffset(16, 16, 10, 10);
            rootVlg.spacing = 6;
            rootVlg.childAlignment = TextAnchor.UpperCenter;
            rootVlg.childControlWidth = true;
            rootVlg.childControlHeight = true;
            rootVlg.childForceExpandWidth = true;
            rootVlg.childForceExpandHeight = false;

            textTitle = CreateLabel(innerGo.transform, "Title", font, 26, Color.white, TextAnchor.MiddleCenter);
            LayoutElement titleLe = textTitle.gameObject.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 40f;

            AddRule(innerGo.transform);

            endReasonRow = CreateOptionalRow(innerGo.transform, "EndReasonRow", font, 14, ImGuiGameUi.TextMuted, out textEndReason);
            defeatRow = CreateOptionalRow(innerGo.transform, "DefeatHintRow", font, 14, ImGuiGameUi.AccentCyan, out textDefeatHint);

            CreateBodyScroll(innerGo.transform, font, out textBody);

            AddRule(innerGo.transform);

            textStats = CreateLabel(innerGo.transform, "Stats", font, 14, ImGuiGameUi.TextMuted, TextAnchor.UpperLeft);
            LayoutElement statsLe = textStats.gameObject.AddComponent<LayoutElement>();
            statsLe.preferredHeight = 36f;

            bonusRow = new GameObject("BonusRow");
            bonusRow.transform.SetParent(innerGo.transform, false);
            LayoutElement bonusRowLe = bonusRow.AddComponent<LayoutElement>();
            bonusRowLe.preferredHeight = 40f;
            HorizontalLayoutGroup bonusH = bonusRow.AddComponent<HorizontalLayoutGroup>();
            bonusH.childAlignment = TextAnchor.UpperLeft;
            bonusH.childForceExpandWidth = true;
            textBonus = CreateLabel(bonusRow.transform, "Bonus", font, 13, ImGuiGameUi.TextMuted, TextAnchor.UpperLeft);
            LayoutElement bonusTextLe = textBonus.gameObject.AddComponent<LayoutElement>();
            bonusTextLe.flexibleWidth = 1f;

            textNextStep = CreateLabel(innerGo.transform, "NextStep", font, 13, ImGuiGameUi.TextMuted, TextAnchor.UpperLeft);
            LayoutElement nextLe = textNextStep.gameObject.AddComponent<LayoutElement>();
            nextLe.preferredHeight = 48f;
            nextLe.minHeight = 22f;

            textRKey = CreateLabel(innerGo.transform, "RKey", font, 17, ImGuiGameUi.AccentGold, TextAnchor.UpperLeft);
            LayoutElement rLe = textRKey.gameObject.AddComponent<LayoutElement>();
            rLe.preferredHeight = 28f;

            textEsc = CreateLabel(innerGo.transform, "Esc", font, 15, ImGuiGameUi.TextMuted, TextAnchor.UpperLeft);
            LayoutElement escLe = textEsc.gameObject.AddComponent<LayoutElement>();
            escLe.preferredHeight = 26f;

            textFooter = CreateLabel(innerGo.transform, "Footer", font, 12, ImGuiGameUi.TextMuted, TextAnchor.UpperLeft);
            LayoutElement footLe = textFooter.gameObject.AddComponent<LayoutElement>();
            footLe.preferredHeight = 34f;

            AddRule(innerGo.transform);

            GameObject btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(innerGo.transform, false);
            HorizontalLayoutGroup btnH = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnH.spacing = 12;
            btnH.childForceExpandWidth = true;
            btnH.childControlWidth = true;
            btnH.childAlignment = TextAnchor.MiddleCenter;
            LayoutElement btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 52f;

            CreateMenuButton(btnRow.transform, "Retry", font, OnRetryClicked, out textRetry);
            CreateMenuButton(btnRow.transform, "Menu", font, OnMenuClicked, out textMenu);

            rootCanvas.gameObject.SetActive(false);
            built = true;
        }

        private static void AddRule(Transform parent)
        {
            GameObject rule = new GameObject("Rule");
            rule.transform.SetParent(parent, false);
            LayoutElement le = rule.AddComponent<LayoutElement>();
            le.preferredHeight = 1f;
            Image img = rule.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 0.58f);
            img.raycastTarget = false;
        }

        private static GameObject CreateOptionalRow(Transform parent, string name, Font font, int size, Color color, out Text textField)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            LayoutElement rowLe = row.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 40f;
            textField = CreateLabel(row.transform, "Text", font, size, color, TextAnchor.UpperLeft);
            LayoutElement tLe = textField.gameObject.AddComponent<LayoutElement>();
            tLe.flexibleWidth = 1f;
            tLe.minHeight = 24f;
            return row;
        }

        private static void CreateBodyScroll(Transform parent, Font font, out Text bodyText)
        {
            GameObject scrollGo = new GameObject("BodyScroll");
            scrollGo.transform.SetParent(parent, false);
            RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            LayoutElement scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.minHeight = 80f;
            scrollLe.flexibleHeight = 1f;

            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            RectTransform vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup contentV = content.AddComponent<VerticalLayoutGroup>();
            contentV.childAlignment = TextAnchor.UpperLeft;
            contentV.childControlWidth = true;
            contentV.childControlHeight = true;
            contentV.childForceExpandWidth = true;
            contentV.padding = new RectOffset(0, 8, 0, 0);

            ContentSizeFitter contentCsf = content.AddComponent<ContentSizeFitter>();
            contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            bodyText = CreateLabel(content.transform, "Body", font, 16, Color.white, TextAnchor.UpperLeft);
            bodyText.lineSpacing = 1.05f;
            ContentSizeFitter bodyCsf = bodyText.gameObject.AddComponent<ContentSizeFitter>();
            bodyCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bodyCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            LayoutElement bodyLayout = bodyText.gameObject.AddComponent<LayoutElement>();
            bodyLayout.minHeight = 40f;

            scroll.viewport = vpRt;
            scroll.content = contentRt;

            Image vpRaycast = viewport.AddComponent<Image>();
            vpRaycast.color = new Color(1f, 1f, 1f, 0.004f);
            vpRaycast.raycastTarget = true;
        }

        private static void CreateMenuButton(Transform parent, string name, Font font, UnityEngine.Events.UnityAction onClick, out Text label)
        {
            GameObject btnGo = new GameObject(name + "Button");
            btnGo.transform.SetParent(parent, false);
            LayoutElement le = btnGo.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            le.flexibleWidth = 1f;

            Image img = btnGo.AddComponent<Image>();
            img.color = new Color(0.09f, 0.1f, 0.12f, 0.95f);
            img.raycastTarget = true;

            Button btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.14f, 0.16f, 0.2f, 1f);
            colors.pressedColor = new Color(0.18f, 0.2f, 0.24f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);
            label = textGo.AddComponent<Text>();
            label.font = font;
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = ImGuiGameUi.TextMuted;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            RectTransform textRt = label.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8f, 4f);
            textRt.offsetMax = new Vector2(-8f, -4f);
        }

        private static Text CreateLabel(Transform parent, string name, Font font, int size, Color color, TextAnchor align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            RectTransform rt = t.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = Vector2.zero;
            return t;
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
    }
}
