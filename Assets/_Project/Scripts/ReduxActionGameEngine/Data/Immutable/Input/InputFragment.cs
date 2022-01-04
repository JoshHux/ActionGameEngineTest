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

        public bool Check(InputFragment frag, bool checkNot, bool superStrict = false)
        {
            bool has4Way = EnumHelper.HasEnum((uint)this.flags, (uint)InputFlags.DIR_AS_4WAY);

            bool fragHasFlags = EnumHelper.HasEnum((uint)frag.flags, (uint)this.flags, true);
            bool checkItem = this.inputItem.Check(frag.inputItem, has4Way, checkNot, superStrict);
            //UnityEngine.Debug.Log("check :: " + this.inputItem.m_rawValue + " " + frag.inputItem.m_rawValue);
            return checkItem && fragHasFlags;
        }


    }
}