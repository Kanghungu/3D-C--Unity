using Game.Units;
using System.Collections.Generic;
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
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !PrototypeProductionCommandCatalog.HasProductionInputThisFrame(keyboard))
            {
                return;
            }

            PrototypeGameDatabase database = PrototypeRuntimeQuery.FindDatabase();
            List<ProductionStructure> playerProductions = PrototypeRuntimeQuery.FindPlayerProductionStructures();

            if (playerProductions.Count == 0 || database == null)
            {
                return;
            }

            int queueAmount = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed ? 3 : 1;
            List<(UnitArchetype Archetype, int Amount)> triggeredCommands = PrototypeProductionCommandCatalog.GetTriggeredCommands(keyboard, queueAmount);

            foreach ((UnitArchetype Archetype, int Amount) command in triggeredCommands)
            {
                QueueUnits(command.Archetype, database, command.Amount);
            }

            if (keyboard.backspaceKey.wasPressedThisFrame)
            {
                CancelLatestQueuedProduction(playerProductions);
            }
        }

        private static void QueueUnits(UnitArchetype archetype, PrototypeGameDatabase database, int amount)
        {
            UnitDefinition definition = database.GetDefinition(archetype);
            ProductionStructure structure = PrototypeRuntimeQuery.FindProductionStructure(UnitTeam.Player, archetype);

            if (definition == null || structure == null || !structure.IsAlive)
            {
                return;
            }

            for (int index = 0; index < amount; index++)
            {
                if (!structure.TryQueueProduction(definition))
                {
                    break;
                }
            }
        }

        private static void CancelLatestQueuedProduction(List<ProductionStructure> playerProductions)
        {
            ProductionStructure bestCandidate = null;
            float newestQueueTime = float.MinValue;

            foreach (ProductionStructure structure in playerProductions)
            {
                if (structure == null || !structure.IsAlive || structure.QueueCount <= 0)
                {
                    continue;
                }

                if (structure.LastQueueCommandTime >= newestQueueTime)
                {
                    newestQueueTime = structure.LastQueueCommandTime;
                    bestCandidate = structure;
                }
            }

            bestCandidate?.TryCancelLastQueuedProduction();
        }
    }
}
