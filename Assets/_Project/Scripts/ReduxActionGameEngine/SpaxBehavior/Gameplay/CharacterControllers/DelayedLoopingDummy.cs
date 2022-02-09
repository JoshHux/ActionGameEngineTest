using UnityEngine;
using ActionGameEngine.Input;

namespace ActionGameEngine
{
    public class DelayedLoopingDummy : ActionCharacterController
    {
        [SerializeField] private bool loopInput;
        [SerializeField] private InputItem toRepeat;
        [SerializeField] private int delay;

        private int elapsedFrames;

        protected override void OnAwake()
        {
            controllable = false;

            base.OnAwake();
            actions.Enable();

            //pressed events
            actions["New action"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, false, false);
            actions["New action1"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, false, false);
            actions["New action2"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, false, false);
            actions["New action3"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, false, false);
            actions["New action4"].started += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000100000000000, false, false);

            //released events
            actions["New action"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000001000000, true, false);
            actions["New action1"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000010000000, true, false);
            actions["New action2"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000000100000000, true, false);
            actions["New action3"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000001000000000, true, false);
            actions["New action4"].canceled += ctx => ApplyInput(new UnityEngine.Vector2(), 0b0000100000000000, true, false);
        }

        protected override void OnStart()
        {
            elapsedFrames = 0;
            base.OnStart();
        }

        protected override void InputUpdate()
        {
            //if we want to loop
            if (loopInput)
            {
                //if the time passed is equal to the delay
                if (elapsedFrames >= delay)
                {
                    fromPlayer.m_rawValue |= toRepeat.m_rawValue;
                    elapsedFrames = 0;
                }
                else
                {
                    elapsedFrames++;
                    fromPlayer.m_rawValue &= (ushort)~toRepeat.m_rawValue;
                }
            }
            base.InputUpdate();
        }
    }
}