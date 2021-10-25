using ActionGameEngine.Enum;
using ActionGameEngine.Data;
using ActionGameEngine.Gameplay;
using BEPUutilities;
using BEPUUnity;

namespace ActionGameEngine
{
    //object that can move around and detect physics
    [System.Serializable]
    public abstract class LivingObject : GameplayBehavior
    {

        //overall data about our character, stuff like all states and movelist
        protected CharacterData data;
        //rigidbody that we will use to move around and collide with the environment
        protected ShapeBase rb;

        //current status about the character, state, persistent state conditions, current hp, etc.
        protected CharacterStatus status;

        //hitstop timer
        protected CallbackTimer stopTimer;
        //for keeping track of out state
        protected CallbackTimer stateTimer;
        //timer that keeps track of whether or not we get rid of persistent state conditions
        protected CallbackTimer persistentTimer;
        //velocity calculated that we will apply to our rigidbody
        protected BepuVector3 calcVel;
        //for things such as setting velocity, makes sure that that velocity is always being applied
        protected BepuVector4 storedVel;


        protected override void OnAwake()
        {
            base.OnAwake();
            stopTimer = new CallbackTimer();
            stateTimer = new CallbackTimer();
            persistentTimer = new CallbackTimer();
            calcVel = new BepuVector3();
            storedVel = new BepuVector4();

            persistentTimer.OnEnd += ctx => ResetPersistentConditions();
        }

        protected override void OnStart()
        {
            base.OnStart();
            rb = this.GetComponent<ShapeBase>();
        }

        protected override void StateUpdate()
        {
            //if not in Hitstop, tick the gameplay state timers
            if (!stopTimer.TickTimer()) { stateTimer.TickTimer(); persistentTimer.TickTimer(); }
        }

        //faces target only rotating along x-axis
        protected void FaceTargetX(ShapeBase target)
        {
            //record target position
            BepuVector3 refvec = target.position;

            //level the target position along x axis so we don't rotate upwards when turning to face target
            refvec.X = rb.position.X;

            FaceTargetByPosition(refvec);
        }
        //faces target only rotating along y-axis
        protected void FaceTargetY(ShapeBase target)
        {
            //record target position
            BepuVector3 refvec = target.position;

            //level the target position along y axis so we don't rotate upwards when turning to face target
            refvec.Y = rb.position.Y;

            FaceTargetByPosition(refvec);
        }

        //faces target only rotating along z-axis
        protected void FaceTargetZ(ShapeBase target)
        {

            //record target position
            BepuVector3 refvec = target.position;

            //level the target position along z axis so we don't rotate upwards when turning to face target
            refvec.Z = rb.position.Z;

            FaceTargetByPosition(refvec);
        }

        //call to start the hitstop timer
        protected void ApplyHitstop(int time)
        {
            stopTimer.StartTimer(time);
        }

        //faces target without any constraints
        protected void FaceTarget(ShapeBase target)
        {
            FaceTargetByPosition(target.position);
        }

        //callback to reset persistent state conditions
        private void ResetPersistentConditions() { status.SetPersistenConditions(0); }

        //for figuring out what to do when you encounter a flag
        private void FaceTargetByPosition(BepuVector3 target)
        {
            //rotation maths to have rigidbody face target
            rb.rotation = BepuQuaternion.CreateFromRotationMatrix(Matrix.CreateWorldRH(rb.position, target, BepuVector3.Up));
        }



        protected virtual void ProcessTransitionEvents(int transitionEvents)
        {
            if (EnumHelper.HasEnum(transitionEvents, (int)TransitionEvent.KILL_VEL))
            {
                if (EnumHelper.HasEnum(transitionEvents, (int)TransitionEvent.KILL_X_VEL)) { calcVel.X = 0; }
                if (EnumHelper.HasEnum(transitionEvents, (int)TransitionEvent.KILL_Y_VEL)) { calcVel.Y = 0; }
                if (EnumHelper.HasEnum(transitionEvents, (int)TransitionEvent.KILL_Z_VEL)) { calcVel.Z = 0; }
            }
        }

        protected virtual void ProcessFrameData(FrameData frame)
        {

            if (EnumHelper.HasEnum((int)frame.flags, (int)FrameEventFlag.SET_TIMER))
            {
                TimerEvent evnt = frame.timerEvent;
                persistentTimer.StartTimer(evnt.TimerDuration);
                status.SetPersistenConditions(evnt.conditions);
            }

            if (EnumHelper.HasEnum((int)frame.flags, (int)FrameEventFlag.APPLY_VEL))
            {
                if (EnumHelper.HasEnum((int)frame.flags, (int)FrameEventFlag.SET_VEL))
                { calcVel = frame.frameVelocity; }
                else
                { calcVel += frame.frameVelocity; }
            }

            if (frame.HasHitboxes()) { ActivateHitboxes(frame.hitboxes); }
            if (frame.HasHurtboxes()) { ActivateHurtboxes(frame.hurtboxes); }
        }

        protected abstract void ActivateHitboxes(HitboxData[] boxData);
        protected abstract void ActivateHurtboxes(HurtboxData[] boxData);
    }
}
