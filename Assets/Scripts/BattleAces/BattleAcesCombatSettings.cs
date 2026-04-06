using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>프로토타입 전투 AI — 자동 표적 전환 방식(V 키로 토글).</summary>
    public enum AutoAcquireMode
    {
        /// <summary>주기적으로 더 가까운 적으로 교체 시도</summary>
        PreferNearest,

        /// <summary>현재 표적이 살아 있는 동안 다른 적으로 덜 자주 갈아탐</summary>
        StickyFocus
    }

    /// <summary>V 표적 모드 — PlayerPrefs 로 세션 간 유지(볼륨·카메라 감도와 동일 패턴).</summary>
    public static class BattleAcesCombatSettings
    {
        private const string PlayerPrefsKeyAutoAcquire = "ba_auto_acquire_mode";

        public static AutoAcquireMode AutoAcquireMode { get; set; } = AutoAcquireMode.PreferNearest;

        static BattleAcesCombatSettings()
        {
            Load();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyOnBoot()
        {
            Load();
        }

        public static void Load()
        {
            int stored = PlayerPrefs.GetInt(PlayerPrefsKeyAutoAcquire, 0);
            AutoAcquireMode = stored == 1 ? AutoAcquireMode.StickyFocus : AutoAcquireMode.PreferNearest;
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeyAutoAcquire, AutoAcquireMode == AutoAcquireMode.StickyFocus ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
