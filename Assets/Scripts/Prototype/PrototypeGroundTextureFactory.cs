using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Generates a high-quality procedural ground texture using FBM, domain warping,
    /// and multi-color blending. No external assets required.
    /// </summary>
    public static class PrototypeGroundTextureFactory
    {
        private const int TexSize = 1024;

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

                    pixels[y * TexSize + x] = style switch
                    {
                        GroundTextureStyle.DesertStone  => SampleDesertStone(u, v, seed, baseColor),
                        GroundTextureStyle.CrackedEarth => SampleCrackedEarth(u, v, seed, baseColor),
                        GroundTextureStyle.AshWasteland => SampleAshWasteland(u, v, seed, baseColor),
                        _                               => SampleDesertStone(u, v, seed, baseColor)
                    };
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 8;
            return tex;
        }

        // ── 사막 석판 ─────────────────────────────────────────────────────────
        private static Color SampleDesertStone(float u, float v, float seed, Color baseColor)
        {
            // 도메인 워핑: 좌표를 노이즈로 살짝 비틀어 유기적 느낌
            float wx = Mathf.PerlinNoise(u * 2.4f + seed + 3.7f, v * 2.4f + seed) - 0.5f;
            float wy = Mathf.PerlinNoise(u * 2.4f + seed, v * 2.4f + seed + 5.1f) - 0.5f;
            float wu = u + wx * 0.55f;
            float wv = v + wy * 0.55f;

            // 대형 지형 기복 (FBM 4옥타브)
            float large = Fbm(wu * 1.6f, wv * 1.6f, seed, 4);
            // 중형 돌 질감
            float mid = Fbm(u * 4.8f, v * 4.8f, seed + 1.9f, 3);
            // 미세 모래알 질감
            float grain = Fbm(u * 18f, v * 18f, seed + 4.2f, 2);
            // 고주파 표면 거칠기
            float micro = Mathf.PerlinNoise(u * 64f + seed, v * 64f + seed) * 0.5f
                        + Mathf.PerlinNoise(u * 128f + seed, v * 128f + seed) * 0.5f;

            float height = large * 0.50f + mid * 0.28f + grain * 0.14f + micro * 0.08f;

            // 균열 레이어 (2단계)
            float crackLarge = CrackIntensity(u, v, seed, 5.5f, 4.5f);
            float crackSmall = CrackIntensity(u, v, seed + 13f, 13f, 3.5f);
            float crack = Mathf.Clamp01(crackLarge * 0.65f + crackSmall * 0.5f);

            // 색상 레이어
            Color highlight = Color.Lerp(baseColor, new Color(1f, 0.94f, 0.78f), 0.32f);
            Color midtone   = baseColor;
            Color shadow    = Color.Lerp(baseColor, new Color(0.2f, 0.15f, 0.09f), 0.48f);
            Color crackTone = Color.Lerp(baseColor, new Color(0.06f, 0.04f, 0.03f), 0.78f);

            float t = Mathf.Clamp01(Remap(height, 0.28f, 0.78f, 0f, 1f));
            Color surface = t > 0.5f
                ? Color.Lerp(midtone, highlight, (t - 0.5f) * 2f)
                : Color.Lerp(shadow,  midtone,   t * 2f);

            return Color.Lerp(surface, crackTone, crack * 0.88f);
        }

        // ── 균열 대지 ─────────────────────────────────────────────────────────
        private static Color SampleCrackedEarth(float u, float v, float seed, Color baseColor)
        {
            float wx = Mathf.PerlinNoise(u * 1.8f + seed + 2.3f, v * 1.8f + seed) - 0.5f;
            float wy = Mathf.PerlinNoise(u * 1.8f + seed, v * 1.8f + seed + 7.1f) - 0.5f;
            float wu = u + wx * 0.7f;
            float wv = v + wy * 0.7f;

            float large  = Fbm(wu * 2.0f, wv * 2.0f, seed, 4);
            float mid    = Fbm(u * 5.5f,  v * 5.5f,  seed + 3.1f, 3);
            float grain  = Fbm(u * 22f,   v * 22f,   seed + 6.8f, 2);

            float height = large * 0.55f + mid * 0.30f + grain * 0.15f;

            // 굵고 선명한 균열이 특징
            float crackMain  = CrackIntensity(u, v, seed, 4.2f, 5.5f);
            float crackCross = CrackIntensity(u, v, seed + 8f, 7.5f, 4.0f);
            float crackFine  = CrackIntensity(u, v, seed + 19f, 16f, 3.0f);
            float crack = Mathf.Clamp01(crackMain * 0.7f + crackCross * 0.55f + crackFine * 0.35f);

            // 붉은 대지 느낌
            Color highlight = Color.Lerp(baseColor, new Color(1f, 0.85f, 0.6f), 0.28f);
            Color midtone   = baseColor;
            Color shadow    = Color.Lerp(baseColor, new Color(0.22f, 0.10f, 0.06f), 0.52f);
            Color crackTone = Color.Lerp(baseColor, new Color(0.04f, 0.02f, 0.01f), 0.85f);

            float t = Mathf.Clamp01(Remap(height, 0.22f, 0.76f, 0f, 1f));
            Color surface = t > 0.5f
                ? Color.Lerp(midtone, highlight, (t - 0.5f) * 2f)
                : Color.Lerp(shadow,  midtone,   t * 2f);

            return Color.Lerp(surface, crackTone, crack * 0.92f);
        }

        // ── 잿빛 황야 ─────────────────────────────────────────────────────────
        private static Color SampleAshWasteland(float u, float v, float seed, Color baseColor)
        {
            float wx = Mathf.PerlinNoise(u * 3.0f + seed + 6.1f, v * 3.0f + seed) - 0.5f;
            float wy = Mathf.PerlinNoise(u * 3.0f + seed, v * 3.0f + seed + 3.4f) - 0.5f;
            float wu = u + wx * 0.4f;
            float wv = v + wy * 0.4f;

            float large = Fbm(wu * 1.4f, wv * 1.4f, seed, 4);
            float mid   = Fbm(u * 4.2f,  v * 4.2f,  seed + 2.6f, 3);
            float dust  = Fbm(u * 12f,   v * 12f,   seed + 9.1f, 2);
            float micro = Mathf.PerlinNoise(u * 55f + seed, v * 55f + seed);

            float height = large * 0.48f + mid * 0.28f + dust * 0.16f + micro * 0.08f;

            // 화산재 얼룩 — 부드러운 어두운 패치
            float ashBlot = Fbm(u * 3.8f, v * 3.8f, seed + 14f, 3);
            float ashMask = Mathf.SmoothStep(0.60f, 0.72f, ashBlot);

            float crackMid  = CrackIntensity(u, v, seed, 6f, 3.5f);
            float crackFine = CrackIntensity(u, v, seed + 22f, 15f, 2.5f);
            float crack = Mathf.Clamp01(crackMid * 0.55f + crackFine * 0.4f);

            Color highlight = Color.Lerp(baseColor, new Color(0.9f, 0.92f, 0.95f), 0.22f);
            Color midtone   = baseColor;
            Color shadow    = Color.Lerp(baseColor, new Color(0.08f, 0.08f, 0.10f), 0.55f);
            Color ashColor  = Color.Lerp(baseColor, new Color(0.10f, 0.10f, 0.12f), 0.65f);
            Color crackTone = Color.Lerp(baseColor, new Color(0.03f, 0.03f, 0.04f), 0.80f);

            float t = Mathf.Clamp01(Remap(height, 0.25f, 0.72f, 0f, 1f));
            Color surface = t > 0.5f
                ? Color.Lerp(midtone, highlight, (t - 0.5f) * 2f)
                : Color.Lerp(shadow,  midtone,   t * 2f);

            surface = Color.Lerp(surface, ashColor, ashMask * 0.6f);
            return Color.Lerp(surface, crackTone, crack * 0.82f);
        }

        // ── FBM (Fractal Brownian Motion) ─────────────────────────────────────
        // 각 옥타브마다 진폭을 줄이고 주파수를 높여 쌓는 노이즈 합산.
        // 단순 Perlin보다 자연스러운 지형 구조가 나온다.
        private static float Fbm(float u, float v, float seed, int octaves)
        {
            float value = 0f;
            float amplitude = 0.5f;
            float frequency = 1f;
            float norm = 0f;

            for (int i = 0; i < octaves; i++)
            {
                norm += amplitude;
                value += amplitude * Mathf.PerlinNoise(
                    u * frequency + seed + i * 3.71f,
                    v * frequency + seed * 1.63f + i * 2.29f);
                amplitude *= 0.50f;
                frequency *= 2.07f;
            }

            return value / norm;
        }

        // ── 균열 강도 ─────────────────────────────────────────────────────────
        // 두 방향 노이즈의 ridge(능선) 함수를 합산해서 교차 균열망을 만든다.
        // 반환값: 0 = 열린 땅, 1 = 균열 중심.
        private static float CrackIntensity(float u, float v, float seed, float scale, float sharpness)
        {
            float n1 = Fbm(u * scale,        v * scale * 0.62f, seed,        3);
            float n2 = Fbm(u * scale * 0.62f, v * scale,        seed + 7.3f, 3);

            // valley → 0.5 근처에서 1, 가장자리에서 0
            float ridge1 = 1f - Mathf.Abs(n1 * 2f - 1f);
            float ridge2 = 1f - Mathf.Abs(n2 * 2f - 1f);

            ridge1 = Mathf.Pow(Mathf.Max(0f, ridge1), sharpness);
            ridge2 = Mathf.Pow(Mathf.Max(0f, ridge2), sharpness);

            return Mathf.Clamp01(ridge1 + ridge2 * 0.65f);
        }

        private static float Remap(float val, float inMin, float inMax, float outMin, float outMax)
        {
            return outMin + Mathf.Clamp01((val - inMin) / (inMax - inMin)) * (outMax - outMin);
        }
    }

    public enum GroundTextureStyle
    {
        DesertStone,
        CrackedEarth,
        AshWasteland,
    }
}
