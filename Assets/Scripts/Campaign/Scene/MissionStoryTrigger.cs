using Game.BattleAces;
using Game.Campaign.Core;
using Game.Units;
using UnityEngine;

namespace Game.Campaign.Scene
{
    /// <summary>
    /// 연출/대사 트리거 — 플레이어가 구역에 들어오면 대사 ID를 한 번(또는 매번) 알린다.
    /// 실제 재생은 PersistentGameCore + UI가 구독하도록 확장하면 된다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MissionStoryTrigger : MonoBehaviour
    {
        [SerializeField] private string dialogueId;

        [SerializeField] private bool fireOnce = true;

        [SerializeField] private string playerTag = "Player";

        private bool fired;

        /// <summary>런타임 스폰용 — 인스펙터 없이 ID 설정</summary>
        public void SetDialogueIdForRuntime(string id)
        {
            dialogueId = id;
        }

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fireOnce && fired)
            {
                return;
            }

            if (!IsPlayerCollider(other))
            {
                return;
            }

            fired = true;

            if (string.IsNullOrEmpty(dialogueId))
            {
                return;
            }

            string text = PersistentGameCore.Instance != null
                ? PersistentGameCore.Instance.TryGetDialogue(dialogueId)
                : null;

            if (string.IsNullOrEmpty(text))
            {
                text = $"[{dialogueId}]";
            }

#if UNITY_EDITOR
            Debug.Log($"[MissionStoryTrigger] {dialogueId}: {text}");
#endif
            if (BattleAcesStoryBanner.Instance != null)
            {
                BattleAcesStoryBanner.Instance.ShowLine(text);
            }
        }

        private bool IsPlayerCollider(Collider other)
        {
            SelectableUnit su = other.GetComponentInParent<SelectableUnit>();
            if (su != null && su.Team == UnitTeam.Player)
            {
                return true;
            }

            return !string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag);
        }
    }
}
