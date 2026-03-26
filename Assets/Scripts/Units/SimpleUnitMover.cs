using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Lightweight point-to-point movement used for the first gameplay prototype.
    /// </summary>
    public class SimpleUnitMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 540f;
        [SerializeField] private float stoppingDistance = 0.15f;

        private bool hasDestination;
        private Vector3 destination;

        public bool IsMoving => hasDestination;
        public float MoveSpeed => moveSpeed;

        private void Update()
        {
            if (!hasDestination)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 flatDestination = new(destination.x, currentPosition.y, destination.z);
            Vector3 toDestination = flatDestination - currentPosition;

            if (toDestination.sqrMagnitude <= stoppingDistance * stoppingDistance)
            {
                hasDestination = false;
                return;
            }

            Vector3 moveDirection = toDestination.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(currentPosition, flatDestination, moveSpeed * Time.deltaTime);
        }

        public void Configure(float newMoveSpeed, float newRotationSpeed, float newStoppingDistance)
        {
            moveSpeed = newMoveSpeed;
            rotationSpeed = newRotationSpeed;
            stoppingDistance = newStoppingDistance;
        }

        public void SetDestination(Vector3 targetPosition)
        {
            destination = targetPosition;
            hasDestination = true;
        }

        public void Stop()
        {
            hasDestination = false;
        }
    }
}
