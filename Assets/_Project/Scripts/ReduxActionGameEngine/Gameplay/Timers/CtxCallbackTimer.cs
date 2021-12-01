using ActionGameEngine.Data;
namespace ActionGameEngine.Gameplay
{
    public class CtxCallbackTimer<T> : CallbackTimer
    {
        private T data;
        public CtxCallbackTimer() : base() { }

        public void StartTimer(int time, T newData)
        {
            SetData(newData);
            StartTimer(time);
        }
        public void SetData(T newData) { this.data = newData; }
        public T GetData() { return this.data; }
    }
}