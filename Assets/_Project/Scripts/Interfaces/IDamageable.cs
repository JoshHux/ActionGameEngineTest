using Spax.StateMachine;
namespace Spax.Interfaces
{
    public interface IDamageable
    {
        public int GetHit(HitBoxData boxData);
    }
}