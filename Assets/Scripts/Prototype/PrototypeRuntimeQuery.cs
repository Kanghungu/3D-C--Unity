using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    public static class PrototypeRuntimeQuery
    {
        public static BaseStructure FindPlayerBase()
        {
            return FindBase(UnitTeam.Player);
        }

        public static BaseStructure FindBase(UnitTeam team)
        {
            foreach (BaseStructure baseStructure in PrototypeRuntimeRegistry.GetBaseStructures())
            {
                if (baseStructure != null && baseStructure.Team == team)
                {
                    return baseStructure;
                }
            }

            return null;
        }

        public static ProductionStructure FindPlayerProductionStructure()
        {
            return FindProductionStructure(UnitTeam.Player);
        }

        public static ProductionStructure FindProductionStructure(UnitTeam team)
        {
            foreach (ProductionStructure structure in PrototypeRuntimeRegistry.GetProductionStructures())
            {
                if (structure != null && structure.Team == team)
                {
                    return structure;
                }
            }

            return null;
        }

        public static ProductionStructure FindProductionStructure(UnitTeam team, UnitArchetype archetype)
        {
            foreach (ProductionStructure structure in FindProductionStructures(team))
            {
                if (structure != null && structure.CanProduce(archetype))
                {
                    return structure;
                }
            }

            return null;
        }

        public static List<ProductionStructure> FindPlayerProductionStructures()
        {
            return FindProductionStructures(UnitTeam.Player);
        }

        public static List<ProductionStructure> FindProductionStructures(UnitTeam team)
        {
            List<ProductionStructure> structures = new();

            foreach (ProductionStructure structure in PrototypeRuntimeRegistry.GetProductionStructures())
            {
                if (structure != null && structure.Team == team)
                {
                    structures.Add(structure);
                }
            }

            return structures;
        }

        public static ControlNode FindControlNode()
        {
            IReadOnlyList<ControlNode> nodes = PrototypeRuntimeRegistry.GetControlNodes();
            return nodes.Count > 0 ? nodes[0] : null;
        }

        public static List<ControlNode> FindControlNodes()
        {
            return new List<ControlNode>(PrototypeRuntimeRegistry.GetControlNodes());
        }

        public static PrototypeGameDatabase FindDatabase()
        {
            return Object.FindAnyObjectByType<PrototypeGameDatabase>();
        }

        public static int CountUnits(UnitTeam team)
        {
            int count = 0;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit != null && unit.Team == team)
                {
                    count++;
                }
            }

            return count;
        }

        public static int CountUnits(UnitTeam team, UnitArchetype archetype)
        {
            int count = 0;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit != null && unit.Team == team && unit.Archetype == archetype)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
