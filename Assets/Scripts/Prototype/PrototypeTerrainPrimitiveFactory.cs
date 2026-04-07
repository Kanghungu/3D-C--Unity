using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared primitive-based terrain and battlefield prop creation helpers.
    /// Keeps simple visual assembly readable without bloating the bootstrapper.
    /// </summary>
    public static class PrototypeTerrainPrimitiveFactory
    {
        /// <summary>FoW 시야 차단용 — 레이어가 없으면 무시(기존 Default 유지).</summary>
        private static void TrySetVisionObstacleLayer(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            int layerId = LayerMask.NameToLayer("VisionObstacle");
            if (layerId < 0)
            {
                return;
            }

            target.layer = layerId;
        }

        // ── 식생 ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 전장 플랭크 구역에 나무 클러스터와 풀밭을 랜덤 배치한다.
        /// 중앙 교도로(Grand Causeway) 주변과 전선 구역은 피한다.
        /// </summary>
        public static void CreateVegetation(Transform parent, Color baseGroundColor)
        {
            Random.InitState(42); // 항상 같은 배치가 나오도록 시드 고정

            Color trunkColor  = new Color(0.22f, 0.16f, 0.10f);
            Color canopyColor = new Color(
                baseGroundColor.r * 0.48f + 0.08f,
                baseGroundColor.g * 0.55f + 0.12f,
                baseGroundColor.b * 0.28f + 0.02f);
            Color dryGrass    = new Color(0.44f, 0.48f, 0.20f);
            Color deadGrass   = new Color(0.38f, 0.30f, 0.14f);

            // 나무 클러스터 — 플랭크 좌우 4개 구역
            PlaceTreeCluster(parent, new Vector3(-1400f, 0f, -1100f), 8,  280f, trunkColor, canopyColor);
            PlaceTreeCluster(parent, new Vector3(-1600f, 0f,   200f), 6,  240f, trunkColor, canopyColor);
            PlaceTreeCluster(parent, new Vector3( 1450f, 0f,  1050f), 7,  260f, trunkColor, canopyColor);
            PlaceTreeCluster(parent, new Vector3( 1550f, 0f,  -300f), 5,  220f, trunkColor, canopyColor);
            // 후방 소규모 숲
            PlaceTreeCluster(parent, new Vector3(-1900f, 0f, -600f),  4,  180f, trunkColor, canopyColor);
            PlaceTreeCluster(parent, new Vector3( 1880f, 0f,  700f),  4,  180f, trunkColor, canopyColor);
            // 전장 안쪽 부서진 나무들
            PlaceDeadTreeScatter(parent, new Vector3(-600f, 0f, -800f), 5, 320f, trunkColor);
            PlaceDeadTreeScatter(parent, new Vector3( 620f, 0f,  820f), 4, 280f, trunkColor);

            // 풀밭 패치 — 여러 구역에 분산
            PlaceGrassField(parent, new Vector3(-1200f, 0f, -400f), 24, 380f, dryGrass, deadGrass);
            PlaceGrassField(parent, new Vector3( 1200f, 0f,  350f), 20, 340f, dryGrass, deadGrass);
            PlaceGrassField(parent, new Vector3(-1700f, 0f,  800f), 16, 280f, dryGrass, deadGrass);
            PlaceGrassField(parent, new Vector3( 1650f, 0f, -900f), 16, 280f, dryGrass, deadGrass);
            PlaceGrassField(parent, new Vector3( -400f, 0f,-1400f), 12, 240f, deadGrass, deadGrass);
            PlaceGrassField(parent, new Vector3(  380f, 0f, 1380f), 12, 240f, deadGrass, deadGrass);
        }

        private static void PlaceTreeCluster(Transform parent, Vector3 center, int count, float radius,
                                              Color trunkColor, Color canopyColor)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                Vector3 pos = center + new Vector3(offset.x, 0f, offset.y);

                // 중앙 교도로 영역(-140~140 X) 침범 방지
                if (Mathf.Abs(pos.x) < 160f) continue;

                float heightScale = Random.Range(0.7f, 1.35f);
                CreateTree($"Tree_{i}_{center.x:F0}", pos, heightScale, trunkColor, canopyColor, parent);
            }
        }

        private static void PlaceDeadTreeScatter(Transform parent, Vector3 center, int count, float radius,
                                                  Color trunkColor)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                Vector3 pos = center + new Vector3(offset.x, 0f, offset.y);
                float heightScale = Random.Range(0.5f, 1.0f);
                CreateDeadTree($"DeadTree_{i}_{center.x:F0}", pos, heightScale, trunkColor, parent);
            }
        }

        private static void PlaceGrassField(Transform parent, Vector3 center, int count, float radius,
                                             Color colorA, Color colorB)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                Vector3 pos = center + new Vector3(offset.x, 0f, offset.y);
                if (Mathf.Abs(pos.x) < 160f) continue;
                Color c = (i % 2 == 0) ? colorA : colorB;
                CreateGrassPatch($"Grass_{i}_{center.x:F0}", pos, c, parent);
            }
        }

        // ── 나무 ──────────────────────────────────────────────────────────────
        private static void CreateTree(string name, Vector3 pos, float heightScale,
                                        Color trunkColor, Color canopyColor, Transform parent)
        {
            float trunkH  = Random.Range(16f, 28f) * heightScale;
            float trunkR  = Random.Range(1.4f, 2.4f);
            float canopyR = Random.Range(12f, 22f) * heightScale;

            // 줄기
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + "_Trunk";
            trunk.transform.position = pos + new Vector3(0f, trunkH * 0.5f, 0f);
            trunk.transform.localScale = new Vector3(trunkR, trunkH * 0.5f, trunkR);
            trunk.transform.SetParent(parent);
            trunk.GetComponent<Renderer>().material.color = trunkColor;

            // 수관 — 구형 (약간 납작하게)
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = name + "_Canopy";
            canopy.transform.position = pos + new Vector3(
                Random.Range(-2f, 2f),
                trunkH + canopyR * 0.55f,
                Random.Range(-2f, 2f));
            canopy.transform.localScale = new Vector3(canopyR, canopyR * Random.Range(0.7f, 1.0f), canopyR);
            canopy.transform.SetParent(parent);
            canopy.GetComponent<Renderer>().material.color = canopyColor;

            // 나무는 배경 오브젝트 — 안개전 숨김 대상에서 제외
        }

        // ── 죽은 나무 (전장 분위기용) ──────────────────────────────────────────
        private static void CreateDeadTree(string name, Vector3 pos, float heightScale,
                                            Color trunkColor, Transform parent)
        {
            float trunkH = Random.Range(10f, 20f) * heightScale;
            float trunkR = Random.Range(1.0f, 1.8f);
            float tilt   = Random.Range(-12f, 12f);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name;
            trunk.transform.position = pos + new Vector3(0f, trunkH * 0.5f, 0f);
            trunk.transform.localScale = new Vector3(trunkR, trunkH * 0.5f, trunkR);
            trunk.transform.eulerAngles = new Vector3(tilt, Random.Range(0f, 360f), 0f);
            trunk.transform.SetParent(parent);
            Color charred = Color.Lerp(trunkColor, Color.black, Random.Range(0.2f, 0.6f));
            trunk.GetComponent<Renderer>().material.color = charred;

            // 꺾인 가지 1~2개
            int branchCount = Random.Range(1, 3);
            for (int b = 0; b < branchCount; b++)
            {
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.name = name + "_Branch" + b;
                float bLen = Random.Range(4f, 9f);
                float bAngle = Random.Range(35f, 65f);
                float bYaw = Random.Range(0f, 360f);
                branch.transform.position = pos + new Vector3(0f, trunkH * Random.Range(0.5f, 0.85f), 0f);
                branch.transform.localScale = new Vector3(0.6f, bLen * 0.5f, 0.6f);
                branch.transform.eulerAngles = new Vector3(bAngle, bYaw, 0f);
                branch.transform.SetParent(parent);
                branch.GetComponent<Renderer>().material.color = charred;
            }
            // 죽은 나무도 배경 오브젝트 — 안개전 숨김 제외
        }

        // ── 풀 패치 ───────────────────────────────────────────────────────────
        private static void CreateGrassPatch(string name, Vector3 pos, Color color, Transform parent)
        {
            int bladeCount = Random.Range(4, 9);
            float patchR   = Random.Range(6f, 18f);

            for (int i = 0; i < bladeCount; i++)
            {
                Vector2 off = Random.insideUnitCircle * patchR;
                float h = Random.Range(3.5f, 9f);
                float w = Random.Range(0.8f, 2.0f);
                float lean = Random.Range(-18f, 18f);

                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = name + "_blade" + i;
                blade.transform.position = pos + new Vector3(off.x, h * 0.5f, off.y);
                blade.transform.localScale = new Vector3(w, h, w * 0.4f);
                blade.transform.eulerAngles = new Vector3(lean, Random.Range(0f, 360f), 0f);
                blade.transform.SetParent(parent);
                Color bladeColor = Color.Lerp(color, Color.black, Random.Range(0f, 0.25f));
                blade.GetComponent<Renderer>().material.color = bladeColor;
                // 풀도 배경 오브젝트 — 안개전 숨김 제외
            }
        }


        public static void CreateTerrainBlock(string objectName, Vector3 position, Vector3 scale, Transform parent, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.transform.SetParent(parent);
            block.GetComponent<Renderer>().material.color = color;
            TrySetVisionObstacleLayer(block);
            EnsureFogObject(block, BattlefieldFogRequirement.Explored);
        }

        public static void CreateTerrainPillar(string objectName, Vector3 position, Transform parent, Color color)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = objectName;
            pillar.transform.position = position;
            pillar.transform.localScale = new Vector3(2.6f, 6.2f, 2.6f);
            pillar.transform.SetParent(parent);
            pillar.GetComponent<Renderer>().material.color = color;
            TrySetVisionObstacleLayer(pillar);
            EnsureFogObject(pillar, BattlefieldFogRequirement.Explored);
        }

        public static void CreateBanner(string objectName, Vector3 position, Color bannerColor, Transform parent)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = objectName + " Pole";
            pole.transform.position = position;
            pole.transform.localScale = new Vector3(0.28f, 7.4f, 0.28f);
            pole.transform.SetParent(parent);
            pole.GetComponent<Renderer>().material.color = new Color(0.34f, 0.28f, 0.2f);

            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = objectName;
            cloth.transform.position = position + new Vector3(3.8f, 3.6f, 0f);
            cloth.transform.localScale = new Vector3(7.6f, 4.2f, 0.16f);
            cloth.transform.SetParent(parent);
            cloth.GetComponent<Renderer>().material.color = bannerColor;

            TrySetVisionObstacleLayer(pole);
            TrySetVisionObstacleLayer(cloth);
            EnsureFogObject(pole, BattlefieldFogRequirement.Explored);
            EnsureFogObject(cloth, BattlefieldFogRequirement.Explored);
        }

        public static void EnsureFogObject(GameObject target, BattlefieldFogRequirement requirement)
        {
            BattlefieldFogObject fogObject = target.GetComponent<BattlefieldFogObject>();
            if (fogObject == null)
            {
                fogObject = target.AddComponent<BattlefieldFogObject>();
            }

            fogObject.Configure(requirement);
        }
    }
}
