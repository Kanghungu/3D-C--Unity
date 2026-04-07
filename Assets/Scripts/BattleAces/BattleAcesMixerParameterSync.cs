using Game.Audio;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// AudioMixer Expose 볼륨을 설정값과 매 프레임 동기화(슬라이더 외 마스터 볼륨 등과 함께 안정적으로).
    /// </summary>
    public sealed class BattleAcesMixerParameterSync : MonoBehaviour
    {
        private void LateUpdate()
        {
            ProceduralAudioUtility.PushMixerVolumesFromUserSettings();
        }
    }
}
