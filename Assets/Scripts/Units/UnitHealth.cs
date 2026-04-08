using System;
using Game.Audio;
using Game.BattleAces;
using Game.Prototype;
using Game.UI;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Minimal health container for prototype combat.
    /// </summary>
    public class UnitHealth : MonoBehaviour
    {
        /// <summary>? ë‹› ?¬ë§ ??ë°œìƒ ??(?¬ë§???€, ë³‘ì¢…)</summary>
        public static event Action<UnitTeam, UnitArchetype> OnUnitDied;

        /// <summary>?¼í•´ ?ìš© ì§í›„ ???¤ì œ ?ìš© ?¼í•´???°ì¶œÂ·?¬ìš´?œìš©)</summary>
        public event Action<float> Damaged;

        [SerializeField] private float maxHealth = 35f;
        [SerializeField] private bool createHealthBar = true;
        [SerializeField] private Vector3 healthBarOffset = new(0f, 2.2f, 0f);

        private static Camera cachedCamera;
        private static float nextCameraRefreshTime;

        private float currentHealth;
        private float displayedDamageNormalized = 1f;
        private float lastDamageTime = -99f;
        private float nextHitSoundUnscaledTime;

        private UnitAbilityState abilityState;
        private CombatTarget combatTarget;
        private SelectableUnit selectableUnit;
        private Renderer[] cachedRenderers;

        public bool IsAlive => currentHealth > 0f;
        public float Normalized => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            abilityState = GetComponent<UnitAbilityState>();
            combatTarget = GetComponent<CombatTarget>();
            selectableUnit = GetComponent<SelectableUnit>();
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            currentHealth = maxHealth;
            displayedDamageNormalized = 1f;

            // êµ¬í˜• 3D ì²´ë ¥ë°??¤ë¸Œ?íŠ¸ ?œê±°
            Transform oldBar = transform.Find("Health Bar");
            if (oldBar != null) Destroy(oldBar.gameObject);
        }

        private void LateUpdate()
        {
            float normalized = Normalized;
            if (displayedDamageNormalized <= normalized)
            {
                displayedDamageNormalized = normalized;
                return;
            }

            displayedDamageNormalized = Mathf.MoveTowards(
                displayedDamageNormalized,
                normalized,
                Time.deltaTime * (0.45f + (1f - normalized) * 1.8f));
        }

        private void OnGUI()
        {
            if (!createHealthBar || !IsAlive) return;

            if (combatTarget != null && combatTarget.Team == UnitTeam.Enemy)
            {
                BattlefieldFogObject fogObject = GetComponent<BattlefieldFogObject>();
                if ((fogObject != null && fogObject.IsFogged) ||
                    (BattleAcesFogOfWarDebug.Instance != null && !BattleAcesFogOfWarDebug.Instance.IsWorldVisible(transform.position)) ||
                    !HasAnyVisibleRenderer())
                {
                    return;
                }
            }

            ImGuiGameUi.BeginScaledGui();
            Camera cam = GetCamera();
            if (cam == null)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            // ì¹´ë©”???’ì´ê°€ ?¼ì • ?´ìƒ?´ë©´ ì²´ë ¥ë°??¨ê?
            if (cam.transform.position.y > 45f)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            Vector3 screenPos = cam.WorldToScreenPoint(transform.position + healthBarOffset);
            if (screenPos.z < 0f)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            const float barW = 36f;
            const float barH = 4f;
            const float border = 1f;

            float x = screenPos.x - barW * 0.5f;
            float y = Screen.height - screenPos.y - barH * 0.5f;

            bool isEnemy = combatTarget != null && combatTarget.Team == UnitTeam.Enemy;
            bool selected = selectableUnit != null && selectableUnit.IsSelected;

            // ?Œë‘ë¦?
            Color frameColor = selected
                ? new Color(1f, 0.92f, 0.2f)
                : new Color(0.05f, 0.05f, 0.05f);
            DrawRect(new Rect(x - border, y - border, barW + border * 2f, barH + border * 2f), frameColor);

            // ë°°ê²½
            DrawRect(new Rect(x, y, barW, barH), new Color(0.1f, 0.1f, 0.12f));

            // ?¼í•´ ?”ìƒ
            float dmgW = barW * Mathf.Max(0.01f, displayedDamageNormalized);
            DrawRect(new Rect(x, y, dmgW, barH), new Color(0.58f, 0.18f, 0.06f));

            // ì²´ë ¥ ì±„ì?
            float normalized = Normalized;
            float hpW = barW * Mathf.Max(0.01f, normalized);
            Color fillColor = normalized <= 0.35f
                ? new Color(0.95f, 0.18f, 0.12f)
                : isEnemy ? new Color(1f, 0.52f, 0.1f) : new Color(0.18f, 0.9f, 0.28f);
            DrawRect(new Rect(x, y, hpW, barH), fillColor);
            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        private bool HasAnyVisibleRenderer()
        {
            if (cachedRenderers == null || cachedRenderers.Length == 0)
            {
                cachedRenderers = GetComponentsInChildren<Renderer>(true);
            }

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer rendererComponent = cachedRenderers[i];
                if (rendererComponent != null &&
                    rendererComponent.enabled &&
                    rendererComponent.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }

        public void ApplyDamage(float damage)
        {
            if (!IsAlive) return;

            if (abilityState != null)
            {
                damage = abilityState.ModifyIncomingDamage(damage);
            }

            float previousNormalized = Normalized;
            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);
            displayedDamageNormalized = Mathf.Max(displayedDamageNormalized, previousNormalized);
            lastDamageTime = Time.time;

            if (damage > 0f)
            {
                Damaged?.Invoke(damage);
                TryPlayHitFeedback(damage);
                TryNotifyPlayerHitFlash(damage);
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            displayedDamageNormalized = Mathf.Max(Normalized, currentHealth / Mathf.Max(0.01f, maxHealth));
        }

        public void Configure(float newMaxHealth, bool withHealthBar, Vector3? newOffset = null)
        {
            maxHealth = newMaxHealth;
            createHealthBar = withHealthBar;

            if (newOffset.HasValue)
            {
                healthBarOffset = newOffset.Value;
            }

            currentHealth = maxHealth;
            displayedDamageNormalized = 1f;
        }

        /// <summary>ì½”ì–´ ?…ê·¸?ˆì´??????ìµœë? ì²´ë ¥ ì¦ê?ë¶„ë§Œ???„ì¬ ì²´ë ¥??ì¦ê?</summary>
        public void AddMaxHealthBonus(float deltaMax)
        {
            if (deltaMax <= 0f || !IsAlive)
            {
                return;
            }

            maxHealth += deltaMax;
            currentHealth += deltaMax;
        }

        private static Camera GetCamera()
        {
            if (cachedCamera == null || Time.unscaledTime >= nextCameraRefreshTime)
            {
                cachedCamera = Camera.main;
                nextCameraRefreshTime = Time.unscaledTime + 1f;
            }

            return cachedCamera;
        }

        /// <summary>? íƒ ê°€??? ë‹›ë§?ê°€ë²¼ìš´ ?¼ê²©??ì½”ì–´Â·êµ¬ì¡°ë¬??œì™¸, ì§§ì? ì¿¨ë‹¤??.</summary>
        private void TryPlayHitFeedback(float damageAmount)
        {
            if (selectableUnit == null || combatTarget == null || damageAmount <= 0f)
            {
                return;
            }

            if (Time.unscaledTime < nextHitSoundUnscaledTime)
            {
                return;
            }

            nextHitSoundUnscaledTime = Time.unscaledTime + 0.085f;
            float norm = damageAmount / Mathf.Max(1f, maxHealth);
            UnitArchetype arch = selectableUnit != null ? selectableUnit.Archetype : UnitArchetype.Spearman;
            ProceduralAudioUtility.PlayUnitHitLight(Mathf.Clamp01(norm * 3.5f), arch);
        }

        /// <summary>?„êµ° ì½”ì–´Â·? ë‹› ?¼ê²© ???”ë©´ ?Œë˜???¤ì»¤ë¯¸ì‹œ ??ë¹„ìº ?˜ì¸?€ ?¤í‚µ).</summary>
        private void TryNotifyPlayerHitFlash(float damageAmount)
        {
            if (combatTarget == null || combatTarget.Team != UnitTeam.Player || damageAmount <= 0f)
            {
                return;
            }

            if (BattleAcesMatchController.Instance == null)
            {
                return;
            }

            float norm = damageAmount / Mathf.Max(1f, maxHealth);
            BattleAcesPlayerHitFlash.NotifyPlayerDamage(Mathf.Clamp01(norm * 2.2f));
        }

        private void Die()
        {
            UnitTeam team = combatTarget != null ? combatTarget.Team : UnitTeam.Player;
            UnitArchetype archetype = selectableUnit != null ? selectableUnit.Archetype : UnitArchetype.Spearman;
            OnUnitDied?.Invoke(team, archetype);
            SpawnDeathRemains();
            Destroy(gameObject);
        }

        private void SpawnDeathRemains()
        {
            UnitTeam team = combatTarget != null ? combatTarget.Team : UnitTeam.Player;
            bool isFlying = selectableUnit != null && selectableUnit.Definition != null && selectableUnit.Definition.IsFlying;
            Color teamColor = team == UnitTeam.Player ? new Color(0.34f, 0.9f, 1f) : new Color(1f, 0.42f, 0.22f);
            Color wreckColor = Color.Lerp(teamColor, new Color(0.18f, 0.16f, 0.14f), 0.68f);
            Vector3 basePosition = transform.position;

            GameObject wreck = GameObject.CreatePrimitive(isFlying ? PrimitiveType.Sphere : PrimitiveType.Cube);
            wreck.name = "Death Remains";
            wreck.transform.position = basePosition + new Vector3(0f, isFlying ? 0.4f : -0.18f, 0f);
            wreck.transform.localScale = isFlying ? Vector3.one * 0.34f : new Vector3(0.42f, 0.14f, 0.42f);
            Collider wreckCollider = wreck.GetComponent<Collider>();
            if (wreckCollider != null) wreckCollider.enabled = false;

            Renderer wreckRenderer = wreck.GetComponent<Renderer>();
            if (wreckRenderer != null) wreckRenderer.material.color = wreckColor;

            TimedWorldEffect wreckEffect = wreck.AddComponent<TimedWorldEffect>();
            wreckEffect.Configure(
                isFlying ? 1.1f : 1.5f,
                isFlying ? Vector3.one * 0.22f : new Vector3(0.72f, 0.03f, 0.72f),
                isFlying ? new Vector3(0f, -0.8f, 0f) : new Vector3(0f, 0.08f, 0f));

            GameObject soul = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soul.name = "Death Beacon";
            soul.transform.position = basePosition + new Vector3(0f, 0.18f, 0f);
            soul.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
            Collider soulCollider = soul.GetComponent<Collider>();
            if (soulCollider != null) soulCollider.enabled = false;

            Renderer soulRenderer = soul.GetComponent<Renderer>();
            if (soulRenderer != null) soulRenderer.material.color = teamColor;

            TimedWorldEffect soulEffect = soul.AddComponent<TimedWorldEffect>();
            soulEffect.Configure(0.85f, new Vector3(0.18f, 0.52f, 0.18f), new Vector3(0f, 0.95f, 0f));

            for (int index = 0; index < 3; index++)
            {
                float angle = index * 120f;
                float radians = angle * Mathf.Deg2Rad;
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = $"Death Shard {index + 1}";
                shard.transform.position = basePosition + new Vector3(Mathf.Cos(radians) * 0.18f, 0.12f, Mathf.Sin(radians) * 0.18f);
                shard.transform.rotation = Quaternion.Euler(18f, angle, 24f);
                shard.transform.localScale = new Vector3(0.08f, 0.08f, 0.18f);
                Collider shardCollider = shard.GetComponent<Collider>();
                if (shardCollider != null) shardCollider.enabled = false;

                Renderer shardRenderer = shard.GetComponent<Renderer>();
                if (shardRenderer != null) shardRenderer.material.color = Color.Lerp(teamColor, Color.white, 0.12f);

                TimedWorldEffect shardEffect = shard.AddComponent<TimedWorldEffect>();
                shardEffect.Configure(
                    0.62f,
                    Vector3.one * 0.04f,
                    new Vector3(Mathf.Cos(radians) * 0.42f, 0.38f, Mathf.Sin(radians) * 0.42f));
            }
        }
    }
}
