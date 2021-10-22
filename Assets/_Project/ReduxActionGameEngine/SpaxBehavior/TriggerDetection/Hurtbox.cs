using ActionGameEngine.Data;
using ActionGameEngine.Interfaces;
using Spax;


namespace ActionGameEngine.Gameplay
{
    public class Hurtbox : SpaxBehavior
    {
        //what to send hit signal to when this is hit
        private IDamageable damageable;

        public int HitThisBox(HitboxData boxData)
        {
            return damageable.GetHit(boxData);
        }
    }
}