using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Prototype infantry triangle plus aerial and elite role adjustments.
    /// </summary>
    public static class CombatTriangleRules
    {
        public static float ResolveDamage(UnitArchetype attacker, CombatTarget target, float baseDamage, bool isProjectile)
        {
            if (target == null)
            {
                return baseDamage;
            }

            SelectableUnit defenderUnit = target.GetComponent<SelectableUnit>();

            if (defenderUnit == null)
            {
                return baseDamage;
            }

            UnitArchetype defender = defenderUnit.Archetype;
            float damage = baseDamage * GetMatchupMultiplier(attacker, defender);

            if (isProjectile)
            {
                damage *= GetProjectileDefenseMultiplier(attacker, defender);
            }

            return Mathf.Max(0f, damage);
        }

        private static float GetMatchupMultiplier(UnitArchetype attacker, UnitArchetype defender)
        {
            if (attacker == UnitArchetype.Spearman && defender == UnitArchetype.ShieldInfantry) return 1.4f;
            if (attacker == UnitArchetype.ShieldInfantry && defender == UnitArchetype.Rifleman) return 1.35f;
            if (attacker == UnitArchetype.Rifleman && defender == UnitArchetype.Spearman) return 1.22f;
            if (attacker == UnitArchetype.ShieldInfantry && defender == UnitArchetype.Spearman) return 0.82f;
            if (attacker == UnitArchetype.Rifleman && defender == UnitArchetype.ShieldInfantry) return 0.72f;
            if (attacker == UnitArchetype.Spearman && defender == UnitArchetype.Rifleman) return 0.88f;
            if (attacker == UnitArchetype.SpecialWarrior && defender == UnitArchetype.ShieldInfantry) return 1.18f;
            if (attacker == UnitArchetype.SpecialWarrior && defender == UnitArchetype.Fighter) return 1.28f;
            if (attacker == UnitArchetype.Rifleman && defender == UnitArchetype.Fighter) return 1.72f;
            if (attacker == UnitArchetype.Artillery && defender == UnitArchetype.ShieldInfantry) return 1.16f;
            if (attacker == UnitArchetype.Fighter && defender == UnitArchetype.Rifleman) return 0.64f;
            return 1f;
        }

        private static float GetProjectileDefenseMultiplier(UnitArchetype attacker, UnitArchetype defender)
        {
            if (defender == UnitArchetype.ShieldInfantry && attacker == UnitArchetype.Rifleman) return 0.45f;
            // 창병 대 소총: 랜덤 완전 회피 대신 고정 감쇠(재현 가능·평균 피해량 유지)
            if (defender == UnitArchetype.Spearman && attacker == UnitArchetype.Rifleman) return 0.5f;
            if (defender == UnitArchetype.ShieldInfantry && attacker == UnitArchetype.Artillery) return 0.8f;
            if (defender == UnitArchetype.Fighter && attacker == UnitArchetype.Rifleman) return 1.2f;
            return 1f;
        }
    }
}
