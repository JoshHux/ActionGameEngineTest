using FixMath.NET;

namespace VelcroPhysics.Dynamics.Solver
{
    public sealed class VelocityConstraintPoint
    {
        public Fix64 NormalImpulse;
        public Fix64 NormalMass;
        public FVector2 rA;
        public FVector2 rB;
        public Fix64 TangentImpulse;
        public Fix64 TangentMass;
        public Fix64 VelocityBias;
    }
}