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
        public bool controllable = true;


        protected override void OnAwake()
        {
            base.OnAwake();
            inputRecorder = new InputRecorder();
            if (controllable)
            {
                //using the input system to do a little mapping, replace as soon as possible
                actions.Enable();
                //pressed events
                actions["Direction"].started += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, false, true);
                actions["Punch"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, false, false);
                actions["Kick"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, false, false);
                actions["Slash"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, false, false);
                actions["Dust"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, false, false);
                actions["Jump"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000010000000000, false, false);
                //released events
                actions["Direction"].canceled += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, true, true);
                actions["Punch"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, true, false);
                actions["Kick"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, true, false);
                actions["Slash"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, true, false);
                actions["Dust"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, true, false);
                actions["Jump"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000010000000000, true, false);
            }
        }

        protected override void InputUpdate()
        {
            //so the input is recorded
            BufferInput();
        }

        protected override void TryTransitionState()
        {

            //get a transition, valid if we found a new state to transition to
            TransitionData transition = data.TryTransitionState(status.GetCurrentStateID(), inputRecorder.GetInputArray(), status.GetCancelConditions(), status.GetTransitionFlags(), status.facing);
            //UnityEngine.Debug.Log(transition.IsValid() + " " + transition.targetState);
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;
                //process the TransitionEvent flags that are set before you set the new state
                ProcessTransitionEvents(transition.transitionEvent);
                AssignNewState(newStateID);

            }
            //we didn't find a valid transition, try looking in the movelist
            else
            {
                //get new state ID from the movelist
                int newState = data.TryMoveList(inputRecorder.GetInputArray(), status.GetCancelConditions(), status.facing);

                //check if it's valid
                if (newState != -1)
                {
                    AssignNewState(newState);
                }

                status.checkState = false;
            }

        }

        private void ApplyInput(UnityEngine.Vector2 dir, short input, bool released, bool isDIr)
        {
            ushort newDir = (ushort)fromPlayer.m_rawValue;

            FVector2 StickInput = new FVector2((Fix64)dir.x, (Fix64)dir.y);
            Fix64 StickAngle = (Fix64.Atan2(StickInput.x, StickInput.y)) * Fix64.PiInv * (Fix64)180f;

            if (isDIr)
            {
                newDir &= 0b1111111111000000;

                if (dir.y < 0)
                {
                    newDir |= 0b0000000000001100;
                }
                else if (dir.y > 0)
                {
                    newDir |= 0b0000000000001000;
                }

                if (dir.x < 0)
                {
                    newDir |= 0b0000000000000011;
                }
                else if (dir.x > 0)
                {
                    newDir |= 0b0000000000000010;
                }

            }
            else
            {

                if (input != 0)
                {
                    if (released)
                    {
                        newDir &= (ushort)~input;
                    }
                    else
                    {
                        newDir |= (ushort)input;
                    }
                }
            }

            fromPlayer.m_rawValue = (short)newDir;
        }

        protected void BufferInput()
        {
            //adds a new input, check if state can transition
            bool buffered = inputRecorder.BufferInput(fromPlayer);
            //if ((fromPlayer.m_rawValue & (1 << 10)) > 0) { UnityEngine.Debug.Log(buffered); }
            //UnityEngine.Debug.Log("buffered :: " + buffered);
            status.SetCheckState(buffered);
        }
    }
}