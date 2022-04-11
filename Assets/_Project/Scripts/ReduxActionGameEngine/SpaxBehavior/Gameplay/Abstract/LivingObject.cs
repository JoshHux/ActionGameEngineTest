using ActionGameEngine.Enum;
using ActionGameEngine.Data;
using ActionGameEngine.Data.Helpers.Static;
using ActionGameEngine.Gameplay;
using FixMath.NET;
using FlatPhysics.Unity;
using Spax;
namespace ActionGameEngine
{
    //state end delegate
    public delegate void StateExitEventHandler();

    //object that can move around and detect physics
    [System.Serializable]
    public abstract class LivingObject : GameplayBehavior
    {
        //renderer to animate everything
        private RendererBehavior _renderer;
        //assign to RendererObject at end of each main update
        protected RendererHelper helper;
        [UnityEngine.SerializeField]

        //overall data about our character, stuff like all states and movelist
        protected CharacterData data;
        //rigidbody that we will use to move around and collide with the environment
        protected FBox rb;
        //current status about the character, state, persistent state conditions, current hp, etc.
        [UnityEngine.SerializeField] protected CharacterStatus status;

        //hitstop timer
        [UnityEngine.SerializeField] protected CallbackTimer stopTimer;
        //for keeping track of out state
        [UnityEngine.SerializeField] protected CallbackTimer stateTimer;
        //timer that keeps track of whether or not we get rid of persistent state conditions
        protected CtxCallbackTimer<StateCondition> persistentTimer;
        //velocity calculated that we will apply to our rigidbody
        [UnityEngine.SerializeField] protected FVector2 calcVel;
        //for things such as setting velocity, makes sure that that velocity is always being applied
        protected FVector2 storedVel;

        //for callbacks to latch onto so they're called when we exit a state
        public StateExitEventHandler onStateExit;


        //----------/OVERRIDE METHODS/----------//


        protected override void OnAwake()
        {
            base.OnAwake();
            stopTimer = new CallbackTimer();
            stateTimer = new CallbackTimer();
            persistentTimer = new CtxCallbackTimer<StateCondition>();
            calcVel = new FVector2();
            storedVel = new FVector2();

            stopTimer.OnEnd += ctx => this.EndHitstop();
            //when state timer ends, add the STATE_END flag to the transition flags
            stateTimer.OnEnd += ctx => status.AddTransitionFlags(TransitionFlag.STATE_END);
            persistentTimer.OnEnd += ctx => status.RemovePersistenConditions(persistentTimer.GetData());
            //persistentTimer.OnEnd += ctx => ResetPersistentConditions();

        }

        protected override void OnStart()
        {
            base.OnStart();
            //get the necessary components for gameplay
            rb = this.GetComponent<FBox>();
            _renderer = this.GetComponent<RendererBehavior>();
            SpaxManager.instance.TrackObject(this);

            //set the facing direction
            status.facing = 1;
            helper.facing = 1;
            AssignNewState(0);
            //set correct status values
            //by default, the player should have the airborne flag
            //COMES AFTER DEFAULT STATE ASSIGNMENT -> this is so that our flags aren't overwritten
            status.AddTransitionFlags(TransitionFlag.AIRBORNE);
            status.persistentCond = 0;
        }

        protected override void StateUpdate()
        {
            //if not in Hitstop, tick the gameplay state timers, start of new frame, tick relevant timers forwards by 1 frame
            //WE SHOULD NOT BE REFERENCING OR TICKING THE TIMERS FORWARDS ANYWHERE ELSE

            //we tick the timers here to prevent a scenario where we exit hitstun, and begin ticking the state/stun timer
            //and "losing"  a frame of stun
            //this way, when hitstop ends, we still get at least one frame of stun/state
            if (!status.GetInHitstop())
            {

                stateTimer.TickTimer();
                //UnityEngine.Debug.Log("time elapsed in state :: " + stateTimer.GetTimeElapsed());
                persistentTimer.TickTimer();
            }
            //record the velocity to do
            //status.SetInHitstop(stopTimer.TickTimer());
            stopTimer.TickTimer();


            //check new state because of whatever reason, will apply frame and tick timers forwards when hitstop ends
            //new state is assigned, but doesn't have frame 1 processed until 
            if (status.checkState)
            {
                //try to transition to new state
                TryTransitionState();
            }



            //if not in Hitstop, tick the gameplay state timers
            if (!status.GetInHitstop())
            {
                calcVel = rb.Velocity;

                //get the next frame to process
                FrameData frame = status.GetFrame(stateTimer.GetTimeElapsed());
                if (stateTimer.IsTicking() && frame.isValid())
                {
                    ProcessFrameData(frame);
                }

            }
        }

        protected override void SpaxUpdate()
        {
            //just return if we're in hitstop
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

        }

        protected override void PreRenderer()
        {
            //we passed the helper, reset relevent values
            helper.newState = false;
            helper.renderFrames = 0;
            helper.damageTaken = 0;
            helper.comboHits = 0;
        }

        //----------/PUBLIC METHODS/----------//

        //only relavent for 2d games, returns the direction we are facing
        //facing right by default
        public int GetFacing() { return status.facing; }
        public void SetNewState(int newState)
        {
            this.AssignNewState(newState);
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
            calcVel = rb.Velocity;
            rb.Velocity = FVector2.zero;
            status.inHitstop = true;
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
            //we're assigning a new state, so we're exiting the previous state, call the delegate
            onStateExit?.Invoke();
            //UnityEngine.Debug.Log("transitioning to : " + newStateID + " " + status.GetTransitionFlags());
            //get the new state we want to enter
            StateData newState = data.GetStateFromID(newStateID);
            //setting new state information to CharacterStatus
            status.SetNewState(newState);
            //reset state timer
            stateTimer.StartTimer(newState.duration);
            status.SetNewStateConditions(data.GetConditionsFromState(newStateID));

            var newCC = data.GetCancelsFromState(newStateID);
            status.SetNewCancelConditions(newCC);
            //new state found, remove transition flags
            //status.SetNewTransitionFlags(0);
            status.RemoveTransitionFlags(TransitionFlag.STATE_END);
            status.RemoveTransitionFlags(TransitionFlag.GOT_HIT);


            //any housekeeping or exceptions we need to cover
            CleanUpNewState();

            //flag to check new state, just in case
            status.checkState = true;

            //tell helper about the new state
            helper.newState = true;
            helper.renderFrames = 0;
            helper.animState = newState.stateID;
        }


        //----------/PRIVATE METHODS/----------//
        //what to do when hitstop ends
        private void EndHitstop()
        {
            UnityEngine.Debug.Log("ending hitstop " + rb.gameObject.name);
            status.inHitstop = false;
            rb.Velocity = this.calcVel;
        }

        //callback to reset persistent state conditions
        private void ResetPersistentConditions() { status.SetPersistenConditions(0); }

        //for figuring out what to do when you encounter a flag
        private void FaceTargetByPosition(FVector2 target)
        {
            //rotation maths to have rigidbody face target
            //rb.rotation = BepuQuaternion.CreateFromRotationMatrix(Matrix.CreateWorldRH(rb.position, target, BepuVector3.Up));
        }


        //----------/VIRTUAL METHODS/----------//

        //call when we assign a new state, takes care of any housekeeping or special cases we need to cover
        protected virtual void CleanUpNewState() { return; }
        protected virtual void TryTransitionState()
        {
            //get a transition, valid if we found a new state to transition to
            TransitionData transition = data.TryTransitionState(status.GetCurrentStateID(), status.GetCancelConditions(), status.GetTransitionFlags(), status.facing, status.resources);
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;

                //get the current state before the transition
                StateData curState = status.currentState;
                int curStateID = curState.stateID;
                //process the exitEvents flags before transitioning
                TransitionEvent exitEvents = data.GetExitFromState(curStateID);
                ProcessTransitionEvents(exitEvents);

                //process the TransitionEvent flags that are set before you transition to the new state
                ProcessTransitionEvents(transition.transitionEvent, status.resources);
                AssignNewState(newStateID);

                //we assign it again since we know that the current state should be the new state 
                curState = status.currentState;
                curStateID = curState.stateID;
                //process the enterEvents flags before transitioning
                TransitionEvent enterEvents = data.GetEnterFromState(curStateID);
                ProcessTransitionEvents(enterEvents);
            }
            else
            {
                status.checkState = false;
            }
        }

        //reads the state conditions and applies certain effects based on that
        protected virtual void ProcessStateData(StateCondition curCond)
        {
            //don't continue if there aren't any conditions to check
            if (curCond == 0) { return; }

            //apply gravity based on mass, clamps to max fall speed if exceeded
            if (EnumHelper.HasEnum((uint)curCond, (int)StateCondition.APPLY_GRAV))
            {
                calcVel = new FVector2(calcVel.x, GameplayHelper.ApplyAcceleration2(calcVel.y, -data.mass, -data.maxVelocity.y));
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

        protected virtual void ProcessTransitionEvents(TransitionEvent transitionEvents, ResourceData resourceData = new ResourceData())
        {

            //int version of event flags to look at
            int te = (int)transitionEvents;

            /*Branchless resource draining*/

            //int version of flag, for convenience
            int drainRsrcFlag = (int)TransitionEvent.DRAIN_RESOURCES;
            //do we have the flags? 1 or 0
            int drainRsrc = EnumHelper.HasEnumInt(te, drainRsrcFlag);
            //resources that we drain are all 0 if we don't have the drain flag
            var toDrain = resourceData * drainRsrc;
            //drain the resource
            status.resources = status.resources - toDrain;

            //for updating the health bar, addition since damageTaken is amount of health lost
            helper.damageTaken += toDrain.Health;


            /*branchless velocity killing (I'm proud of this)*/

            //int mask for the kill velocity flags
            int killXFlag = (int)TransitionEvent.KILL_X_VEL;
            int killYFlag = (int)TransitionEvent.KILL_Y_VEL;
            //int killZFlag=(int)TransitionEvent.KILL_Z_VEL;

            //do we have the flags? 1 or 0
            int killXVel = EnumHelper.HasEnumInt(te, killXFlag);
            int killYVel = EnumHelper.HasEnumInt(te, killYFlag);
            //int killZVel=EnumHelper.HasEnumInt(te,killZFlag);

            //flip 1 and 0, since we want to set 0 if true (aka 1)
            int keepX = killXVel ^ 1;
            int keepY = killYVel ^ 1;
            //int keepZ=killZVel^1;

            //record the old velcity
            Fix64 oldXVel = calcVel.x;
            Fix64 oldYVel = calcVel.y;
            //Fix64 oldZVel=calcVel.z;

            //multiply by 1 or 0 depending on if we want to keep or kill the velocity
            Fix64 newXVel = oldXVel * keepX;
            Fix64 newYVel = oldYVel * keepY;
            //Fix64 newZVel=oldZVel*kee[Z;

            //make the new velocity vector
            //FVector3 newVel = new FVector3(newXVel, newYVel, newZVel);
            FVector2 newVel = new FVector2(newXVel, newYVel);

            //set the new velocity
            calcVel = newVel;

        }

        protected virtual void ProcessFrameData(FrameData frame)
        {
            //flags from frame, casted as an int
            int flags = (int)frame.flags;
            //velocity from frame
            FVector2 frameVel = frame.frameVelocity;
            //resources from the frame
            var frameRsrc = frame.resources;
            //Set and Apply Velocity flags as ints
            int applyVelFlag = (int)FrameEventFlag.APPLY_VEL;
            int setVelFlag = (int)FrameEventFlag.SET_VEL;
            //Gain and Drain Velocity flags as ints
            int gainRsrcFlag = (int)FrameEventFlag.GAIN_RESOURCES;
            int drainRsrcFlag = (int)FrameEventFlag.DRAIN_RESOURCES;

            if (EnumHelper.HasEnum((uint)flags, (int)FrameEventFlag.SET_TIMER))
            {
                TimerEvent evnt = frame.timerEvent;
                int time = evnt.TimerDuration;
                StateCondition cond = evnt.conditions;
                persistentTimer.StartTimer(time, cond);
                status.AddPersistenConditions(cond);
            }

            /*branchless resource gaining and draining*/

            //is 1 or 0, depending on flag exitence
            int gainRsrc = EnumHelper.HasEnumInt(flags, gainRsrcFlag);
            //will be -1 if has the drain flag
            int drainRsrc = EnumHelper.HasEnumInt(flags, drainRsrcFlag) * -1;
            //resources to add
            var rsrcPlus = frameRsrc * gainRsrc;
            //resources to take away
            var rsrcMinus = frameRsrc * drainRsrc;
            //net resources
            var rsrcNet = rsrcPlus + rsrcMinus;
            //apply the resource change to status
            status.resources = status.resources + rsrcNet;

            //for updating the health bar, subtraction since damageTaken is amount of health lost
            helper.damageTaken -= rsrcNet.Health;

            /*branchless velocity application (I'm also proud of this)*/

            //should only be 1 or 0
            //1 if flag exists
            //0 if flag does not exist
            int applyVel = EnumHelper.HasEnumInt(flags, applyVelFlag);

            //same as above, but should still be 0 if only apply vel flag exists, since that flag's int value is smaller that set vel
            int setVel = EnumHelper.HasEnumInt(flags, setVelFlag);
            //UnityEngine.Debug.Log("frame detected");
            //if (applyVel > 0) { UnityEngine.Debug.Log("applying velocity"); }

            //1 if setVel is 0 and 0 if setVel is 1
            //a flip of 0 to 1 and 1 to 0
            int keepVel = setVel ^ 1;

            //multiplied so that if we don't want to apply velocity, we don't
            FVector2 impulse = frameVel * applyVel;

            //if we want to set the velocity, we just multiply the velocity we have now by 0
            FVector2 keptVel = calcVel * keepVel;

            //we then add the two vectors together to get the new velocity, set it and we're done!
            FVector2 newVel = impulse + keptVel;

            //set new velocity
            calcVel = newVel;


        }


        //----------/ABSTRACT METHODS/----------//


    }
}
