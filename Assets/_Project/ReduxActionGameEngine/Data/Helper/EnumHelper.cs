namespace ActionGameEngine.Enum
{
    public static class EnumHelper
    {
        //checks to see if val has the enums
        public static bool HasEnum(int val, int compare)
        {
            return EnumHelper.HasEnum(val, compare, false);
        }

        //if strict is true, it will check to see if the whole enum is in val, not just if it exists
        //otherwise, it will only check for if any of the enum exists in val
        public static bool HasEnum(int val, int compare, bool strict)
        {
            if (strict)
            {
                return (val & compare) == compare;
            }

            return (val & compare) > 0;
        }
    }
}
