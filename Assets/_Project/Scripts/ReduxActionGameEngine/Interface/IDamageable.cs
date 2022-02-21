using FixMath.NET;
using ActionGameEngine.Data;
using ActionGameEngine.Enum;
namespace ActionGameEngine.Interfaces
{
    public interface IDamageable
    {
        HitIndicator GetHit(int attackerID, HitboxData boxData, Fix64 proration);
    }
}