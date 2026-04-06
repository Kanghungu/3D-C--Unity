using Game.Settings;
using Game.Units;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// 외부 WAV 없이 짧은 비프/타격음(프로토타입용).
    /// </summary>
    public static class ProceduralAudioUtility
    {
        private static AudioSource globalSource;

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

        private static float EffectiveVolume(float linear)
        {
            return Mathf.Clamp01(linear * GameUserSettings.MasterVolume01);
        }

        private static void PlayClip(AudioClip clip, float volumeLinear)
        {
            if (clip == null)
            {
                return;
            }

            EnsureSource();
            globalSource.PlayOneShot(clip, EffectiveVolume(volumeLinear));
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
            PlayClip(BuildTone(660f, 0.08f), 0.45f);
        }

        /// <summary>생산 완료(아군 유닛 스폰)</summary>
        public static void PlayProductionComplete()
        {
            PlayClip(BuildTone(520f, 0.05f, 0.04f), 0.28f);
            PlayClip(BuildTone(880f, 0.04f, 0.03f), 0.2f);
        }

        /// <summary>목표 임박·갱신 알림</summary>
        public static void PlayObjectivePulse()
        {
            PlayClip(BuildTone(620f, 0.07f, 0.05f), 0.32f);
        }

        /// <summary>승리/패배 스팅</summary>
        public static void PlayResultSting(bool victory)
        {
            float f = victory ? 784f : 196f;
            PlayClip(BuildTone(f, 0.22f, 0.18f), 0.5f);
        }

        /// <summary>유닛 사망 — 팀별 톤</summary>
        public static void PlayUnitDeath(UnitTeam team)
        {
            float f = team == UnitTeam.Player ? 280f : 220f;
            PlayClip(BuildTone(f, 0.06f, 0.04f), 0.32f);
            // 짧은 노이즈 한 번 더
            PlayClip(BuildNoiseBlip(0.05f, 0.35f), team == UnitTeam.Player ? 0.22f : 0.18f);
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
            PlayClip(clip, Mathf.Lerp(0.25f, 0.55f, intensity01));
        }
    }
}
