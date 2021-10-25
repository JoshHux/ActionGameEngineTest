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
            status.SetCheckState(inputRecorder.BufferInput(fromPlayer));
        }

        protected override void InputUpdate()
        {
            //so the input is recorded
            BufferInput();
        }

        protected override void StateUpdate()
        {
            //call base to tick timers
            base.StateUpdate();
            if (status.GetCheckState())
            {
                TransitionData transition = data.TryTransitionState(status.GetCurrentStateID().stateID, inputRecorder.GetInputArray(), status.GetCancelConditions(), status.GetTransitionFlags());
                if (transition.IsValid())
                {
                    int newStateID = transition.targetState;
                    StateData newState = data.GetStateFromID(newStateID);

                    //setting new state information to CharacterStatus
                    status.SetNewState(newState);
                    status.SetNewStateConditions(data.GetConditionsFromState(newStateID));
                    status.SetNewCancelConditions(newState.cancelConditions);

                    //process the TransitionEvent flags that are set
                    ProcessTransitionEvents((int)transition.transitionEvent);
                }
            }
        }
    }
}