using ActionGameEngine.Data;
namespace ActionGameEngine.Interfaces
{
    public interface IDamager
    {
        int ConnectedHit(HitboxData boxData);
    }
}