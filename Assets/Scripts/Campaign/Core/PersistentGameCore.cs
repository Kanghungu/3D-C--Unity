// =============================================================================
// [Scripts ?ˆì´?? Campaign]
// - ?¤í† ë¦?ë¯¸ì…˜ ?ë¦„: ?œì„± ë¯¸ì…˜Â·?€???Œì´ë¸”Â·ë©”??ë³µê? ???´ë¦„. DontDestroyOnLoad.
// - ?„íˆ¬ ê·œì¹™?€ BattleAces???ê³ , ?¬ê¸°?œëŠ” "ë¬´ìŠ¨ ë¯¸ì…˜???£ëŠ”ì§€"ë§?? ì??œë‹¤.
// - PrototypeBootstrapper?€ ë¬´ê? ??CampaignMenu ?±ì—?œë§Œ ë°°ì¹˜?œë‹¤.
// =============================================================================
using Game.Campaign.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Campaign.Core
{
    /// <summary>
    /// ìº í˜??ê³µí†µ ë¶€?¸ìŠ¤?¸ë© ?????„í™˜ ?„ì—??? ì??˜ì–´ ?„ì¬ ë¯¸ì…˜Â·?€???Œì´ë¸?ì°¸ì¡°ë¥?ê³µìœ ?œë‹¤.
    /// ì²??¬ì— ë¹??¤ë¸Œ?íŠ¸ë¡??˜ë‚˜ë§??”ë‹¤.
    /// </summary>
    public sealed class PersistentGameCore : MonoBehaviour
    {
        public static PersistentGameCore Instance { get; private set; }

        [Header("? íƒ ???œì‘ ??ê¸°ë³¸ ë¯¸ì…˜")]
        [SerializeField] private MissionDefinition startingMission;

        [Header("Shared dialogue table")]
        [SerializeField] private DialogueTable dialogueTable;

        [Header("ìº í˜??UI")]
        [Tooltip("?„íˆ¬ ì¢…ë£Œ ??'ë©”ë‰´ë¡? ?ì„œ ë¶ˆëŸ¬?????´ë¦„(Build Settings ?±ë¡)")]
        [SerializeField] private string campaignMenuSceneName = "CampaignMenu";

        /// <summary>?„ì¬ ë¡œë“œ??ë¯¸ì…˜ ?•ì˜(?„íˆ¬ ??ë¶€?¸ìŠ¤?¸ë˜?¼ê? ?½ìŒ)</summary>
        public MissionDefinition ActiveMission { get; private set; }

        /// <summary>ë¯¸ì…˜ ? íƒ ë©”ë‰´?ì„œ ?¤ì • ???¹ë¦¬ ???´ê¸ˆ ?¸ë±???€?¥ìš©(-1 ?´ë©´ MissionDefinition.sortOrder ?¬ìš©)</summary>
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

        /// <summary>ë¯¸ì…˜ ? íƒ ?”ë©´ ?ëŠ” ??ë¡œë“œ ì§í›„ ?¸ì¶œ</summary>
        public void SetActiveMission(MissionDefinition mission)
        {
            ActiveMission = mission;
        }

        /// <summary>?€??IDë¡?ë³¸ë¬¸ ì¡°íšŒ ???†ìœ¼ë©?null</summary>
        public string TryGetDialogue(string dialogueId)
        {
            if (dialogueTable == null || string.IsNullOrEmpty(dialogueId))
            {
                return null;
            }

            return dialogueTable.TryGetText(dialogueId);
        }

        /// <summary>?ë””???”ë²„ê·¸ìš© ???„ì¬ ?¬ë§Œ ?¤ì‹œ ë¡œë“œ</summary>
        public static void ReloadActiveScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }
    }
}
