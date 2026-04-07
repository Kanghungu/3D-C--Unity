using System;
using Game.Audio;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// 매치 승패. EnemyCoreDestroyed 모드: 적 코어 파괴 시 승리. MissionCustom: DeclarePlayerVictory 로만 승리.
    /// 패배는 아군 코어 파괴 또는 미션에서 DeclarePlayerDefeat 호출. R 재시작.
    /// </summary>
    public class BattleAcesMatchController : MonoBehaviour
    {
        public enum MatchState
        {
            Playing,
            Victory,
            Defeat
        }

        /// <summary>승패 화면 한 줄 요약(Battle Aces 전용).</summary>
        public enum MatchEndReason
        {
            None,
            VictoryEnemyCoreDestroyed,
            VictoryMissionObjective,
            DefeatPlayerCoreDestroyed,
            DefeatRelicOrKeyObjectiveLost,
            DefeatMissionFailed
        }

        /// <summary>적 코어 파괴로 승리 vs 미션 전용 승리 조건</summary>
        public enum VictoryMode
        {
            EnemyCoreDestroyed,
            MissionCustom
        }

        [SerializeField] private BattleAcesCore playerCore;
        [SerializeField] private BattleAcesCore enemyCore;

        [SerializeField] private VictoryMode victoryMode = VictoryMode.EnemyCoreDestroyed;

        private MatchState state = MatchState.Playing;
        private MatchEndReason lastEndReason = MatchEndReason.None;

        /// <summary>적 코어 임박 승리 톤 1회</summary>
        private bool victoryImminentChimePlayed;

        public MatchState State => state;
        public bool IsFinished => state != MatchState.Playing;
        public VictoryMode WinMode => victoryMode;
        public MatchEndReason LastEndReason => lastEndReason;

        public BattleAcesCore PlayerCore => playerCore;
        public BattleAcesCore EnemyCore => enemyCore;

        public event Action<MatchState> MatchEnded;

        // -------------------------------------------------------------------------
        // 씬당 BA_Systems 에 하나. 씬 전환·언로드 후 Instance 는 null — 직접 Instance 접근 전 null 검사 또는 TryGetInstance 사용.
        // -------------------------------------------------------------------------
        public static BattleAcesMatchController Instance { get; private set; }

        public static bool TryGetInstance(out BattleAcesMatchController controller)
        {
            controller = Instance;
            return controller != null;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Time.timeScale = 1f;
        }

        public void BindCores(BattleAcesCore player, BattleAcesCore enemy)
        {
            playerCore = player;
            enemyCore = enemy;
        }

        public void SetVictoryMode(VictoryMode mode)
        {
            victoryMode = mode;
        }

        public void DeclarePlayerVictory()
        {
            if (state != MatchState.Playing)
            {
                return;
            }

            lastEndReason = MatchEndReason.VictoryMissionObjective;
            state = MatchState.Victory;
            Time.timeScale = 0f;
            MatchEnded?.Invoke(state);
        }

        public void DeclarePlayerDefeat(MatchEndReason reason = MatchEndReason.DefeatPlayerCoreDestroyed)
        {
            if (state != MatchState.Playing)
            {
                return;
            }

            lastEndReason = reason;
            state = MatchState.Defeat;
            Time.timeScale = 0f;
            MatchEnded?.Invoke(state);
        }

        private void Update()
        {
            if (state != MatchState.Playing)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().path);
                }

                return;
            }

            if (playerCore == null || enemyCore == null)
            {
                return;
            }

            UnitHealth playerHealth = playerCore.Health;
            UnitHealth enemyHealth = enemyCore.Health;

            if (playerHealth == null || enemyHealth == null)
            {
                return;
            }

            if (victoryMode == VictoryMode.EnemyCoreDestroyed)
            {
                if (!victoryImminentChimePlayed && enemyHealth.IsAlive && enemyHealth.Normalized <= 0.22f)
                {
                    victoryImminentChimePlayed = true;
                    ProceduralAudioUtility.PlayVictoryImminentChime();
                }

                if (!enemyHealth.IsAlive)
                {
                    lastEndReason = MatchEndReason.VictoryEnemyCoreDestroyed;
                    state = MatchState.Victory;
                    Time.timeScale = 0f;
                    MatchEnded?.Invoke(state);
                    return;
                }
            }

            if (!playerHealth.IsAlive)
            {
                lastEndReason = MatchEndReason.DefeatPlayerCoreDestroyed;
                state = MatchState.Defeat;
                Time.timeScale = 0f;
                MatchEnded?.Invoke(state);
            }
        }

    }
}
