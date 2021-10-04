using Spax.Input;

namespace Spax.StateMachine
{

    [System.Serializable]
    public class Transition
    {
        public uint ID;
        public bool Enabled;
        public int Target;
        public uint meterReq;
        public TransitionCondition Condition;
        public InputCodeFlags inputConditions;
        public CancelCondition cancelCondition;

        public Transition DeepCopy()
        {
            Transition ret = new Transition();
            ret.ID = this.ID;
            ret.Target = this.Target;
            ret.meterReq = this.meterReq;
            ret.Condition = this.Condition;
            ret.inputConditions = this.inputConditions;
            ret.cancelCondition = this.cancelCondition;
            return ret;
        }

    }
}