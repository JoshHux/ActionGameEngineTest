

namespace ActionGameEngine.Gameplay
{
    public abstract class FrameTimer
    {
        //timer ends when this hits 0
        private int timeRemaining = 0;
        //true if we want to pause the timer
        private bool paused = true;

        public FrameTimer()
        {
            timeRemaining = 0;
            paused = true;
        }

        //returns true if timer is able to and does tick
        public bool TickTimer()
        {
            if (!paused)
            {
                //ends if the time remaining is 0
                //guarantees at least one tick if timer is set to 1
                if (timeRemaining > 0)
                {
                    timeRemaining -= 1;
                    return true;
                }
                else
                {
                    EndTimer();
                }
            }
            return false;
        }

        public int GetTimeRemaining()
        {
            return timeRemaining;
        }

        public bool IsPaused()
        {
            return paused;
        }


        public bool IsTicking()
        {
            return timeRemaining > 0;
        }

        //sets the time for the timer and automatically plays it


        //sets the new time without checking if it's paused, useful if you want to prep the time before letting it play
        public void SetTime(int time)
        {
            timeRemaining = time;
        }

        private void SetPaused(bool tf)
        {
            paused = tf;
        }

        public void PauseTimer()
        {
            this.SetPaused(true);
        }

        public void PlayTimer()
        {
            this.SetPaused(false);
        }

        public void StartTimer(int newTime)
        {
            if (this.IsTicking()) { this.EndTimer(); }

            this.SetTime(newTime);
            this.PlayTimer();
        }

        public virtual void EndTimer()
        {
            this.SetTime(0);
            this.OnTimerEnd();
        }

        protected virtual void OnTimerEnd()
        {
            this.PauseTimer();
            this.OnTimerEnd();
        }

    }
}