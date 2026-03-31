using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Dedicated production building for player reinforcements.
    /// </summary>
    public class ProductionStructure : MonoBehaviour
    {
        [SerializeField] private int maxUnits = 1000;
        [SerializeField] private int maxQueueLength = 48;
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private Vector3 defaultRallyOffset = new(8f, 0f, 4f);
        [SerializeField] private float threatWarningRadius = 22f;

        private readonly List<UnitDefinition> productionQueue = new();
        private readonly List<UnitArchetype> allowedArchetypes = new();
        private CombatTarget combatTarget;
        private UnitHealth health;
        private Transform unitRoot;
        private PrototypeGameDatabase database;
        private BaseStructure linkedBase;
        private float productionTimer;
        private bool isProducing;
        private bool hasRallyPoint;
        private float productionSpeedMultiplier = 1f;
        private float lastQueueCommandTime;
        private UnitDefinition currentProductionDefinition;
        private int rallyDispatchSequence;
        private Vector3 rallyPoint;
        private GameObject rallyMarker;
        private string structureLabel = "Foundry";
        private Transform statusAnchor;
        private Transform productionCore;
        private Renderer productionCoreRenderer;
        private Transform rallyGuideRoot;
        private Transform rallyGuide;
        private Renderer rallyGuideRenderer;
        private Transform rallyTip;
        private Renderer rallyTipRenderer;
        private Transform threatAnchor;
        private Transform threatRing;
        private Renderer threatRingRenderer;
        private Transform threatCore;
        private Renderer threatCoreRenderer;
        private Transform threatDirectionRoot;
        private Transform threatDirectionBeam;
        private Renderer threatDirectionBeamRenderer;
        private Transform threatDirectionTip;
        private Renderer threatDirectionTipRenderer;
        private readonly Transform[] queuePips = new Transform[4];
        private readonly Renderer[] queuePipRenderers = new Renderer[4];

        public UnitTeam Team => combatTarget != null ? combatTarget.Team : UnitTeam.Player;
        public bool IsAlive => health != null && health.IsAlive;
        public bool HasQueuedProduction => productionQueue.Count > 0 || isProducing;
        public int QueueCount => productionQueue.Count + (isProducing ? 1 : 0);
        public float LastQueueCommandTime => lastQueueCommandTime;
        public bool HasRallyPoint => hasRallyPoint;
        public Vector3 RallyPoint => rallyPoint;
        public float ProductionProgressNormalized => isProducing && currentProductionDefinition != null
            ? Mathf.Clamp01(1f - (productionTimer / Mathf.Max(0.01f, currentProductionDefinition.ProductionDuration)))
            : 0f;
        public string QueueLabel => isProducing && currentProductionDefinition != null ? currentProductionDefinition.DisplayName : "Idle";
        public string QueuePreview => BuildQueuePreview();
        public string RallyLabel => hasRallyPoint ? $"{rallyPoint.x:0.0}, {rallyPoint.z:0.0}" : "Unset";
        public float SpeedMultiplier => productionSpeedMultiplier;
        public string StructureLabel => structureLabel;
        public string AllowedUnitsLabel => BuildAllowedUnitsLabel();

        private void Awake()
        {
            EnsureRallyMarker();
            EnsureStatusVisuals();
            EnsureThreatVisuals();
            UpdateStatusVisuals();
            UpdateThreatVisuals();
        }

        private void OnEnable()
        {
            PrototypeRuntimeRegistry.Register(this);
        }

        private void OnDisable()
        {
            PrototypeRuntimeRegistry.Unregister(this);
        }

        private void Update()
        {
            UpdateRallyMarker();
            UpdateStatusVisuals();
            UpdateThreatVisuals();

            if (!IsAlive || unitRoot == null || database == null || Team != UnitTeam.Player)
            {
                return;
            }

            if (linkedBase != null && !linkedBase.IsAlive)
            {
                return;
            }

            RunProduction();
        }

        public void Initialize(CombatTarget assignedTarget, UnitHealth assignedHealth, Transform assignedUnitRoot, PrototypeGameDatabase assignedDatabase, BaseStructure assignedLinkedBase)
        {
            combatTarget = assignedTarget;
            health = assignedHealth;
            unitRoot = assignedUnitRoot;
            database = assignedDatabase;
            linkedBase = assignedLinkedBase;
            ApplyVisuals();
            SetRallyPoint(transform.position + defaultRallyOffset);
            UpdateStatusVisuals();
            UpdateThreatVisuals();
        }

        public void ConfigureStructure(string newLabel, Vector3 rallyOffset, Color color, params UnitArchetype[] newAllowedArchetypes)
        {
            structureLabel = newLabel;
            defaultRallyOffset = rallyOffset;
            allowedArchetypes.Clear();
            allowedArchetypes.AddRange(newAllowedArchetypes);

            Renderer rendererComponent = GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            if (rallyMarker != null)
            {
                rallyMarker.GetComponent<Renderer>().material.color = color;
            }

            UpdateStatusVisuals();
            UpdateThreatVisuals();
        }

        public bool TryQueueProduction(UnitDefinition definition)
        {
            int reservedTotal = productionQueue.Count + (isProducing ? 1 : 0);

            if (!IsAlive || Team != UnitTeam.Player || definition == null || !CanProduce(definition.Archetype) || productionQueue.Count >= maxQueueLength)
            {
                return false;
            }

            if (!PrototypeProductionRules.CanReserveUnit(Team, definition, reservedTotal, CountQueued(definition.Archetype), maxUnits))
            {
                return false;
            }

            productionQueue.Add(definition);
            lastQueueCommandTime = Time.unscaledTime;

            if (!isProducing)
            {
                BeginNextProduction();
            }

            return true;
        }

        public bool TryCancelLastQueuedProduction()
        {
            if (productionQueue.Count == 0)
            {
                return false;
            }

            productionQueue.RemoveAt(productionQueue.Count - 1);
            lastQueueCommandTime = Time.unscaledTime;
            return true;
        }

        public bool CanProduce(UnitArchetype archetype)
        {
            return allowedArchetypes.Count == 0 || allowedArchetypes.Contains(archetype);
        }

        public void SetRallyPoint(Vector3 worldPoint)
        {
            rallyPoint = new Vector3(worldPoint.x, 1f, worldPoint.z);
            hasRallyPoint = true;
            UpdateRallyMarker();
            UpdateStatusVisuals();
        }

        public void SetProductionSpeedMultiplier(float multiplier)
        {
            productionSpeedMultiplier = Mathf.Max(0.5f, multiplier);
            UpdateStatusVisuals();
        }

        private void RunProduction()
        {
            if (!isProducing)
            {
                if (productionQueue.Count > 0)
                {
                    BeginNextProduction();
                }

                return;
            }

            productionTimer -= Time.deltaTime * productionSpeedMultiplier;
            if (productionTimer > 0f)
            {
                return;
            }

            SpawnUnit(currentProductionDefinition);
            isProducing = false;
            currentProductionDefinition = null;

            if (productionQueue.Count > 0)
            {
                BeginNextProduction();
            }
        }

        private void BeginNextProduction()
        {
            if (productionQueue.Count == 0)
            {
                isProducing = false;
                currentProductionDefinition = null;
                return;
            }

            currentProductionDefinition = productionQueue[0];
            productionQueue.RemoveAt(0);
            productionTimer = currentProductionDefinition.ProductionDuration;
            isProducing = true;
        }

        private void SpawnUnit(UnitDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            Vector3 offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y = definition.IsFlying ? definition.HoverHeight : 1f;
            SelectableUnit unit = PrototypeEntityFactory.CreateUnit(Team, definition, spawnPosition, unitRoot);

            if (unit != null && hasRallyPoint)
            {
                unit.MoveTo(GetRallyDestination());
            }
        }

        private Vector3 GetRallyDestination()
        {
            rallyDispatchSequence++;

            if (rallyDispatchSequence <= 1)
            {
                return rallyPoint;
            }

            const float goldenAngle = 2.39996323f;
            float radius = Mathf.Min(spawnRadius * 1.8f, 8f + Mathf.Sqrt(rallyDispatchSequence) * 1.9f);
            float angle = rallyDispatchSequence * goldenAngle;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            return rallyPoint + offset;
        }

        private int CountQueued(UnitArchetype archetype)
        {
            int count = 0;

            if (isProducing && currentProductionDefinition != null && currentProductionDefinition.Archetype == archetype)
            {
                count++;
            }

            foreach (UnitDefinition definition in productionQueue)
            {
                if (definition != null && definition.Archetype == archetype)
                {
                    count++;
                }
            }

            return count;
        }

        private string BuildQueuePreview()
        {
            if (!isProducing && productionQueue.Count == 0)
            {
                return "Idle";
            }

            List<string> labels = new();
            if (isProducing && currentProductionDefinition != null)
            {
                labels.Add($"> {currentProductionDefinition.DisplayName}");
            }

            foreach (UnitDefinition definition in productionQueue)
            {
                if (definition != null)
                {
                    labels.Add(definition.DisplayName);
                }
            }

            return string.Join(", ", labels);
        }

        private string BuildAllowedUnitsLabel()
        {
            if (allowedArchetypes.Count == 0)
            {
                return "All";
            }

            if (database == null)
            {
                return string.Join(", ", allowedArchetypes);
            }

            List<string> labels = new();
            foreach (UnitArchetype archetype in allowedArchetypes)
            {
                UnitDefinition definition = database.GetDefinition(archetype);
                labels.Add(definition != null ? definition.DisplayName : archetype.ToString());
            }

            return string.Join(", ", labels);
        }

        private void ApplyVisuals()
        {
            Renderer rendererComponent = GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = new Color(0.32f, 0.78f, 1f);
            }
        }

        private void EnsureRallyMarker()
        {
            if (rallyMarker != null)
            {
                return;
            }

            rallyMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rallyMarker.name = $"{name} Rally Marker";
            rallyMarker.transform.localScale = new Vector3(1.12f, 0.03f, 1.12f);
            rallyMarker.GetComponent<Collider>().enabled = false;
            rallyMarker.GetComponent<Renderer>().material.color = new Color(0.25f, 1f, 0.85f, 0.9f);
            rallyMarker.SetActive(false);
        }

        private void UpdateRallyMarker()
        {
            if (rallyMarker == null)
            {
                return;
            }

            rallyMarker.transform.position = new Vector3(rallyPoint.x, 0.12f, rallyPoint.z);
            rallyMarker.SetActive(hasRallyPoint && IsAlive);
        }

        private void EnsureStatusVisuals()
        {
            if (statusAnchor != null)
            {
                return;
            }

            statusAnchor = new GameObject("Production Status Anchor").transform;
            statusAnchor.SetParent(transform);
            statusAnchor.localPosition = new Vector3(0f, 1.3f, 0f);
            statusAnchor.localRotation = Quaternion.identity;
            statusAnchor.localScale = Vector3.one;

            Transform mast = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cylinder,
                "Status Mast",
                new Vector3(0f, 0.22f, 0f),
                new Vector3(0.05f, 0.22f, 0.05f),
                new Color(0.28f, 0.3f, 0.34f));

            productionCore = CreateStatusPrimitive(
                statusAnchor,
                PrimitiveType.Cube,
                "Production Core",
                new Vector3(0f, 0.48f, 0f),
                new Vector3(0.18f, 0.18f, 0.18f),
                new Color(0.28f, 0.9f, 1f));
            productionCoreRenderer = productionCore.GetComponent<Renderer>();

            rallyGuideRoot = new GameObject("Rally Guide Root").transform;
            rallyGuideRoot.SetParent(statusAnchor);
            rallyGuideRoot.localPosition = new Vector3(0f, 0.14f, 0f);
            rallyGuideRoot.localRotation = Quaternion.identity;
            rallyGuideRoot.localScale = Vector3.one;

            rallyGuide = CreateStatusPrimitive(
                rallyGuideRoot,
                PrimitiveType.Cube,
                "Rally Guide",
                new Vector3(0f, 0f, 0.28f),
                new Vector3(0.06f, 0.04f, 0.56f),
                new Color(0.36f, 0.95f, 0.86f));
            rallyGuideRenderer = rallyGuide.GetComponent<Renderer>();

            rallyTip = CreateStatusPrimitive(
                rallyGuideRoot,
                PrimitiveType.Sphere,
                "Rally Tip",
                new Vector3(0f, 0f, 0.58f),
                new Vector3(0.12f, 0.12f, 0.12f),
                new Color(0.62f, 1f, 0.9f));
            rallyTipRenderer = rallyTip.GetComponent<Renderer>();

            for (int i = 0; i < queuePips.Length; i++)
            {
                float angle = i * Mathf.PI * 0.5f;
                Vector3 pipPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.28f + new Vector3(0f, 0.78f, 0f);
                queuePips[i] = CreateStatusPrimitive(
                    statusAnchor,
                    PrimitiveType.Cube,
                    $"Queue Pip {i + 1}",
                    pipPosition,
                    new Vector3(0.08f, 0.08f, 0.08f),
                    new Color(0.24f, 0.24f, 0.26f));
                queuePipRenderers[i] = queuePips[i].GetComponent<Renderer>();
            }

            mast.gameObject.SetActive(true);
        }

        private void UpdateStatusVisuals()
        {
            if (statusAnchor == null)
            {
                return;
            }

            bool alive = IsAlive;
            statusAnchor.gameObject.SetActive(alive);
            if (!alive)
            {
                return;
            }

            Color idleColor = Team == UnitTeam.Player ? new Color(0.34f, 0.66f, 0.82f) : new Color(0.74f, 0.32f, 0.18f);
            Color activeColor = Team == UnitTeam.Player ? new Color(0.34f, 0.95f, 1f) : new Color(1f, 0.6f, 0.18f);
            bool producing = isProducing && currentProductionDefinition != null;
            float progress = producing ? Mathf.Clamp01(ProductionProgressNormalized) : 0f;
            float pulse = 0.86f + Mathf.PingPong(Time.time * (producing ? 2.2f : 0.8f), 0.14f);
            Color coreColor = Color.Lerp(idleColor, activeColor, producing ? 0.85f : 0.2f) * pulse;

            if (productionCore != null)
            {
                float coreHeight = Mathf.Lerp(0.08f, 0.46f, producing ? Mathf.Max(0.16f, progress) : 0.1f);
                productionCore.localPosition = new Vector3(0f, 0.34f + coreHeight * 0.5f, 0f);
                productionCore.localScale = new Vector3(0.18f, coreHeight, 0.18f);
            }

            if (productionCoreRenderer != null)
            {
                productionCoreRenderer.material.color = coreColor;
            }

            int shownQueueCount = Mathf.Clamp(QueueCount, 0, queuePips.Length);
            for (int i = 0; i < queuePips.Length; i++)
            {
                bool active = i < shownQueueCount;
                if (queuePips[i] != null)
                {
                    queuePips[i].gameObject.SetActive(active);
                }

                if (active && queuePipRenderers[i] != null)
                {
                    float pipPulse = producing && i == 0 ? pulse : 1f;
                    queuePipRenderers[i].material.color = Color.Lerp(idleColor, activeColor, 0.58f + 0.08f * i) * pipPulse;
                }
            }

            UpdateRallyGuideVisuals(activeColor);
        }

        private void UpdateRallyGuideVisuals(Color accentColor)
        {
            if (rallyGuideRoot == null)
            {
                return;
            }

            bool showGuide = hasRallyPoint && IsAlive;
            rallyGuideRoot.gameObject.SetActive(showGuide);
            if (!showGuide)
            {
                return;
            }

            Vector3 worldTarget = new Vector3(rallyPoint.x, transform.position.y, rallyPoint.z);
            Vector3 localTarget = transform.InverseTransformPoint(worldTarget);
            Vector3 planarDirection = new Vector3(localTarget.x, 0f, localTarget.z);
            if (planarDirection.sqrMagnitude < 0.01f)
            {
                planarDirection = Vector3.forward;
            }

            float guideLength = Mathf.Clamp(planarDirection.magnitude * 0.08f, 0.32f, 0.72f);
            rallyGuideRoot.localRotation = Quaternion.LookRotation(planarDirection.normalized, Vector3.up);

            if (rallyGuide != null)
            {
                rallyGuide.localPosition = new Vector3(0f, 0f, 0.14f + guideLength * 0.5f);
                rallyGuide.localScale = new Vector3(0.06f, 0.04f, guideLength);
            }

            if (rallyTip != null)
            {
                rallyTip.localPosition = new Vector3(0f, 0f, 0.14f + guideLength);
            }

            if (rallyGuideRenderer != null)
            {
                rallyGuideRenderer.material.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.92f);
            }

            if (rallyTipRenderer != null)
            {
                rallyTipRenderer.material.color = Color.Lerp(accentColor, Color.white, 0.28f);
            }
        }

        private void EnsureThreatVisuals()
        {
            if (threatAnchor != null)
            {
                return;
            }

            threatAnchor = new GameObject("Threat Anchor").transform;
            threatAnchor.SetParent(statusAnchor != null ? statusAnchor : transform);
            threatAnchor.localPosition = new Vector3(0f, 1.02f, 0f);
            threatAnchor.localRotation = Quaternion.identity;
            threatAnchor.localScale = Vector3.one;

            threatRing = CreateStatusPrimitive(
                threatAnchor,
                PrimitiveType.Cylinder,
                "Threat Ring",
                new Vector3(0f, -0.08f, 0f),
                new Vector3(0.18f, 0.035f, 0.18f),
                new Color(1f, 0.46f, 0.24f));
            threatRingRenderer = threatRing.GetComponent<Renderer>();

            threatCore = CreateStatusPrimitive(
                threatAnchor,
                PrimitiveType.Sphere,
                "Threat Core",
                new Vector3(0f, 0.12f, 0f),
                new Vector3(0.16f, 0.16f, 0.16f),
                new Color(1f, 0.46f, 0.24f));
            threatCoreRenderer = threatCore.GetComponent<Renderer>();

            threatDirectionRoot = new GameObject("Threat Direction Root").transform;
            threatDirectionRoot.SetParent(threatAnchor);
            threatDirectionRoot.localPosition = new Vector3(0f, 0.02f, 0f);
            threatDirectionRoot.localRotation = Quaternion.identity;
            threatDirectionRoot.localScale = Vector3.one;

            threatDirectionBeam = CreateStatusPrimitive(
                threatDirectionRoot,
                PrimitiveType.Cube,
                "Threat Direction Beam",
                new Vector3(0f, 0f, 0.22f),
                new Vector3(0.03f, 0.03f, 0.44f),
                new Color(1f, 0.46f, 0.24f));
            threatDirectionBeamRenderer = threatDirectionBeam.GetComponent<Renderer>();

            threatDirectionTip = CreateStatusPrimitive(
                threatDirectionRoot,
                PrimitiveType.Cube,
                "Threat Direction Tip",
                new Vector3(0f, 0f, 0.48f),
                new Vector3(0.1f, 0.08f, 0.1f),
                new Color(1f, 0.46f, 0.24f));
            threatDirectionTipRenderer = threatDirectionTip.GetComponent<Renderer>();
        }

        private void UpdateThreatVisuals()
        {
            if (threatAnchor == null)
            {
                return;
            }

            int hostileCount = CountNearbyHostiles(out float pressure, out Vector3 threatDirection);
            bool showThreat = IsAlive && hostileCount > 0;
            threatAnchor.gameObject.SetActive(showThreat);

            if (!showThreat)
            {
                return;
            }

            Color threatColor = Team == UnitTeam.Player
                ? new Color(1f, 0.46f, 0.24f)
                : new Color(0.34f, 0.92f, 1f);
            float pulse = 0.88f + Mathf.PingPong(Time.time * (2.4f + pressure * 3.2f), 0.12f + pressure * 0.14f);
            float ringRadius = 0.16f + pressure * 0.18f;
            float coreScale = 0.12f + pressure * 0.14f;

            threatAnchor.localPosition = new Vector3(0f, 1.02f + Mathf.PingPong(Time.time * 1.8f, 0.08f), 0f);

            if (threatRing != null)
            {
                threatRing.localScale = new Vector3(ringRadius, 0.035f, ringRadius);
            }

            if (threatRingRenderer != null)
            {
                threatRingRenderer.material.color = threatColor * pulse;
            }

            if (threatCore != null)
            {
                threatCore.localScale = Vector3.one * coreScale;
            }

            if (threatCoreRenderer != null)
            {
                threatCoreRenderer.material.color = Color.Lerp(threatColor, Color.white, 0.18f) * pulse;
            }

            bool showDirection = threatDirectionRoot != null && threatDirection.sqrMagnitude > 0.0001f;
            if (threatDirectionRoot != null)
            {
                threatDirectionRoot.gameObject.SetActive(showDirection);
            }

            if (!showDirection)
            {
                return;
            }

            float directionLength = 0.38f + pressure * 0.22f;
            threatDirectionRoot.localRotation = Quaternion.LookRotation(threatDirection.normalized, Vector3.up);

            if (threatDirectionBeam != null)
            {
                threatDirectionBeam.localPosition = new Vector3(0f, 0f, directionLength * 0.5f);
                threatDirectionBeam.localScale = new Vector3(0.03f, 0.03f, directionLength);
            }

            if (threatDirectionBeamRenderer != null)
            {
                threatDirectionBeamRenderer.material.color = threatColor * pulse;
            }

            if (threatDirectionTip != null)
            {
                threatDirectionTip.localPosition = new Vector3(0f, 0f, directionLength + 0.08f);
                threatDirectionTip.localScale = new Vector3(0.08f + pressure * 0.04f, 0.08f, 0.08f + pressure * 0.04f);
            }

            if (threatDirectionTipRenderer != null)
            {
                threatDirectionTipRenderer.material.color = Color.Lerp(threatColor, Color.white, 0.22f) * pulse;
            }
        }

        private int CountNearbyHostiles(out float pressure, out Vector3 threatDirection)
        {
            pressure = 0f;
            threatDirection = Vector3.zero;
            int hostileCount = 0;
            Vector3 centroid = Vector3.zero;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || unit.Team == Team)
                {
                    continue;
                }

                CombatTarget target = unit.GetComponent<CombatTarget>();
                if (target != null && !target.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance > threatWarningRadius)
                {
                    continue;
                }

                hostileCount++;
                centroid += unit.transform.position;
                pressure = Mathf.Max(pressure, 1f - distance / Mathf.Max(0.01f, threatWarningRadius));
            }

            pressure = Mathf.Clamp01(Mathf.Max(pressure, hostileCount / 5f));
            if (hostileCount > 0)
            {
                Vector3 averagePosition = centroid / hostileCount;
                threatDirection = averagePosition - transform.position;
                threatDirection.y = 0f;
                if (threatDirection.sqrMagnitude <= 0.0001f)
                {
                    threatDirection = Vector3.forward;
                }
            }

            return hostileCount;
        }

        private static Transform CreateStatusPrimitive(Transform parent, PrimitiveType primitiveType, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = objectName;
            child.transform.SetParent(parent);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = localScale;

            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer rendererComponent = child.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            return child.transform;
        }
    }
}
