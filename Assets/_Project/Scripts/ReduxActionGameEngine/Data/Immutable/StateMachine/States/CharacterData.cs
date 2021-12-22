using ActionGameEngine.Enum;
using ActionGameEngine.Input;
using ActionGameEngine.Data.Helpers.Wrappers;
using FixMath.NET;

namespace ActionGameEngine.Data
{
    //static struct to keep all of our character's stats, move list, and other immutable data
    [System.Serializable]
    public struct CharacterData
    {
        public int maxHp;

        public Fix64 mass;
        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        public Fix64 friction;
        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        public FVector2 maxVelocity;
        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        public FVector2 acceleration;

        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        public StateListWrapper stateList;
        public CommandList moveList;

        public CharacterData(int mhp, Fix64 m, Fix64 f, FVector2 mv, FVector2 a, StateListWrapper sl, CommandList cl)
        {
            maxHp = mhp;
            mass = m;
            friction = f;
            maxVelocity = mv;
            acceleration = a;
            stateList = sl;
            moveList = cl;
        }
#if UNITY_EDITOR

        //call to make the states in the state list have their ID's match their index in the list
        //helps prevent states with the same ID
        public void CorrectStateID()
        {
            int len = stateList.Length;
            //while loop instead of for no particular reason
            int i = 0;
            while (i < len)
            {
                //structs automatically make deep copies, reassign after manipulation
                StateData hold = stateList[i];
                hold.stateID = i;
                stateList[i] = hold;
                i++;
            }
        }
#endif

        public StateData GetStateFromID(int stateID)
        {
            StateData ret = stateList[stateID];
            return ret;
        }

        public StateCondition GetConditionsFromState(int stateID)
        {
            StateData state = stateList[stateID];
            StateCondition ret = state.stateConditions;

            //recur to keep adding the parent's conditions if there is a parent
            if (state.HasParent() && (stateID > -1) && (!EnumHelper.HasEnum((uint)ret, (uint)StateCondition.NO_PARENT_COND)))
            {
                //UnityEngine.Debug.Log(stateID + " " + (int)(ret & StateCondition.NO_PARENT_COND) + " " + ret);
                ret |= this.GetConditionsFromState(state.parentID);
            }

            return ret;
        }

        //returns index of transition if true
        //version without input for non-controllable object
        public TransitionData TryTransitionState(int fromState, CancelConditions playerCond, TransitionFlag playerFlags, int facing)
        {
            RecorderElement[] playerInputs = new RecorderElement[1];
            return TryTransitionState(fromState, playerInputs, playerCond, playerFlags, facing);
        }

        //returns index of transition if true
        //state's transitions are checked before movelist's
        public TransitionData TryTransitionState(int fromState, RecorderElement[] playerInputs, CancelConditions playerCond, TransitionFlag playerFlags, int facing)
        {
            StateData state = stateList[fromState];
            int check = state.CheckTransitions(playerInputs, playerFlags, playerCond, facing);

            //make invalid transition by setting the target state to -1
            TransitionData ret = new TransitionData();
            ret.targetState = -1;

            //we found a valid transition, make final checks and prep data for player script to process
            //check is the index of the transition if found            
            if (check > -1)
            {
                TransitionData potenTransition = state.GetTransitionFromIndex(check);
                int potenStateID = potenTransition.targetState;
                StateData potenState = stateList[potenStateID];

                //check if target state is a node state, if so, then check transitions in that state
                //we should NEVER return a transition to a node state
                if (potenState.IsNodeState())
                {
                    ret = TryTransitionState(potenStateID, playerInputs, playerCond, playerFlags, facing);
                }
                //we only transition if we go to another state, ID mismatch *OR* it's okay to transition to self
                //if ID mimatch failed, then we know the ID are the same
                else if ((fromState != potenStateID) || EnumHelper.HasEnum((uint)state.stateConditions, (uint)StateCondition.CAN_TRANSITION_TO_SELF))
                {
                    ret = potenTransition;
                }
            }
            //we didn't find a valid transition from the state's transitions

            //check if state has parent (parent id != state's id)
            bool hasParent = state.stateID != state.parentID;

            //should we check the parent's transitions
            //but ONLY IF we haven't found a valid transition yet
            if (!ret.IsValid() && hasParent && !EnumHelper.HasEnum((uint)state.stateConditions, (uint)StateCondition.NO_PARENT_COND))
            {
                //check the parent's transitions
                //recur with passing the parent's ID
                ret = TryTransitionState(state.parentID, playerInputs, playerCond, playerFlags, facing);
            }

            //last check before we return

            //did we get hit?
            bool gotHit = EnumHelper.HasEnum((uint)playerFlags, (uint)TransitionFlag.GOT_HIT);
            //does the transition we get care about if we got hit or not?
            bool caresAbtHit = EnumHelper.HasEnum((uint)ret.transitionFlag, (uint)TransitionFlag.GOT_HIT);

            //only override to go into stun state if we got hit and the transition doesn't care about that
            //also, we found a transition we found is valid
            if (gotHit && ret.IsValid() && !caresAbtHit)
            {
                //all stun states should be attached to the default state
                ret = TryTransitionState(0, playerInputs, playerCond, playerFlags, facing);
            }


            return ret;
        }

        public int TryMoveList(RecorderElement[] playerInputs, CancelConditions playerCond, int facing)
        {
            //search the movelist for a valid transition
            //integer of the target state
            int ret = moveList.Check(playerInputs, playerCond, facing);

            return ret;

        }
    }
}