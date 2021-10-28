using ActionGameEngine.Enum;
using ActionGameEngine.Input;
using ActionGameEngine.Data.Helpers.Wrappers;
using BEPUutilities;
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
        private Fix64 friction;
        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        private BepuVector3 maxVelocity;
        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        private BepuVector3 acceleration;

        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        private StateListWrapper stateList;
        public CommandList moveList;

        public Fix64 GetFriction() { return friction; }
        public Fix64 GetForwardsAccel() { return acceleration.X; }
        public Fix64 GetSideAccel() { return acceleration.Y; }
        public Fix64 GetBackAccel() { return acceleration.Z; }
        public Fix64 GetMaxForwardsVel() { return maxVelocity.X; }
        public Fix64 GetMaxSideVel() { return maxVelocity.Y; }
        public Fix64 GetMaxBackVel() { return maxVelocity.Z; }

        public CharacterData(int mhp, Fix64 m, Fix64 f, BepuVector3 mv, BepuVector3 a, StateListWrapper sl, CommandList cl)
        {
            maxHp = mhp;
            mass = m;
            friction = f;
            maxVelocity = mv;
            acceleration = a;
            stateList = sl;
            moveList = cl;
        }

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
            if (state.HasParent() && (stateID > -1))
            {
                ret |= this.GetConditionsFromState(state.parentID);
            }

            return ret;
        }

        //returns index of transition if true
        //version without input for non-controllable object
        public TransitionData TryTransitionState(int fromState, CancelConditions playerCond, TransitionFlag playerFlags)
        {
            RecorderElement[] playerInputs = new RecorderElement[1];
            return TryTransitionState(fromState, playerInputs, playerCond, playerFlags);
        }

        //returns index of transition if true
        public TransitionData TryTransitionState(int fromState, RecorderElement[] playerInputs, CancelConditions playerCond, TransitionFlag playerFlags)
        {
            StateData state = stateList[fromState];
            int check = state.CheckTransitions(playerInputs, playerFlags, playerCond);

            if (check > -1)
            {
                TransitionData potenTransition = state.GetTransitionFromIndex(check);
                int potenStateID = potenTransition.targetState;
                StateData potenState = stateList[potenStateID];

                //check if it's a node state, if so, then check transitions on that
                //we should NEVER return a transition TO a node state
                if (potenState.IsNodeState())
                {
                    return TryTransitionState(potenStateID, playerInputs, playerCond, playerFlags);
                }

                return potenTransition;
            }

            //make invalid transition by setting the target state to -1
            TransitionData ret = new TransitionData();
            ret.targetState = -1;
            return ret;
        }


    }
}
