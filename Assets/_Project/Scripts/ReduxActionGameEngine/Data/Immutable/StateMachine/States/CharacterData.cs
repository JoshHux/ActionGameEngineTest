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
            //make array to record the old state ID
            int[] oldID = new int[len];
            //while loop instead of for no particular reason
            int i = 0;
            while (i < len)
            {
                //structs automatically make deep copies, reassign after manipulation
                StateData hold = stateList[i];

                //record the old ID in Array
                oldID[i] = hold.stateID;

                //assign the new ID based on index
                hold.stateID = i;
                //reassign the state (deep copy)
                stateList[i] = hold;

                //increment the while loop
                i++;
            }

            //go through and correct the parent ID for the states
            i = 0;
            while (i < len)
            {
                //structs automatically make deep copies, reassign after manipulation
                StateData hold = stateList[i];

                //record the old ID for the parent
                int oldPID = hold.parentID;

                //get the new parent ID
                int newPID = FindNewID(oldPID, oldID);

                //assign the new parent ID based on what was returned
                hold.parentID = newPID;

                //get the array of transitions from the state for ease of access
                TransitionData[] transitions = hold.transitions;
                //length of transition array in state
                int transLen = transitions.Length;
                //go through and correct the parent ID for the states
                int j = 0;
                while (j < transLen)
                {
                    //structs automatically make deep copies, reassign after manipulation
                    TransitionData holdTrans = transitions[j];

                    //record the old ID for the target state
                    int oldTID = holdTrans.targetState;

                    //get the new target ID
                    int newTID = FindNewID(oldTID, oldID);

                    //assign the new parent ID based on what was returned
                    holdTrans.targetState = newTID;

                    //reassign the state (deep copy)
                    transitions[j] = holdTrans;

                    //increment the while loop
                    j++;
                }
                //reassign the transition list because deep copy
                hold.transitions = transitions;


                //reassign the state (deep copy)
                stateList[i] = hold;

                //increment the while loop
                i++;
            }

            //to make sure movelist also has reassigned state ID
            //store the movelist for ease of access
            MotionTransition[] cmdList = moveList.command;
            //length of command list
            len = cmdList.Length;
            //while loop instead of for no particular reason
            i = 0;
            while (i < len)
            {
                //structs automatically make deep copies, reassign after manipulation
                MotionTransition hold = cmdList[i];

                //record the old ID for the target state
                int oldTID = hold.targetState;

                //get the new target ID
                int newTID = this.FindNewID(oldTID, oldID);

                //assign the new parent ID based on what was returned
                hold.targetState = newTID;

                //reassign the state (deep copy)
                cmdList[i] = hold;

                //increment the while loop
                i++;
            }
            //reassign command list (deep copy)
            moveList.command = cmdList;
        }

        private int FindNewID(int oldID, int[] arrOldID)
        {
            int len = arrOldID.Length;
            //while loop instead of for no particular reason
            int i = 0;
            while (i < len)
            {
                //structs automatically make deep copies, reassign after manipulation
                int hold = arrOldID[i];

                //old ID was found at index i, return the new ID of that state
                if (oldID == hold) { return stateList[i].stateID; }

                //increment while loop
                i++;
            }

            //the new state was not found
            return -1;
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
        public TransitionData TryTransitionState(int fromState, CancelConditions playerCond, TransitionFlag playerFlags, int facing, ResourceData playerResources)
        {
            RecorderElement[] playerInputs = new RecorderElement[1];
            return TryTransitionState(fromState, playerInputs, playerCond, playerFlags, facing, playerResources);
        }

        //returns index of transition if true
        //state's transitions are checked before movelist's
        public TransitionData TryTransitionState(int fromState, RecorderElement[] playerInputs, CancelConditions playerCond, TransitionFlag playerFlags, int facing, ResourceData playerResources)
        {
            StateData state = stateList[fromState];
            int check = state.CheckTransitions(playerInputs, playerFlags, playerCond, facing, playerResources);

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
                    ret = TryTransitionState(potenStateID, playerInputs, playerCond, playerFlags, facing, playerResources);
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
            if (!ret.IsValid() && hasParent && !EnumHelper.HasEnum((uint)state.stateConditions, (uint)StateCondition.NO_PARENT_TRANS))
            {
                //check the parent's transitions
                //recur with passing the parent's ID
                TransitionData potenTransition = TryTransitionState(state.parentID, playerInputs, playerCond, playerFlags, facing, playerResources);
                int potenStateID = potenTransition.targetState;

                //we only transition if we go to another state, ID mismatch *OR* it's okay to transition to self
                //if ID mimatch failed, then we know the ID are the same
                if (potenTransition.IsValid() && ((fromState != potenStateID) || EnumHelper.HasEnum((uint)state.stateConditions, (uint)StateCondition.CAN_TRANSITION_TO_SELF)))
                {
                    ret = potenTransition;
                }
            }

            //last check before we return

            //did we get hit?
            bool gotHit = EnumHelper.HasEnum((uint)playerFlags, (uint)TransitionFlag.GOT_HIT);
            //does the transition we get care about if we got hit or not?
            bool caresAbtHit = EnumHelper.HasEnum((uint)ret.transitionFlag, (uint)TransitionFlag.GOT_HIT);
            //UnityEngine.Debug.Log(gotHit + " - " + caresAbtHit);

            //only override to go into stun state if we got hit and the transition doesn't care about that
            //also, we found a transition we found is valid
            if (gotHit && !caresAbtHit && (fromState != 0))
            {
                UnityEngine.Debug.Log("attempting to find stun state");
                //all stun states should be attached to the default state
                ret = TryTransitionState(0, playerInputs, playerCond, playerFlags, facing, playerResources);
            }


            return ret;
        }

        public int TryMoveList(RecorderElement[] playerInputs, CancelConditions playerCond, int facing, ResourceData playerResources)
        {
            //search the movelist for a valid transition
            //integer of the target state
            int ret = moveList.Check(playerInputs, playerCond, facing, playerResources);

            return ret;

        }
    }
}