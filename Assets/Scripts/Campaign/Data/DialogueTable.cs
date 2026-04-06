using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Campaign.Data
{
    /// <summary>
    /// 대사 ID → 본문. 추후 JSON/로컬라이즈로 옮길 때 이 테이블과 키를 매핑하면 된다.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "Game/Campaign/Dialogue Table", order = 2)]
    public sealed class DialogueTable : ScriptableObject
    {
        [SerializeField] private List<DialogueEntry> entries = new List<DialogueEntry>();

        private readonly Dictionary<string, string> lookup = new Dictionary<string, string>(StringComparer.Ordinal);

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        /// <summary>없는 ID면 null</summary>
        public string TryGetText(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId))
            {
                return null;
            }

            if (lookup.Count == 0)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(dialogueId, out string text) ? text : null;
        }

        private void RebuildLookup()
        {
            lookup.Clear();
            if (entries == null)
            {
                return;
            }

            foreach (DialogueEntry e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.Id))
                {
                    continue;
                }

                lookup[e.Id] = e.Text ?? string.Empty;
            }
        }

        [Serializable]
        public sealed class DialogueEntry
        {
            [Tooltip("미션·브리핑에서 참조하는 고유 ID")]
            public string Id;

            [TextArea(2, 6)]
            public string Text;
        }
    }
}
