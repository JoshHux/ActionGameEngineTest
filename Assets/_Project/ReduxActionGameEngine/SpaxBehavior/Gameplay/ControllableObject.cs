using ActionGameEngine.Input;
namespace ActionGameEngine.Gameplay
{
    public abstract class ControllableObject : CombatObject
    {
        protected InputRecorder inputRecorder;

        protected InputItem fromPlayer;

        protected void BufferInput()
        {
            status.checkState = inputRecorder.BufferInput(fromPlayer);
        }
    }
}