using UnityEngine;
using UnityEngine.AI;

namespace Game.Units
{
    /// <summary>
    /// Mecanim 파라미터 동기 — Speed / InCombat / Die 등. Animator 없으면 무시.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitAnimationDriver : MonoBehaviour
    {
        private SelectableUnit ownerUnit;
        private Animator animator;
        private SimpleUnitMover mover;
        private NavMeshAgent agent;
        private UnitHealth health;
        private UnitCombat combat;
        private Vector3 lastPosition;
        private AnimCombatProfileData profile;
        private bool initialized;

        /// <summary>스폰 직후 <see cref="SelectableUnit.Initialize"/> 이후 호출</summary>
        public void InitializeAfterSpawn(SelectableUnit unit)
        {
            ownerUnit = unit;
            mover = GetComponent<SimpleUnitMover>();
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<UnitHealth>();
            combat = GetComponent<UnitCombat>();
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            lastPosition = transform.position;
            profile = unit != null && unit.Definition != null
                ? unit.Definition.AnimCombatProfile
                : AnimCombatProfileData.CreateDefault();
            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized || animator == null || !animator.isActiveAndEnabled)
            {
                return;
            }

            if (health != null && !health.IsAlive)
            {
                if (!string.IsNullOrEmpty(profile.speedFloatParam))
                {
                    animator.SetFloat(profile.speedFloatParam, 0f);
                }

                SafeSetBool(profile.inCombatBoolParam, false);
                return;
            }

            float speedParam = 0f;
            if (agent != null && agent.isOnNavMesh)
            {
                Vector3 v = agent.velocity;
                v.y = 0f;
                float maxS = Mathf.Max(0.01f, mover != null ? mover.MoveSpeed : 6f);
                speedParam = Mathf.Clamp01(v.magnitude / maxS);
            }
            else
            {
                Vector3 delta = transform.position - lastPosition;
                delta.y = 0f;
                float maxS = Mathf.Max(0.01f, mover != null ? mover.MoveSpeed : 6f);
                float dt = Time.deltaTime;
                speedParam = dt > 0.0001f
                    ? Mathf.Clamp01(delta.magnitude / (maxS * dt))
                    : 0f;
            }

            lastPosition = transform.position;

            if (!string.IsNullOrEmpty(profile.speedFloatParam))
            {
                animator.SetFloat(profile.speedFloatParam, speedParam);
            }

            bool inCombat = combat != null && combat.CurrentTarget != null && combat.CurrentTarget.IsAlive;
            SafeSetBool(profile.inCombatBoolParam, inCombat);
        }

        /// <summary>공격 스윙 시작 — Attack 트리거</summary>
        public void NotifyAttackSwing()
        {
            if (animator == null || string.IsNullOrEmpty(profile.attackTriggerParam))
            {
                return;
            }

            animator.SetTrigger(profile.attackTriggerParam);
        }

        /// <summary>사망 시 호출(외부에서)</summary>
        public void NotifyDeath()
        {
            if (animator == null || string.IsNullOrEmpty(profile.dieTriggerParam))
            {
                return;
            }

            animator.SetTrigger(profile.dieTriggerParam);
        }

        /// <summary>Die 트리거 후 파괴를 잠시 미룰지 — Mecanim+지연값 있을 때만</summary>
        public bool TryGetDeferredDeathDestroyDelay(out float delayUnscaled)
        {
            delayUnscaled = 0f;
            if (!initialized || animator == null || animator.runtimeAnimatorController == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(profile.dieTriggerParam))
            {
                return false;
            }

            if (profile.deathDestroyDelayUnscaled <= 0.02f)
            {
                return false;
            }

            delayUnscaled = profile.deathDestroyDelayUnscaled;
            return true;
        }

        private void SafeSetBool(string paramName, bool value)
        {
            if (animator == null || string.IsNullOrEmpty(paramName))
            {
                return;
            }

            animator.SetBool(paramName, value);
        }
    }
}
