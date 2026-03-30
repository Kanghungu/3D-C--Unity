using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Small battlefield helper methods shared across prototype systems.
    /// Keeps common distance and anchor logic in one place without over-abstracting.
    /// </summary>
    public static class PrototypeBattlefieldUtility
    {
        public static List<SelectableUnit> GetUnits(UnitTeam team)
        {
            List<SelectableUnit> units = new();

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit != null && unit.Team == team)
                {
                    units.Add(unit);
                }
            }

            return units;
        }

        public static int CountUnitsNear(Vector3 position, UnitTeam team, float radius, IReadOnlyList<SelectableUnit> allUnits)
        {
            int count = 0;

            foreach (SelectableUnit unit in allUnits)
            {
                if (unit == null || unit.Team != team)
                {
                    continue;
                }

                if (Vector3.Distance(position, unit.transform.position) <= radius)
                {
                    count++;
                }
            }

            return count;
        }

        public static CombatTarget FindNearestEnemyNear(Vector3 position, UnitTeam targetTeam, float radius, IReadOnlyList<CombatTarget> allTargets)
        {
            CombatTarget bestTarget = null;
            float bestDistance = radius;

            foreach (CombatTarget target in allTargets)
            {
                if (target == null || !target.IsAlive || target.Team != targetTeam)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, target.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        public static int CompareNodesByReference(ControlNode left, ControlNode right, Vector3 referencePosition)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            float leftDistance = Vector3.Distance(left.transform.position, referencePosition);
            float rightDistance = Vector3.Distance(right.transform.position, referencePosition);
            return leftDistance.CompareTo(rightDistance);
        }

        public static bool TryFindClosestFriendlyAnchor(UnitTeam team, Vector3 referencePoint, out Vector3 anchor)
        {
            anchor = Vector3.zero;
            float bestDistance = float.MaxValue;

            BaseStructure baseStructure = PrototypeRuntimeQuery.FindBase(team);
            if (baseStructure != null && baseStructure.IsAlive)
            {
                bestDistance = Vector3.Distance(referencePoint, baseStructure.transform.position);
                anchor = baseStructure.transform.position;
            }

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null || node.OwnerTeam != team)
                {
                    continue;
                }

                float distance = Vector3.Distance(referencePoint, node.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    anchor = node.transform.position;
                }
            }

            foreach (ProductionStructure structure in PrototypeRuntimeRegistry.GetProductionStructures())
            {
                if (structure == null || !structure.IsAlive || structure.Team != team)
                {
                    continue;
                }

                float distance = Vector3.Distance(referencePoint, structure.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    anchor = structure.transform.position;
                }
            }

            return bestDistance < float.MaxValue;
        }
    }
}
