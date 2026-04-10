using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// ClassicDuel 무대용 경량 포스트 — 비네팅·미세 대비(Built-in + <see cref="Camera.OnRenderImage"/>).
    /// SRP(URP) 사용 시 <see cref="BattleAcesDemoStagePresentation"/> 가 이 컴포넌트 대신
    /// <see cref="BattleAcesUrpClassicDuelVolumeBootstrap"/> 으로 Global Volume 을 붙입니다.
    /// 빌드 스트리핑 방지: 에디터 Tools/Battle Aces/Graphics/Always Include Demo Stage Tone Shader.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleAcesDemoStageScreenTone : MonoBehaviour
    {
        private Material toneMaterial;

        public void Configure(Material material)
        {
            if (toneMaterial != null)
            {
                Destroy(toneMaterial);
            }

            toneMaterial = material;
        }

        private void OnDestroy()
        {
            if (toneMaterial != null)
            {
                Destroy(toneMaterial);
                toneMaterial = null;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (source == null)
            {
                Graphics.Blit(Texture2D.blackTexture, destination);
                return;
            }

            try
            {
                if (toneMaterial != null && toneMaterial.shader != null && toneMaterial.shader.isSupported)
                {
                    Graphics.Blit(source, destination, toneMaterial);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BattleAces] Demo stage screen tone disabled after render failure: {ex.Message}", this);
                if (toneMaterial != null)
                {
                    Destroy(toneMaterial);
                    toneMaterial = null;
                }

                enabled = false;
            }

            Graphics.Blit(source, destination);
        }
    }
}
