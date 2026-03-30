using Game.CameraSystem;
using Game.Prototype;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Selection
{
    /// <summary>
    /// Handles drag selection, move orders, and attack orders for the prototype.
    /// Adds lightweight control groups for large-battle management.
    /// </summary>
    public class PrototypeSelectionController : MonoBehaviour
    {
        private const float DoubleClickThreshold = 0.3f;
        private const float ControlGroupDoubleTapThreshold = 0.35f;

        private Camera mainCamera;
        private readonly List<SelectableUnit> selectedUnits = new();
        private readonly Dictionary<int, List<SelectableUnit>> controlGroups = new();
        private Vector2 dragStartScreenPosition;
        private bool isDraggingSelection;
        private Texture2D selectionTexture;
        private GameObject moveMarker;
        private float lastClickTime;
        private SelectableUnit lastClickedUnit;
        private int lastRecalledControlGroup = -1;
        private float lastControlGroupRecallTime;

        public static PrototypeSelectionController Instance { get; private set; }
        public IReadOnlyList<SelectableUnit> SelectedUnits => selectedUnits;

        private void Awake()
        {
            Instance = this;
            mainCamera = Camera.main;
            selectionTexture = new Texture2D(1, 1);
            selectionTexture.SetPixel(0, 0, new Color(0.2f, 0.9f, 0.3f, 0.2f));
            selectionTexture.Apply();
            CreateMoveMarker();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            RemoveDestroyedSelections();
            RemoveDestroyedControlGroupUnits();

            if (mainCamera == null)
            {
                mainCamera = Camera.main;

                if (mainCamera == null)
                {
                    return;
                }
            }

            if (Mouse.current == null || Keyboard.current == null)
            {
                return;
            }

            HandleControlGroupHotkeys();
            HandleCommandHotkeys();

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

        public void RemoveDestroyedSelections()
        {
            bool removedAny = false;

            for (int index = selectedUnits.Count - 1; index >= 0; index--)
            {
                if (selectedUnits[index] == null)
                {
                    selectedUnits.RemoveAt(index);
                    removedAny = true;
                }
            }

            if (removedAny && selectedUnits.Count == 0)
            {
                lastClickTime = 0f;
            }
        }

        public string GetControlGroupSummary()
        {
            return PrototypeSelectionUtility.BuildControlGroupSummary(controlGroups);
        }

        private void RemoveDestroyedControlGroupUnits()
        {
            List<int> emptyGroups = new();

            foreach (KeyValuePair<int, List<SelectableUnit>> pair in controlGroups)
            {
                for (int index = pair.Value.Count - 1; index >= 0; index--)
                {
                    if (pair.Value[index] == null)
                    {
                        pair.Value.RemoveAt(index);
                    }
                }

                if (pair.Value.Count == 0)
                {
                    emptyGroups.Add(pair.Key);
                }
            }

            foreach (int groupIndex in emptyGroups)
            {
                controlGroups.Remove(groupIndex);
            }
        }

        private void HandleControlGroupHotkeys()
        {
            for (int index = 1; index <= 5; index++)
            {
                Key functionKey = index switch
                {
                    1 => Key.F1,
                    2 => Key.F2,
                    3 => Key.F3,
                    4 => Key.F4,
                    _ => Key.F5
                };

                if (!Keyboard.current[functionKey].wasPressedThisFrame)
                {
                    continue;
                }

                bool assignGroup = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

                if (assignGroup)
                {
                    AssignControlGroup(index);
                }
                else
                {
                    RecallControlGroup(index);
                }
            }
        }

        private void AssignControlGroup(int groupIndex)
        {
            List<SelectableUnit> groupUnits = new();

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit != null)
                {
                    groupUnits.Add(unit);
                }
            }

            if (groupUnits.Count == 0)
            {
                controlGroups.Remove(groupIndex);
                return;
            }

            controlGroups[groupIndex] = groupUnits;
        }

        private void RecallControlGroup(int groupIndex)
        {
            if (!controlGroups.TryGetValue(groupIndex, out List<SelectableUnit> groupUnits) || groupUnits.Count == 0)
            {
                return;
            }

            List<SelectableUnit> aliveUnits = new();
            foreach (SelectableUnit unit in groupUnits)
            {
                if (unit != null)
                {
                    aliveUnits.Add(unit);
                }
            }

            if (aliveUnits.Count == 0)
            {
                controlGroups.Remove(groupIndex);
                return;
            }

            SetSelection(aliveUnits);

            bool isDoubleTap = lastRecalledControlGroup == groupIndex && Time.time - lastControlGroupRecallTime <= ControlGroupDoubleTapThreshold;
            lastRecalledControlGroup = groupIndex;
            lastControlGroupRecallTime = Time.time;

            if (isDoubleTap)
            {
                SnapCameraToUnits(aliveUnits);
            }
        }

        private void SnapCameraToUnits(List<SelectableUnit> units)
        {
            RTSCameraController cameraController = mainCamera != null ? mainCamera.GetComponent<RTSCameraController>() : null;
            if (cameraController == null || units == null || units.Count == 0)
            {
                return;
            }

            Vector3 center = PrototypeSelectionUtility.GetSelectionCenter(units);
            cameraController.SnapToWorldPoint(center);
        }

        private void HandleCommandHotkeys()
        {
            if (Keyboard.current == null || selectedUnits.Count == 0)
            {
                return;
            }

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                IssueHoldCommand();
            }

            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                IssueGuardCommand();
            }

            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                IssueFallbackCommand();
            }
        }

        private void IssueHoldCommand()
        {
            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit != null)
                {
                    unit.HoldPosition();
                }
            }
        }

        private void IssueGuardCommand()
        {
            Vector3 guardPoint = GetSelectionCenter();
            if (PrototypeBattlefieldUtility.TryFindClosestFriendlyAnchor(UnitTeam.Player, guardPoint, out Vector3 anchor))
            {
                guardPoint = anchor;
            }

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit != null)
                {
                    unit.GuardPoint(guardPoint, 22f);
                }
            }

            ShowMoveMarker(guardPoint, new Color(0.55f, 0.95f, 0.45f, 0.9f));
        }

        private void IssueFallbackCommand()
        {
            if (!PrototypeBattlefieldUtility.TryFindClosestFriendlyAnchor(UnitTeam.Player, GetSelectionCenter(), out Vector3 fallbackPoint))
            {
                return;
            }

            List<Vector3> formationPoints = PrototypeSelectionUtility.BuildFormationPoints(fallbackPoint, selectedUnits.Count, 3.2f);
            for (int index = 0; index < selectedUnits.Count; index++)
            {
                if (selectedUnits[index] != null)
                {
                    selectedUnits[index].MoveTo(formationPoints[index]);
                }
            }

            ShowMoveMarker(fallbackPoint, new Color(0.45f, 0.85f, 1f, 0.9f));
        }

        private Vector3 GetSelectionCenter()
        {
            return PrototypeSelectionUtility.GetSelectionCenter(selectedUnits);
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
                if (unit != null && unit.Team == UnitTeam.Player && unit.Archetype == archetype)
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
            if (!TryGetMouseRaycastHit(out RaycastHit hit))
            {
                return;
            }

            if (IsRallyModifierPressed())
            {
                BaseStructure playerBase = PrototypeRuntimeQuery.FindPlayerBase();
                List<ProductionStructure> playerProductions = PrototypeRuntimeQuery.FindPlayerProductionStructures();

                if (playerBase != null && playerBase.IsAlive)
                {
                    playerBase.SetRallyPoint(hit.point);
                }

                foreach (ProductionStructure structure in playerProductions)
                {
                    if (structure != null && structure.IsAlive)
                    {
                        structure.SetRallyPoint(hit.point);
                    }
                }

                ShowMoveMarker(hit.point, new Color(1f, 0.9f, 0.25f, 0.9f));
                return;
            }

            if (selectedUnits.Count == 0)
            {
                return;
            }

            if (hit.collider.TryGetComponent(out CombatTarget target) && target.Team == UnitTeam.Enemy)
            {
                if (!CanAttackTarget(target))
                {
                    RedirectAssaultToPriorityNode();
                    return;
                }

                foreach (SelectableUnit selectedUnit in selectedUnits)
                {
                    if (selectedUnit != null)
                    {
                        selectedUnit.Attack(target);
                    }
                }

                ShowMoveMarker(hit.point, new Color(1f, 0.45f, 0.25f, 0.9f));
                return;
            }

            Vector3 targetPoint = hit.point;
            List<Vector3> formationPoints = PrototypeSelectionUtility.BuildFormationPoints(targetPoint, selectedUnits.Count, 2.8f);

            if (IsAttackMoveModifierPressed())
            {
                ShowMoveMarker(targetPoint, new Color(1f, 0.65f, 0.2f, 0.9f));

                for (int index = 0; index < selectedUnits.Count; index++)
                {
                    if (selectedUnits[index] != null)
                    {
                        selectedUnits[index].AttackMoveTo(formationPoints[index]);
                    }
                }

                return;
            }

            ShowMoveMarker(targetPoint, new Color(0.2f, 0.8f, 1f, 0.8f));

            for (int index = 0; index < selectedUnits.Count; index++)
            {
                if (selectedUnits[index] != null)
                {
                    selectedUnits[index].MoveTo(formationPoints[index]);
                }
            }
        }

        private bool CanAttackTarget(CombatTarget target)
        {
            if (target == null)
            {
                return false;
            }

            BaseStructure baseStructure = target.GetComponent<BaseStructure>();
            if (baseStructure == null)
            {
                return true;
            }

            return BattleDirectiveController.CanTargetEnemyBaseStatic(UnitTeam.Player);
        }

        private void RedirectAssaultToPriorityNode()
        {
            BattleDirectiveController directiveController = BattleDirectiveController.Instance;
            ControlNode priorityNode = directiveController != null ? directiveController.FindPriorityNodeFor(UnitTeam.Player) : null;

            if (priorityNode == null)
            {
                return;
            }

            Vector3 targetPoint = priorityNode.transform.position;
            List<Vector3> formationPoints = PrototypeSelectionUtility.BuildFormationPoints(targetPoint, selectedUnits.Count, 4.4f);
            ShowMoveMarker(targetPoint, new Color(0.95f, 0.72f, 0.2f, 0.95f));

            for (int index = 0; index < selectedUnits.Count; index++)
            {
                if (selectedUnits[index] != null)
                {
                    selectedUnits[index].AttackMoveTo(formationPoints[index]);
                }
            }
        }

        private bool TryGetMouseRaycastHit(out RaycastHit hit)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return Physics.Raycast(ray, out hit, 1000f);
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

        private static bool IsRallyModifierPressed()
        {
            return Keyboard.current != null && (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);
        }

        private static bool IsAttackMoveModifierPressed()
        {
            if (Keyboard.current == null)
            {
                return false;
            }

            return Keyboard.current.aKey.isPressed || Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        }

        private static Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            start.y = Screen.height - start.y;
            end.y = Screen.height - end.y;
            Vector2 topLeft = Vector2.Min(start, end);
            Vector2 bottomRight = Vector2.Max(start, end);
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private void CreateMoveMarker()
        {
            moveMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            moveMarker.name = "Move Marker";
            moveMarker.transform.localScale = new Vector3(0.7f, 0.03f, 0.7f);
            moveMarker.GetComponent<Collider>().enabled = false;
            moveMarker.SetActive(false);
        }

        private void ShowMoveMarker(Vector3 position, Color color)
        {
            if (moveMarker == null)
            {
                return;
            }

            Renderer markerRenderer = moveMarker.GetComponent<Renderer>();
            markerRenderer.material.color = color;
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




