using ActionGameEngine.Data;
using ActionGameEngine.Enum;
namespace ActionGameEngine.Interfaces
{
    public interface IDamageable
    {
        HitIndicator GetHit(int attackerID, HitboxData boxData);
    }
}