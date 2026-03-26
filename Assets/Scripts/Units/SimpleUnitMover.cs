using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Lightweight point-to-point movement used for the first gameplay prototype.
    /// Includes simple separation to reduce unit overlap.
    /// </summary>
    public class SimpleUnitMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 540f;
        [SerializeField] private float stoppingDistance = 0.15f;
        [SerializeField] private float separationRadius = 1.1f;
        [SerializeField] private float separationStrength = 4.5f;

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
            Vector3 separationOffset = CalculateSeparationOffset();
            Vector3 finalDirection = (moveDirection + separationOffset).normalized;

            if (finalDirection.sqrMagnitude <= 0.0001f)
            {
                finalDirection = moveDirection;
            }

            Quaternion targetRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 nextPosition = currentPosition + finalDirection * (moveSpeed * Time.deltaTime);
            nextPosition.y = currentPosition.y;
            transform.position = nextPosition;
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

        private Vector3 CalculateSeparationOffset()
        {
            Vector3 offset = Vector3.zero;
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, separationRadius);

            foreach (Collider nearbyCollider in nearbyColliders)
            {
                if (nearbyCollider == null || nearbyCollider.transform == transform)
                {
                    continue;
                }

                if (!nearbyCollider.TryGetComponent(out SelectableUnit otherUnit))
                {
                    continue;
                }

                Vector3 away = transform.position - otherUnit.transform.position;
                away.y = 0f;
                float distance = away.magnitude;

                if (distance <= 0.001f)
                {
                    away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                    distance = Mathf.Max(away.magnitude, 0.001f);
                }

                float weight = 1f - Mathf.Clamp01(distance / separationRadius);
                offset += away.normalized * weight;
            }

            return offset * separationStrength;
        }
    }
}
