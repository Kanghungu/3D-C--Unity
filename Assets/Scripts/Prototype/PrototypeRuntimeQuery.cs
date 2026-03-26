using Game.Units;
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
            foreach (BaseStructure baseStructure in Object.FindObjectsByType<BaseStructure>())
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
            foreach (ProductionStructure structure in Object.FindObjectsByType<ProductionStructure>())
            {
                if (structure != null && structure.Team == team)
                {
                    return structure;
                }
            }

            return null;
        }

        public static ControlNode FindControlNode()
        {
            return Object.FindAnyObjectByType<ControlNode>();
        }

        public static PrototypeGameDatabase FindDatabase()
        {
            return Object.FindAnyObjectByType<PrototypeGameDatabase>();
        }

        public static int CountUnits(UnitTeam team)
        {
            int count = 0;

            foreach (SelectableUnit unit in Object.FindObjectsByType<SelectableUnit>())
            {
                if (unit != null && unit.Team == team)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
