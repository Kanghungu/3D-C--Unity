using Game.Audio;
using Game.Prototype;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>
    /// 단일 코어: 덱 8슬롯 중 선택한 유닛만 생산 큐에 넣는다. 증원은 이 건물 하나에서만.
    /// </summary>
    public class BattleAcesCore : MonoBehaviour
    {
        private const int MaxQueueLength = 14;
        private const int MaxUnitsOnFieldPerTeam = 36;

        [SerializeField] private UnitTeam team = UnitTeam.Player;
        private readonly UnitArchetype[] deckSlots = new UnitArchetype[8];
        private readonly Queue<UnitArchetype> productionQueue = new();
        private PrototypeGameDatabase database;
        private BattleAcesEconomy economy;
        private Transform unitsParent;
        private Vector3 rallyWorldPosition;
        private Transform opponentCoreTransform;
        private UnitHealth health;
        private float productionTimeRemaining;
        private bool isProducing;
        private UnitArchetype currentProductionArchetype;

        // 플레이어 코어만 사용 — 생산 가속/장갑/자원 (티어 0~3, 최대 3회 강화)
        private int productionUpgradeTier;
        private int hullUpgradeTier;
        private int incomeUpgradeTier;

        /// <summary>팩션 규칙 — 생산 시간에 곱함(1 미만이면 빠른 생산)</summary>
        private float factionProductionDurationMultiplier = 1f;

        private static readonly int[] ProductionUpgradeCosts = { 65, 100, 140 };
        private static readonly int[] HullUpgradeCosts = { 55, 90, 130 };
        private static readonly int[] IncomeUpgradeCosts = { 75, 110, 150 };

        public UnitTeam Team => team;
        public UnitHealth Health => health;
        public int QueueCount => productionQueue.Count + (isProducing ? 1 : 0);

        /// <summary>
        /// HUD용 — 현재 생산 중인 유닛과 남은 시간(아군 코어·생산 중일 때만 유효).
        /// </summary>
        public bool TryGetNextProductionPreview(out UnitArchetype archetype, out float secondsRemaining)
        {
            archetype = UnitArchetype.Spearman;
            secondsRemaining = 0f;

            if (team != UnitTeam.Player || health == null || !health.IsAlive)
            {
                return false;
            }

            if (isProducing)
            {
                archetype = currentProductionArchetype;
                secondsRemaining = Mathf.Max(0f, productionTimeRemaining);
                return true;
            }

            if (productionQueue.Count > 0)
            {
                archetype = productionQueue.Peek();
                secondsRemaining = 0f;
                return true;
            }

            return false;
        }
        public int ProductionUpgradeTier => productionUpgradeTier;
        public int HullUpgradeTier => hullUpgradeTier;
        public int IncomeUpgradeTier => incomeUpgradeTier;

        /// <summary>덱 초기화 후 팩션에 따라 호출</summary>
        public void SetFactionProductionDurationMultiplier(float multiplier)
        {
            factionProductionDurationMultiplier = Mathf.Max(0.12f, multiplier);
        }

        public UnitArchetype GetDeckSlot(int index)
        {
            index = Mathf.Clamp(index, 0, 7);
            return deckSlots[index];
        }

        public static string GetDeckHotkeyLabel(int slot)
        {
            return slot switch
            {
                0 => "1",
                1 => "2",
                2 => "3",
                3 => "4",
                4 => "5",
                5 => "6",
                6 => "7",
                _ => "8"
            };
        }

        public void Initialize(
            UnitTeam assignedTeam,
            IReadOnlyList<UnitArchetype> deckEight,
            PrototypeGameDatabase gameDatabase,
            BattleAcesEconomy economySystem,
            Transform unitRoot,
            Vector3 rallyPosition,
            Transform enemyOrPlayerCoreForAttackMove)
        {
            team = assignedTeam;
            database = gameDatabase;
            economy = economySystem;
            unitsParent = unitRoot;
            rallyWorldPosition = rallyPosition;
            opponentCoreTransform = enemyOrPlayerCoreForAttackMove;

            for (int i = 0; i < 8; i++)
            {
                deckSlots[i] = deckEight != null && i < deckEight.Count ? deckEight[i] : UnitArchetype.Spearman;
            }
        }

        private void Awake()
        {
            health = GetComponent<UnitHealth>();
        }

        private void Update()
        {
            if (health == null || !health.IsAlive)
            {
                return;
            }

            if (team == UnitTeam.Player)
            {
                HandlePlayerDeckHotkeys();
                HandlePlayerUpgradeHotkeys();
            }

            if (isProducing)
            {
                productionTimeRemaining -= Time.deltaTime;
                if (productionTimeRemaining <= 0f)
                {
                    FinishCurrentProduction();
                }
            }
            else if (productionQueue.Count > 0)
            {
                StartNextProduction();
            }
        }

        private void HandlePlayerDeckHotkeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            for (int slot = 0; slot < 8; slot++)
            {
                Key key = slot switch
                {
                    0 => Key.Digit1,
                    1 => Key.Digit2,
                    2 => Key.Digit3,
                    3 => Key.Digit4,
                    4 => Key.Digit5,
                    5 => Key.Digit6,
                    6 => Key.Digit7,
                    _ => Key.Digit8
                };

                if (keyboard[key].wasPressedThisFrame)
                {
                    TryEnqueueDeckSlot(slot);
                }
            }
        }

        private void HandlePlayerUpgradeHotkeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || economy == null)
            {
                return;
            }

            if (keyboard.tKey.wasPressedThisFrame)
            {
                TryPurchaseProductionUpgrade();
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                TryPurchaseHullUpgrade();
            }

            if (keyboard.uKey.wasPressedThisFrame)
            {
                TryPurchaseIncomeUpgrade();
            }
        }

        /// <summary>생산 시간 단축 (플레이어 전용)</summary>
        public bool TryPurchaseProductionUpgrade()
        {
            if (team != UnitTeam.Player || economy == null || productionUpgradeTier >= 3)
            {
                return false;
            }

            int cost = ProductionUpgradeCosts[productionUpgradeTier];
            if (!economy.TrySpendPlayer(cost))
            {
                return false;
            }

            productionUpgradeTier++;
            return true;
        }

        /// <summary>코어 최대 체력 증가 (플레이어 전용)</summary>
        public bool TryPurchaseHullUpgrade()
        {
            if (team != UnitTeam.Player || economy == null || health == null || hullUpgradeTier >= 3)
            {
                return false;
            }

            int cost = HullUpgradeCosts[hullUpgradeTier];
            if (!economy.TrySpendPlayer(cost))
            {
                return false;
            }

            health.AddMaxHealthBonus(480f);
            hullUpgradeTier++;
            return true;
        }

        /// <summary>자동 자원 증가 (플레이어 전용)</summary>
        public bool TryPurchaseIncomeUpgrade()
        {
            if (team != UnitTeam.Player || economy == null || incomeUpgradeTier >= 3)
            {
                return false;
            }

            int cost = IncomeUpgradeCosts[incomeUpgradeTier];
            if (!economy.TrySpendPlayer(cost))
            {
                return false;
            }

            economy.AddPlayerIncomePerSecond(1.05f);
            incomeUpgradeTier++;
            return true;
        }

        /// <summary>슬롯 0~7 — 플레이어는 키보드, 적은 EnemyBrain에서 호출</summary>
        public bool TryEnqueueDeckSlot(int slotIndex)
        {
            if (health == null || !health.IsAlive || slotIndex < 0 || slotIndex > 7)
            {
                return false;
            }

            if (productionQueue.Count >= MaxQueueLength)
            {
                return false;
            }

            if (CountTeamUnitsOnField() >= MaxUnitsOnFieldPerTeam)
            {
                return false;
            }

            UnitArchetype archetype = deckSlots[slotIndex];
            UnitDefinition definition = database != null ? database.GetDefinition(archetype) : null;
            if (definition == null)
            {
                return false;
            }

            int cost = BattleAcesEconomy.GetTrainCost(definition);
            bool paid = team == UnitTeam.Player
                ? economy.TrySpendPlayer(cost)
                : economy.TrySpendEnemy(cost);

            if (!paid)
            {
                return false;
            }

            productionQueue.Enqueue(archetype);

            if (!isProducing)
            {
                StartNextProduction();
            }

            return true;
        }

        private void StartNextProduction()
        {
            if (productionQueue.Count == 0)
            {
                isProducing = false;
                return;
            }

            currentProductionArchetype = productionQueue.Dequeue();
            UnitDefinition def = database.GetDefinition(currentProductionArchetype);
            float duration = def != null ? Mathf.Max(1.2f, def.ProductionDuration * 0.85f) : 3f;
            if (team == UnitTeam.Player)
            {
                // 업그레이드 티어당 약 9% 생산 시간 감소 (최대 약 27%)
                float mul = Mathf.Max(0.58f, 1f - 0.09f * productionUpgradeTier);
                duration *= mul;
            }

            duration *= factionProductionDurationMultiplier;

            productionTimeRemaining = duration;
            isProducing = true;
        }

        private void FinishCurrentProduction()
        {
            UnitDefinition def = database.GetDefinition(currentProductionArchetype);
            if (def != null && health != null && health.IsAlive)
            {
                Vector3 spawnPos = GetSpawnPositionNearCore();
                SelectableUnit unit = PrototypeEntityFactory.CreateUnit(team, def, spawnPos, unitsParent);

                if (unit != null)
                {
                    if (team == UnitTeam.Player)
                    {
                        BattleAcesRunStats rs = BattleAcesRunStats.Instance;
                        if (rs != null)
                        {
                            rs.RegisterPlayerUnitProduced(currentProductionArchetype);
                        }

                        unit.MoveTo(rallyWorldPosition);
                        ProceduralAudioUtility.PlayProductionComplete();
                    }
                    else if (opponentCoreTransform != null)
                    {
                        unit.AttackMoveTo(opponentCoreTransform.position);
                    }
                }
            }

            isProducing = false;
            productionTimeRemaining = 0f;
        }

        private Vector3 GetSpawnPositionNearCore()
        {
            const float ringRadius = 8.5f;
            const float minSeparation = 2.6f;
            Vector3 best = transform.position + new Vector3(0f, 1f, ringRadius * 0.35f);
            for (int attempt = 0; attempt < 14; attempt++)
            {
                Vector2 ring = Random.insideUnitCircle * ringRadius;
                Vector3 candidate = transform.position + new Vector3(ring.x, 1f, ring.y);
                if (!IsTooCloseToFriendlySpawn(candidate, minSeparation))
                {
                    return candidate;
                }

                if (attempt > 6)
                {
                    float a = Random.Range(0f, Mathf.PI * 2f);
                    candidate = transform.position + new Vector3(Mathf.Cos(a) * ringRadius, 1f, Mathf.Sin(a) * ringRadius);
                    if (!IsTooCloseToFriendlySpawn(candidate, minSeparation * 0.85f))
                    {
                        return candidate;
                    }
                }
            }

            return best;
        }

        private bool IsTooCloseToFriendlySpawn(Vector3 candidate, float minDist)
        {
            float sq = minDist * minDist;
            foreach (SelectableUnit u in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (u == null || u.Team != team)
                {
                    continue;
                }

                Vector3 p = u.transform.position;
                p.y = candidate.y;
                if ((p - candidate).sqrMagnitude < sq)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountTeamUnitsOnField()
        {
            int count = 0;
            foreach (SelectableUnit u in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (u != null && u.Team == team && u.GetComponent<CombatTarget>() is { } ct && ct.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
