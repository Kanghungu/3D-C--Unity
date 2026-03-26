using Game.Units;
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

            int queueAmount = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed ? 3 : 1;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                QueueUnits(playerBase, UnitArchetype.Vanguard, queueAmount);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                QueueUnits(playerBase, UnitArchetype.Skirmisher, queueAmount);
            }

            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                playerBase.TryCancelLastQueuedProduction();
            }
        }

        private static void QueueUnits(BaseStructure playerBase, UnitArchetype archetype, int amount)
        {
            for (int index = 0; index < amount; index++)
            {
                if (!playerBase.TryQueueProduction(archetype))
                {
                    break;
                }
            }
        }
    }
}
