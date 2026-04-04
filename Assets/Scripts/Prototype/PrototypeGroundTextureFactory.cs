using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Generates a procedural ground texture using layered Perlin noise.
    /// Produces a cracked, battle-worn desert/stone surface without any external assets.
    /// </summary>
    public static class PrototypeGroundTextureFactory
    {
        // Texture resolution — 512 is a good balance of quality vs memory
        private const int TexSize = 512;

        public static Texture2D Generate(Color baseColor, GroundTextureStyle style = GroundTextureStyle.DesertStone)
        {
            Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGB24, mipChain: true);
            Color[] pixels = new Color[TexSize * TexSize];

            float seed = Random.Range(0f, 999f);

            for (int y = 0; y < TexSize; y++)
            {
                for (int x = 0; x < TexSize; x++)
                {
                    float u = (float)x / TexSize;
                    float v = (float)y / TexSize;

                    float value = style switch
                    {
                        GroundTextureStyle.DesertStone  => SampleDesertStone(u, v, seed),
                        GroundTextureStyle.CrackedEarth => SampleCrackedEarth(u, v, seed),
                        GroundTextureStyle.AshWasteland => SampleAshWasteland(u, v, seed),
                        _                               => SampleDesertStone(u, v, seed)
                    };

                    // 밝기 변화를 baseColor에 곱해서 색조 유지
                    float bright = Mathf.Clamp01(value);
                    pixels[y * TexSize + x] = new Color(
                        baseColor.r * bright,
                        baseColor.g * bright,
                        baseColor.b * bright
                    );
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 4;
            return tex;
        }

        // ── 사막 석판 ─────────────────────────────────────────────────────────
        // 큰 노이즈(땅 기복) + 중간 노이즈(돌 질감) + 작은 노이즈(모래 알갱이)
        private static float SampleDesertStone(float u, float v, float seed)
        {
            float large  = Mathf.PerlinNoise(u * 2.8f  + seed, v * 2.8f  + seed);
            float medium = Mathf.PerlinNoise(u * 7.4f  + seed, v * 7.4f  + seed);
            float fine   = Mathf.PerlinNoise(u * 22f   + seed, v * 22f   + seed);
            float grain  = Mathf.PerlinNoise(u * 58f   + seed, v * 58f   + seed);

            float base_ = large * 0.42f + medium * 0.32f + fine * 0.18f + grain * 0.08f;

            // 어두운 균열선 — 노이즈가 특정 임계값 근처면 어둡게
            float crackNoise = Mathf.PerlinNoise(u * 12f + seed + 3.7f, v * 12f + seed + 3.7f);
            float crackMask  = 1f - Mathf.SmoothStep(0.46f, 0.52f, crackNoise) * 0.55f;

            return Remap(base_ * crackMask, 0.18f, 0.88f, 0.62f, 1.0f);
        }

        // ── 균열 대지 ─────────────────────────────────────────────────────────
        private static float SampleCrackedEarth(float u, float v, float seed)
        {
            float large  = Mathf.PerlinNoise(u * 3.2f + seed, v * 3.2f + seed);
            float medium = Mathf.PerlinNoise(u * 8.6f + seed, v * 8.6f + seed);
            float fine   = Mathf.PerlinNoise(u * 24f  + seed, v * 24f  + seed);

            float base_ = large * 0.50f + medium * 0.34f + fine * 0.16f;

            // 균열: 여러 방향 노이즈의 교차점을 어둡게
            float crackA = Mathf.PerlinNoise(u * 14f + seed + 1.1f, v * 6f  + seed);
            float crackB = Mathf.PerlinNoise(u * 6f  + seed,        v * 14f + seed + 2.3f);
            float crack  = Mathf.Min(crackA, crackB);
            float crackMask = 1f - Mathf.SmoothStep(0.40f, 0.50f, crack) * 0.72f;

            return Remap(base_ * crackMask, 0.10f, 0.82f, 0.52f, 1.0f);
        }

        // ── 잿빛 황야 ─────────────────────────────────────────────────────────
        private static float SampleAshWasteland(float u, float v, float seed)
        {
            float large  = Mathf.PerlinNoise(u * 2.2f + seed, v * 2.2f + seed);
            float medium = Mathf.PerlinNoise(u * 6.8f + seed, v * 6.8f + seed);
            float fine   = Mathf.PerlinNoise(u * 18f  + seed, v * 18f  + seed);
            float dust   = Mathf.PerlinNoise(u * 44f  + seed, v * 44f  + seed);

            float base_ = large * 0.38f + medium * 0.30f + fine * 0.20f + dust * 0.12f;

            // 화산재 얼룩
            float ashBlot = Mathf.PerlinNoise(u * 9f + seed + 7.3f, v * 9f + seed + 7.3f);
            float ashMask = 1f - Mathf.SmoothStep(0.55f, 0.65f, ashBlot) * 0.45f;

            return Remap(base_ * ashMask, 0.20f, 0.80f, 0.48f, 0.96f);
        }

        private static float Remap(float val, float inMin, float inMax, float outMin, float outMax)
        {
            return outMin + (Mathf.Clamp01((val - inMin) / (inMax - inMin))) * (outMax - outMin);
        }
    }

    public enum GroundTextureStyle
    {
        DesertStone,
        CrackedEarth,
        AshWasteland,
    }
}
