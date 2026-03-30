using UnityEngine;
using UnityEngine.InputSystem;
using Game.Selection;
using Game.Prototype;

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

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 120f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 720f;
        [SerializeField] private float minHeight = 60f;
        [SerializeField] private float maxHeight = 820f;

        [Header("Bounds")]
        [SerializeField] private Vector2 xBounds = new(-1600f, 1600f);
        [SerializeField] private Vector2 zBounds = new(-1600f, 1600f);
        [SerializeField] private float mapBoundsExtension = 240f;

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
            zoomSpeed = Mathf.Max(zoomSpeed, longestSide * 0.36f);
            minHeight = Mathf.Max(70f, longestSide * 0.025f);
            maxHeight = Mathf.Max(maxHeight, longestSide * 0.48f);
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                return;
            }

            HandleMovement();
            HandleRotation();
            HandleZoom();
            ClampPosition();
        }

        private void HandleMovement()
        {
            Vector3 inputDirection = Vector3.zero;
            bool isAttackMoveChord = Keyboard.current.aKey.isPressed && Mouse.current.rightButton.isPressed;
            bool reserveAForOrders = Keyboard.current.aKey.isPressed
                && PrototypeSelectionController.Instance != null
                && PrototypeSelectionController.Instance.SelectedUnits.Count > 0;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                inputDirection += Vector3.forward;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                inputDirection += Vector3.back;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                inputDirection += Vector3.right;
            }

            if (!isAttackMoveChord && !reserveAForOrders && (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed))
            {
                inputDirection += Vector3.left;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();

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

            float currentMoveSpeed = moveSpeed;

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

            Vector3 position = transform.position;
            position.y -= scrollDelta * zoomSpeed * Time.deltaTime;
            transform.position = position;
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
    }
}

