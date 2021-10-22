using ActionGameEngine.Gameplay;
using ActionGameEngine.Data;
using ActionGameEngine.Interfaces;
namespace ActionGameEngine.Gameplay
{
    public abstract class VulnerableObject : LivingObject, IDamageable
    {
        protected Hurtbox[] hurtboxes;
        public abstract int GetHit(HitboxData boxData);
    }
}