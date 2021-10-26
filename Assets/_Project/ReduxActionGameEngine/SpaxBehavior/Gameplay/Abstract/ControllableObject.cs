using ActionGameEngine.Data;
using ActionGameEngine.Input;
namespace ActionGameEngine.Gameplay
{
    public abstract class ControllableObject : CombatObject
    {
        protected InputRecorder inputRecorder;

        protected InputItem fromPlayer;


        protected override void OnAwake()
        {
            base.OnAwake();
            inputRecorder = new InputRecorder();
        }

        protected void BufferInput()
        {
            //adds a new input, check if state can transition
            status.SetCheckState(inputRecorder.BufferInput(fromPlayer));
        }

        protected override void InputUpdate()
        {
            //so the input is recorded
            BufferInput();
        }

        protected override void TryTransitionState()
        {

            //get a transition, valid if we found a new state to transition to
            TransitionData transition = data.TryTransitionState(status.GetCurrentStateID(), inputRecorder.GetInputArray(), status.GetCancelConditions(), status.GetTransitionFlags());
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;

                AssignNewState(newStateID);
                //process the TransitionEvent flags that are set
                ProcessTransitionEvents(transition.transitionEvent);
            }

        }
    }
}