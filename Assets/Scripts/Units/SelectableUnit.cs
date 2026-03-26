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
        private GameObject selectionRing;
        private UnitTeam team;
        private UnitArchetype archetype;
        private string displayName = "Unit";
        private Color defaultColor;
        private Color selectedColor;

        public UnitTeam Team => team;
        public UnitArchetype Archetype => archetype;
        public string DisplayName => displayName;

        private void Awake()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            unitMover = GetComponent<SimpleUnitMover>();
            combat = GetComponent<UnitCombat>();
            combatTarget = GetComponent<CombatTarget>();
            CreateSelectionRing();
            ApplyTeamColors();
        }

        public void Initialize(UnitTeam assignedTeam, UnitArchetype assignedArchetype, string assignedDisplayName, SimpleUnitMover mover, UnitCombat unitCombat)
        {
            team = assignedTeam;
            archetype = assignedArchetype;
            displayName = assignedDisplayName;
            unitMover = mover;
            combat = unitCombat;
            ApplyTeamColors();
        }

        public void MoveTo(Vector3 destination)
        {
            combat?.ClearTarget();
            unitMover.SetDestination(destination);
        }

        public void Attack(CombatTarget target)
        {
            combat?.SetTarget(target);
        }

        public void SetSelected(bool isSelected)
        {
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

            defaultColor = team == UnitTeam.Player
                ? (archetype == UnitArchetype.Vanguard ? new Color(0.55f, 0.75f, 1f) : new Color(0.72f, 0.92f, 1f))
                : (archetype == UnitArchetype.Vanguard ? new Color(0.88f, 0.35f, 0.35f) : new Color(1f, 0.58f, 0.32f));

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
