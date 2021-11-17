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
            bool has4Way = EnumHelper.HasEnum((int)this.flags, (int)InputFlags.DIR_AS_4WAY);
            bool fragHasFlags = EnumHelper.HasEnum((int)frag.flags, (int)this.flags, true);
            bool checkItem = this.inputItem.Check(frag.inputItem, has4Way);
            //UnityEngine.Debug.Log("check :: " + this.inputItem.m_rawValue + " " + frag.inputItem.m_rawValue);
            return checkItem && fragHasFlags;
        }


    }
}