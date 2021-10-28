using ActionGameEngine.Enum;

namespace ActionGameEngine.Data
{
    //struct int FrameData that has the data of a timer to run
    //has data to influence a character's given conditions at any given time
    //7 bytes
    [System.Serializable]
    public struct TimerEvent
    {
        //how long the timer lasts
        public int TimerDuration;

        //conditions to toggle when the timer ends
        public StateCondition conditions;

        //whether or not to have timer last beyond the state,
        //if true, the timer's data will be set to a timer that persists past the state
        //if false, the timer's data will be set to a timer that resets once state is exited
        public bool isPersistent;
        public TimerEvent(int d, StateCondition sc, bool p)
        {
            TimerDuration = d;
            conditions = sc;
            isPersistent = p;
        }
    }
}
