using Game.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Lightweight runtime registry used to avoid repeated scene-wide searches.
    /// Keeps prototype code simple while reducing per-frame allocations.
    /// </summary>
    public static class PrototypeRuntimeRegistry
    {
        private static readonly HashSet<SelectableUnit> selectableUnits = new();
        private static readonly HashSet<UnitCombat> unitCombats = new();
        private static readonly HashSet<CombatTarget> combatTargets = new();
        private static readonly HashSet<DefensiveTurret> defensiveTurrets = new();
        private static readonly HashSet<BaseStructure> baseStructures = new();
        private static readonly HashSet<ProductionStructure> productionStructures = new();
        private static readonly HashSet<ControlNode> controlNodes = new();

        private static readonly List<SelectableUnit> selectableUnitsCache = new();
        private static readonly List<UnitCombat> unitCombatsCache = new();
        private static readonly List<CombatTarget> combatTargetsCache = new();
        private static readonly List<DefensiveTurret> defensiveTurretsCache = new();
        private static readonly List<BaseStructure> baseStructuresCache = new();
        private static readonly List<ProductionStructure> productionStructuresCache = new();
        private static readonly List<ControlNode> controlNodesCache = new();

        private static bool selectableUnitsDirty = true;
        private static bool unitCombatsDirty = true;
        private static bool combatTargetsDirty = true;
        private static bool defensiveTurretsDirty = true;
        private static bool baseStructuresDirty = true;
        private static bool productionStructuresDirty = true;
        private static bool controlNodesDirty = true;

        public static void Register(SelectableUnit unit)
        {
            if (unit != null && selectableUnits.Add(unit))
            {
                selectableUnitsDirty = true;
            }
        }

        public static void Unregister(SelectableUnit unit)
        {
            if (unit != null && selectableUnits.Remove(unit))
            {
                selectableUnitsDirty = true;
            }
        }

        public static void Register(UnitCombat combat)
        {
            if (combat != null && unitCombats.Add(combat))
            {
                unitCombatsDirty = true;
            }
        }

        public static void Unregister(UnitCombat combat)
        {
            if (combat != null && unitCombats.Remove(combat))
            {
                unitCombatsDirty = true;
            }
        }

        public static void Register(CombatTarget target)
        {
            if (target != null && combatTargets.Add(target))
            {
                combatTargetsDirty = true;
            }
        }

        public static void Unregister(CombatTarget target)
        {
            if (target != null && combatTargets.Remove(target))
            {
                combatTargetsDirty = true;
            }
        }

        public static void Register(DefensiveTurret turret)
        {
            if (turret != null && defensiveTurrets.Add(turret))
            {
                defensiveTurretsDirty = true;
            }
        }

        public static void Unregister(DefensiveTurret turret)
        {
            if (turret != null && defensiveTurrets.Remove(turret))
            {
                defensiveTurretsDirty = true;
            }
        }

        public static void Register(BaseStructure structure)
        {
            if (structure != null && baseStructures.Add(structure))
            {
                baseStructuresDirty = true;
            }
        }

        public static void Unregister(BaseStructure structure)
        {
            if (structure != null && baseStructures.Remove(structure))
            {
                baseStructuresDirty = true;
            }
        }

        public static void Register(ProductionStructure structure)
        {
            if (structure != null && productionStructures.Add(structure))
            {
                productionStructuresDirty = true;
            }
        }

        public static void Unregister(ProductionStructure structure)
        {
            if (structure != null && productionStructures.Remove(structure))
            {
                productionStructuresDirty = true;
            }
        }

        public static void Register(ControlNode node)
        {
            if (node != null && controlNodes.Add(node))
            {
                controlNodesDirty = true;
            }
        }

        public static void Unregister(ControlNode node)
        {
            if (node != null && controlNodes.Remove(node))
            {
                controlNodesDirty = true;
            }
        }

        public static IReadOnlyList<SelectableUnit> GetSelectableUnits()
        {
            if (selectableUnitsDirty)
            {
                RebuildCache(selectableUnits, selectableUnitsCache);
                selectableUnitsDirty = false;
            }

            return selectableUnitsCache;
        }

        public static IReadOnlyList<UnitCombat> GetUnitCombats()
        {
            if (unitCombatsDirty)
            {
                RebuildCache(unitCombats, unitCombatsCache);
                unitCombatsDirty = false;
            }

            return unitCombatsCache;
        }

        public static IReadOnlyList<CombatTarget> GetCombatTargets()
        {
            if (combatTargetsDirty)
            {
                RebuildCache(combatTargets, combatTargetsCache);
                combatTargetsDirty = false;
            }

            return combatTargetsCache;
        }

        public static IReadOnlyList<DefensiveTurret> GetDefensiveTurrets()
        {
            if (defensiveTurretsDirty)
            {
                RebuildCache(defensiveTurrets, defensiveTurretsCache);
                defensiveTurretsDirty = false;
            }

            return defensiveTurretsCache;
        }

        public static IReadOnlyList<BaseStructure> GetBaseStructures()
        {
            if (baseStructuresDirty)
            {
                RebuildCache(baseStructures, baseStructuresCache);
                baseStructuresDirty = false;
            }

            return baseStructuresCache;
        }

        public static IReadOnlyList<ProductionStructure> GetProductionStructures()
        {
            if (productionStructuresDirty)
            {
                RebuildCache(productionStructures, productionStructuresCache, static (left, right) =>
                    string.Compare(left.StructureLabel, right.StructureLabel, StringComparison.Ordinal));
                productionStructuresDirty = false;
            }

            return productionStructuresCache;
        }

        public static IReadOnlyList<ControlNode> GetControlNodes()
        {
            if (controlNodesDirty)
            {
                RebuildCache(controlNodes, controlNodesCache, static (left, right) =>
                    string.Compare(left.NodeLabel, right.NodeLabel, StringComparison.Ordinal));
                controlNodesDirty = false;
            }

            return controlNodesCache;
        }

        private static void RebuildCache<T>(HashSet<T> source, List<T> cache, Comparison<T> comparison = null) where T : UnityEngine.Object
        {
            cache.Clear();

            foreach (T item in source)
            {
                if (item != null)
                {
                    cache.Add(item);
                }
            }

            if (comparison != null)
            {
                cache.Sort(comparison);
            }
        }
    }
}
