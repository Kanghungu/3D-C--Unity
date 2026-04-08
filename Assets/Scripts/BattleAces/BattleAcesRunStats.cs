using System.Collections.Generic;
using System.Globalization;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces 한 판 통계 — 생산·격파·손실·플레이 시간(승패 카드 한 줄용).
    /// </summary>
    public sealed class BattleAcesRunStats : MonoBehaviour
    {
        public static BattleAcesRunStats Instance { get; private set; }

        private readonly Dictionary<UnitArchetype, int> playerProductionCount = new();

        private int enemyUnitsKilled;
        private int playerUnitsLost;

        /// <summary>작전 시작(브리핑 종료 후) 시각 — unscaled</summary>
        private float battleClockStartUnscaled = -1f;

        /// <summary>매치 종료 시점에 고정한 플레이 초(승패 화면·스커미시 오버레이 공통)</summary>
        private float frozenPlaySecondsUnscaled = -1f;

        private void Awake()
        {
            Instance = this;
        }

        private BattleAcesMatchController subscribedMatch;

        private void Start()
        {
            // 부트 순서상 MatchController 가 이미 있을 때 구독
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

        /// <summary>BattleMissionFlow 가 작전 시작할 때 호출</summary>
        public void MarkBattleClockStart()
        {
            if (battleClockStartUnscaled < 0f)
            {
                battleClockStartUnscaled = Time.unscaledTime;
            }
        }

        /// <summary>종료 후 저장된 길이 — 없으면 0</summary>
        public float GetFrozenPlaySecondsUnscaled()
        {
            return frozenPlaySecondsUnscaled >= 0f ? frozenPlaySecondsUnscaled : 0f;
        }

        /// <summary>분:초 — 결과 카드 한 줄용</summary>
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

        /// <summary>플레이 시간·자원·생산·격파·손실을 한 줄로(데모 승패 인상용)</summary>
        public string BuildFullResultSummaryLine(float playSecondsUnscaled, int endCredits)
        {
            string t = FormatPlayTimeMmSs(playSecondsUnscaled);
            string head = $"플레이 {t} · 종료 시 자원 {endCredits}";
            if (TryGetMostProducedArchetype(out UnitArchetype arch, out int n) && n > 0)
            {
                return $"{head} · 생산 최다 {FormatArchetypeShortKo(arch)}×{n} · 적 격파 {enemyUnitsKilled} · 아군 손실 {playerUnitsLost}";
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
                UnitArchetype.Outrider => "기동",
                UnitArchetype.MobileFortress => "요새",
                UnitArchetype.AirborneCitadel => "공성",
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

        /// <summary>아군 코어에서 생산 완료 시 호출</summary>
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

        /// <summary>생산 완료 누계 — 첫 미션 온보딩 체크용</summary>
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

