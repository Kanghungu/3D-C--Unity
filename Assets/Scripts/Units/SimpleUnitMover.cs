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
        [SerializeField] private float movementEffectInterval = 0.16f;

        private readonly Collider[] separationBuffer = new Collider[24];
        private bool hasDestination;
        private Vector3 destination;
        private Vector3 cachedSeparationOffset;
        private float separationRefreshTimer;
        private float movementEffectTimer;
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
                movementEffectTimer = 0f;
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

            movementEffectTimer -= Time.deltaTime;
            if (movementEffectTimer <= 0f)
            {
                movementEffectTimer = movementEffectInterval;
                SpawnMovementEffect(finalDirection, effectiveMoveSpeed);
            }
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
            movementEffectTimer = 0f;
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

        private void SpawnMovementEffect(Vector3 moveDirection, float effectiveMoveSpeed)
        {
            if (selectableUnit == null || moveDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            bool isFlying = selectableUnit.Definition != null && selectableUnit.Definition.IsFlying;
            Color effectColor = selectableUnit.Team == UnitTeam.Player
                ? (isFlying ? new Color(0.3f, 0.86f, 1f) : new Color(0.78f, 0.72f, 0.54f))
                : (isFlying ? new Color(1f, 0.48f, 0.22f) : new Color(0.72f, 0.46f, 0.28f));

            GameObject effectObject = GameObject.CreatePrimitive(isFlying ? PrimitiveType.Sphere : PrimitiveType.Cylinder);
            effectObject.name = isFlying ? "Move Wake" : "Move Dust";
            effectObject.transform.position = transform.position
                + (isFlying ? new Vector3(0f, selectableUnit.Definition.HoverHeight * 0.45f, 0f) : new Vector3(0f, -0.38f, 0f))
                - moveDirection.normalized * (isFlying ? 0.34f : 0.2f);
            effectObject.transform.localScale = isFlying
                ? Vector3.one * Mathf.Lerp(0.12f, 0.22f, Mathf.Clamp01(effectiveMoveSpeed / 12f))
                : new Vector3(0.18f, 0.05f, 0.18f);

            Collider effectCollider = effectObject.GetComponent<Collider>();
            if (effectCollider != null)
            {
                effectCollider.enabled = false;
            }

            Renderer rendererComponent = effectObject.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = effectColor;
            }

            TimedWorldEffect effect = effectObject.AddComponent<TimedWorldEffect>();
            effect.Configure(
                isFlying ? 0.34f : 0.48f,
                isFlying ? Vector3.one * 0.42f : new Vector3(0.42f, 0.03f, 0.42f),
                isFlying ? -moveDirection.normalized * 1.1f : new Vector3(0f, 0.28f, 0f));
        }
    }
}


