using Game.Units;
using UnityEngine;

namespace Game.Campaign.Scene
{
    /// <summary>
    /// 미션 씬 전용 스폰 마커 — 지오메트리와 함께 배치한다.
    /// </summary>
    public sealed class MissionSpawnPoint : MonoBehaviour
    {
        [SerializeField] private UnitTeam team = UnitTeam.Player;

        [Tooltip("덱 슬롯과 연결할 인덱스(생산형 미션에서 사용 예정)")]
        [SerializeField] private int deckSlotHint;

        [Tooltip("이 지점에서만 특정 병종을 스폰할 때(비우면 제한 없음)")]
        [SerializeField] private bool useArchetypeOverride;

        [SerializeField] private UnitArchetype archetypeOverride;

        public UnitTeam Team => team;
        public int DeckSlotHint => deckSlotHint;
        public bool UseArchetypeOverride => useArchetypeOverride;
        public UnitArchetype ArchetypeOverride => archetypeOverride;
    }
}
