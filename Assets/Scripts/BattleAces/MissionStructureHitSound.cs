using Game.Audio;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>미션 구조물(이단 거점 등) 피격 시 가벼운 타격음</summary>
    public sealed class MissionStructureHitSound : MonoBehaviour
    {
        private UnitHealth health;

        private void Awake()
        {
            health = GetComponent<UnitHealth>();
            if (health != null)
            {
                health.Damaged += OnDamaged;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }
        }

        private void OnDamaged(float amount)
        {
            ProceduralAudioUtility.PlayStructureHit(Mathf.Clamp01(amount / 220f));
        }
    }
}
