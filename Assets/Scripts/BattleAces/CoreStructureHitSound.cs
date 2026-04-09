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

        /// <summary>플레이어 코어 — 타격음·전역 플래시·HUD 한 줄을 동일 unscaled 쿨다운으로 묶음</summary>
        private float lastPlayerCoreBundledFeedbackUnscaled = -999f;

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
            if (amount <= 0f)
            {
                return;
            }

            float i01 = Mathf.Clamp01(amount / 140f);

            // 적 코어 — 가벼운 타격음(유닛·아군 코어와 무게 구분)
            if (combatTarget != null && combatTarget.Team == UnitTeam.Enemy)
            {
                ProceduralAudioUtility.PlayStructureHit(i01);
                return;
            }

            // 플레이어 지휘 코어 — 유닛 피격보다 강한 체감(소리+플래시+HUD 동시에, 쿨다운 공유)
            if (combatTarget != null && combatTarget.Team == UnitTeam.Player)
            {
                float now = Time.unscaledTime;
                if (now - lastPlayerCoreBundledFeedbackUnscaled >=
                    BattleAcesFeedbackTiming.PlayerCoreHitFeedbackCooldownUnscaled)
                {
                    lastPlayerCoreBundledFeedbackUnscaled = now;
                    ProceduralAudioUtility.PlayCoreHit(i01);
                    if (BattleAcesMatchController.Instance != null)
                    {
                        BattleAcesPlayerHitFlash.NotifyPlayerDamage(Mathf.Clamp01(amount / 180f));
                        BattleAcesHudOverlay.PulsePlayerCoreHitHud();
                    }
                }

                return;
            }

            ProceduralAudioUtility.PlayCoreHit(i01);
        }
    }
}
