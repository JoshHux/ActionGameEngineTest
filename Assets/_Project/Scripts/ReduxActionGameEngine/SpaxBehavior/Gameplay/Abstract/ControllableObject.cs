using ActionGameEngine.Data;
using ActionGameEngine.Enum;
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
                actions["Direction"].performed += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, false, true);
                actions["Punch"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, false, false);
                actions["Kick"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, false, false);
                actions["Slash"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, false, false);
                actions["Dust"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, false, false);
                actions["Jump"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000010000000000, false, false);
                actions["Block"].performed += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000100000000000, false, false);
                //released events
                actions["Direction"].canceled += ctx => ApplyInput(ctx.ReadValue<UnityEngine.Vector2>(), 0b0000000000000000, true, true);
                actions["Punch"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, true, false);
                actions["Kick"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, true, false);
                actions["Slash"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, true, false);
                actions["Dust"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, true, false);
                actions["Jump"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000010000000000, true, false);
                actions["Block"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000100000000000, true, false);
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
            TransitionData transition = data.TryTransitionState(status.GetCurrentStateID(), inputRecorder.GetInputArray(), status.GetCancelConditions(), status.GetTransitionFlags(), status.facing, status.resources);
            //UnityEngine.Debug.Log(transition.IsValid() + " " + transition.targetState);
            if (transition.IsValid())
            {
                int newStateID = transition.targetState;

                //get the current state before the transition
                StateData curState = status.currentState;
                //process the exitEvents flags before transitioning
                TransitionEvent exitEvents = curState.exitEvents;
                ProcessTransitionEvents(exitEvents);

                //process the TransitionEvent flags that are set before you transition to the new state
                ProcessTransitionEvents(transition.transitionEvent, status.resources);
                AssignNewState(newStateID);

                //we assign it again since we know that the current state should be the new state 
                curState = status.currentState;
                //process the enterEvents flags before transitioning
                TransitionEvent enterEvents = curState.enterEvents;
                ProcessTransitionEvents(enterEvents);

            }
            //we didn't find a valid transition, try looking in the movelist
            else
            {
                //get new state ID from the movelist
                int newState = data.TryMoveList(inputRecorder.GetInputArray(), status.GetCancelConditions(), status.facing, status.resources);

                //check if it's valid
                if (newState != -1)
                {
                    //get the current state before the transition
                    StateData curState = status.currentState;
                    //process the exitEvents flags before transitioning
                    TransitionEvent exitEvents = curState.exitEvents;
                    ProcessTransitionEvents(exitEvents);

                    //assign the new state
                    AssignNewState(newState);

                    //we assign it again since we know that the current state should be the new state 
                    curState = status.currentState;
                    //process the enterEvents flags before transitioning
                    TransitionEvent enterEvents = curState.enterEvents;
                    ProcessTransitionEvents(enterEvents);
                }
                else
                {

                    status.checkState = false;
                }
            }

        }

        protected void ApplyInput(UnityEngine.Vector2 dir, short input, bool released, bool isDIr)
        {
            ushort newDir = fromPlayer.m_rawValue;

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

            fromPlayer.m_rawValue = newDir;
        }

        protected void BufferInput()
        {
            //adds a new input, check if state can transition
            bool buffered = inputRecorder.BufferInput(fromPlayer);
            //if ((fromPlayer.m_rawValue & (1 << 10)) > 0) { UnityEngine.Debug.Log(buffered); }
            //UnityEngine.Debug.Log("buffered :: " + buffered);


            //make sure to keep any previous reason for checking a state
            status.SetCheckState(buffered || status.checkState);
        }
    }
}