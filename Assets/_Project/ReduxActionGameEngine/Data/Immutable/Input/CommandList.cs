using ActionGameEngine.Input;

namespace ActionGameEngine.Data
{
    [System.Serializable]
    //container that holds the 
    //the moves that are closer to the start of the list have higher priority
    public struct CommandList
    {
        public MotionTransition[] command;

        //-1 if state is not found
        public int Check(InputFragment[] playerInputs)
        {
            foreach (MotionTransition cmd in command)
            {
                bool found = cmd.Check(playerInputs);
                if (found)
                {
                    return cmd.targetState;
                }
            }
            return -1;
        }
    }
}
