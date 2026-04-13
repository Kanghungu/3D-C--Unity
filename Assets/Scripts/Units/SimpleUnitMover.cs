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

        /// <summary>비행 유닛 수평 속도 스무딩용</summary>
        private float flyHorizontalSpeedCurrent;

        private UnitAbilityState abilityState;
        private AdvancedUnitRoleController roleController;
        private SelectableUnit selectableUnit;
        private NavMeshAgent navMeshAgent;

        /// <summary>애니 선딜 등 — 1 미만이면 이동 속도만 일시적으로 줄임</summary>
        private float externalSpeedMultiplier = 1f;

        public bool IsMoving => hasDestination;
        public float MoveSpeed => moveSpeed;

        public void SetExternalSpeedMultiplier(float multiplier)
        {
            externalSpeedMultiplier = Mathf.Clamp(multiplier, 0.12f, 1f);
        }

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
            float passiveSpeedMult = roleController != null ? roleController.GetPassiveMoveSpeedMultiplier() : 1f;
            navMeshAgent.speed = moveSpeed * speedMult * passiveSpeedMult * externalSpeedMultiplier;

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
                flyHorizontalSpeedCurrent = 0f;
                return;
            }

            float speedMult = abilityState != null ? abilityState.GetMoveSpeedMultiplier() : 1f;
            float passiveSpeedMult = roleController != null ? roleController.GetPassiveMoveSpeedMultiplier() : 1f;
            float effectiveSpeed = moveSpeed * speedMult * passiveSpeedMult * externalSpeedMultiplier;
            float flyAccel = 20f;
            if (selectableUnit != null && selectableUnit.Definition != null)
            {
                float fa = selectableUnit.Definition.MoveProfile.flyHorizontalAcceleration;
                if (fa > 0.5f)
                {
                    flyAccel = fa;
                }
            }

            flyHorizontalSpeedCurrent = Mathf.MoveTowards(flyHorizontalSpeedCurrent, effectiveSpeed, flyAccel * Time.deltaTime);
            Vector3 dir = toDest.normalized;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            Vector3 next = current + dir * (flyHorizontalSpeedCurrent * Time.deltaTime);
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

        /// <summary>UnitDefinition MoveProfile 로 NavMeshAgent 튜닝</summary>
        public void ApplyMoveProfileNav(MoveProfileData profile)
        {
            if (navMeshAgent == null || !profile.applyToNavAgent)
            {
                return;
            }

            if (profile.navAcceleration > 0.5f)
            {
                navMeshAgent.acceleration = profile.navAcceleration;
            }

            if (profile.navAngularSpeedDeg > 1f)
            {
                navMeshAgent.angularSpeed = profile.navAngularSpeedDeg;
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
            flyHorizontalSpeedCurrent = 0f;

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
            UnitArchetype archetype = selectableUnit.Archetype;
            PrimitiveType effectPrimitive = isFlying ? PrimitiveType.Sphere : PrimitiveType.Cylinder;
            string effectName = isFlying ? "Move Wake" : "Move Dust";
            Color effectColor = selectableUnit.Team == UnitTeam.Player
                ? (isFlying ? new Color(0.3f, 0.86f, 1f) : new Color(0.78f, 0.72f, 0.54f))
                : (isFlying ? new Color(1f, 0.48f, 0.22f) : new Color(0.72f, 0.46f, 0.28f));
            Vector3 spawnOffset = isFlying
                ? new Vector3(0f, selectableUnit.Definition.HoverHeight * 0.45f, 0f)
                : new Vector3(0f, -0.38f, 0f);
            float backOffset = isFlying ? 0.34f : 0.2f;
            Vector3 effectScale = isFlying
                ? Vector3.one * Mathf.Lerp(0.12f, 0.22f, Mathf.Clamp01(effectiveMoveSpeed / 12f))
                : new Vector3(0.18f, 0.05f, 0.18f);
            float duration = isFlying ? 0.34f : 0.48f;
            Vector3 endScale = isFlying ? Vector3.one * 0.42f : new Vector3(0.42f, 0.03f, 0.42f);
            Vector3 drift = isFlying ? -moveDirection.normalized * 1.1f : new Vector3(0f, 0.28f, 0f);

            switch (archetype)
            {
                case UnitArchetype.SpecialWarrior:
                    effectPrimitive = PrimitiveType.Cube;
                    effectName = "Phase Wake";
                    effectColor = selectableUnit.Team == UnitTeam.Player
                        ? new Color(0.38f, 0.94f, 1f)
                        : new Color(1f, 0.62f, 0.28f);
                    spawnOffset = new Vector3(0f, -0.16f, 0f);
                    backOffset = 0.3f;
                    effectScale = new Vector3(0.12f, 0.12f, 0.26f);
                    duration = 0.28f;
                    endScale = new Vector3(0.04f, 0.04f, 0.5f);
                    drift = -moveDirection.normalized * 1.6f + new Vector3(0f, 0.24f, 0f);
                    break;
                case UnitArchetype.RoyalGuard:
                    effectPrimitive = PrimitiveType.Cylinder;
                    effectName = "Guard Sigil";
                    effectColor = selectableUnit.Team == UnitTeam.Player
                        ? new Color(0.82f, 0.92f, 1f)
                        : new Color(1f, 0.76f, 0.34f);
                    spawnOffset = new Vector3(0f, -0.3f, 0f);
                    backOffset = 0f;
                    effectScale = new Vector3(0.24f, 0.04f, 0.24f);
                    duration = 0.42f;
                    endScale = new Vector3(0.46f, 0.02f, 0.46f);
                    drift = new Vector3(0f, 0.1f, 0f);
                    break;
                case UnitArchetype.Outrider:
                    effectPrimitive = PrimitiveType.Sphere;
                    effectName = "Hover Wake";
                    effectColor = selectableUnit.Team == UnitTeam.Player
                        ? new Color(0.32f, 0.92f, 1f)
                        : new Color(1f, 0.58f, 0.26f);
                    spawnOffset = new Vector3(0f, 0.08f, 0f);
                    backOffset = 0.42f;
                    effectScale = Vector3.one * Mathf.Lerp(0.12f, 0.2f, Mathf.Clamp01(effectiveMoveSpeed / 10f));
                    duration = 0.3f;
                    endScale = Vector3.one * 0.26f;
                    drift = -moveDirection.normalized * 1.45f;
                    break;
            }

            GameObject effectObject = GameObject.CreatePrimitive(effectPrimitive);
            effectObject.name = effectName;
            effectObject.transform.position = transform.position + spawnOffset - moveDirection.normalized * backOffset;
            effectObject.transform.localScale = effectScale;

            Collider effectCollider = effectObject.GetComponent<Collider>();
            if (effectCollider != null) effectCollider.enabled = false;

            Renderer rendererComponent = effectObject.GetComponent<Renderer>();
            if (rendererComponent != null) rendererComponent.material.color = effectColor;

            TimedWorldEffect effect = effectObject.AddComponent<TimedWorldEffect>();
            effect.Configure(duration, endScale, drift);
        }
    }
}
