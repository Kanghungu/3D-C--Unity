using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared HUD layout helpers so camera and selection input can respect the same UI regions.
    /// Screen positions passed here should use the Input System convention with origin at bottom-left.
    /// </summary>
    public static class PrototypeHudLayoutUtility
    {
        private const float BottomBarMargin = 12f;
        private const float BottomBarHeight = 180f;
        private const float BottomBarWidth  = 24f + 180f + 10f + 260f + 10f + 260f; // must match DrawBottomBar
        private const float NewsWidth = 344f;
        private const float NewsHeight = 54f;
        private const float NewsTopMargin = 40f;
        private const float NewsRightMargin = 12f;

        public static Rect GetBottomBarInputRect()
        {
            float totalH = BottomBarHeight + BottomBarMargin * 2f;
            float left   = (Screen.width - BottomBarWidth) * 0.5f;
            return new Rect(left, 0f, BottomBarWidth, totalH);
        }

        public static Rect GetWarNewsInputRect()
        {
            float left = Screen.width - NewsWidth - NewsRightMargin;
            float bottom = Screen.height - NewsHeight - NewsTopMargin;
            return new Rect(left, bottom, NewsWidth, NewsHeight);
        }

        public static bool IsScreenPositionOverInteractiveHud(Vector2 screenPosition)
        {
            return GetBottomBarInputRect().Contains(screenPosition) || GetWarNewsInputRect().Contains(screenPosition);
        }
    }
}
