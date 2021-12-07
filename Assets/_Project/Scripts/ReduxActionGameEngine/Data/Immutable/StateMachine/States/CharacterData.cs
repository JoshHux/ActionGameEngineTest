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
        public TransitionData TryTransitionState(int fromState, RecorderElement[] playerInputs, CancelConditions playerCond, TransitionFlag playerFlags, int facing)
        {
            StateData state = stateList[fromState];
            int check = state.CheckTransitions(playerInputs, playerFlags, playerCond, facing);

            //we found a valid transition, make final checks and prep data for player script to process
            if (check > -1)
            {
                TransitionData potenTransition = state.GetTransitionFromIndex(check);
                int potenStateID = potenTransition.targetState;
                StateData potenState = stateList[potenStateID];

                //check if target state is a node state, if so, then check transitions in that state
                //we should NEVER return a transition to a node state
                if (potenState.IsNodeState())
                {
                    return TryTransitionState(potenStateID, playerInputs, playerCond, playerFlags, facing);
                }

                return potenTransition;
            }
            //we didn't find a valid transition from the state's transitions

            //make invalid transition by setting the target state to -1
            TransitionData ret = new TransitionData();
            ret.targetState = -1;
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