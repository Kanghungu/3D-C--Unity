using System.Collections.Generic;
using Game.Prototype;
using Game.Units;
using UnityEngine;

namespace Game.Campaign.Scene
{
    /// <summary>
    /// Keeps track of player units inside the campaign capture zone.
    /// Uses direct position checks instead of trigger callbacks so mission progress
    /// stays reliable with NavMesh-driven prototype units.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MissionCaptureZone : MonoBehaviour
    {
        private static readonly HashSet<SelectableUnit> OccupyingUnits = new();

        [SerializeField] private float holdSecondsRequired = 14f;

        private float holdTimer;
        private bool completed;
        private Collider zoneCollider;

        public static MissionCaptureZone Instance { get; private set; }

        public bool IsCompleted => completed;
        public float HoldProgress01 => completed ? 1f : Mathf.Clamp01(holdTimer / Mathf.Max(0.01f, holdSecondsRequired));
        public float HoldSecondsRequired => Mathf.Max(1f, holdSecondsRequired);
        public int OccupyingPlayerUnitCount => OccupyingUnits.Count;
        public bool IsPlayerInside => OccupyingUnits.Count > 0;

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
    }
}
