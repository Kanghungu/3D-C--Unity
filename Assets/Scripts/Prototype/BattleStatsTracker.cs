using System.Collections.Generic;
using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// 전투 통계를 실시간으로 수집한다.
    /// PrototypeBootstrapper가 생성하며, Tab HUD 패널에서 조회한다.
    /// </summary>
    public class BattleStatsTracker : MonoBehaviour
    {
        public static BattleStatsTracker Instance { get; private set; }

        private readonly Dictionary<UnitArchetype, int> playerKills = new();
        private readonly Dictionary<UnitArchetype, int> enemyKills  = new();
        private float matchStartTime;

        public float ElapsedSeconds => Time.time - matchStartTime;
        public IReadOnlyDictionary<UnitArchetype, int> PlayerKills => playerKills;
        public IReadOnlyDictionary<UnitArchetype, int> EnemyKills  => enemyKills;

        public int TotalPlayerKills
        {
            get
            {
                int total = 0;
                foreach (int v in playerKills.Values) total += v;
                return total;
            }
        }

        public int TotalEnemyKills
        {
            get
            {
                int total = 0;
                foreach (int v in enemyKills.Values) total += v;
                return total;
            }
        }

        private void Awake()
        {
            Instance = this;
            matchStartTime = Time.time;
        }

        private void OnEnable()
        {
            UnitHealth.OnUnitDied += HandleUnitDied;
        }

        private void OnDisable()
        {
            UnitHealth.OnUnitDied -= HandleUnitDied;
        }

        private void HandleUnitDied(UnitTeam team, UnitArchetype archetype)
        {
            // 사망한 팀의 반대편이 킬을 획득
            if (team == UnitTeam.Player)
            {
                enemyKills.TryGetValue(archetype, out int prev);
                enemyKills[archetype] = prev + 1;
            }
            else
            {
                playerKills.TryGetValue(archetype, out int prev);
                playerKills[archetype] = prev + 1;
            }
        }

        public void Reset()
        {
            playerKills.Clear();
            enemyKills.Clear();
            matchStartTime = Time.time;
        }
    }
}
