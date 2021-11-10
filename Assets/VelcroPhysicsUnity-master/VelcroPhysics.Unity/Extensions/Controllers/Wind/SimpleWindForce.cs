using UnityEngine;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Utilities;
using FixMath.NET;

namespace VelcroPhysics.Extensions.Controllers.Wind
{
    /// <summary>
    /// Reference implementation for forces based on AbstractForceController
    /// It supports all features provided by the base class and illustrates proper
    /// usage as an easy to understand example.
    /// As a side-effect it is a nice and easy to use wind force for your projects
    /// </summary>
    public class SimpleWindForce : AbstractForceController
    {
        /// <summary>
        /// Direction of the windforce
        /// </summary>
        public FVector2 Direction { get; set; }

        /// <summary>
        /// The amount of Direction randomization. Allowed range is 0-1.
        /// </summary>
        public Fix64 Divergence { get; set; }

        /// <summary>
        /// Ignore the position and apply the force. If off only in the "front" (relative to position and direction)
        /// will be affected
        /// </summary>
        public bool IgnorePosition { get; set; }

        public override void ApplyForce(Fix64 dt, Fix64 strength)
        {
            foreach (var body in World.BodyList)
            {
                //TODO: Consider Force Type
                var decayMultiplier = GetDecayMultiplier(body);

                if (decayMultiplier != 0)
                {
                    FVector2 forceVector;

                    if (ForceType == ForceTypes.Point)
                    {
                        forceVector = body.Position - Position;
                    }
                    else
                    {
                        Direction.Normalize();

                        forceVector = Direction;

                        if (forceVector.magnitude == 0)
                            forceVector = new FVector2(0, 1);
                    }

                    //TODO: Consider Divergence:
                    //forceVector = FVector2.VTransform(forceVector, Matrix.CreateRotationZ((Fix64.PI - Fix64.PI/2) * (Fix64)Randomize.NextDouble()));

                    // Calculate random Variation
                    if (Variation != 0)
                    {
                        var strengthVariation = Random.value * MathUtils.Clamp(Variation, 0, 1);
                        forceVector.Normalize();
                        body.ApplyForce(forceVector * strength * decayMultiplier * strengthVariation);
                    }
                    else
                    {
                        forceVector.Normalize();
                        body.ApplyForce(forceVector * strength * decayMultiplier);
                    }
                }
            }
        }
    }
}