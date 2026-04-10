using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Units
{
    /// <summary>
    /// 런타임 생성 프리미티브(기본 Lit/Standard)에 알베도·이미터를 안전하게 넣어
    /// 아군/적/거점이 멀리서도 색·윤곽으로 구분되게 한다.
    /// Battle Aces 데모: 메탈릭 대략 0~0.18, 스무스니스 0.2~0.45 — 지면 그라데이션 머티리얼은 약 0.12 / 0.3.
    /// </summary>
    public static class ReadablePrimitiveMaterialUtility
    {
        /// <summary>무기·차체 등 — 아주 약한 자기발광(실루엣만 살짝)</summary>
        public const float EmissionSubtleBody = 0.075f;

        /// <summary>깃발·표지 등 중간 강조</summary>
        public const float EmissionAccent = 0.55f;

        /// <summary>조준부·코어 비콘·거점 구슬 등 강한 포인트</summary>
        public const float EmissionStrongGlow = 0.92f;

        /// <summary>
        /// 런타임 프리미티브 전역 — 과발광이면 무대 전체가 싸 보이므로 상한 고정.
        /// (<c>BattleAcesDemoStagePresentation</c> 비콘 등은 이 한도 안에서만 튜닝)
        /// </summary>
        public const float EmissionIntensityClampMax = 1.28f;

        /// <summary>알베도만 변경(이미터는 건드리지 않음) — 선택 시 몸통 틴트 등</summary>
        public static void ApplyAlbedoOnly(Renderer renderer, Color albedo)
        {
            if (renderer == null)
            {
                return;
            }

            // sharedMaterial 은 에셋을 건드릴 수 있으므로 인스턴스만 수정
            Material material = renderer.material;
            TrySetMainColor(material, albedo);
            ConfigureSurface(material, albedo, 0f);
        }

        /// <summary>알베도 + 이미터(강도 0이면 이미터 끔)</summary>
        public static void Apply(Renderer renderer, Color albedo, float emissionIntensity)
        {
            if (renderer == null)
            {
                return;
            }

            emissionIntensity = Mathf.Clamp(emissionIntensity, 0f, EmissionIntensityClampMax);

            Material material = renderer.material;
            TrySetMainColor(material, albedo);
            ConfigureSurface(material, albedo, emissionIntensity);

            if (!material.HasProperty("_EmissionColor"))
            {
                return;
            }

            if (emissionIntensity <= 0.0001f)
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                return;
            }

            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", albedo * emissionIntensity);
        }

        private static void TrySetMainColor(Material material, Color albedo)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", albedo);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", albedo);
            }
        }

        private static void ConfigureSurface(Material material, Color albedo, float emissionIntensity)
        {
            if (material == null)
            {
                return;
            }

            float luminance = albedo.grayscale;
            // 낮은 채도 알베도에서도 금속·석재 구분이 조금 더 읽히게(팔레트 밖 색 추가 없음)
            float metallic = Mathf.Clamp(0.07f + luminance * 0.18f + emissionIntensity * 0.055f, 0.05f, 0.26f);
            float smoothness = Mathf.Clamp(0.22f + luminance * 0.22f + emissionIntensity * 0.14f, 0.2f, 0.68f);

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_SpecColor"))
            {
                material.SetColor("_SpecColor", Color.Lerp(Color.white, albedo, 0.22f));
            }

            material.enableInstancing = true;
        }
    }
}
