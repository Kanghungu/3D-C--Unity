using Game.Units;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Prototype
{
    public static class PrototypeEntityFactory
    {
        private static int serialNumber = 1;

        public static SelectableUnit CreateUnit(UnitTeam team, UnitDefinition definition, Vector3 position, Transform parent)
        {
            GameObject unit = GameObject.CreatePrimitive(definition.PrimitiveType);
            unit.name = $"{team} {definition.DisplayName} {serialNumber++}";
            unit.transform.position = position;
            unit.transform.localScale = PrototypeUnitBalanceUtility.GetAdjustedScale(team, definition);
            unit.transform.SetParent(parent);

            // 루트 primitive 의 외형은 BuildUnitSilhouette 자식 오브젝트가 담당한다.
            // Renderer 만 끄고 Collider 는 마우스 피킹(레이캐스트)용으로 유지한다.
            Renderer rootRenderer = unit.GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;

            // 지상 유닛만 NavMeshAgent 부착 — 비행 유닛은 직접 이동 처리
            if (!definition.IsFlying)
            {
                // 스폰 위치를 NavMesh 위로 보정 (반경을 넓게 잡아 후방 예비대 등 외곽 스폰도 처리)
                bool onNavMesh = NavMesh.SamplePosition(position, out NavMeshHit navHit, 80f, NavMesh.AllAreas);
                if (onNavMesh)
                {
                    unit.transform.position = navHit.position;

                    NavMeshAgent navAgent = unit.AddComponent<NavMeshAgent>();
                    navAgent.height                = 2f;
                    navAgent.radius                = 0.5f;
                    navAgent.angularSpeed          = 540f;
                    navAgent.acceleration          = 20f;
                    navAgent.autoBraking           = false;
                    navAgent.updateRotation        = false;
                    navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
                    navAgent.avoidancePriority     = Random.Range(30, 70);

                    float sy = unit.transform.localScale.y;
                    navAgent.baseOffset = definition.Archetype switch
                    {
                        UnitArchetype.MobileFortress => 0f,
                        UnitArchetype.Artillery      => -(sy * 0.12f),
                        _                            => -(sy * 0.42f)
                    };
                }
                else
                {
                    Debug.LogWarning($"[EntityFactory] NavMesh 범위 밖 스폰: {unit.name} @ {position} — NavMeshAgent 생략");
                }
            }

            unit.AddComponent<UnitAbilityState>();
            SimpleUnitMover mover = unit.AddComponent<SimpleUnitMover>();
            mover.Configure(PrototypeUnitBalanceUtility.GetAdjustedMoveSpeed(team, definition), 540f, definition.StoppingDistance);

            UnitHealth health = unit.AddComponent<UnitHealth>();
            health.Configure(PrototypeUnitBalanceUtility.GetAdjustedMaxHealth(team, definition), true, new Vector3(0f, 1.9f, 0f));

            CombatTarget combatTarget = unit.AddComponent<CombatTarget>();
            UnitCombat combat = unit.AddComponent<UnitCombat>();
            combat.Configure(
                PrototypeUnitBalanceUtility.GetAdjustedAttackRange(team, definition),
                PrototypeUnitBalanceUtility.GetAdjustedAttackDamage(team, definition),
                definition.AttackCooldown,
                definition.AggroRange,
                0.55f,
                definition.UsesProjectile,
                definition.ProjectileSpeed,
                definition.ProjectileArc,
                definition.SplashRadius,
                definition.ImpactEffectScale);

            SelectableUnit selectableUnit = unit.AddComponent<SelectableUnit>();
            unit.AddComponent<AdvancedUnitRoleController>();

            combatTarget.Initialize(team, health);
            selectableUnit.Initialize(team, definition, mover, combat);
            PrototypeEntityVisualFactory.BuildUnitSilhouette(unit.transform, definition, team);
            combat.Initialize(combatTarget, health);

            if (definition.Archetype == UnitArchetype.MobileFortress)
            {
                AttachMobileFortressProduction(team, unit, combatTarget, health, parent);
            }

            return selectableUnit;
        }

        public static BaseStructure CreateBase(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObject.name = name;
            baseObject.transform.position = position;
            baseObject.transform.localScale = team == UnitTeam.Player ? new Vector3(6.4f, 3.6f, 6.4f) : new Vector3(5.8f, 3.4f, 5.8f);
            baseObject.transform.SetParent(parent);
            PrototypeEntityVisualFactory.ApplyBaseVisuals(baseObject, team);

            UnitHealth health = baseObject.AddComponent<UnitHealth>();
            health.Configure(team == UnitTeam.Player ? 2600f : 2400f, true, new Vector3(0f, 4.2f, 0f));

            CombatTarget combatTarget = baseObject.AddComponent<CombatTarget>();
            BaseStructure baseStructure = baseObject.AddComponent<BaseStructure>();
            combatTarget.Initialize(team, health);
            baseStructure.InitializeTarget(combatTarget, health);
            return baseStructure;
        }

        public static ProductionStructure CreateProductionStructure(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject structureObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structureObject.name = name;
            structureObject.transform.position = position;
            structureObject.transform.localScale = new Vector3(4f, 2.4f, 4f);
            structureObject.transform.SetParent(parent);
            PrototypeEntityVisualFactory.ApplyProductionVisuals(structureObject, team, name.Contains("Siege"));

            UnitHealth health = structureObject.AddComponent<UnitHealth>();
            health.Configure(team == UnitTeam.Player ? 320f : 220f, true, new Vector3(0f, 3.4f, 0f));

            CombatTarget combatTarget = structureObject.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);
            return structureObject.AddComponent<ProductionStructure>();
        }

        public static DefensiveTurret CreateTurret(string name, Vector3 position, UnitTeam team, Transform parent)
        {
            GameObject turretObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretObject.name = name;
            turretObject.transform.position = position;
            turretObject.transform.localScale = team == UnitTeam.Player ? new Vector3(1.05f, 1.1f, 1.05f) : new Vector3(1f, 1f, 1f);
            turretObject.transform.SetParent(parent);
            PrototypeEntityVisualFactory.ApplyTurretVisuals(turretObject, team);

            UnitHealth health = turretObject.AddComponent<UnitHealth>();
            health.Configure(team == UnitTeam.Player ? 185f : 105f, true, new Vector3(0f, 2.6f, 0f));

            CombatTarget combatTarget = turretObject.AddComponent<CombatTarget>();
            combatTarget.Initialize(team, health);
            DefensiveTurret turret = turretObject.AddComponent<DefensiveTurret>();
            turret.Configure(team == UnitTeam.Player ? 14f : 11f, team == UnitTeam.Player ? 16f : 11f, 1.05f, 24f, 0.4f);
            return turret;
        }

        public static ControlNode CreateControlNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nodeObject.name = name;
            nodeObject.transform.position = position;
            nodeObject.transform.localScale = new Vector3(13.5f, 0.55f, 13.5f);
            nodeObject.transform.SetParent(parent);
            PrototypeEntityVisualFactory.BuildControlNodeSilhouette(nodeObject.transform);
            return nodeObject.AddComponent<ControlNode>();
        }

        private static void AttachMobileFortressProduction(UnitTeam team, GameObject unit, CombatTarget combatTarget, UnitHealth health, Transform parent)
        {
            ProductionStructure production = unit.AddComponent<ProductionStructure>();
            production.ConfigureStructure(
                team == UnitTeam.Player ? "Moving Bastion" : "War Bastion",
                new Vector3(24f, 0f, 12f),
                team == UnitTeam.Player ? new Color(0.7f, 0.86f, 0.98f) : new Color(0.98f, 0.6f, 0.34f),
                UnitArchetype.Spearman,
                UnitArchetype.ShieldInfantry,
                UnitArchetype.Rifleman,
                UnitArchetype.SpecialWarrior);
            production.Initialize(combatTarget, health, parent, PrototypeRuntimeQuery.FindDatabase(), PrototypeRuntimeQuery.FindBase(team));
        }
    }
}
