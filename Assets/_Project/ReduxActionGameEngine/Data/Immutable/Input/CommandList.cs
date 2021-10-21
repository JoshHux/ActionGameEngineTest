using ActionGameEngine.Input;
using ActionGameEngine.Enum;

namespace ActionGameEngine.Data
{
    [System.Serializable]
    //container that holds the 
    //the moves that are closer to the start of the list have higher priority
    public struct CommandList
    {
        public MotionTransition[] command;

        //-1 if state is not found
        public int Check(RecorderElement[] playerInputs, CancelConditions cond)
        {
            int len = command.Length;
            for (int i = 0; i < len; i++)
            {
                MotionTransition cmd = command[i];
                bool found = cmd.Check(playerInputs, cond);
                if (found)
                {
                    return cmd.targetState;
                }
            }
            return -1;
        }
    }
}
