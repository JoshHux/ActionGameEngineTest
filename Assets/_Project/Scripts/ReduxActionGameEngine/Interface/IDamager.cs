using ActionGameEngine.Data;
using ActionGameEngine.Enum;
namespace ActionGameEngine.Interfaces
{
    public interface IDamager
    {
        int ConnectedHit(HitboxData boxData, HitIndicator indicator);
    }
}