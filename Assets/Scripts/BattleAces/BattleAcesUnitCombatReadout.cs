using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces HUD용 — 최근 피해 윈도우·교전 상태를 얇게 집계(다른 시스템과 분리).
    /// </summary>
    public sealed class BattleAcesUnitCombatReadout : MonoBehaviour
    {
        private const float DamageSumWindowUnscaled = 1.15f;

        private UnitHealth unitHealth;
        private UnitCombat unitCombat;

        private float damageSumInWindow;
        private float lastDamageReceivedUnscaled = -999f;
        private float damageWindowStartUnscaled = -999f;

        private void Awake()
        {
            unitHealth = GetComponent<UnitHealth>();
            unitCombat = GetComponent<UnitCombat>();
        }

        private void OnEnable()
        {
            if (unitHealth != null)
            {
                unitHealth.Damaged += OnDamaged;
            }
        }

        private void OnDisable()
        {
            if (unitHealth != null)
            {
                unitHealth.Damaged -= OnDamaged;
            }
        }

        /// <summary>들어온 피해만 기록(출처는 UnitHealth 경로에서 처리)</summary>
        private void OnDamaged(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now - lastDamageReceivedUnscaled > DamageSumWindowUnscaled || damageSumInWindow <= 0.001f)
            {
                damageSumInWindow = 0f;
                damageWindowStartUnscaled = now;
            }

            damageSumInWindow += amount;
            lastDamageReceivedUnscaled = now;
        }

        /// <summary>최근 창 구간 합산 피해(없으면 0)</summary>
        public float GetRecentDamageSumInWindow()
        {
            if (Time.unscaledTime - lastDamageReceivedUnscaled > DamageSumWindowUnscaled)
            {
                return 0f;
            }

            return damageSumInWindow;
        }

        /// <summary>초당 피해 근사(창 길이로 나눔)</summary>
        public bool TryGetRecentDamagePerSecond(out float dps)
        {
            dps = 0f;
            float sum = GetRecentDamageSumInWindow();
            if (sum <= 0.01f)
            {
                return false;
            }

            float now = Time.unscaledTime;
            float span = Mathf.Clamp(now - damageWindowStartUnscaled, 0.18f, DamageSumWindowUnscaled);
            dps = sum / span;
            return true;
        }

        /// <summary>현재 추적 중인 적이 있으면 true</summary>
        public bool IsEngagingHostile()
        {
            if (unitCombat == null)
            {
                return false;
            }

            CombatTarget t = unitCombat.CurrentTarget;
            return t != null && t.IsAlive;
        }
    }
}
