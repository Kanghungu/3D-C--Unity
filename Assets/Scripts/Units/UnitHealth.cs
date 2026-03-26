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

        private float currentHealth;
        private Transform healthBarRoot;
        private Transform healthBarFill;

        public bool IsAlive => currentHealth > 0f;
        public float Normalized => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;

            if (createHealthBar)
            {
                EnsureHealthBar();
                UpdateHealthBar();
            }
        }

        private void LateUpdate()
        {
            if (healthBarRoot != null && Camera.main != null)
            {
                healthBarRoot.forward = Camera.main.transform.forward;
            }
        }

        public void ApplyDamage(float damage)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHealth -= damage;
            UpdateHealthBar();

            if (currentHealth <= 0f)
            {
                Die();
            }
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

            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Background";
            background.transform.SetParent(healthBarRoot);
            background.transform.localPosition = Vector3.zero;
            background.transform.localScale = new Vector3(1.1f, 0.12f, 0.12f);
            background.GetComponent<Collider>().enabled = false;
            background.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "Fill";
            fill.transform.SetParent(healthBarRoot);
            fill.GetComponent<Collider>().enabled = false;
            fill.GetComponent<Renderer>().material.color = new Color(0.15f, 0.9f, 0.25f);
            healthBarFill = fill.transform;
        }

        private void UpdateHealthBar()
        {
            if (healthBarFill == null)
            {
                return;
            }

            float normalized = Normalized;
            healthBarFill.localScale = new Vector3(Mathf.Max(0.01f, normalized), 0.08f, 0.08f);
            healthBarFill.localPosition = new Vector3(-0.5f + healthBarFill.localScale.x * 0.5f, 0f, 0f);
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}
