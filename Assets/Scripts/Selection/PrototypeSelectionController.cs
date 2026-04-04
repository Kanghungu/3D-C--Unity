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
        public readonly struct ControlGroupMarkerInfo
        {
            public ControlGroupMarkerInfo(int groupIndex, Vector3 worldCenter, int unitCount, int selectedCount, bool isActive, float recallEmphasis, float assignmentEmphasis)
            {
                GroupIndex = groupIndex;
                WorldCenter = worldCenter;
                UnitCount = unitCount;
                SelectedCount = selectedCount;
                IsActive = isActive;
                RecallEmphasis = recallEmphasis;
                AssignmentEmphasis = assignmentEmphasis;
            }

            public int GroupIndex { get; }
            public Vector3 WorldCenter { get; }
            public int UnitCount { get; }
            public int SelectedCount { get; }
            public bool IsActive { get; }
            public float RecallEmphasis { get; }
            public float AssignmentEmphasis { get; }
            public bool HasSelectedMembers => SelectedCount > 0;
            public bool IsFullySelected => SelectedCount > 0 && SelectedCount >= UnitCount;
            public float SelectionCoverage => UnitCount > 0 ? (float)SelectedCount / UnitCount : 0f;
        }

        public readonly struct SelectedControlGroupInfo
        {
            public SelectedControlGroupInfo(int groupIndex, int selectedCount, int totalCount)
            {
                GroupIndex = groupIndex;
                SelectedCount = selectedCount;
                TotalCount = totalCount;
            }

            public int GroupIndex { get; }
            public int SelectedCount { get; }
            public int TotalCount { get; }
            public bool IsFullySelected => SelectedCount > 0 && SelectedCount >= TotalCount;
        }

        private const float DoubleClickThreshold = 0.3f;
        private const float ControlGroupDoubleTapThreshold = 0.35f;
        private const float ControlGroupRecallHighlightDuration = 1.1f;
        private const float ControlGroupAssignHighlightDuration = 0.95f;
        private const float CommandMarkerLifetime = 0.85f;

        private Camera mainCamera;
        private readonly List<SelectableUnit> selectedUnits = new();
        private readonly Dictionary<int, List<SelectableUnit>> controlGroups = new();
        private Vector2 dragStartScreenPosition;
        private bool isDraggingSelection;
        private Texture2D selectionTexture;
        private GameObject moveMarker;
        private string moveMarkerLabel = string.Empty;
        private float lastClickTime;
        private SelectableUnit lastClickedUnit;
        private int lastAssignedControlGroup = -1;
        private float lastControlGroupAssignTime;
        private int lastRecalledControlGroup = -1;
        private float lastControlGroupRecallTime;
        private string lastRecalledControlGroupRouteLabel = string.Empty;
        private bool selectionStartedOverHud;

        public static PrototypeSelectionController Instance { get; private set; }
        public IReadOnlyList<SelectableUnit> SelectedUnits => selectedUnits;
        public bool HasVisibleMoveMarker => moveMarker != null && moveMarker.activeSelf;
        public Vector3 MoveMarkerWorldPosition => moveMarker != null ? moveMarker.transform.position : Vector3.zero;
        public string MoveMarkerLabel => HasVisibleMoveMarker ? moveMarkerLabel : string.Empty;
        public bool HasRecentControlGroupAssignment =>
            lastAssignedControlGroup > 0 && Time.time - lastControlGroupAssignTime <= ControlGroupAssignHighlightDuration;
        public string RecentControlGroupAssignmentLabel => HasRecentControlGroupAssignment ? $"F{lastAssignedControlGroup}" : string.Empty;
        public bool HasRecentControlGroupRecall =>
            lastRecalledControlGroup > 0 && Time.time - lastControlGroupRecallTime <= ControlGroupRecallHighlightDuration;
        public string RecentControlGroupRecallLabel => HasRecentControlGroupRecall ? $"F{lastRecalledControlGroup}" : string.Empty;
        public string RecentControlGroupRouteLabel => HasRecentControlGroupRecall ? lastRecalledControlGroupRouteLabel : string.Empty;
        public Color MoveMarkerColor
        {
            get
            {
                if (moveMarker == null)
                {
                    return Color.white;
                }

                Renderer markerRenderer = moveMarker.GetComponent<Renderer>();
                return markerRenderer != null ? markerRenderer.material.color : Color.white;
            }
        }

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
                selectionStartedOverHud = PrototypeHudLayoutUtility.IsScreenPositionOverInteractiveHud(dragStartScreenPosition);
                isDraggingSelection = !selectionStartedOverHud;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleSelectionRelease();
                isDraggingSelection = false;
                selectionStartedOverHud = false;
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

        public void GetControlGroupMarkers(List<ControlGroupMarkerInfo> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();

            for (int index = 1; index <= 5; index++)
            {
                if (!controlGroups.TryGetValue(index, out List<SelectableUnit> units) || units == null || units.Count == 0)
                {
                    continue;
                }

                int aliveCount = 0;
                int selectedCount = 0;

                foreach (SelectableUnit unit in units)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    aliveCount++;

                    if (selectedUnits.Contains(unit))
                    {
                        selectedCount++;
                    }
                }

                if (aliveCount <= 0)
                {
                    continue;
                }

                Vector3 center = PrototypeSelectionUtility.GetSelectionCenter(units);
                bool hasSelectedMember = selectedCount > 0;
                bool isActive = hasSelectedMember || lastRecalledControlGroup == index;
                float recallEmphasis = 0f;

                if (lastRecalledControlGroup == index)
                {
                    float age = Time.time - lastControlGroupRecallTime;
                    recallEmphasis = Mathf.Clamp01(1f - age / ControlGroupRecallHighlightDuration);
                }

                float assignmentEmphasis = 0f;

                if (lastAssignedControlGroup == index)
                {
                    float age = Time.time - lastControlGroupAssignTime;
                    assignmentEmphasis = Mathf.Clamp01(1f - age / ControlGroupAssignHighlightDuration);
                }

                results.Add(new ControlGroupMarkerInfo(index, center, aliveCount, selectedCount, isActive, recallEmphasis, assignmentEmphasis));
            }
        }

        public void GetSelectedControlGroupInfos(List<SelectedControlGroupInfo> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();

            if (selectedUnits.Count == 0)
            {
                return;
            }

            for (int index = 1; index <= 5; index++)
            {
                if (!controlGroups.TryGetValue(index, out List<SelectableUnit> units) || units == null || units.Count == 0)
                {
                    continue;
                }

                int aliveCount = 0;
                int selectedCount = 0;

                foreach (SelectableUnit unit in units)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    aliveCount++;

                    if (selectedUnits.Contains(unit))
                    {
                        selectedCount++;
                    }
                }

                if (selectedCount > 0 && aliveCount > 0)
                {
                    results.Add(new SelectedControlGroupInfo(index, selectedCount, aliveCount));
                }
            }
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
            lastAssignedControlGroup = groupIndex;
            lastControlGroupAssignTime = Time.time;
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
            lastRecalledControlGroupRouteLabel = BuildControlGroupRouteLabel(aliveUnits);

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
            cameraController.CenterViewOnWorldPoint(center);
        }

        private static string BuildControlGroupRouteLabel(List<SelectableUnit> units)
        {
            if (units == null || units.Count == 0)
            {
                return string.Empty;
            }

            Vector3 center = PrototypeSelectionUtility.GetSelectionCenter(units);
            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);

            if (playerBase != null && playerBase.IsAlive && Vector3.Distance(center, playerBase.transform.position) <= 240f)
            {
                return "본진 축";
            }

            ControlNode nearestNode = null;
            float nearestDistance = float.MaxValue;
            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(center, node.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNode = node;
                }
            }

            if (nearestNode != null && nearestDistance <= 180f)
            {
                return $"{nearestNode.NodeLabel} 축";
            }

            if (enemyBase != null && enemyBase.IsAlive && Vector3.Distance(center, enemyBase.transform.position) <= 260f)
            {
                return "적 본진 축";
            }

            return "전선 축";
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
            Vector3 holdPoint = GetSelectionCenter();

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit != null)
                {
                    unit.HoldPosition();
                }
            }

            ShowMoveMarker(holdPoint, new Color(0.9f, 0.82f, 1f, 0.92f), "Hold");
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

            ShowMoveMarker(guardPoint, new Color(0.55f, 0.95f, 0.45f, 0.9f), "Guard");
        }

        private void IssueFallbackCommand()
        {
            if (!PrototypeBattlefieldUtility.TryFindClosestFriendlyAnchor(UnitTeam.Player, GetSelectionCenter(), out Vector3 fallbackPoint))
            {
                return;
            }

            Vector3 fallbackDir = (fallbackPoint - GetSelectionCenter());
            fallbackDir.y = 0f;
            if (fallbackDir.sqrMagnitude > 0.01f) fallbackDir = fallbackDir.normalized;
            List<Vector3> formationPoints = PrototypeSelectionUtility.BuildFormationPoints(fallbackPoint, selectedUnits.Count, 3.2f, fallbackDir);
            for (int index = 0; index < selectedUnits.Count; index++)
            {
                if (selectedUnits[index] != null)
                {
                    selectedUnits[index].MoveTo(formationPoints[index]);
                }
            }

            ShowMoveMarker(fallbackPoint, new Color(0.45f, 0.85f, 1f, 0.9f), "Fallback");
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
            if (selectionStartedOverHud)
            {
                return;
            }

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

            if (hit.collider.TryGetComponent(out SelectableUnit unit))
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

        private void TryIssueCommand()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (PrototypeHudLayoutUtility.IsScreenPositionOverInteractiveHud(mousePosition))
            {
                return;
            }

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

                ShowMoveMarker(hit.point, new Color(1f, 0.9f, 0.25f, 0.9f), "Rally");
                return;
            }

            if (selectedUnits.Count == 0)
            {
                return;
            }

            // 선택된 유닛 중 플레이어 유닛만 명령 대상
            bool hasPlayerUnit = false;
            foreach (SelectableUnit su in selectedUnits)
            {
                if (su != null && su.Team == UnitTeam.Player) { hasPlayerUnit = true; break; }
            }
            if (!hasPlayerUnit) return;

            if (hit.collider.TryGetComponent(out CombatTarget target) && target.Team == UnitTeam.Enemy)
            {
                if (!CanAttackTarget(target))
                {
                    RedirectAssaultToPriorityNode();
                    return;
                }

                foreach (SelectableUnit selectedUnit in selectedUnits)
                {
                    if (selectedUnit != null && selectedUnit.Team == UnitTeam.Player)
                    {
                        selectedUnit.Attack(target);
                    }
                }

                ShowMoveMarker(hit.point, new Color(1f, 0.45f, 0.25f, 0.9f), "Attack");
                return;
            }

            Vector3 targetPoint = hit.point;
            Vector3 moveDirection = (targetPoint - GetSelectionCenter());
            moveDirection.y = 0f;
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                moveDirection = moveDirection.normalized;
            }

            List<Vector3> formationPoints = PrototypeSelectionUtility.BuildFormationPoints(targetPoint, selectedUnits.Count, 2.8f, moveDirection);

            if (IsAttackMoveModifierPressed())
            {
                ShowMoveMarker(targetPoint, new Color(1f, 0.65f, 0.2f, 0.9f), "Attack Move");

                for (int index = 0; index < selectedUnits.Count; index++)
                {
                    if (selectedUnits[index] != null && selectedUnits[index].Team == UnitTeam.Player)
                    {
                        selectedUnits[index].AttackMoveTo(formationPoints[index]);
                    }
                }

                return;
            }

            ShowMoveMarker(targetPoint, new Color(0.2f, 0.8f, 1f, 0.8f), "Move");

            for (int index = 0; index < selectedUnits.Count; index++)
            {
                if (selectedUnits[index] != null && selectedUnits[index].Team == UnitTeam.Player)
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
            Vector3 assaultDir = (targetPoint - GetSelectionCenter());
            assaultDir.y = 0f;
            if (assaultDir.sqrMagnitude > 0.01f) assaultDir = assaultDir.normalized;
            List<Vector3> formationPoints = PrototypeSelectionUtility.BuildFormationPoints(targetPoint, selectedUnits.Count, 4.4f, assaultDir);
            ShowMoveMarker(targetPoint, new Color(0.95f, 0.72f, 0.2f, 0.95f), "Assault");

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

        private void ShowMoveMarker(Vector3 position, Color color, string label)
        {
            if (moveMarker == null)
            {
                return;
            }

            Renderer markerRenderer = moveMarker.GetComponent<Renderer>();
            markerRenderer.material.color = color;
            moveMarkerLabel = string.IsNullOrWhiteSpace(label) ? "Move" : label;
            moveMarker.transform.position = new Vector3(position.x, 0.1f, position.z);
            moveMarker.SetActive(true);
            CancelInvoke(nameof(HideMoveMarker));
            Invoke(nameof(HideMoveMarker), CommandMarkerLifetime);
        }

        private void HideMoveMarker()
        {
            if (moveMarker != null)
            {
                moveMarker.SetActive(false);
            }

            moveMarkerLabel = string.Empty;
        }
    }
}





