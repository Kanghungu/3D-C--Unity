using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 교전 대상 탐색, 추격, 공격 이동·홀드·가드 등 명령 상태 처리.
    /// </summary>
    public partial class UnitCombat
    {
        private Vector3 GetEngagementPosition(CombatTarget target)
        {
            // 같은 팀에서 같은 타겟을 공격하는 유닛들 수집
            int mySlot = 0;
            int totalAttackers = 0;

            foreach (UnitCombat other in PrototypeRuntimeRegistry.GetUnitCombats())
            {
                if (other == null || other.owner == null || other.owner.Team != owner.Team)
                {
                    continue;
                }

                if (other.currentTarget != target)
                {
                    continue;
                }

                if (other == this)
                {
                    mySlot = totalAttackers;
                }

                totalAttackers++;
            }

            if (totalAttackers <= 1)
            {
                return target.transform.position;
            }

            // 공격 반지름: attackRange의 75% 지점에 원형 배치
            float radius = attackRange * 0.75f;
            float angle = mySlot * (Mathf.PI * 2f / totalAttackers);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 pos = target.transform.position + offset;
            pos.y = transform.position.y;
            return pos;
        }

        private CombatTarget FindClosestEnemyTarget()
        {
            CombatTarget bestTarget = null;
            float bestDistance = aggroRange;
            Vector3 searchOrigin = GetSearchOrigin();

            foreach (CombatTarget target in PrototypeRuntimeRegistry.GetCombatTargets())
            {
                if (target == null || target == owner || !target.IsAlive || target.Team == owner.Team)
                {
                    continue;
                }

                if (!CanAcceptCombatTarget(target))
                {
                    continue;
                }

                float distance = Vector3.Distance(searchOrigin, target.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            CombatTarget supportTarget = FindNearbySupportTarget();
            if (supportTarget != null)
            {
                float supportDistance = Vector3.Distance(transform.position, supportTarget.transform.position);
                if (bestTarget == null || supportDistance < bestDistance)
                {
                    bestTarget = supportTarget;
                }
            }

            return bestTarget;
        }

        private CombatTarget FindNearbySupportTarget()
        {
            if (selectableUnit != null && selectableUnit.IsSelected)
            {
                return null;
            }

            CombatTarget bestTarget = null;
            float bestDistance = supportTargetRadius;

            foreach (UnitCombat allyCombat in PrototypeRuntimeRegistry.GetUnitCombats())
            {
                if (allyCombat == null || allyCombat == this)
                {
                    continue;
                }

                CombatTarget allyOwner = allyCombat.owner;
                if (allyOwner == null || allyOwner.Team != owner.Team || allyCombat.CurrentTarget == null || !allyCombat.CurrentTarget.IsAlive)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, allyCombat.transform.position) > supportAssistRadius)
                {
                    continue;
                }

                if (!CanAcceptCombatTarget(allyCombat.CurrentTarget))
                {
                    continue;
                }

                float targetDistance = Vector3.Distance(transform.position, allyCombat.CurrentTarget.transform.position);
                if (targetDistance > supportAssistRadius)
                {
                    continue;
                }

                if (targetDistance < bestDistance)
                {
                    bestDistance = targetDistance;
                    bestTarget = allyCombat.CurrentTarget;
                }
            }

            return bestTarget;
        }

        private bool CanAcceptCombatTarget(CombatTarget target)
        {
            if (target == null)
            {
                return false;
            }

            if (roleController != null && !roleController.CanAcceptTarget(target))
            {
                return false;
            }

            BaseStructure baseStructure = target.GetComponent<BaseStructure>();
            if (baseStructure != null && !BattleDirectiveController.CanTargetEnemyBaseStatic(owner.Team))
            {
                return false;
            }

            if (hasHoldPosition || hasGuardPoint)
            {
                Vector3 anchor = hasGuardPoint ? guardPoint : holdPosition;
                float leash = hasGuardPoint ? guardRadius + aggroRange + 6f : Mathf.Max(aggroRange + 2f, 10f);
                if (Vector3.Distance(anchor, target.transform.position) > leash)
                {
                    return false;
                }
            }

            return true;
        }

        private bool RunGuardOrderIfNeeded()
        {
            if (hasGuardPoint)
            {
                Vector3 anchor = new Vector3(guardPoint.x, transform.position.y, guardPoint.z);
                float distance = Vector3.Distance(transform.position, anchor);

                if (distance > guardRadius * 0.6f)
                {
                    mover.SetDestination(anchor);
                    return true;
                }

                mover.Stop();
                return true;
            }

            if (hasHoldPosition)
            {
                Vector3 anchor = new Vector3(holdPosition.x, transform.position.y, holdPosition.z);
                float distance = Vector3.Distance(transform.position, anchor);

                if (distance > Mathf.Max(0.8f, attackRange * 0.28f))
                {
                    mover.SetDestination(anchor);
                    return true;
                }

                mover.Stop();
                return true;
            }

            return false;
        }

        private bool RunPursuitAdvance()
        {
            if (!hasPursuitDestination)
            {
                return false;
            }

            Vector3 destination = new Vector3(pursuitDestination.x, transform.position.y, pursuitDestination.z);
            float remainingDistance = Vector3.Distance(transform.position, destination);

            if (remainingDistance <= Mathf.Max(0.45f, attackRange * 0.3f))
            {
                hasPursuitDestination = false;
                mover.Stop();
                return false;
            }

            mover.SetDestination(destination);
            return true;
        }

        private void RunAttackMoveIfNeeded()
        {
            if (!hasAttackMoveDestination)
            {
                return;
            }

            Vector3 flatDestination = new Vector3(attackMoveDestination.x, transform.position.y, attackMoveDestination.z);
            float remainingDistance = Vector3.Distance(transform.position, flatDestination);

            if (remainingDistance <= Mathf.Max(0.35f, attackRange * 0.2f))
            {
                hasAttackMoveDestination = false;
                mover.Stop();
                return;
            }

            mover.SetDestination(flatDestination);
        }

        private Vector3 GetSearchOrigin()
        {
            if (hasGuardPoint)
            {
                return guardPoint;
            }

            if (hasHoldPosition)
            {
                return holdPosition;
            }

            return transform.position;
        }

        private string BuildOrderLabel()
        {
            if (currentTarget != null)
            {
                return "Engage";
            }

            if (hasAttackMoveDestination)
            {
                return "Advance";
            }

            if (hasGuardPoint)
            {
                return "Guard";
            }

            if (hasHoldPosition)
            {
                return "Hold";
            }

            return mover != null && mover.IsMoving ? "Move" : "Idle";
        }

        private void ClearDirectiveState()
        {
            currentTarget = null;
            hasAttackMoveDestination = false;
            hasPursuitDestination = false;
            hasHoldPosition = false;
            hasGuardPoint = false;
        }
    }
}
