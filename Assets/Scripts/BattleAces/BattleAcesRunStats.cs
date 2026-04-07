using System.Collections.Generic;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces 한 판 통계 — 생산·격파·손실(결과 카드 한 줄용).
    /// </summary>
    public sealed class BattleAcesRunStats : MonoBehaviour
    {
        public static BattleAcesRunStats Instance { get; private set; }

        private readonly Dictionary<UnitArchetype, int> playerProductionCount = new();

        private int enemyUnitsKilled;
        private int playerUnitsLost;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
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
