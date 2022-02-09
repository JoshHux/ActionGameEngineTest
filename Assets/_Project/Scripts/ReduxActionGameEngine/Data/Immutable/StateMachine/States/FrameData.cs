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
        public bool thing;


        //flags for whatever event the frame might haves
        public FrameEventFlag flags;


        //[ShowWhen("flags", FrameEventFlag.SET_TIMER)]
        public TimerEvent timerEvent;
        public ResourceData resources;
        public FVector2 frameVelocity;
        public HitboxData[] hitboxes;
        public HurtboxData[] hurtboxes;

        public FrameData(int f, StateCondition sc, CancelConditions cc, FrameEventFlag ff, TimerEvent te, ResourceData rd, FVector2 fv, HitboxData[] hib, HurtboxData[] hub)
        {
            this.thing = true;
            this.atFrame = f;
            this.toggleStateConditions = sc;
            this.toggleCancelConditions = cc;
            this.flags = ff;
            this.timerEvent = te;
            this.resources = rd;
            this.frameVelocity = fv;
            this.hitboxes = hib;
            this.hurtboxes = hub;
        }

        public bool isValid() { return (atFrame > 0) || (flags > 0) || HasHitboxes() || HasHurtboxes(); }
        public bool HasHitboxes() { return EnumHelper.HasEnum((uint)flags, (uint)FrameEventFlag.ACTIVATE_HITBOXES); }
        public bool HasHurtboxes() { return EnumHelper.HasEnum((uint)flags, (uint)FrameEventFlag.ACTIVATE_HURTBOXES); }
    }
}
