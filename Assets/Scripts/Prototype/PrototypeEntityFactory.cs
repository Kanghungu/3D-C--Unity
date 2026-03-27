using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    public static class PrototypeEntityFactory
    {
        private static int serialNumber = 1;

        public static SelectableUnit CreateUnit(UnitTeam team, UnitDefinition definition, Vector3 position, Transform parent)
        {
            GameObject unit = GameObject.CreatePrimitive(definition.PrimitiveType);
            unit.name = $"{team} {definition.DisplayName} {serialNumber++}";
            unit.transform.position = position;
            unit.transform.localScale = GetAdjustedScale(team, definition);
            unit.transform.SetParent(parent);

            UnitAbilityState abilityState = unit.AddComponent<UnitAbilityState>();

            float moveSpeed = GetAdjustedMoveSpeed(team, definition);
            float maxHealth = GetAdjustedMaxHealth(team, definition);
            float attackDamage = GetAdjustedAttackDamage(team, definition);
            float attackRange = GetAdjustedAttackRange(team, definition);

            SimpleUnitMover mover = unit.AddComponent<SimpleUnitMover>();
            mover.Configure(moveSpeed, 540f, definition.StoppingDistance);

            UnitHealth health = unit.AddComponent<UnitHealth>();
            health.Configure(maxHealth, true, new Vector3(0f, 1.7f, 0f));

            CombatTarget combatTarget = unit.AddComponent<CombatTarget>();
            UnitCombat combat = unit.AddComponent<UnitCombat>();
            combat.Configure(
                attackRange,
                attackDamage,
                definition.AttackCooldown,
                definition.AggroRange,
                0.55f,
                definition.UsesProjectile,
                definition.ProjectileSpeed,
                definition.ProjectileArc,
                definition.SplashRadius,
                definition.ImpactEffectScale);

            SelectableUnit selectableUnit = unit.AddComponent<SelectableUnit>();

            combatTarget.Initialize(team, health);
            selectableUnit.Initialize(team, definition, mover, combat);
            combat.Initialize(combatTarget, health);

            return selectableUnit;
        }

        public static BaseStructure CreateBase(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObject.name = name;
            baseObject.transform.position = position;
            baseObject.transform.localScale = team == UnitTeam.Player ? new Vector3(4.6f, 3.4f, 4.6f) : new Vector3(4.2f, 3f, 4.2f);
            baseObject.transform.SetParent(parent);

            Renderer rendererComponent = baseObject.GetComponent<Renderer>();
            rendererComponent.material.color = team == UnitTeam.Player
                ? new Color(0.56f, 0.64f, 0.78f)
                : new Color(0.66f, 0.29f, 0.2f);

            UnitHealth health = baseObject.AddComponent<UnitHealth>();
            float baseHealth = team == UnitTeam.Player ? 320f : 230f;
            health.Configure(baseHealth, true, new Vector3(0f, 3.2f, 0f));

            CombatTarget combatTarget = baseObject.AddComponent<CombatTarget>();
            BaseStructure baseStructure = baseObject.AddComponent<BaseStructure>();

            combatTarget.Initialize(team, health);
            baseStructure.InitializeTarget(combatTarget, health);

            return baseStructure;
        }

        public static ProductionStructure CreateProductionStructure(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject structureObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structureObject.name = name;
            structureObject.transform.position = position;
            structureObject.transform.localScale = new Vector3(3.2f, 2.5f, 3.2f);
            structureObject.transform.SetParent(parent);

            Renderer rendererComponent = structureObject.GetComponent<Renderer>();
            rendererComponent.material.color = team == UnitTeam.Player
                ? new Color(0.78f, 0.72f, 0.58f)
                : new Color(0.8f, 0.38f, 0.22f);

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "Assembly Pad";
            pad.transform.SetParent(structureObject.transform);
            pad.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            pad.transform.localScale = new Vector3(1.15f, 0.08f, 1.15f);
            pad.GetComponent<Collider>().enabled = false;
            pad.GetComponent<Renderer>().material.color = new Color(0.22f, 0.85f, 0.95f);

            UnitHealth health = structureObject.AddComponent<UnitHealth>();
            health.Configure(180f, true, new Vector3(0f, 2.9f, 0f));

            CombatTarget combatTarget = structureObject.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);

            return structureObject.AddComponent<ProductionStructure>();
        }

        public static DefensiveTurret CreateTurret(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject turretObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretObject.name = name;
            turretObject.transform.position = position;
            turretObject.transform.localScale = team == UnitTeam.Player ? new Vector3(1.0f, 1.3f, 1.0f) : new Vector3(1.18f, 1.0f, 1.18f);
            turretObject.transform.SetParent(parent);

            Renderer rendererComponent = turretObject.GetComponent<Renderer>();
            rendererComponent.material.color = team == UnitTeam.Player
                ? new Color(0.74f, 0.79f, 0.88f)
                : new Color(0.92f, 0.42f, 0.22f);

            UnitHealth health = turretObject.AddComponent<UnitHealth>();
            health.Configure(team == UnitTeam.Player ? 110f : 95f, true, new Vector3(0f, 2.4f, 0f));

            CombatTarget combatTarget = turretObject.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);

            DefensiveTurret turret = turretObject.AddComponent<DefensiveTurret>();
            turret.Configure(team == UnitTeam.Player ? 10.5f : 11f, team == UnitTeam.Player ? 11f : 13f, 1.15f, 20f, 0.4f);
            return turret;
        }

        public static ControlNode CreateControlNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nodeObject.name = name;
            nodeObject.transform.position = position;
            nodeObject.transform.localScale = new Vector3(2.4f, 0.35f, 2.4f);
            nodeObject.transform.SetParent(parent);
            return nodeObject.AddComponent<ControlNode>();
        }

        private static Vector3 GetAdjustedScale(UnitTeam team, UnitDefinition definition)
        {
            Vector3 scale = definition.Scale;

            if (team == UnitTeam.Player)
            {
                scale *= definition.Archetype == UnitArchetype.Vanguard ? 1.14f : 1.06f;
            }
            else
            {
                scale *= definition.Archetype == UnitArchetype.Artillery ? 1.1f : 0.98f;
            }

            return scale;
        }

        private static float GetAdjustedMaxHealth(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Player)
            {
                return definition.MaxHealth * 1.22f;
            }

            return definition.MaxHealth * 0.92f;
        }

        private static float GetAdjustedMoveSpeed(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Player)
            {
                return definition.MoveSpeed * 0.92f;
            }

            return definition.MoveSpeed * 1.06f;
        }

        private static float GetAdjustedAttackDamage(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Player)
            {
                return definition.AttackDamage * 0.94f;
            }

            return definition.AttackDamage * 1.18f;
        }

        private static float GetAdjustedAttackRange(UnitTeam team, UnitDefinition definition)
        {
            if (team == UnitTeam.Enemy && definition.Archetype != UnitArchetype.Vanguard)
            {
                return definition.AttackRange + 0.4f;
            }

            return definition.AttackRange;
        }
    }
}
