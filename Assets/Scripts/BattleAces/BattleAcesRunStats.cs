using System.Collections.Generic;
using System.Globalization;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Collects simple Battle Aces match statistics for result cards and first-play guidance.
    /// </summary>
    public sealed class BattleAcesRunStats : MonoBehaviour
    {
        public static BattleAcesRunStats Instance { get; private set; }

        private readonly Dictionary<UnitArchetype, int> playerProductionCount = new();

        private int enemyUnitsKilled;
        private int playerUnitsLost;

        /// <summary>Unscaled time when the playable battle actually begins.</summary>
        private float battleClockStartUnscaled = -1f;

        /// <summary>Frozen play time captured when the match ends.</summary>
        private float frozenPlaySecondsUnscaled = -1f;

        private BattleAcesMatchController subscribedMatch;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // MatchController may already exist by the time this component starts.
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m))
            {
                subscribedMatch = m;
                m.MatchEnded += OnBattleMatchEnded;
            }
        }

        private void OnDestroy()
        {
            if (subscribedMatch != null)
            {
                subscribedMatch.MatchEnded -= OnBattleMatchEnded;
                subscribedMatch = null;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
            UnitHealth.OnUnitDied += OnUnitDied;
        }

        private void OnDisable()
        {
            UnitHealth.OnUnitDied -= OnUnitDied;
        }

        private void OnBattleMatchEnded(BattleAcesMatchController.MatchState st)
        {
            frozenPlaySecondsUnscaled = battleClockStartUnscaled >= 0f
                ? Mathf.Max(0f, Time.unscaledTime - battleClockStartUnscaled)
                : 0f;
        }

        /// <summary>Called when gameplay control begins after briefing or intro flow.</summary>
        public void MarkBattleClockStart()
        {
            if (battleClockStartUnscaled < 0f)
            {
                battleClockStartUnscaled = Time.unscaledTime;
            }
        }

        /// <summary>Returns the frozen end-of-match play length, or 0 when unavailable.</summary>
        public float GetFrozenPlaySecondsUnscaled()
        {
            return frozenPlaySecondsUnscaled >= 0f ? frozenPlaySecondsUnscaled : 0f;
        }

        /// <summary>Formats a result-card time string as mm:ss.</summary>
        public static string FormatPlayTimeMmSs(float secondsUnscaled)
        {
            if (secondsUnscaled < 0f)
            {
                secondsUnscaled = 0f;
            }

            int total = Mathf.FloorToInt(secondsUnscaled);
            int mm = total / 60;
            int ss = total % 60;
            return mm.ToString(CultureInfo.InvariantCulture) + ":" + ss.ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>Builds a short Korean summary used by result cards and dev notes.</summary>
        public string BuildFullResultSummaryLine(float playSecondsUnscaled, int endCredits)
        {
            string t = FormatPlayTimeMmSs(playSecondsUnscaled);
            string head = $"플레이 {t} · 종료 자원 {endCredits}";
            if (TryGetMostProducedArchetype(out UnitArchetype arch, out int n) && n > 0)
            {
                return $"{head} · 생산 최다 {FormatArchetypeShortKo(arch)} x{n} · 적 격파 {enemyUnitsKilled} · 아군 손실 {playerUnitsLost}";
            }

            return $"{head} · 적 격파 {enemyUnitsKilled} · 아군 손실 {playerUnitsLost}";
        }

        private static string FormatArchetypeShortKo(UnitArchetype archetype)
        {
            return archetype switch
            {
                UnitArchetype.Spearman => "창",
                UnitArchetype.ShieldInfantry => "방패",
                UnitArchetype.Rifleman => "소총",
                UnitArchetype.Artillery => "포",
                UnitArchetype.Fighter => "전투기",
                UnitArchetype.SpecialWarrior => "특전",
                UnitArchetype.RoyalGuard => "근위",
                UnitArchetype.Outrider => "기수",
                UnitArchetype.MobileFortress => "요새",
                UnitArchetype.AirborneCitadel => "성채",
                _ => archetype.ToString()
            };
        }

        private void OnUnitDied(UnitTeam deadTeam, UnitArchetype archetype)
        {
            if (deadTeam == UnitTeam.Player)
            {
                playerUnitsLost++;
            }
            else
            {
                enemyUnitsKilled++;
            }
        }

        /// <summary>Called when the player core finishes producing a unit.</summary>
        public void RegisterPlayerUnitProduced(UnitArchetype archetype)
        {
            playerProductionCount.TryGetValue(archetype, out int n);
            playerProductionCount[archetype] = n + 1;
        }

        public int CountDistinctPlayerProductionArchetypes()
        {
            int c = 0;
            foreach (KeyValuePair<UnitArchetype, int> kv in playerProductionCount)
            {
                if (kv.Value > 0)
                {
                    c++;
                }
            }

            return c;
        }

        /// <summary>Total number of units produced by the player during the match.</summary>
        public int GetTotalPlayerUnitsProduced()
        {
            int sum = 0;
            foreach (KeyValuePair<UnitArchetype, int> kv in playerProductionCount)
            {
                if (kv.Value > 0)
                {
                    sum += kv.Value;
                }
            }

            return sum;
        }

        public bool TryGetMostProducedArchetype(out UnitArchetype archetype, out int count)
        {
            archetype = UnitArchetype.Spearman;
            count = 0;
            foreach (KeyValuePair<UnitArchetype, int> kv in playerProductionCount)
            {
                if (kv.Value > count)
                {
                    count = kv.Value;
                    archetype = kv.Key;
                }
            }

            return count > 0;
        }

        public int TotalEnemyUnitsKilled => enemyUnitsKilled;
        public int TotalPlayerUnitsLost => playerUnitsLost;
    }
}
