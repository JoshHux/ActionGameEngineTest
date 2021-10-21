
namespace ActionGameEngine.Interfaces
{
    public interface ICollideable
    {
        public void TriggerCollided(object sender);
        public void TriggerExitCollided(object sender);
    }
}