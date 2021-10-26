using ActionGameEngine.Enum;
using ActionGameEngine.Data;
using ActionGameEngine.Data.Helpers.Static;
using ActionGameEngine.Gameplay;
using BEPUutilities;
using BEPUUnity;
using FixMath.NET;
namespace ActionGameEngine
{
    //object that can move around and detect physics
    [System.Serializable]
    public abstract class LivingObject : GameplayBehavior
    {
        private RendererBehavior _renderer;
        //assign to RendererObject at end of each main update
        protected RendererHelper helper;
        [UnityEngine.SerializeField]

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


        //----------/OVERRIDE METHODS/----------//


        protected override void OnAwake()
        {
            base.OnAwake();
            stopTimer = new CallbackTimer();
            stateTimer = new CallbackTimer();
            persistentTimer = new CallbackTimer();
            calcVel = new BepuVector3();
            storedVel = new BepuVector4();

            //when state timer ends, add the STATE_END flag to the transition flags
            stateTimer.OnEnd += ctx => status.AddTransitionFlags(TransitionFlag.STATE_END);
            persistentTimer.OnEnd += ctx => ResetPersistentConditions();

            AssignNewState(0);
        }

        protected override void OnStart()
        {
            base.OnStart();
            rb = this.GetComponent<ShapeBase>();
            _renderer = this.GetComponent<RendererBehavior>();
        }

        protected override void StateUpdate()
        {
            //recordthe vlocity to do
            calcVel = rb.velocity;
            //if not in Hitstop, tick the gameplay state timers
            if (!stopTimer.TickTimer())
            {
                stateTimer.TickTimer();
                persistentTimer.TickTimer();

                //check new state because of whatever reason
                if (status.GetCheckState())
                {
                    TryTransitionState();
                }

                FrameData frame = status.GetFrame(stateTimer.GetTimeElapsed());
                if (frame.isValid())
                {
                    ProcessFrameData(frame);
                }
            }
        }

        protected override void SpaxUpdate()
        {
            //get the current state conditions
            StateCondition curCond = status.GetStateConditions();
            ProcessStateData(curCond);
            rb.velocity = calcVel;
        }


        protected override void PostUpdate()
        {
            //incerment the number of frames we need to render
            helper.animFrames++;
        }

        protected override void PrepRenderer()
        {
            _renderer.AssignHelper(helper);
            //we passed the helper, reset relevent values
            helper.newState = false;
            helper.animFrames = 0;
        }


        //----------/PROTECTED METHODS/----------//


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
            rb.velocity = BepuVector3.Zero;
            stopTimer.StartTimer(time);
        }

        //faces target without any constraints
        protected void FaceTarget(ShapeBase target)
        {
            FaceTargetByPosition(target.position);
        }

        //assigns new state
        protected void AssignNewState(int newStateID)
        {

            StateData newState = data.GetStateFromID(newStateID);
            //setting new state information to CharacterStatus
            status.SetNewState(newState);
            status.SetNewStateConditions(data.GetConditionsFromState(newStateID));
            status.SetNewCancelConditions(newState.cancelConditions);

            //reset state timer
            stateTimer.SetTime(newState.duration);

            //tell helper about the new state
            helper.newState = true;
            helper.animFrames = 0;
            helper.animState = newState.animName;
        }


        //----------/PRIVATE METHODS/----------//


        //callback to reset persistent state conditions
        private void ResetPersistentConditions() { status.SetPersistenConditions(0); }

        //for figuring out what to do when you encounter a flag
        private void FaceTargetByPosition(BepuVector3 target)
        {
            //rotation maths to have rigidbody face target
            rb.rotation = BepuQuaternion.CreateFromRotationMatrix(Matrix.CreateWorldRH(rb.position, target, BepuVector3.Up));
        }


        //----------/VIRTUAL METHODS/----------//


        protected virtual void TryTransitionState()
        {
            //get a transition, valid if we found a new state to transition to
            TransitionData transition = data.TryTransitionState(status.GetCurrentStateID(), status.GetCancelConditions(), status.GetTransitionFlags());
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;

                AssignNewState(newStateID);
                //process the TransitionEvent flags that are set
                ProcessTransitionEvents(transition.transitionEvent);
            }
        }

        protected virtual void ProcessStateData(StateCondition curCond)
        {
            //apply gravity based on mass, clamps to max fall speed if exceeded
            if (EnumHelper.HasEnum((int)curCond, (int)StateCondition.APPLY_GRAV))
            {
                calcVel.Y = GameplayHelper.ApplyAcceleration(calcVel.Y, -data.mass, -GameplayHelper.TerminalVel);
            }

            //apply friction in corresponding direction
            if (EnumHelper.HasEnum((int)curCond, (int)StateCondition.APPLY_FRICTION))
            {
                BepuVector3 topDownVel = new BepuVector3(calcVel.X, 0, calcVel.Z);

                Fix64 friction = data.GetFriction();
                if ((topDownVel.Length() - friction) < 0)
                { calcVel = BepuVector3.Zero; }
                else
                { calcVel -= (topDownVel.Normalized() * friction); }
            }
        }

        protected virtual void ProcessTransitionEvents(TransitionEvent transitionEvents)
        {
            if (EnumHelper.HasEnum((int)transitionEvents, (int)TransitionEvent.KILL_VEL))
            {
                if (EnumHelper.HasEnum((int)transitionEvents, (int)TransitionEvent.KILL_X_VEL)) { calcVel.X = 0; }
                if (EnumHelper.HasEnum((int)transitionEvents, (int)TransitionEvent.KILL_Y_VEL)) { calcVel.Y = 0; }
                if (EnumHelper.HasEnum((int)transitionEvents, (int)TransitionEvent.KILL_Z_VEL)) { calcVel.Z = 0; }
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


        //----------/ABSTRACT METHODS/----------//


        protected abstract void ActivateHitboxes(HitboxData[] boxData);
        protected abstract void ActivateHurtboxes(HurtboxData[] boxData);
    }
}
