using Game.Audio;
using Game.BattleAces;
using Game.Prototype;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Selection
{
    /// <summary>
    /// 이동·공격·랠리·홀드 등 우클릭/단축키 명령과 이동 마커.
    /// </summary>
    public partial class PrototypeSelectionController
    {
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
                if (SelectionHasAlivePlayerUnit())
                {
                    TryPlayCommandRejectFeedback();
                }

                return;
            }

            if (IsRallyModifierPressed())
            {
                // Battle Aces — 코어 생산 유닛 집결점(프로토타입 거점 랠리와 별도)
                if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController baMatch) &&
                    baMatch.PlayerCore != null &&
                    !baMatch.IsFinished)
                {
                    if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
                    {
                        return;
                    }

                    baMatch.PlayerCore.SetRallyWorldPosition(hit.point);
                    ShowMoveMarker(hit.point, new Color(0.22f, 0.92f, 0.88f, 0.96f), "Rally");
                    BattleAcesHudOverlay.PulseRallyPointSetHint();
                    ProceduralAudioUtility.PlayRallySetConfirm();
                    return;
                }

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
                BattleAcesCombatJuice.NotifyAttackOrder(hit.point);
                BattleAcesFirstPlayGuide.NotifyGroundCommandIssued();
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

                BattleAcesCombatJuice.NotifyAttackMoveOrder(targetPoint);
                BattleAcesFirstPlayGuide.NotifyGroundCommandIssued();
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

            BattleAcesCombatJuice.NotifyAttackMoveOrder(targetPoint);
            BattleAcesFirstPlayGuide.NotifyGroundCommandIssued();
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
                TryPlayCommandRejectFeedback();
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

            BattleAcesFirstPlayGuide.NotifyGroundCommandIssued();
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

        private bool SelectionHasAlivePlayerUnit()
        {
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                SelectableUnit u = selectedUnits[i];
                if (u == null || u.Team != UnitTeam.Player)
                {
                    continue;
                }

                CombatTarget ct = u.GetComponent<CombatTarget>();
                if (ct == null || ct.IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        private void TryPlayCommandRejectFeedback()
        {
            if (Time.unscaledTime - lastCommandRejectFeedbackUnscaledTime < CommandRejectFeedbackCooldown)
            {
                return;
            }

            lastCommandRejectFeedbackUnscaledTime = Time.unscaledTime;
            ProceduralAudioUtility.PlayUiCommandRejected();
            BattleAcesHudOverlay.PulseCommandRejectTextHint();
        }
    }
}
