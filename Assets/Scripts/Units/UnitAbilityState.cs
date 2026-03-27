using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Stores temporary role-based combat and movement modifiers for a unit.
    /// </summary>
    public class UnitAbilityState : MonoBehaviour
    {
        private const float Epsilon = 0.001f;

        private SelectableUnit selectableUnit;
        private float moveSpeedMultiplier = 1f;
        private float attackDamageMultiplier = 1f;
        private float attackCooldownMultiplier = 1f;
        private float damageTakenMultiplier = 1f;
        private float durationTimer;
        private float cooldownTimer;
        private bool isActive;
        private string activeLabel = "Ready";

        public bool IsActive => isActive;
        public bool IsReady => cooldownTimer <= 0f && !isActive;
        public string StatusLabel => BuildStatusLabel();

        private void Awake()
        {
            selectableUnit = GetComponent<SelectableUnit>();
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (!isActive)
            {
                return;
            }

            durationTimer -= Time.deltaTime;

            if (durationTimer <= 0f)
            {
                ResetModifiers();
            }
        }

        public bool TryActivateRoleAbility()
        {
            if (!IsReady || selectableUnit == null)
            {
                return false;
            }

            switch (selectableUnit.Archetype)
            {
                case UnitArchetype.Vanguard:
                    Activate("Fortify", 5f, 10f, 0.82f, 1f, 1f, 0.55f);
                    return true;
                case UnitArchetype.Skirmisher:
                    Activate("Thrusters", 4f, 9f, 1.6f, 1f, 1f, 1f);
                    return true;
                case UnitArchetype.Artillery:
                    Activate("Siege Load", 5f, 11f, 0.8f, 1.35f, 0.72f, 1.1f);
                    return true;
                default:
                    return false;
            }
        }

        public float GetMoveSpeedMultiplier()
        {
            return moveSpeedMultiplier;
        }

        public float GetAttackDamageMultiplier()
        {
            return attackDamageMultiplier;
        }

        public float GetAttackCooldownMultiplier()
        {
            return attackCooldownMultiplier;
        }

        public float ModifyIncomingDamage(float damage)
        {
            return damage * damageTakenMultiplier;
        }

        private void Activate(string label, float duration, float cooldown, float moveMultiplier, float damageMultiplier, float cooldownMultiplier, float incomingDamageMultiplier)
        {
            activeLabel = label;
            durationTimer = duration;
            cooldownTimer = cooldown;
            moveSpeedMultiplier = moveMultiplier;
            attackDamageMultiplier = damageMultiplier;
            attackCooldownMultiplier = cooldownMultiplier;
            damageTakenMultiplier = incomingDamageMultiplier;
            isActive = true;
        }

        private void ResetModifiers()
        {
            moveSpeedMultiplier = 1f;
            attackDamageMultiplier = 1f;
            attackCooldownMultiplier = 1f;
            damageTakenMultiplier = 1f;
            durationTimer = 0f;
            isActive = false;
            activeLabel = "Cooldown";
        }

        private string BuildStatusLabel()
        {
            if (isActive)
            {
                return $"{activeLabel} {Mathf.CeilToInt(durationTimer)}s";
            }

            if (cooldownTimer > Epsilon)
            {
                return $"Cooldown {Mathf.CeilToInt(cooldownTimer)}s";
            }

            return "Ready";
        }
    }
}
