using Game.UI;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces 한정 — 월드 피해 수치를 짧게 띄워 타격 판독을 돕는 IMGUI 풀(연속 타격 합산·강조).
    /// </summary>
    public sealed class BattleAcesWorldDamageNumbers : MonoBehaviour
    {
        public static BattleAcesWorldDamageNumbers Instance { get; private set; }

        private struct Slot
        {
            public bool Active;
            public float ExpireUnscaled;
            public Vector3 WorldPosition;
            public int RoundedDamage;
            public Color Color;
            public int ComboCount;
            public float BirthUnscaled;
            public bool VictimIsEnemy;
        }

        private const int PoolSize = 28;
        private const float LifetimeUnscaled = 0.82f;
        private const float MergeWorldRadius = 1.05f;
        private const float MergeTimeWindowUnscaled = 0.16f;
        private const float HeavyDamageThreshold = 14f;

        private readonly Slot[] slots = new Slot[PoolSize];

        private static Camera cachedCamera;
        private static float nextCameraRefreshUnscaled = -999f;
        private GUIStyle damageLabelStyle;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>피해 숫자 큐 — victimIsEnemy 가 true 이면 적이 맞은 것(주황), false 이면 아군이 맞은 것(청록)</summary>
        public void EnqueueWorldDamage(Vector3 worldPosition, float damageAmount, bool victimIsEnemy)
        {
            if (damageAmount <= 0.01f)
            {
                return;
            }

            float now = Time.unscaledTime;
            int mergeIdx = -1;
            for (int i = 0; i < PoolSize; i++)
            {
                if (!slots[i].Active)
                {
                    continue;
                }

                float age = now - slots[i].BirthUnscaled;
                if (age > MergeTimeWindowUnscaled)
                {
                    continue;
                }

                Vector3 a = slots[i].WorldPosition;
                a.y = 0f;
                Vector3 b = worldPosition;
                b.y = 0f;
                if (Vector3.Distance(a, b) < MergeWorldRadius)
                {
                    if (slots[i].VictimIsEnemy != victimIsEnemy)
                    {
                        continue;
                    }

                    mergeIdx = i;
                    break;
                }
            }

            if (mergeIdx >= 0)
            {
                slots[mergeIdx].RoundedDamage += Mathf.Max(1, Mathf.RoundToInt(damageAmount));
                slots[mergeIdx].ComboCount++;
                slots[mergeIdx].ExpireUnscaled = now + LifetimeUnscaled;
                slots[mergeIdx].BirthUnscaled = now;
                if (slots[mergeIdx].RoundedDamage >= HeavyDamageThreshold)
                {
                    slots[mergeIdx].Color = victimIsEnemy
                        ? new Color(1f, 0.22f, 0.12f, 1f)
                        : new Color(0.55f, 0.95f, 1f, 1f);
                }

                return;
            }

            int idx = -1;
            float oldestExpire = float.MaxValue;
            for (int i = 0; i < PoolSize; i++)
            {
                if (!slots[i].Active)
                {
                    idx = i;
                    break;
                }

                if (slots[i].ExpireUnscaled < oldestExpire)
                {
                    oldestExpire = slots[i].ExpireUnscaled;
                    idx = i;
                }
            }

            if (idx < 0)
            {
                return;
            }

            Vector3 jitter = new Vector3(
                Random.Range(-0.14f, 0.14f),
                1.02f + Random.Range(0f, 0.28f),
                Random.Range(-0.14f, 0.14f));

            slots[idx].Active = true;
            slots[idx].ExpireUnscaled = now + LifetimeUnscaled;
            slots[idx].WorldPosition = worldPosition + jitter;
            slots[idx].RoundedDamage = Mathf.Max(1, Mathf.RoundToInt(damageAmount));
            slots[idx].ComboCount = 1;
            slots[idx].BirthUnscaled = now;
            slots[idx].VictimIsEnemy = victimIsEnemy;
            bool heavy = slots[idx].RoundedDamage >= HeavyDamageThreshold;
            slots[idx].Color = victimIsEnemy
                ? (heavy ? new Color(1f, 0.28f, 0.12f, 1f) : new Color(1f, 0.52f, 0.18f, 1f))
                : (heavy ? new Color(0.45f, 0.98f, 1f, 1f) : new Color(0.32f, 0.9f, 1f, 1f));
        }

        private void OnGUI()
        {
            if (!BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match) || match.IsFinished)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();
            Camera cam = GetCamera();
            if (cam == null)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            if (damageLabelStyle == null)
            {
                damageLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
            }

            Color previousGuiColor = GUI.color;
            int previousLabelFontSize = GUI.skin.label.fontSize;
            float now = Time.unscaledTime;
            for (int i = 0; i < PoolSize; i++)
            {
                if (!slots[i].Active)
                {
                    continue;
                }

                if (now >= slots[i].ExpireUnscaled)
                {
                    slots[i].Active = false;
                    continue;
                }

                Vector3 screenPoint = cam.WorldToScreenPoint(slots[i].WorldPosition);
                if (screenPoint.z <= 0.1f)
                {
                    continue;
                }

                float tRem = Mathf.Clamp01((slots[i].ExpireUnscaled - now) / LifetimeUnscaled);
                float risePixels = (1f - tRem) * 28f;
                float sway = Mathf.Sin((now - slots[i].BirthUnscaled) * 14f) * 6f * (1f - tRem);
                float x = screenPoint.x + sway;
                float y = Screen.height - screenPoint.y - risePixels;
                string text = slots[i].RoundedDamage.ToString();

                int baseFont = slots[i].RoundedDamage >= HeavyDamageThreshold ? 18 : 14;
                damageLabelStyle.fontSize = Mathf.Clamp(
                    Mathf.RoundToInt(Mathf.Lerp(baseFont + 4, baseFont, 1f - tRem)),
                    11,
                    26);
                Color c = slots[i].Color;
                c.a = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, tRem * 5f)) * Mathf.Clamp01(tRem * 1.55f);
                damageLabelStyle.normal.textColor = c;

                Vector2 size = damageLabelStyle.CalcSize(new GUIContent(text));
                float ox = 1.2f;
                Color outline = new Color(0f, 0f, 0f, c.a * 0.78f);
                damageLabelStyle.normal.textColor = outline;
                GUI.Label(new Rect(x - size.x * 0.5f - ox, y - size.y * 0.5f, size.x + 8f, size.y + 6f), text, damageLabelStyle);
                GUI.Label(new Rect(x - size.x * 0.5f + ox, y - size.y * 0.5f, size.x + 8f, size.y + 6f), text, damageLabelStyle);
                GUI.Label(new Rect(x - size.x * 0.5f, y - size.y * 0.5f - ox, size.x + 8f, size.y + 6f), text, damageLabelStyle);
                GUI.Label(new Rect(x - size.x * 0.5f, y - size.y * 0.5f + ox, size.x + 8f, size.y + 6f), text, damageLabelStyle);

                damageLabelStyle.normal.textColor = c;
                GUI.Label(new Rect(x - size.x * 0.5f, y - size.y * 0.5f, size.x + 8f, size.y + 6f), text, damageLabelStyle);

                if (slots[i].ComboCount > 1)
                {
                    GUI.skin.label.fontSize = 10;
                    GUI.color = new Color(1f, 0.92f, 0.35f, c.a * 0.9f);
                    string combo = $"×{slots[i].ComboCount}";
                    Vector2 cs = GUI.skin.label.CalcSize(new GUIContent(combo));
                    GUI.Label(new Rect(x + size.x * 0.42f, y - size.y * 0.62f, cs.x + 4f, cs.y), combo);
                }
            }

            GUI.skin.label.fontSize = previousLabelFontSize;
            GUI.color = previousGuiColor;
            ImGuiGameUi.EndScaledGui();
        }

        private static Camera GetCamera()
        {
            if (cachedCamera == null || Time.unscaledTime >= nextCameraRefreshUnscaled)
            {
                cachedCamera = Camera.main;
                nextCameraRefreshUnscaled = Time.unscaledTime + 0.75f;
            }

            return cachedCamera;
        }
    }
}
