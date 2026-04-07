using Game.Audio;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 작전 시작 후 매치 종료 전까지 저음 앰비언트 루프 — ProceduralAudioUtility + AudioMixer BattleAmbient 그룹.
    /// </summary>
    public sealed class BattleAcesCombatAmbientLoop : MonoBehaviour
    {
        private void LateUpdate()
        {
            bool want = false;
            if (CampaignBattleFlow.Instance != null && CampaignBattleFlow.Instance.IsGameplayStarted)
            {
                want = true;
                if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m) && m.IsFinished)
                {
                    want = false;
                }
            }

            ProceduralAudioUtility.SetBattleAmbientActive(want);
        }

        private void OnDestroy()
        {
            ProceduralAudioUtility.SetBattleAmbientActive(false);
        }
    }
}
