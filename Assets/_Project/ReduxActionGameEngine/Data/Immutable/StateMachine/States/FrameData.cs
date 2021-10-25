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
        //conditions to toggle for the rest of the state
        public StateCondition toggleStateConditions;
        public CancelConditions toggleCancelConditions;

        public TimerEvent timerEvent;

        //flags for whatever event the frame might haves
        public FrameEventFlag flags;


        public BepuVector3 frameVelocity;
        public HitboxData[] hitboxes;
        public HurtboxData[] hurtboxes;

        public bool isValid() { return (atFrame > 0) || (flags > 0) || HasHitboxes() || HasHurtboxes(); }
        public bool HasHitboxes() { return hitboxes.Length > 0; }
        public bool HasHurtboxes() { return hurtboxes.Length > 0; }
    }
}
