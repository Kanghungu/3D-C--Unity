using Game.Prototype;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Units
{
    /// <summary>
    /// 같은 팀 유닛이 밀집할 때 아주 약하게 밀어내 가독성 보조 (NavMeshAgent.Move).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitCrowdSeparation : MonoBehaviour
    {
        private const int MaxNeighborContributors = 8;

        private float radius = 1.2f;
        private float pushPerSecond = 0.5f;
        private NavMeshAgent agent;
        private SelectableUnit selfUnit;

        public void Configure(float separationRadius, float pushStrengthPerSecond)
        {
            radius = Mathf.Max(0.35f, separationRadius);
            pushPerSecond = Mathf.Max(0f, pushStrengthPerSecond);
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            selfUnit = GetComponent<SelectableUnit>();
        }

        private void LateUpdate()
        {
            if (agent == null || !agent.isOnNavMesh || selfUnit == null || pushPerSecond <= 0.01f)
            {
                return;
            }

            Vector3 push = Vector3.zero;
            int count = 0;
            foreach (SelectableUnit other in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (other == null || other == selfUnit || !other.isActiveAndEnabled)
                {
                    continue;
                }

                if (other.Team != selfUnit.Team)
                {
                    continue;
                }

                Vector3 delta = transform.position - other.transform.position;
                delta.y = 0f;
                float d = delta.magnitude;
                if (d < 0.01f || d >= radius)
                {
                    continue;
                }

                float strength = (1f - d / radius) * pushPerSecond;
                push += delta.normalized * strength;
                count++;
                if (count >= MaxNeighborContributors)
                {
                    break;
                }
            }

            if (count == 0 || push.sqrMagnitude < 0.0001f)
            {
                return;
            }

            push = Vector3.ClampMagnitude(push, pushPerSecond);
            agent.Move(push * Time.deltaTime);
        }
    }
}
