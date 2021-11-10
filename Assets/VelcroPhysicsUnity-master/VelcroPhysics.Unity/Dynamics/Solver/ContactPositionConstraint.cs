using VelcroPhysics.Collision.Narrowphase;
using FixMath.NET;

namespace VelcroPhysics.Dynamics.Solver
{
    public sealed class ContactPositionConstraint
    {
        public int IndexA;
        public int IndexB;
        public Fix64 InvIA, InvIB;
        public Fix64 InvMassA, InvMassB;
        public FVector2 LocalCenterA, LocalCenterB;
        public FVector2 LocalNormal;
        public FVector2 LocalPoint;
        public FVector2[] LocalPoints = new FVector2[Settings.MaxManifoldPoints];
        public int PointCount;
        public Fix64 RadiusA, RadiusB;
        public ManifoldType Type;
    }
}