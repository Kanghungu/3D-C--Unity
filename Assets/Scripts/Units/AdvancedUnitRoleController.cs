using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Archetype-specific prototype behaviors for advanced unit roles.
    /// </summary>
    public class AdvancedUnitRoleController : MonoBehaviour
    {
        private SelectableUnit selectableUnit;
        private SimpleUnitMover mover;
        private UnitCombat combat;
        private UnitAbilityState abilityState;
        private ProductionStructure productionStructure;
        private BaseStructure homeBase;
        private int dockedRiflemen;
        private bool isAirborne;

        public UnitArchetype Archetype => selectableUnit != null ? selectableUnit.Archetype : UnitArchetype.Spearman;
        public bool IsAirborne => isAirborne;
        public int DockedRiflemen => dockedRiflemen;

        private void Awake()
        {
            selectableUnit = GetComponent<SelectableUnit>();
            mover = GetComponent<SimpleUnitMover>();
            combat = GetComponent<UnitCombat>();
            abilityState = GetComponent<UnitAbilityState>();
            productionStructure = GetComponent<ProductionStructure>();
        }

        private void Start()
        {
            if (selectableUnit != null)
            {
                homeBase = PrototypeRuntimeQuery.FindBase(selectableUnit.Team);
                isAirborne = selectableUnit.Definition != null && selectableUnit.Definition.IsFlying && selectableUnit.Archetype == UnitArchetype.Fighter;
                SnapVerticalState();
            }
        }

        private void Update()
        {
            if (selectableUnit == null || selectableUnit.Definition == null)
            {
                return;
            }

            if (Archetype == UnitArchetype.RoyalGuard)
            {
                EnforceRoyalGuardLeash();
            }

            if (Archetype == UnitArchetype.AirborneCitadel)
            {
                UpdateAirborneCitadelState();
                AbsorbNearbyRiflemen();
            }
            else if (selectableUnit.Definition.IsFlying)
            {
                MaintainHoverHeight(selectableUnit.Definition.HoverHeight);
            }

            TryAutoActivateRoleAbilityIfNeeded();
        }

        public bool AllowsMovement()
        {
            return !(productionStructure != null && productionStructure.HasQueuedProduction);
        }

        public bool AllowsCombat()
        {
            if (productionStructure != null && productionStructure.HasQueuedProduction)
            {
                return false;
            }

            if (Archetype == UnitArchetype.AirborneCitadel)
            {
                return isAirborne;
            }

            return true;
        }

        public float GetPassiveMoveSpeedMultiplier()
        {
            return Archetype switch
            {
                UnitArchetype.SpecialWarrior => combat != null && combat.CurrentTarget != null ? 1.22f : 1.08f,
                UnitArchetype.RoyalGuard => IsNearHomeBase(18f) ? 1.02f : 0.92f,
                UnitArchetype.Outrider => combat != null && combat.CurrentTarget != null ? 1.3f : 1.18f,
                _ => 1f
            };
        }

        public float GetPassiveAttackDamageMultiplier()
        {
            return Archetype switch
            {
                UnitArchetype.SpecialWarrior => combat != null && combat.CurrentTarget != null ? 1.22f : 1.1f,
                UnitArchetype.RoyalGuard => IsNearHomeBase(22f) ? 1.28f : 1.14f,
                UnitArchetype.Outrider => 1.08f,
                _ => 1f
            };
        }

        public float GetPassiveAttackRangeBonus()
        {
            return Archetype switch
            {
                UnitArchetype.SpecialWarrior => 0.65f,
                UnitArchetype.RoyalGuard => 0.4f,
                UnitArchetype.Outrider => 0.25f,
                _ => 0f
            };
        }

        public float ModifyIncomingDamage(float damage)
        {
            float multiplier = Archetype switch
            {
                UnitArchetype.SpecialWarrior => combat != null && combat.CurrentTarget != null ? 0.9f : 0.96f,
                UnitArchetype.RoyalGuard => IsNearHomeBase(24f) ? 0.68f : 0.82f,
                UnitArchetype.Outrider => 0.94f,
                _ => 1f
            };

            return damage * multiplier;
        }

        public Vector3 AdjustDestination(Vector3 destination)
        {
            if (Archetype == UnitArchetype.RoyalGuard && homeBase != null)
            {
                return ClampToRadius(destination, homeBase.transform.position, 86f);
            }

            if (Archetype == UnitArchetype.MobileFortress)
            {
                destination = KeepAwayFromEnemyStrongholds(destination, 78f);
            }

            return destination;
        }

        public bool CanAcceptTarget(CombatTarget target)
        {
            if (target == null || selectableUnit == null)
            {
                return false;
            }

            if (Archetype == UnitArchetype.RoyalGuard && target.Team != selectableUnit.Team && target.GetComponent<BaseStructure>() != null)
            {
                return false;
            }

            if (Archetype == UnitArchetype.MobileFortress && target.Team != selectableUnit.Team && IsNearEnemyRestrictedZone(target.transform.position))
            {
                return false;
            }

            return true;
        }

        public int GetProjectileBurstCount(float targetDistance)
        {
            if (Archetype != UnitArchetype.AirborneCitadel)
            {
                return 1;
            }

            return Mathf.Clamp(1 + dockedRiflemen, 1, 7);
        }

        public Vector3 GetProjectileAimOffset(float targetDistance)
        {
            if (Archetype != UnitArchetype.AirborneCitadel || dockedRiflemen <= 0)
            {
                return Vector3.zero;
            }

            float spread = Mathf.Lerp(0.12f, 1.25f, Mathf.Clamp01(targetDistance / 24f));
            return new Vector3(Random.Range(-spread, spread), 0f, Random.Range(-spread, spread));
        }

        private void EnforceRoyalGuardLeash()
        {
            if (homeBase == null)
            {
                return;
            }

            Vector3 anchor = homeBase.transform.position;
            anchor.y = transform.position.y;

            if (Vector3.Distance(transform.position, anchor) <= 86f)
            {
                return;
            }

            combat?.ClearTarget();
            mover?.SetDestination(ClampToRadius(transform.position, anchor, 72f));
        }

        private void TryAutoActivateRoleAbilityIfNeeded()
        {
            if (selectableUnit == null ||
                selectableUnit.Team != UnitTeam.Enemy ||
                abilityState == null ||
                !abilityState.IsReady ||
                combat == null ||
                combat.CurrentTarget == null)
            {
                return;
            }

            float targetDistance = Vector3.Distance(transform.position, combat.CurrentTarget.transform.position);
            switch (Archetype)
            {
                case UnitArchetype.SpecialWarrior:
                    if (targetDistance <= 8.5f)
                    {
                        abilityState.TryActivateRoleAbility();
                    }

                    break;
                case UnitArchetype.RoyalGuard:
                    if (targetDistance <= 7.5f || IsNearHomeBase(24f))
                    {
                        abilityState.TryActivateRoleAbility();
                    }

                    break;
                case UnitArchetype.Outrider:
                    if (targetDistance <= 6.5f)
                    {
                        abilityState.TryActivateRoleAbility();
                    }

                    break;
            }
        }

        private void UpdateAirborneCitadelState()
        {
            bool shouldFly = (combat != null && combat.CurrentTarget != null) || (mover != null && mover.IsMoving);
            isAirborne = shouldFly;
            MaintainHoverHeight(shouldFly ? selectableUnit.Definition.HoverHeight : 1f);
        }

        private void MaintainHoverHeight(float targetHeight)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(position.y, targetHeight, Time.deltaTime * 3f);
            transform.position = position;
        }

        private void SnapVerticalState()
        {
            if (selectableUnit == null || selectableUnit.Definition == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.y = selectableUnit.Definition.IsFlying ? selectableUnit.Definition.HoverHeight : 1f;
            transform.position = position;
        }

        private void AbsorbNearbyRiflemen()
        {
            if (isAirborne || dockedRiflemen >= 6 || selectableUnit.Team != UnitTeam.Player)
            {
                return;
            }

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || unit == selectableUnit || unit.Team != selectableUnit.Team || unit.Archetype != UnitArchetype.Rifleman)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, unit.transform.position) > 6f)
                {
                    continue;
                }

                dockedRiflemen++;
                Destroy(unit.gameObject);

                if (dockedRiflemen >= 6)
                {
                    break;
                }
            }
        }

        private Vector3 KeepAwayFromEnemyStrongholds(Vector3 destination, float minDistance)
        {
            if (selectableUnit == null)
            {
                return destination;
            }

            UnitTeam enemyTeam = selectableUnit.Team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player;
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(enemyTeam);

            if (enemyBase != null)
            {
                destination = PushAwayFromPoint(destination, enemyBase.transform.position, minDistance);
            }

            foreach (ControlNode node in PrototypeRuntimeQuery.FindControlNodes())
            {
                if (node == null)
                {
                    continue;
                }

                destination = PushAwayFromPoint(destination, node.transform.position, minDistance * 0.65f);
            }

            return destination;
        }

        private bool IsNearEnemyRestrictedZone(Vector3 position)
        {
            if (selectableUnit == null)
            {
                return false;
            }

            UnitTeam enemyTeam = selectableUnit.Team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player;
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(enemyTeam);

            if (enemyBase != null && Vector3.Distance(position, enemyBase.transform.position) < 78f)
            {
                return true;
            }

            foreach (ControlNode node in PrototypeRuntimeQuery.FindControlNodes())
            {
                if (node != null && Vector3.Distance(position, node.transform.position) < 52f)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNearHomeBase(float radius)
        {
            if (homeBase == null)
            {
                return false;
            }

            return Vector3.Distance(transform.position, homeBase.transform.position) <= radius;
        }

        private static Vector3 ClampToRadius(Vector3 point, Vector3 center, float radius)
        {
            Vector3 offset = point - center;
            offset.y = 0f;

            if (offset.sqrMagnitude <= radius * radius)
            {
                point.y = 1f;
                return point;
            }

            Vector3 clamped = center + offset.normalized * radius;
            clamped.y = 1f;
            return clamped;
        }

        private static Vector3 PushAwayFromPoint(Vector3 point, Vector3 center, float minDistance)
        {
            Vector3 offset = point - center;
            offset.y = 0f;

            if (offset.sqrMagnitude >= minDistance * minDistance)
            {
                point.y = Mathf.Max(1f, point.y);
                return point;
            }

            if (offset.sqrMagnitude <= 0.001f)
            {
                offset = Vector3.forward;
            }

            Vector3 adjusted = center + offset.normalized * minDistance;
            adjusted.y = Mathf.Max(1f, point.y);
            return adjusted;
        }
    }
}

