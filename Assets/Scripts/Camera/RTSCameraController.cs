using UnityEngine;
using UnityEngine.InputSystem;
using Game.BattleAces;
using Game.Selection;
using Game.Prototype;
using Game.Settings;
using Game.Units;

namespace Game.CameraSystem
{
    /// <summary>
    /// Battle Aces / 프로토타입 공용 RTS 카메라.
    /// 브리핑 중: <see cref="BattleMissionFlow.IsBriefingBlocking"/> 일 때 Space 는 교전 포커스가 아니라
    /// 작전 시작에 쓰이므로, <see cref="HandleQuickFocusHotkeys"/> 에서 Space 포커스를 막음(문구는 <c>DemoPresentationCopy.BriefingContinueFooterKo</c>).
    /// </summary>
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 260f;
        [SerializeField] private float fastMoveMultiplier = 3.8f;
        [SerializeField] private float edgePanSize = 16f;
        [SerializeField] private float zoomedInMoveMultiplier = 0.85f;
        [SerializeField] private float zoomedOutMoveMultiplier = 2.35f;

        [Header("Zoom")]
        [SerializeField] private float minHeight = 3f;
        [SerializeField] private float maxHeight = 820f;
        [SerializeField] private bool zoomTowardCursor = true;

        // SmoothDamp 대신 관성 감쇠로 줌 속도 누적
        private float _zoomVelocity;

        // 유닛 미선택 시 우드래그로 지면 패닝
        private bool _rightDragActive;
        private bool _rightDragPanning;
        private Vector2 _rightDragStartScreenPos;
        private Vector3 _rightDragGrabPoint;

        public bool IsRightDragPanning => _rightDragPanning;

        [Header("Combat Camera Shake")]
        [Tooltip("Maximum XZ world-space offset added by accumulated combat shake impulses.")]
        [SerializeField] private float combatShakeMaxWorldOffset = 0.92f;

        [Tooltip("Higher values make combat shake settle faster in unscaled time.")]
        [SerializeField] private float combatShakeDecayPerSecond = 3.8f;

        private float combatShakeStrength;

        [Header("Bounds")]
        [SerializeField] private Vector2 xBounds = new(-1600f, 1600f);
        [SerializeField] private Vector2 zBounds = new(-1600f, 1600f);
        [SerializeField] private float mapBoundsExtension = 240f;
        private readonly Vector3[] defaultViewportCornerBuffer = new Vector3[4];
        private Camera attachedCamera;

        private float sensitivityMul = 1f;

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

        /// <summary>맵 경계 — 카메라 XZ 위치를 이 사각 안으로 클램프</summary>
        public void SetWorldXZBounds(float minX, float maxX, float minZ, float maxZ)
        {
            if (minX > maxX)
            {
                (minX, maxX) = (maxX, minX);
            }

            if (minZ > maxZ)
            {
                (minZ, maxZ) = (maxZ, minZ);
            }

            xBounds = new Vector2(minX, maxX);
            zBounds = new Vector2(minZ, maxZ);
            ClampPosition();
        }

        /// <summary>카메라 높이(y) 최소·최대 — 줌 한계</summary>
        public void SetHeightClamp(float minGroundHeight, float maxGroundHeight)
        {
            minHeight = Mathf.Max(2f, minGroundHeight);
            maxHeight = Mathf.Max(minHeight + 4f, maxGroundHeight);
            ClampPosition();
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

            sensitivityMul = Mathf.Clamp(GameUserSettings.CameraSensitivityMultiplier, 0.35f, 2.5f);

            HandleQuickFocusHotkeys();
            HandleRightDragPan();
            HandleMovement();
            HandleZoom();
            ApplyZoomSmooth();
            ClampPosition();
        }

        /// <summary>명중·명령·승패 등 — 0~1 impulse, LateUpdate 에서 XZ 노이즈로 소모</summary>
        public void AddCombatShake(float impulse01)
        {
            impulse01 = Mathf.Clamp01(impulse01);
            combatShakeStrength = Mathf.Clamp01(combatShakeStrength + impulse01 * 0.92f);
        }

        private void LateUpdate()
        {
            ApplyCombatShakeFrame();
        }

        private void ApplyCombatShakeFrame()
        {
            if (combatShakeStrength <= 0.001f)
            {
                return;
            }

            float w = combatShakeStrength * combatShakeMaxWorldOffset;
            float n1 = Mathf.PerlinNoise(Time.unscaledTime * 38.7f, 1.718f) - 0.5f;
            float n2 = Mathf.PerlinNoise(2.31f, Time.unscaledTime * 38.7f) - 0.5f;
            transform.position += new Vector3(n1 * 2f * w, 0f, n2 * 2f * w);
            ClampPosition();
            combatShakeStrength = Mathf.MoveTowards(
                combatShakeStrength,
                0f,
                Time.unscaledDeltaTime * combatShakeDecayPerSecond);
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
                // 유닛 선택 중이면 우클릭은 명령용 — 카메라 드래그 시작 안 함
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

            // 작은 흔들림은 무시 — 임계 넘기면 패닝 모드
            if (!_rightDragPanning && Vector2.Distance(currentScreenPos, _rightDragStartScreenPos) > 6f)
            {
                _rightDragPanning = true;
            }

            if (!_rightDragPanning)
            {
                return;
            }

            // 잡은 지점이 화면과 함께 움직이도록 역방향 패닝
            Ray currentRay = cam.ScreenPointToRay(new Vector3(currentScreenPos.x, currentScreenPos.y, 0f));
            Plane groundPlane = new(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(currentRay, out float dist))
            {
                Vector3 currentGroundPoint = currentRay.GetPoint(dist);
                Vector3 pan = _rightDragGrabPoint - currentGroundPoint;
                pan.y = 0f;
                transform.position += pan * sensitivityMul;
            }
        }

        private void HandleMovement()
        {
            // 우드래그 패닝 중에는 WASD·가장자리 이동 생략
            if (_rightDragPanning)
            {
                return;
            }

            Vector3 inputDirection = Vector3.zero;
            // Ctrl 누른 채 WASD 는 덱 단축(1~8)과 겹치므로 카메라 이동 차단
            bool ctrlBlocksWasdPan =
                Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

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

            // WASD 패닝(Q/E 회전 없음 — 덱 키와 분리)
            if (!ctrlBlocksWasdPan)
            {
                if (Keyboard.current.wKey.isPressed)
                {
                    inputDirection += Vector3.forward;
                }

                if (Keyboard.current.sKey.isPressed)
                {
                    inputDirection += Vector3.back;
                }

                if (Keyboard.current.dKey.isPressed)
                {
                    inputDirection += Vector3.right;
                }

                if (Keyboard.current.aKey.isPressed)
                {
                    inputDirection += Vector3.left;
                }
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

            float currentMoveSpeed = moveSpeed * EvaluateZoomMoveMultiplier() * sensitivityMul;

            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            {
                currentMoveSpeed *= fastMoveMultiplier;
            }

            Vector3 movement = (forward * inputDirection.z + right * inputDirection.x) * currentMoveSpeed * Time.deltaTime;
            transform.position += movement;
        }

        private void HandleZoom()
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Approximately(scrollDelta, 0f))
            {
                return;
            }

            // 높이에 비례한 스크롤 임펄스 — 고도에서도 줌 체감 유지
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

            // 한계에 닿으면 관성 제거
            if (pos.y <= minHeight || pos.y >= maxHeight)
            {
                _zoomVelocity = 0f;
            }

            float deltaY = pos.y - prevY;
            transform.position = pos;

            // 지수 감쇠로 관성 소멸
            _zoomVelocity *= Mathf.Pow(0.003f, Time.deltaTime);

            // 커서 기준 줌(옵션) — 피벗 유지
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

            // 줌 시 커서 아래 지점을 화면에 고정 — 유사 삼각형으로 XZ 보정
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

        /// <summary>
        /// 스토어 캡처·연출용 — 높이를 먼저 맞춘 뒤 화면 중앙이 월드 초점을 보도록 이동.
        /// </summary>
        public void ApplyPresentationView(Vector3 worldFocusPoint, float cameraHeightWorldY)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(cameraHeightWorldY, minHeight, maxHeight);
            transform.position = pos;
            CenterViewOnWorldPoint(worldFocusPoint, 0f);
        }

        /// <summary>월드 XZ 평면상 델타만큼 카메라 이동</summary>
        public void PanWorldDeltaXZ(Vector3 deltaWorldXZ)
        {
            deltaWorldXZ.y = 0f;
            transform.position += deltaWorldXZ;
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
                    return;
                }

                // Battle Aces — BaseStructure 없을 때 지휘 코어로 폴백
                if (BattleAcesCore.TryFindAliveCore(UnitTeam.Player, out BattleAcesCore baPlayerCore))
                {
                    CenterViewOnWorldPoint(baPlayerCore.transform.position);
                }

                return;
            }

            if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                return;
            }

            // 브리핑 중 Space 는 작전 개시용 — 여기서는 카메라 포커스 금지
            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
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

            // 프로토 거점 없음 — 양 코어 중점으로 시야(Battle Aces)
            if (BattleAcesCore.TryFindAliveCore(UnitTeam.Player, out BattleAcesCore baPlayer)
                && BattleAcesCore.TryFindAliveCore(UnitTeam.Enemy, out BattleAcesCore baEnemy))
            {
                return Vector3.Lerp(baPlayer.transform.position, baEnemy.transform.position, 0.38f);
            }

            if (BattleAcesCore.TryFindAliveCore(UnitTeam.Player, out BattleAcesCore baPOnly))
            {
                return baPOnly.transform.position;
            }

            if (BattleAcesCore.TryFindAliveCore(UnitTeam.Enemy, out BattleAcesCore baEOnly))
            {
                return baEOnly.transform.position;
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

