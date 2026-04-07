using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// 기지·생산·거점·초기 병력 배치 (MVP 규모 축소 버전).
    /// </summary>
    public partial class PrototypeBootstrapper
    {
        private void SetupBases()
        {
            Transform structuresRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Structures");

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            if (playerBase == null)
            {
                playerBase = PrototypeEntityFactory.CreateBase("Player Sanctum", playerBasePosition, UnitTeam.Player, structuresRoot);
            }

            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            if (enemyBase == null)
            {
                enemyBase = PrototypeEntityFactory.CreateBase("Enemy Bastion", enemyBasePosition, UnitTeam.Enemy, structuresRoot);
            }

            ProductionStructure playerFoundry = PrototypeRuntimeQuery.FindProductionStructure(UnitTeam.Player, UnitArchetype.Spearman);
            if (playerFoundry == null)
            {
                playerFoundry = PrototypeEntityFactory.CreateProductionStructure("Holy Foundry", playerFoundryPosition, UnitTeam.Player, structuresRoot);
            }

            ProductionStructure playerSiegeWorks = PrototypeRuntimeQuery.FindProductionStructure(UnitTeam.Player, UnitArchetype.Artillery);
            if (playerSiegeWorks == null || playerSiegeWorks == playerFoundry)
            {
                playerSiegeWorks = PrototypeEntityFactory.CreateProductionStructure("Siege Chapel", playerSiegePosition, UnitTeam.Player, structuresRoot);
            }

            // 보병 삼각만 Foundry — 특수 유닛 생산 제거로 입력 단순화
            playerFoundry.ConfigureStructure(
                "Holy Foundry",
                new Vector3(380f, 0f, 150f),
                new Color(0.74f, 0.7f, 0.56f),
                UnitArchetype.Spearman,
                UnitArchetype.ShieldInfantry,
                UnitArchetype.Rifleman);

            // 공성 라인: 포병 + 전투기만 (이동/비행 거점 생산 제거)
            playerSiegeWorks.ConfigureStructure(
                "Siege Chapel",
                new Vector3(460f, 0f, 180f),
                new Color(0.84f, 0.58f, 0.32f),
                UnitArchetype.Artillery,
                UnitArchetype.Fighter);

            EnsureTurret("Player Sentinel Left", new Vector3(-1460f, 1f, -720f), UnitTeam.Player, structuresRoot);
            EnsureTurret("Enemy Cannon Right", new Vector3(1620f, 1f, 680f), UnitTeam.Enemy, structuresRoot);

            RebuildBattlefieldNodes(structuresRoot);

            playerBase.Initialize(playerUnitRoot, database);
            enemyBase.Initialize(enemyUnitRoot, database);

            CombatTarget foundryTarget = playerFoundry.GetComponent<CombatTarget>();
            UnitHealth foundryHealth = playerFoundry.GetComponent<UnitHealth>();
            playerFoundry.Initialize(foundryTarget, foundryHealth, playerUnitRoot, database, playerBase);

            CombatTarget siegeTarget = playerSiegeWorks.GetComponent<CombatTarget>();
            UnitHealth siegeHealth = playerSiegeWorks.GetComponent<UnitHealth>();
            playerSiegeWorks.Initialize(siegeTarget, siegeHealth, playerUnitRoot, database, playerBase);
        }

        private void RebuildBattlefieldNodes(Transform parent)
        {
            foreach (ControlNode existingNode in FindObjectsByType<ControlNode>(FindObjectsSortMode.None))
            {
                if (existingNode != null)
                {
                    DestroyImmediate(existingNode.gameObject);
                }
            }

            CreateBattlefieldNodeGrid(parent);
        }

        private static void EnsureTurret(string turretName, Vector3 position, UnitTeam team, Transform parent)
        {
            foreach (DefensiveTurret turret in FindObjectsByType<DefensiveTurret>(FindObjectsSortMode.None))
            {
                if (turret != null && turret.name == turretName)
                {
                    return;
                }
            }

            PrototypeEntityFactory.CreateTurret(turretName, position, team, parent);
        }

        /// <summary>
        /// 과거 7×5(35거점) 대신 3×3(9거점)으로 축소 — 중앙 쟁탈은 유지.
        /// </summary>
        private void CreateBattlefieldNodeGrid(Transform parent)
        {
            float[] xPositions = BuildAxisPositions(3, groundScale.x * 4.35f, 260f);
            float[] zPositions = BuildAxisPositions(3, groundScale.z * 4.05f, 240f);
            Transform terrainRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");

            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Shrine Center Spine", new Vector3(0f, 0.16f, 0f), new Vector3(140f, 0.32f, groundScale.z * 0.78f), terrainRoot, Color.Lerp(stoneColor, altarColor, 0.18f));
            PrototypeTerrainPrimitiveFactory.CreateTerrainBlock("Shrine Mid Crossway", new Vector3(0f, 0.16f, 0f), new Vector3(groundScale.x * 0.62f, 0.28f, 24f), terrainRoot, Color.Lerp(stoneColor, ruinColor, 0.14f));

            for (int z = 0; z < zPositions.Length; z++)
            {
                for (int x = 0; x < xPositions.Length; x++)
                {
                    string label = $"Shrine {z * xPositions.Length + x + 1}";
                    bool isGrandNode = x == xPositions.Length / 2 && z == zPositions.Length / 2;
                    bool isMajorNode = x == xPositions.Length / 2 || z == zPositions.Length / 2;
                    float bonus = isGrandNode ? 1.2f : (isMajorNode ? 1.1f : 1.05f);
                    float healing = isGrandNode ? 8.4f : (isMajorNode ? 6.8f : 5.4f);
                    Vector3 position = new Vector3(xPositions[x], 0.35f, zPositions[z]);

                    if (isGrandNode)
                    {
                        label = "Grand Altar";
                    }
                    else if (isMajorNode)
                    {
                        label = $"Major Shrine {z * xPositions.Length + x + 1}";
                    }

                    Vector3 platformScale = isGrandNode ? new Vector3(30f, 0.4f, 30f) : (isMajorNode ? new Vector3(24f, 0.34f, 24f) : new Vector3(18f, 0.26f, 18f));
                    PrototypeTerrainPrimitiveFactory.CreateTerrainBlock($"{label} Platform", position + new Vector3(0f, 0.16f, 0f), platformScale, terrainRoot, isMajorNode ? altarColor : Color.Lerp(stoneColor, altarColor, 0.35f));

                    if (isGrandNode)
                    {
                        PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"{label} Pillar A", position + new Vector3(-12f, 4.2f, -12f), terrainRoot, ruinColor);
                        PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"{label} Pillar B", position + new Vector3(12f, 4.2f, 12f), terrainRoot, ruinColor);
                    }
                    else if (isMajorNode)
                    {
                        PrototypeTerrainPrimitiveFactory.CreateTerrainPillar($"{label} Marker", position + new Vector3(0f, 3.2f, -10f), terrainRoot, ruinColor);
                    }

                    CreateShrineNode($"{label} Node", position, label, bonus, healing, isGrandNode ? ControlNodeTier.Grand : (isMajorNode ? ControlNodeTier.Major : ControlNodeTier.Minor), parent);
                }
            }
        }

        private void SetupUnits()
        {
            if (FindAnyObjectByType<SelectableUnit>() != null)
            {
                return;
            }

            // 초기 그리드: 삼각 보병 위주 (특수전사 행 제거)
            PrototypeArmyDeploymentUtility.CreateGridFormation(database, friendlyStart, friendlyGrid, unitSpacing, UnitTeam.Player, playerUnitRoot,
                row => row switch
                {
                    <= 4 => UnitArchetype.ShieldInfantry,
                    <= 8 => UnitArchetype.Spearman,
                    _ => UnitArchetype.Rifleman
                });

            PrototypeArmyDeploymentUtility.CreateGridFormation(database, enemyStart, enemyGrid, unitSpacing, UnitTeam.Enemy, enemyUnitRoot,
                row => row switch
                {
                    <= 3 => UnitArchetype.Rifleman,
                    <= 7 => UnitArchetype.Spearman,
                    _ => UnitArchetype.Rifleman
                });

            PrototypeArmyDeploymentUtility.CreateLine(database, friendlyStart + new Vector3(160f, 0f, 260f), UnitTeam.Player, playerUnitRoot, 6, 12f, UnitArchetype.Artillery);
            PrototypeArmyDeploymentUtility.CreateLine(database, enemyStart + new Vector3(120f, 0f, -220f), UnitTeam.Enemy, enemyUnitRoot, 4, 12f, UnitArchetype.Artillery);
            PrototypeArmyDeploymentUtility.CreateWing(database, friendlyStart + new Vector3(180f, 7.5f, 420f), UnitTeam.Player, playerUnitRoot, 5, 14f, UnitArchetype.Fighter);
            PrototypeArmyDeploymentUtility.CreateWing(database, enemyStart + new Vector3(140f, 7.5f, -320f), UnitTeam.Enemy, enemyUnitRoot, 4, 14f, UnitArchetype.Fighter);

            CreatePlayerVanguard();
            CreateEnemyGunline();
            CreateRearReserves();
        }

        private static void CreateShrineNode(string objectName, Vector3 position, string label, float captureMultiplier, float healingRadius, ControlNodeTier tier, Transform parent)
        {
            ControlNode node = PrototypeEntityFactory.CreateControlNode(objectName, position, parent);
            node.ConfigureNode(label, captureMultiplier, healingRadius, tier);
            PrototypeTerrainPrimitiveFactory.EnsureFogObject(node.gameObject, BattlefieldFogRequirement.Explored);
        }

        private void CreatePlayerVanguard()
        {
            Vector3 spearAnchor = friendlyStart + new Vector3(90f, 0f, 332f);
            for (int index = 0; index < 8; index++)
            {
                float offsetX = (index - 3.5f) * 10f;
                float offsetZ = Mathf.Abs(index - 3.5f) * 5.8f;
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, spearAnchor + new Vector3(offsetX, 0f, offsetZ), UnitTeam.Player, playerUnitRoot, UnitArchetype.Spearman);
            }

            Vector3 shieldAnchor = friendlyStart + new Vector3(220f, 0f, 288f);
            for (int index = 0; index < 6; index++)
            {
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, shieldAnchor + new Vector3((index - 2.5f) * 9f, 0f, (index % 2 == 0 ? 0f : 10f)), UnitTeam.Player, playerUnitRoot, UnitArchetype.ShieldInfantry);
            }
        }

        private void CreateEnemyGunline()
        {
            Vector3 rifleAnchor = enemyStart + new Vector3(180f, 0f, -336f);
            for (int index = 0; index < 10; index++)
            {
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, rifleAnchor + new Vector3((index - 4.5f) * 9.6f, 0f, (index % 2 == 0 ? 0f : -12f)), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Rifleman);
            }

            Vector3 spearAnchor = enemyStart + new Vector3(-40f, 0f, -246f);
            for (int index = 0; index < 6; index++)
            {
                PrototypeArmyDeploymentUtility.CreateSingleUnit(database, spearAnchor + new Vector3((index - 2.5f) * 10.4f, 0f, Mathf.Abs(index - 2.5f) * -4.2f), UnitTeam.Enemy, enemyUnitRoot, UnitArchetype.Spearman);
            }
        }

        private void CreateRearReserves()
        {
            PrototypeArmyDeploymentUtility.CreateLine(database, enemyBasePosition + new Vector3(-240f, 0f, -220f), UnitTeam.Enemy, enemyUnitRoot, 2, 12f, UnitArchetype.Rifleman);
            PrototypeArmyDeploymentUtility.CreateLine(database, playerBasePosition + new Vector3(40f, 0f, 360f), UnitTeam.Player, playerUnitRoot, 2, 12f, UnitArchetype.ShieldInfantry);
        }
    }
}
