using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Fleet
{
    /// <summary>
    /// Minimal fleet selection and command controller for the experiment scene.
    /// </summary>
    public class FleetSelectionController : MonoBehaviour
    {
        private Camera mainCamera;
        private readonly List<FleetShip> selectedShips = new();

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (Mouse.current == null || mainCamera == null)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TrySelectShip();
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                TryIssueCommand();
            }
        }

        private void TrySelectShip()
        {
            if (!TryRaycast(out RaycastHit hit))
            {
                ClearSelection();
                return;
            }

            if (hit.collider.TryGetComponent(out FleetShip ship) && ship.IsPlayerControlled)
            {
                ClearSelection();
                selectedShips.Add(ship);
                ship.SetSelected(true);
                return;
            }

            ClearSelection();
        }

        private void TryIssueCommand()
        {
            if (selectedShips.Count == 0 || !TryRaycast(out RaycastHit hit))
            {
                return;
            }

            if (hit.collider.TryGetComponent(out FleetShip ship) && !ship.IsPlayerControlled)
            {
                foreach (FleetShip selectedShip in selectedShips)
                {
                    if (selectedShip != null)
                    {
                        selectedShip.Attack(ship);
                    }
                }

                return;
            }

            Vector3 point = hit.point;

            for (int index = 0; index < selectedShips.Count; index++)
            {
                if (selectedShips[index] != null)
                {
                    selectedShips[index].SetDestination(point + new Vector3(index * 1.6f, 0f, 0f));
                }
            }
        }

        private bool TryRaycast(out RaycastHit hit)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return Physics.Raycast(ray, out hit, 1000f);
        }

        private void ClearSelection()
        {
            foreach (FleetShip ship in selectedShips)
            {
                if (ship != null)
                {
                    ship.SetSelected(false);
                }
            }

            selectedShips.Clear();
        }
    }
}
