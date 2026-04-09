using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared faction/unit balance adjustments for the prototype battlefield.
    /// Keeps team-specific stat nudges in one place.
    /// </summary>
    public static class PrototypeUnitBalanceUtility
    {
        public static Vector3 GetAdjustedScale(UnitTeam team, UnitDefinition definition)
        {
            Vector3 scale = definition.Scale;
            return definition.Archetype switch
            {
                UnitArchetype.ShieldInfantry => scale * (team == UnitTeam.Player ? 1.24f : 0.96f),
                UnitArchetype.SpecialWarrior => scale * 1.14f,
                UnitArchetype.RoyalGuard => scale * 1.18f,
                UnitArchetype.Outrider => scale * 1.16f,
                UnitArchetype.MobileFortress => scale * 1.06f,
                UnitArchetype.AirborneCitadel => scale * 1.08f,
                _ => scale * (team == UnitTeam.Player ? 1.08f : 0.98f)
            };
        }

        public static float GetAdjustedMaxHealth(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Player)
            {
                return definition.Archetype switch
                {
                    UnitArchetype.Spearman => definition.MaxHealth * 1.34f,
                    UnitArchetype.ShieldInfantry => definition.MaxHealth * 1.42f,
                    UnitArchetype.Rifleman => definition.MaxHealth * 1.1f,
                    UnitArchetype.SpecialWarrior => definition.MaxHealth * 1.22f,
                    UnitArchetype.RoyalGuard => definition.MaxHealth * 1.38f,
                    UnitArchetype.Artillery => definition.MaxHealth * 1.18f,
                    UnitArchetype.Outrider => definition.MaxHealth * 1.08f,
                    _ => definition.MaxHealth
                };
            }

            return definition.Archetype switch
            {
                UnitArchetype.SpecialWarrior => definition.MaxHealth * 1.12f,
                UnitArchetype.RoyalGuard => definition.MaxHealth * 1.16f,
                UnitArchetype.Rifleman => definition.MaxHealth * 1.08f,
                UnitArchetype.Fighter => definition.MaxHealth * 0.92f,
                UnitArchetype.Outrider => definition.MaxHealth * 1.04f,
                _ => definition.MaxHealth * 0.96f
            };
        }

        public static float GetAdjustedMoveSpeed(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Player)
            {
                return definition.Archetype switch
                {
                    UnitArchetype.ShieldInfantry => definition.MoveSpeed * 0.9f,
                    UnitArchetype.SpecialWarrior => definition.MoveSpeed * 1.08f,
                    UnitArchetype.RoyalGuard => definition.MoveSpeed * 0.94f,
                    UnitArchetype.Outrider => definition.MoveSpeed * 1.22f,
                    UnitArchetype.MobileFortress => definition.MoveSpeed * 0.92f,
                    _ => definition.MoveSpeed
                };
            }

            return definition.Archetype switch
            {
                UnitArchetype.SpecialWarrior => definition.MoveSpeed * 1.04f,
                UnitArchetype.RoyalGuard => definition.MoveSpeed * 0.96f,
                UnitArchetype.Rifleman => definition.MoveSpeed * 1.14f,
                UnitArchetype.Fighter => definition.MoveSpeed * 1.12f,
                UnitArchetype.Outrider => definition.MoveSpeed * 1.12f,
                _ => definition.MoveSpeed * 0.98f
            };
        }

        public static float GetAdjustedAttackDamage(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Player)
            {
                return definition.Archetype switch
                {
                    UnitArchetype.Spearman => definition.AttackDamage * 1.12f,
                    UnitArchetype.Rifleman => definition.AttackDamage * 0.96f,
                    UnitArchetype.Fighter => definition.AttackDamage * 0.88f,
                    UnitArchetype.SpecialWarrior => definition.AttackDamage * 1.12f,
                    UnitArchetype.RoyalGuard => definition.AttackDamage * 1.24f,
                    UnitArchetype.Outrider => definition.AttackDamage * 1.08f,
                    _ => definition.AttackDamage
                };
            }

            return definition.Archetype switch
            {
                UnitArchetype.SpecialWarrior => definition.AttackDamage * 1.08f,
                UnitArchetype.RoyalGuard => definition.AttackDamage * 1.14f,
                UnitArchetype.Rifleman => definition.AttackDamage * 1.18f,
                UnitArchetype.Fighter => definition.AttackDamage * 0.96f,
                UnitArchetype.Outrider => definition.AttackDamage * 1.04f,
                _ => definition.AttackDamage * 0.98f
            };
        }

        public static float GetAdjustedAttackRange(UnitTeam team, UnitDefinition definition)
        {
            if (definition.Archetype == UnitArchetype.SpecialWarrior)
            {
                return definition.AttackRange + 0.35f;
            }

            if (definition.Archetype == UnitArchetype.RoyalGuard)
            {
                return definition.AttackRange + 0.2f;
            }

            if (definition.Archetype == UnitArchetype.Outrider)
            {
                return definition.AttackRange + 0.2f;
            }

            if (team == UnitTeam.Enemy && definition.Archetype == UnitArchetype.Rifleman)
            {
                return definition.AttackRange + 0.5f;
            }

            return definition.AttackRange;
        }
    }
}
