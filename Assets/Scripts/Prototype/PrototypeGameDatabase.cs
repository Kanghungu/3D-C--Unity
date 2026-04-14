using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Holds prototype unit definitions so gameplay can scale beyond hard-coded stats.
    /// </summary>
    public class PrototypeGameDatabase : MonoBehaviour
    {
        [SerializeField] private List<UnitDefinition> unitDefinitions = new();

        public IReadOnlyList<UnitDefinition> UnitDefinitions => unitDefinitions;

        private void Awake()
        {
            EnsureDefaults();
        }

        public UnitDefinition GetDefinition(UnitArchetype archetype)
        {
            EnsureDefaults();

            foreach (UnitDefinition unitDefinition in unitDefinitions)
            {
                if (unitDefinition != null && unitDefinition.Archetype == archetype)
                {
                    return unitDefinition;
                }
            }

            return unitDefinitions.Count > 0 ? unitDefinitions[0] : null;
        }

        public IReadOnlyList<UnitDefinition> GetProductionOptions()
        {
            EnsureDefaults();
            return unitDefinitions;
        }

        private void EnsureDefaults()
        {
            if (unitDefinitions.Count > 0)
            {
                return;
            }

            unitDefinitions.Add(CreateDefinition(UnitArchetype.Spearman, "Spear Cohort", PrimitiveType.Capsule, new Vector3(0.96f, 1.32f, 0.96f), 74f, 5.2f, 0.22f, 2.8f, 18f, 0.95f, 8.4f, 3.2f, false, 0f, 0f, 0f, 0.6f, 0, false, 1f, new Color(0.78f, 0.8f, 0.9f), new Color(0.93f, 0.46f, 0.28f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.ShieldInfantry, "Shield Legionary", PrimitiveType.Cube, new Vector3(1.12f, 1.18f, 0.9f), 92f, 4.4f, 0.18f, 2.3f, 15f, 0.88f, 7.4f, 3.5f, false, 0f, 0f, 0f, 0.6f, 0, false, 1f, new Color(0.84f, 0.8f, 0.7f), new Color(0.96f, 0.62f, 0.34f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.Rifleman, "Long Rifle Cohort", PrimitiveType.Cylinder, new Vector3(0.88f, 1.08f, 0.88f), 48f, 5.9f, 0.16f, 10.8f, 12f, 0.62f, 13.6f, 4.1f, true, 26f, 0.42f, 0f, 0.45f, 0, false, 1f, new Color(0.68f, 0.82f, 0.92f), new Color(1f, 0.52f, 0.22f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.Fighter, "Strike Fighter", PrimitiveType.Sphere, new Vector3(0.92f, 0.54f, 1.24f), 34f, 8.4f, 0.28f, 9.8f, 8f, 0.34f, 15f, 5.6f, true, 30f, 0.16f, 0f, 0.4f, 0, true, 7.5f, new Color(0.62f, 0.9f, 1f), new Color(1f, 0.58f, 0.28f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.SpecialWarrior, "Special Warrior", PrimitiveType.Capsule, new Vector3(0.98f, 1.28f, 0.98f), 128f, 6.8f, 0.14f, 4.8f, 24f, 0.58f, 10f, 7.4f, true, 22f, 0.12f, 0f, 0.62f, 24, false, 1f, new Color(0.96f, 0.94f, 0.8f), new Color(1f, 0.74f, 0.44f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.RoyalGuard, "King's Guard", PrimitiveType.Cube, new Vector3(1.08f, 1.36f, 0.96f), 178f, 5.4f, 0.12f, 4.2f, 28f, 0.54f, 11f, 0f, true, 24f, 0.08f, 0f, 0.75f, 10, false, 1f, new Color(0.9f, 0.96f, 1f), new Color(1f, 0.72f, 0.54f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.Artillery, "Siege Engine", PrimitiveType.Cylinder, new Vector3(1.34f, 0.95f, 1.34f), 56f, 4.5f, 0.18f, 12.4f, 22f, 1.6f, 15f, 6.4f, true, 14f, 3.4f, 2.8f, 1.25f, 0, false, 1f, new Color(0.74f, 0.72f, 0.8f), new Color(0.96f, 0.42f, 0.3f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.MobileFortress, "Moving Bastion", PrimitiveType.Cube, new Vector3(2.2f, 1.6f, 2.8f), 420f, 3.8f, 0.3f, 15.5f, 28f, 1.2f, 18f, 18f, true, 22f, 1.2f, 1.4f, 1.4f, 1, false, 1f, new Color(0.72f, 0.8f, 0.86f), new Color(0.86f, 0.42f, 0.3f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.AirborneCitadel, "Sky Citadel", PrimitiveType.Cube, new Vector3(2.6f, 1.8f, 2.6f), 360f, 4.6f, 0.3f, 18f, 16f, 0.9f, 22f, 22f, true, 24f, 0.6f, 0.8f, 1.5f, 1, true, 12f, new Color(0.78f, 0.9f, 1f), new Color(1f, 0.62f, 0.34f)));
            unitDefinitions.Add(CreateDefinition(UnitArchetype.Outrider, "Hover Lancer", PrimitiveType.Capsule, new Vector3(0.92f, 1.22f, 0.92f), 70f, 6.4f, 0.2f, 2.75f, 17f, 0.88f, 9f, 3.1f, false, 0f, 0f, 0f, 0.58f, 0, false, 1f, new Color(0.7f, 0.86f, 0.98f), new Color(0.92f, 0.48f, 0.26f)));

            // Resources Mecanim + 애니 주도 타격 — 공격 모션·AnimStrike 동기(에셋 없으면 경고만)
            BattleAcesDemoUnitAnimatorRuntime.BindMeleeDemoAnimatorsIfAvailable(unitDefinitions);
        }

        private static UnitDefinition CreateDefinition(
            UnitArchetype archetype,
            string displayName,
            PrimitiveType primitiveType,
            Vector3 scale,
            float maxHealth,
            float moveSpeed,
            float stoppingDistance,
            float attackRange,
            float attackDamage,
            float attackCooldown,
            float aggroRange,
            float productionDuration,
            bool usesProjectile,
            float projectileSpeed,
            float projectileArc,
            float splashRadius,
            float impactEffectScale,
            int maxPerTeam,
            bool isFlying,
            float hoverHeight,
            Color playerColor,
            Color enemyColor)
        {
            UnitDefinition definition = ScriptableObject.CreateInstance<UnitDefinition>();
            definition.Configure(
                archetype,
                displayName,
                primitiveType,
                scale,
                maxHealth,
                moveSpeed,
                stoppingDistance,
                attackRange,
                attackDamage,
                attackCooldown,
                aggroRange,
                productionDuration,
                usesProjectile,
                projectileSpeed,
                projectileArc,
                splashRadius,
                impactEffectScale,
                maxPerTeam,
                isFlying,
                hoverHeight,
                playerColor,
                enemyColor);
            return definition;
        }
    }
}
