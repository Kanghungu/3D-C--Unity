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
                    72f,
                    5.5f,
                    0.2f,
                    2.3f,
                    17f,
                    0.9f,
                    7.5f),
                UnitArchetype.Skirmisher => new UnitArchetypeStats(
                    "Skirmisher",
                    UnityEngine.PrimitiveType.Sphere,
                    new UnityEngine.Vector3(1.05f, 1.05f, 1.05f),
                    42f,
                    6.9f,
                    0.15f,
                    8.4f,
                    9f,
                    0.62f,
                    10.5f),
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
