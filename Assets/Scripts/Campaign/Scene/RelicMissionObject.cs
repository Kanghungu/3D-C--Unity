using Game.Units;
using UnityEngine;

namespace Game.Campaign.Scene
{
    /// <summary>
    /// 성물/유물 — 호위·점령·파괴 미션의 손에 잡히는 목표물.
    /// 씬에 배치하고 ID를 미션 로직과 맞춘다.
    /// </summary>
    public sealed class RelicMissionObject : MonoBehaviour
    {
        [Header("식별")]
        [Tooltip("MissionDefinition·승패 로직과 매칭하는 ID")]
        [SerializeField] private string relicId = "relic_alpha";

        [Header("선택 — 체력이 있으면 CombatTarget과 연결")]
        [SerializeField] private CombatTarget combatTarget;

        [Header("호위/철수 — 목표 구역(트리거 콜라이더)")]
        [SerializeField] private Collider escortTargetZone;

        [SerializeField] private Collider evacuationZone;

        public string RelicId => relicId;
        public CombatTarget CombatTarget => combatTarget;
        public Collider EscortTargetZone => escortTargetZone;
        public Collider EvacuationZone => evacuationZone;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(relicId))
            {
                relicId = "relic_unset";
            }
        }

        /// <summary>유물이 파괴 가능한지(전투 타겟 없으면 마커만)</summary>
        public bool IsDestroyable()
        {
            return combatTarget != null && combatTarget.IsAlive;
        }

        /// <summary>런타임 생성 시 존 참조 설정</summary>
        public void AssignZones(Collider escortZone, Collider evacZone)
        {
            escortTargetZone = escortZone;
            evacuationZone = evacZone;
        }

        /// <summary>월드 좌표가 콜라이더 안에 있는지(트리거/일반 모두)</summary>
        public static bool IsPointInsideCollider(Collider col, Vector3 worldPoint)
        {
            if (col == null)
            {
                return false;
            }

            Vector3 closest = col.ClosestPoint(worldPoint);
            return (closest - worldPoint).sqrMagnitude < 0.25f;
        }

        public bool IsInsideEscortZone(Vector3 worldPoint)
        {
            return IsPointInsideCollider(escortTargetZone, worldPoint);
        }

        public bool IsInsideEvacuationZone(Vector3 worldPoint)
        {
            return IsPointInsideCollider(evacuationZone, worldPoint);
        }
    }
}
