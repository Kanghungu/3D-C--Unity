using Game.BattleAces;
using Game.Prototype;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Selection
{
    /// <summary>
    /// Battle Aces 전투 중 편의 단축키(Ctrl+A 전체 선택, Esc 선택 해제).
    /// </summary>
    public partial class PrototypeSelectionController
    {
        private void HandleBattleSelectionQualityHotkeys()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (!CanUseBattleAcesSelectionHotkeys())
            {
                return;
            }

            Keyboard kb = Keyboard.current;

            if (kb.aKey.wasPressedThisFrame &&
                (kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed))
            {
                SelectAllPlayerUnitsOnBattlefield();
                return;
            }

            if (kb.escapeKey.wasPressedThisFrame && selectedUnits.Count > 0)
            {
                ClearSelection();
            }
        }

        private static bool CanUseBattleAcesSelectionHotkeys()
        {
            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
            {
                return false;
            }

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match) && match.IsFinished)
            {
                return false;
            }

            return true;
        }

        private void SelectAllPlayerUnitsOnBattlefield()
        {
            List<SelectableUnit> list = new List<SelectableUnit>();

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (IsPlayerSelectable(unit))
                {
                    list.Add(unit);
                }
            }

            if (list.Count == 0)
            {
                ClearSelection();
                return;
            }

            SetSelection(list);
        }
    }
}
