using Game.Selection;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Prototype
{
    /// <summary>
    /// Handles player-triggered role abilities for selected units.
    /// </summary>
    public class PlayerAbilityController : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.fKey.wasPressedThisFrame)
            {
                return;
            }

            PrototypeSelectionController selectionController = PrototypeSelectionController.Instance;

            if (selectionController == null)
            {
                return;
            }

            foreach (SelectableUnit unit in selectionController.SelectedUnits)
            {
                if (unit != null)
                {
                    unit.TryActivateAbility();
                }
            }
        }
    }
}
