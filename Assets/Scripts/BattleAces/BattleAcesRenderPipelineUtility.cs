using UnityEngine;
using UnityEngine.Rendering;

namespace Game.BattleAces
{
    /// <summary>
    /// Built-in vs SRP 판별 — 데모 무대 포스트(스크린 톤 vs 볼륨) 분기용.
    /// </summary>
    public static class BattleAcesRenderPipelineUtility
    {
        /// <summary>기본 파이프라인이 없으면 Built-in (OnRenderImage 사용 가능).</summary>
        public static bool IsBuiltInRenderPipeline()
        {
            return GraphicsSettings.defaultRenderPipeline == null;
        }
    }
}
