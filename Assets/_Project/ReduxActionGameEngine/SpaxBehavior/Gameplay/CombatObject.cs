using ActionGameEngine.Gameplay;
using ActionGameEngine.Interfaces;
using ActionGameEngine.Data;
public abstract class CombatObject : VulnerableObject, IDamager
{
    protected Hitbox[] hitboxes;

    //PLEASE TREAT AS PRIVATE
    public abstract int ConnectedHit(HitboxData boxData);
}
