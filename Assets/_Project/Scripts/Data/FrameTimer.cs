using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class FrameTimer
{
    //maybe add event handler that is invoked every time it ticks
    public event TimerEventHandler onEnd;

    private bool isTicking = false;
    public bool isDone = true;
    [SerializeField] private int endTime;
    [SerializeField] private int time;

    public void StartTimer(int setTime)
    {
        SetTimer(setTime);
        PlayTimer();
    }

    public void SetTimer(int setTime)
    {

        endTime = setTime;
        isDone = false;
        time = 0;


        isTicking = false;

    }

    public void PlayTimer()
    {

        isTicking = true;

    }

    public bool TickTimer()
    {
        if (endTime > 0 && isTicking)
        {
            //if == instead, then it ends one frame early
            if (++time > endTime)
            {
                EndTimer();
            }
        }

        return isTicking;
    }

    public int ElapsedTime()
    {
        return time;
    }

    public int TimeLeft()
    {
        return (endTime - time);
    }

    public bool IsTicking()
    {
        return isTicking;
    }

    public void ForceTimerEnd()
    {
        EndTimer();
    }

    private void EndTimer()
    {
        isTicking = false;
        isDone = true;
        endTime = 0;
        onEnd?.Invoke(this);
    }

    public bool IsPaused()
    {
        return endTime > 0;
    }
    public delegate void TimerEventHandler(object sender);

}
