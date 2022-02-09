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
        //required resources to transition
        public ResourceData resources;

        public TransitionData(int ts, Command cm, CancelConditions cc, TransitionFlag tf, TransitionEvent te, ResourceData rd)
        {
            this.targetState = ts;
            this.cmdMotion = cm;
            this.cancelConditions = cc;
            this.transitionFlag = tf;
            this.transitionEvent = te;
            this.resources = rd;
        }

        public bool IsValid()
        {
            return targetState > -1;
        }

        public bool Check(RecorderElement[] playerInputs, TransitionFlag playerFlags, CancelConditions playerCond, int facing, ResourceData playerResources)
        {
            //UnityEngine.Debug.Log(playerInputs[0].frag.inputItem.m_rawValue);
            bool passCancelConditions = EnumHelper.HasEnum((uint)playerCond, (uint)cancelConditions, true);
            bool passResources = passCancelConditions && this.resources.Check(playerResources);
            bool passTransitionFlags = passResources && EnumHelper.HasEnum((uint)playerFlags, (uint)transitionFlag, true);
            bool checkInput = passTransitionFlags && cmdMotion.Check(playerInputs, facing);

            return checkInput;
        }

    }
}