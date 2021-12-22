namespace ActionGameEngine.Enum
{
    public static class EnumHelper
    {
        //checks to see if val has the enums
        public static bool HasEnum(uint val, uint compare)
        {
            return EnumHelper.HasEnum(val, compare, false);
        }

        //if strict is true, it will check to see if the whole enum is in val, not just if it exists
        //otherwise, it will only check for if any of the enum exists in val
        public static bool HasEnum(uint val, uint compare, bool strict)
        {
            if (strict)
            {
                return (val & compare) == compare;
            }

            return (val & compare) > 0;
        }

        //checks to see if val has the compare enums
        public static int HasEnumInt(uint val, uint compare)
        {
            //should be 0 if enum isn't there and 1 if it is
            uint check = (val & compare) / compare;
            int ret = (int)check;
            return ret;
        }

        //checks to see if val has the compare enums
        public static int HasEnumInt(int val, int compare)
        {
            //should be 0 if enum isn't there and 1 if it is
            int ret = (val & compare) / compare;
            return ret;
        }
    }
}
