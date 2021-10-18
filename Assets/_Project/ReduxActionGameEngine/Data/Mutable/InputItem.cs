using ActionGameEngine.Enum;
namespace ActionGameEngine.Input
{
    //represents one instance of an input
    [System.Serializable]
    public struct InputItem
    {
        //1<<0 x sign
        //1<<1 x val
        //1<<2 y sign
        //1<<3 y val
        //1<<4 x half
        //1<<5 y half
        //1<<6 btn A
        //1<<7 btn B
        //1<<8 btn C
        //1<<9 btn D
        //1<<10 btn E
        //1<<11 btn F
        //1<<12 btn G
        //1<<13 btn H
        //1<<14 btn start
        //1<<15 btn select
        public short input;

        //for reading from json, so making commands can be more user-readable
        public InputItem(DigitalInput npt)
        {
            input = 0;

            //if has a nonzero x value
            if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.X_NONZERO))
            {
                input |= 1 << 1;
                //if x value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.X_NEGATIVE))
                {
                    input |= 1 << 0;
                }
            }

            //if has a nonzero y value
            if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.Y_NONZERO))
            {
                input |= 1 << 3;
                //if y value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.Y_NEGATIVE))
                {
                    input |= 1 << 2;
                }
            }
        }

        //TODO: add method to convert the information in the short to DigitalInput

    }
}