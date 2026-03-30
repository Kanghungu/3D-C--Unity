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
                case UnitArchetype.Spearman:
                    Activate("Pike Brace", 5f, 10f, 0.9f, 1.22f, 1f, 0.72f);
                    return true;
                case UnitArchetype.ShieldInfantry:
                    Activate("Shield Wall", 5f, 11f, 0.72f, 1f, 0.9f, 0.45f);
                    return true;
                case UnitArchetype.Rifleman:
                    Activate("Overcharge", 4f, 9f, 1.18f, 1.18f, 0.74f, 1.15f);
                    return true;
                case UnitArchetype.Fighter:
                    Activate("Afterburn", 4f, 10f, 1.5f, 1.12f, 0.82f, 1.2f);
                    return true;
                case UnitArchetype.SpecialWarrior:
                    Activate("Blink Rush", 4f, 12f, 1.34f, 1.28f, 0.8f, 0.72f);
                    return true;
                case UnitArchetype.RoyalGuard:
                    Activate("Guardian Oath", 5f, 14f, 1f, 1.24f, 0.82f, 0.5f);
                    return true;
                case UnitArchetype.Artillery:
                    Activate("Siege Load", 5f, 11f, 0.8f, 1.35f, 0.72f, 1.1f);
                    return true;
                case UnitArchetype.MobileFortress:
                    Activate("War March", 5f, 16f, 1.18f, 1.18f, 0.88f, 0.76f);
                    return true;
                case UnitArchetype.AirborneCitadel:
                    Activate("Sky Salvo", 5f, 16f, 1.1f, 1.24f, 0.76f, 0.9f);
                    return true;
                default:
                    return false;
            }
        }

        public float GetMoveSpeedMultiplier() => moveSpeedMultiplier;
        public float GetAttackDamageMultiplier() => attackDamageMultiplier;
        public float GetAttackCooldownMultiplier() => attackCooldownMultiplier;
        public float ModifyIncomingDamage(float damage) => damage * damageTakenMultiplier;

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
