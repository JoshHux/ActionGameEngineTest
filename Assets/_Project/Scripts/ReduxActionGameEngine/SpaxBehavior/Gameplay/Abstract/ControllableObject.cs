using ActionGameEngine.Data;
using ActionGameEngine.Input;
using UnityEngine.InputSystem;
using FixMath.NET;
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
            actions["Direction"].started += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000010);
            //released events
            actions["Direction"].canceled += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000011);
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
            //UnityEngine.Debug.Log(transition.IsValid() + " " + transition.targetState);
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;

                AssignNewState(newStateID);
                //process the TransitionEvent flags that are set
                ProcessTransitionEvents(transition.transitionEvent);
            }

        }

        private void ApplyInput(UnityEngine.Vector2 dir, short input)
        {
            short newDir = 0;
            FVector2 StickInput = new FVector2((Fix64)dir.x, (Fix64)dir.y);
            Fix64 StickAngle = (Fix64.Atan2(StickInput.x, StickInput.y)) * Fix64.PiInv * (Fix64)180f;

            if (dir.y < 0)
            {
                newDir = 0b0000000000001100;
            }
            else if (dir.y > 0)
            {
                newDir = 0b0000000000001000;
            }

            if (dir.x < 0)
            {
                newDir |= 0b0000000000000011;
            }
            else if (dir.x > 0)
            {
                newDir |= 0b0000000000000010;
            }

            fromPlayer.m_rawValue = newDir;
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