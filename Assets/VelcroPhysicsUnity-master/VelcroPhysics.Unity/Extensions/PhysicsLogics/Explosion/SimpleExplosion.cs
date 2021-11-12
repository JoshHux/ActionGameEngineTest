using System.Collections.Generic;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Extensions.PhysicsLogics.PhysicsLogicBase;
using VelcroPhysics.Shared;
using FixMath.NET;

namespace VelcroPhysics.Extensions.PhysicsLogics.Explosion
{
    /// <summary>
    /// Creates a simple explosion that ignores other bodies hiding behind static bodies.
    /// </summary>
    public sealed class SimpleExplosion : PhysicsLogic
    {
        public SimpleExplosion(World world)
            : base(world, PhysicsLogicType.Explosion)
        {
            Power = 1; //linear
        }

        /// <summary>
        /// This is the power used in the power function. A value of 1 means the force
        /// applied to bodies in the explosion is linear. A value of 2 means it is exponential.
        /// </summary>
        public Fix64 Power { get; set; }

        /// <summary>
        /// Activate the explosion at the specified position.
        /// </summary>
        /// <param name="pos">The position (center) of the explosion.</param>
        /// <param name="radius">The radius of the explosion.</param>
        /// <param name="force">The force applied</param>
        /// <param name="maxForce">A maximum amount of force. When force gets over this value, it will be equal to maxForce</param>
        /// <returns>A list of bodies and the amount of force that was applied to them.</returns>
        public Dictionary<Body, FVector2> Activate(FVector2 pos, Fix64 radius, Fix64 force,
            Fix64? holdMaxForce = null)
        {

            Fix64 maxForce = holdMaxForce ??  Fix64.MaxValue;
            var affectedBodies = new HashSet<Body>();

            AABB aabb;
            aabb.LowerBound = pos - new FVector2(radius, radius);
            aabb.UpperBound = pos + new FVector2(radius, radius);

            // Query the world for bodies within the radius.
            World.QueryAABB(fixture =>
            {
                if (FVector2.Distance(fixture.Body.Position, pos) <= radius)
                    if (!affectedBodies.Contains(fixture.Body))
                        affectedBodies.Add(fixture.Body);

                return true;
            }, ref aabb);

            return ApplyImpulse(pos, radius, force, maxForce, affectedBodies);
        }

        private Dictionary<Body, FVector2> ApplyImpulse(FVector2 pos, Fix64 radius, Fix64 force, Fix64 maxForce,
            HashSet<Body> overlappingBodies)
        {
            var forces = new Dictionary<Body, FVector2>(overlappingBodies.Count);

            foreach (var overlappingBody in overlappingBodies)
                if (IsActiveOn(overlappingBody))
                {
                    var distance = FVector2.Distance(pos, overlappingBody.Position);
                    var forcePercent = GetPercent(distance, radius);

                    var forceVector = pos - overlappingBody.Position;
                    forceVector *=
                        Fix64.One / Fix64.Sqrt(forceVector.x * forceVector.x + forceVector.y * forceVector.y);
                    forceVector *=  Fix64.Min(force * forcePercent, maxForce);
                    forceVector *= -1;

                    overlappingBody.ApplyLinearImpulse(forceVector);
                    forces.Add(overlappingBody, forceVector);
                }

            return forces;
        }

        private Fix64 GetPercent(Fix64 distance, Fix64 radius)
        {
            //(1-(distance/radius))^power-1
            var percent = Fix64.Pow(1 - (distance - radius) / radius, Power) - 1;

            if (Fix64.IsNaN(percent))
                return Fix64.Zero;

            return Fix64.Clamp(percent, Fix64.Zero, Fix64.One);
        }
    }
}