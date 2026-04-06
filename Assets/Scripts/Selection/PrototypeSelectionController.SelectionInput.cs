using Game.Prototype;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Selection
{
    /// <summary>
    /// ?쒕옒洹??좏깮, ?⑥씪 ?좏깮, ?붾㈃ ?ш컖 ?좏깮.
    /// </summary>
    public partial class PrototypeSelectionController
    {
        private const float DragSelectThresholdPixels = 6f;

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
            if (selectionStartedOverHud)
            {
                return;
            }

            Vector2 releasePosition = Mouse.current.position.ReadValue();

            if (Vector2.Distance(dragStartScreenPosition, releasePosition) < DragSelectThresholdPixels)
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

            SelectableUnit unit = hit.collider.GetComponentInParent<SelectableUnit>();
            if (unit != null)
            {
                bool isDoubleClick = lastClickedUnit == unit && Time.time - lastClickTime <= DoubleClickThreshold;
                lastClickedUnit = unit;
                lastClickTime = Time.time;

                if (isDoubleClick)
                {
                    SelectAllMatchingArchetype(unit.Archetype);
                    return;
                }

                SetSelection(new[] { unit });
                return;
            }

            ClearSelection();
        }

        private void SelectAllMatchingArchetype(UnitArchetype archetype)
        {
            List<SelectableUnit> matchingUnits = new();

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit != null && unit.Archetype == archetype)
                {
                    matchingUnits.Add(unit);
                }
            }

            SetSelection(matchingUnits);
        }

        private void SelectUnitsInRect(Rect selectionRect)
        {
            List<SelectableUnit> unitsInRect = new();

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null)
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

        /// <summary>
        /// 誘몃땲留?諛뺤뒪 ?????붾뱶 XZ 踰붿쐞 ?덉쓽 ?꾧뎔 ?좊떅留??좏깮(?꾨줈?좏??낆슜).
        /// </summary>
        public void SelectPlayerUnitsInWorldXZBounds(float worldXMin, float worldXMax, float worldZMin, float worldZMax)
        {
            if (worldXMin > worldXMax)
            {
                (worldXMin, worldXMax) = (worldXMax, worldXMin);
            }

            if (worldZMin > worldZMax)
            {
                (worldZMin, worldZMax) = (worldZMax, worldZMin);
            }

            List<SelectableUnit> list = new List<SelectableUnit>();

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || unit.Team != UnitTeam.Player)
                {
                    continue;
                }

                CombatTarget ct = unit.GetComponent<CombatTarget>();
                if (ct == null || !ct.IsAlive)
                {
                    continue;
                }

                Vector3 p = unit.transform.position;
                if (p.x >= worldXMin && p.x <= worldXMax && p.z >= worldZMin && p.z <= worldZMax)
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

        private bool TryGetMouseRaycastHit(out RaycastHit hit)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return Physics.Raycast(ray, out hit, 1000f);
        }

        private static Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            start.y = Screen.height - start.y;
            end.y = Screen.height - end.y;
            Vector2 topLeft = Vector2.Min(start, end);
            Vector2 bottomRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }
    }
}

