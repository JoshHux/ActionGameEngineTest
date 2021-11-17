using ActionGameEngine.Enum;
using ActionGameEngine.Data;
using ActionGameEngine.Data.Helpers.Static;
using ActionGameEngine.Gameplay;
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
        protected VelcroBody rb;
        //current status about the character, state, persistent state conditions, current hp, etc.
        [UnityEngine.SerializeField] protected CharacterStatus status;

        //hitstop timer
        protected CallbackTimer stopTimer;
        //for keeping track of out state
        protected CallbackTimer stateTimer;
        //timer that keeps track of whether or not we get rid of persistent state conditions
        protected CallbackTimer persistentTimer;
        //velocity calculated that we will apply to our rigidbody
        [UnityEngine.SerializeField] protected FVector2 calcVel;
        //for things such as setting velocity, makes sure that that velocity is always being applied
        protected FVector2 storedVel;


        //----------/OVERRIDE METHODS/----------//


        protected override void OnAwake()
        {
            base.OnAwake();
            stopTimer = new CallbackTimer();
            stateTimer = new CallbackTimer();
            persistentTimer = new CallbackTimer();
            calcVel = new FVector2();
            storedVel = new FVector2();

            //when state timer ends, add the STATE_END flag to the transition flags
            stateTimer.OnEnd += ctx => status.AddTransitionFlags(TransitionFlag.STATE_END);
            persistentTimer.OnEnd += ctx => ResetPersistentConditions();

        }

        protected override void OnStart()
        {
            base.OnStart();
            rb = this.GetComponent<VelcroBody>();
            _renderer = this.GetComponent<RendererBehavior>();
            AssignNewState(0);
        }

        protected override void StateUpdate()
        {
            //recordthe vlocity to do
            calcVel = rb.Velocity;
            status.SetInHitstop(stopTimer.TickTimer());

            //if not in Hitstop, tick the gameplay state timers
            if (!status.GetInHitstop())
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
            if (status.GetInHitstop()) { return; }
            //get the current state conditions
            StateCondition curCond = status.GetStateConditions();
            ProcessStateData(curCond);
            //set the rgidbody's velocity to the new calculated value
            rb.Velocity = calcVel;

        }


        protected override void PostUpdate()
        {
            if (status.GetInHitstop()) { return; }
            //incerment the number of frames we need to render
            helper.renderFrames++;
        }

        protected override void PrepRenderer()
        {
            _renderer.AssignHelper(helper);
            //we passed the helper, reset relevent values
            helper.newState = false;
            helper.renderFrames = 0;
        }


        //----------/PROTECTED METHODS/----------//


        //faces target only rotating along x-axis
        /*protected void FaceTargetX(ShapeBase target)
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
        }*/

        //call to start the hitstop timer
        protected void ApplyHitstop(int time)
        {
            //rb.velocity = BepuVector3.Zero;
            stopTimer.StartTimer(time);
        }

        //faces target without any constraints
        /*protected void FaceTarget(ShapeBase target)
        {
            FaceTargetByPosition(target.position);
        }*/

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
            helper.renderFrames = 0;
            helper.animState = newState.animName;
        }


        //----------/PRIVATE METHODS/----------//


        //callback to reset persistent state conditions
        private void ResetPersistentConditions() { status.SetPersistenConditions(0); }

        //for figuring out what to do when you encounter a flag
        private void FaceTargetByPosition(FVector2 target)
        {
            //rotation maths to have rigidbody face target
            //rb.rotation = BepuQuaternion.CreateFromRotationMatrix(Matrix.CreateWorldRH(rb.position, target, BepuVector3.Up));
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
            if (EnumHelper.HasEnum((uint)curCond, (int)StateCondition.APPLY_GRAV))
            {
                calcVel = new FVector2(calcVel.x, GameplayHelper.ApplyAcceleration(calcVel.y, -data.mass, -data.maxVelocity.y));
            }

            //apply friction in corresponding direction
            if (EnumHelper.HasEnum((uint)curCond, (int)StateCondition.APPLY_FRICTION))
            {
                FVector2 topDownVel = new FVector2(calcVel.x, 0);

                Fix64 friction = data.friction;
                if ((topDownVel.magnitude - friction) < 0)
                { calcVel = new FVector2(0, calcVel.y); }
                else
                { calcVel -= (topDownVel.normalized * friction); }
                //UnityEngine.Debug.Log(calcVel.x);
            }
        }

        protected virtual void ProcessTransitionEvents(TransitionEvent transitionEvents)
        {
            TransitionEvent te = transitionEvents;
            if (EnumHelper.HasEnum((uint)te, (int)TransitionEvent.KILL_VEL))
            {
                if (EnumHelper.HasEnum((uint)te, (int)TransitionEvent.KILL_X_VEL)) { new FVector2(0, calcVel.y); }
                if (EnumHelper.HasEnum((uint)te, (int)TransitionEvent.KILL_Y_VEL)) { new FVector2(calcVel.x, 0); }
                //if (EnumHelper.HasEnum((int)te, (int)TransitionEvent.KILL_Z_VEL)) { calcVel.x = 0; }
            }
        }

        protected virtual void ProcessFrameData(FrameData frame)
        {

            if (EnumHelper.HasEnum((uint)frame.flags, (int)FrameEventFlag.SET_TIMER))
            {
                TimerEvent evnt = frame.timerEvent;
                persistentTimer.StartTimer(evnt.TimerDuration);
                status.SetPersistenConditions(evnt.conditions);
            }

            if (EnumHelper.HasEnum((uint)frame.flags, (int)FrameEventFlag.APPLY_VEL))
            {
                if (EnumHelper.HasEnum((uint)frame.flags, (int)FrameEventFlag.SET_VEL))
                { calcVel = frame.frameVelocity; }
                else
                { calcVel += frame.frameVelocity; }
            }

        }


        //----------/ABSTRACT METHODS/----------//


    }
}
