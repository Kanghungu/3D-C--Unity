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
                new Vector3(1.18f, 1.24f, 1.18f),
                82f,
                5.1f,
                0.22f,
                2.4f,
                17f,
                0.95f,
                7.8f,
                3.6f,
                false,
                0f,
                0f,
                0f,
                0.6f,
                new Color(0.72f, 0.76f, 0.88f),
                new Color(0.87f, 0.34f, 0.26f)));

            unitDefinitions.Add(CreateDefinition(
                UnitArchetype.Skirmisher,
                "Skirmisher",
                PrimitiveType.Sphere,
                new Vector3(1.02f, 1.02f, 1.02f),
                46f,
                6.9f,
                0.15f,
                8.6f,
                10f,
                0.62f,
                11f,
                4.1f,
                true,
                23f,
                0.65f,
                0f,
                0.45f,
                new Color(0.88f, 0.87f, 0.75f),
                new Color(0.98f, 0.55f, 0.24f)));

            unitDefinitions.Add(CreateDefinition(
                UnitArchetype.Artillery,
                "Artillery",
                PrimitiveType.Cylinder,
                new Vector3(1.34f, 0.95f, 1.34f),
                56f,
                4.5f,
                0.18f,
                12.4f,
                22f,
                1.6f,
                15f,
                6.4f,
                true,
                14f,
                3.4f,
                2.8f,
                1.25f,
                new Color(0.74f, 0.72f, 0.8f),
                new Color(0.96f, 0.42f, 0.3f)));
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
