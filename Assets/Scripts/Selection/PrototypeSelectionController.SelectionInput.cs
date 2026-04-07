using Game.Prototype;
using Game.UI;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Selection
{
    /// <summary>
    /// 드래그 선택, 단일 선택, 화면 사각 선택.
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

            ImGuiGameUi.BeginScaledGui();
            Rect selectionRect = GetScreenRect(dragStartScreenPosition, Mouse.current.position.ReadValue());
            GUI.DrawTexture(selectionRect, selectionTexture);
            ImGuiGameUi.EndScaledGui();
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
            if (IsPlayerSelectable(unit))
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
                if (IsPlayerSelectable(unit) && unit.Archetype == archetype)
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
                if (!IsPlayerSelectable(unit))
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
                if (!IsPlayerSelectable(unit))
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
        /// 미니맵 박스 등 — 월드 XZ 범위 안의 아군 유닛만 선택(프로토타입용).
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

        private static bool IsPlayerSelectable(SelectableUnit unit)
        {
            if (unit == null || unit.Team != UnitTeam.Player)
            {
                return false;
            }

            CombatTarget ct = unit.GetComponent<CombatTarget>();
            return ct == null || ct.IsAlive;
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


