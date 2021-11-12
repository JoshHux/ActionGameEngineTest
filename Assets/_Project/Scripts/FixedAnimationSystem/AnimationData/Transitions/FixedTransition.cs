using ActionGameEngine.Enum;
using FixMath.NET;
namespace FixedAnimationSystem
{
    [System.Serializable]
    public struct FixedTransition
    {
        public int targetState;
        public CompareOperation op;
        //can be as int or Fix64
        public Fix64 value;

        public bool check(long param)
        {
            if (op == 0) { return true; }

            if (EnumHelper.HasEnum((int)op, (int)CompareOperation.AS_FIXEDPOINT))
            {
                Fix64 p = Fix64.FromRaw(param);
                if (EnumHelper.HasEnum((int)op, (int)CompareOperation.EQUALS))
                {
                    return p == value;
                }
                else
                if (EnumHelper.HasEnum((int)op, (int)CompareOperation.NOT_EQUALS))
                {
                    return p != value;

                }
                else

                if (EnumHelper.HasEnum((int)op, (int)CompareOperation.GREATER_THAN))
                {
                    return p > value;

                }
                return p < value;
            }

            if (EnumHelper.HasEnum((int)op, (int)CompareOperation.EQUALS))
            {
                return param == value.m_rawValue;
            }
            else if (EnumHelper.HasEnum((int)op, (int)CompareOperation.NOT_EQUALS))
            {
                return param != value.m_rawValue;

            }
            else if (EnumHelper.HasEnum((int)op, (int)CompareOperation.GREATER_THAN))
            {
                return param > value.m_rawValue;

            }
            return param < value.m_rawValue;
        }

    }
}
