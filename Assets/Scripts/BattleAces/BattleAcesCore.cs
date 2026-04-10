using Game.Audio;
using Game.Prototype;
using Game.Settings;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>???앹궛 ???ㅽ뙣 ?먯씤 ??HUD ??以??덈궡??/summary>
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

    /// <summary>T/Y/U 肄붿뼱 媛뺥솕 ?ㅽ뙣 ???앹궛 嫄곗젅怨??숈씪 HUD 留됰?</summary>
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
    /// ?⑥씪 肄붿뼱: ??8?щ’ 以??좏깮???좊떅留??앹궛 ?먯뿉 ?ｋ뒗?? 利앹썝? ??嫄대Ъ ?섎굹?먯꽌留?
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

        /// <summary>?붾뱶 ?좊━ 吏?????앹궛 ?꾨즺 ?좊떅??癒쇱? ?ν븿(?뚮젅?댁뼱??Alt+?고겢由?쑝濡??ㅼ젙)</summary>
        private GameObject rallyWorldPing;
        private UnitHealth health;
        private float productionTimeRemaining;
        private bool isProducing;
        private UnitArchetype currentProductionArchetype;

        // ?뚮젅?댁뼱 肄붿뼱留??ъ슜 ???앹궛 媛???κ컩/?먯썝 (?곗뼱 0~3, 理쒕? 3??媛뺥솕)
        private int productionUpgradeTier;
        private int hullUpgradeTier;
        private int incomeUpgradeTier;

        /// <summary>?⑹뀡 洹쒖튃 ???앹궛 ?쒓컙??怨깊븿(1 誘몃쭔?대㈃ 鍮좊Ⅸ ?앹궛)</summary>
        private float factionProductionDurationMultiplier = 1f;

        /// <summary>?굿룰컯??嫄곗젅 ?뚰듃 ?ㅽ뙵 諛⑹?(媛숈? 荑⑤떎??怨듭쑀)</summary>
        private static float lastPlayerEconomyRejectUnscaled = -999f;
        private const float PlayerEconomyRejectCooldownSeconds = 0.38f;

        /// <summary>T/Y/U 嫄곗젅 ??以꾩뿉 遺숇뒗 吏㏃? 瑗щ━???쑣룹쁺)</summary>
        private enum UpgradeHotkeySlot
        {
            Production,
            Hull,
            Income
        }

        // 梨뺥꽣3: ?????덉뿉??1~2?④퀎??臾대궃?? 3?④퀎???먯꽭 ?곕씪 ?ъ꽦(?댁쟾蹂대떎 ?쎄컙 ??는룻슚怨쇄넁)
        // 1?곗뼱 鍮꾩슜 媛꾧꺽?쇰줈 珥덈컲 2~3遺??덉뿉 T(?앹궛)쨌Y(?좎껜)쨌U(?섏엯) 以?臾댁뾿??癒쇱? ?댁? 媛덈━寃???
        // ????1~2?곗뼱 紐⑺몴??留욎텣 ?쎄컙 ?꾪솕(諛섎났 ?뚮젅???섏씠??
        private static readonly int[] ProductionUpgradeCosts = { 52, 77, 112 };
        private static readonly int[] HullUpgradeCosts = { 40, 71, 102 };
        private static readonly int[] IncomeUpgradeCosts = { 60, 86, 116 };

        public UnitTeam Team => team;
        public UnitHealth Health => health;
        public int QueueCount => productionQueue.Count + (isProducing ? 1 : 0);

        /// <summary>?꾩옱 議곗꽑 以묒씤 ?좊떅???쒖쇅???湲곗뿴 湲몄씠(HUD??</summary>
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
        /// HUD?????꾩옱 ?앹궛 以묒씤 ?좊떅쨌?⑥? ?쒓컙, ?먮뒗 ?湲곗뿴 留???議곗꽑 ?쒖옉 ?? 誘몃━蹂닿린.
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

        /// <summary>?꾩옱 ?앹궛 ?좊━(?붾뱶 醫뚰몴)</summary>
        public Vector3 RallyWorldPosition => rallyWorldPosition;

        /// <summary>?뚮젅?댁뼱 ?꾩슜 ??吏硫??고겢由?Alt)?쇰줈 吏묎껐 吏???ㅼ젙</summary>
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

        /// <summary>??珥덇린?????⑹뀡???곕씪 ?몄텧</summary>
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

        /// <summary>?ъ뿉 ?덈뒗 ?댁븘 ?덈뒗 Battle Aces 肄붿뼱 寃????RTS 移대찓??Home/Space ??</summary>
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

        /// <summary>吏묎껐 ?꾩튂 ?쒓컖 ?쒖떆(?뚮젅?댁뼱留?</summary>
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
                // ?꾪듃 諛⑺뼢: ?섏떇 ?곗뼹 ?ъ씤?몃쭔 梨꾨룄 ?덇쾶
                rend.material.color = BattleAcesArtDirection.PointTeal;
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

                // 釉뚮━?뫢룹듅???뺤젙 ?꾩뿉???굿룹뾽洹몃젅?대뱶 ?낅젰 臾댁떆(?먯썝 ?뚮え쨌嫄곗젅??諛⑹?)
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
                            string hint = GetDeckEnqueueFailHint(failReason);
                            if (!string.IsNullOrEmpty(hint))
                            {
                                ProceduralAudioUtility.PlayUiCommandRejected();
                                BattleAcesHudOverlay.PulseEconomyRejectHint(hint);
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
                TryHotkeyUpgrade(TryPurchaseProductionUpgrade, UpgradeHotkeySlot.Production);
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                TryHotkeyUpgrade(TryPurchaseHullUpgrade, UpgradeHotkeySlot.Hull);
            }

            if (keyboard.uKey.wasPressedThisFrame)
            {
                TryHotkeyUpgrade(TryPurchaseIncomeUpgrade, UpgradeHotkeySlot.Income);
            }
        }

        private delegate bool TryUpgradeOutDelegate(out UpgradePurchaseFailReason failReason);

        private void TryHotkeyUpgrade(TryUpgradeOutDelegate tryPurchase, UpgradeHotkeySlot hotkeySlot)
        {
            // ?앹궛 ?ㅼ? ?숈씪: ?쒕룄????긽 ?섍퀬, 嫄곗젅 ?쇰뱶諛깅쭔 荑⑤떎??荑⑤떎??以묒뿉??援щℓ ?깃났? 利됱떆 諛섏쓳)
            if (tryPurchase(out UpgradePurchaseFailReason fail))
            {
                return;
            }

            if (Time.unscaledTime - lastPlayerEconomyRejectUnscaled < PlayerEconomyRejectCooldownSeconds)
            {
                return;
            }

            lastPlayerEconomyRejectUnscaled = Time.unscaledTime;
            string hint = GetUpgradePurchaseFailHint(fail, hotkeySlot);
            if (!string.IsNullOrEmpty(hint))
            {
                ProceduralAudioUtility.PlayUiCommandRejected();
                BattleAcesHudOverlay.PulseEconomyRejectHint(hint);
            }
            else if (fail != UpgradePurchaseFailReason.NotApplicable)
            {
                ProceduralAudioUtility.PlayUiCommandRejected();
            }
        }

        private static string UpgradeHotkeyTail(UpgradeHotkeySlot slot)
        {
            bool ko = GameUserSettings.Language == GameLanguage.Korean;
            return slot switch
            {
                UpgradeHotkeySlot.Production => ko ? "?앹궛 T" : "production T",
                UpgradeHotkeySlot.Hull => ko ? "?κ컩 Y" : "armor Y",
                UpgradeHotkeySlot.Income => ko ? "?섏엯 U" : "income U",
                _ => string.Empty
            };
        }

        private static string GetUpgradePurchaseFailHint(UpgradePurchaseFailReason reason, UpgradeHotkeySlot slot)
        {
            string tail = UpgradeHotkeyTail(slot);
            bool ko = GameUserSettings.Language == GameLanguage.Korean;
            return reason switch
            {
                UpgradePurchaseFailReason.BriefingBlocking => ko
                    ? $"媛뺥솕 遺덇?: 釉뚮━??以?({tail})"
                    : $"Upgrade blocked: briefing ({tail})",
                UpgradePurchaseFailReason.MatchFinished => ko
                    ? $"媛뺥솕 遺덇?: ?꾪닾 醫낅즺 ({tail})"
                    : $"Upgrade blocked: battle over ({tail})",
                UpgradePurchaseFailReason.InsufficientCredits => ko
                    ? $"媛뺥솕 遺덇?: ?먯썝 遺議?({tail})"
                    : $"Upgrade blocked: not enough credits ({tail})",
                UpgradePurchaseFailReason.MaxTier => ko
                    ? $"媛뺥솕 遺덇?: {tail} 留뚮졊"
                    : $"Upgrade blocked: {tail} at max",
                UpgradePurchaseFailReason.InvalidState => ko
                    ? $"媛뺥솕 遺덇?: 肄붿뼱 ?곹깭 ?뺤씤 ({tail})"
                    : $"Upgrade blocked: core state ({tail})",
                _ => null
            };
        }

        /// <summary>?뚮젅?댁뼱 媛뺥솕 怨듯넻 寃뚯씠?????앹궛 ?먯? ?숈씪 議곌굔</summary>
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

        /// <summary>?앹궛 ?쒓컙 ?⑥텞 (?뚮젅?댁뼱 ?꾩슜)</summary>
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
            ProceduralAudioUtility.PlayCoreUpgradeApplied();
            return true;
        }

        /// <summary>肄붿뼱 理쒕? 泥대젰 利앷? (?뚮젅?댁뼱 ?꾩슜)</summary>
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
            ProceduralAudioUtility.PlayCoreUpgradeApplied();
            return true;
        }

        /// <summary>?먮룞 ?먯썝 利앷? (?뚮젅?댁뼱 ?꾩슜)</summary>
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
            ProceduralAudioUtility.PlayCoreUpgradeApplied();
            return true;
        }

        /// <summary>?щ’ 0~7 ???뚮젅?댁뼱???ㅻ낫?? ?곸? EnemyBrain?먯꽌 ?몄텧</summary>
        public bool TryEnqueueDeckSlot(int slotIndex)
        {
            return TryEnqueueDeckSlot(slotIndex, out _);
        }

        /// <summary>?ㅽ뙣 ??<paramref name="failReason"/> 濡?援щ텇 ???뚮젅?댁뼱 HUD ?덈궡??/summary>
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

            if (team == UnitTeam.Player)
            {
                ProceduralAudioUtility.PlayDeckOrderQueued();
                BattleAcesHudOverlay.PulseDeckOrderSuccessHint(archetype);
            }

            if (!isProducing)
            {
                StartNextProduction();
            }

            return true;
        }

        /// <summary>?앹궛 嫄곗젅 HUD ??以???null ?대㈃ ?쒖떆 ?앸왂(?몄뼱 ?ㅼ젙 諛섏쁺)</summary>
        private static string GetDeckEnqueueFailHint(DeckEnqueueFailReason reason)
        {
            bool ko = GameUserSettings.Language == GameLanguage.Korean;
            return reason switch
            {
                DeckEnqueueFailReason.BriefingBlocking => ko
                    ? "?앹궛 遺덇?: 釉뚮━??以?(?묒쟾 ?쒖옉 ??"
                    : "Build blocked: briefing (start the operation first)",
                DeckEnqueueFailReason.MatchFinished => ko
                    ? "?앹궛 遺덇?: ?꾪닾 醫낅즺"
                    : "Build blocked: battle ended",
                DeckEnqueueFailReason.QueueFull => ko
                    ? "?? ??: ??? ?? ?"
                    : "Build blocked: queue full",
                DeckEnqueueFailReason.FieldCap => ko
                    ? "?앹궛 遺덇?: ?꾩옣 ?좊떅 ?곹븳"
                    : "Build blocked: unit cap on field",
                DeckEnqueueFailReason.NoDefinition => ko
                    ? "?앹궛 遺덇?: ???щ’ ?놁쓬"
                    : "Build blocked: empty deck slot",
                DeckEnqueueFailReason.InsufficientCredits => ko
                    ? "?? ??: ?? ??"
                    : "Build blocked: not enough credits",
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
                // ?낃렇?덉씠???곗뼱??12% ?앹궛 ?쒓컙 媛먯냼 (3?곗뼱 ??理쒕? ??36%)
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
                        SpawnPlayerProductionReadyRing(spawnPos);
                    }
                    else
                    {
                        // ?곸? 諛붾줈 ??肄붿뼱濡??щ━吏 ?딄퀬 吏묎껐 吏?먯쑝濡?紐⑥씤 ?? EnemyBrain ??臾쇨껐 怨듦꺽 紐낅졊
                        unit.MoveTo(rallyWorldPosition);
                        ProceduralAudioUtility.PlayEnemyProductionComplete();
                    }
                }
            }

            isProducing = false;
            productionTimeRemaining = 0f;
        }

        /// <summary>?앹궛 ?꾨즺 ?쒓컙 ??吏㏃? 留??뺤옣(?꾨줈?쒖????ㅺ낵 吏?</summary>
        private static void SpawnPlayerProductionReadyRing(Vector3 groundPosition)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "BA_ProdReadyFX";
            go.transform.position = groundPosition + new Vector3(0f, 0.07f, 0f);
            go.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
            if (go.TryGetComponent(out Collider col))
            {
                col.enabled = false;
            }

            if (go.TryGetComponent(out Renderer rend))
            {
                Color c = BattleAcesArtDirection.PointTeal;
                c.a = 0.92f;
                rend.material.color = c;
            }

            TimedWorldEffect tw = go.AddComponent<TimedWorldEffect>();
            tw.Configure(0.24f, new Vector3(2.35f, 0.018f, 2.35f), Vector3.zero);
        }

        private Vector3 GetSpawnPositionNearCore()
        {
            const float ringRadius = 8.5f;
            const float minSeparation = 2.6f;
            Vector3 best = transform.position + new Vector3(0f, 1f, ringRadius * 0.35f);
            for (int attempt = 0; attempt < 14; attempt++)
            {
                Vector2 ring = UnityEngine.Random.insideUnitCircle * ringRadius;
                Vector3 candidate = transform.position + new Vector3(ring.x, 1f, ring.y);
                if (!IsTooCloseToFriendlySpawn(candidate, minSeparation))
                {
                    return candidate;
                }

                if (attempt > 6)
                {
                    float a = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
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
