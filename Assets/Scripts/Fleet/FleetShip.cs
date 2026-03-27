using UnityEngine;

namespace Game.Fleet
{
    /// <summary>
    /// Minimal fleet ship used for the separate space-combat experiment scene.
    /// </summary>
    public class FleetShip : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 45f;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float stoppingDistance = 0.8f;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float attackDamage = 9f;
        [SerializeField] private float attackCooldown = 0.8f;

        private FleetShip currentTarget;
        private Vector3 destination;
        private bool hasDestination;
        private float currentHealth;
        private float cooldownTimer;
        private bool isPlayerControlled;
        private Renderer cachedRenderer;
        private GameObject selectionRing;

        public bool IsAlive => currentHealth > 0f;
        public bool IsPlayerControlled => isPlayerControlled;

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            currentHealth = maxHealth;
            CreateSelectionRing();
        }

        private void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (currentTarget != null && (!currentTarget.IsAlive || currentTarget == this))
            {
                currentTarget = null;
            }

            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (distance > attackRange)
                {
                    SetDestination(currentTarget.transform.position);
                }
                else
                {
                    hasDestination = false;
                    TryFire();
                }
            }

            if (!hasDestination)
            {
                return;
            }

            Vector3 flatDestination = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 offset = flatDestination - transform.position;

            if (offset.sqrMagnitude <= stoppingDistance * stoppingDistance)
            {
                hasDestination = false;
                return;
            }

            Vector3 step = offset.normalized * moveSpeed * Time.deltaTime;
            transform.position += step;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(offset.normalized, Vector3.up), 240f * Time.deltaTime);
        }

        public void Configure(bool playerControlled, Color color, float assignedHealth, float assignedMoveSpeed, float assignedAttackRange, float assignedAttackDamage)
        {
            isPlayerControlled = playerControlled;
            maxHealth = assignedHealth;
            currentHealth = maxHealth;
            moveSpeed = assignedMoveSpeed;
            attackRange = assignedAttackRange;
            attackDamage = assignedAttackDamage;

            if (cachedRenderer != null)
            {
                cachedRenderer.material.color = color;
            }
        }

        public void SetDestination(Vector3 worldPoint)
        {
            destination = new Vector3(worldPoint.x, transform.position.y, worldPoint.z);
            hasDestination = true;
            currentTarget = null;
        }

        public void Attack(FleetShip target)
        {
            if (target == null || target == this || target.isPlayerControlled == isPlayerControlled)
            {
                return;
            }

            currentTarget = target;
        }

        public void SetSelected(bool selected)
        {
            if (selectionRing != null)
            {
                selectionRing.SetActive(selected);
            }
        }

        public void ApplyDamage(float damage)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHealth -= damage;

            if (currentHealth <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void TryFire()
        {
            if (currentTarget == null || cooldownTimer > 0f)
            {
                return;
            }

            currentTarget.ApplyDamage(attackDamage);
            cooldownTimer = attackCooldown;
        }

        private void CreateSelectionRing()
        {
            selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionRing.name = "Selection Ring";
            selectionRing.transform.SetParent(transform);
            selectionRing.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            selectionRing.transform.localScale = new Vector3(1.3f, 0.03f, 1.3f);
            selectionRing.GetComponent<Collider>().enabled = false;
            selectionRing.GetComponent<Renderer>().material.color = new Color(0.25f, 1f, 0.9f, 0.9f);
            selectionRing.SetActive(false);
        }
    }
}
