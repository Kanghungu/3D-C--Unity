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
            unit.transform.localScale = definition.Scale;
            unit.transform.SetParent(parent);

            SimpleUnitMover mover = unit.AddComponent<SimpleUnitMover>();
            mover.Configure(definition.MoveSpeed, 540f, definition.StoppingDistance);

            UnitHealth health = unit.AddComponent<UnitHealth>();
            health.Configure(definition.MaxHealth, true, new Vector3(0f, 1.7f, 0f));

            CombatTarget combatTarget = unit.AddComponent<CombatTarget>();
            UnitCombat combat = unit.AddComponent<UnitCombat>();
            combat.Configure(
                definition.AttackRange,
                definition.AttackDamage,
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
            baseObject.transform.localScale = new Vector3(4f, 3f, 4f);
            baseObject.transform.SetParent(parent);

            UnitHealth health = baseObject.AddComponent<UnitHealth>();
            float baseHealth = team == UnitTeam.Player ? 300f : 220f;
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
            structureObject.transform.localScale = new Vector3(3f, 2.2f, 3f);
            structureObject.transform.SetParent(parent);

            Renderer rendererComponent = structureObject.GetComponent<Renderer>();
            rendererComponent.material.color = team == UnitTeam.Player
                ? new Color(0.3f, 0.8f, 1f)
                : new Color(1f, 0.45f, 0.28f);

            UnitHealth health = structureObject.AddComponent<UnitHealth>();
            health.Configure(180f, true, new Vector3(0f, 2.7f, 0f));

            CombatTarget combatTarget = structureObject.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);

            return structureObject.AddComponent<ProductionStructure>();
        }

        public static DefensiveTurret CreateTurret(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject turretObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretObject.name = name;
            turretObject.transform.position = position;
            turretObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            turretObject.transform.SetParent(parent);

            Renderer rendererComponent = turretObject.GetComponent<Renderer>();
            rendererComponent.material.color = team == UnitTeam.Player
                ? new Color(0.25f, 0.85f, 1f)
                : new Color(1f, 0.4f, 0.25f);

            UnitHealth health = turretObject.AddComponent<UnitHealth>();
            health.Configure(90f, true, new Vector3(0f, 2.4f, 0f));

            CombatTarget combatTarget = turretObject.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);

            DefensiveTurret turret = turretObject.AddComponent<DefensiveTurret>();
            turret.Configure(10f, 12f, 1.15f, 20f, 0.4f);
            return turret;
        }

        public static ControlNode CreateControlNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nodeObject.name = name;
            nodeObject.transform.position = position;
            nodeObject.transform.localScale = new Vector3(2.2f, 0.35f, 2.2f);
            nodeObject.transform.SetParent(parent);
            return nodeObject.AddComponent<ControlNode>();
        }
    }
}
