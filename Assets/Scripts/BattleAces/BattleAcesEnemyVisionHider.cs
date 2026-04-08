using System.Collections.Generic;
using Game.Prototype;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Hides enemy combat targets when they are outside the player's current fog-of-war vision.
    /// Keeps gameplay objects alive while reducing map-wide information.
    /// </summary>
    public sealed class BattleAcesEnemyVisionHider : MonoBehaviour
    {
        [SerializeField] private float refreshInterval = 0.08f;

        private float nextRefreshTime;

        private void LateUpdate()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + Mathf.Max(0.03f, refreshInterval);
            RefreshEnemyTargets();
        }

        private static void RefreshEnemyTargets()
        {
            BattleAcesFogOfWarDebug fog = BattleAcesFogOfWarDebug.Instance;
            if (fog == null)
            {
                return;
            }

            IReadOnlyList<CombatTarget> targets = PrototypeRuntimeRegistry.GetCombatTargets();
            for (int i = 0; i < targets.Count; i++)
            {
                CombatTarget target = targets[i];
                if (target == null || target.Team != UnitTeam.Enemy || !target.IsAlive)
                {
                    continue;
                }

                BattlefieldFogObject fogObject = target.GetComponent<BattlefieldFogObject>();
                if (fogObject == null)
                {
                    fogObject = target.gameObject.AddComponent<BattlefieldFogObject>();
                    fogObject.Configure(BattlefieldFogRequirement.Visible);
                }

                bool shouldHide = !fog.IsWorldVisible(target.transform.position);
                fogObject.SetFogged(shouldHide);
            }
        }
    }
}
