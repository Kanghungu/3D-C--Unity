using Game.Units;

namespace Game.Prototype
{
    /// <summary>
    /// Shared production reservation rules for bases and production structures.
    /// Keeps unit-cap and per-archetype checks consistent across prototype systems.
    /// </summary>
    public static class PrototypeProductionRules
    {
        public static bool CanReserveUnit(UnitTeam team, UnitDefinition definition, int extraReservedTotal, int extraReservedForArchetype, int teamUnitCap)
        {
            if (definition == null)
            {
                return false;
            }

            int reservedTotal = PrototypeRuntimeQuery.CountUnits(team) + extraReservedTotal;
            if (reservedTotal >= teamUnitCap)
            {
                return false;
            }

            if (definition.MaxPerTeam > 0)
            {
                int reservedForArchetype = PrototypeRuntimeQuery.CountUnits(team, definition.Archetype) + extraReservedForArchetype;
                if (reservedForArchetype >= definition.MaxPerTeam)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
