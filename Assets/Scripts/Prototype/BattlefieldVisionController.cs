using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Keeps a simple explored/visible grid for minimap shroud and battlefield hiding.
    /// </summary>
    public class BattlefieldVisionController : MonoBehaviour
    {
        public static BattlefieldVisionController Instance { get; private set; }

        [SerializeField] private int gridWidth = 28;
        [SerializeField] private int gridHeight = 28;
        [SerializeField] private float refreshInterval = 0.28f;
        [SerializeField] private float unitRevealRadius = 150f;
        [SerializeField] private float structureRevealRadius = 240f;
        [SerializeField] private float ownedNodeRevealRadius = 130f;

        private BattlefieldMapProfile mapProfile;
        private bool[] exploredCells;
        private bool[] visibleCells;
        private float refreshTimer;

        public BattlefieldMapProfile MapProfile => mapProfile;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        private void Awake()
        {
            Instance = this;
            ResolveProfile();
            EnsureGrid();
            RebuildVision();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            refreshTimer -= Time.deltaTime;

            if (refreshTimer > 0f)
            {
                return;
            }

            refreshTimer = refreshInterval;
            RebuildVision();
        }

        public void Configure(BattlefieldMapProfile profile)
        {
            mapProfile = profile;
            EnsureGrid();
            RebuildVision();
        }

        public bool IsCellExplored(int x, int y)
        {
            return IsInBounds(x, y) && exploredCells[(y * gridWidth) + x];
        }

        public bool IsCellVisible(int x, int y)
        {
            return IsInBounds(x, y) && visibleCells[(y * gridWidth) + x];
        }

        public bool IsWorldExplored(Vector3 worldPosition)
        {
            return TryGetCell(worldPosition, out int x, out int y) && IsCellExplored(x, y);
        }

        public bool IsWorldVisible(Vector3 worldPosition)
        {
            return TryGetCell(worldPosition, out int x, out int y) && IsCellVisible(x, y);
        }

        private void ResolveProfile()
        {
            if (mapProfile == null)
            {
                mapProfile = FindAnyObjectByType<BattlefieldMapProfile>();
            }
        }

        private void EnsureGrid()
        {
            int cellCount = Mathf.Max(4, gridWidth) * Mathf.Max(4, gridHeight);

            if (exploredCells != null && exploredCells.Length == cellCount)
            {
                return;
            }

            exploredCells = new bool[cellCount];
            visibleCells = new bool[cellCount];
        }

        private void RebuildVision()
        {
            ResolveProfile();

            if (mapProfile == null)
            {
                return;
            }

            for (int index = 0; index < visibleCells.Length; index++)
            {
                visibleCells[index] = false;
            }

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit != null && unit.Team == UnitTeam.Player)
                {
                    MarkCircle(unit.transform.position, unitRevealRadius + GetRevealBonus(unit));
                }
            }

            foreach (BaseStructure baseStructure in PrototypeRuntimeRegistry.GetBaseStructures())
            {
                if (baseStructure != null && baseStructure.Team == UnitTeam.Player && baseStructure.IsAlive)
                {
                    MarkCircle(baseStructure.transform.position, structureRevealRadius);
                }
            }

            foreach (ProductionStructure structure in PrototypeRuntimeRegistry.GetProductionStructures())
            {
                if (structure != null && structure.Team == UnitTeam.Player && structure.IsAlive)
                {
                    MarkCircle(structure.transform.position, structureRevealRadius * 0.82f);
                }
            }

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node != null && node.OwnerTeam == UnitTeam.Player)
                {
                    MarkCircle(node.transform.position, ownedNodeRevealRadius);
                }
            }

            for (int index = 0; index < exploredCells.Length; index++)
            {
                exploredCells[index] |= visibleCells[index];
            }

            RefreshFogObjects();
        }

        private void RefreshFogObjects()
        {
            foreach (BattlefieldFogObject fogObject in BattlefieldFogObject.RegisteredObjects)
            {
                if (fogObject == null)
                {
                    continue;
                }

                bool shouldShow = fogObject.Requirement == BattlefieldFogRequirement.Visible
                    ? IsWorldVisible(fogObject.WorldPosition)
                    : IsWorldExplored(fogObject.WorldPosition);
                fogObject.SetFogged(!shouldShow);
            }
        }

        private float GetRevealBonus(SelectableUnit unit)
        {
            return unit.Archetype switch
            {
                UnitArchetype.Fighter => 80f,
                UnitArchetype.MobileFortress => 95f,
                UnitArchetype.AirborneCitadel => 140f,
                _ => 0f
            };
        }

        private void MarkCircle(Vector3 worldPosition, float radius)
        {
            if (mapProfile == null)
            {
                return;
            }

            TryGetCell(new Vector3(worldPosition.x - radius, 0f, worldPosition.z + radius), out int startX, out int startY);
            TryGetCell(new Vector3(worldPosition.x + radius, 0f, worldPosition.z - radius), out int endX, out int endY);

            for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
            {
                for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
                {
                    Vector3 cellCenter = GetCellCenter(x, y);
                    cellCenter.y = worldPosition.y;

                    if (Vector3.Distance(cellCenter, worldPosition) <= radius)
                    {
                        visibleCells[(y * gridWidth) + x] = true;
                    }
                }
            }
        }

        private Vector3 GetCellCenter(int x, int y)
        {
            float normalizedX = (x + 0.5f) / gridWidth;
            float normalizedY = (y + 0.5f) / gridHeight;
            return mapProfile.NormalizedToWorld(new Vector2(normalizedX, normalizedY));
        }

        private bool TryGetCell(Vector3 worldPosition, out int x, out int y)
        {
            x = 0;
            y = 0;

            if (mapProfile == null)
            {
                return false;
            }

            Vector2 normalized = mapProfile.WorldToNormalized(worldPosition);
            x = Mathf.Clamp(Mathf.FloorToInt(normalized.x * gridWidth), 0, gridWidth - 1);
            y = Mathf.Clamp(Mathf.FloorToInt(normalized.y * gridHeight), 0, gridHeight - 1);
            return true;
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }
    }
}
