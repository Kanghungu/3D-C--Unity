using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// XZ 평면 격자 — 셀마다 탐색(explored)·가시(visible) 플래그만 유지 (FoW 1단계).
    /// visible은 매 프레임 비운 뒤 다시 채우고, explored는 한 번 true면 유지.
    /// </summary>
    public sealed class FogOfWarGrid
    {
        private bool[] explored;
        private bool[] visible;

        public int CellsX { get; private set; }
        public int CellsZ { get; private set; }
        public float CellSize { get; private set; }
        public Vector2 WorldMin { get; private set; }
        public Vector2 WorldMax { get; private set; }

        public void Initialize(Vector2 worldMin, Vector2 worldMax, float cellWorldSize)
        {
            CellSize = Mathf.Max(0.5f, cellWorldSize);
            WorldMin = worldMin;
            WorldMax = worldMax;

            float spanX = Mathf.Max(0.01f, worldMax.x - worldMin.x);
            float spanZ = Mathf.Max(0.01f, worldMax.y - worldMin.y);
            CellsX = Mathf.Max(1, Mathf.CeilToInt(spanX / CellSize));
            CellsZ = Mathf.Max(1, Mathf.CeilToInt(spanZ / CellSize));

            int n = CellsX * CellsZ;
            explored = new bool[n];
            visible = new bool[n];
        }

        public void ClearVisible()
        {
            if (visible == null)
            {
                return;
            }

            for (int i = 0; i < visible.Length; i++)
            {
                visible[i] = false;
            }
        }

        /// <summary>이번 프레임 가시 영역을 탐색에 누적</summary>
        public void MergeVisibleIntoExplored()
        {
            if (visible == null || explored == null)
            {
                return;
            }

            for (int i = 0; i < visible.Length; i++)
            {
                if (visible[i])
                {
                    explored[i] = true;
                }
            }
        }

        /// <summary>월드 XZ 기준 원 안 셀을 가시로 표시</summary>
        public void AddVisibleCircle(float worldX, float worldZ, float radius)
        {
            if (visible == null || radius <= 0.01f)
            {
                return;
            }

            float r = radius + CellSize * 0.55f;
            float r2 = r * r;

            float minWx = worldX - r;
            float maxWx = worldX + r;
            float minWz = worldZ - r;
            float maxWz = worldZ + r;

            int ix0 = WorldXToCellIndexClamped(minWx);
            int ix1 = WorldXToCellIndexClamped(maxWx);
            int iz0 = WorldZToCellIndexClamped(minWz);
            int iz1 = WorldZToCellIndexClamped(maxWz);

            for (int iz = iz0; iz <= iz1; iz++)
            {
                for (int ix = ix0; ix <= ix1; ix++)
                {
                    GetCellCenterWorld(ix, iz, out float cx, out float cz);
                    float dx = cx - worldX;
                    float dz = cz - worldZ;
                    if (dx * dx + dz * dz <= r2)
                    {
                        int idx = iz * CellsX + ix;
                        visible[idx] = true;
                    }
                }
            }
        }

        /// <summary>
        /// 원형 시야 + 선택적 장애물 가림(VisionObstacle 레이어). 비행 등 LOS 생략 시 blockByObstacles=false.
        /// </summary>
        public void AddVisibleDiscFromObserver(
            float observerX,
            float observerY,
            float observerZ,
            float radiusWorld,
            float eyeHeightOffset,
            LayerMask visionBlockMask,
            bool blockByObstacles,
            float losRayStartInset,
            float losRayEndInset)
        {
            if (visible == null || radiusWorld <= 0.01f)
            {
                return;
            }

            if (!blockByObstacles || visionBlockMask.value == 0)
            {
                AddVisibleCircle(observerX, observerZ, radiusWorld);
                return;
            }

            Vector3 observerWorld = new Vector3(observerX, observerY, observerZ);
            float r = radiusWorld + CellSize * 0.55f;
            float r2 = r * r;

            float minWx = observerX - r;
            float maxWx = observerX + r;
            float minWz = observerZ - r;
            float maxWz = observerZ + r;

            int ix0 = WorldXToCellIndexClamped(minWx);
            int ix1 = WorldXToCellIndexClamped(maxWx);
            int iz0 = WorldZToCellIndexClamped(minWz);
            int iz1 = WorldZToCellIndexClamped(maxWz);

            for (int iz = iz0; iz <= iz1; iz++)
            {
                for (int ix = ix0; ix <= ix1; ix++)
                {
                    GetCellCenterWorld(ix, iz, out float cx, out float cz);
                    float dx = cx - observerX;
                    float dz = cz - observerZ;
                    if (dx * dx + dz * dz > r2)
                    {
                        continue;
                    }

                    if (!FogOfWarLineOfSight.HasClearSight(
                            observerWorld,
                            cx,
                            cz,
                            eyeHeightOffset,
                            visionBlockMask,
                            losRayStartInset,
                            losRayEndInset))
                    {
                        continue;
                    }

                    int idx = iz * CellsX + ix;
                    visible[idx] = true;
                }
            }
        }

        public bool IsExplored(int ix, int iz)
        {
            if (!IsInside(ix, iz))
            {
                return false;
            }

            return explored[iz * CellsX + ix];
        }

        public bool IsVisible(int ix, int iz)
        {
            if (!IsInside(ix, iz))
            {
                return false;
            }

            return visible[iz * CellsX + ix];
        }

        /// <summary>디버그 텍스처용 — 미탐색 / 탐색만 / 가시</summary>
        public Color32 GetDebugCellColor(int ix, int iz)
        {
            if (!IsInside(ix, iz))
            {
                return new Color32(0, 0, 0, 255);
            }

            int i = iz * CellsX + ix;
            if (visible[i])
            {
                // 가시 — 연한 연두(덮어쓰기 알파 낮게)
                return new Color32(60, 220, 120, 45);
            }

            if (explored[i])
            {
                // 탐색만 — 밝은 쿨그레이 옅은 안개(게임플레이 오버레이와 톤 맞춤)
                return new Color32(100, 108, 132, 118);
            }

            // 미탐색
            return new Color32(28, 22, 42, 225);
        }

        /// <summary>지형·미니맵 — 가시=맑음, 탐색만=옅은 안개 유지, 미탐색=짙은 어둠 (유닛 메시는 별도)</summary>
        public Color32 GetGameplayFogColor(int ix, int iz)
        {
            if (!IsInside(ix, iz))
            {
                return new Color32(0, 0, 0, 0);
            }

            int i = iz * CellsX + ix;
            if (visible[i])
            {
                // 시야 안 — 오버레이 없음
                return new Color32(0, 0, 0, 0);
            }

            if (explored[i])
            {
                // 탐색됐지만 시야 밖 — 밝은 RGB + 낮은 알파로 옅은 안개(거의 검정 RGB면 안개가 아니라 먹먹한 막처럼 보임)
                return new Color32(92, 100, 124, 72);
            }

            // 미탐색 — 높은 알파의 짙은 톤
            return new Color32(6, 7, 14, 248);
        }

        private bool IsInside(int ix, int iz)
        {
            return explored != null &&
                   visible != null &&
                   ix >= 0 &&
                   ix < CellsX &&
                   iz >= 0 &&
                   iz < CellsZ;
        }

        private int WorldXToCellIndexClamped(float worldX)
        {
            float t = (worldX - WorldMin.x) / CellSize;
            int ix = Mathf.FloorToInt(t);
            return Mathf.Clamp(ix, 0, CellsX - 1);
        }

        private int WorldZToCellIndexClamped(float worldZ)
        {
            float t = (worldZ - WorldMin.y) / CellSize;
            int iz = Mathf.FloorToInt(t);
            return Mathf.Clamp(iz, 0, CellsZ - 1);
        }

        private void GetCellCenterWorld(int ix, int iz, out float cx, out float cz)
        {
            cx = WorldMin.x + (ix + 0.5f) * CellSize;
            cz = WorldMin.y + (iz + 0.5f) * CellSize;
        }
    }
}
