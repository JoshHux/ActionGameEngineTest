using ActionGameEngine.Enum;
using ActionGameEngine.Input;
namespace ActionGameEngine.Data
{
    [System.Serializable]
    public struct TransitionData
    {

        //target state to transition to once requirements are met
        public int targetState;
        //motion required to transition to state
        public Command cmdMotion;
        //conditions that must be met if we want it to transition to the target state
        public CancelConditions cancelConditions;
        //required flags in order to transition to state
        public TransitionFlag transitionFlag;
        //events to influence the gamestate one the transition is ran
        public TransitionEvent transitionEvent;

        public TransitionData(int ts, Command cm, CancelConditions cc, TransitionFlag tf, TransitionEvent te)
        {
            targetState = ts;
            cmdMotion = cm;
            cancelConditions = cc;
            transitionFlag = tf;
            transitionEvent = te;
        }

        public bool IsValid()
        {
            return targetState > -1;
        }

        public bool Check(RecorderElement[] playerInputs, TransitionFlag playerFlags, CancelConditions playerCond)
        {
            return EnumHelper.HasEnum((int)playerCond, (int)cancelConditions) && EnumHelper.HasEnum((int)playerFlags, (int)transitionFlag, true) && cmdMotion.Check(playerInputs);
        }

    }
}