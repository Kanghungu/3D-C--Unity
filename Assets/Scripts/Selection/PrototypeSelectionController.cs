using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Selection
{
    /// <summary>
    /// Handles drag selection, move orders, and attack orders for the prototype.
    /// </summary>
    public class PrototypeSelectionController : MonoBehaviour
    {
        private Camera mainCamera;
        private readonly List<SelectableUnit> selectedUnits = new();
        private Vector2 dragStartScreenPosition;
        private bool isDraggingSelection;
        private Texture2D selectionTexture;
        private GameObject moveMarker;

        private void Awake()
        {
            mainCamera = Camera.main;
            selectionTexture = new Texture2D(1, 1);
            selectionTexture.SetPixel(0, 0, new Color(0.2f, 0.9f, 0.3f, 0.2f));
            selectionTexture.Apply();
            CreateMoveMarker();
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;

                if (mainCamera == null)
                {
                    return;
                }
            }

            if (Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                dragStartScreenPosition = Mouse.current.position.ReadValue();
                isDraggingSelection = true;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleSelectionRelease();
                isDraggingSelection = false;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                TryIssueCommand();
            }
        }

        private void OnGUI()
        {
            if (!isDraggingSelection || Mouse.current == null)
            {
                return;
            }

            Rect selectionRect = GetScreenRect(dragStartScreenPosition, Mouse.current.position.ReadValue());
            GUI.DrawTexture(selectionRect, selectionTexture);
        }

        private void HandleSelectionRelease()
        {
            Vector2 releasePosition = Mouse.current.position.ReadValue();

            if (Vector2.Distance(dragStartScreenPosition, releasePosition) < 10f)
            {
                TrySingleSelect();
                return;
            }

            Rect selectionRect = GetScreenRect(dragStartScreenPosition, releasePosition);
            SelectUnitsInRect(selectionRect);
        }

        private void TrySingleSelect()
        {
            if (!TryGetMouseRaycastHit(out RaycastHit hit))
            {
                ClearSelection();
                return;
            }

            if (hit.collider.TryGetComponent(out SelectableUnit unit) && unit.Team == UnitTeam.Player)
            {
                SetSelection(new[] { unit });
                return;
            }

            ClearSelection();
        }

        private void SelectUnitsInRect(Rect selectionRect)
        {
            List<SelectableUnit> unitsInRect = new();

            foreach (SelectableUnit unit in FindObjectsByType<SelectableUnit>(FindObjectsSortMode.None))
            {
                if (unit == null || unit.Team != UnitTeam.Player)
                {
                    continue;
                }

                Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

                if (screenPosition.z < 0f)
                {
                    continue;
                }

                screenPosition.y = Screen.height - screenPosition.y;

                if (selectionRect.Contains(screenPosition))
                {
                    unitsInRect.Add(unit);
                }
            }

            if (unitsInRect.Count == 0)
            {
                ClearSelection();
                return;
            }

            SetSelection(unitsInRect);
        }

        private void TryIssueCommand()
        {
            if (selectedUnits.Count == 0 || !TryGetMouseRaycastHit(out RaycastHit hit))
            {
                return;
            }

            if (hit.collider.TryGetComponent(out CombatTarget target) && target.Team == UnitTeam.Enemy)
            {
                foreach (SelectableUnit selectedUnit in selectedUnits)
                {
                    if (selectedUnit != null)
                    {
                        selectedUnit.Attack(target);
                    }
                }

                return;
            }

            Vector3 targetPoint = hit.point;
            ShowMoveMarker(targetPoint);
            List<Vector3> formationPoints = BuildFormationPoints(targetPoint, selectedUnits.Count, 2f);

            for (int index = 0; index < selectedUnits.Count; index++)
            {
                if (selectedUnits[index] != null)
                {
                    selectedUnits[index].MoveTo(formationPoints[index]);
                }
            }
        }

        private bool TryGetMouseRaycastHit(out RaycastHit hit)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return Physics.Raycast(ray, out hit, 500f);
        }

        private void SetSelection(IEnumerable<SelectableUnit> units)
        {
            ClearSelection();

            foreach (SelectableUnit unit in units)
            {
                if (unit == null)
                {
                    continue;
                }

                selectedUnits.Add(unit);
                unit.SetSelected(true);
            }
        }

        private void ClearSelection()
        {
            foreach (SelectableUnit selectedUnit in selectedUnits)
            {
                if (selectedUnit != null)
                {
                    selectedUnit.SetSelected(false);
                }
            }

            selectedUnits.Clear();
        }

        private static Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            start.y = Screen.height - start.y;
            end.y = Screen.height - end.y;
            Vector2 topLeft = Vector2.Min(start, end);
            Vector2 bottomRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private static List<Vector3> BuildFormationPoints(Vector3 center, int count, float spacing)
        {
            List<Vector3> points = new(count);
            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));

            for (int index = 0; index < count; index++)
            {
                int row = index / columns;
                int column = index % columns;
                float xOffset = (column - (columns - 1) * 0.5f) * spacing;
                float zOffset = (row - (columns - 1) * 0.5f) * spacing;
                points.Add(center + new Vector3(xOffset, 0f, zOffset));
            }

            return points;
        }

        private void CreateMoveMarker()
        {
            moveMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            moveMarker.name = "Move Marker";
            moveMarker.transform.localScale = new Vector3(0.5f, 0.03f, 0.5f);
            moveMarker.GetComponent<Collider>().enabled = false;
            moveMarker.SetActive(false);

            Renderer markerRenderer = moveMarker.GetComponent<Renderer>();
            markerRenderer.material.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        }

        private void ShowMoveMarker(Vector3 position)
        {
            if (moveMarker == null)
            {
                return;
            }

            moveMarker.transform.position = new Vector3(position.x, 0.1f, position.z);
            moveMarker.SetActive(true);
            CancelInvoke(nameof(HideMoveMarker));
            Invoke(nameof(HideMoveMarker), 0.6f);
        }

        private void HideMoveMarker()
        {
            if (moveMarker != null)
            {
                moveMarker.SetActive(false);
            }
        }
    }
}
