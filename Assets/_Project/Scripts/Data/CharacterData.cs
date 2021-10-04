using UnityEngine;
using Spax.StateMachine;
using Spax.Input;
using FixMath.NET;

namespace Spax
{
    [System.Serializable]
    //will contatin various things about the player, namely nonphysics conditions and stats
    public class CharacterData
    {
        //static stats of the character pertaining to movement
        public MoveStats moveStats;

        //dynamic condition of the character, based on movement
        public MoveCondition moveCondition;

        public int MaxHealth;
        public int CurHealth;


        [SerializeField] private StateFrameData currentState;
        public StateFrameData defaultState;

        public StateFrameData[] allStates;

        public InputRecorder inputRecorder;
        public MoveList commandList;

        //what we can cancel into
        [SerializeField] private CancelCondition cancelCondition;
        public StateConditions stateCondition;

        public TransitionCondition xtraCondition;

        public void Initialize()
        {
            inputRecorder = new InputRecorder();
            inputRecorder.Initialize();
            int hold = 0;
            //assigning default state
            this.AssignNewCurState(0, out hold);

            int len = allStates.Length;
            for (int i = 0; i < len; i++)
            {
                allStates[i].stateID = (uint)i;
                //Debug.Log("state -- " + allStates[i].stateID);
            }

            //get the default state, first state
            defaultState = allStates[0];

            //just to be safe
            xtraCondition = 0;


            //commandList.Prepare();

            //Debug.Log(commandList.CheckTest(testInputs));


        }

        private bool inputChanged = false;

        //call to buffer an input for later
        public bool BufferPrev(SpaxInput input, bool inStop = false)
        {
            inputChanged = inputRecorder.RecordInput(input, inStop);
            return inputChanged;

        }

        public Fix64 GetGravForce()
        {
            return moveStats.mass;
        }



        //returns nonzero if player can jump
        //0: player cannot jump
        //1: player is grounded, can jump
        //2: player has not used maximum number of air jumps
        public int CanJump()
        {
            if (moveCondition.isGrounded)
            {
                return 1;
            }
            else if (moveCondition.curAirJumps < moveStats.maxAirJumps)
            {
                return 2;
            }

            return 0;
        }

        public Fix64 GetAcceleration(bool walking = false)
        {
            if (moveCondition.isGrounded)
            {
                if (walking)
                {
                    return moveStats.walkAcceleration;
                }

                return moveStats.groundAcceleration;
            }


            return moveStats.airAcceleration;
        }

        public Fix64 GetMaxSpeed(bool walking = false)
        {
            if (moveCondition.isGrounded)
            {
                if (walking)
                {
                    //Debug.Log("wants to walk");
                    return moveStats.maxWalkSpeed;
                }

                return moveStats.maxGroundSpeed;
            }



            return moveStats.maxAirSpeed;
        }

        public Fix64 GetFriction()
        {
            if (moveCondition.isGrounded)
            {
                return moveStats.groundFriction;
            }

            return moveStats.airFriction;
        }

        public StateConditions GetStateConditions()
        {
            //return this.GetConditionsFromState(currentState);
            return this.stateCondition;
        }

        public CancelCondition GetCancelCondition()
        {
            return GetCancelConditionFromState(this.currentState);
        }

        private CancelCondition GetCancelConditionFromState(StateFrameData stt)
        {
            if (stt.stateID != stt.parentID)
            {
                return stt.cancelCondition | GetCancelConditionFromState(allStates[stt.parentID]);
            }
            return stt.cancelCondition;
        }

        private StateConditions GetConditionsFromState(StateFrameData stt)
        {
            //if the we don't want parent conditions, parent is out of bounds, or the parent is the current state
            if ((stt.stateConditions & StateConditions.NO_PARENT_COND) > 0 || stt.parentID > allStates.Length || (stt.parentID == stt.stateID))
            {

                return stt.stateConditions;
            }
            return stt.stateConditions | GetConditionsFromState(allStates[stt.parentID]);
        }

        public EnterStateConditions GetEnterConditions()
        {
            //return this.GetConditionsFromState(currentState);
            return GetEnterConditions(currentState);
        }

        private EnterStateConditions GetEnterConditions(StateFrameData stt)
        {
            if (stt.parentID > allStates.Length || (stt.parentID == stt.stateID))
            {
                return stt.enterStateConditions;
            }

            return stt.enterStateConditions | GetEnterConditions(allStates[stt.parentID]);
        }

        //gets string lisitng of previous inputs, for debugging purposes
        public ulong[] GetStringPrevInputs()
        {
            return inputRecorder.GetInputCodes();
            //return null;
        }

        //public method to interface from
        public StateFrameData GetCommand(out int exitCond)
        {

            return FindCommandFromState(out exitCond);
        }

        //finds the command using previous inputs
        public StateFrameData FindCommandFromState(out int exitCond)
        {

            int ret = -1;
            //Debug.Log(GetStringPrevInputs());

            CancelCondition groundedCheck = (this.moveCondition.isGrounded) ? CancelCondition.GROUNDED : CancelCondition.AIRBORNE;
            CancelCondition condition = this.GetCancelCondition() | groundedCheck;

            ret = commandList.FindCommand(this.GetStringPrevInputs(), moveCondition.facingRight,
                condition | (CancelCondition)((this.CanJump() / 2) << 5) | groundedCheck);

            //Debug.Log(ret);

            if (ret > -1)
            {
                AssignNewCurState(ret, out exitCond);
                return allStates[ret];
            }

            exitCond = 0;
            return null;
        }

        public StateFrameData TransitionState(bool timerIsDone, SpaxInput input, out int exitCond)
        {
            return CheckTransitionState(timerIsDone, input, currentState, out exitCond);
        }

        //this transition algorithm feels very bad, rewrite and organize
        private StateFrameData CheckTransitionState(bool timerIsDone, SpaxInput input, StateFrameData srcState, out int exitCond)
        {
            //gets current transition conditions
            TransitionCondition curConditions = GetTransitionCondition() | this.xtraCondition;


            StateFrameData ret = CheckStateTransition(timerIsDone, input, currentState.stateID, curConditions, out exitCond);


            //for stun states, specifically if there isn't another transition triggered
            if (((this.GetStateConditions() & StateConditions.STUN_STATE) == 0) && ((curConditions & (TransitionCondition.GET_HIT)) > 0) && (ret == null))
            {
                //Debug.Log("got hit");
                /*int len = defaultState._transitions.Length;
                //searching through the main stun state through the default state
                for (int i = 0; i < len; i++)
                {
                    Transition_PH potenTrans = defaultState._transitions[i];

                    if ((potenTrans.Condition & TransitionCondition.GET_HIT) > 0)
                    {
                        return AssignNewCurState((int)potenTrans.Target, out exitCond);
                    }
                }*/
                ret = CheckStateTransition(timerIsDone, input, 0, curConditions, out exitCond);


            }



            return ret;
        }

        private StateFrameData CheckStateTransition(bool timerIsDone, SpaxInput input, uint origin, TransitionCondition curConditions, out int exitCond)
        {
            StateFrameData srcState = allStates[origin];

            int len = srcState._transitions.Length;


            //if the timer is done
            if (timerIsDone)
            {
                curConditions |= TransitionCondition.ON_END;

            }

            for (int i = 0; i < len; i++)
            {
                //the current transition we're looking at
                Transition curTrans = srcState._transitions[i];

                //target state for the transition
                int potenState = curTrans.Target;
                //the required transition condition
                TransitionCondition compare = curTrans.Condition;
                //input requirements for the transition
                InputCodeFlags inputCond = curTrans.inputConditions;
                //cancel conditions for the transition
                CancelCondition cancelCond = curTrans.cancelCondition;

                if (((compare & (TransitionCondition.FULL_METER | TransitionCondition.HALF_METER |
                                TransitionCondition.QUART_METER)) == 0) &&
                                (((curConditions ^ compare) & TransitionCondition.GET_HIT) == 0))
                {


                    //if all the conditions are met
                    if ((compare & curConditions) == compare && (cancelCond & this.cancelCondition) == cancelCond)
                    {
                        //get the last input change
                        int fromInput = (int)inputRecorder.GetLatestCode();

                        bool initialCheck = ((inputCond & (InputCodeFlags)fromInput) == inputCond) || InputCode.WorksAs4Way(fromInput, (int)inputCond);
                        bool freePass = false;

                        if (!initialCheck && (inputCond & InputCodeFlags.CURRENTLY_HELD) > 0)
                        {
                            inputCond ^= InputCodeFlags.CURRENTLY_HELD;
                            int codeFromInput = (((int)input.direction & 510) << 2) | ((int)input.buttons << 11);
                            int inputCondSimple = ((int)inputCond >> 3) << 3;

                            //Debug.Log((InputCodeFlags)codeFromInput);

                            freePass = ((codeFromInput & inputCondSimple) == inputCondSimple) || InputCode.WorksAs4Way(codeFromInput, inputCondSimple);
                        }

                        //check it
                        //if the state is not the same state, then pass
                        //if the state is the same state, then check if it can transition to itself
                        //if the state can transition to self, then pass
                        if ((potenState != currentState.stateID) ||
                            (stateCondition & StateConditions.TRANSITION_TO_SELF) > 0)
                        {


                            if (inputCond == 0 || initialCheck || freePass)
                            {
                                /*if (origin == 49)
                                {
                                    Debug.Log("TRANSITIONING --> " + potenState);
                                }
                                /*if ((inputCond & (InputCodeFlags.B | InputCodeFlags.RELEASED)) == (InputCodeFlags.B | InputCodeFlags.RELEASED))
                                {
                                    Debug.Log("released B released");
                                }*/
                                //Debug.Log(currentState.stateID + "==>" + potenState.stateID);
                                //if (currentState.stateID == 49)
                                //{
                                //    Debug.Log("leaving stun state -- timer is done :: "+timerIsDone);
                                //}

                                StateFrameData ret = AssignNewCurState(potenState, out exitCond);

                                return ret;
                            }
                        }
                    }
                }
            }

            //only really reached if state to transition to isn't found
            if (((srcState.stateConditions & StateConditions.NO_PARENT_TRANS) == 0) && (srcState.parentID < allStates.Length) && (srcState.parentID != srcState.stateID))
            {
                return CheckStateTransition(timerIsDone, input, (uint)srcState.parentID, curConditions, out exitCond);
            }

            exitCond = 0;
            return null;
        }

        //sets and returns the new state to current state
        private StateFrameData AssignNewCurState(int newStateID, out int exitCond)
        {
            if (currentState != null)
            {
                //Debug.Log("current state :: " + currentState.stateID + " || new state :: " + newStateID);

                exitCond = (int)GetExitCondition((int)currentState.stateID);
            }
            else
            {
                exitCond = 0;
            }
            currentState = allStates[newStateID];
            //Debug.Log(currentState.stateID);
            this.cancelCondition = currentState.cancelCondition;
            this.stateCondition = GetConditionsFromState(currentState);


            return currentState;
        }

        public StateFrameData GetState()
        {
            return currentState;
        }

        public ExitStateConditions GetExitCondition(int state)
        {
            ExitStateConditions ret = allStates[state].exitStateConditions;

            if (allStates[state].stateID != allStates[state].parentID)
            {

                ret |= GetExitCondition(allStates[state].parentID);
            }

            return ret;
        }

        private TransitionCondition GetTransitionCondition()
        {
            //Debug.Log("getting transition conditions :: ");
            TransitionCondition ret = 0;

            if (moveCondition.isGrounded)
            {
                ret |= TransitionCondition.GROUNDED;
                //Debug.Log("getting transition conditions :: grounded");

            }
            else
            {
                ret |= TransitionCondition.AERIAL;
            }

            //ret |= (TransitionCondition)input.buttons;

            if (this.CanJump() > 0)
            {
                ret |= TransitionCondition.CANJUMP;
            }

            //Debug.Log(ret);

            return ret;
        }

        //edits the cancel condition on hit
        public void AddCancelCondition(CancelCondition cond)
        {
            this.cancelCondition |= cond;
        }
        //edits the cancel condition on hit
        public void RemoveCancelCondition(CancelCondition cond)
        {
            this.cancelCondition &= (~cond);
        }
        //edits the transition condition on hit
        public void AddTransitionCondition(TransitionCondition cond)
        {
            this.xtraCondition |= cond;
        }
        //edits the transition condition on to remove
        public void RemoveTransitionCondition(TransitionCondition cond)
        {
            this.xtraCondition &= (~cond);
        }


    }


}




