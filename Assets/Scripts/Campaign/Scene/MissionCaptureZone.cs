using System.Collections.Generic;
using Game.Prototype;
using Game.Units;
using UnityEngine;

namespace Game.Campaign.Scene
{
    /// <summary>
    /// 캠페인 점령 구역 — 아군이 안에 있을 때만 홀드 타이머 진행.
    /// 플레이어가 ‘거점 정령’ 느낌을 받도록 약한 체력 회복을 줄 수 있음(0이면 없음).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MissionCaptureZone : MonoBehaviour
    {
        private static readonly HashSet<SelectableUnit> OccupyingUnits = new();

        [SerializeField] private float holdSecondsRequired = 14f;

        [SerializeField]
        [Tooltip("구역 안 아군 초당 체력 회복. 0이면 회복 없음(순수 타이머만).")]
        private float occupantHealPerSecond = 3.5f;

        private float holdTimer;
        private bool completed;
        private Collider zoneCollider;

        public static MissionCaptureZone Instance { get; private set; }

        public bool IsCompleted => completed;
        public float HoldProgress01 => completed ? 1f : Mathf.Clamp01(holdTimer / Mathf.Max(0.01f, holdSecondsRequired));
        public float HoldSecondsRequired => Mathf.Max(1f, holdSecondsRequired);
        public int OccupyingPlayerUnitCount => OccupyingUnits.Count;
        public bool IsPlayerInside => OccupyingUnits.Count > 0;

        /// <summary>UI·툴팁용 — 회복이 켜져 있는지</summary>
        public float OccupantHealPerSecond => Mathf.Max(0f, occupantHealPerSecond);

        private void OnEnable()
        {
            Instance = this;
            OccupyingUnits.Clear();
            zoneCollider = GetComponent<Collider>();
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            OccupyingUnits.Clear();
        }

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }

        private void Update()
        {
            if (completed)
            {
                return;
            }

            RefreshOccupyingUnits();

            if (OccupyingUnits.Count == 0)
            {
                holdTimer = 0f;
                return;
            }

            ApplyOccupantHeal(Time.deltaTime);

            holdTimer += Time.deltaTime;
            if (holdTimer >= holdSecondsRequired)
            {
                completed = true;
            }
        }

        private void RefreshOccupyingUnits()
        {
            OccupyingUnits.Clear();

            if (zoneCollider == null)
            {
                return;
            }

            IReadOnlyList<SelectableUnit> units = PrototypeRuntimeRegistry.GetSelectableUnits();
            for (int i = 0; i < units.Count; i++)
            {
                SelectableUnit unit = units[i];
                if (unit == null || unit.Team != UnitTeam.Player)
                {
                    continue;
                }

                CombatTarget target = unit.GetComponent<CombatTarget>();
                if (target != null && !target.IsAlive)
                {
                    continue;
                }

                if (RelicMissionObject.IsPointInsideCollider(zoneCollider, unit.transform.position))
                {
                    OccupyingUnits.Add(unit);
                }
            }
        }

        public void ConfigureHoldSeconds(float seconds)
        {
            holdSecondsRequired = Mathf.Max(1f, seconds);
        }

        /// <summary>구역 안 살아 있는 아군에게만 소량 회복 — 전투 밸런스는 낮은 값 유지</summary>
        private void ApplyOccupantHeal(float deltaTime)
        {
            if (occupantHealPerSecond <= 0f || deltaTime <= 0f)
            {
                return;
            }

            float amount = occupantHealPerSecond * deltaTime;
            if (amount <= 0f)
            {
                return;
            }

            foreach (SelectableUnit unit in OccupyingUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                CombatTarget target = unit.GetComponent<CombatTarget>();
                if (target == null || target.Health == null || !target.Health.IsAlive)
                {
                    continue;
                }

                if (target.Health.Normalized >= 0.999f)
                {
                    continue;
                }

                target.Health.Heal(amount);
            }
        }
    }
}
