using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ActionGameEngine.Gameplay
{
    public delegate void TimerEventHandler(object sender);
    [System.Serializable]
    //frame timer for if you want the timer to invoke a function when it ends
    public class CallbackTimer : FrameTimer
    {
        public event TimerEventHandler OnEnd;

        public CallbackTimer() : base() { }
        protected override void OnTimerEnd()
        {
            base.OnTimerEnd();
            OnEnd?.Invoke(this);
        }
    }
}