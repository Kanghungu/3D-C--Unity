using UnityEngine;

namespace Game.Units
{
    public partial class UnitCombat
    {
        private bool awaitingAnimStrike;
        private float animStrikeTimeoutRemainingUnscaled;
        private CombatTarget animStrikeVictim;

        /// <summary>애니 이벤트 <see cref="UnitCombatStrikeBridge.AnimStrike"/> 에서 호출</summary>
        public void NotifyAnimStrikeFromAnimation()
        {
            if (!awaitingAnimStrike)
            {
                return;
            }

            CompleteAnimDrivenStrike();
        }

        private bool ShouldUseAnimDrivenStrike()
        {
            if (selectableUnit == null || selectableUnit.Definition == null)
            {
                return false;
            }

            if (!selectableUnit.Definition.AnimCombatProfile.useAnimDrivenStrike)
            {
                return false;
            }

            if (GetComponent<UnitCombatStrikeBridge>() == null)
            {
                return false;
            }

            // Animator·컨트롤러 없이 켜면 타임아웃만 타격 → 체감 지연만 커짐
            Animator anim = GetComponent<Animator>();
            if (anim == null)
            {
                anim = GetComponentInChildren<Animator>(true);
            }

            return anim != null && anim.runtimeAnimatorController != null;
        }

        private void BeginAnimDrivenStrike(CombatTarget victim)
        {
            awaitingAnimStrike = true;
            animStrikeVictim = victim;
            animStrikeTimeoutRemainingUnscaled = Mathf.Max(0.12f, selectableUnit.Definition.AnimCombatProfile.strikeFallbackTimeoutUnscaled);
            TryPlayAttackWindupFeedback();
            UnitAnimationDriver driver = GetComponent<UnitAnimationDriver>();
            driver?.NotifyAttackSwing();
            float windMul = selectableUnit.Definition != null
                ? selectableUnit.Definition.MoveProfile.windupMoveSpeedMultiplier
                : 0.74f;
            mover?.SetExternalSpeedMultiplier(windMul);
        }

        private void TickAnimStrikeFallback()
        {
            if (!awaitingAnimStrike)
            {
                return;
            }

            animStrikeTimeoutRemainingUnscaled -= Time.unscaledDeltaTime;
            if (animStrikeTimeoutRemainingUnscaled <= 0f)
            {
                CompleteAnimDrivenStrike();
            }
        }

        private void CompleteAnimDrivenStrike()
        {
            awaitingAnimStrike = false;
            mover?.SetExternalSpeedMultiplier(1f);

            CombatTarget victim = animStrikeVictim;
            animStrikeVictim = null;
            if (victim == null || !victim.IsAlive || currentTarget != victim)
            {
                float cooldownMultiplier = abilityState != null ? abilityState.GetAttackCooldownMultiplier() : 1f;
                cooldownTimer = attackCooldown * cooldownMultiplier * 0.35f;
                return;
            }

            suppressAttackWindupOnce = true;
            if (usesProjectile)
            {
                LaunchProjectile(victim);
            }
            else
            {
                ApplyDirectDamage(victim);
            }

            recentAttackPulse = 1f;
            float cooldownMultiplier2 = abilityState != null ? abilityState.GetAttackCooldownMultiplier() : 1f;
            cooldownTimer = attackCooldown * cooldownMultiplier2;
        }

        private void TryPerformAttackWhenReady()
        {
            if (cooldownTimer > 0f)
            {
                return;
            }

            if (ShouldUseAnimDrivenStrike())
            {
                BeginAnimDrivenStrike(currentTarget);
                return;
            }

            if (usesProjectile)
            {
                LaunchProjectile(currentTarget);
            }
            else
            {
                ApplyDirectDamage(currentTarget);
            }

            recentAttackPulse = 1f;
            float cooldownMultiplier = abilityState != null ? abilityState.GetAttackCooldownMultiplier() : 1f;
            cooldownTimer = attackCooldown * cooldownMultiplier;
        }
    }
}
