using UnityEngine;
using Spax.StateMachine;
using Spax.Input;
using UnityEngine.InputSystem;
using BEPUUnity;
using FixMath.NET;

public class FighterController : MovementObject, IDamageable, IDamager
{
    public delegate void OnHealthChangedAction(int current, int maxHealth);
    public ShapeBase grabAnchor;
    public OnHealthChangedAction OnHealthChanged; //e.g. for healthbar

    public int playerID; //change for later 

    //hotfix for getting the correct character data, delete later
    public int charDataID = 0;



    //used check if we need to apply at this frame
    [SerializeField] private CharacterFrame refFrame;

    private BEPUutilities.Vector4 storedVelocity;


    protected override void OnAwake()
    {
        base.OnAwake();

        //ENABLE ALL ACTIONS BEFORE USING CALLBACKS, I DON'T KNOW WHY I HAVE TO DO IT, I JUST DO
        InputAction action = PlayerInput.actions["Dash"];
        action.performed += ctx => CallbackButtonInput(Button.X);
        action.canceled += ctx => CallbackButtonInput(Button.X);
        action.Enable();

        action = PlayerInput.actions["LightAttack"];
        action.performed += ctx => CallbackButtonInput(Button.I);
        action.canceled += ctx => CallbackButtonInput(Button.I);
        action.Enable();

        action = PlayerInput.actions["MediumAttack"];
        action.performed += ctx => CallbackButtonInput(Button.J);
        action.canceled += ctx => CallbackButtonInput(Button.J);
        action.Enable();

        action = PlayerInput.actions["HeavyAttack"];
        action.performed += ctx => CallbackButtonInput(Button.K);
        action.canceled += ctx => CallbackButtonInput(Button.K);
        action.Enable();

        action = PlayerInput.actions["Grab"];
        action.performed += ctx => CallbackButtonInput(Button.L);
        action.canceled += ctx => CallbackButtonInput(Button.L);
        action.Enable();

        // controls.Player.Move.Enable();
        // controls.Player.Jump.Enable();
        // controls.Enable();

        // controls.Player.Move.performed += ctx => CallbackDirectionInput(ctx.ReadValue<Vector2>());
        // controls.Player.Move.canceled += ctx => CallbackDirectionInput(ctx.ReadValue<Vector2>());
        // controls.Player.Jump.performed += ctx => CallbackButtonInput(Button.W);
        // controls.Player.Jump.canceled += ctx => CallbackButtonInput(Button.W);
        // controls.Player.Dash.performed += ctx => CallbackButtonInput(Button.X);
        // controls.Player.Dash.canceled += ctx => CallbackButtonInput(Button.X);
        //
        // controls.Player.LightAttack.performed += ctx => CallbackButtonInput(Button.I);
        // controls.Player.LightAttack.canceled += ctx => CallbackButtonInput(Button.I);
        // controls.Player.MediumAttack.performed += ctx => CallbackButtonInput(Button.J);
        // controls.Player.MediumAttack.canceled += ctx => CallbackButtonInput(Button.J);

    }



    // Start is called before the first frame update
    protected override void OnStart()
    {
        base.OnStart();


        //data.moveCondition.facingRight = false;

        stopTimer.onEnd += OnHitstopEnd;

        OnNonGrounded();

        storedVelocity.W = -1;


    }

    //call ONCE before your fixed update stuff, just an organizational thing
    protected override void InputUpdate()
    {
        base.InputUpdate();
    }

    protected override void StateCleanUpdate()
    {

        base.StateCleanUpdate();
        //will have this condition if the opponent is attacking
        if ((data.xtraCondition & TransitionCondition.CAUSE_FOR_BLOCK) > 0)
        {
            int exitCond = 0;
            StateFrameData newState = data.GetCommand(out exitCond);

            if (newState != null)
            {
                /*Debug.Log("new state -- " + newState.stateID);
                if (newState != null && newState.stateID == 49)
                {
                    Debug.Log("kljfa;dsjk;ldsfa");
                }*/
                TransitionNewState(exitCond, newState);
                timer.StartTimer(enemyTarget.GetTimeLeftInState());
            }

        }
        pos = rb.position;
    }


    // Update is called once per frame
    protected override void PreUpdate()
    {
        if (!enemyTarget)
        {
            Debug.LogError("No enemy target");
            return;
        }
        if (!stopTimer.IsTicking())
        {
            //if there is a frame to apply some stuff, do it

            if (data.GetState().FindFrame(timer.ElapsedTime(), ref refFrame))
            {
                ApplyStateFrame(refFrame);

            }

            StateConditions curCond = data.GetStateConditions();

            //for holding change in velocity for faster access
            Fix64 accel = 0;
            //holds the x-axis input for faster access
            Fix64 xInput = StickInput.Length();

            //look, I don't like doing this, but I HAVE TO access the current enemy position like this, otherwise, one of the players will continuously think it's grounded
            BEPUutilities.Vector3 refvec = enemyTarget.pos;

            //rb.SetAngVelocity(BEPUutilities.Vector3.Zero);
            if ((curCond & StateConditions.CAN_TURN) > 0)
            {
                refvec.Y = rb.position.Y;
                refvec = rb.position - refvec;
                rb.rotation = BEPUutilities.Quaternion.CreateFromRotationMatrix(BEPUutilities.Matrix.CreateWorldRH(rb.position, refvec, BEPUutilities.Vector3.Up));
                rb.velocity = BEPUutilities.Vector3.Zero;
                //rb.LookAt(new BEPUutilities.Vector3(refvec.X, rb.position.Y, refvec.Z));
            }

            //if the state allows for gravity application
            if ((curCond & StateConditions.APPLY_GRAV) > 0)
            {
                if ((calcVelocity.Y - data.GetGravForce()) <= (-data.moveStats.maxFallSpeed))
                {
                    calcVelocity.Y = -data.moveStats.maxFallSpeed;
                }
                else
                {
                    calcVelocity.Y -= data.GetGravForce();
                }
            }

            //read if the player wants to move
            //if the state allows for movement
            if (((curCond & StateConditions.CAN_MOVE) > 0) && (xInput != 0) && (storedVelocity.W < 0))
            {


                bool isWalking = (curCond & StateConditions.WALKING) == StateConditions.WALKING;

                //hold the acceleration for quicker access
                accel = data.GetAcceleration();
                BEPUutilities.Vector3 accelVec = new BEPUutilities.Vector3(StickInput.X * accel, Fix64.Zero, StickInput.Y * accel);

                BEPUutilities.Vector3 refCalVel = new BEPUutilities.Vector3(calcVelocity.X, Fix64.Zero, calcVelocity.Z);

                if (Fix64.Abs((refCalVel + accelVec).Length()) >= data.GetMaxSpeed())
                {
                    calcVelocity = accelVec.Normalized() * data.GetMaxSpeed() + new BEPUutilities.Vector3(0, calcVelocity.Y, 0);

                }
                else
                {

                    calcVelocity += accelVec;
                    //Debug.Log("calcVelocity");

                }
            }
            //if the player doesn't want to move//if the state applies friction

            else if ((curCond & StateConditions.APPLY_FRICTION) > 0)
            {

                //hold the friction for quicker access
                accel = data.GetFriction();
                BEPUutilities.Vector3 refCalVel = new BEPUutilities.Vector3(calcVelocity.X, 0, calcVelocity.Z);
                BEPUutilities.Vector3 accelVec = refCalVel.Normalized() * accel;
                //Debug.Log(calcVelocity.X);

                if ((refCalVel.Length() - accelVec.Length()) <= Fix64.Zero)
                {
                    calcVelocity = new BEPUutilities.Vector3(0, calcVelocity.Y, 0);
                }
                else
                {
                    //multiply by normalized to counter the current velocity
                    calcVelocity -= accelVec;
                    //Debug.Log("slow");
                }


            }
            //if we do want to set velocity, overriding velocity so far
            else if (storedVelocity.W > 0)
            {
                //Debug.Log("ADsfadf");
                calcVelocity = new BEPUutilities.Vector3(storedVelocity.X, storedVelocity.Y, storedVelocity.Z);

            }
            //assign the new calculated velocity to the rigidbody
            //rotateRigidBodyAroundPointBy(refvec, BEPUutilities.Vector3.Down, calcVelocity.X);
            //rb.velocity = (calcVelocity.Z * rb.position) + new BEPUutilities.Vector3(0, calcVelocity.Y, 0);
            //rotateRigidBodyAroundPointBy(enemyTarget.pos, BEPUutilities.Vector3.Down, sideVelocity);


            //calcVelocity.X = calcVelocity.X;
            BEPUutilities.Vector3 hold = new BEPUutilities.Vector3(calcVelocity.X, calcVelocity.Y, calcVelocity.Z);
            BEPUutilities.Vector3 what = BEPUutilities.Vector3.Zero;
            BEPUutilities.Quaternion.Transform(ref hold, ref rb.GetEntity().orientation, out what);
            rb.velocity = what;


            //rb.GetEntity().ApplyImpulse(rb.position, ((calcVelocity.Z * rb.position) + new BEPUutilities.Vector3(0, calcVelocity.Y, 0)) - rb.velocity);

        }
    }

    protected override void PostUpdate()
    {
        base.PostUpdate();

    }

    protected override void RenderUpdate()
    {
        //Debug.Log("REACHING--" + opposingData.priority + " -- " + (!stopTimer.IsTicking()));


        //pnly way for this to be true if we were hit this frame
        if (opposingData.priority > -1)
        {
            //Debug.Log("reached");
            this.Animator.SetInteger("Hittype", (int)opposingData.renderType);
        }

        //this.Animator.SetInteger
        base.RenderUpdate();

    }

    void OnEnable()
    {
        //I CAN APPARENTLY JUST ENABLE THE WHOLE THING? WHAT!?
        //controls.Enable();
    }
    void OnDisable()
    {
        //doing this just for safety
        //controls.Disable();
    }

    protected override void ApplyStateFrame(CharacterFrame currentFrame)
    {
        base.ApplyStateFrame(currentFrame);

        //if the state applies its own custom velocity
        if ((currentFrame.flags & FrameFlags.APPLY_VEL) > 0)
        {
            //makes it so that we no longer apply the stored velocity
            storedVelocity.W = -1;

            if ((currentFrame.flags & FrameFlags.SET_VEL) == FrameFlags.SET_VEL)
            {
                calcVelocity = new BEPUutilities.Vector3(currentFrame.velocity.X, currentFrame.velocity.Y, currentFrame.velocity.Z);
                storedVelocity = new BEPUutilities.Vector4(currentFrame.velocity.X, currentFrame.velocity.Y, currentFrame.velocity.Z, Fix64.One);
                //calcVelocity = new BEPUutilities.Vector3(currentFrame.velocity.x, 0, currentFrame.velocity.z);
                //calcVelocity=new BEPUutilities.Vector3(towardsTarget.x, 0, towardsTarget.z).normalized*calcVelocity.Length();
                //Debug.Log("applying vel");
                //calcVelocity = Fix64Quaternion.LookRotation((enemyTarget.position - rb.position).normalized) * calcVelocity;
                //calcVelocity.Y = 0;


            }
            else
            {

                calcVelocity += new BEPUutilities.Vector3(currentFrame.velocity.X, currentFrame.velocity.Y, currentFrame.velocity.Z);
                //calcVelocity += new BEPUutilities.Vector3(currentFrame.velocity.x, 0, currentFrame.velocity.z);

                //calcVelocity.Y = 0;
            }

        }

        if ((currentFrame.flags & FrameFlags.AUTO_JUMP) > 0)
        {
            //we want to jump, get the jump type
            int jumpId = data.CanJump();

            //may swap the next series of if statements to switch
            //check if we can jump (0 means we can't)
            if (jumpId > 0)
            {
                //player wants to air jump
                if (jumpId == 2)
                {
                    //increment the number of current air jumps
                    data.moveCondition.curAirJumps += 1;
                    //Debug.Log("jumping");
                }

                //gets the jump velocity and sets it to the y value.
                calcVelocity.Y = data.moveStats.jumpForce;
            }
        }


    }

    //call to set a new state
    protected override void ApplyNewState(StateFrameData newState)
    {
        base.ApplyNewState(newState);

        //no longer apply stored velocity, just in case, we have one here
        storedVelocity.W = -1;

        setNewState = true;

        EnterStateConditions enterCond = data.GetEnterConditions();

        //only check for condiditions if we have any
        if (enterCond > 0)
        {
            //for zero-ing any momentum in the relative direction
            //up
            if ((enterCond & EnterStateConditions.KILL_Y_MOMENTUM) > 0)
            {
                //Debug.Log("kill y vel");
                calcVelocity.Y = 0;
            }
            //relative left,right
            if ((enterCond & EnterStateConditions.KILL_X_MOMENTUM) > 0)
            {
                calcVelocity.X = 0;
            }
            //relative forward, back
            if ((enterCond & EnterStateConditions.KILL_Z_MOMENTUM) > 0)
            {
                calcVelocity.Z = 0;
            }



        }

    }

    //applies any exit conditions the state migh have
    protected override void ApplyExitCond(int cond)
    {
        base.ApplyExitCond(cond);
        if (cond == 0)
        {
            return;
        }

        //exit block state
        if ((cond & (int)ExitStateConditions.CAUSE_FOR_BLOCK) > 0)
        {
            data.RemoveCancelCondition(CancelCondition.CAUSE_FOR_BLOCK);
        }

        //exit stun state
        if ((cond & (int)ExitStateConditions.EXIT_STUN) > 0)
        {
            //Debug.Log("exit stun");
            data.RemoveTransitionCondition(TransitionCondition.GET_HIT);
            if ((data.stateCondition & StateConditions.STUN_STATE) == 0)
            {
            }
        }


        data.RemoveTransitionCondition(TransitionCondition.HIT_CONFIRM);

    }

    //call when player becomes grounded
    public void OnGrounded()
    {
        data.moveCondition.curAirJumps = 0;
        data.moveCondition.isGrounded = true;
    }

    //call when player stops being grounded
    public void OnNonGrounded()
    {
        data.moveCondition.isGrounded = false;
    }


    private void OnHitstopEnd(object sender)
    {
        //Debug.Log("hitstop over");
        rb.velocity = calcVelocity;
        //animator.SetBool("TransitionState", true);

        //Debug.Log(rb.velocity + " | " + calcVelocity);

    }

    public int GetTimeLeftInState()
    {
        return timer.TimeLeft();
    }


    //for if the character is hit, we return a value to the other player if hit
    public int GetHit(HitBoxData hitBoxData)
    {
        int ret = 0;
        if (hitBoxData.priority > opposingData.priority)
        {

            //NOTE: A PLAYER BEING GRABBED MUST BE HIT IN ORDER FOR STUN STATES TO RETURN TO NORMAL FUNCTION
            if ((hitBoxData.type & HitboxType.STRIKE) > 0)
            {
                StateConditions conditions = data.GetStateConditions();
                // Debug.Log(conditions);

                //on block hit
                if ((conditions & StateConditions.GUARD_POINT) > 0)
                {
                    ret = 2;
                }
                else
                {

                    if ((conditions & StateConditions.STUN_STATE) > 0)
                    {
                    }
                    else
                    {
                    }
                    //Debug.Log("called GetsHit " + gameObject.name + " priority " + opposingData.priority);

                    ret = 1;
                }

            }
            else if (((hitBoxData.type & HitboxType.GRAB) > 0) && data.moveCondition.isGrounded)
            {
                StateConditions conditions = data.GetStateConditions();

                //if not in hit or blockstun
                if ((conditions & StateConditions.STUN_STATE) == 0)
                {

                    ret = 1;
                }

            }

            //Debug.Log("state timer :: "+timer.isDone);

            //would return a successful hit
            if (ret > 0)
            {
                //record hitbox for future query
                opposingData = hitBoxData;
            }
        }
        return ret;

    }
    //called during the hurtbox query, apply the hitbox's stats likehit type and the such
    protected override void ProcessHit(HitBoxData boxData)
    {
        base.ProcessHit(boxData);
        rb.velocity = BEPUutilities.Vector3.Zero;
        //rb.LookAt(new BEPUutilities.Vector3(enemyTarget.pos.x, ShapeBase.position.y, enemyTarget.pos.z));
        data.AddTransitionCondition(TransitionCondition.GET_HIT);

        //check grab first so hitgrabs are possible
        if (((boxData.type & HitboxType.GRAB) > 0) && data.moveCondition.isGrounded)
        {
            stopTimer.StartTimer(boxData.hitstop);
            //grab hitstun should be indefinite, so it's -1, it tells the state to go on forever
            timer.StartTimer(boxData.hitstun);

            this.rb.parent = enemyTarget.grabAnchor;
            this.rb.position = enemyTarget.grabAnchor.position;

            //Debug.Log("grabbed");
        }
        //NOTE: A PLAYER BEING GRABBED MUST BE HIT IN ORDER FOR STUN STATES TO RETURN TO NORMAL FUNCTION
        else if ((boxData.type & HitboxType.STRIKE) > 0)
        {
            //part of removing the player if grabbed
            if (rb.parent != null)
            {
                this.rb.parent = null;
            }
            //tells whether or not to apply blocktun/stop
            //stun states have a -1 for duration so that the timer can be set to whatever we want
            //blocked
            if ((data.GetStateConditions() & StateConditions.GUARD_POINT) > 0)
            {
                stopTimer.StartTimer(boxData.hitstop);
                timer.StartTimer(boxData.blockstun);

            }
            //hit
            else
            {
                //grounded, apply regular hitstun
                if (data.moveCondition.isGrounded)
                {
                    timer.StartTimer(boxData.hitstun);

                }
                //airborne, apply untech time
                else
                {
                    timer.StartTimer(boxData.untechTime);

                }
                stopTimer.StartTimer(boxData.hitstop);
            }
            Debug.Log(gameObject.name + " GotHit!");


            this.calcVelocity = new BEPUutilities.Vector3(0, Fix64.Sin(boxData.launchAngle), Fix64.Cos(boxData.launchAngle) * -1).Normalized() * boxData.launchForce;
            this.rb.velocity = (calcVelocity.Z * rb.GetEntity().Forward) + new BEPUutilities.Vector3(0, calcVelocity.Y, 0);


        }

    }


    public void TriggerBlock()
    {
        data.AddTransitionCondition(TransitionCondition.CAUSE_FOR_BLOCK);
        data.AddCancelCondition(CancelCondition.CAUSE_FOR_BLOCK);
    }

    public void OnHitConnect(HitBoxData hitBox, int hitType)
    {
        base.OnHit(hitBox, hitType);
    }

}
