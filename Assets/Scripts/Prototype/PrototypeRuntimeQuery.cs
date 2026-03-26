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
            foreach (BaseStructure baseStructure in Object.FindObjectsByType<BaseStructure>(FindObjectsSortMode.None))
            {
                if (baseStructure != null && baseStructure.Team == team)
                {
                    return baseStructure;
                }
            }

            return null;
        }

        public static int CountUnits(UnitTeam team)
        {
            int count = 0;

            foreach (SelectableUnit unit in Object.FindObjectsByType<SelectableUnit>(FindObjectsSortMode.None))
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
