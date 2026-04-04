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
                QueueUnits(command.Archetype, database, playerProductions, command.Amount);
            }

            if (keyboard.backspaceKey.wasPressedThisFrame)
            {
                CancelLatestQueuedProduction(playerProductions);
            }
        }

        private static void QueueUnits(UnitArchetype archetype, PrototypeGameDatabase database, List<ProductionStructure> playerProductions, int amount)
        {
            UnitDefinition definition = database.GetDefinition(archetype);

            if (definition == null || playerProductions == null || playerProductions.Count == 0)
            {
                return;
            }

            for (int index = 0; index < amount; index++)
            {
                ProductionStructure structure = FindBestProductionStructure(playerProductions, archetype);

                if (structure == null || !structure.IsAlive)
                {
                    break;
                }

                if (!structure.TryQueueProduction(definition))
                {
                    playerProductions.Remove(structure);
                    index--;
                }
            }
        }

        private static ProductionStructure FindBestProductionStructure(List<ProductionStructure> playerProductions, UnitArchetype archetype)
        {
            ProductionStructure bestCandidate = null;
            float bestScore = float.MinValue;

            foreach (ProductionStructure structure in playerProductions)
            {
                if (structure == null || !structure.IsAlive || !structure.CanProduce(archetype))
                {
                    continue;
                }

                float score = EvaluateStructureScore(structure);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = structure;
                }
            }

            return bestCandidate;
        }

        private static float EvaluateStructureScore(ProductionStructure structure)
        {
            float score = 0f;

            if (structure.IsThreatEmergency)
            {
                score += 1000f;
            }
            else if (structure.IsThreatened)
            {
                score += 500f;
            }

            score += structure.NearbyThreatPressure * 100f;
            score += Mathf.Clamp(structure.NearbyHostileCount, 0, 12) * 12f;
            score -= structure.QueueCount * 8f;
            score += structure.SpeedMultiplier * 4f;

            return score;
        }

        private static void CancelLatestQueuedProduction(List<ProductionStructure> playerProductions)
        {
            ProductionStructure bestCandidate = null;
            float bestScore = float.MinValue;

            foreach (ProductionStructure structure in playerProductions)
            {
                if (structure == null || !structure.IsAlive || structure.QueueCount <= 0)
                {
                    continue;
                }

                float score = EvaluateCancelScore(structure);
                if (score >= bestScore)
                {
                    bestScore = score;
                    bestCandidate = structure;
                }
            }

            bestCandidate?.TryCancelLastQueuedProduction();
        }

        private static float EvaluateCancelScore(ProductionStructure structure)
        {
            float score = structure.LastQueueCommandTime * 100f;

            if (structure.IsThreatEmergency)
            {
                score += 1000f;
            }
            else if (structure.IsThreatened)
            {
                score += 500f;
            }

            score += structure.NearbyThreatPressure * 100f;
            score += Mathf.Clamp(structure.NearbyHostileCount, 0, 12) * 8f;
            score -= structure.QueueCount * 4f;
            return score;
        }
    }
}
