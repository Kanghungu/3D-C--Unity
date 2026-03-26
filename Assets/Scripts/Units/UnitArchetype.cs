namespace Game.Units
{
    public enum UnitArchetype
    {
        Vanguard,
        Skirmisher
    }

    public readonly struct UnitArchetypeStats
    {
        public UnitArchetypeStats(string displayName, UnityEngine.PrimitiveType primitiveType, UnityEngine.Vector3 scale, float maxHealth, float moveSpeed, float stoppingDistance, float attackRange, float attackDamage, float attackCooldown, float aggroRange)
        {
            DisplayName = displayName;
            PrimitiveType = primitiveType;
            Scale = scale;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            StoppingDistance = stoppingDistance;
            AttackRange = attackRange;
            AttackDamage = attackDamage;
            AttackCooldown = attackCooldown;
            AggroRange = aggroRange;
        }

        public string DisplayName { get; }
        public UnityEngine.PrimitiveType PrimitiveType { get; }
        public UnityEngine.Vector3 Scale { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float StoppingDistance { get; }
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackCooldown { get; }
        public float AggroRange { get; }
    }

    public static class UnitArchetypeCatalog
    {
        public static UnitArchetypeStats Get(UnitArchetype archetype)
        {
            return archetype switch
            {
                UnitArchetype.Vanguard => new UnitArchetypeStats(
                    "Vanguard",
                    UnityEngine.PrimitiveType.Capsule,
                    new UnityEngine.Vector3(1.1f, 1.1f, 1.1f),
                    60f,
                    5.4f,
                    0.2f,
                    2.1f,
                    15f,
                    0.9f,
                    7f),
                UnitArchetype.Skirmisher => new UnitArchetypeStats(
                    "Skirmisher",
                    UnityEngine.PrimitiveType.Sphere,
                    new UnityEngine.Vector3(1.05f, 1.05f, 1.05f),
                    38f,
                    6.8f,
                    0.15f,
                    7.8f,
                    8f,
                    0.6f,
                    10f),
                _ => new UnitArchetypeStats(
                    "Unit",
                    UnityEngine.PrimitiveType.Capsule,
                    UnityEngine.Vector3.one,
                    40f,
                    6f,
                    0.15f,
                    2f,
                    10f,
                    0.8f,
                    7f)
            };
        }
    }
}
