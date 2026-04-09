// =============================================================================
// [Scripts 레이어: Campaign]
// - DontDestroyOnLoad 로 씬 전환 후에도 미션·대화표 유지.
// - 전투 씬은 BattleAces 부트스트랩이 한 판 루프를 책임.
// - PrototypeBootstrapper 는 구형 전장 — 진입은 CampaignMenu 경로 권장.
// =============================================================================
using System.Collections.Generic;
using Game.Campaign.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Campaign.Core
{
    /// <summary>
    /// 캠페인 전역 상태 — 현재 미션·대화표·메뉴 씬 이름.
    /// 씬 전환 후에도 유지(DontDestroyOnLoad).
    /// </summary>
    public sealed class PersistentGameCore : MonoBehaviour
    {
        public static PersistentGameCore Instance { get; private set; }

        [Header("에디터 테스트용 시작 미션(선택)")]
        [SerializeField] private MissionDefinition startingMission;

        [Header("Shared dialogue table")]
        [SerializeField] private DialogueTable dialogueTable;

        [Header("캠페인 UI")]
        [Tooltip("메인 메뉴로 돌아갈 씬 이름(Build Settings 등록)")]
        [SerializeField] private string campaignMenuSceneName = "CampaignMenu";

        /// <summary>현재 플레이 중인 미션(없으면 전투 폴백이 주입될 수 있음)</summary>
        public MissionDefinition ActiveMission { get; private set; }

        /// <summary>캠페인에서 고른 다음 미션 인덱스(-1 이면 미지정)</summary>
        public int PendingMissionOrderIndex { get; set; } = -1;

        public DialogueTable DialogueTable => dialogueTable;
        public string CampaignMenuSceneName => campaignMenuSceneName;

        /// <summary>누락 대사 ID 경고는 ID 당 1회만</summary>
        private static readonly HashSet<string> WarnedMissingDialogueIds = new HashSet<string>();

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

        /// <summary>현재 미션 교체 — 씬 로드 전에 호출</summary>
        public void SetActiveMission(MissionDefinition mission)
        {
            ActiveMission = mission;
        }

        /// <summary>대사 ID 로 본문 조회 — 없으면 null</summary>
        public string TryGetDialogue(string dialogueId)
        {
            if (dialogueTable == null || string.IsNullOrEmpty(dialogueId))
            {
                return null;
            }

            string text = dialogueTable.TryGetText(dialogueId);
            if (string.IsNullOrEmpty(text) && WarnedMissingDialogueIds.Add(dialogueId))
            {
                Debug.LogWarning("[PersistentGameCore] DialogueTable 에 없는 대사 ID: " + dialogueId);
            }

            return text;
        }

        /// <summary>현재 활성 씬을 경로 기준으로 다시 로드</summary>
        public static void ReloadActiveScene()
        {
            UnityEngine.SceneManagement.Scene active = SceneManager.GetActiveScene();
            string path = active.path;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[PersistentGameCore] 씬 경로가 비어 있어 Reload 할 수 없습니다.");
                return;
            }

            SceneManager.LoadScene(path);
        }

        /// <summary>
        /// 전투 씬에 코어가 없으면 빈 오브젝트로 생성(DontDestroyOnLoad 유지).
        /// </summary>
        public static PersistentGameCore FindOrCreateForBattleScene()
        {
            if (Instance != null)
            {
                return Instance;
            }

            PersistentGameCore existing = Object.FindAnyObjectByType<PersistentGameCore>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            GameObject go = new GameObject("PersistentGameCore");
            return go.AddComponent<PersistentGameCore>();
        }
    }
}
