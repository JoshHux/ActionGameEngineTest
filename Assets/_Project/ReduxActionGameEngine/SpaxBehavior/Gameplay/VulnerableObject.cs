using ActionGameEngine.Data;
using ActionGameEngine.Interfaces;
namespace ActionGameEngine.Gameplay
{
    public abstract class VulnerableObject : LivingObject, IDamageable
    {
        public abstract int GetHit(HitboxData boxData);
    }
}