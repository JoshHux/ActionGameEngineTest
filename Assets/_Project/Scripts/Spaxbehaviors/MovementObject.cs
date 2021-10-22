using BEPUUnity;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Materials;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities;
using FixMath.NET;
using Spax;
using Spax.Input;
using Spax.StateMachine;
using Spax.Data;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementObject : SpaxBehavior
{
    //input that is parsed at the beginning on the frame
    public string FighterName = "Aganju";

    [SerializeField]
    protected SpaxInput Input;

    //real-time controller state
    [SerializeField]
    protected SpaxInput AsyncInput;

    [SerializeField]
    protected BepuVector2 StickInput;

    [SerializeField]
    protected Animator Animator;

    protected Hitbox[] HitboxObj;

    protected Hurtbox[] HurtboxObj;

    //public InputActionAsset actions;
    [SerializeField]
    protected FrameTimer timer;

    //controls hit and block-stop
    [SerializeField]
    protected FrameTimer stopTimer;

    [SerializeField]
    protected BepuVector3 calcVelocity;

    [SerializeField]
    public FighterController enemyTarget;

    public CharacterData data;

    private Fix64 StickAngle;

    //if the script should read player inputs or not, for debug purposes
    private bool acceptInputs = true;

    protected PlayerInput PlayerInput;

    protected ShapeBase rb;

    protected AudioSource AudioSource;

    public BepuVector3 pos;

    //protected ShapeBase ShapeBase;
    protected bool setNewState = false;

    protected override void OnAwake()
    {
        AudioSource = GetComponent<AudioSource>();
        PlayerInput = GetComponent<PlayerInput>();
        rb = GetComponent<ShapeBase>();

        timer = new FrameTimer();
        opposingData = new HitBoxData();
        opposingData.priority = -1;

        //ENABLE ALL ACTIONS BEFORE USING CALLBACKS, I DON'T KNOW WHY I HAVE TO DO IT, I JUST DO
        InputAction action = PlayerInput.actions["Move"];
        action.performed += ctx =>
            CallbackDirectionInput(ctx.ReadValue<Vector2>());
        action.canceled += ctx =>
            CallbackDirectionInput(ctx.ReadValue<Vector2>());
        action.Enable();

        action = PlayerInput.actions["Jump"];
        action.performed += ctx => CallbackButtonInput(Button.W);
        action.canceled += ctx => CallbackButtonInput(Button.W);
        action.Enable();
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        //animator = GetComponent<Animator>();
        this.ApplyNewState(data.GetState());

        //animator.SetBool("TransitionState", true);
        //Debug.Log(data.name);
        data = new CharacterData();
        rb.GetEntity().CollisionInformation.Events.DetectingInitialCollision +=
            RemoveFriction;

        Debug.Log("is data null? :: " + (data == null));
        data.Initialize();

        //find the gameobject that holds all hitboxes
        foreach (Transform child in this.transform)
        {
            if (child.tag == "Hitboxes")
            {
                HitboxObj = child.GetComponentsInChildren<Hitbox>();
            }
        }

        //find the gameobject that holds all hurtboxes
        foreach (Transform child in this.transform)
        {
            if (child.tag == "Hurtboxes")
            {
                HurtboxObj = child.GetComponentsInChildren<Hurtbox>();
            }
        }
    }

    protected override void InputUpdate()
    {
        //put the synchronous input to the input we read for the tick if we accept player inputs
        if (acceptInputs)
        {
            Input = AsyncInput;
        }
        else
        //
        {
            StickInput = BepuVector2.Zero;
        }

        //buffer the input
        //if the stop timer is running
        if (stopTimer.IsPaused())
        {
            stopTimer.PlayTimer();
        }

        if (data == null) return;

        //buffer the input
        bool newInput = data.BufferPrev(Input);

        StateFrameData newState = null;
        int exitCond = 0;
        if (newInput)
        {
            newState = data.GetCommand(out exitCond);
        }

        //finding a command
        if (newState == null)
        {
            newState =
                data.TransitionState(!timer.IsTicking(), Input, out exitCond);
            /* if (newState != null && newState.stateID == 49)
             {
                 Debug.Log("hitstun state entered");
             }*/
        }

        //true is not is hitstop
        if (!stopTimer.TickTimer())
        {
            //Debug.Log("tick timer");
            timer.TickTimer();

            calcVelocity =
                BepuQuaternion
                    .Transform(rb.velocity,
                    BepuQuaternion.Inverse(rb.rotation));
            //calcVelocity = BEPUutilities.Quaternion.Transform(rb.velocity, (rb.rotation));
            //calcVelocity.X = sideVelocity;

            //Debug.Log(calcVelocity);
        }
        else
        {
            rb.velocity = (BepuVector3.Zero);
        }

        //can't be else statement, checks null before apply possible new state
        //is true if new state is found
        if (newState != null)
        {
            TransitionNewState(exitCond, newState);
        }

        //moved resetting this data to here so its data can be used elsewhere
        //can't be in either query updates or trades will not work
        opposingData = new HitBoxData();
        opposingData.priority = -1;
    }

    //we don't query hit and hurt in the same method because it can cause a scenario where theyy fire at the same time
    //which can cause some issues depending on how simultaneous events are handled
    //so I decided to play it safe and query hits before hurts
    protected override void HitboxQueryUpdate()
    {
        //get the length of for the for loop
        int len = this.HitboxObj.Length;
        int hitType = 0;

        //local object to hold for efficiency
        HitBoxData boxData = new HitBoxData();
        boxData.priority = -1;

        //find a hitbox that connected with highest priority
        for (int i = 0; i < len; i++)
        {
            Hitbox hitbox = this.HitboxObj[i];
            HitBoxData compare = hitbox.GetBoxData();
            if (hitbox.IsActiveBox() && (compare.priority > boxData.priority))
            {
                hitType = hitbox.QueryCollisions();

                //Debug.Log(hitType);
                boxData = compare;
            }
        }

        //if nonzero, not a whiff
        if (hitType > 0)
        {
            this.OnHit(boxData, hitType);

            //Debug.Log("hit from " + gameObject.name);
        }

        //Debug.Log("======= " + gameObject.name + " priority " + opposingData.priority);
    }

    //for remembering the hitbox you were hit for processing hurtbox
    [SerializeField]
    protected HitBoxData opposingData;

    protected override void HurtboxQueryUpdate()
    {
        if (opposingData.priority > -1)
        {
            //Debug.Log("getting hit " + gameObject.name + " priority " + opposingData.priority);
            ProcessHit(opposingData);

            //resets the priority so the data doesn't stick around
            //commented out so it RenderUpdate can use the opposingData for different hit animations
            //opposingData = new HitBoxData();
            //opposingData.priority = -1;
        }
    }

    protected override void RenderUpdate()
    {
        if (!stopTimer.IsTicking())
        {
            //sets new animation state is a new state has been set
            //won't transition if the name of the state is NewState
            //useful for when we want the state to update without the animation (like with untech)
            if (setNewState && (data.GetState().animName != "NewState"))
            {
                ApplyNewAnimationState(data.GetState().animName, Fix64.Zero);
            }

            //updates the animator componenet
            Animator.speed = 1.0f;
            Animator.Update(Time.fixedDeltaTime);
            Animator.speed = 0f;
        }
    }

    protected void TransitionNewState(int exitCond, StateFrameData newState)
    {
        //the previous state's exit conditions are applied
        ApplyExitCond(exitCond);

        //the new state is assigned along with any enter conditions it may have
        ApplyNewState(newState);

        //just force the transition to hitstun if hit
        if ((data.xtraCondition & TransitionCondition.GET_HIT) > 0)
        {
            ApplyNewAnimationState(data.GetState().animName, Fix64.Zero);
        }
    }

    protected virtual void ApplyNewState(StateFrameData newState)
    {
        //Debug.Log(newState.stateName);
        //animator.SetTrigger("StateChanged");
        //currentState = newState;
        //if the state's duration is non-negative, then we set it's duration
        //else, we don't do anything, we set the variable state duration manually in ProcessHit
        if (newState.duration >= 0)
        {
            //-1 is there to have the elapsed time equal the index
            timer.StartTimer(newState.duration);
            //Debug.Log(newState.Frames.Length);
        }

        //else
        //{
        //    Debug.Log("variable duration");
        //}
        setNewState = true;
    }

    protected virtual void ApplyExitCond(int cond)
    {
        if ((cond & (int)ExitStateConditions.CLEAN_HITBOXES) > 0)
        {
            int len = HitboxObj.Length;
            for (int i = 0; i < len; i++)
            {
                Hitbox box = HitboxObj[i];
                if (box.IsActiveBox())
                {
                    box.DeactivateBox();
                }
            }
        }
        //data.RemoveTransitionCondition(TransitionCondition.HIT_CONFIRM);
    }

    protected void ApplyNewAnimationState(string stateName, Fix64 startTime)
    {
        Animator.PlayInFixedTime(stateName, 0, (float)startTime);

        //Debug.Log(data.GetState().stateName);
        setNewState = false;
    }

    protected virtual void ApplyStateFrame(CharacterFrame currentFrame)
    {
        //check if there are hitboxes
        if (currentFrame.HasHitboxes())
        {
            //Debug.Log("Hitboxes");
            //sets the data of the bitboxes
            //get the array for easier access
            HitBoxData[] hitBoxData = currentFrame.hitboxes;

            //get the length of for the for loop
            int len = hitBoxData.Length;

            for (int i = 0; i < len; i++)
            {
                Hitbox box = HitboxObj[i];
                HitBoxData data = hitBoxData[i];
                box.SetBoxData(data);
            }
        }

        //check if there are hurtboxes
        if (currentFrame.HasHurtboxes())
        {
            //sets the data of the hurtboxes
            //get the array for easier access
            HurtBoxData[] hurtBoxData = currentFrame.hurtboxes;

            //get the length of for the for loop
            int len = hurtBoxData.Length;

            for (int i = 0; i < len; i++)
            {
                Hurtbox box = HurtboxObj[i];
                HurtBoxData data = hurtBoxData[i];

                //Debug.Log(len);
                box.SetBoxData(data);
            }
        }
    }

    protected virtual void ProcessHit(HitBoxData boxData)
    {
    }

    //callback for reading and recording directional inputs
    public void CallbackDirectionInput(Vector2 ctx)
    {
        //_movementInput = context.ReadValue<Vector2>();
        //Debug.Log("called :: " + ctx);
        //ctx.x *= data.moveCondition.facing;
        int newDir = 1;
        StickInput = new BepuVector2((Fix64)ctx.x, (Fix64)ctx.y);
        StickAngle =
            (Fix64.Atan2(StickInput.X, StickInput.X)) *
            Fix64.PiInv *
            (Fix64)180f;

        if (ctx.y < 0)
        {
            newDir = newDir << 6;
        }
        else if (ctx.y > 0)
        {
            newDir = newDir << 3;
        }

        if (ctx.x < 0)
        {
            newDir = newDir << 2;
        }
        else if (ctx.x > 0)
        {
            newDir = newDir << 1;
        }

        AsyncInput.direction = (Direction)newDir;

        //theres the possibility that the a button is tapped before the controller state is registered in syncInput
        //this scenario will result in an eaten input
        //to mitigate this, we will always add the new input to the read sync input and then re-assign the current controller state
        //after the synchronized input is parsed
        Input.direction = (Direction)newDir;
    }

    public void CallbackButtonInput(Button button)
    {
        //Debug.Log("jump");
        //little delicate thatn I would like, but it works, and theoretically won't break, but still not as safe as I would like
        AsyncInput.buttons ^= button;

        //theres the possibility that the a button is tapped before the controller state is registered in syncInput
        //this scenario will result in an eaten input
        //to mitigate this, we will always add the new input to the read sync input and then re-assign the current controller state
        //after the synchronized input is parsed
        Input.buttons |= (button);
    }

    //when the character's attack hits
    public virtual void OnHit(HitBoxData hitBox, int hitType)
    {
        //Debug.Log("Hititintititititit");
        data.AddTransitionCondition(TransitionCondition.HIT_CONFIRM);

        data.AddCancelCondition(hitBox.onHitCancel);
        stopTimer.SetTimer(hitBox.hitstop);

        Debug.Log(gameObject.name + " hits with skill " + hitType);
        rb.velocity = BepuVector3.Zero;
        //animator.SetBool("TransitionState", false);
    }

    //general reference
    public bool IsInHitstop()
    {
        return stopTimer.IsTicking();
    }

    //this is needed to have proper control over character's velocity
    //DO NOT TOUCH
    void RemoveFriction(
        EntityCollidable sender,
        BroadPhaseEntry other,
        NarrowPhasePair pair
    )
    {
        var collidablePair = pair as CollidablePairHandler;
        if (collidablePair != null)
        {
            //The default values for InteractionProperties is all zeroes- zero friction, zero bounciness.
            //That's exactly how we want the character to behave when hitting objects.
            collidablePair
                .UpdateMaterialProperties(new InteractionProperties());
        }
    }
}
