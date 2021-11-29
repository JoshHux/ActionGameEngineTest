using ActionGameEngine;

namespace ActionGameEngine.Interfaces
{
    public interface ICollideable
    {
        void TriggerCollided(EnvironmentDetector sender);
        void TriggerExitCollided(EnvironmentDetector sender);
    }
}