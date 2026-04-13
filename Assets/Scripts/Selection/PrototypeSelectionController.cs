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
    public partial class PrototypeSelectionController : MonoBehaviour
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

        private float lastCommandRejectFeedbackUnscaledTime = -100f;
        private const float CommandRejectFeedbackCooldown = 0.42f;

        /// <summary>선택 변경 틱음 스팸 방지 — BattleAcesFeedbackTiming.SelectionChangeAudioCooldownUnscaled</summary>
        private float lastSelectionChangeAudioUnscaled = -999f;

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
            HandleBattleSelectionQualityHotkeys();

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

        /// <summary>사망 연기 중에도 선택 목록에서 즉시 제거</summary>
        public void DeselectUnit(SelectableUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            for (int index = selectedUnits.Count - 1; index >= 0; index--)
            {
                if (selectedUnits[index] != unit)
                {
                    continue;
                }

                unit.SetSelected(false);
                selectedUnits.RemoveAt(index);
                if (selectedUnits.Count == 0)
                {
                    lastClickTime = 0f;
                }

                return;
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
    }
}
