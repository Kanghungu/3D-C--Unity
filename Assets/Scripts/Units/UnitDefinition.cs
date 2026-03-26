using UnityEngine;

namespace Game.Units
{
    [CreateAssetMenu(menuName = "Game/Unit Definition", fileName = "UnitDefinition")]
    public class UnitDefinition : ScriptableObject
    {
        [SerializeField] private UnitArchetype archetype;
        [SerializeField] private string displayName = "Unit";
        [SerializeField] private PrimitiveType primitiveType = PrimitiveType.Capsule;
        [SerializeField] private Vector3 scale = Vector3.one;
        [SerializeField] private float maxHealth = 40f;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float stoppingDistance = 0.15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 0.8f;
        [SerializeField] private float aggroRange = 7f;
        [SerializeField] private float productionDuration = 4f;
        [SerializeField] private bool usesProjectile;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float projectileArc = 1f;
        [SerializeField] private float splashRadius;
        [SerializeField] private float impactEffectScale = 0.7f;
        [SerializeField] private Color playerColor = new(0.7f, 0.8f, 1f);
        [SerializeField] private Color enemyColor = new(0.9f, 0.35f, 0.35f);

        public UnitArchetype Archetype => archetype;
        public string DisplayName => displayName;
        public PrimitiveType PrimitiveType => primitiveType;
        public Vector3 Scale => scale;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float StoppingDistance => stoppingDistance;
        public float AttackRange => attackRange;
        public float AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;
        public float AggroRange => aggroRange;
        public float ProductionDuration => productionDuration;
        public bool UsesProjectile => usesProjectile;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileArc => projectileArc;
        public float SplashRadius => splashRadius;
        public float ImpactEffectScale => impactEffectScale;
        public Color PlayerColor => playerColor;
        public Color EnemyColor => enemyColor;

        public void Configure(
            UnitArchetype assignedArchetype,
            string assignedDisplayName,
            PrimitiveType assignedPrimitiveType,
            Vector3 assignedScale,
            float assignedMaxHealth,
            float assignedMoveSpeed,
            float assignedStoppingDistance,
            float assignedAttackRange,
            float assignedAttackDamage,
            float assignedAttackCooldown,
            float assignedAggroRange,
            float assignedProductionDuration,
            bool assignedUsesProjectile,
            float assignedProjectileSpeed,
            float assignedProjectileArc,
            float assignedSplashRadius,
            float assignedImpactEffectScale,
            Color assignedPlayerColor,
            Color assignedEnemyColor)
        {
            archetype = assignedArchetype;
            displayName = assignedDisplayName;
            primitiveType = assignedPrimitiveType;
            scale = assignedScale;
            maxHealth = assignedMaxHealth;
            moveSpeed = assignedMoveSpeed;
            stoppingDistance = assignedStoppingDistance;
            attackRange = assignedAttackRange;
            attackDamage = assignedAttackDamage;
            attackCooldown = assignedAttackCooldown;
            aggroRange = assignedAggroRange;
            productionDuration = assignedProductionDuration;
            usesProjectile = assignedUsesProjectile;
            projectileSpeed = assignedProjectileSpeed;
            projectileArc = assignedProjectileArc;
            splashRadius = assignedSplashRadius;
            impactEffectScale = assignedImpactEffectScale;
            playerColor = assignedPlayerColor;
            enemyColor = assignedEnemyColor;
        }
    }
}
