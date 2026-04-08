using Game.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>V 키 — 자동 표적 모드(가까운 적 우선 / 표적 고정) 토글.</summary>
    public sealed class BattleAcesInputToggles : MonoBehaviour
    {
        private void Update()
        {
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController mc) && mc.IsFinished)
            {
                return;
            }

            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            if (kb == null || !kb.vKey.wasPressedThisFrame)
            {
                return;
            }

            BattleAcesCombatSettings.AutoAcquireMode = BattleAcesCombatSettings.AutoAcquireMode == AutoAcquireMode.PreferNearest
                ? AutoAcquireMode.StickyFocus
                : AutoAcquireMode.PreferNearest;

            BattleAcesCombatSettings.Save();
            ProceduralAudioUtility.PlayUiConfirm();
        }
    }
}
