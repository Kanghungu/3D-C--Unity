#if UNITY_EDITOR
namespace Game.EditorTools
{
    /// <summary>
    /// 챕터 1 빌드에 넣을 씬 경로 단일 출처 — CampaignBuildMenu·스모크 검사가 동일 목록을 쓴다.
    /// </summary>
    public static class CampaignChapter1BuildPaths
    {
        public static readonly string[] RequiredScenes =
        {
            "Assets/Scenes/CampaignMenu.unity",
            "Assets/Scenes/NewSampleScene.unity",
        };
    }
}
#endif
