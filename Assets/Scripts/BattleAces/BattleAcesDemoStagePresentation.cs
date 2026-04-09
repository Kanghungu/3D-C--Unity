using Game.Prototype;
using Game.Settings;
using Game.Units;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.BattleAces
{
    /// <summary>
    /// NewSampleScene 데모용 <b>ClassicDuel 한 레이아웃</b> 무대 연출만 담당.
    /// 색·안개는 <see cref="BattleAcesArtDirection"/> 과 맞춤 — 다른 ArenaLayoutKind 는 연출 생략.
    /// </summary>
    public static class BattleAcesDemoStagePresentation
    {
        private const string StageRootName = "BA_DemoStage_ClassicDuel";

        private static Texture2D cachedDiagonalGradient;

        /// <summary>마지막으로 ClassicDuel 대기 연출을 적용한 스케일 — 설정 슬라이더로 안개만 갱신할 때 사용</summary>
        private static Vector3? cachedClassicDuelPlaneScaleForAtmosphere;

        /// <summary>지면이 준비된 직후·내비 베이크 전에 호출 — <b>ClassicDuel</b> 만 무대 연출, 나머지 레이아웃은 기존 톤 유지</summary>
        public static void Apply(
            GameObject ground,
            Vector3 planeScale,
            Transform structuresParent,
            BattleArenaLayoutKind layoutKind,
            bool mirrorX)
        {
            if (layoutKind != BattleArenaLayoutKind.ClassicDuel)
            {
                cachedClassicDuelPlaneScaleForAtmosphere = null;
                RemoveDemoPostEffectsFromMainCamera();
                RenderSettings.fog = false;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = BattleAcesArtDirection.AmbientFlatNeutral;
                return;
            }

            ApplyAtmosphereAndCamera(planeScale);
            cachedClassicDuelPlaneScaleForAtmosphere = planeScale;
            ApplyGroundGradientMaterial(ground);

            if (structuresParent == null)
            {
                return;
            }

            if (structuresParent.Find(StageRootName) != null)
            {
                return;
            }

            Transform root = new GameObject(StageRootName).transform;
            root.SetParent(structuresParent, false);

            SpawnLowTerrainRolls(root, mirrorX);
            SpawnSilhouetteObstacles(root, mirrorX);
        }

        /// <summary>메뉴 복구·레이아웃 전환 시 캐시 무효화</summary>
        public static void ClearClassicDuelAtmosphereCache()
        {
            cachedClassicDuelPlaneScaleForAtmosphere = null;
        }

        /// <summary>O 키 설정에서 안개 거리/강도 변경 직후 호출</summary>
        public static void RefreshClassicDuelAtmosphereFromUserSettings()
        {
            if (!cachedClassicDuelPlaneScaleForAtmosphere.HasValue)
            {
                return;
            }

            ApplyAtmosphereAndCamera(cachedClassicDuelPlaneScaleForAtmosphere.Value);
        }

        private static void ApplyAtmosphereAndCamera(Vector3 planeScale)
        {
            float halfExtent = Mathf.Max(planeScale.x, planeScale.z) * 5f;
            float fogStart = Mathf.Clamp(halfExtent * 0.28f, 42f, 95f);
            float fogEnd = Mathf.Clamp(halfExtent * 0.92f, 160f, 340f);

            float distMul = GameUserSettings.BattleFogDistanceScale;
            fogStart *= distMul;
            fogEnd *= distMul;
            fogStart = Mathf.Max(8f, fogStart);
            fogEnd = Mathf.Max(fogStart + 12f, fogEnd);

            float fogWeight = GameUserSettings.BattleFogIntensity01;
            Color fogColor = Color.Lerp(BattleAcesArtDirection.CameraBackdrop, BattleAcesArtDirection.FogHorizon, fogWeight);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = BattleAcesArtDirection.AmbientSky;
            RenderSettings.ambientEquatorColor = BattleAcesArtDirection.AmbientEquator;
            RenderSettings.ambientGroundColor = BattleAcesArtDirection.AmbientGround;
            RenderSettings.ambientIntensity = 1.05f;

            Camera main = Camera.main;
            if (main != null)
            {
                main.clearFlags = CameraClearFlags.SolidColor;
                main.backgroundColor = BattleAcesArtDirection.CameraBackdrop;
            }

            TryWarmDirectionalLight();
            ApplyDemoStagePostEffect(main);
        }

        /// <summary>Built-in: <see cref="BattleAcesDemoStageScreenTone"/> — URP: 글로벌 볼륨(리플렉션)</summary>
        private static void ApplyDemoStagePostEffect(Camera main)
        {
            if (main == null)
            {
                return;
            }

            if (!BattleAcesRenderPipelineUtility.IsBuiltInRenderPipeline())
            {
                BattleAcesDemoStageScreenTone legacy = main.GetComponent<BattleAcesDemoStageScreenTone>();
                if (legacy != null)
                {
                    Object.Destroy(legacy);
                }

                BattleAcesUrpClassicDuelVolumeBootstrap.TryAttachUnderCamera(main);
                return;
            }

            BattleAcesUrpClassicDuelVolumeBootstrap.RemoveUnderCamera(main);

            Shader toneShader = Shader.Find("Hidden/BattleAcesDemoStageTone");
            if (toneShader == null)
            {
                return;
            }

            BattleAcesDemoStageScreenTone existing = main.GetComponent<BattleAcesDemoStageScreenTone>();
            if (existing == null)
            {
                existing = main.gameObject.AddComponent<BattleAcesDemoStageScreenTone>();
            }

            Material mat = new Material(toneShader);
            existing.Configure(mat);
        }

        /// <summary>메인 카메라에서 데모 스크린 톤만 제거 — 메뉴 복구·비 Classic 레이아웃 공용</summary>
        public static void RemoveDemoPostEffectsFromMainCamera()
        {
            Camera main = Camera.main;
            if (main == null)
            {
                return;
            }

            BattleAcesUrpClassicDuelVolumeBootstrap.RemoveUnderCamera(main);

            BattleAcesDemoStageScreenTone tone = main.GetComponent<BattleAcesDemoStageScreenTone>();
            if (tone != null)
            {
                Object.Destroy(tone);
            }
        }

        private static void TryWarmDirectionalLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.type != LightType.Directional)
                {
                    continue;
                }

                light.color = Color.Lerp(light.color, BattleAcesArtDirection.KeyLightTint, 0.35f);
                light.intensity = Mathf.Clamp(light.intensity * 0.92f, 0.65f, 1.35f);
                light.shadowStrength = Mathf.Clamp(light.shadowStrength, 0.55f, 0.85f);
                break;
            }
        }

        private static void ApplyGroundGradientMaterial(GameObject ground)
        {
            if (ground == null || !ground.TryGetComponent(out Renderer renderer))
            {
                return;
            }

            Texture2D gradient = GetOrCreateDiagonalGradient();

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                renderer.material.mainTexture = gradient;
                return;
            }

            // 이전에 붙인 런타임 지면 머티리얼이 있으면 제거(재시작·핫 리로드 시 누적 방지)
            Material previousShared = renderer.sharedMaterial;
            if (previousShared != null &&
                previousShared.name != null &&
                previousShared.name.StartsWith("BA_DemoGroundGradient", System.StringComparison.Ordinal))
            {
                Object.Destroy(previousShared);
            }

            Material stageMat = new Material(shader)
            {
                name = "BA_DemoGroundGradient"
            };

            if (stageMat.HasProperty("_BaseMap"))
            {
                stageMat.SetTexture("_BaseMap", gradient);
            }

            if (stageMat.HasProperty("_BaseColor"))
            {
                stageMat.SetColor("_BaseColor", Color.white);
            }

            stageMat.mainTexture = gradient;
            stageMat.color = Color.white;

            if (stageMat.HasProperty("_Glossiness"))
            {
                stageMat.SetFloat("_Glossiness", 0.22f);
            }

            if (stageMat.HasProperty("_Metallic"))
            {
                stageMat.SetFloat("_Metallic", 0.08f);
            }

            if (stageMat.HasProperty("_Smoothness"))
            {
                stageMat.SetFloat("_Smoothness", 0.22f);
            }

            renderer.material = stageMat;
        }

        private static Texture2D GetOrCreateDiagonalGradient()
        {
            if (cachedDiagonalGradient != null)
            {
                return cachedDiagonalGradient;
            }

            cachedDiagonalGradient = BuildDiagonalArenaGradient(192, 192);
            cachedDiagonalGradient.wrapMode = TextureWrapMode.Clamp;
            cachedDiagonalGradient.filterMode = FilterMode.Bilinear;
            cachedDiagonalGradient.anisoLevel = 2;
            return cachedDiagonalGradient;
        }

        /// <summary>대각 그라데이션 — 한 장면에서 깊이·톤 분리</summary>
        private static Texture2D BuildDiagonalArenaGradient(int width, int height)
        {
            width = Mathf.Max(2, width);
            height = Mathf.Max(2, height);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            Color coolDeep = BattleAcesArtDirection.GroundGradientCool;
            Color mid = BattleAcesArtDirection.GroundGradientMid;
            Color warmRim = BattleAcesArtDirection.GroundGradientAshRim;

            float denomX = Mathf.Max(1, width - 1);
            float denomY = Mathf.Max(1, height - 1);

            for (int y = 0; y < height; y++)
            {
                float v = y / denomY;
                for (int x = 0; x < width; x++)
                {
                    float u = x / denomX;
                    float d = (u + v) * 0.5f;
                    Color a = Color.Lerp(coolDeep, mid, Mathf.SmoothStep(0f, 1f, d));
                    Color b = Color.Lerp(mid, warmRim, Mathf.SmoothStep(0.35f, 1f, d));
                    tex.SetPixel(x, y, Color.Lerp(a, b, 0.55f));
                }
            }

            tex.Apply(false, true);
            return tex;
        }

        private static void SpawnLowTerrainRolls(Transform parent, bool mirrorX)
        {
            Color earth = BattleAcesArtDirection.TerrainBerm;

            // 맵 가장자리로 밀어 코어↔중앙 대각 통로가 덜 막히게(NavMesh 스모크 완화)
            AddBerm(
                parent,
                "DemoRoll_A",
                M(new Vector3(26f, 0.32f, -38f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? -12f : 12f, 0f),
                new Vector3(22f, 0.52f, 12f),
                earth);

            AddBerm(
                parent,
                "DemoRoll_B",
                M(new Vector3(-28f, 0.3f, 36f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? 8f : -8f, 0f),
                new Vector3(20f, 0.48f, 14f),
                earth);

            AddBerm(
                parent,
                "DemoRidge_C",
                M(new Vector3(0f, 0.28f, 44f), mirrorX),
                Quaternion.identity,
                new Vector3(40f, 0.45f, 7f),
                earth);
        }

        private static void AddBerm(Transform parent, string objectName, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = scale;
            if (cube.TryGetComponent(out Collider col))
            {
                col.enabled = true;
            }

            if (cube.TryGetComponent(out Renderer renderer))
            {
                ReadablePrimitiveMaterialUtility.Apply(renderer, color, ReadablePrimitiveMaterialUtility.EmissionSubtleBody * 0.6f);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(cube, BattlefieldFogRequirement.Explored);
        }

        private static void SpawnSilhouetteObstacles(Transform parent, bool mirrorX)
        {
            Color monolith = BattleAcesArtDirection.ObstacleMonolith;
            Color slab = BattleAcesArtDirection.ObstacleSlab;
            // 포인트 티얼을 한 군데만 살짝 — 스샷에서 ‘의식 색’ 힌트
            Color monolithAccent = Color.Lerp(monolith, BattleAcesArtDirection.PointTeal, 0.1f);

            AddProp(parent, "DemoMonolith_A", M(new Vector3(34f, 2.15f, -30f), mirrorX), Quaternion.identity, new Vector3(3f, 4.6f, 3f), monolithAccent, true, 1.85f);
            AddProp(parent, "DemoMonolith_B", M(new Vector3(-30f, 2.05f, 30f), mirrorX), Quaternion.Euler(0f, 18f, 0f), new Vector3(2.6f, 4.4f, 2.6f), monolith, true, 1f);
            AddProp(parent, "DemoSlab_C", M(new Vector3(0f, 1.02f, -42f), mirrorX), Quaternion.identity, new Vector3(12f, 1.45f, 1.05f), slab, true, 1f);

            Vector3 archBase = M(new Vector3(-36f, 1.72f, -12f), mirrorX);
            AddProp(parent, "DemoArchL", archBase + new Vector3(-2.8f, 0f, 0f), Quaternion.identity, new Vector3(1.8f, 3.8f, 1.8f), monolith, true, 1f);
            AddProp(parent, "DemoArchR", archBase + new Vector3(2.8f, 0f, 0f), Quaternion.identity, new Vector3(1.8f, 3.8f, 1.8f), monolith, true, 1f);
            AddProp(parent, "DemoArchLintel", archBase + new Vector3(0f, 2.05f, 0f), Quaternion.identity, new Vector3(6.8f, 0.48f, 1.05f), slab, true, 1f);

            AddProp(parent, "DemoShard_D", M(new Vector3(32f, 1.08f, 16f), mirrorX), Quaternion.Euler(0f, -28f, 0f), new Vector3(1.15f, 2.2f, 5.5f), slab, true, 1f);
        }

        private static void AddProp(
            Transform parent,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Color color,
            bool visionObstacleLayer,
            float emissionIntensityMultiplier = 1f)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = scale;

            if (cube.TryGetComponent(out Renderer renderer))
            {
                ReadablePrimitiveMaterialUtility.Apply(
                    renderer,
                    color,
                    ReadablePrimitiveMaterialUtility.EmissionSubtleBody * 0.45f * emissionIntensityMultiplier);
            }

            if (visionObstacleLayer)
            {
                int vis = LayerMask.NameToLayer("VisionObstacle");
                if (vis >= 0)
                {
                    cube.layer = vis;
                }
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(cube, BattlefieldFogRequirement.Explored);
        }

        private static Vector3 M(Vector3 world, bool mirrorX)
        {
            if (mirrorX)
            {
                world.x *= -1f;
            }

            return world;
        }
    }
}
