using Game.Prototype;
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
        [SerializeField] private float separationRadius = 1.45f;
        [SerializeField] private float separationStrength = 5.8f;
        [SerializeField] private float separationRefreshInterval = 0.08f;

        private readonly Collider[] separationBuffer = new Collider[24];
        private bool hasDestination;
        private Vector3 destination;
        private Vector3 cachedSeparationOffset;
        private float separationRefreshTimer;
        private UnitAbilityState abilityState;
        private AdvancedUnitRoleController roleController;
        private SelectableUnit selectableUnit;

        public bool IsMoving => hasDestination;
        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            abilityState = GetComponent<UnitAbilityState>();
            roleController = GetComponent<AdvancedUnitRoleController>();
            selectableUnit = GetComponent<SelectableUnit>();
        }

        private void Update()
        {
            if (roleController != null && !roleController.AllowsMovement())
            {
                hasDestination = false;
                return;
            }

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
                cachedSeparationOffset = Vector3.zero;
                return;
            }

            separationRefreshTimer -= Time.deltaTime;
            if (separationRefreshTimer <= 0f)
            {
                separationRefreshTimer = separationRefreshInterval;
                cachedSeparationOffset = CalculateSeparationOffset();
            }

            Vector3 moveDirection = toDestination.normalized;
            float arrivalBlend = Mathf.InverseLerp(stoppingDistance * 2.2f, separationRadius * 4.5f, toDestination.magnitude);
            Vector3 finalDirection = (moveDirection + cachedSeparationOffset * Mathf.Lerp(1.35f, 0.8f, arrivalBlend)).normalized;
            if (finalDirection.sqrMagnitude <= 0.0001f)
            {
                finalDirection = moveDirection;
            }

            Quaternion targetRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            float effectiveMoveSpeed = moveSpeed * (abilityState != null ? abilityState.GetMoveSpeedMultiplier() : 1f);
            Vector3 nextPosition = currentPosition + finalDirection * (effectiveMoveSpeed * Time.deltaTime);
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
            if (roleController != null)
            {
                if (!roleController.AllowsMovement())
                {
                    return;
                }

                targetPosition = roleController.AdjustDestination(targetPosition);
            }

            destination = targetPosition;
            hasDestination = true;
            separationRefreshTimer = 0f;
        }

        public void Stop()
        {
            hasDestination = false;
            cachedSeparationOffset = Vector3.zero;
        }

        private Vector3 CalculateSeparationOffset()
        {
            Vector3 offset = Vector3.zero;
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, separationRadius, separationBuffer);

            for (int index = 0; index < hitCount; index++)
            {
                Collider nearbyCollider = separationBuffer[index];
                if (nearbyCollider == null || nearbyCollider.transform == transform)
                {
                    continue;
                }

                if (!nearbyCollider.TryGetComponent(out SelectableUnit otherUnit))
                {
                    continue;
                }

                if (selectableUnit != null && otherUnit.Team != selectableUnit.Team)
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


