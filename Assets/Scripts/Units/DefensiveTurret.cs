using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Simple fixed defense that protects a lane or base approach.
    /// </summary>
    [RequireComponent(typeof(CombatTarget))]
    [RequireComponent(typeof(UnitHealth))]
    public class DefensiveTurret : MonoBehaviour
    {
        [SerializeField] private float attackRange = 10f;
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private float attackCooldown = 1.15f;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileArc = 0.4f;
        [SerializeField] private float retargetInterval = 0.5f;

        private CombatTarget owner;
        private UnitHealth health;
        private CombatTarget currentTarget;
        private float cooldownTimer;
        private float retargetTimer;

        private void Awake()
        {
            owner = GetComponent<CombatTarget>();
            health = GetComponent<UnitHealth>();
        }

        private void Update()
        {
            if (health == null || !health.IsAlive)
            {
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (retargetTimer > 0f)
            {
                retargetTimer -= Time.deltaTime;
            }

            if ((currentTarget == null || !currentTarget.IsAlive || Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange) && retargetTimer <= 0f)
            {
                currentTarget = FindClosestEnemy();
                retargetTimer = retargetInterval;
            }

            if (currentTarget == null)
            {
                return;
            }

            FaceTarget(currentTarget.transform.position);

            if (cooldownTimer > 0f)
            {
                return;
            }

            FireProjectile(currentTarget);
            cooldownTimer = attackCooldown;
        }

        public void Configure(float newAttackRange, float newAttackDamage, float newAttackCooldown, float newProjectileSpeed, float newProjectileArc)
        {
            attackRange = newAttackRange;
            attackDamage = newAttackDamage;
            attackCooldown = newAttackCooldown;
            projectileSpeed = newProjectileSpeed;
            projectileArc = newProjectileArc;
        }

        private CombatTarget FindClosestEnemy()
        {
            CombatTarget bestTarget = null;
            float bestDistance = attackRange;

            foreach (CombatTarget target in FindObjectsByType<CombatTarget>())
            {
                if (target == null || target == owner || !target.IsAlive || target.Team == owner.Team)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private void FireProjectile(CombatTarget target)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Turret Shot";
            projectileObject.transform.position = transform.position + Vector3.up * 1.5f;
            projectileObject.transform.localScale = Vector3.one * 0.28f;

            Collider projectileCollider = projectileObject.GetComponent<Collider>();

            if (projectileCollider != null)
            {
                projectileCollider.enabled = false;
            }

            Renderer rendererComponent = projectileObject.GetComponent<Renderer>();
            rendererComponent.material.color = owner.Team == UnitTeam.Player ? new Color(0.35f, 1f, 1f) : new Color(1f, 0.45f, 0.25f);

            UnitProjectile projectile = projectileObject.AddComponent<UnitProjectile>();
            projectile.Initialize(target, owner.Team, attackDamage, projectileSpeed, projectileArc, 0f, 0.45f, rendererComponent.material.color);
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized, Vector3.up),
                520f * Time.deltaTime);
        }
    }
}
