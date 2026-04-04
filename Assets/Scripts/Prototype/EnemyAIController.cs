using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Sends enemy forces toward player targets and strategically important shrines at intervals.
    /// Preserves rear garrisons while concentrating pressure on high-value nodes.
    /// </summary>
    public class EnemyAIController : MonoBehaviour
    {
        [SerializeField] private float thinkInterval = 3f;
        [SerializeField] private float garrisonRadius = 22f;
        [SerializeField] private int rearGarrisonCount = 3;
        [SerializeField] private float localThreatRange = 150f;

        private float thinkTimer;

        private void Update()
        {
            thinkTimer -= Time.deltaTime;

            if (thinkTimer > 0f)
            {
                return;
            }

            thinkTimer = thinkInterval;
            IssueEnemyOrders();
        }

        private void IssueEnemyOrders()
        {
            IReadOnlyList<CombatTarget> allTargets = PrototypeRuntimeRegistry.GetCombatTargets();
            IReadOnlyList<SelectableUnit> allUnits = PrototypeRuntimeRegistry.GetSelectableUnits();
            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            List<ControlNode> enemyNodes = PrototypeRuntimeQuery.FindControlNodes();
            enemyNodes.RemoveAll(node => node == null || node.OwnerTeam != UnitTeam.Enemy);

            Vector3 priorityReference = playerBase != null ? playerBase.transform.position : Vector3.zero;
            enemyNodes.Sort((left, right) => CompareEnemyDefensePriority(left, right, priorityReference));

            BattleDirectiveController directiveController = BattleDirectiveController.Instance;
            ControlNode targetNode = directiveController != null ? directiveController.FindPriorityNodeFor(UnitTeam.Enemy) : null;
            bool baseUnlocked = BattleDirectiveController.CanTargetEnemyBaseStatic(UnitTeam.Enemy);

            foreach (SelectableUnit enemyUnit in allUnits)
            {
                if (enemyUnit == null || enemyUnit.Team != UnitTeam.Enemy || enemyUnit.IsSelected)
                {
                    continue;
                }

                if (ShouldHoldGarrison(enemyUnit, enemyNodes, allUnits))
                {
                    continue;
                }

                CombatTarget nearestTarget = FindNearestTarget(enemyUnit.transform.position, UnitTeam.Player, allTargets, baseUnlocked);
                if (nearestTarget != null && Vector3.Distance(enemyUnit.transform.position, nearestTarget.transform.position) <= localThreatRange)
                {
                    enemyUnit.Attack(nearestTarget);
                    continue;
                }

                if (!baseUnlocked && targetNode != null)
                {
                    float offsetRadius = targetNode.IsGrand ? 18f : (targetNode.IsMajor ? 13f : 10f);
                    Vector3 offset = new Vector3(Random.Range(-offsetRadius, offsetRadius), 0f, Random.Range(-offsetRadius, offsetRadius));
                    enemyUnit.AttackMoveTo(targetNode.transform.position + offset);
                    continue;
                }

                if (nearestTarget != null)
                {
                    enemyUnit.Attack(nearestTarget);
                    continue;
                }

                if (playerBase != null && baseUnlocked)
                {
                    enemyUnit.AttackMoveTo(playerBase.transform.position + new Vector3(Random.Range(-22f, 22f), 0f, Random.Range(-22f, 22f)));
                    continue;
                }

                // 폴백: 가장 가까운 플레이어 점령 노드로 전진
                ControlNode fallbackNode = FindNearestPlayerNode(enemyUnit.transform.position);
                if (fallbackNode != null)
                {
                    Vector3 offset = new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-12f, 12f));
                    enemyUnit.AttackMoveTo(fallbackNode.transform.position + offset);
                }
                else if (playerBase != null)
                {
                    enemyUnit.AttackMoveTo(playerBase.transform.position + new Vector3(Random.Range(-22f, 22f), 0f, Random.Range(-22f, 22f)));
                }
            }
        }

        private static ControlNode FindNearestPlayerNode(Vector3 fromPosition)
        {
            ControlNode nearest = null;
            float bestDist = float.MaxValue;
            foreach (ControlNode node in PrototypeRuntimeQuery.FindControlNodes())
            {
                if (node == null || node.OwnerTeam != UnitTeam.Player)
                {
                    continue;
                }

                float d = Vector3.Distance(fromPosition, node.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = node;
                }
            }

            return nearest;
        }

        private bool ShouldHoldGarrison(SelectableUnit unit, List<ControlNode> enemyNodes, IReadOnlyList<SelectableUnit> allUnits)
        {
            for (int index = 0; index < enemyNodes.Count; index++)
            {
                ControlNode node = enemyNodes[index];
                if (node == null)
                {
                    continue;
                }

                if (Vector3.Distance(unit.transform.position, node.transform.position) > garrisonRadius + node.StrategicWeight * 2f)
                {
                    continue;
                }

                int stationed = PrototypeBattlefieldUtility.CountUnitsNear(node.transform.position, UnitTeam.Enemy, garrisonRadius + node.StrategicWeight * 2f, allUnits);
                int desired = Mathf.Max(rearGarrisonCount, node.DesiredGarrison - (index == 0 ? 2 : 0));
                if (stationed <= desired)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CompareEnemyDefensePriority(ControlNode left, ControlNode right, Vector3 referencePosition)
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

            int frontlineCompare = PrototypeBattlefieldUtility.CompareNodesByReference(left, right, referencePosition);
            if (frontlineCompare != 0)
            {
                return frontlineCompare;
            }

            int weightCompare = right.StrategicWeight.CompareTo(left.StrategicWeight);
            if (weightCompare != 0)
            {
                return weightCompare;
            }

            return 0;
        }

        private static CombatTarget FindNearestTarget(Vector3 fromPosition, UnitTeam desiredTeam, IReadOnlyList<CombatTarget> targets, bool canAttackBase)
        {
            CombatTarget bestTarget = null;
            float bestDistance = float.MaxValue;

            foreach (CombatTarget target in targets)
            {
                if (target == null || target.Team != desiredTeam || !target.IsAlive)
                {
                    continue;
                }

                if (!canAttackBase && target.GetComponent<BaseStructure>() != null)
                {
                    continue;
                }

                float distance = Vector3.Distance(fromPosition, target.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }
    }
}
