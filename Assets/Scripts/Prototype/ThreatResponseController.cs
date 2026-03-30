using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Makes nearby idle units defend important structures, maintains shrine garrisons,
    /// and pushes surplus soldiers toward strategic control nodes.
    /// </summary>
    public class ThreatResponseController : MonoBehaviour
    {
        [SerializeField] private float thinkInterval = 1.1f;
        [SerializeField] private float structureThreatRadius = 28f;
        [SerializeField] private float localDefenseRadius = 52f;
        [SerializeField] private float baseThreatRadius = 76f;
        [SerializeField] private float baseDefenseResponseRadius = 120f;
        [SerializeField] private float garrisonRadius = 22f;
        [SerializeField] private int rearGarrisonCount = 10;
        [SerializeField] private int baseGuardCount = 18;
        [SerializeField] private float controlNodeSupportRadius = 34f;
        [SerializeField] private int neutralNodeStrikeForce = 22;
        [SerializeField] private int hostileNodeStrikeForce = 16;

        private float thinkTimer;

        private void Update()
        {
            thinkTimer -= Time.deltaTime;

            if (thinkTimer > 0f)
            {
                return;
            }

            thinkTimer = thinkInterval;
            EvaluateTeam(UnitTeam.Player);
            EvaluateTeam(UnitTeam.Enemy);
        }

        private void EvaluateTeam(UnitTeam defendingTeam)
        {
            UnitTeam attackingTeam = defendingTeam == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player;
            BaseStructure mainBase = PrototypeRuntimeQuery.FindBase(defendingTeam);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(attackingTeam);
            List<SelectableUnit> defendingUnits = PrototypeBattlefieldUtility.GetUnits(defendingTeam);
            IReadOnlyList<CombatTarget> allTargets = PrototypeRuntimeRegistry.GetCombatTargets();
            IReadOnlyList<SelectableUnit> allUnits = PrototypeRuntimeRegistry.GetSelectableUnits();

            if (defendingUnits.Count == 0)
            {
                return;
            }

            if (mainBase != null && mainBase.IsAlive)
            {
                CombatTarget baseThreat = PrototypeBattlefieldUtility.FindNearestEnemyNear(mainBase.transform.position, attackingTeam, baseThreatRadius, allTargets);
                if (baseThreat != null)
                {
                    CallBaseDefense(defendingUnits, mainBase.transform.position, baseThreat);
                    return;
                }
            }

            foreach (ProductionStructure structure in PrototypeRuntimeQuery.FindProductionStructures(defendingTeam))
            {
                if (structure == null || !structure.IsAlive)
                {
                    continue;
                }

                CombatTarget threat = PrototypeBattlefieldUtility.FindNearestEnemyNear(structure.transform.position, attackingTeam, structureThreatRadius, allTargets);
                if (threat != null)
                {
                    CallLocalDefense(defendingUnits, structure.transform.position, threat);
                }
            }

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null)
                {
                    continue;
                }

                if (node.OwnerTeam.HasValue && node.OwnerTeam.Value != defendingTeam)
                {
                    continue;
                }

                CombatTarget threat = PrototypeBattlefieldUtility.FindNearestEnemyNear(node.transform.position, attackingTeam, structureThreatRadius + node.StrategicWeight * 5f, allTargets);
                if (threat != null)
                {
                    CallLocalDefense(defendingUnits, node.transform.position, threat);
                }
            }

            List<SelectableUnit> idleUnits = MaintainGarrisons(defendingUnits, allUnits, defendingTeam, mainBase, enemyBase);
            PushIdleUnitsTowardControlNodes(idleUnits, allUnits, defendingTeam, enemyBase);
        }

        private List<SelectableUnit> MaintainGarrisons(List<SelectableUnit> defenders, IReadOnlyList<SelectableUnit> allUnits, UnitTeam team, BaseStructure mainBase, BaseStructure enemyBase)
        {
            List<SelectableUnit> idleUnits = new();

            foreach (SelectableUnit unit in defenders)
            {
                if (CanBeRedirected(unit))
                {
                    idleUnits.Add(unit);
                }
            }

            if (idleUnits.Count == 0)
            {
                return idleUnits;
            }

            if (mainBase != null && mainBase.IsAlive)
            {
                int stationedAtBase = PrototypeBattlefieldUtility.CountUnitsNear(mainBase.transform.position, team, garrisonRadius + 12f, allUnits);
                int neededAtBase = Mathf.Max(0, baseGuardCount - stationedAtBase);
                AssignUnitsToAnchor(idleUnits, mainBase.transform.position, neededAtBase, 8f);
            }

            List<ControlNode> nodes = PrototypeRuntimeQuery.FindControlNodes();
            if (nodes.Count == 0)
            {
                return idleUnits;
            }

            Vector3 frontlineReference = enemyBase != null ? enemyBase.transform.position : Vector3.zero;
            nodes.Sort((left, right) => CompareFriendlyNodePriority(left, right, frontlineReference, team));

            foreach (ControlNode node in nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (node.OwnerTeam != team)
                {
                    continue;
                }

                int stationed = PrototypeBattlefieldUtility.CountUnitsNear(node.transform.position, team, garrisonRadius + node.StrategicWeight * 2f, allUnits);
                int desired = Mathf.Max(rearGarrisonCount, node.DesiredGarrison);
                int needed = Mathf.Max(0, desired - stationed);
                AssignUnitsToAnchor(idleUnits, node.transform.position, needed, 10f + node.StrategicWeight * 2f);

                if (idleUnits.Count == 0)
                {
                    break;
                }
            }

            return idleUnits;
        }

        private void PushIdleUnitsTowardControlNodes(List<SelectableUnit> idleUnits, IReadOnlyList<SelectableUnit> allUnits, UnitTeam team, BaseStructure enemyBase)
        {
            if (idleUnits == null || idleUnits.Count == 0)
            {
                return;
            }

            List<ControlNode> candidateNodes = PrototypeRuntimeQuery.FindControlNodes();
            candidateNodes.RemoveAll(node => node == null || (node.OwnerTeam.HasValue && node.OwnerTeam.Value == team));

            if (candidateNodes.Count == 0)
            {
                if (!BattleDirectiveController.CanTargetEnemyBaseStatic(team) || enemyBase == null)
                {
                    return;
                }

                AssignUnitsToAnchor(idleUnits, enemyBase.transform.position, idleUnits.Count, 24f);
                return;
            }

            candidateNodes.Sort((left, right) => CompareAssaultPriority(left, right, team, enemyBase));

            foreach (ControlNode node in candidateNodes)
            {
                int stationed = PrototypeBattlefieldUtility.CountUnitsNear(node.transform.position, team, controlNodeSupportRadius + node.StrategicWeight * 3f, allUnits);
                int desiredBase = node.OwnerTeam.HasValue ? hostileNodeStrikeForce : neutralNodeStrikeForce;
                int desired = Mathf.Max(desiredBase, node.DesiredStrikeForce);
                int needed = Mathf.Max(0, desired - stationed);
                AssignUnitsToAnchor(idleUnits, node.transform.position, needed, 14f + node.StrategicWeight * 3f);

                if (idleUnits.Count == 0)
                {
                    return;
                }
            }

            int rolloverIndex = 0;
            while (idleUnits.Count > 0 && candidateNodes.Count > 0)
            {
                ControlNode node = candidateNodes[rolloverIndex % candidateNodes.Count];
                AssignUnitsToAnchor(idleUnits, node.transform.position, 1, 18f + node.StrategicWeight * 2f);
                rolloverIndex++;
            }
        }

        private static int CompareFriendlyNodePriority(ControlNode left, ControlNode right, Vector3 frontlineReference, UnitTeam team)
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

            int weightCompare = right.StrategicWeight.CompareTo(left.StrategicWeight);
            if (weightCompare != 0)
            {
                return weightCompare;
            }

            return PrototypeBattlefieldUtility.CompareNodesByReference(left, right, frontlineReference);
        }

        private static int CompareAssaultPriority(ControlNode left, ControlNode right, UnitTeam team, BaseStructure enemyBase)
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

            if (left.OwnerTeam != right.OwnerTeam)
            {
                bool leftNeutral = !left.OwnerTeam.HasValue;
                bool rightNeutral = !right.OwnerTeam.HasValue;
                if (leftNeutral != rightNeutral)
                {
                    return leftNeutral ? -1 : 1;
                }
            }

            int weightCompare = right.StrategicWeight.CompareTo(left.StrategicWeight);
            if (weightCompare != 0)
            {
                return weightCompare;
            }

            if (enemyBase != null)
            {
                int frontlineCompare = PrototypeBattlefieldUtility.CompareNodesByReference(left, right, enemyBase.transform.position);
                if (frontlineCompare != 0)
                {
                    return frontlineCompare;
                }
            }

            float anchorBias = team == UnitTeam.Player
                ? left.transform.position.x.CompareTo(right.transform.position.x)
                : right.transform.position.x.CompareTo(left.transform.position.x);
            return anchorBias < 0f ? -1 : (anchorBias > 0f ? 1 : 0);
        }

        private static void AssignUnitsToAnchor(List<SelectableUnit> idleUnits, Vector3 anchor, int needed, float scatterRadius)
        {
            while (needed > 0 && idleUnits.Count > 0)
            {
                int bestIndex = -1;
                float bestDistance = float.MaxValue;

                for (int index = 0; index < idleUnits.Count; index++)
                {
                    SelectableUnit unit = idleUnits[index];
                    if (unit == null)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(unit.transform.position, anchor);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestIndex = index;
                    }
                }

                if (bestIndex < 0)
                {
                    break;
                }

                SelectableUnit selectedUnit = idleUnits[bestIndex];
                idleUnits.RemoveAt(bestIndex);
                Vector3 offset = new Vector3(Random.Range(-scatterRadius, scatterRadius), 0f, Random.Range(-scatterRadius, scatterRadius));
                selectedUnit.AttackMoveTo(anchor + offset);
                needed--;
            }
        }

        private void CallBaseDefense(List<SelectableUnit> defenders, Vector3 basePosition, CombatTarget threat)
        {
            foreach (SelectableUnit unit in defenders)
            {
                if (!CanBeRedirected(unit))
                {
                    continue;
                }

                if (Vector3.Distance(unit.transform.position, basePosition) > baseDefenseResponseRadius)
                {
                    continue;
                }

                unit.Attack(threat);
            }
        }

        private void CallLocalDefense(List<SelectableUnit> defenders, Vector3 anchor, CombatTarget threat)
        {
            foreach (SelectableUnit unit in defenders)
            {
                if (!CanBeRedirected(unit))
                {
                    continue;
                }

                if (Vector3.Distance(unit.transform.position, anchor) > localDefenseRadius)
                {
                    continue;
                }

                unit.Attack(threat);
            }
        }

        private static bool CanBeRedirected(SelectableUnit unit)
        {
            if (unit == null)
            {
                return false;
            }

            UnitCombat combat = unit.GetComponent<UnitCombat>();
            if (combat == null)
            {
                return false;
            }

            return combat.CurrentTarget == null && !unit.IsSelected;
        }
    }
}
