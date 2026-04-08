using System.Collections.Generic;
using Game.Prototype;
using Game.UI;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>
    /// FoW: 격자·탐색/가시 + 게임플레이 마스크(지형·미니맵) + 우측 디버그 미리보기.
    /// </summary>
    public sealed class BattleAcesFogOfWarDebug : MonoBehaviour
    {
        public static BattleAcesFogOfWarDebug Instance { get; private set; }

        /// <summary>지형 오버레이·전술 맵에 공유 — 가시=투명·탐색=안개·미탐색=어둡게</summary>
        public Texture2D FogMaskTexture => fogMaskTexture;

        public bool IsWorldVisible(Vector3 worldPosition)
        {
            if (!initialized || grid == null)
            {
                return true;
            }

            return grid.TryGetCell(worldPosition.x, worldPosition.z, out int ix, out int iz) && grid.IsVisible(ix, iz);
        }

        [Header("격자")]
        [SerializeField] private float cellWorldSize = 8f;

        [Tooltip("아군 코어 시야 반경(월드)")]
        [SerializeField] private float visionRadiusFromPlayerCore = 26f;

        [Tooltip("병종 미매칭 시 아군 유닛 기본 시야 반경")]
        [SerializeField] private float defaultUnitVisionRadius = 14f;

        [Tooltip("비행(Definition.IsFlying 또는 공중요새) 시야 반경에 곱함 — 공중 시야 확장")]
        [SerializeField] private float flyingVisionRadiusMultiplier = 1.35f;

        [Header("장애물 시야 가림")]
        [Tooltip("끄면 기존처럼 원형 시야만(레이캐스트 없음)")]
        [SerializeField] private bool useObstacleVisionBlocking = true;

        [Tooltip("비어 있으면 런타임에 VisionObstacle 레이어로 채움")]
        [SerializeField] private LayerMask visionObstacleMask;

        [Tooltip("지상 유닛·코어 눈 높이(피벗 Y 기준 오프셋) — 수평 LOS")]
        [SerializeField] private float unitEyeHeightOffset = 1.25f;

        [SerializeField] private float coreEyeHeightOffset = 2.65f;

        [Tooltip("레이 시작·끝을 줄여 자기/목표 셀 오인 방지")]
        [SerializeField] private float losRayStartInset = 0.4f;

        [SerializeField] private float losRayEndInset = 0.45f;

        [Header("갱신 비용")]
        [Tooltip("1=매 프레임, 2=격 프레임마다 … (부담 줄이기)")]
        [SerializeField] private int visionUpdateEveryNFrames = 1;

        [Header("디버그 UI")]
        [SerializeField] private bool showOverlay = false;

        [SerializeField] private int overlayMaxSide = 220;

        private FogOfWarGrid grid;
        private Texture2D debugTexture;
        private Texture2D fogMaskTexture;
        private BattleAcesMatchController matchRef;
        private bool initialized;

        /// <summary>Initialize 시점에 visionObstacleMask 보정 결과</summary>
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

        /// <summary>미니맵과 동일한 XZ 범위로 초기화</summary>
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

            // 브리핑 중에는 시야를 갱신하지 않음(작전 시작 후 탐색 누적)
            BattleMissionFlow flow = BattleMissionFlow.Instance;
            if (flow != null && !flow.IsGameplayStarted)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            // F10 은 BattleAcesDevelopmentHud(에디터·개발 빌드)와 겹침 → FoW 미리보기는 F11
            if (kb != null && kb.f11Key.wasPressedThisFrame)
            {
                showOverlay = !showOverlay;
            }

            int interval = Mathf.Max(1, visionUpdateEveryNFrames);
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

        /// <summary>등록된 SelectableUnit 중 아군·생존 유닛만 시야 원 추가</summary>
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

        /// <summary>공중 유닛은 장애물 너머도 보고, 반경만 넓힘(수평 LOS 생략).</summary>
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

            // 병종별 살짝만 분리 — 나중에 데이터(ScriptableObject)로 빼기 쉽게
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
                    // 화면 위쪽이 월드 Z 큰 쪽이 되도록 뒤집기(미니맵 감각과 맞춤)
                    int texRow = h - 1 - iz;
                    pixels[texRow * w + ix] = grid.GetDebugCellColor(ix, iz);
                }
            }

            debugTexture.SetPixels32(pixels);
            debugTexture.Apply(false);
        }

        /// <summary>월드 Z 증가 = 텍스처 v 증가(쿼드 UV·미니맵과 동일)</summary>
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
                $"검=미탐색  회청=탐색만  연두=가시\nF11 토글 · 코어+아군 · 장애물LOS{(useObstacleVisionBlocking ? "ON" : "OFF")} · 공중확대 · {Mathf.Max(1, visionUpdateEveryNFrames)}프레임");

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
