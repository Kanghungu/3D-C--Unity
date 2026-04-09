using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Units
{
    /// <summary>
    /// 런타임 생성 프리미티브(기본 Lit/Standard)에 알베도·이미터를 안전하게 넣어
    /// 아군/적/거점이 멀리서도 색·윤곽으로 구분되게 한다.
    /// </summary>
    public static class ReadablePrimitiveMaterialUtility
    {
        /// <summary>무기·차체 등 — 아주 약한 자기발광(실루엣만 살짝)</summary>
        public const float EmissionSubtleBody = 0.07f;

        /// <summary>깃발·표지 등 중간 강조</summary>
        public const float EmissionAccent = 0.55f;

        /// <summary>조준부·코어 비콘·거점 구슬 등 강한 포인트</summary>
        public const float EmissionStrongGlow = 0.92f;

        /// <summary>알베도만 변경(이미터는 건드리지 않음) — 선택 시 몸통 틴트 등</summary>
        public static void ApplyAlbedoOnly(Renderer renderer, Color albedo)
        {
            if (renderer == null)
            {
                return;
            }

            // sharedMaterial 은 에셋을 건드릴 수 있으므로 인스턴스만 수정
            TrySetMainColor(renderer.material, albedo);
        }

        /// <summary>알베도 + 이미터(강도 0이면 이미터 끔)</summary>
        public static void Apply(Renderer renderer, Color albedo, float emissionIntensity)
        {
            if (renderer == null)
            {
                return;
            }

            Material material = renderer.material;
            TrySetMainColor(material, albedo);

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
    }
}
