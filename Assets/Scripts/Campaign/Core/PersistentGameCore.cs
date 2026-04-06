// =============================================================================
// [Scripts 레이어: Campaign]
// - 스토리 미션 흐름: 활성 미션·대사 테이블·메뉴 복귀 씬 이름. DontDestroyOnLoad.
// - 전투 규칙은 BattleAces에 두고, 여기서는 "무슨 미션을 싣는지"만 유지한다.
// - PrototypeBootstrapper와 무관 — CampaignMenu 등에서만 배치한다.
// =============================================================================
using Game.Campaign.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Campaign.Core
{
    /// <summary>
    /// 캠페인 공통 부트스트랩 — 씬 전환 후에도 유지되어 현재 미션·대사 테이블 참조를 공유한다.
    /// 첫 씬에 빈 오브젝트로 하나만 둔다.
    /// </summary>
    public sealed class PersistentGameCore : MonoBehaviour
    {
        public static PersistentGameCore Instance { get; private set; }

        [Header("선택 — 시작 시 기본 미션")]
        [SerializeField] private MissionDefinition startingMission;

        [Header("선택 — 공통 대사 테이블")]
        [SerializeField] private DialogueTable dialogueTable;

        [Header("캠페인 UI")]
        [Tooltip("전투 종료 후 '메뉴로' 에서 불러올 씬 이름(Build Settings 등록)")]
        [SerializeField] private string campaignMenuSceneName = "CampaignMenu";

        /// <summary>현재 로드된 미션 정의(전투 씬 부트스트래퍼가 읽음)</summary>
        public MissionDefinition ActiveMission { get; private set; }

        /// <summary>미션 선택 메뉴에서 설정 — 승리 시 해금 인덱스 저장용(-1 이면 MissionDefinition.sortOrder 사용)</summary>
        public int PendingMissionOrderIndex { get; set; } = -1;

        public DialogueTable DialogueTable => dialogueTable;
        public string CampaignMenuSceneName => campaignMenuSceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (startingMission != null)
            {
                ActiveMission = startingMission;
            }
        }

        /// <summary>미션 선택 화면 또는 씬 로드 직후 호출</summary>
        public void SetActiveMission(MissionDefinition mission)
        {
            ActiveMission = mission;
        }

        /// <summary>대사 ID로 본문 조회 — 없으면 null</summary>
        public string TryGetDialogue(string dialogueId)
        {
            if (dialogueTable == null || string.IsNullOrEmpty(dialogueId))
            {
                return null;
            }

            return dialogueTable.TryGetText(dialogueId);
        }

        /// <summary>에디터/디버그용 — 현재 씬만 다시 로드</summary>
        public static void ReloadActiveScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }
    }
}
