using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Sends enemy forces toward nearby player targets at intervals.
    /// </summary>
    public class EnemyAIController : MonoBehaviour
    {
        [SerializeField] private float thinkInterval = 1.5f;

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
            CombatTarget[] allTargets = FindObjectsByType<CombatTarget>(FindObjectsSortMode.None);
            SelectableUnit[] enemyUnits = FindObjectsByType<SelectableUnit>(FindObjectsSortMode.None);

            foreach (SelectableUnit enemyUnit in enemyUnits)
            {
                if (enemyUnit == null || enemyUnit.Team != UnitTeam.Enemy)
                {
                    continue;
                }

                CombatTarget nearestTarget = FindNearestTarget(enemyUnit.transform.position, UnitTeam.Player, allTargets);

                if (nearestTarget != null)
                {
                    enemyUnit.Attack(nearestTarget);
                }
            }
        }

        private static CombatTarget FindNearestTarget(Vector3 fromPosition, UnitTeam desiredTeam, CombatTarget[] targets)
        {
            CombatTarget bestTarget = null;
            float bestDistance = float.MaxValue;

            foreach (CombatTarget target in targets)
            {
                if (target == null || target.Team != desiredTeam || !target.IsAlive)
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
