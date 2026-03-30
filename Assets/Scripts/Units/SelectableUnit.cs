using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Represents a unit that can be selected and commanded by the prototype controller.
    /// </summary>
    [RequireComponent(typeof(SimpleUnitMover))]
    [RequireComponent(typeof(CombatTarget))]
    public class SelectableUnit : MonoBehaviour
    {
        private Renderer[] cachedRenderers;
        private SimpleUnitMover unitMover;
        private UnitCombat combat;
        private CombatTarget combatTarget;
        private UnitAbilityState abilityState;
        private GameObject selectionRing;
        private UnitTeam team;
        private UnitDefinition definition;
        private Color defaultColor;
        private Color selectedColor;

        public UnitTeam Team => team;
        public UnitArchetype Archetype => definition != null ? definition.Archetype : UnitArchetype.Spearman;
        public string DisplayName => definition != null ? definition.DisplayName : "Unit";
        public UnitDefinition Definition => definition;
        public bool IsSelected { get; private set; }
        public string AbilityStatus => abilityState != null ? abilityState.StatusLabel : "None";
        public string OrderLabel => combat != null ? combat.OrderLabel : "Idle";

        private void Awake()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            unitMover = GetComponent<SimpleUnitMover>();
            combat = GetComponent<UnitCombat>();
            combatTarget = GetComponent<CombatTarget>();
            abilityState = GetComponent<UnitAbilityState>();
            CreateSelectionRing();
            ApplyTeamColors();
        }

        private void OnEnable()
        {
            PrototypeRuntimeRegistry.Register(this);
        }

        private void OnDisable()
        {
            PrototypeRuntimeRegistry.Unregister(this);
        }

        public void Initialize(UnitTeam assignedTeam, UnitDefinition assignedDefinition, SimpleUnitMover mover, UnitCombat unitCombat)
        {
            team = assignedTeam;
            definition = assignedDefinition;
            unitMover = mover;
            combat = unitCombat;
            abilityState = GetComponent<UnitAbilityState>();
            ApplyTeamColors();
        }

        public void MoveTo(Vector3 destination)
        {
            combat?.ClearOrders();
            unitMover.SetDestination(destination);
        }

        public void AttackMoveTo(Vector3 destination)
        {
            combat?.SetAttackMoveDestination(destination);
        }

        public void Attack(CombatTarget target)
        {
            combat?.SetTarget(target);
        }

        public void HoldPosition()
        {
            combat?.SetHoldPosition(transform.position);
        }

        public void GuardPoint(Vector3 point, float radius)
        {
            combat?.SetGuardPoint(point, radius);
        }

        public bool TryActivateAbility()
        {
            return abilityState != null && abilityState.TryActivateRoleAbility();
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            if (selectionRing != null)
            {
                selectionRing.SetActive(isSelected);
            }

            ApplyColor(isSelected ? selectedColor : defaultColor);
        }

        private void ApplyColor(Color color)
        {
            foreach (Renderer cachedRenderer in cachedRenderers)
            {
                if (cachedRenderer != null)
                {
                    cachedRenderer.material.color = color;
                }
            }
        }

        private void ApplyTeamColors()
        {
            team = combatTarget != null ? combatTarget.Team : team;

            if (definition == null)
            {
                defaultColor = team == UnitTeam.Player ? new Color(0.7f, 0.8f, 1f) : new Color(0.9f, 0.35f, 0.35f);
            }
            else
            {
                defaultColor = team == UnitTeam.Player ? definition.PlayerColor : definition.EnemyColor;
            }

            selectedColor = team == UnitTeam.Player ? new Color(0.2f, 0.95f, 0.3f) : new Color(1f, 0.75f, 0.2f);

            if (cachedRenderers != null && cachedRenderers.Length > 0)
            {
                ApplyColor(defaultColor);
            }

            if (selectionRing != null)
            {
                Renderer ringRenderer = selectionRing.GetComponent<Renderer>();
                ringRenderer.material.color = team == UnitTeam.Player ? new Color(0.1f, 1f, 0.3f, 0.9f) : new Color(1f, 0.4f, 0.1f, 0.9f);
            }
        }

        private void CreateSelectionRing()
        {
            if (selectionRing != null)
            {
                return;
            }

            selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionRing.name = "Selection Ring";
            selectionRing.transform.SetParent(transform);
            selectionRing.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            selectionRing.transform.localRotation = Quaternion.identity;
            selectionRing.transform.localScale = new Vector3(1.2f, 0.03f, 1.2f);
            selectionRing.GetComponent<Collider>().enabled = false;
            selectionRing.SetActive(false);
        }
    }
}


