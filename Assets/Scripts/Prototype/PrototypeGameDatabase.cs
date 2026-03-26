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

            unitDefinitions.Add(CreateDefinition(
                UnitArchetype.Vanguard,
                "Vanguard",
                PrimitiveType.Capsule,
                new Vector3(1.12f, 1.12f, 1.12f),
                78f,
                5.4f,
                0.22f,
                2.4f,
                18f,
                0.92f,
                7.8f,
                3.4f,
                false,
                0f,
                0f,
                0f,
                0.6f,
                new Color(0.5f, 0.75f, 1f),
                new Color(0.9f, 0.35f, 0.35f)));

            unitDefinitions.Add(CreateDefinition(
                UnitArchetype.Skirmisher,
                "Skirmisher",
                PrimitiveType.Sphere,
                new Vector3(1.04f, 1.04f, 1.04f),
                44f,
                7.1f,
                0.15f,
                8.8f,
                10f,
                0.62f,
                11f,
                4.2f,
                true,
                23f,
                0.65f,
                0f,
                0.45f,
                new Color(0.72f, 0.92f, 1f),
                new Color(1f, 0.58f, 0.32f)));

            unitDefinitions.Add(CreateDefinition(
                UnitArchetype.Artillery,
                "Artillery",
                PrimitiveType.Cylinder,
                new Vector3(1.3f, 0.9f, 1.3f),
                55f,
                4.6f,
                0.18f,
                12.5f,
                22f,
                1.6f,
                15f,
                6.5f,
                true,
                14f,
                3.4f,
                2.8f,
                1.25f,
                new Color(0.82f, 0.78f, 1f),
                new Color(1f, 0.45f, 0.68f)));
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
                playerColor,
                enemyColor);
            return definition;
        }
    }
}
