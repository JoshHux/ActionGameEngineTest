namespace FixedAnimationSystem
{
    public enum CompareOperation : byte
    {
        LESS_THAN = 1,
        GREATER_THAN = 1 << 1,
        EQUALS = 1 << 2,
        NOT_EQUALS = 1 << 3,
        AS_FIXEDPOINT = 1 << 4,
    }
}

