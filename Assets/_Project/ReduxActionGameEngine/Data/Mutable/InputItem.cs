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
        //1<<14 btn I
        //1<<15 btn J
        public short input;

        public InputItem(DigitalInput npt)
        {
            if (EnumHelper.HasEnum((int)npt, (int)(DigitalInput._1 | DigitalInput._4 | DigitalInput._7 | DigitalInput._3 | DigitalInput._6 | DigitalInput._9))) { }
            input = 0;
        }
    }
}