
namespace ActionGameEngine.Interfaces
{
    public interface ICollideable
    {
        void TriggerCollided(object sender);
        void TriggerExitCollided(object sender);
    }
}