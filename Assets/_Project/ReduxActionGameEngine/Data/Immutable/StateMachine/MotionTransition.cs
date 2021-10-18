using ActionGameEngine.Input;
using ActionGameEngine.Enum;

namespace ActionGameEngine.Data
{
    [System.Serializable]
    //motion input must be completed in order to transition to target state
    public struct MotionTransition
    {
        //target state to transition to once requirements are met
        public int targetState;
        //conditions that must be met if we want it to transition to the target state
        public CancelConditions cancelConditions;
        //motion required to transition to state
        public Command cmdMotion;

        public bool Check(InputFragment[] playerInputs, CancelConditions playerCond)
        {
            return EnumHelper.HasEnum((int)playerCond, (int)cancelConditions) && cmdMotion.Check(playerInputs);
        }
    }
}
