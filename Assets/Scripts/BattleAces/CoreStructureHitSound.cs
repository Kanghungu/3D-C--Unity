using Game.Audio;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>플레이어 코어 피격 시 짧은 타격음(프로시저럴)</summary>
    public sealed class CoreStructureHitSound : MonoBehaviour
    {
        private UnitHealth health;
        private CombatTarget combatTarget;

        private void Awake()
        {
            health = GetComponent<UnitHealth>();
            combatTarget = GetComponent<CombatTarget>();
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
            float i01 = Mathf.Clamp01(amount / 140f);
            // 적 코어는 아군보다 가벼운 타격음(구분·연출 최소)
            if (combatTarget != null && combatTarget.Team == UnitTeam.Enemy)
            {
                ProceduralAudioUtility.PlayStructureHit(i01);
            }
            else
            {
                ProceduralAudioUtility.PlayCoreHit(i01);
            }

            if (BattleAcesMatchController.Instance != null &&
                amount > 0f &&
                combatTarget != null &&
                combatTarget.Team == UnitTeam.Player)
            {
                BattleAcesPlayerHitFlash.NotifyPlayerDamage(Mathf.Clamp01(amount / 180f));
            }
        }
    }
}
