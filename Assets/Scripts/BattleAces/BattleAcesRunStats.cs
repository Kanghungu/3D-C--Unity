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
        private float battleClockStartUnscaled = -1f;
        private float frozenPlaySecondsUnscaled = -1f;

        private BattleAcesMatchController subscribedMatch;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match))
            {
                subscribedMatch = match;
                match.MatchEnded += OnBattleMatchEnded;
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

        private void OnBattleMatchEnded(BattleAcesMatchController.MatchState state)
        {
            frozenPlaySecondsUnscaled = battleClockStartUnscaled >= 0f
                ? Mathf.Max(0f, Time.unscaledTime - battleClockStartUnscaled)
                : 0f;
        }

        public void MarkBattleClockStart()
        {
            if (battleClockStartUnscaled < 0f)
            {
                battleClockStartUnscaled = Time.unscaledTime;
            }
        }

        public float GetFrozenPlaySecondsUnscaled()
        {
            return frozenPlaySecondsUnscaled >= 0f ? frozenPlaySecondsUnscaled : 0f;
        }

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

        public string BuildFullResultSummaryLine(float playSecondsUnscaled, int endCredits)
        {
            string timeLabel = FormatPlayTimeMmSs(playSecondsUnscaled);
            string head = $"플레이 {timeLabel} · 종료 자원 {endCredits}";
            if (TryGetMostProducedArchetype(out UnitArchetype archetype, out int count) && count > 0)
            {
                return $"{head} · 생산 최다 {FormatArchetypeShortKo(archetype)} x{count} · 적 격파 {enemyUnitsKilled} · 아군 손실 {playerUnitsLost}";
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
                UnitArchetype.Artillery => "포병",
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

        public void RegisterPlayerUnitProduced(UnitArchetype archetype)
        {
            playerProductionCount.TryGetValue(archetype, out int count);
            playerProductionCount[archetype] = count + 1;
        }

        public int CountDistinctPlayerProductionArchetypes()
        {
            int count = 0;
            foreach (KeyValuePair<UnitArchetype, int> entry in playerProductionCount)
            {
                if (entry.Value > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetTotalPlayerUnitsProduced()
        {
            int sum = 0;
            foreach (KeyValuePair<UnitArchetype, int> entry in playerProductionCount)
            {
                if (entry.Value > 0)
                {
                    sum += entry.Value;
                }
            }

            return sum;
        }

        public bool TryGetMostProducedArchetype(out UnitArchetype archetype, out int count)
        {
            archetype = UnitArchetype.Spearman;
            count = 0;
            foreach (KeyValuePair<UnitArchetype, int> entry in playerProductionCount)
            {
                if (entry.Value > count)
                {
                    count = entry.Value;
                    archetype = entry.Key;
                }
            }

            return count > 0;
        }

        public int TotalEnemyUnitsKilled => enemyUnitsKilled;
        public int TotalPlayerUnitsLost => playerUnitsLost;
    }
}
