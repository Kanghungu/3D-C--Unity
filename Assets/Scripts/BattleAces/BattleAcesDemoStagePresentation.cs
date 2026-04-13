using Game.Prototype;
using Game.Settings;
using Game.Units;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.BattleAces
{
    /// <summary>
    /// Classic Duel demo-stage presentation pass for NewSampleScene.
    /// Focuses on battlefield tone, readable landmark silhouettes, and a denser
    /// "ritual military arena" feeling without requiring authored assets.
    /// </summary>
    public static class BattleAcesDemoStagePresentation
    {
        private const string StageRootName = "BA_DemoStage_ClassicDuel";

        private static Texture2D cachedDiagonalGradient;

        /// <summary>대각선 그라데이션 텍스처 캐시. 빌드 버전이 바뀌면 무효화(에디터·재컴파일 시 자동)</summary>
        private static int cachedDiagonalGradientBuildVersion = -1;

        private const int DiagonalGradientBuildVersion = 4;

        private static Vector3? cachedClassicDuelPlaneScaleForAtmosphere;

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
            SpawnProcessionalLanes(root, mirrorX);
            SpawnBattleAxisGuideSpines(root, mirrorX);
            SpawnSanctumFrames(root, mirrorX);
            SpawnPerimeterShrines(root, mirrorX);
            SpawnCentralDais(root);
        }

        public static void ClearClassicDuelAtmosphereCache()
        {
            cachedClassicDuelPlaneScaleForAtmosphere = null;
        }

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
            float fogStart = Mathf.Clamp(
                halfExtent * BattleAcesClassicDuelAtmosphereTuning.FogStartHalfExtentFactor,
                BattleAcesClassicDuelAtmosphereTuning.FogStartDistanceMin,
                BattleAcesClassicDuelAtmosphereTuning.FogStartDistanceMax);
            float fogEnd = Mathf.Clamp(
                halfExtent * BattleAcesClassicDuelAtmosphereTuning.FogEndHalfExtentFactor,
                BattleAcesClassicDuelAtmosphereTuning.FogEndDistanceMin,
                BattleAcesClassicDuelAtmosphereTuning.FogEndDistanceMax);

            float distMul = GameUserSettings.BattleFogDistanceScale;
            fogStart *= distMul;
            fogEnd *= distMul;
            fogStart = Mathf.Max(BattleAcesClassicDuelAtmosphereTuning.FogDistanceFloor, fogStart);
            fogEnd = Mathf.Max(fogStart + BattleAcesClassicDuelAtmosphereTuning.FogDistanceMinimumGap, fogEnd);

            float fogWeight = GameUserSettings.BattleFogIntensity01;
            float fogColorMix01 = Mathf.Clamp01(fogWeight * 0.985f + 0.015f);
            Color fogColor = Color.Lerp(BattleAcesArtDirection.CameraBackdrop, BattleAcesArtDirection.FogHorizon, fogColorMix01);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = BattleAcesArtDirection.AmbientSky;
            RenderSettings.ambientEquatorColor = BattleAcesArtDirection.AmbientEquator;
            RenderSettings.ambientGroundColor = BattleAcesArtDirection.AmbientGround;
            RenderSettings.ambientIntensity = BattleAcesClassicDuelAtmosphereTuning.AmbientIntensity;

            Camera main = Camera.main;
            if (main != null)
            {
                main.clearFlags = CameraClearFlags.SolidColor;
                main.backgroundColor = BattleAcesArtDirection.CameraBackdrop;
                main.farClipPlane = Mathf.Max(
                    main.farClipPlane,
                    fogEnd + BattleAcesClassicDuelAtmosphereTuning.CameraFarClipBeyondFogEnd);
            }

            TryWarmDirectionalLight();
            ApplyDemoStagePostEffect(main);
        }

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
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.type != LightType.Directional)
                {
                    continue;
                }

                // ?ъ뿉 諛뺥엺 諛⑺뼢愿??됱씠 ?쒓컖媛곸씠?대룄 ?붾젅?????댄듃 履쎌쑝濡??섎졃(湲곕?移?怨좎젙)
                Color paletteKey = Color.Lerp(
                    Color.white,
                    BattleAcesArtDirection.KeyLightTint,
                    BattleAcesClassicDuelAtmosphereTuning.DirectionalColorKeyTintMix);
                light.color = Color.Lerp(
                    light.color,
                    paletteKey,
                    BattleAcesClassicDuelAtmosphereTuning.DirectionalColorSnapTowardsPalette);
                light.intensity = Mathf.Clamp(
                    light.intensity * BattleAcesClassicDuelAtmosphereTuning.DirectionalIntensityScale,
                    BattleAcesClassicDuelAtmosphereTuning.DirectionalIntensityMin,
                    BattleAcesClassicDuelAtmosphereTuning.DirectionalIntensityMax);
                light.shadowStrength = Mathf.Clamp(
                    light.shadowStrength + BattleAcesClassicDuelAtmosphereTuning.DirectionalShadowStrengthBias,
                    BattleAcesClassicDuelAtmosphereTuning.DirectionalShadowStrengthMin,
                    BattleAcesClassicDuelAtmosphereTuning.DirectionalShadowStrengthMax);

                // ?ъ뿉 洹몃┝?먭? 爰쇱졇 ?덉쑝硫??좊떅 諛쒕컩????蹂댁씠誘濡??뚰봽?몃쭔 耳?嫄곕━쨌罹먯뒪耳?대뱶???덉쭏 ?꾨━??
                if (light.shadows == LightShadows.None)
                {
                    light.shadows = LightShadows.Soft;
                }

                light.shadowNormalBias = Mathf.Clamp(light.shadowNormalBias, 0.02f, 0.65f);
                light.shadowBias = Mathf.Clamp(light.shadowBias, 0.02f, 0.65f);
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

            // 吏硫? ??? 硫뷀깉由?룹쨷媛??ㅻТ?????용튆 ?앹옱쨌湲덉냽 ?뚮씪?ㅽ떛 ?먮굦 ?꾪솕(BATTLE_ACES_ART_DIRECTION)
            const float groundMetallic = 0.1f;
            const float groundSmoothness = 0.28f;

            if (stageMat.HasProperty("_Glossiness"))
            {
                stageMat.SetFloat("_Glossiness", groundSmoothness);
            }

            if (stageMat.HasProperty("_Metallic"))
            {
                stageMat.SetFloat("_Metallic", groundMetallic);
            }

            if (stageMat.HasProperty("_Smoothness"))
            {
                stageMat.SetFloat("_Smoothness", groundSmoothness);
            }

            renderer.material = stageMat;
        }

        private static Texture2D GetOrCreateDiagonalGradient()
        {
            if (cachedDiagonalGradient != null && cachedDiagonalGradientBuildVersion == DiagonalGradientBuildVersion)
            {
                return cachedDiagonalGradient;
            }

            if (cachedDiagonalGradient != null)
            {
                Object.Destroy(cachedDiagonalGradient);
                cachedDiagonalGradient = null;
            }

            // ?댁긽???뚰룺 ?곹뼢 + ?몄씠利?諛대뱶 ???먭굅由ъ뿉?쒕룄 吏덇컧?????됰㈃?곸쑝濡??쏀옒
            cachedDiagonalGradient = BuildDiagonalArenaGradient(384, 384);
            cachedDiagonalGradient.wrapMode = TextureWrapMode.Clamp;
            cachedDiagonalGradient.filterMode = FilterMode.Bilinear;
            cachedDiagonalGradient.anisoLevel = 8;
            cachedDiagonalGradientBuildVersion = DiagonalGradientBuildVersion;
            return cachedDiagonalGradient;
        }

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
                    float antiD = (u + (1f - v)) * 0.5f;
                    float edge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                    float edgeMask = 1f - Mathf.SmoothStep(0.08f, 0.22f, edge);
                    float centralBand = 1f - Mathf.SmoothStep(0.02f, 0.1f, Mathf.Abs(d - 0.5f));
                    float crossBand = 1f - Mathf.SmoothStep(0.025f, 0.12f, Mathf.Abs(antiD - 0.5f));
                    float panelLine = PanelLine(u, 7f, 0.024f) + PanelLine(v, 7f, 0.024f);
                    float fineGrid = PanelLine(u, 16f, 0.012f) + PanelLine(v, 16f, 0.012f);
                    float centerDistance = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));
                    float sanctumRing = 1f - Mathf.SmoothStep(0.12f, 0.18f, Mathf.Abs(centerDistance - 0.2f));

                    Color baseA = Color.Lerp(coolDeep, mid, Mathf.SmoothStep(0f, 1f, d));
                    Color baseB = Color.Lerp(mid, warmRim, Mathf.SmoothStep(0.28f, 1f, d));
                    Color color = Color.Lerp(baseA, baseB, 0.6f);
                    color = Color.Lerp(color, warmRim * 0.92f, edgeMask * 0.4f);
                    color = Color.Lerp(color, BattleAcesArtDirection.PointTeal * 0.48f, centralBand * 0.18f);
                    color = Color.Lerp(color, BattleAcesArtDirection.GunmetalLift, crossBand * 0.14f);
                    color = Color.Lerp(color, BattleAcesArtDirection.PointTeal, sanctumRing * 0.12f);
                    color = Color.Lerp(color, BattleAcesArtDirection.GunmetalDark, Mathf.Clamp01(panelLine) * 0.19f);
                    color = Color.Lerp(color, BattleAcesArtDirection.GunmetalLift, Mathf.Clamp01(fineGrid) * 0.04f);

                    // Add subtle value-only texture so the ground reads less flat from the RTS camera.
                    float grain =
                        (Mathf.PerlinNoise(u * 19.3f + 1.71f, v * 19.3f + 2.29f) - 0.5f) * 0.052f;
                    float band = Mathf.Sin(v * Mathf.PI * 28f) * 0.018f;
                    float micro = (Mathf.PerlinNoise(u * 61f, v * 61f) - 0.5f) * 0.022f;
                    // 거시 사구 능선 음영 — 줌 아웃에서도 평면이 아닌 고도감
                    float macroRidge1 = Mathf.Sin((u * 2.1f + v * 1.7f) * Mathf.PI * 2.3f) * 0.056f;
                    float macroRidge2 = Mathf.Cos((u * 1.35f - v * 2.45f) * Mathf.PI * 1.95f) * 0.048f;
                    float macroRidge3 = Mathf.Sin(u * Mathf.PI * 3.15f + v * Mathf.PI * 2.55f) * 0.034f;
                    float tone = grain + band + micro + macroRidge1 + macroRidge2 + macroRidge3;
                    color.r = Mathf.Clamp01(color.r + tone);
                    color.g = Mathf.Clamp01(color.g + tone);
                    color.b = Mathf.Clamp01(color.b + tone);

                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply(false, true);
            return tex;
        }

        private static float PanelLine(float coord, float count, float thickness)
        {
            float cell = Mathf.Repeat(coord * count, 1f);
            float dist = Mathf.Min(cell, 1f - cell);
            return 1f - Mathf.SmoothStep(0f, thickness, dist);
        }

        private static void SpawnLowTerrainRolls(Transform parent, bool mirrorX)
        {
            Color earth = BattleAcesArtDirection.TerrainBerm;

            // 큐브 중심 Y = scale.y * 0.5f 근처로 두어 지면(y≈0)에 안착
            AddBerm(
                parent,
                "DemoRoll_A",
                M(new Vector3(28f, 0.74f, -40f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? -12f : 12f, 0f),
                new Vector3(26f, 1.48f, 13f),
                earth);

            AddBerm(
                parent,
                "DemoRoll_B",
                M(new Vector3(-30f, 0.66f, 38f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? 8f : -8f, 0f),
                new Vector3(24f, 1.32f, 15f),
                earth);

            AddBerm(
                parent,
                "DemoRidge_C",
                M(new Vector3(0f, 0.58f, 46f), mirrorX),
                Quaternion.identity,
                new Vector3(46f, 1.16f, 9f),
                earth);

            // 추가 사구·능선 — 주름 방향을 달리해 사막 능선 느낌
            AddBerm(
                parent,
                "DemoRoll_D",
                M(new Vector3(-22f, 0.34f, -50f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? 52f : -52f, 0f),
                new Vector3(18f, 0.68f, 7.5f),
                earth);

            AddBerm(
                parent,
                "DemoRoll_E",
                M(new Vector3(24f, 0.38f, 50f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? -30f : 30f, 0f),
                new Vector3(15f, 0.76f, 8f),
                earth);

            AddBerm(
                parent,
                "DemoRoll_F",
                M(new Vector3(-44f, 0.45f, 6f), mirrorX),
                Quaternion.Euler(0f, mirrorX ? 78f : -78f, 0f),
                new Vector3(11f, 0.9f, 26f),
                earth);
        }

        private static void SpawnSilhouetteObstacles(Transform parent, bool mirrorX)
        {
            Color monolith = BattleAcesArtDirection.ObstacleMonolith;
            Color slab = BattleAcesArtDirection.ObstacleSlab;
            Color monolithAccent = Color.Lerp(monolith, BattleAcesArtDirection.PointTeal, 0.1f);

            AddProp(parent, "DemoMonolith_A", M(new Vector3(34f, 2.15f, -30f), mirrorX), Quaternion.identity, new Vector3(3f, 4.6f, 3f), monolithAccent, true, true, 1.85f);
            AddProp(parent, "DemoMonolith_B", M(new Vector3(-30f, 2.05f, 30f), mirrorX), Quaternion.Euler(0f, 18f, 0f), new Vector3(2.6f, 4.4f, 2.6f), monolith, true, true, 1f);
            AddProp(parent, "DemoSlab_C", M(new Vector3(0f, 1.02f, -42f), mirrorX), Quaternion.identity, new Vector3(12f, 1.45f, 1.05f), slab, true, true, 1f);

            Vector3 archBase = M(new Vector3(-36f, 1.72f, -12f), mirrorX);
            AddProp(parent, "DemoArchL", archBase + new Vector3(-2.8f, 0f, 0f), Quaternion.identity, new Vector3(1.8f, 3.8f, 1.8f), monolith, true, true, 1f);
            AddProp(parent, "DemoArchR", archBase + new Vector3(2.8f, 0f, 0f), Quaternion.identity, new Vector3(1.8f, 3.8f, 1.8f), monolith, true, true, 1f);
            AddProp(parent, "DemoArchLintel", archBase + new Vector3(0f, 2.05f, 0f), Quaternion.identity, new Vector3(6.8f, 0.48f, 1.05f), slab, true, true, 1f);

            AddProp(parent, "DemoShard_D", M(new Vector3(32f, 1.08f, 16f), mirrorX), Quaternion.Euler(0f, -28f, 0f), new Vector3(1.15f, 2.2f, 5.5f), slab, true, true, 1f);
        }

        private static void SpawnProcessionalLanes(Transform parent, bool mirrorX)
        {
            Color lane = Color.Lerp(BattleAcesArtDirection.GunmetalLift, BattleAcesArtDirection.PointTeal, 0.18f);
            Color enemyLane = Color.Lerp(BattleAcesArtDirection.ObstacleSlab, BattleAcesArtDirection.EnemyEmber, 0.16f);

            AddProp(parent, "Lane_Player_Main", M(new Vector3(-24f, 0.06f, -18f), mirrorX), Quaternion.identity, new Vector3(26f, 0.06f, 1.5f), lane, false, false, 0.8f);
            AddProp(parent, "Lane_Player_SideA", M(new Vector3(-12f, 0.05f, -8f), mirrorX), Quaternion.Euler(0f, 22f, 0f), new Vector3(18f, 0.05f, 1f), lane, false, false, 0.64f);
            AddProp(parent, "Lane_Player_SideB", M(new Vector3(-30f, 0.05f, -28f), mirrorX), Quaternion.Euler(0f, -22f, 0f), new Vector3(18f, 0.05f, 1f), lane, false, false, 0.64f);

            AddProp(parent, "Lane_Enemy_Main", M(new Vector3(24f, 0.06f, 18f), mirrorX), Quaternion.identity, new Vector3(26f, 0.06f, 1.5f), enemyLane, false, false, 0.72f);
            AddProp(parent, "Lane_Enemy_SideA", M(new Vector3(12f, 0.05f, 8f), mirrorX), Quaternion.Euler(0f, 22f, 0f), new Vector3(18f, 0.05f, 1f), enemyLane, false, false, 0.58f);
            AddProp(parent, "Lane_Enemy_SideB", M(new Vector3(30f, 0.05f, 28f), mirrorX), Quaternion.Euler(0f, -22f, 0f), new Vector3(18f, 0.05f, 1f), enemyLane, false, false, 0.58f);

            AddBeacon(parent, "LaneBeacon_Player_A", M(new Vector3(-17f, 0f, -18f), mirrorX), BattleAcesArtDirection.PointTeal);
            AddBeacon(parent, "LaneBeacon_Player_B", M(new Vector3(-30f, 0f, -18f), mirrorX), BattleAcesArtDirection.PointTeal);
            AddBeacon(parent, "LaneBeacon_Enemy_A", M(new Vector3(17f, 0f, 18f), mirrorX), BattleAcesArtDirection.EnemyEmber);
            AddBeacon(parent, "LaneBeacon_Enemy_B", M(new Vector3(30f, 0f, 18f), mirrorX), BattleAcesArtDirection.EnemyEmber);
        }

        /// <summary>
        /// 蹂몄쭊 履쎌뿉??以묒븰 ?ㅼ씠?ㅻ줈 ?쒖꽑쨌?대룞???댁뼱吏?꾨줉 ?뉗? 諛붾떏 ?ㅽ뙆???붾젅??誘뱀뒪留? ???낆꽱?????놁쓬).
        /// </summary>
        private static void SpawnBattleAxisGuideSpines(Transform parent, bool mirrorX)
        {
            Color allySpine = Color.Lerp(BattleAcesArtDirection.GunmetalMid, BattleAcesArtDirection.PointTeal, 0.07f);
            Color enemySpine = Color.Lerp(BattleAcesArtDirection.GunmetalMid, BattleAcesArtDirection.EnemyEmber, 0.07f);
            float yawDeg = mirrorX ? -40.5f : 40.5f;
            AddProp(
                parent,
                "GuideSpine_Player",
                M(new Vector3(-18f, 0.028f, -15f), mirrorX),
                Quaternion.Euler(0f, yawDeg, 0f),
                new Vector3(40f, 0.035f, 1.35f),
                allySpine,
                false,
                false,
                0.5f);
            AddProp(
                parent,
                "GuideSpine_Enemy",
                M(new Vector3(18f, 0.028f, 15f), mirrorX),
                Quaternion.Euler(0f, yawDeg, 0f),
                new Vector3(40f, 0.035f, 1.35f),
                enemySpine,
                false,
                false,
                0.5f);
        }

        private static void SpawnSanctumFrames(Transform parent, bool mirrorX)
        {
            Color frame = Color.Lerp(BattleAcesArtDirection.GunmetalLift, BattleAcesArtDirection.GunmetalDark, 0.24f);
            Color playerAccent = Color.Lerp(frame, BattleAcesArtDirection.PointTeal, 0.22f);
            Color enemyAccent = Color.Lerp(frame, BattleAcesArtDirection.EnemyEmber, 0.22f);

            SpawnCoreFrame(parent, "PlayerSanctum", M(new Vector3(-34f, 0f, -28f), mirrorX), playerAccent);
            SpawnCoreFrame(parent, "EnemySanctum", M(new Vector3(34f, 0f, 28f), mirrorX), enemyAccent);
        }

        private static void SpawnPerimeterShrines(Transform parent, bool mirrorX)
        {
            Color shrine = Color.Lerp(BattleAcesArtDirection.ObstacleMonolith, BattleAcesArtDirection.GunmetalLift, 0.18f);

            AddProp(parent, "Shrine_NW", M(new Vector3(-46f, 2.4f, -46f), mirrorX), Quaternion.identity, new Vector3(2.2f, 4.8f, 2.2f), shrine, false, false, 0.8f);
            AddProp(parent, "Shrine_NE", M(new Vector3(46f, 2.4f, -46f), mirrorX), Quaternion.identity, new Vector3(2.2f, 4.8f, 2.2f), shrine, false, false, 0.8f);
            AddProp(parent, "Shrine_SW", M(new Vector3(-46f, 2.4f, 46f), mirrorX), Quaternion.identity, new Vector3(2.2f, 4.8f, 2.2f), shrine, false, false, 0.8f);
            AddProp(parent, "Shrine_SE", M(new Vector3(46f, 2.4f, 46f), mirrorX), Quaternion.identity, new Vector3(2.2f, 4.8f, 2.2f), shrine, false, false, 0.8f);

            AddBeacon(parent, "ShrineBeacon_N", M(new Vector3(0f, 0f, -48f), mirrorX), BattleAcesArtDirection.PointTeal);
            AddBeacon(parent, "ShrineBeacon_S", M(new Vector3(0f, 0f, 48f), mirrorX), BattleAcesArtDirection.EnemyEmber);
        }

        private static void SpawnCentralDais(Transform parent)
        {
            Color baseStone = Color.Lerp(BattleAcesArtDirection.GunmetalMid, BattleAcesArtDirection.AshStone, 0.3f);
            Color accent = Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.14f);

            AddPrimitiveProp(parent, PrimitiveType.Cylinder, "CenterDais_Base", new Vector3(0f, 0.2f, 0f), Quaternion.identity, new Vector3(8f, 0.26f, 8f), baseStone, false, false, 0.5f);
            AddPrimitiveProp(parent, PrimitiveType.Cylinder, "CenterDais_Ring", new Vector3(0f, 0.28f, 0f), Quaternion.identity, new Vector3(5.8f, 0.05f, 5.8f), accent, false, false, 0.95f);
            AddPrimitiveProp(parent, PrimitiveType.Cylinder, "CenterDais_Spire", new Vector3(0f, 1.1f, 0f), Quaternion.identity, new Vector3(0.65f, 2f, 0.65f), accent, false, false, 0.88f);
        }

        private static void SpawnCoreFrame(Transform parent, string prefix, Vector3 center, Color accent)
        {
            Color stone = Color.Lerp(BattleAcesArtDirection.GunmetalLift, BattleAcesArtDirection.GunmetalDark, 0.24f);
            AddProp(parent, prefix + "_North", center + new Vector3(0f, 0.6f, 7.2f), Quaternion.identity, new Vector3(9.4f, 0.8f, 1f), stone, false, false, 0.36f);
            AddProp(parent, prefix + "_South", center + new Vector3(0f, 0.6f, -7.2f), Quaternion.identity, new Vector3(9.4f, 0.8f, 1f), stone, false, false, 0.36f);
            AddProp(parent, prefix + "_East", center + new Vector3(7.2f, 0.6f, 0f), Quaternion.identity, new Vector3(1f, 0.8f, 9.4f), stone, false, false, 0.36f);
            AddProp(parent, prefix + "_West", center + new Vector3(-7.2f, 0.6f, 0f), Quaternion.identity, new Vector3(1f, 0.8f, 9.4f), stone, false, false, 0.36f);
            AddBeacon(parent, prefix + "_BeaconA", center + new Vector3(-5.2f, 0f, 5.2f), accent);
            AddBeacon(parent, prefix + "_BeaconB", center + new Vector3(5.2f, 0f, -5.2f), accent);
        }

        private static void AddBeacon(Transform parent, string name, Vector3 position, Color accent)
        {
            AddProp(parent, name + "_Stem", position + new Vector3(0f, 0.72f, 0f), Quaternion.identity, new Vector3(0.3f, 1.2f, 0.3f), Color.Lerp(BattleAcesArtDirection.GunmetalLift, accent, 0.12f), false, false, 0.32f);
            AddPrimitiveProp(parent, PrimitiveType.Cylinder, name + "_Halo", position + new Vector3(0f, 1.42f, 0f), Quaternion.identity, new Vector3(0.62f, 0.08f, 0.62f), accent, false, false, 0.94f);
            AddPrimitiveProp(parent, PrimitiveType.Sphere, name + "_Orb", position + new Vector3(0f, 1.88f, 0f), Quaternion.identity, new Vector3(0.42f, 0.42f, 0.42f), accent, false, false, 1.12f);
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

        private static void AddProp(
            Transform parent,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Color color,
            bool visionObstacleLayer,
            bool enableCollider,
            float emissionIntensityMultiplier = 1f)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = scale;

            if (cube.TryGetComponent(out Collider collider))
            {
                collider.enabled = enableCollider;
            }

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

        private static void AddPrimitiveProp(
            Transform parent,
            PrimitiveType primitiveType,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Color color,
            bool visionObstacleLayer,
            bool enableCollider,
            float emissionIntensityMultiplier = 1f)
        {
            GameObject obj = GameObject.CreatePrimitive(primitiveType);
            obj.name = objectName;
            obj.transform.SetParent(parent, false);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.transform.localScale = scale;

            if (obj.TryGetComponent(out Collider collider))
            {
                collider.enabled = enableCollider;
            }

            if (obj.TryGetComponent(out Renderer renderer))
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
                    obj.layer = vis;
                }
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(obj, BattlefieldFogRequirement.Explored);
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
