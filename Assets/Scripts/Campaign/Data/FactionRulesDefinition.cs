using UnityEngine;

namespace Game.Campaign.Data
{
    /// <summary>
    /// 교단 vs 교단 — 한 축만 강하게 주는 팩션 규칙(예: A 생산 빠름 / B 유닛 튼튼).
    /// 인스펙터에서 ScriptableObject 자산으로 만든 뒤 미션에 할당한다.
    /// </summary>
    [CreateAssetMenu(fileName = "FactionRules", menuName = "Game/Campaign/Faction Rules", order = 0)]
    public sealed class FactionRulesDefinition : ScriptableObject
    {
        [Header("표시")]
        [Tooltip("내부 ID — 저장/대사 키에 사용")]
        [SerializeField] private string factionId = "faction_a";

        [Tooltip("에디터에서 구분용 이름")]
        [SerializeField] private string displayName = "재림 교단";

        [Header("한 축 차이(1 = 기본값)")]
        [Tooltip("1 미만이면 생산 시간 단축(빠른 생산)")]
        [SerializeField] private float productionDurationMultiplier = 1f;

        [Tooltip("1 초과면 유닛 최대 체력 보너스에 곱함(튼튼한 병력)")]
        [SerializeField] private float unitMaxHealthMultiplier = 1f;

        public string FactionId => factionId;
        public string DisplayName => displayName;
        public float ProductionDurationMultiplier => Mathf.Max(0.2f, productionDurationMultiplier);
        public float UnitMaxHealthMultiplier => Mathf.Max(0.5f, unitMaxHealthMultiplier);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(factionId))
            {
                factionId = "faction_unknown";
            }
        }
    }
}
