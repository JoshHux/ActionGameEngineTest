using Spax.Input;

namespace Spax.StateMachine
{
    [System.Serializable]
    public struct Transition
    {
        public int TargetStateID;
        public uint meterReq;

        public TransitionCondition Condition;

        public InputCodeFlags inputConditions;

        public CancelCondition cancelCondition;

        public Transition DeepCopy()
        {
            Transition ret = new Transition();
            ret.meterReq = this.meterReq;
            ret.Condition = this.Condition;
            ret.inputConditions = this.inputConditions;
            ret.cancelCondition = this.cancelCondition;
            return ret;
        }
    }
}
