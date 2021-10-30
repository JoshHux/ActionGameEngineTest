using ActionGameEngine.Enum;
using FixMath.NET;

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


        public FVector2 frameVelocity;
        public HitboxData[] hitboxes;
        public HurtboxData[] hurtboxes;

        public FrameData(int f, StateCondition sc, CancelConditions cc, TimerEvent te, FrameEventFlag ff, FVector2 fv, HitboxData[] hib, HurtboxData[] hub)
        {
            atFrame = f;
            toggleStateConditions = sc;
            toggleCancelConditions = cc;
            timerEvent = te;
            flags = ff;
            frameVelocity = fv;
            hitboxes = hib;
            hurtboxes = hub;
        }

        public bool isValid() { return (atFrame > 0) || (flags > 0) || HasHitboxes() || HasHurtboxes(); }
        public bool HasHitboxes() { return hitboxes != null; }
        public bool HasHurtboxes() { return hurtboxes != null; }
    }
}
