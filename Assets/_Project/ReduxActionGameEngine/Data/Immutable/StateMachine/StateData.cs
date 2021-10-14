using ActionGameEngine.Enum;

namespace ActionGameEngine.Data
{
    //struct to hold all the data of a given character's state
    [System.Serializable]
    public struct StateData
    {
        //ID of state
        public int ID;

        //ID of parent state, if negative, no parent
        public int parentID;

        //how many frames the state goes on for
        public int duration;

        //characteristics of state
        public StateCondition conditions;

        //frames of state
        public FrameData[] frames;
    }
}
