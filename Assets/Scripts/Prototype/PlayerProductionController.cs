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

            ProductionStructure playerProduction = PrototypeRuntimeQuery.FindPlayerProductionStructure();
            PrototypeGameDatabase database = PrototypeRuntimeQuery.FindDatabase();

            if (playerProduction == null || !playerProduction.IsAlive || database == null)
            {
                return;
            }

            int queueAmount = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed ? 3 : 1;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                QueueUnits(playerProduction, database.GetDefinition(UnitArchetype.Vanguard), queueAmount);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                QueueUnits(playerProduction, database.GetDefinition(UnitArchetype.Skirmisher), queueAmount);
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                QueueUnits(playerProduction, database.GetDefinition(UnitArchetype.Artillery), queueAmount);
            }

            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                playerProduction.TryCancelLastQueuedProduction();
            }
        }

        private static void QueueUnits(ProductionStructure playerProduction, UnitDefinition definition, int amount)
        {
            if (definition == null)
            {
                return;
            }

            for (int index = 0; index < amount; index++)
            {
                if (!playerProduction.TryQueueProduction(definition))
                {
                    break;
                }
            }
        }
    }
}
