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
        private MatchResult result = MatchResult.Ongoing;

        public MatchResult Result => result;
        public bool IsFinished => result != MatchResult.Ongoing;

        private void Update()
        {
            if (!IsFinished)
            {
                EvaluateMatch();
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

            bool playerLost = (playerBase == null || !playerBase.IsAlive) && playerUnits == 0;
            bool enemyLost = (enemyBase == null || !enemyBase.IsAlive) && enemyUnits == 0;

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
