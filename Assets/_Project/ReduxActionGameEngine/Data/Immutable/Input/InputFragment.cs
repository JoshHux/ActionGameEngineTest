using ActionGameEngine.Enum;
namespace ActionGameEngine.Input
{
    [System.Serializable]
    //3 bytes
    public struct InputFragment
    {
        public InputItem inputItem;
        public InputFlags flags;

        public InputFragment(InputItem newItem)
        {
            inputItem = newItem;
            flags = 0;
        }

        public InputFragment(InputItem newItem, InputFlags newFlags)
        {
            inputItem = newItem;
            flags = newFlags;
        }

        public bool Check(InputFragment frag)
        {
            return this.inputItem.Check(frag.inputItem, EnumHelper.HasEnum((int)this.flags, (int)InputFlags.DIR_AS_4WAY, true)) && EnumHelper.HasEnum((int)frag.flags, (int)this.flags, true);
        }
    }
}