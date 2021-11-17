using ActionGameEngine.Data;
using ActionGameEngine.Input;
using UnityEngine.InputSystem;
namespace ActionGameEngine.Gameplay
{
    public abstract class ControllableObject : CombatObject
    {
        protected InputRecorder inputRecorder;

        [UnityEngine.SerializeField] protected InputItem fromPlayer;

        //Unity's input mapping, replace when given the opportunity
        public InputActionAsset actions;


        protected override void OnAwake()
        {
            base.OnAwake();
            inputRecorder = new InputRecorder();

            //using the input system to do a little mapping, replace as soon as possible
            actions.Enable();
            //pressed events
            actions["Forwards"].started += ctx => ApplyInput(0b0000000000000010, true);
            actions["Backwards"].started += ctx => ApplyInput(0b0000000000000011, true);
            //released events
            actions["Forwards"].canceled += ctx => ApplyInput(0b0000000000000010, false);
            actions["Backwards"].canceled += ctx => ApplyInput(0b0000000000000011, false);
        }

        protected override void InputUpdate()
        {
            fromPlayer.MultX(status.facing);
            //so the input is recorded
            BufferInput();
        }

        protected override void TryTransitionState()
        {

            //get a transition, valid if we found a new state to transition to
            TransitionData transition = data.TryTransitionState(status.GetCurrentStateID(), inputRecorder.GetInputArray(), status.GetCancelConditions(), status.GetTransitionFlags());
            //UnityEngine.Debug.Log(transition.IsValid() + " " + transition.targetState);
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;

                AssignNewState(newStateID);
                //process the TransitionEvent flags that are set
                ProcessTransitionEvents(transition.transitionEvent);
            }

        }

        private void ApplyInput(short input, bool pressed)
        {
            if (pressed)
            {
                fromPlayer.m_rawValue |= input;
            }
            else
            {
                fromPlayer.m_rawValue ^= input;
            }
        }

        protected void BufferInput()
        {
            //adds a new input, check if state can transition
            bool buffered = inputRecorder.BufferInput(fromPlayer);
            //UnityEngine.Debug.Log("buffered :: " + buffered);
            status.SetCheckState(buffered);
        }
    }
}