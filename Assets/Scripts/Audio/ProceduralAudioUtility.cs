using Game.Settings;
using Game.Units;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// 외부 WAV 없이 짧은 비프/타격음(프로토타입용).
    /// 승패 스팅은 전용 AudioSource; 그 외는 보이스 풀 + 프레임 상한(Normal) + 스팅 구간 짧은 SFX 덕.
    /// </summary>
    public static class ProceduralAudioUtility
    {
        private static AudioSource globalSource;

        /// <summary>승/패 스팅 전용 — AudioMixer 그룹이 있을 때만 사용</summary>
        private static AudioSource resultStingSource;

        private static AudioMixerGroup resultStingMixerGroup;

        /// <summary>전투 중 저음 앰비언트 루프 — BattleAmbient 믹서 그룹 권장</summary>
        private static AudioSource ambientSource;

        private static AudioClip ambientBattleCachedClip;

        private static AudioMixerGroup battleAmbientMixerGroup;

        private static AudioMixer registeredMixer;

        /// <summary>Audio Mixer 창에서 Volume → Expose 한 이름과 동일해야 함</summary>
        private static string exposedBattleAmbientVolumeParam;

        private static string exposedResultStingVolumeParam;

        private static bool warnedAmbientMixerParam;

        private static bool warnedResultStingMixerParam;

        private static bool warnedResultStingPlayFailed;

        /// <summary>전투 SFX 분산용 보이스 — [0]=고우선(히트·명령), [1..]=일반 라운드로빈</summary>
        private static AudioSource[] sfxVoices;

        private const int SfxVoiceCount = 6;

        private static int sfxRoundRobin;

        /// <summary>승패 스팅 구간에 얕은 히트·UI 비프가 뭉개지지 않게 짧은 연타 억제</summary>
        private static float sfxDuckLightCombatUntilUnscaled = -999f;

        private static int sfxFrameId = -1;

        private static int sfxNormalPlaysThisFrame;

        private const int MaxNormalSfxPlaysPerFrame = 14;

        private enum ProceduralAudioPriority
        {
            Normal = 0,
            High = 1,

            /// <summary>믹서 없을 때 스팅 폴백 — 프레임 상한·덕 제외</summary>
            ResultStingFallback = 2
        }

        /// <summary>일반 우선순위만 프레임당 상한 — 초과 시 조용히 드롭</summary>
        private static bool TryConsumeNormalFrameBudget(ProceduralAudioPriority priority)
        {
            if (priority != ProceduralAudioPriority.Normal)
            {
                return true;
            }

            int f = Time.frameCount;
            if (f != sfxFrameId)
            {
                sfxFrameId = f;
                sfxNormalPlaysThisFrame = 0;
            }

            if (sfxNormalPlaysThisFrame >= MaxNormalSfxPlaysPerFrame)
            {
                return false;
            }

            sfxNormalPlaysThisFrame++;
            return true;
        }

        /// <summary>
        /// BattleAces_Main.mixer + 그룹 Volume Expose 후 호출 — GameUserSettings 슬라이더와 연결.
        /// </summary>
        public static void RegisterMixerExposedVolumeParameters(
            AudioMixer mixer,
            string battleAmbientExposedName,
            string resultStingExposedName)
        {
            registeredMixer = mixer;
            exposedBattleAmbientVolumeParam = battleAmbientExposedName;
            exposedResultStingVolumeParam = resultStingExposedName;
            PushMixerVolumesFromUserSettings();
        }

        /// <summary>0~1 선형 볼륨을 믹서 dB로 — Expose Volume 파라미터에 SetFloat</summary>
        public static float Linear01ToMixerDecibels(float linear01)
        {
            linear01 = Mathf.Clamp01(linear01);
            if (linear01 <= 0.0001f)
            {
                return -80f;
            }

            return Mathf.Log10(linear01) * 20f;
        }

        /// <summary>설정 UI·LateUpdate에서 호출 — AudioMixer 그룹 Volume Expose 이름과 매칭</summary>
        public static void PushMixerVolumesFromUserSettings()
        {
            if (registeredMixer == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(exposedBattleAmbientVolumeParam))
            {
                if (!registeredMixer.SetFloat(
                        exposedBattleAmbientVolumeParam,
                        Linear01ToMixerDecibels(GameUserSettings.BattleAmbientVolume01)) &&
                    !warnedAmbientMixerParam)
                {
                    warnedAmbientMixerParam = true;
                    Debug.LogWarning(
                        "[ProceduralAudio] AudioMixer Expose 이름을 확인하세요: '" + exposedBattleAmbientVolumeParam +
                        "' (Battle Aces Root 인스펙터 또는 믹서 창)");
                }
            }

            if (!string.IsNullOrEmpty(exposedResultStingVolumeParam))
            {
                if (!registeredMixer.SetFloat(
                        exposedResultStingVolumeParam,
                        Linear01ToMixerDecibels(GameUserSettings.ResultStingVolume01)) &&
                    !warnedResultStingMixerParam)
                {
                    warnedResultStingMixerParam = true;
                    Debug.LogWarning(
                        "[ProceduralAudio] AudioMixer Expose 이름을 확인하세요: '" + exposedResultStingVolumeParam +
                        "' (Battle Aces Root 인스펙터 또는 믹서 창)");
                }
            }
        }

        private static bool UseMixerExposedAmbientVolume =>
            registeredMixer != null && !string.IsNullOrEmpty(exposedBattleAmbientVolumeParam);

        private static bool UseMixerExposedResultStingVolume =>
            registeredMixer != null && !string.IsNullOrEmpty(exposedResultStingVolumeParam);

        /// <summary>
        /// 승/패 스팅만 지정 믹서 그룹으로 라우팅(볼륨은 믹서에서 조절).
        /// 씬 부트스트랩 등에서 AudioMixer 자식 그룹을 할당하면 됨.
        /// </summary>
        public static void SetResultStingMixerGroup(AudioMixerGroup group)
        {
            resultStingMixerGroup = group;
            if (resultStingSource != null)
            {
                resultStingSource.outputAudioMixerGroup = group;
            }
        }

        private static void EnsureSource()
        {
            if (globalSource != null)
            {
                return;
            }

            GameObject go = new GameObject("ProceduralAudio");
            Object.DontDestroyOnLoad(go);
            globalSource = go.AddComponent<AudioSource>();
            globalSource.playOnAwake = false;
            globalSource.spatialBlend = 0f;
            globalSource.volume = 1f;
        }

        private static void EnsureResultStingSource()
        {
            if (resultStingSource != null)
            {
                return;
            }

            GameObject go = new GameObject("ProceduralAudioResultSting");
            Object.DontDestroyOnLoad(go);
            resultStingSource = go.AddComponent<AudioSource>();
            resultStingSource.playOnAwake = false;
            resultStingSource.spatialBlend = 0f;
            resultStingSource.volume = 1f;
            resultStingSource.outputAudioMixerGroup = resultStingMixerGroup;
        }

        /// <summary>전투 앰비언트 전용 믹서 그룹 — ResultSting 과 분리해 슬라이더 독립</summary>
        public static void SetBattleAmbientMixerGroup(AudioMixerGroup group)
        {
            battleAmbientMixerGroup = group;
            if (ambientSource != null)
            {
                ambientSource.outputAudioMixerGroup = group;
            }
        }

        /// <summary>브리핑 종료 후 ~ 매치 종료 전까지만 true 권장</summary>
        public static void SetBattleAmbientActive(bool active)
        {
            if (!active)
            {
                if (ambientSource != null && ambientSource.isPlaying)
                {
                    ambientSource.Stop();
                }

                return;
            }

            EnsureAmbientSource();
            if (ambientBattleCachedClip == null)
            {
                ambientBattleCachedClip = BuildAmbientBattleLoopClip();
            }

            if (ambientSource.clip != ambientBattleCachedClip)
            {
                ambientSource.clip = ambientBattleCachedClip;
            }

            ambientSource.outputAudioMixerGroup = battleAmbientMixerGroup;
            // 믹서 Expose 로 볼륨 제어 시 소스는 1 — 마스터는 AudioListener
            ambientSource.volume = UseMixerExposedAmbientVolume ? 1f : EffectiveVolume(0.14f);
            if (!ambientSource.isPlaying)
            {
                ambientSource.Play();
            }
        }

        private static void EnsureAmbientSource()
        {
            if (ambientSource != null)
            {
                return;
            }

            GameObject go = new GameObject("ProceduralAudioAmbient");
            Object.DontDestroyOnLoad(go);
            ambientSource = go.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = UseMixerExposedAmbientVolume ? 1f : EffectiveVolume(0.14f);
            ambientSource.outputAudioMixerGroup = battleAmbientMixerGroup;
        }

        private static AudioClip BuildAmbientBattleLoopClip()
        {
            int sampleRate = 44100;
            int samples = sampleRate * 2;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * 52f * t) * 0.11f +
                          Mathf.Sin(2f * Mathf.PI * 71f * t) * 0.065f +
                          Mathf.Sin(2f * Mathf.PI * 0.4f * t) * 0.02f;
            }

            AudioClip clip = AudioClip.Create("ambient_battle", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float EffectiveVolume(float linear)
        {
            return Mathf.Clamp01(linear * GameUserSettings.MasterVolume01);
        }

        private static void EnsureSfxVoicePool()
        {
            EnsureSource();
            if (sfxVoices != null && sfxVoices.Length == SfxVoiceCount)
            {
                return;
            }

            sfxVoices = new AudioSource[SfxVoiceCount];
            sfxVoices[0] = globalSource;
            Transform parent = globalSource.transform;
            for (int i = 1; i < SfxVoiceCount; i++)
            {
                string childName = "SfxVoice_" + i;
                Transform existingChild = parent.Find(childName);
                GameObject go = existingChild != null
                    ? existingChild.gameObject
                    : new GameObject(childName);
                go.transform.SetParent(parent, false);
                AudioSource src = go.GetComponent<AudioSource>();
                if (src == null)
                {
                    src = go.AddComponent<AudioSource>();
                }

                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = 1f;
                sfxVoices[i] = src;
            }
        }

        private static bool ShouldSkipLightSfxDuringResultStingDuck(AudioClip clip, ProceduralAudioPriority priority)
        {
            if (clip == null || priority != ProceduralAudioPriority.Normal)
            {
                return false;
            }

            if (Time.unscaledTime >= sfxDuckLightCombatUntilUnscaled)
            {
                return false;
            }

            // 스팅 레이어와 겹치면 아주 짧은 비프·히트만 삭제(스팅 전용 소스는 별도)
            return clip.length < 0.11f;
        }

        private static AudioSource PickSfxSource(ProceduralAudioPriority priority)
        {
            EnsureSfxVoicePool();
            if (priority == ProceduralAudioPriority.High || priority == ProceduralAudioPriority.ResultStingFallback)
            {
                return sfxVoices[0];
            }

            int idx = 1 + (sfxRoundRobin++ % (SfxVoiceCount - 1));
            return sfxVoices[idx];
        }

        private static void PlayClip(AudioClip clip, float volumeLinear, ProceduralAudioPriority priority = ProceduralAudioPriority.Normal)
        {
            if (clip == null)
            {
                return;
            }

            if (priority == ProceduralAudioPriority.ResultStingFallback)
            {
                EnsureSource();
                globalSource.PlayOneShot(clip, EffectiveVolume(volumeLinear));
                return;
            }

            if (ShouldSkipLightSfxDuringResultStingDuck(clip, priority))
            {
                return;
            }

            if (!TryConsumeNormalFrameBudget(priority))
            {
                return;
            }

            AudioSource src = PickSfxSource(priority);
            src.PlayOneShot(clip, EffectiveVolume(volumeLinear));
        }

        private static AudioClip BuildTone(float frequencyHz, float durationSec, float fadeOut = 0.12f)
        {
            int sampleRate = 44100;
            int samples = Mathf.Max(256, Mathf.CeilToInt(sampleRate * durationSec));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float env = 1f;
                int fadeSamples = Mathf.CeilToInt(sampleRate * fadeOut);
                if (i > samples - fadeSamples)
                {
                    env = (samples - i) / (float)fadeSamples;
                }

                data[i] = Mathf.Sin(2f * Mathf.PI * frequencyHz * t) * 0.35f * env;
            }

            AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>브리핑 시작/확인용 짧은 음</summary>
        public static void PlayUiConfirm()
        {
            PlayClip(BuildTone(660f, 0.08f), 0.45f, ProceduralAudioPriority.High);
        }

        /// <summary>명령 불가(지형 미적중·우선 목표 없음 등) — 짧고 낮은 톤</summary>
        public static void PlayUiCommandRejected()
        {
            PlayClip(BuildTone(200f, 0.055f, 0.04f), 0.2f, ProceduralAudioPriority.High);
        }

        /// <summary>덱 생산 주문 성공(큐에 들어감) — 거절음보다 밝고 짧게</summary>
        public static void PlayDeckOrderQueued()
        {
            PlayClip(BuildTone(440f, 0.038f, 0.03f), 0.2f);
            PlayClip(BuildTone(620f, 0.032f, 0.026f), 0.16f);
        }

        /// <summary>T/Y/U 지휘 코어 강화 구매 성공 — 브리핑 확인음과 구분되는 짧은 중저음</summary>
        public static void PlayCoreUpgradeApplied()
        {
            PlayClip(BuildTone(520f, 0.065f, 0.045f), 0.32f, ProceduralAudioPriority.High);
            PlayClip(BuildTone(380f, 0.045f, 0.035f), 0.18f, ProceduralAudioPriority.High);
        }

        /// <summary>미니맵 클릭 시야 이동 — 짧은 확인음(링 피드백과 짝)</summary>
        public static void PlayUiMinimapPing()
        {
            PlayClip(BuildTone(780f, 0.05f, 0.035f), 0.22f, ProceduralAudioPriority.Normal);
        }

        /// <summary>생산 완료(아군 유닛 스폰)</summary>
        public static void PlayProductionComplete()
        {
            PlayClip(BuildTone(520f, 0.05f, 0.04f), 0.28f);
            PlayClip(BuildTone(880f, 0.04f, 0.03f), 0.2f);
        }

        /// <summary>적 생산 완료 — 아군보다 낮고 짧게(전장이 텅 빈 느낌 방지)</summary>
        public static void PlayEnemyProductionComplete()
        {
            PlayClip(BuildTone(340f, 0.045f, 0.035f), 0.14f);
        }

        /// <summary>집결(랠리) 지점 확정 — 짧은 확인음</summary>
        public static void PlayRallySetConfirm()
        {
            PlayClip(BuildTone(720f, 0.055f, 0.04f), 0.24f);
        }

        /// <summary>적 대상 공격 명령 — 날카로운 짧은 확인음(주스와 톤 맞춤)</summary>
        public static void PlayCombatAttackOrder()
        {
            PlayClip(BuildTone(480f, 0.042f, 0.032f), 0.24f, ProceduralAudioPriority.High);
            PlayClip(BuildTone(920f, 0.028f, 0.022f), 0.18f, ProceduralAudioPriority.High);
        }

        /// <summary>A+이동 등 공격 이동 — 공격 명령보다 살짝 낮고 넓게</summary>
        public static void PlayCombatAttackMoveOrder()
        {
            PlayClip(BuildTone(420f, 0.038f, 0.03f), 0.2f, ProceduralAudioPriority.High);
            PlayClip(BuildTone(740f, 0.032f, 0.026f), 0.17f, ProceduralAudioPriority.High);
        }

        /// <summary>목표 임박·갱신 알림</summary>
        public static void PlayObjectivePulse()
        {
            PlayClip(BuildTone(620f, 0.07f, 0.05f), 0.32f, ProceduralAudioPriority.High);
        }

        /// <summary>적 코어 붕괴 직전(승리 임박) — 짧게 1회만 재생 권장</summary>
        public static void PlayVictoryImminentChime()
        {
            PlayClip(BuildTone(880f, 0.06f, 0.04f), 0.26f, ProceduralAudioPriority.High);
            PlayClip(BuildTone(990f, 0.055f, 0.035f), 0.2f, ProceduralAudioPriority.High);
        }

        /// <summary>승리/패배 스팅 — 짧은 화음 레이어(믹서 그룹 시 동일 라우팅)</summary>
        public static void PlayResultSting(bool victory)
        {
            // 얕은 전투 비프가 스팅과 뭉치지 않게 짧은 구간만 억제(전용 소스 레이어는 그대로)
            sfxDuckLightCombatUntilUnscaled = Time.unscaledTime + 0.55f;
            try
            {
                if (victory)
                {
                    PlayResultStingOneShot(BuildTone(523f, 0.09f, 0.07f), 0.4f);
                    PlayResultStingOneShot(BuildTone(659f, 0.1f, 0.075f), 0.42f);
                    PlayResultStingOneShot(BuildTone(784f, 0.14f, 0.1f), 0.48f);
                    PlayResultStingOneShot(BuildTone(990f, 0.09f, 0.065f), 0.38f);
                }
                else
                {
                    PlayResultStingOneShot(BuildTone(155f, 0.16f, 0.12f), 0.42f);
                    PlayResultStingOneShot(BuildTone(98f, 0.22f, 0.16f), 0.38f);
                    PlayResultStingOneShot(BuildNoiseBlip(0.12f, 0.45f), 0.28f);
                }
            }
            catch (System.Exception e)
            {
                if (!warnedResultStingPlayFailed)
                {
                    warnedResultStingPlayFailed = true;
                    Debug.LogWarning("[ProceduralAudio] 승패 스팅 재생 실패 — 무시: " + e.Message);
                }
            }
        }

        private static void PlayResultStingOneShot(AudioClip clip, float volumeLinear)
        {
            if (clip == null)
            {
                return;
            }

            if (resultStingMixerGroup != null)
            {
                EnsureResultStingSource();
                if (resultStingSource == null)
                {
                    PlayClip(clip, volumeLinear, ProceduralAudioPriority.ResultStingFallback);
                    return;
                }

                resultStingSource.outputAudioMixerGroup = resultStingMixerGroup;
                PushMixerVolumesFromUserSettings();
                float shot = UseMixerExposedResultStingVolume
                    ? Mathf.Clamp01(GameUserSettings.MasterVolume01)
                    : EffectiveVolume(volumeLinear);
                resultStingSource.PlayOneShot(clip, shot);
                return;
            }

            PlayClip(clip, volumeLinear, ProceduralAudioPriority.ResultStingFallback);
        }

        /// <summary>유닛 피격 — 병종별 주파수 살짝 분리(스팸 방지는 호출 측에서)</summary>
        public static void PlayUnitHitLight(float intensity01, UnitArchetype archetype = UnitArchetype.Spearman)
        {
            intensity01 = Mathf.Clamp01(intensity01);
            float baseF = Mathf.Lerp(360f, 540f, intensity01);
            float archHz = archetype switch
            {
                UnitArchetype.Fighter => 95f,
                UnitArchetype.Artillery => -55f,
                UnitArchetype.MobileFortress => -75f,
                UnitArchetype.AirborneCitadel => 110f,
                UnitArchetype.Rifleman => 35f,
                UnitArchetype.ShieldInfantry => -25f,
                UnitArchetype.SpecialWarrior => 50f,
                UnitArchetype.RoyalGuard => 20f,
                UnitArchetype.Outrider => 65f,
                _ => 0f
            };

            float f = Mathf.Clamp(baseF + archHz, 220f, 720f);
            float vol = Mathf.Lerp(0.1f, 0.22f, intensity01);
            PlayClip(BuildTone(f, 0.035f, 0.028f), vol);
            float fHigh = Mathf.Clamp(f * 1.62f, 380f, 980f);
            PlayClip(BuildTone(fHigh, 0.022f, 0.018f), vol * 0.42f);
        }

        /// <summary>유닛 사망 — 팀별 저·고역 + 짧은 노이즈</summary>
        public static void PlayUnitDeath(UnitTeam team)
        {
            bool ally = team == UnitTeam.Player;
            float fLow = ally ? 300f : 240f;
            float fMid = ally ? 520f : 380f;
            PlayClip(BuildTone(fLow, 0.055f, 0.042f), ally ? 0.3f : 0.28f);
            PlayClip(BuildTone(fMid, 0.04f, 0.03f), ally ? 0.22f : 0.2f);
            PlayClip(BuildNoiseBlip(0.06f, 0.38f), ally ? 0.2f : 0.16f);
        }

        /// <summary>일반 구조물 피격(이단 거점 등) — 코어보다 가볍게</summary>
        public static void PlayStructureHit(float intensity01)
        {
            float dur = Mathf.Lerp(0.03f, 0.07f, intensity01);
            PlayClip(BuildNoiseBlip(dur, 0.4f), Mathf.Lerp(0.18f, 0.38f, intensity01));
        }

        private static AudioClip BuildNoiseBlip(float durationSec, float noiseAmount)
        {
            int sampleRate = 44100;
            int samples = Mathf.Max(64, Mathf.CeilToInt(sampleRate * durationSec));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float env = 1f - t;
                data[i] = (Random.value * 2f - 1f) * noiseAmount * env;
            }

            AudioClip clip = AudioClip.Create("noise", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>코어 피격 — 세기 0~1</summary>
        public static void PlayCoreHit(float intensity01)
        {
            float dur = Mathf.Lerp(0.04f, 0.1f, intensity01);
            int sampleRate = 44100;
            int samples = Mathf.Max(128, Mathf.CeilToInt(sampleRate * dur));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float n = (Random.value * 2f - 1f) * 0.5f;
                float t = i / (float)samples;
                float env = 1f - t;
                data[i] = n * env * Mathf.Lerp(0.2f, 0.55f, intensity01);
            }

            AudioClip clip = AudioClip.Create("thud", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            PlayClip(clip, Mathf.Lerp(0.25f, 0.55f, intensity01), ProceduralAudioPriority.High);
        }
    }
}
