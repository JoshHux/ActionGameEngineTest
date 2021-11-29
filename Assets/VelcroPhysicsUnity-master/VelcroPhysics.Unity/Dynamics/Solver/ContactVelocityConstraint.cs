using VelcroPhysics.Shared;
using FixMath.NET;

namespace VelcroPhysics.Dynamics.Solver
{
    public sealed class ContactVelocityConstraint
    {
        public int ContactIndex;
        public Fix64 Friction = 0;
        public int IndexA;
        public int IndexB;
        public Fix64 InvIA, InvIB;
        public Fix64 InvMassA, InvMassB;
        public Mat22 K;
        public FVector2 Normal;
        public Mat22 NormalMass;
        public int PointCount;
        public VelocityConstraintPoint[] Points = new VelocityConstraintPoint[Settings.MaxManifoldPoints];
        public Fix64 Restitution;
        public Fix64 TangentSpeed;

        public ContactVelocityConstraint()
        {
            for (var i = 0; i < Settings.MaxManifoldPoints; i++) Points[i] = new VelocityConstraintPoint();
        }
    }
}