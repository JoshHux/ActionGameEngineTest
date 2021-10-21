using Spax.StateMachine;
namespace Spax.Interfaces
{
    public interface IDamager
    {
        public void OnHitConnect(HitBoxData boxData, int hitType);
    }
}