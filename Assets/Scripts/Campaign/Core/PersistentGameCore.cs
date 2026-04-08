// =============================================================================
// [Scripts ???: Campaign]
// - DontDestroyOnLoad ? ?? ??ù?? ???ù??? ?? ? ??? ?? ?? ??.
// - ?? ??? BattleAces ?, ???? "?? ??? ???"? ??.
// - PrototypeBootstrapper? ?? ? CampaignMenu ???? ??.
// =============================================================================
using System.Collections.Generic;
using Game.Campaign.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Campaign.Core
{
    /// <summary>
    /// ??? ?? ????? ? ??? ?? ? ???? ?? ??ù?? ???? ???.
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

        /// <summary>?? ?? ? ?? ?? ?? ù ?? 1?? ??</summary>
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

            string text = dialogueTable.TryGetText(dialogueId);
            if (string.IsNullOrEmpty(text) && WarnedMissingDialogueIds.Add(dialogueId))
            {
                Debug.LogWarning("[PersistentGameCore] Dialogue ?? ? ó ?? ?? ??: " + dialogueId);
            }

            return text;
        }

        /// <summary>???ù???? ? ?? ?? ?? ??</summary>
        public static void ReloadActiveScene()
        {
            UnityEngine.SceneManagement.Scene active = SceneManager.GetActiveScene();
            string path = active.path;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[PersistentGameCore] ?? ? ??? ?? ?? Reload ? ?????.");
                return;
            }

            SceneManager.LoadScene(path);
        }

        /// <summary>
        /// ?? ? ?????? ù ?? ??? ??? ??(DontDestroyOnLoad).
        /// </summary>
        public static PersistentGameCore FindOrCreateForBattleScene()
        {
            if (Instance != null)
            {
                return Instance;
            }

            PersistentGameCore existing = Object.FindFirstObjectByType<PersistentGameCore>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            GameObject go = new GameObject("PersistentGameCore");
            return go.AddComponent<PersistentGameCore>();
        }
    }
}
