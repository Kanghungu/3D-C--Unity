// =============================================================================
// [Scripts ???: Campaign]
// - DontDestroyOnLoad ? ?? ??·?? ???·??? ?? ? ??? ?? ?? ??.
// - ?? ??? BattleAces ?, ???? "?? ??? ???"? ??.
// - PrototypeBootstrapper? ?? ? CampaignMenu ???? ??.
// =============================================================================
using Game.Campaign.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Campaign.Core
{
    /// <summary>
    /// ??? ?? ????? ? ??? ?? ? ???? ?? ??·?? ???? ???.
    /// ??? ??? ?? ??.
    /// </summary>
    public sealed class PersistentGameCore : MonoBehaviour
    {
        public static PersistentGameCore Instance { get; private set; }

        [Header("?? ? ?? ? ?? ??")]
        [SerializeField] private MissionDefinition startingMission;

        [Header("Shared dialogue table")]
        [SerializeField] private DialogueTable dialogueTable;

        [Header("??? UI")]
        [Tooltip("?? ?? ? '???'?? ??? ? ??(Build Settings ??)")]
        [SerializeField] private string campaignMenuSceneName = "CampaignMenu";

        /// <summary>?? ??? ??(?? ?????? ??)</summary>
        public MissionDefinition ActiveMission { get; private set; }

        /// <summary>?? ?? ???? ?? ?? ???(-1?? MissionDefinition.sortOrder ??)</summary>
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

        /// <summary>?? ?? ? ?? ?? ??? ??</summary>
        public void SetActiveMission(MissionDefinition mission)
        {
            ActiveMission = mission;
        }

        /// <summary>?? ID? ?? ?? ? ??? null</summary>
        public string TryGetDialogue(string dialogueId)
        {
            if (dialogueTable == null || string.IsNullOrEmpty(dialogueId))
            {
                return null;
            }

            return dialogueTable.TryGetText(dialogueId);
        }

        /// <summary>???·???? ? ?? ?? ?? ??</summary>
        public static void ReloadActiveScene()
        {
            Scene active = SceneManager.GetActiveScene();
            string path = active.path;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[PersistentGameCore] ?? ? ??? ?? ?? Reload ? ?????.");
                return;
            }

            SceneManager.LoadScene(path);
        }
    }
}
