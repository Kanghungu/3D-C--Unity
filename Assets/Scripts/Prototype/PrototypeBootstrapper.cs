// =============================================================================
// [Scripts 레이어: Prototype]
// - SampleScene 등 "구형 대형 전장"을 런타임에 조립하는 부트스트랩.
// - Battle Aces(NewSampleScene)·Campaign(메뉴·미션 데이터)와 독립 — 여기서 씬을 만들지 않는다.
// - PrototypeGameDatabase·유닛 생성은 Battle Aces가 재사용한다(의존: BA → Prototype 데이터).
// =============================================================================
using Game.CameraSystem;
using Game.Selection;
using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Creates a more complete playable RTS prototype scene at runtime.
    /// Add this to an empty GameObject in the scene and press Play.
    /// </summary>
    public partial class PrototypeBootstrapper : MonoBehaviour
    {
        [Header("Runtime Rebuild")]
        [SerializeField] private bool rebuildPrototypeWorldOnPlay = true;

        [Header("Battlefield Theme")]
        [SerializeField] private BattlefieldTheme battlefieldTheme = BattlefieldTheme.DesertShrine;
        [SerializeField] private bool randomizeBattlefieldThemeOnPlay;

        [Header("Ground")]
        [SerializeField] private Vector3 groundScale = new(460f, 1f, 440f);
        [SerializeField] private Vector3 groundPosition = Vector3.zero;
        [Tooltip("외부 텍스처를 여기에 연결하면 절차적 생성 대신 이 텍스처를 사용합니다.")]
        [SerializeField] private Texture2D groundTextureOverride;
        [Tooltip("텍스처 타일 반복 횟수 (값이 클수록 작게 반복)")]
        [SerializeField] private Vector2 groundTextureTiling = new(12f, 12f);

        [Header("Camera")]
        [SerializeField] private Vector3 cameraPosition = new(0f, 720f, -1540f);
        [SerializeField] private Vector3 cameraRotation = new(68f, 0f, 0f);

        [Header("Army Setup")]
        [SerializeField] private Vector3 friendlyStart = new(-2060f, 1f, -840f);
        [SerializeField] private Vector2Int friendlyGrid = new(20, 10);
        [SerializeField] private Vector3 enemyStart = new(1580f, 1f, 620f);
        [SerializeField] private Vector2Int enemyGrid = new(16, 8);
        [SerializeField] private float unitSpacing = 8.8f;

        [Header("Bases")]
        [SerializeField] private Vector3 playerBasePosition = new(-2240f, 1f, -1460f);
        [SerializeField] private Vector3 enemyBasePosition = new(2240f, 1f, 1460f);
        [SerializeField] private Vector3 playerFoundryPosition = new(-2090f, 1f, -1630f);
        [SerializeField] private Vector3 playerSiegePosition = new(-2320f, 1f, -1780f);

        private Color sandColor = new(0.76f, 0.64f, 0.42f);
        private Color duneColor = new(0.68f, 0.55f, 0.34f);
        private Color stoneColor = new(0.62f, 0.56f, 0.46f);
        private Color altarColor = new(0.82f, 0.72f, 0.48f);
        private Color ruinColor = new(0.48f, 0.42f, 0.36f);

        private Transform playerUnitRoot;
        private Transform enemyUnitRoot;
        private PrototypeGameDatabase database;
        private BattlefieldMapProfile mapProfile;
        private BattlefieldVisionController visionController;
        private Light sceneLight;
        private float rootLabelRefreshTimer;

        private void Awake()
        {
            if (rebuildPrototypeWorldOnPlay)
            {
                ClearExistingPrototypeWorld();
            }

            if (randomizeBattlefieldThemeOnPlay)
            {
                battlefieldTheme = (BattlefieldTheme)Random.Range(0, 3);
            }

            ApplyThemeLayout();
            ApplyThemePalette();
            SetupGround();
            SetupLighting();
            SetupMapProfile();
            SetupCamera();
            SetupRoots();
            SetupDatabase();
            SetupTerrainFeatures();
            SetupNavMesh();         // 지형 배치 후 NavMesh 베이크 → 유닛 스폰 전에 완료
            SetupAmbientAnimator();
            SetupSkyAtmosphere();
            SetupPrototypeSystems();
            SetupBases();
            SetupUnits();
            RefreshRootLabels();
        }

        private void Update()
        {
            rootLabelRefreshTimer -= Time.deltaTime;

            if (rootLabelRefreshTimer <= 0f)
            {
                rootLabelRefreshTimer = 0.5f;
                RefreshRootLabels();
            }
        }

        private void ClearExistingPrototypeWorld()
        {
            PrototypeSceneHierarchyUtility.DestroyByType<SelectableUnit>();
            PrototypeSceneHierarchyUtility.DestroyByType<BaseStructure>();
            PrototypeSceneHierarchyUtility.DestroyByType<ProductionStructure>();
            PrototypeSceneHierarchyUtility.DestroyByType<ControlNode>();
            PrototypeSceneHierarchyUtility.DestroyByType<DefensiveTurret>();
            PrototypeSceneHierarchyUtility.DestroyByType<BattleDirectiveController>();
            PrototypeSceneHierarchyUtility.DestroyByType<BattlefieldMapProfile>();
            PrototypeSceneHierarchyUtility.DestroyByType<BattlefieldVisionController>();

            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Friendly Units");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Enemy Units");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Structures");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Terrain Features");
            PrototypeSceneHierarchyUtility.DestroyRootIfExists("Move Marker");
        }

        private void SetupRoots()
        {
            playerUnitRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Friendly Units");
            enemyUnitRoot = PrototypeSceneHierarchyUtility.GetOrCreateRoot("Enemy Units");
            PrototypeSceneHierarchyUtility.GetOrCreateRoot("Structures");
            PrototypeSceneHierarchyUtility.GetOrCreateRoot("Terrain Features");
        }

        private void SetupDatabase()
        {
            database = FindAnyObjectByType<PrototypeGameDatabase>();

            if (database == null)
            {
                database = gameObject.AddComponent<PrototypeGameDatabase>();
            }
        }

        private void SetupPrototypeSystems()
        {
            if (FindAnyObjectByType<PrototypeSelectionController>() == null) gameObject.AddComponent<PrototypeSelectionController>();
            if (FindAnyObjectByType<EnemyAIController>() == null) gameObject.AddComponent<EnemyAIController>();
            if (FindAnyObjectByType<PrototypeHUD>() == null) gameObject.AddComponent<PrototypeHUD>();
            if (FindAnyObjectByType<PlayerProductionController>() == null) gameObject.AddComponent<PlayerProductionController>();
            if (FindAnyObjectByType<PlayerAbilityController>() == null) gameObject.AddComponent<PlayerAbilityController>();
            if (FindAnyObjectByType<ThreatResponseController>() == null) gameObject.AddComponent<ThreatResponseController>();
            if (FindAnyObjectByType<BattleDirectiveController>() == null) gameObject.AddComponent<BattleDirectiveController>();
            if (FindAnyObjectByType<PrototypeMatchController>() == null) gameObject.AddComponent<PrototypeMatchController>();
            if (FindAnyObjectByType<BattleStatsTracker>() == null) gameObject.AddComponent<BattleStatsTracker>();
        }

        private void RefreshRootLabels()
        {
            if (playerUnitRoot != null)
            {
                PrototypeSceneHierarchyUtility.RefreshUnitRootLabel(playerUnitRoot, "Friendly Units", UnitTeam.Player);
            }

            if (enemyUnitRoot != null)
            {
                PrototypeSceneHierarchyUtility.RefreshUnitRootLabel(enemyUnitRoot, "Enemy Units", UnitTeam.Enemy);
            }
        }

        private static float[] BuildAxisPositions(int count, float halfExtent, float margin)
        {
            float usableMin = -Mathf.Max(halfExtent - margin, 120f);
            float usableMax = Mathf.Max(halfExtent - margin, 120f);
            float[] positions = new float[count];

            for (int index = 0; index < count; index++)
            {
                positions[index] = Mathf.Lerp(usableMin, usableMax, count == 1 ? 0.5f : index / (float)(count - 1));
            }

            return positions;
        }

        private static string GetBattlefieldLabel(BattlefieldTheme theme)
        {
            return theme switch
            {
                BattlefieldTheme.CrimsonBasin => "Crimson Basin Front",
                BattlefieldTheme.PaleSaltFlats => "Pale Salt Flats",
                _ => "Desert Shrine Front"
            };
        }
    }
}
