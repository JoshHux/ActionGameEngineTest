using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ActionGameEngine.Gameplay
{
    public delegate void TimerEventHandler(object sender);
    //frame timer for if you want the timer to invoke a function when it ends
    public class CallbackTimer : FrameTimer
    {
        private event TimerEventHandler OnEnd;
        protected override void OnTimerEnd()
        {
            OnEnd?.Invoke(this);
        }
    }
}