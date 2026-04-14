using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// 임포트 메시에 팀 틴트만 주입 — 팔레트는 티얼/앰버 축 유지(과한 채도 금지).
    /// </summary>
    public static class PrototypeUnitVisualTintUtility
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        /// <summary>렌더러별 MaterialPropertyBlock 으로 알베도만 살짝 기울인다(인스턴스 머티리얼 남발 방지).</summary>
        public static void ApplyTeamTint(Renderer[] renderers, UnitTeam team)
        {
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            // 아군: 살짝 청백 / 적: 살짝 따뜻한 톤 — 원 메시 색을 완전 덮어쓰지 않음
            Color tint = team == UnitTeam.Player
                ? new Color(0.94f, 0.98f, 1f, 1f)
                : new Color(1f, 0.88f, 0.78f, 1f);

            var block = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                block.Clear();
                renderer.GetPropertyBlock(block);
                if (renderer.sharedMaterial != null)
                {
                    if (renderer.sharedMaterial.HasProperty(BaseColorId))
                    {
                        Color baseCol = renderer.sharedMaterial.GetColor(BaseColorId);
                        block.SetColor(BaseColorId, Color.Lerp(baseCol, tint, 0.38f));
                    }

                    if (renderer.sharedMaterial.HasProperty(ColorId))
                    {
                        Color legacy = renderer.sharedMaterial.GetColor(ColorId);
                        block.SetColor(ColorId, Color.Lerp(legacy, tint, 0.38f));
                    }
                }

                renderer.SetPropertyBlock(block);
            }
        }
    }
}
