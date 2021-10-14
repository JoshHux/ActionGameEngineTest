using ActionGameEngine.Enum;
namespace ActionGameEngine.Input
{
    [System.Serializable]
    //3 bytes
    public struct InputFragment
    {
        public InputItem input;
        public InputFlags flags;

        public bool Check(InputFragment frag)
        {
            return EnumHelper.HasEnum((int)frag.input.input, (int)this.input.input, true) && EnumHelper.HasEnum((int)frag.flags, (int)this.flags, true);
        }
    }
}