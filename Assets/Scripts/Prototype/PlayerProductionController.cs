using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Prototype
{
    /// <summary>
    /// Handles player unit production commands from the keyboard.
    /// </summary>
    public class PlayerProductionController : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            BaseStructure playerBase = PrototypeRuntimeQuery.FindPlayerBase();

            if (playerBase == null || !playerBase.IsAlive)
            {
                return;
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                playerBase.TryQueueProduction(Game.Units.UnitArchetype.Vanguard);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                playerBase.TryQueueProduction(Game.Units.UnitArchetype.Skirmisher);
            }
        }
    }
}
