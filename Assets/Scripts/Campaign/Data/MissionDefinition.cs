using Game.Units;
using UnityEngine;

namespace Game.Campaign.Data
{
    /// <summary>
    /// 미션 데이터 — 유닛 스탯/비용은 기존 DB를 쓰고, 여기서는 덱·목표·대사 ID·팩션 규칙만 묶는다.
    /// JSON으로 내보내기/가져오기할 때도 동일 필드를 맞추면 된다.
    /// </summary>
    [CreateAssetMenu(fileName = "Mission", menuName = "Game/Campaign/Mission Definition", order = 1)]
    public sealed class MissionDefinition : ScriptableObject
    {
        [Header("식별")]
        [SerializeField] private string missionId = "mission_01";

        [SerializeField] private string displayName = "성역 방어";

        [Header("씬 로드")]
        [Tooltip("Build Settings 에 등록된 씬 이름(확장자 없음). 예: NewSampleScene")]
        [SerializeField] private string gameplaySceneName = "NewSampleScene";

        [Tooltip("미션 선택 화면 정렬(낮을수록 먼저)")]
        [SerializeField] private int campaignSortOrder;

        [Header("목표")]
        [SerializeField] private MissionObjectiveKind objectiveKind = MissionObjectiveKind.DestroyEnemyCore;

        [Tooltip("같은 맵 레이아웃을 미러·자원만 바꿔 재사용할 때 체크")]
        [SerializeField] private bool mirroredLayoutVariant;

        [Tooltip("체크 시 브리핑/승패 대사 키에 _air 접미사, 덱·적 패턴이 공중 요새 위주로 조정됨")]
        [SerializeField] private bool airborneCitadelFocus;

        [Tooltip("브리핑 본문이 길 때만 체크 — 폰트 15·카드 높이를 조금 늘려 잘림을 줄임")]
        [SerializeField] private bool briefingUseCompactFont;

        [Header("대사 ID (DialogueTable 키)")]
        [SerializeField] private string briefingDialogueId;

        [SerializeField] private string victoryDialogueId;

        [SerializeField] private string defeatDialogueId;

        [Header("플레이어 덱 (고정 슬롯, 최대 8)")]
        [SerializeField] private UnitArchetype[] playerDeck = new UnitArchetype[0];

        [Header("적 패턴 (문자열 ID — 적 AI/웨이브 테이블과 연결 예정)")]
        [SerializeField] private string enemyPatternId = "default_skirmish";

        [Header("적 AI (Battle Aces)")]
        [Tooltip("0 이하면 자동(기본 간격 + 팩션 생산 배율). 지정 시 초 단위로 적 코어 생산 시도 간격을 덮어씀")]
        [SerializeField] private float enemyBrainThinkIntervalOverride;

        [Tooltip("최종 간격에 곱함 — 1=기본, 0.75=더 자주 생산")]
        [SerializeField] private float enemyBrainThinkIntervalMultiplier = 1f;

        [Header("팩션 규칙 (선택)")]
        [SerializeField] private FactionRulesDefinition playerFactionRules;

        [SerializeField] private FactionRulesDefinition enemyFactionRules;

        [Header("목표 수치 (미션 종류별로 사용)")]
        [SerializeField] private float defenseDurationSeconds = 135f;

        [SerializeField] private float seizeHoldSeconds = 14f;

        [Header("스폰 밸런스 (부트스트래퍼 — 유물/이단 거점)")]
        [SerializeField] private float relicMaxHealth = 520f;

        [SerializeField] private float heresyStrongholdMaxHealth = 1280f;

        [Header("보조 목표 (선택 — 실패해도 미션 진행)")]
        [Tooltip("비어 있으면 없음. core_survive_50: 승리 시 아군 코어 50% 이상 / train_variety_3: 서로 다른 병과 3종 이상 생산")]
        [SerializeField] private string optionalBonusObjectiveId;

        public string MissionId => missionId;
        public string DisplayName => displayName;
        public string GameplaySceneName => gameplaySceneName;
        public int CampaignSortOrder => campaignSortOrder;
        public MissionObjectiveKind ObjectiveKind => objectiveKind;
        public bool MirroredLayoutVariant => mirroredLayoutVariant;

        /// <summary>공중 요새(공성) 위주 미션 변주 — 대사는 *_air 키 우선</summary>
        public bool AirborneCitadelFocus => airborneCitadelFocus;

        /// <summary>긴 브리핑 전용 — UI에서 본문 폰트·카드 높이만 조정</summary>
        public bool BriefingUseCompactFont => briefingUseCompactFont;

        public string BriefingDialogueId => briefingDialogueId;
        public string VictoryDialogueId => victoryDialogueId;
        public string DefeatDialogueId => defeatDialogueId;

        /// <summary>공중 변주면 briefingDialogueId + "_air" (테이블에 없으면 폴백)</summary>
        public string EffectiveBriefingDialogueId => ResolveAirDialogueId(briefingDialogueId);

        public string EffectiveVictoryDialogueId => ResolveAirDialogueId(victoryDialogueId);

        public string EffectiveDefeatDialogueId => ResolveAirDialogueId(defeatDialogueId);

        private string ResolveAirDialogueId(string baseId)
        {
            if (!airborneCitadelFocus || string.IsNullOrEmpty(baseId))
            {
                return baseId;
            }

            return baseId + "_air";
        }
        public string EnemyPatternId => enemyPatternId;

        /// <summary>0 이하면 부트스트랩이 자동 계산</summary>
        public float EnemyBrainThinkIntervalOverride => enemyBrainThinkIntervalOverride;

        public float EnemyBrainThinkIntervalMultiplier => Mathf.Max(0.12f, enemyBrainThinkIntervalMultiplier);

        public FactionRulesDefinition PlayerFactionRules => playerFactionRules;
        public FactionRulesDefinition EnemyFactionRules => enemyFactionRules;
        public float DefenseDurationSeconds => Mathf.Max(5f, defenseDurationSeconds);
        public float SeizeHoldSeconds => Mathf.Max(1f, seizeHoldSeconds);
        public float RelicMaxHealth => Mathf.Max(80f, relicMaxHealth);
        public float HeresyStrongholdMaxHealth => Mathf.Max(200f, heresyStrongholdMaxHealth);

        /// <summary>보조 목표 ID — 빈 문자열이면 없음</summary>
        public string OptionalBonusObjectiveId => optionalBonusObjectiveId != null ? optionalBonusObjectiveId.Trim() : string.Empty;

        /// <summary>코드에서만 쓰는 런타임 데모/테스트 설정(메뉴 없이 씬 단독 실행 시 등)</summary>
        public void AssignRuntimeCampaign(
            string title,
            string sceneName,
            MissionObjectiveKind kind,
            UnitArchetype[] deckEight,
            string briefingId,
            string victoryId,
            string defeatId,
            float defenseSec,
            float seizeSec,
            int sortOrder = 0)
        {
            displayName = string.IsNullOrEmpty(title) ? "미션" : title;
            missionId = "runtime_" + kind + "_" + sortOrder;
            gameplaySceneName = string.IsNullOrEmpty(sceneName) ? "NewSampleScene" : sceneName;
            campaignSortOrder = sortOrder;
            objectiveKind = kind;
            briefingDialogueId = briefingId;
            victoryDialogueId = victoryId;
            defeatDialogueId = defeatId;
            defenseDurationSeconds = defenseSec;
            seizeHoldSeconds = seizeSec;
            playerDeck = deckEight != null && deckEight.Length > 0 ? deckEight : new[] { UnitArchetype.Spearman };
        }

        /// <summary>덱 복사본(읽기 전용으로 사용 권장)</summary>
        public UnitArchetype[] GetPlayerDeckCopy()
        {
            if (playerDeck == null || playerDeck.Length == 0)
            {
                return System.Array.Empty<UnitArchetype>();
            }

            var copy = new UnitArchetype[playerDeck.Length];
            System.Array.Copy(playerDeck, copy, playerDeck.Length);
            return copy;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(missionId))
            {
                missionId = "mission_unnamed";
            }

            if (playerDeck != null && playerDeck.Length > 8)
            {
                Debug.LogWarning($"[MissionDefinition] 덱은 8슬롯 이하 권장: {name} ({playerDeck.Length}개)");
            }
        }
    }
}
