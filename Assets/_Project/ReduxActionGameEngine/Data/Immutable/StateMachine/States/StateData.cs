using ActionGameEngine.Enum;
using ActionGameEngine.Input;

namespace ActionGameEngine.Data
{
    //struct to hold all the data of a given character's state
    [System.Serializable]
    public struct StateData
    {
        //ID of state
        public int stateID;

        //ID of parent state, if negative, no parent
        public int parentID;

        //how many frames the state goes on for
        public int duration;

        //characteristics of state
        public StateCondition stateConditions;
        //what it can cancel into
        public CancelConditions cancelConditions;

        //possible transitions
        public TransitionData[] transitions;

        //frames of state
        public FrameData[] frames;

        public string animName;

        public FrameData GetFrameAt(int f)
        {

            //check for out of bounds or invalid frames
            if (f >= duration || f < 0)
            {
                //return invalid frame
                return new FrameData();
            }

            int i = 0;
            int len = frames.Length;
            while (i < len)
            {
                FrameData retFrame = frames[i];

                if (retFrame.atFrame == f)
                {
                    return retFrame;
                }

                i++;
            }

            //return invalid frame
            return new FrameData();
        }

        //returns the index of the transition if true, -1 otherwise
        public int CheckTransitions(RecorderElement[] playerInputs, TransitionFlag playerFlags, CancelConditions playerCond)
        {
            int len = transitions.Length;

            for (int i = 0; i < len; i++)
            {
                TransitionData transition = transitions[i];

                if (transition.Check(playerInputs, playerFlags, playerCond))
                {
                    return i;
                }
            }

            return -1;
        }

        //check if this is a node state
        //should not be assigned as a player state is true
        //if there are no frames, then this is a node state
        public bool IsNodeState() { return frames.Length == 0; }
        public bool HasParent() { return stateID != parentID; }
        public TransitionData GetTransitionFromIndex(int ind) { return transitions[ind]; }
    }
}
