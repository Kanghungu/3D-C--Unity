using UnityEngine;

namespace Game.Prototype
{
    public enum BattlefieldTheme
    {
        DesertShrine,
        CrimsonBasin,
        PaleSaltFlats
    }

    /// <summary>
    /// Single source of truth for battlefield world bounds and minimap presentation.
    /// </summary>
    public class BattlefieldMapProfile : MonoBehaviour
    {
        [SerializeField] private BattlefieldTheme theme = BattlefieldTheme.DesertShrine;
        [SerializeField] private string mapLabel = "Desert Shrine Front";
        [SerializeField] private Vector2 worldCenter = Vector2.zero;
        [SerializeField] private Vector2 worldSize = new(3600f, 3600f);
        [SerializeField] private Color minimapBackgroundColor = new(0.19f, 0.15f, 0.11f, 0.98f);
        [SerializeField] private Color shroudColor = new(0.03f, 0.03f, 0.04f, 0.95f);
        [SerializeField] private Color memoryColor = new(0.12f, 0.1f, 0.08f, 0.72f);

        public BattlefieldTheme Theme => theme;
        public string MapLabel => mapLabel;
        public Vector2 WorldCenter => worldCenter;
        public Vector2 WorldSize => worldSize;
        public float MinX => worldCenter.x - worldSize.x * 0.5f;
        public float MaxX => worldCenter.x + worldSize.x * 0.5f;
        public float MinZ => worldCenter.y - worldSize.y * 0.5f;
        public float MaxZ => worldCenter.y + worldSize.y * 0.5f;
        public Color MinimapBackgroundColor => minimapBackgroundColor;
        public Color ShroudColor => shroudColor;
        public Color MemoryColor => memoryColor;

        public void Configure(
            BattlefieldTheme newTheme,
            string newMapLabel,
            Vector2 newWorldCenter,
            Vector2 newWorldSize,
            Color newMinimapBackgroundColor,
            Color newShroudColor,
            Color newMemoryColor)
        {
            theme = newTheme;
            mapLabel = newMapLabel;
            worldCenter = newWorldCenter;
            worldSize = new Vector2(Mathf.Max(200f, newWorldSize.x), Mathf.Max(200f, newWorldSize.y));
            minimapBackgroundColor = newMinimapBackgroundColor;
            shroudColor = newShroudColor;
            memoryColor = newMemoryColor;
        }

        public Vector2 WorldToNormalized(Vector3 worldPosition)
        {
            return new Vector2(
                Mathf.InverseLerp(MinX, MaxX, worldPosition.x),
                Mathf.InverseLerp(MaxZ, MinZ, worldPosition.z));
        }

        public Vector3 NormalizedToWorld(Vector2 normalizedPosition)
        {
            float worldX = Mathf.Lerp(MinX, MaxX, normalizedPosition.x);
            float worldZ = Mathf.Lerp(MaxZ, MinZ, normalizedPosition.y);
            return new Vector3(worldX, 0f, worldZ);
        }

        public Vector3 ClampWorldPoint(Vector3 worldPoint, float padding = 0f)
        {
            worldPoint.x = Mathf.Clamp(worldPoint.x, MinX + padding, MaxX - padding);
            worldPoint.z = Mathf.Clamp(worldPoint.z, MinZ + padding, MaxZ - padding);
            return worldPoint;
        }
    }
}
