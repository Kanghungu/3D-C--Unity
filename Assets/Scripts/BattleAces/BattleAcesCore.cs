using Game.Audio;
using Game.Prototype;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>덱 생산 큐 실패 원인 — HUD 한 줄 안내용</summary>
    public enum DeckEnqueueFailReason
    {
        None = 0,
        InvalidOrDead,
        BriefingBlocking,
        MatchFinished,
        QueueFull,
        FieldCap,
        NoDefinition,
        InsufficientCredits
    }

    /// <summary>T/Y/U 코어 강화 실패 — 생산 거절과 동일 HUD 막대</summary>
    public enum UpgradePurchaseFailReason
    {
        None = 0,
        NotApplicable,
        BriefingBlocking,
        MatchFinished,
        MaxTier,
        InsufficientCredits,
        InvalidState
    }

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

        /// <summary>월드 랠리 지점 — 생산 완료 유닛이 먼저 향함(플레이어는 Alt+우클릭으로 설정)</summary>
        private GameObject rallyWorldPing;
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

        /// <summary>덱·강화 거절 힌트 스팸 방지(같은 쿨다운 공유)</summary>
        private static float lastPlayerEconomyRejectUnscaled = -999f;
        private const float PlayerEconomyRejectCooldownSeconds = 0.38f;

        // 챕터3: 한 판 안에서 1~2단계는 무난히, 3단계는 판세 따라 달성(이전보다 약간 저렴·효과↑)
        private static readonly int[] ProductionUpgradeCosts = { 52, 82, 115 };
        private static readonly int[] HullUpgradeCosts = { 45, 75, 105 };
        private static readonly int[] IncomeUpgradeCosts = { 58, 88, 118 };

        public UnitTeam Team => team;
        public UnitHealth Health => health;
        public int QueueCount => productionQueue.Count + (isProducing ? 1 : 0);

        /// <summary>현재 조선 중인 유닛을 제외한 대기열 길이(HUD용)</summary>
        public int QueuedProductionCount => productionQueue.Count;

        public bool TryGetNextProductionUpgradeCost(out int cost)
        {
            cost = 0;
            if (team != UnitTeam.Player || productionUpgradeTier >= 3)
            {
                return false;
            }

            cost = ProductionUpgradeCosts[productionUpgradeTier];
            return true;
        }

        public bool TryGetNextHullUpgradeCost(out int cost)
        {
            cost = 0;
            if (team != UnitTeam.Player || hullUpgradeTier >= 3)
            {
                return false;
            }

            cost = HullUpgradeCosts[hullUpgradeTier];
            return true;
        }

        public bool TryGetNextIncomeUpgradeCost(out int cost)
        {
            cost = 0;
            if (team != UnitTeam.Player || incomeUpgradeTier >= 3)
            {
                return false;
            }

            cost = IncomeUpgradeCosts[incomeUpgradeTier];
            return true;
        }

        /// <summary>
        /// HUD용 — 현재 생산 중인 유닛·남은 시간, 또는 대기열 맨 앞(조선 시작 전) 미리보기.
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

        /// <summary>현재 생산 랠리(월드 좌표)</summary>
        public Vector3 RallyWorldPosition => rallyWorldPosition;

        /// <summary>플레이어 전용 — 지면 우클릭(Alt)으로 집결 지점 설정</summary>
        public void SetRallyWorldPosition(Vector3 worldOnGround)
        {
            if (team != UnitTeam.Player)
            {
                return;
            }

            rallyWorldPosition = worldOnGround;
            EnsurePlayerRallyPing();
            if (rallyWorldPing != null)
            {
                rallyWorldPing.transform.position = new Vector3(worldOnGround.x, 0.14f, worldOnGround.z);
                rallyWorldPing.SetActive(true);
            }
        }

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

        /// <summary>씬에 있는 살아 있는 Battle Aces 코어 검색 — RTS 카메라 Home/Space 등.</summary>
        public static bool TryFindAliveCore(UnitTeam team, out BattleAcesCore core)
        {
            core = null;
            BattleAcesCore[] found = Object.FindObjectsByType<BattleAcesCore>(FindObjectsInactive.Exclude);
            if (found == null || found.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < found.Length; i++)
            {
                BattleAcesCore candidate = found[i];
                if (candidate == null || candidate.Team != team)
                {
                    continue;
                }

                UnitHealth candidateHealth = candidate.Health;
                if (candidateHealth != null && candidateHealth.IsAlive)
                {
                    core = candidate;
                    return true;
                }
            }

            return false;
        }

        public void Initialize(
            UnitTeam assignedTeam,
            IReadOnlyList<UnitArchetype> deckEight,
            PrototypeGameDatabase gameDatabase,
            BattleAcesEconomy economySystem,
            Transform unitRoot,
            Vector3 rallyPosition)
        {
            team = assignedTeam;
            database = gameDatabase;
            economy = economySystem;
            unitsParent = unitRoot;
            rallyWorldPosition = rallyPosition;

            for (int i = 0; i < 8; i++)
            {
                deckSlots[i] = deckEight != null && i < deckEight.Count ? deckEight[i] : UnitArchetype.Spearman;
            }
        }

        private void Awake()
        {
            health = GetComponent<UnitHealth>();
        }

        private void OnDestroy()
        {
            if (rallyWorldPing != null)
            {
                Destroy(rallyWorldPing);
                rallyWorldPing = null;
            }
        }

        /// <summary>집결 위치 시각 표시(플레이어만)</summary>
        private void EnsurePlayerRallyPing()
        {
            if (team != UnitTeam.Player || rallyWorldPing != null)
            {
                return;
            }

            rallyWorldPing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rallyWorldPing.name = "BA_PlayerRallyPing";
            rallyWorldPing.transform.localScale = new Vector3(3.8f, 0.035f, 3.8f);
            if (rallyWorldPing.TryGetComponent(out Collider col))
            {
                col.enabled = false;
            }

            if (rallyWorldPing.TryGetComponent(out Renderer rend))
            {
                rend.material.color = new Color(0.22f, 0.82f, 0.95f, 1f);
            }
        }

        private void Update()
        {
            if (health == null || !health.IsAlive)
            {
                return;
            }

            if (team == UnitTeam.Player)
            {
                bool briefingBlocks =
                    BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking;

                // 브리핑·승패 확정 후에는 덱·업그레이드 입력 무시(자원 소모·거절음 방지)
                if (!briefingBlocks &&
                    (!BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController matchCtrl) ||
                     !matchCtrl.IsFinished))
                {
                    HandlePlayerDeckHotkeys();
                    HandlePlayerUpgradeHotkeys();
                }
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
                    if (!TryEnqueueDeckSlot(slot, out DeckEnqueueFailReason failReason))
                    {
                        if (Time.unscaledTime - lastPlayerEconomyRejectUnscaled >= PlayerEconomyRejectCooldownSeconds)
                        {
                            lastPlayerEconomyRejectUnscaled = Time.unscaledTime;
                            string hint = GetDeckEnqueueFailHintKo(failReason);
                            if (!string.IsNullOrEmpty(hint))
                            {
                                ProceduralAudioUtility.PlayUiCommandRejected();
                                BattleAcesHudOverlay.PulseDeckRejectHint(hint);
                            }
                            else if (failReason != DeckEnqueueFailReason.InvalidOrDead)
                            {
                                ProceduralAudioUtility.PlayUiCommandRejected();
                            }
                        }
                    }
                }
            }
        }

        private void HandlePlayerUpgradeHotkeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || economy == null || team != UnitTeam.Player)
            {
                return;
            }

            if (keyboard.tKey.wasPressedThisFrame)
            {
                TryHotkeyUpgrade(TryPurchaseProductionUpgrade, "생산 T");
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                TryHotkeyUpgrade(TryPurchaseHullUpgrade, "장갑 Y");
            }

            if (keyboard.uKey.wasPressedThisFrame)
            {
                TryHotkeyUpgrade(TryPurchaseIncomeUpgrade, "수입 U");
            }
        }

        private delegate bool TryUpgradeOutDelegate(out UpgradePurchaseFailReason failReason);

        private void TryHotkeyUpgrade(TryUpgradeOutDelegate tryPurchase, string labelForHint)
        {
            if (Time.unscaledTime - lastPlayerEconomyRejectUnscaled < PlayerEconomyRejectCooldownSeconds)
            {
                return;
            }

            if (tryPurchase(out UpgradePurchaseFailReason fail))
            {
                return;
            }

            string hint = GetUpgradePurchaseFailHintKo(fail, labelForHint);
            lastPlayerEconomyRejectUnscaled = Time.unscaledTime;
            if (!string.IsNullOrEmpty(hint))
            {
                ProceduralAudioUtility.PlayUiCommandRejected();
                BattleAcesHudOverlay.PulseDeckRejectHint(hint);
            }
            else if (fail != UpgradePurchaseFailReason.NotApplicable)
            {
                ProceduralAudioUtility.PlayUiCommandRejected();
            }
        }

        public static string GetUpgradePurchaseFailHintKo(UpgradePurchaseFailReason reason, string upgradeShortLabel)
        {
            return reason switch
            {
                UpgradePurchaseFailReason.BriefingBlocking => $"강화 불가: 브리핑 중 ({upgradeShortLabel})",
                UpgradePurchaseFailReason.MatchFinished => $"강화 불가: 전투 종료 ({upgradeShortLabel})",
                UpgradePurchaseFailReason.InsufficientCredits => $"강화 불가: 자원 부족 ({upgradeShortLabel})",
                UpgradePurchaseFailReason.MaxTier => $"강화 불가: {upgradeShortLabel} 만령",
                UpgradePurchaseFailReason.InvalidState => $"강화 불가: 코어 상태 확인 ({upgradeShortLabel})",
                _ => null
            };
        }

        /// <summary>플레이어 강화 공통 게이트 — 생산 큐와 동일 조건</summary>
        private static bool TryGetPlayerUpgradeBlockedReason(out UpgradePurchaseFailReason blockReason)
        {
            blockReason = UpgradePurchaseFailReason.None;
            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
            {
                blockReason = UpgradePurchaseFailReason.BriefingBlocking;
                return true;
            }

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m) && m.IsFinished)
            {
                blockReason = UpgradePurchaseFailReason.MatchFinished;
                return true;
            }

            return false;
        }

        /// <summary>생산 시간 단축 (플레이어 전용)</summary>
        public bool TryPurchaseProductionUpgrade()
        {
            return TryPurchaseProductionUpgrade(out _);
        }

        public bool TryPurchaseProductionUpgrade(out UpgradePurchaseFailReason failReason)
        {
            failReason = UpgradePurchaseFailReason.None;
            if (team != UnitTeam.Player || economy == null)
            {
                failReason = UpgradePurchaseFailReason.NotApplicable;
                return false;
            }

            if (TryGetPlayerUpgradeBlockedReason(out UpgradePurchaseFailReason blocked))
            {
                failReason = blocked;
                return false;
            }

            if (productionUpgradeTier >= 3)
            {
                failReason = UpgradePurchaseFailReason.MaxTier;
                return false;
            }

            int cost = ProductionUpgradeCosts[productionUpgradeTier];
            if (!economy.TrySpendPlayer(cost))
            {
                failReason = UpgradePurchaseFailReason.InsufficientCredits;
                return false;
            }

            productionUpgradeTier++;
            ProceduralAudioUtility.PlayUiConfirm();
            return true;
        }

        /// <summary>코어 최대 체력 증가 (플레이어 전용)</summary>
        public bool TryPurchaseHullUpgrade()
        {
            return TryPurchaseHullUpgrade(out _);
        }

        public bool TryPurchaseHullUpgrade(out UpgradePurchaseFailReason failReason)
        {
            failReason = UpgradePurchaseFailReason.None;
            if (team != UnitTeam.Player || economy == null)
            {
                failReason = UpgradePurchaseFailReason.NotApplicable;
                return false;
            }

            if (TryGetPlayerUpgradeBlockedReason(out UpgradePurchaseFailReason blocked))
            {
                failReason = blocked;
                return false;
            }

            if (health == null)
            {
                failReason = UpgradePurchaseFailReason.InvalidState;
                return false;
            }

            if (hullUpgradeTier >= 3)
            {
                failReason = UpgradePurchaseFailReason.MaxTier;
                return false;
            }

            int cost = HullUpgradeCosts[hullUpgradeTier];
            if (!economy.TrySpendPlayer(cost))
            {
                failReason = UpgradePurchaseFailReason.InsufficientCredits;
                return false;
            }

            health.AddMaxHealthBonus(480f);
            hullUpgradeTier++;
            ProceduralAudioUtility.PlayUiConfirm();
            return true;
        }

        /// <summary>자동 자원 증가 (플레이어 전용)</summary>
        public bool TryPurchaseIncomeUpgrade()
        {
            return TryPurchaseIncomeUpgrade(out _);
        }

        public bool TryPurchaseIncomeUpgrade(out UpgradePurchaseFailReason failReason)
        {
            failReason = UpgradePurchaseFailReason.None;
            if (team != UnitTeam.Player || economy == null)
            {
                failReason = UpgradePurchaseFailReason.NotApplicable;
                return false;
            }

            if (TryGetPlayerUpgradeBlockedReason(out UpgradePurchaseFailReason blocked))
            {
                failReason = blocked;
                return false;
            }

            if (incomeUpgradeTier >= 3)
            {
                failReason = UpgradePurchaseFailReason.MaxTier;
                return false;
            }

            int cost = IncomeUpgradeCosts[incomeUpgradeTier];
            if (!economy.TrySpendPlayer(cost))
            {
                failReason = UpgradePurchaseFailReason.InsufficientCredits;
                return false;
            }

            economy.AddPlayerIncomePerSecond(1.32f);
            incomeUpgradeTier++;
            ProceduralAudioUtility.PlayUiConfirm();
            return true;
        }

        /// <summary>슬롯 0~7 — 플레이어는 키보드, 적은 EnemyBrain에서 호출</summary>
        public bool TryEnqueueDeckSlot(int slotIndex)
        {
            return TryEnqueueDeckSlot(slotIndex, out _);
        }

        /// <summary>실패 시 <paramref name="failReason"/> 로 구분 — 플레이어 HUD 안내용</summary>
        public bool TryEnqueueDeckSlot(int slotIndex, out DeckEnqueueFailReason failReason)
        {
            failReason = DeckEnqueueFailReason.None;

            if (health == null || !health.IsAlive || slotIndex < 0 || slotIndex > 7)
            {
                failReason = DeckEnqueueFailReason.InvalidOrDead;
                return false;
            }

            if (team == UnitTeam.Player)
            {
                if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
                {
                    failReason = DeckEnqueueFailReason.BriefingBlocking;
                    return false;
                }

                if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController matchForEnqueue) &&
                    matchForEnqueue.IsFinished)
                {
                    failReason = DeckEnqueueFailReason.MatchFinished;
                    return false;
                }
            }

            if (productionQueue.Count >= MaxQueueLength)
            {
                failReason = DeckEnqueueFailReason.QueueFull;
                return false;
            }

            if (CountTeamUnitsOnField() >= MaxUnitsOnFieldPerTeam)
            {
                failReason = DeckEnqueueFailReason.FieldCap;
                return false;
            }

            UnitArchetype archetype = deckSlots[slotIndex];
            UnitDefinition definition = database != null ? database.GetDefinition(archetype) : null;
            if (definition == null)
            {
                failReason = DeckEnqueueFailReason.NoDefinition;
                return false;
            }

            int cost = BattleAcesEconomy.GetTrainCost(definition);
            bool paid = team == UnitTeam.Player
                ? economy.TrySpendPlayer(cost)
                : economy.TrySpendEnemy(cost);

            if (!paid)
            {
                failReason = DeckEnqueueFailReason.InsufficientCredits;
                return false;
            }

            productionQueue.Enqueue(archetype);

            if (!isProducing)
            {
                StartNextProduction();
            }

            return true;
        }

        /// <summary>생산 거절 HUD 한 줄 — null 이면 표시 생략</summary>
        public static string GetDeckEnqueueFailHintKo(DeckEnqueueFailReason reason)
        {
            return reason switch
            {
                DeckEnqueueFailReason.BriefingBlocking => "생산 불가: 브리핑 중 (작전 시작 후)",
                DeckEnqueueFailReason.MatchFinished => "생산 불가: 전투 종료",
                DeckEnqueueFailReason.QueueFull => "생산 불가: 큐 가득 참",
                DeckEnqueueFailReason.FieldCap => "생산 불가: 전장 유닛 상한",
                DeckEnqueueFailReason.NoDefinition => "생산 불가: 덱 슬롯 없음",
                DeckEnqueueFailReason.InsufficientCredits => "생산 불가: 자원 부족",
                _ => null
            };
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
                // 업그레이드 티어당 12% 생산 시간 감소 (3티어 시 최대 약 36%)
                float mul = Mathf.Max(0.52f, 1f - 0.12f * productionUpgradeTier);
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
                    else
                    {
                        // 적은 바로 적 코어로 달리지 않고 집결 지점으로 모인 뒤, EnemyBrain 이 물결 공격 명령
                        unit.MoveTo(rallyWorldPosition);
                        ProceduralAudioUtility.PlayEnemyProductionComplete();
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
