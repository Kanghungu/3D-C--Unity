using Game.Settings;
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
        [SerializeField] private int maxPerTeam;
        [SerializeField] private bool isFlying;
        [SerializeField] private float hoverHeight = 1f;
        [SerializeField] private Color playerColor = new(0.7f, 0.8f, 1f);
        [SerializeField] private Color enemyColor = new(0.9f, 0.35f, 0.35f);

        [Header("RTS 무게감 (이동·애니)")]
        [SerializeField] private MoveProfileData moveProfile = default;
        [SerializeField] private AnimCombatProfileData animCombatProfile = default;
        [SerializeField] private RuntimeAnimatorController optionalAnimatorController;

        public UnitArchetype Archetype => archetype;
        public string DisplayName => ResolveDisplayName(archetype, displayName);
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
        public int MaxPerTeam => maxPerTeam;
        public bool IsFlying => isFlying;
        public float HoverHeight => hoverHeight;
        public Color PlayerColor => playerColor;
        public Color EnemyColor => enemyColor;

        /// <summary>인스펙터에서 한 번도 안 건드렸을 때 기본 무브 프로필</summary>
        public MoveProfileData MoveProfile =>
            IsMoveProfileUninitialized(moveProfile)
                ? MoveProfileData.CreateDefault()
                : moveProfile;

        /// <summary>애니 전투 프로필 — 기본은 레거시(타이머 타격)</summary>
        public AnimCombatProfileData AnimCombatProfile => animCombatProfile;

        public RuntimeAnimatorController OptionalAnimatorController => optionalAnimatorController;

        private void OnValidate()
        {
            if (IsMoveProfileUninitialized(moveProfile))
            {
                moveProfile = MoveProfileData.CreateDefault();
            }

            AnimCombatProfileData defaults = AnimCombatProfileData.CreateDefault();
            if (string.IsNullOrEmpty(animCombatProfile.speedFloatParam) &&
                string.IsNullOrEmpty(animCombatProfile.attackTriggerParam) &&
                string.IsNullOrEmpty(animCombatProfile.dieTriggerParam) &&
                string.IsNullOrEmpty(animCombatProfile.inCombatBoolParam) &&
                animCombatProfile.strikeFallbackTimeoutUnscaled <= 0.01f &&
                animCombatProfile.deathDestroyDelayUnscaled <= 0.01f &&
                !animCombatProfile.useAnimDrivenStrike)
            {
                animCombatProfile = defaults;
                return;
            }

            if (string.IsNullOrEmpty(animCombatProfile.speedFloatParam))
            {
                animCombatProfile.speedFloatParam = defaults.speedFloatParam;
            }

            if (string.IsNullOrEmpty(animCombatProfile.attackTriggerParam))
            {
                animCombatProfile.attackTriggerParam = defaults.attackTriggerParam;
            }

            if (string.IsNullOrEmpty(animCombatProfile.dieTriggerParam))
            {
                animCombatProfile.dieTriggerParam = defaults.dieTriggerParam;
            }

            if (string.IsNullOrEmpty(animCombatProfile.inCombatBoolParam))
            {
                animCombatProfile.inCombatBoolParam = defaults.inCombatBoolParam;
            }

            if (animCombatProfile.useAnimDrivenStrike &&
                animCombatProfile.strikeFallbackTimeoutUnscaled <= 0.01f)
            {
                animCombatProfile.strikeFallbackTimeoutUnscaled = defaults.strikeFallbackTimeoutUnscaled;
            }
        }

        public static string ResolveDisplayName(UnitArchetype archetype, string englishName = null)
        {
            string fallback = string.IsNullOrWhiteSpace(englishName) ? archetype.ToString() : englishName;
            if (GameUserSettings.Language != GameLanguage.Korean)
            {
                return fallback;
            }

            return archetype switch
            {
                UnitArchetype.Spearman => "창병",
                UnitArchetype.ShieldInfantry => "방패병",
                UnitArchetype.Rifleman => "소총병",
                UnitArchetype.Fighter => "전투기",
                UnitArchetype.SpecialWarrior => "특수 전사",
                UnitArchetype.RoyalGuard => "근위병",
                UnitArchetype.Artillery => "포병",
                UnitArchetype.MobileFortress => "기동 요새",
                UnitArchetype.AirborneCitadel => "공중 성채",
                UnitArchetype.Outrider => "선봉 기수",
                _ => fallback
            };
        }

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
            int assignedMaxPerTeam,
            bool assignedIsFlying,
            float assignedHoverHeight,
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
            maxPerTeam = assignedMaxPerTeam;
            isFlying = assignedIsFlying;
            hoverHeight = assignedHoverHeight;
            playerColor = assignedPlayerColor;
            enemyColor = assignedEnemyColor;
        }

        /// <summary>런타임 생성 정의(예: PrototypeGameDatabase)에서 애니 전투 프로필 덮어쓰기</summary>
        public void SetAnimCombatProfileRuntime(AnimCombatProfileData data)
        {
            animCombatProfile = data;
        }

        private static bool IsMoveProfileUninitialized(MoveProfileData profile)
        {
            return !profile.applyToNavAgent &&
                   profile.navAcceleration <= 0.01f &&
                   profile.navAngularSpeedDeg <= 0.01f &&
                   profile.windupMoveSpeedMultiplier <= 0.01f &&
                   !profile.enableCrowdSeparation &&
                   profile.separationRadius <= 0.01f &&
                   profile.separationPushPerSecond <= 0.01f &&
                   profile.flyHorizontalAcceleration <= 0.01f;
        }
    }
}
