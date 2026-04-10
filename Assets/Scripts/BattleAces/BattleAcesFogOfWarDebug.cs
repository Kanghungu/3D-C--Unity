using System.Collections.Generic;
using Game.Prototype;
using Game.Settings;
using Game.UI;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces fog-of-war debug runtime.
    /// Tracks visible/explored cells, pushes a fog mask texture, and can draw the F11 overlay.
    /// </summary>
    public sealed class BattleAcesFogOfWarDebug : MonoBehaviour
    {
        public static BattleAcesFogOfWarDebug Instance { get; private set; }

        /// <summary>Shared fog mask used by the world overlay and minimap rendering.</summary>
        public Texture2D FogMaskTexture => fogMaskTexture;

        public bool IsWorldVisible(Vector3 worldPosition)
        {
            if (!initialized || grid == null)
            {
                return true;
            }

            return grid.TryGetCell(worldPosition.x, worldPosition.z, out int ix, out int iz) && grid.IsVisible(ix, iz);
        }

        [Header("Grid")]
        [SerializeField] private float cellWorldSize = 8f;

        [Tooltip("Vision radius granted by the player command core.")]
        [SerializeField] private float visionRadiusFromPlayerCore = 26f;

        [Tooltip("Default vision radius applied to standard player units.")]
        [SerializeField] private float defaultUnitVisionRadius = 14f;

        [Tooltip("Extra multiplier applied to flying-unit vision radius.")]
        [SerializeField] private float flyingVisionRadiusMultiplier = 1.35f;

        [Header("Obstacle Vision Blocking")]
        [Tooltip("When enabled, line-of-sight raycasts can be blocked by obstacle layers.")]
        [SerializeField] private bool useObstacleVisionBlocking = true;

        [Tooltip("Optional mask for LOS blockers. Falls back to the VisionObstacle layer if empty.")]
        [SerializeField] private LayerMask visionObstacleMask;

        [Tooltip("Eye height used for unit LOS checks.")]
        [SerializeField] private float unitEyeHeightOffset = 1.25f;

        [SerializeField] private float coreEyeHeightOffset = 2.65f;

        [Tooltip("Inset applied to the LOS ray start to reduce self-hit noise.")]
        [SerializeField] private float losRayStartInset = 0.4f;

        [SerializeField] private float losRayEndInset = 0.45f;

        [Header("Update Cost")]
        [Tooltip("1 = every frame, 2 = every other frame, etc.")]
        [SerializeField] private int visionUpdateEveryNFrames = 1;

        [Header("Debug UI")]
        [SerializeField] private bool showOverlay = false;

        [SerializeField] private int overlayMaxSide = 220;

        private FogOfWarGrid grid;
        private Texture2D debugTexture;
        private Texture2D fogMaskTexture;
        private BattleAcesMatchController matchRef;
        private bool initialized;

        /// <summary>Resolved obstacle mask after initialization.</summary>
        private LayerMask runtimeVisionObstacleMask;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Initializes the grid using the same XZ range as the minimap.</summary>
        public void Initialize(Vector2 worldMin, Vector2 worldMax, BattleAcesMatchController match)
        {
            matchRef = match;
            grid = new FogOfWarGrid();
            grid.Initialize(worldMin, worldMax, cellWorldSize);

            int w = grid.CellsX;
            int h = grid.CellsZ;
            debugTexture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            fogMaskTexture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            FillFogMaskFullyClear();
            BattleAcesFogWorldOverlay.Instance?.SetFogTexture(fogMaskTexture);

            runtimeVisionObstacleMask = visionObstacleMask;
            if (runtimeVisionObstacleMask.value == 0)
            {
                int obstacleLayer = LayerMask.NameToLayer("VisionObstacle");
                if (obstacleLayer >= 0)
                {
                    runtimeVisionObstacleMask = 1 << obstacleLayer;
                }
            }

            initialized = true;
        }

        /// <summary>
        /// Enforces a minimum two-frame update interval in Performance mode.
        /// </summary>
        private int GetEffectiveVisionUpdateInterval()
        {
            int v = Mathf.Max(1, visionUpdateEveryNFrames);
            if (GameUserSettings.GraphicQualityPreset == DemoGraphicQualityPreset.Performance)
            {
                v = Mathf.Max(v, 2);
            }

            return v;
        }

        private void FillFogMaskFullyClear()
        {
            if (fogMaskTexture == null || grid == null)
            {
                return;
            }

            int n = grid.CellsX * grid.CellsZ;
            var clear = new Color32[n];
            for (int i = 0; i < n; i++)
            {
                clear[i] = new Color32(0, 0, 0, 0);
            }

            fogMaskTexture.SetPixels32(clear);
            fogMaskTexture.Apply(false);
        }

        private void Update()
        {
            if (!initialized || grid == null || matchRef == null || matchRef.IsFinished)
            {
                return;
            }

            // Do not update vision during briefing; gameplay start controls the first reveal pass.
            BattleMissionFlow flow = BattleMissionFlow.Instance;
            if (flow != null && !flow.IsGameplayStarted)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            // F11 toggles the FoW preview. F10 controls the separate development HUD.
            if (kb != null && kb.f11Key.wasPressedThisFrame)
            {
                showOverlay = !showOverlay;
            }

            int interval = GetEffectiveVisionUpdateInterval();
            if (Time.frameCount % interval != 0)
            {
                return;
            }

            grid.ClearVisible();
            ApplyPlayerCoreVision();
            ApplyPlayerUnitsVision();
            grid.MergeVisibleIntoExplored();
            UploadDebugTexture();
            UploadFogMaskTexture();
        }

        private void ApplyPlayerCoreVision()
        {
            BattleAcesCore core = matchRef.PlayerCore;
            if (core != null && core.Health != null && core.Health.IsAlive)
            {
                Vector3 p = core.transform.position;
                grid.AddVisibleDiscFromObserver(
                    p.x,
                    p.y,
                    p.z,
                    visionRadiusFromPlayerCore,
                    coreEyeHeightOffset,
                    runtimeVisionObstacleMask,
                    useObstacleVisionBlocking,
                    losRayStartInset,
                    losRayEndInset);
            }
        }

        /// <summary>Adds vision for living player-owned selectable units.</summary>
        private void ApplyPlayerUnitsVision()
        {
            IReadOnlyList<SelectableUnit> units = PrototypeRuntimeRegistry.GetSelectableUnits();
            for (int i = 0; i < units.Count; i++)
            {
                SelectableUnit u = units[i];
                if (u == null || u.Team != UnitTeam.Player)
                {
                    continue;
                }

                CombatTarget ct = u.GetComponent<CombatTarget>();
                if (ct == null || !ct.IsAlive)
                {
                    continue;
                }

                Vector3 p = u.transform.position;
                float r = ResolveUnitVisionRadius(u);
                bool airObserver = IsAirVisionObserver(u);
                grid.AddVisibleDiscFromObserver(
                    p.x,
                    p.y,
                    p.z,
                    r,
                    unitEyeHeightOffset,
                    runtimeVisionObstacleMask,
                    useObstacleVisionBlocking && !airObserver,
                    losRayStartInset,
                    losRayEndInset);
            }
        }

        /// <summary>Air observers ignore obstacle LOS blocking and use the expanded air radius.</summary>
        private static bool IsAirVisionObserver(SelectableUnit unit)
        {
            if (unit == null)
            {
                return false;
            }

            if (unit.Definition != null && unit.Definition.IsFlying)
            {
                return true;
            }

            return unit.Archetype == UnitArchetype.AirborneCitadel;
        }

        private float ResolveUnitVisionRadius(SelectableUnit unit)
        {
            if (unit == null)
            {
                return defaultUnitVisionRadius;
            }

            // Keep per-archetype tuning local here instead of pushing extra data onto the unit definitions.
            float r = unit.Archetype switch
            {
                UnitArchetype.Fighter => Mathf.Max(defaultUnitVisionRadius, 19f),
                UnitArchetype.Artillery => Mathf.Max(defaultUnitVisionRadius, 17f),
                UnitArchetype.AirborneCitadel => Mathf.Max(defaultUnitVisionRadius, 22f),
                UnitArchetype.MobileFortress => Mathf.Max(defaultUnitVisionRadius, 12f),
                UnitArchetype.Rifleman => Mathf.Max(defaultUnitVisionRadius, 16f),
                UnitArchetype.Outrider => Mathf.Max(defaultUnitVisionRadius, 15f),
                UnitArchetype.RoyalGuard => Mathf.Max(defaultUnitVisionRadius, 13f),
                UnitArchetype.ShieldInfantry => Mathf.Max(defaultUnitVisionRadius, 13f),
                UnitArchetype.Spearman => Mathf.Max(defaultUnitVisionRadius, 14f),
                UnitArchetype.SpecialWarrior => Mathf.Max(defaultUnitVisionRadius, 15f),
                _ => defaultUnitVisionRadius
            };

            if (IsAirVisionObserver(unit))
            {
                r *= Mathf.Max(1f, flyingVisionRadiusMultiplier);
            }

            return r;
        }

        private void UploadDebugTexture()
        {
            if (debugTexture == null || grid == null)
            {
                return;
            }

            int w = grid.CellsX;
            int h = grid.CellsZ;
            Color32[] pixels = new Color32[w * h];
            for (int iz = 0; iz < h; iz++)
            {
                for (int ix = 0; ix < w; ix++)
                {
                    // Flip rows so world +Z maps upward like the minimap and debug texture expect.
                    int texRow = h - 1 - iz;
                    pixels[texRow * w + ix] = grid.GetDebugCellColor(ix, iz);
                }
            }

            debugTexture.SetPixels32(pixels);
            debugTexture.Apply(false);
        }

        /// <summary>World +Z maps to texture +V so the fog mask lines up with the ground quad UVs.</summary>
        private void UploadFogMaskTexture()
        {
            if (fogMaskTexture == null || grid == null)
            {
                return;
            }

            int w = grid.CellsX;
            int h = grid.CellsZ;
            Color32[] pixels = new Color32[w * h];
            for (int iz = 0; iz < h; iz++)
            {
                for (int ix = 0; ix < w; ix++)
                {
                    pixels[iz * w + ix] = grid.GetGameplayFogColor(ix, iz);
                }
            }

            fogMaskTexture.SetPixels32(pixels);
            fogMaskTexture.Apply(false);
            BattleAcesFogWorldOverlay.Instance?.SetFogTexture(fogMaskTexture);
        }

        private void OnGUI()
        {
            if (!initialized || !showOverlay || debugTexture == null || grid == null)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();

            float aspect = grid.CellsZ / (float)Mathf.Max(1, grid.CellsX);
            float panelW = overlayMaxSide;
            float panelH = panelW * aspect;
            float x = Screen.width - panelW - 16f;
            float y = 18f;

            ImGuiGameUi.DrawFilledRect(new Rect(x - 3f, y - 3f, panelW + 6f, panelH + 52f), ImGuiGameUi.PanelBgDeep);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y, panelW, panelH), debugTexture, ScaleMode.ScaleToFit, true);

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(x, y + panelH + 4f, panelW, 18f), "FoW 디버그 격자");
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(x, y + panelH + 20f, panelW, 44f),
                $"검정=미탐색 · 회색=탐색 완료 · 밝은 테두리=가시 상태\nF11 토글 · 코어+유닛 · 장애물 LOS {(useObstacleVisionBlocking ? "ON" : "OFF")} · 시야 갱신 {GetEffectiveVisionUpdateInterval()}프레임");

            GUI.color = Color.white;
            ImGuiGameUi.EndScaledGui();
        }

        private void OnDestroy()
        {
            if (debugTexture != null)
            {
                Destroy(debugTexture);
                debugTexture = null;
            }

            if (fogMaskTexture != null)
            {
                Destroy(fogMaskTexture);
                fogMaskTexture = null;
            }
        }
    }
}
