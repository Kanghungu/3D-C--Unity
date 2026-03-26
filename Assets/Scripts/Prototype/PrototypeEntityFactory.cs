using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    public static class PrototypeEntityFactory
    {
        private static int serialNumber = 1;

        public static SelectableUnit CreateUnit(UnitTeam team, UnitArchetype archetype, Vector3 position, Transform parent)
        {
            UnitArchetypeStats stats = UnitArchetypeCatalog.Get(archetype);
            GameObject unit = GameObject.CreatePrimitive(stats.PrimitiveType);
            unit.name = $"{team} {stats.DisplayName} {serialNumber++}";
            unit.transform.position = position;
            unit.transform.localScale = stats.Scale;
            unit.transform.SetParent(parent);

            SimpleUnitMover mover = unit.AddComponent<SimpleUnitMover>();
            mover.Configure(stats.MoveSpeed, 540f, stats.StoppingDistance);

            UnitHealth health = unit.AddComponent<UnitHealth>();
            health.Configure(stats.MaxHealth, true, new Vector3(0f, 1.7f, 0f));

            CombatTarget combatTarget = unit.AddComponent<CombatTarget>();
            UnitCombat combat = unit.AddComponent<UnitCombat>();
            combat.Configure(stats.AttackRange, stats.AttackDamage, stats.AttackCooldown, stats.AggroRange, 0.45f);

            SelectableUnit selectableUnit = unit.AddComponent<SelectableUnit>();

            combatTarget.Initialize(team, health);
            selectableUnit.Initialize(team, archetype, stats.DisplayName, mover, combat);
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
            health.Configure(220f, true, new Vector3(0f, 3.2f, 0f));

            CombatTarget combatTarget = baseObject.AddComponent<CombatTarget>();
            BaseStructure baseStructure = baseObject.AddComponent<BaseStructure>();

            combatTarget.Initialize(team, health);
            baseStructure.InitializeTarget(combatTarget, health);

            return baseStructure;
        }
    }
}
