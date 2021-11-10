using VelcroPhysics.Dynamics;
using FixMath.NET;

namespace VelcroPhysics.Extensions.PhysicsLogics.PhysicsLogicBase
{
    public struct ShapeData
    {
        public Body Body;
        public Fix64 Max;
        public Fix64 Min; // absolute angles
    }
}