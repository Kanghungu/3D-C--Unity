using Game.Units;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Game.Prototype
{
    /// <summary>
    /// Shared production hotkey mapping for prototype builds.
    /// Keeps keyboard-to-archetype rules in one place so HUD and production logic can evolve together.
    /// </summary>
    public static class PrototypeProductionCommandCatalog
    {
        public readonly struct ProductionCommand
        {
            public ProductionCommand(Key triggerKey, string keyLabel, UnitArchetype archetype, bool supportsBatch)
            {
                TriggerKey = triggerKey;
                KeyLabel = keyLabel;
                Archetype = archetype;
                SupportsBatch = supportsBatch;
            }

            public Key TriggerKey { get; }
            public string KeyLabel { get; }
            public UnitArchetype Archetype { get; }
            public bool SupportsBatch { get; }
        }

        // MVP: Foundry(1~3) + Siege Chapel(4~5)만 사용. 나머지 병종은 씬/AI 전용.
        private static readonly ProductionCommand[] Commands =
        {
            new(Key.Digit1, "1", UnitArchetype.Spearman, true),
            new(Key.Digit2, "2", UnitArchetype.ShieldInfantry, true),
            new(Key.Digit3, "3", UnitArchetype.Rifleman, true),
            new(Key.Digit4, "4", UnitArchetype.Fighter, true),
            new(Key.Digit5, "5", UnitArchetype.Artillery, true),
        };

        public static IReadOnlyList<ProductionCommand> GetCommands()
        {
            return Commands;
        }

        public static bool HasProductionInputThisFrame(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return false;
            }

            foreach (ProductionCommand command in Commands)
            {
                if (keyboard[command.TriggerKey].wasPressedThisFrame)
                {
                    return true;
                }
            }

            return keyboard.backspaceKey.wasPressedThisFrame;
        }

        public static List<(UnitArchetype Archetype, int Amount)> GetTriggeredCommands(Keyboard keyboard, int queuedBatchAmount)
        {
            List<(UnitArchetype Archetype, int Amount)> triggered = new();
            if (keyboard == null)
            {
                return triggered;
            }

            foreach (ProductionCommand command in Commands)
            {
                if (!keyboard[command.TriggerKey].wasPressedThisFrame)
                {
                    continue;
                }

                int amount = command.SupportsBatch ? queuedBatchAmount : 1;
                triggered.Add((command.Archetype, amount));
            }

            return triggered;
        }
    }
}
