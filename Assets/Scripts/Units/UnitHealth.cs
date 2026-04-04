using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Minimal health container for prototype combat.
    /// </summary>
    public class UnitHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 35f;
        [SerializeField] private bool createHealthBar = true;
        [SerializeField] private Vector3 healthBarOffset = new(0f, 1.8f, 0f);

        private static Transform cachedCameraTransform;
        private static float nextCameraRefreshTime;

        private float currentHealth;
        private Transform healthBarRoot;
        private Transform healthBarFrame;
        private Renderer healthBarFrameRenderer;
        private Transform healthBarBackground;
        private Renderer healthBarBackgroundRenderer;
        private Transform healthBarDamageFill;
        private Renderer healthBarDamageFillRenderer;
        private Transform healthBarFill;
        private Renderer healthBarFillRenderer;
        private UnitAbilityState abilityState;
        private CombatTarget combatTarget;
        private SelectableUnit selectableUnit;
        private float displayedDamageNormalized = 1f;
        private float lastDamageTime = -99f;

        public bool IsAlive => currentHealth > 0f;
        public float Normalized => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            abilityState = GetComponent<UnitAbilityState>();
            combatTarget = GetComponent<CombatTarget>();
            selectableUnit = GetComponent<SelectableUnit>();
            currentHealth = maxHealth;
            displayedDamageNormalized = 1f;

            if (createHealthBar)
            {
                EnsureHealthBar();
                UpdateHealthBar();
            }
        }

        private void LateUpdate()
        {
            if (healthBarRoot == null)
            {
                return;
            }

            Transform cameraTransform = GetCameraTransform();
            if (cameraTransform != null)
            {
                healthBarRoot.forward = cameraTransform.forward;
            }

            UpdateDamageLag();
            UpdateHealthBarVisuals();
        }

        public void ApplyDamage(float damage)
        {
            if (!IsAlive)
            {
                return;
            }

            if (abilityState != null)
            {
                damage = abilityState.ModifyIncomingDamage(damage);
            }

            float previousNormalized = Normalized;
            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);
            displayedDamageNormalized = Mathf.Max(displayedDamageNormalized, previousNormalized);
            lastDamageTime = Time.time;
            UpdateHealthBar();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            displayedDamageNormalized = Mathf.Max(Normalized, currentHealth / Mathf.Max(0.01f, maxHealth));
            UpdateHealthBar();
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

            if (createHealthBar)
            {
                EnsureHealthBar();
                UpdateHealthBar();
            }
        }

        private void EnsureHealthBar()
        {
            if (healthBarRoot != null)
            {
                healthBarRoot.localPosition = healthBarOffset;
                return;
            }

            GameObject root = new("Health Bar");
            root.transform.SetParent(transform);
            root.transform.localPosition = healthBarOffset;
            root.transform.localRotation = Quaternion.identity;
            healthBarRoot = root.transform;

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(healthBarRoot);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(1.08f, 0.10f, 0.10f);
            frame.GetComponent<Collider>().enabled = false;
            healthBarFrame = frame.transform;
            healthBarFrameRenderer = frame.GetComponent<Renderer>();

            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Background";
            background.transform.SetParent(healthBarRoot);
            background.transform.localPosition = Vector3.zero;
            background.transform.localScale = new Vector3(1.0f, 0.07f, 0.08f);
            background.GetComponent<Collider>().enabled = false;
            healthBarBackground = background.transform;
            healthBarBackgroundRenderer = background.GetComponent<Renderer>();

            GameObject damageFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            damageFill.name = "Damage Fill";
            damageFill.transform.SetParent(healthBarRoot);
            damageFill.GetComponent<Collider>().enabled = false;
            healthBarDamageFill = damageFill.transform;
            healthBarDamageFillRenderer = damageFill.GetComponent<Renderer>();

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "Fill";
            fill.transform.SetParent(healthBarRoot);
            fill.GetComponent<Collider>().enabled = false;
            healthBarFillRenderer = fill.GetComponent<Renderer>();
            healthBarFill = fill.transform;

            UpdateHealthBarVisuals();
        }

        private void UpdateHealthBar()
        {
            if (healthBarFill == null)
            {
                return;
            }

            float normalized = Normalized;
            healthBarFill.localScale = new Vector3(Mathf.Max(0.01f, normalized), 0.06f, 0.06f);
            healthBarFill.localPosition = new Vector3(-0.5f + healthBarFill.localScale.x * 0.5f, 0f, 0f);

            if (healthBarDamageFill != null)
            {
                float damageWidth = Mathf.Max(0.01f, displayedDamageNormalized);
                healthBarDamageFill.localScale = new Vector3(damageWidth, 0.06f, 0.06f);
                healthBarDamageFill.localPosition = new Vector3(-0.5f + damageWidth * 0.5f, 0f, 0f);
            }

            UpdateHealthBarVisuals();
        }

        private void UpdateDamageLag()
        {
            if (healthBarDamageFill == null)
            {
                return;
            }

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
            float damageWidth = Mathf.Max(0.01f, displayedDamageNormalized);
            healthBarDamageFill.localScale = new Vector3(damageWidth, 0.06f, 0.06f);
            healthBarDamageFill.localPosition = new Vector3(-0.5f + damageWidth * 0.5f, 0f, 0f);
        }

        private void UpdateHealthBarVisuals()
        {
            if (healthBarRoot == null)
            {
                return;
            }

            float normalized = Normalized;
            bool isEnemy = combatTarget != null && combatTarget.Team == UnitTeam.Enemy;
            bool selected = selectableUnit != null && selectableUnit.IsSelected;

            // 프레임: 선택 시 흰색, 평상시 팀 색 어둡게
            if (healthBarFrameRenderer != null)
            {
                healthBarFrameRenderer.material.color = selected
                    ? new Color(1f, 1f, 1f, 0.9f)
                    : new Color(0.15f, 0.15f, 0.18f, 0.9f);
            }

            // 배경: 어두운 단색
            if (healthBarBackgroundRenderer != null)
            {
                healthBarBackgroundRenderer.material.color = new Color(0.08f, 0.08f, 0.1f, 1f);
            }

            // 체력 채움: 팀 색 (적=주황, 아군=녹색), 저체력 시 빨간색
            if (healthBarFillRenderer != null)
            {
                Color fillColor = normalized <= 0.35f
                    ? new Color(0.9f, 0.2f, 0.15f)
                    : isEnemy ? new Color(1f, 0.55f, 0.2f) : new Color(0.22f, 0.85f, 0.3f);
                healthBarFillRenderer.material.color = fillColor;
            }

            // 피해 잔상: 단색 어두운 주황
            if (healthBarDamageFillRenderer != null)
            {
                healthBarDamageFillRenderer.material.color = new Color(0.55f, 0.25f, 0.1f);
            }
        }

        private static Transform GetCameraTransform()
        {
            if (cachedCameraTransform == null || Time.unscaledTime >= nextCameraRefreshTime)
            {
                Camera mainCamera = Camera.main;
                cachedCameraTransform = mainCamera != null ? mainCamera.transform : null;
                nextCameraRefreshTime = Time.unscaledTime + 0.5f;
            }

            return cachedCameraTransform;
        }

        private void Die()
        {
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
            if (wreckCollider != null)
            {
                wreckCollider.enabled = false;
            }

            Renderer wreckRenderer = wreck.GetComponent<Renderer>();
            if (wreckRenderer != null)
            {
                wreckRenderer.material.color = wreckColor;
            }

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
            if (soulCollider != null)
            {
                soulCollider.enabled = false;
            }

            Renderer soulRenderer = soul.GetComponent<Renderer>();
            if (soulRenderer != null)
            {
                soulRenderer.material.color = teamColor;
            }

            TimedWorldEffect soulEffect = soul.AddComponent<TimedWorldEffect>();
            soulEffect.Configure(
                0.85f,
                new Vector3(0.18f, 0.52f, 0.18f),
                new Vector3(0f, 0.95f, 0f));

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
                if (shardCollider != null)
                {
                    shardCollider.enabled = false;
                }

                Renderer shardRenderer = shard.GetComponent<Renderer>();
                if (shardRenderer != null)
                {
                    shardRenderer.material.color = Color.Lerp(teamColor, Color.white, 0.12f);
                }

                TimedWorldEffect shardEffect = shard.AddComponent<TimedWorldEffect>();
                shardEffect.Configure(
                    0.62f,
                    Vector3.one * 0.04f,
                    new Vector3(Mathf.Cos(radians) * 0.42f, 0.38f, Mathf.Sin(radians) * 0.42f));
            }
        }
    }
}
