using Game.Prototype;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Units
{
    /// <summary>
    /// NavMeshAgent 기반 유닛 이동. 장애물 우회 경로를 자동으로 탐색한다.
    /// </summary>
    public class SimpleUnitMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 540f;
        [SerializeField] private float stoppingDistance = 0.15f;
        [SerializeField] private float movementEffectInterval = 0.16f;

        private bool hasDestination;
        private Vector3 destination;
        private float movementEffectTimer;
        private UnitAbilityState abilityState;
        private AdvancedUnitRoleController roleController;
        private SelectableUnit selectableUnit;
        private NavMeshAgent navMeshAgent;

        public bool IsMoving => hasDestination;
        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            abilityState  = GetComponent<UnitAbilityState>();
            roleController = GetComponent<AdvancedUnitRoleController>();
            selectableUnit = GetComponent<SelectableUnit>();
            navMeshAgent   = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (roleController != null && !roleController.AllowsMovement())
            {
                if (navMeshAgent != null) navMeshAgent.isStopped = true;
                hasDestination = false;
                return;
            }

            if (!hasDestination)
            {
                movementEffectTimer = 0f;
                return;
            }

            if (navMeshAgent == null)
            {
                // 비행 유닛 — NavMesh 없이 직접 이동
                UpdateFlying();
                return;
            }

            if (!navMeshAgent.isOnNavMesh)
            {
                return;
            }

            // 버프/디버프에 따른 속도 반영
            float speedMult = abilityState != null ? abilityState.GetMoveSpeedMultiplier() : 1f;
            navMeshAgent.speed = moveSpeed * speedMult;

            // 도착 판정
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                hasDestination = false;
                navMeshAgent.isStopped = true;
                return;
            }

            // 이동 방향으로 회전
            Vector3 velocity = navMeshAgent.velocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // 이동 이펙트
            movementEffectTimer -= Time.deltaTime;
            if (movementEffectTimer <= 0f && velocity.sqrMagnitude > 0.01f)
            {
                movementEffectTimer = movementEffectInterval;
                SpawnMovementEffect(velocity.normalized, navMeshAgent.speed);
            }
        }

        private void UpdateFlying()
        {
            float hoverY = selectableUnit?.Definition != null ? selectableUnit.Definition.HoverHeight : 2f;
            Vector3 current = transform.position;
            Vector3 flatDest = new Vector3(destination.x, hoverY, destination.z);
            Vector3 toDest = flatDest - current;
            toDest.y = 0f;

            if (toDest.sqrMagnitude <= stoppingDistance * stoppingDistance)
            {
                hasDestination = false;
                return;
            }

            float speedMult = abilityState != null ? abilityState.GetMoveSpeedMultiplier() : 1f;
            float effectiveSpeed = moveSpeed * speedMult;
            Vector3 dir = toDest.normalized;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            Vector3 next = current + dir * (effectiveSpeed * Time.deltaTime);
            next.y = Mathf.Lerp(current.y, hoverY, Time.deltaTime * 5f);
            transform.position = next;

            movementEffectTimer -= Time.deltaTime;
            if (movementEffectTimer <= 0f)
            {
                movementEffectTimer = movementEffectInterval;
                SpawnMovementEffect(dir, effectiveSpeed);
            }
        }

        public void Configure(float newMoveSpeed, float newRotationSpeed, float newStoppingDistance)
        {
            moveSpeed        = newMoveSpeed;
            rotationSpeed    = newRotationSpeed;
            stoppingDistance = newStoppingDistance;

            if (navMeshAgent != null)
            {
                navMeshAgent.speed           = moveSpeed;
                navMeshAgent.stoppingDistance = stoppingDistance;
            }
        }

        public void SetDestination(Vector3 targetPosition)
        {
            if (roleController != null)
            {
                if (!roleController.AllowsMovement()) return;
                targetPosition = roleController.AdjustDestination(targetPosition);
            }

            destination    = targetPosition;
            hasDestination = true;

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(targetPosition);
            }
        }

        public void Stop()
        {
            hasDestination      = false;
            movementEffectTimer = 0f;

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = true;
            }
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
            if (effectCollider != null) effectCollider.enabled = false;

            Renderer rendererComponent = effectObject.GetComponent<Renderer>();
            if (rendererComponent != null) rendererComponent.material.color = effectColor;

            TimedWorldEffect effect = effectObject.AddComponent<TimedWorldEffect>();
            effect.Configure(
                isFlying ? 0.34f : 0.48f,
                isFlying ? Vector3.one * 0.42f : new Vector3(0.42f, 0.03f, 0.42f),
                isFlying ? -moveDirection.normalized * 1.1f : new Vector3(0f, 0.28f, 0f));
        }
    }
}
