using UnityEngine;
using UnityEngine.InputSystem;
using Game.Selection;
using Game.Prototype;
using Game.Units;

namespace Game.CameraSystem
{
    /// <summary>
    /// Simple RTS-style camera movement for early prototyping.
    /// Attach this to the main camera and tune the serialized values in the inspector.
    /// </summary>
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 260f;
        [SerializeField] private float fastMoveMultiplier = 3.8f;
        [SerializeField] private float edgePanSize = 16f;
        [SerializeField] private float zoomedInMoveMultiplier = 0.85f;
        [SerializeField] private float zoomedOutMoveMultiplier = 2.35f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 120f;

        [Header("Zoom")]
        [SerializeField] private float minHeight = 3f;
        [SerializeField] private float maxHeight = 820f;
        [SerializeField] private bool zoomTowardCursor = true;
        [SerializeField] private float zoomCursorFollowStrength = 0.12f;

        // 속도 기반 줌 — 스크롤 = 충격량, 매 프레임 지수 감쇠
        private float _zoomVelocity;

        // 우클릭 드래그 패닝
        private bool _rightDragActive;
        private bool _rightDragPanning;
        private Vector2 _rightDragStartScreenPos;
        private Vector3 _rightDragGrabPoint;

        public bool IsRightDragPanning => _rightDragPanning;

        [Header("Bounds")]
        [SerializeField] private Vector2 xBounds = new(-1600f, 1600f);
        [SerializeField] private Vector2 zBounds = new(-1600f, 1600f);
        [SerializeField] private float mapBoundsExtension = 240f;
        private readonly Vector3[] defaultViewportCornerBuffer = new Vector3[4];
        private Camera attachedCamera;

        public void ApplyMapProfile(BattlefieldMapProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            float longestSide = Mathf.Max(profile.WorldSize.x, profile.WorldSize.y);
            float extension = Mathf.Max(mapBoundsExtension, longestSide * 0.08f);

            xBounds = new Vector2(profile.MinX - extension, profile.MaxX + extension);
            zBounds = new Vector2(profile.MinZ - extension, profile.MaxZ + extension);
            moveSpeed = Mathf.Max(moveSpeed, longestSide * 0.1f);
            minHeight = 3f;
            maxHeight = Mathf.Max(maxHeight, longestSide * 0.48f);
        }

        private void Awake()
        {
            _zoomVelocity = 0f;
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                return;
            }

            HandleQuickFocusHotkeys();
            HandleRightDragPan();
            HandleMovement();
            HandleRotation();
            HandleZoom();
            ApplyZoomSmooth();
            ClampPosition();
        }

        private void HandleRightDragPan()
        {
            Camera cam = GetAttachedCamera();
            if (cam == null)
            {
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                // 유닛이 선택된 상태면 우클릭은 유닛 명령용 — 드래그 패닝 비활성
                bool hasSelection = PrototypeSelectionController.Instance != null
                    && PrototypeSelectionController.Instance.SelectedUnits.Count > 0;
                if (hasSelection)
                {
                    _rightDragActive = false;
                    _rightDragPanning = false;
                    return;
                }

                Vector2 pressPos = Mouse.current.position.ReadValue();
                Ray ray = cam.ScreenPointToRay(new Vector3(pressPos.x, pressPos.y, 0f));
                Plane ground = new(Vector3.up, Vector3.zero);
                if (ground.Raycast(ray, out float enter))
                {
                    _rightDragGrabPoint = ray.GetPoint(enter);
                    _rightDragStartScreenPos = pressPos;
                    _rightDragActive = true;
                    _rightDragPanning = false;
                }
            }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                _rightDragActive = false;
                _rightDragPanning = false;
            }

            if (!_rightDragActive || !Mouse.current.rightButton.isPressed)
            {
                return;
            }

            Vector2 currentScreenPos = Mouse.current.position.ReadValue();

            // 6px 이상 이동 시 드래그 패닝 시작
            if (!_rightDragPanning && Vector2.Distance(currentScreenPos, _rightDragStartScreenPos) > 6f)
            {
                _rightDragPanning = true;
            }

            if (!_rightDragPanning)
            {
                return;
            }

            // 그라운드 락 패닝: 드래그 시작점이 항상 커서 아래에 오도록 카메라 이동
            Ray currentRay = cam.ScreenPointToRay(new Vector3(currentScreenPos.x, currentScreenPos.y, 0f));
            Plane groundPlane = new(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(currentRay, out float dist))
            {
                Vector3 currentGroundPoint = currentRay.GetPoint(dist);
                Vector3 pan = _rightDragGrabPoint - currentGroundPoint;
                pan.y = 0f;
                transform.position += pan;
            }
        }

        private void HandleMovement()
        {
            // 우클릭 드래그 패닝 중에는 키보드/엣지 이동 생략
            if (_rightDragPanning)
            {
                return;
            }

            Vector3 inputDirection = Vector3.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                inputDirection += Vector3.forward;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                inputDirection += Vector3.back;
            }

            if (Keyboard.current.rightArrowKey.isPressed)
            {
                inputDirection += Vector3.right;
            }

            if (Keyboard.current.leftArrowKey.isPressed)
            {
                inputDirection += Vector3.left;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (ShouldIgnoreEdgePan(mousePosition))
            {
                mousePosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            if (mousePosition.x <= edgePanSize)
            {
                inputDirection += Vector3.left;
            }

            if (mousePosition.x >= Screen.width - edgePanSize)
            {
                inputDirection += Vector3.right;
            }

            if (mousePosition.y <= edgePanSize)
            {
                inputDirection += Vector3.back;
            }

            if (mousePosition.y >= Screen.height - edgePanSize)
            {
                inputDirection += Vector3.forward;
            }

            if (inputDirection.sqrMagnitude <= 0f)
            {
                return;
            }

            inputDirection.Normalize();

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            float currentMoveSpeed = moveSpeed * EvaluateZoomMoveMultiplier();

            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            {
                currentMoveSpeed *= fastMoveMultiplier;
            }

            Vector3 movement = (forward * inputDirection.z + right * inputDirection.x) * currentMoveSpeed * Time.deltaTime;
            transform.position += movement;
        }

        private void HandleRotation()
        {
            float rotationInput = 0f;

            if (Keyboard.current.qKey.isPressed)
            {
                rotationInput -= 1f;
            }

            if (Keyboard.current.eKey.isPressed)
            {
                rotationInput += 1f;
            }

            if (Mathf.Approximately(rotationInput, 0f))
            {
                return;
            }

            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void HandleZoom()
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Approximately(scrollDelta, 0f))
            {
                return;
            }

            // 현재 높이 비례 충격량 — 높을수록 빠르게, 낮을수록 섬세하게
            float impulse = Mathf.Max(20f, transform.position.y * 1.8f);
            _zoomVelocity -= Mathf.Sign(scrollDelta) * impulse;
        }

        private void ApplyZoomSmooth()
        {
            if (Mathf.Approximately(_zoomVelocity, 0f))
            {
                return;
            }

            float prevY = transform.position.y;
            Vector3 pos = transform.position;
            pos.y += _zoomVelocity * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);

            // 경계에 닿으면 속도 제거
            if (pos.y <= minHeight || pos.y >= maxHeight)
            {
                _zoomVelocity = 0f;
            }

            float deltaY = pos.y - prevY;
            transform.position = pos;

            // 지수 감쇠 — 약 0.25초 안에 속도가 거의 0으로
            _zoomVelocity *= Mathf.Pow(0.003f, Time.deltaTime);

            // 커서 방향 이동
            if (!zoomTowardCursor || Mathf.Approximately(deltaY, 0f))
            {
                return;
            }

            Camera cam = GetAttachedCamera();
            if (cam == null)
            {
                return;
            }

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));
            Plane ground = new(Vector3.up, Vector3.zero);
            if (!ground.Raycast(ray, out float enter))
            {
                return;
            }

            // 커서 아래 지점이 화면에 고정되도록 카메라 XZ 보정
            // newCamXZ = cursorXZ + (oldCamXZ - cursorXZ) * (newY / oldY)
            Vector3 cursorGround = ray.GetPoint(enter);
            Vector3 camToCursor = transform.position - cursorGround;
            camToCursor.y = 0f;
            transform.position += camToCursor * (deltaY / Mathf.Max(prevY, 0.001f));
        }

        private float EvaluateZoomMoveMultiplier()
        {
            if (maxHeight <= minHeight)
            {
                return 1f;
            }

            float zoomT = Mathf.InverseLerp(minHeight, maxHeight, transform.position.y);
            return Mathf.Lerp(zoomedInMoveMultiplier, zoomedOutMoveMultiplier, zoomT);
        }

        private void ClampPosition()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, xBounds.x, xBounds.y);
            position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
            position.z = Mathf.Clamp(position.z, zBounds.x, zBounds.y);
            transform.position = position;
        }

        public void SnapToWorldPoint(Vector3 worldPoint)
        {
            Vector3 position = transform.position;
            position.x = worldPoint.x;
            position.z = worldPoint.z;
            transform.position = position;
            ClampPosition();
        }

        public void CenterViewOnWorldPoint(Vector3 worldPoint, float groundY = 0f)
        {
            Camera sourceCamera = GetAttachedCamera();
            if (sourceCamera == null)
            {
                SnapToWorldPoint(worldPoint);
                return;
            }

            Plane groundPlane = new(Vector3.up, new Vector3(0f, groundY, 0f));
            Ray centerRay = sourceCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            bool hasAnyHit = false;
            Vector3 currentFocusPoint = SampleGroundPoint(centerRay, groundPlane, transform.position.y, groundY, ref hasAnyHit);
            if (!hasAnyHit)
            {
                SnapToWorldPoint(worldPoint);
                return;
            }

            Vector3 delta = worldPoint - currentFocusPoint;
            delta.y = 0f;
            transform.position += delta;
            ClampPosition();
        }

        private void HandleQuickFocusHotkeys()
        {
            if (Keyboard.current.homeKey.wasPressedThisFrame)
            {
                BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
                if (playerBase != null && playerBase.IsAlive)
                {
                    CenterViewOnWorldPoint(playerBase.transform.position);
                }

                return;
            }

            if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                return;
            }

            Vector3 focusPoint = ResolvePriorityFocusPoint();
            CenterViewOnWorldPoint(focusPoint);
        }

        private Vector3 ResolvePriorityFocusPoint()
        {
            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            BattleDirectiveController directiveController = BattleDirectiveController.Instance;
            ProductionStructure threatenedProduction = ResolveThreatenedProduction();

            if (playerBase != null && playerBase.IsAlive && playerBase.IsDefenseEmergency)
            {
                return playerBase.transform.position;
            }

            ControlNode urgentNode = null;
            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null || !node.IsPlayerRecaptureEmergency)
                {
                    continue;
                }

                if (urgentNode == null
                    || node.StrategicWeight > urgentNode.StrategicWeight
                    || node.CaptureProgressNormalized > urgentNode.CaptureProgressNormalized)
                {
                    urgentNode = node;
                }
            }

            if (urgentNode != null)
            {
                if (threatenedProduction != null && threatenedProduction.IsAlive)
                {
                    Vector3 linkedFocus = Vector3.Lerp(threatenedProduction.transform.position, urgentNode.transform.position, 0.58f);
                    linkedFocus.y = 0f;
                    return linkedFocus;
                }

                return urgentNode.transform.position;
            }

            if (threatenedProduction != null && threatenedProduction.IsAlive)
            {
                return threatenedProduction.transform.position;
            }

            if (directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Enemy) && playerBase != null && playerBase.IsAlive)
            {
                return playerBase.transform.position;
            }

            if (directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Player) && enemyBase != null && enemyBase.IsAlive)
            {
                return enemyBase.transform.position;
            }

            ControlNode priorityNode = null;
            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null || node.OwnerTeam == UnitTeam.Player)
                {
                    continue;
                }

                if (priorityNode == null || node.StrategicWeight > priorityNode.StrategicWeight)
                {
                    priorityNode = node;
                }
            }

            if (priorityNode != null)
            {
                return priorityNode.transform.position;
            }

            if (playerBase != null && enemyBase != null)
            {
                return Vector3.Lerp(playerBase.transform.position, enemyBase.transform.position, 0.38f);
            }

            return transform.position;
        }

        private static ProductionStructure ResolveThreatenedProduction()
        {
            ProductionStructure threatenedProduction = null;

            foreach (ProductionStructure structure in PrototypeRuntimeQuery.FindPlayerProductionStructures())
            {
                if (structure == null || !structure.IsAlive || !structure.IsThreatened)
                {
                    continue;
                }

                if (threatenedProduction == null
                    || structure.IsThreatEmergency && !threatenedProduction.IsThreatEmergency
                    || structure.NearbyHostileCount > threatenedProduction.NearbyHostileCount
                    || structure.NearbyThreatPressure > threatenedProduction.NearbyThreatPressure)
                {
                    threatenedProduction = structure;
                }
            }

            return threatenedProduction;
        }

        public bool TryGetViewportGroundCorners(Vector3[] cornersBuffer, float groundY = 0f)
        {
            if (cornersBuffer == null || cornersBuffer.Length < 4)
            {
                return false;
            }

            Camera sourceCamera = GetAttachedCamera();
            if (sourceCamera == null)
            {
                return false;
            }

            Plane groundPlane = new(Vector3.up, new Vector3(0f, groundY, 0f));
            bool hasAnyHit = false;

            cornersBuffer[0] = SampleGroundPoint(sourceCamera.ViewportPointToRay(new Vector3(0f, 0f, 0f)), groundPlane, transform.position.y, groundY, ref hasAnyHit);
            cornersBuffer[1] = SampleGroundPoint(sourceCamera.ViewportPointToRay(new Vector3(0f, 1f, 0f)), groundPlane, transform.position.y, groundY, ref hasAnyHit);
            cornersBuffer[2] = SampleGroundPoint(sourceCamera.ViewportPointToRay(new Vector3(1f, 1f, 0f)), groundPlane, transform.position.y, groundY, ref hasAnyHit);
            cornersBuffer[3] = SampleGroundPoint(sourceCamera.ViewportPointToRay(new Vector3(1f, 0f, 0f)), groundPlane, transform.position.y, groundY, ref hasAnyHit);
            return hasAnyHit;
        }

        public bool TryGetViewportGroundCorners(out Vector3[] corners, float groundY = 0f)
        {
            corners = defaultViewportCornerBuffer;
            return TryGetViewportGroundCorners(corners, groundY);
        }

        private bool ShouldIgnoreEdgePan(Vector2 mousePosition)
        {
            return PrototypeHudLayoutUtility.IsScreenPositionOverInteractiveHud(mousePosition);
        }

        private static Vector3 SampleGroundPoint(Ray ray, Plane groundPlane, float cameraHeight, float groundY, ref bool hasAnyHit)
        {
            if (groundPlane.Raycast(ray, out float enter))
            {
                hasAnyHit = true;
                return ray.GetPoint(enter);
            }

            float fallbackDistance = Mathf.Max(cameraHeight * 2f, 400f);
            Vector3 fallbackPoint = ray.origin + ray.direction * fallbackDistance;
            fallbackPoint.y = groundY;
            return fallbackPoint;
        }

        private Camera GetAttachedCamera()
        {
            if (attachedCamera == null)
            {
                attachedCamera = GetComponent<Camera>();
            }

            return attachedCamera;
        }
    }
}

