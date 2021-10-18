using ActionGameEngine.Enum;
namespace ActionGameEngine.Input
{
    [System.Serializable]
    //3 bytes
    public struct InputFragment
    {
        public InputItem inputItem;
        public InputFlags flags;

        public bool Check(InputFragment frag)
        {
            return EnumHelper.HasEnum((int)frag.inputItem.input, (int)this.inputItem.input, true) && EnumHelper.HasEnum((int)frag.flags, (int)this.flags, true);
        }
    }
}