using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Shared combat target data for units and structures.
    /// </summary>
    public class CombatTarget : MonoBehaviour
    {
        private UnitTeam team;
        private UnitHealth health;

        public UnitTeam Team => team;
        public bool IsAlive => health != null && health.IsAlive;
        public UnitHealth Health => health;

        private void OnEnable()
        {
            PrototypeRuntimeRegistry.Register(this);
        }

        private void OnDisable()
        {
            PrototypeRuntimeRegistry.Unregister(this);
        }

        public void Initialize(UnitTeam assignedTeam, UnitHealth assignedHealth)
        {
            team = assignedTeam;
            health = assignedHealth;
        }
    }
}
