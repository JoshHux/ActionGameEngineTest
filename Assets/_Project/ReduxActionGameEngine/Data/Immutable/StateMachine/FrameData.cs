using ActionGameEngine.Enum;
using BEPUutilities;

namespace ActionGameEngine.Data
{
    //struct that represents a frame of a state
    //12 bytes
    [System.Serializable]
    public struct FrameData
    {
        //frame in the state that this represents
        public int atFrame;

        public TimerEvent timerEvent;

        //flags for whatever event the frame might haves
        public FrameEventFlag flags;

        public BepuVector3 frameVelocity;

        //call to check if there's a timer event event to check or not
        public bool HasTimerEvent()
        {
            return timerEvent.TimerDuration > 0;
        }
    }
}
