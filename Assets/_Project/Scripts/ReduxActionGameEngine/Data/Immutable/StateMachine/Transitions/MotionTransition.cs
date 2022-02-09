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
        //required resources to transition
        public ResourceData resources;

        public MotionTransition(int ts, CancelConditions cc, Command cmd, ResourceData rd)
        {
            this.targetState = ts;
            this.cancelConditions = cc;
            this.cmdMotion = cmd;
            this.resources = rd;
        }

        public bool Check(RecorderElement[] playerInputs, CancelConditions playerCond, int facing, ResourceData playerResources)
        {
            var checkCC = EnumHelper.HasEnum((uint)playerCond, (uint)cancelConditions, true);
            var checkRD = checkCC && resources.Check(playerResources);
            var checkCMD = checkRD && cmdMotion.Check(playerInputs, facing);
            return checkCMD;
        }
    }
}
