using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.Prototype
{
    public enum MatchResult
    {
        Ongoing,
        Victory,
        Defeat
    }

    /// <summary>
    /// Resolves win and loss conditions for the prototype battle.
    /// </summary>
    public class PrototypeMatchController : MonoBehaviour
    {
        [SerializeField] private float evaluationInterval = 0.25f;

        private MatchResult result = MatchResult.Ongoing;
        private float evaluationTimer;

        public MatchResult Result => result;
        public bool IsFinished => result != MatchResult.Ongoing;

        private void Update()
        {
            if (!IsFinished)
            {
                evaluationTimer -= Time.deltaTime;

                if (evaluationTimer <= 0f)
                {
                    evaluationTimer = evaluationInterval;
                    EvaluateMatch();
                }
            }

            if (IsFinished && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                Time.timeScale = 1f;
                SceneManager.LoadScene(activeScene.path);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        private void EvaluateMatch()
        {
            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            int playerUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Player);
            int enemyUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Enemy);

            bool playerBaseDestroyed = playerBase == null || !playerBase.IsAlive;
            bool enemyBaseDestroyed = enemyBase == null || !enemyBase.IsAlive;
            bool playerArmyDestroyed = playerUnits == 0;
            bool enemyArmyDestroyed = enemyUnits == 0;

            bool playerLost = playerBaseDestroyed || playerArmyDestroyed;
            bool enemyLost = enemyBaseDestroyed || enemyArmyDestroyed;

            if (enemyLost)
            {
                result = MatchResult.Victory;
                Time.timeScale = 0f;
            }
            else if (playerLost)
            {
                result = MatchResult.Defeat;
                Time.timeScale = 0f;
            }
        }
    }
}
