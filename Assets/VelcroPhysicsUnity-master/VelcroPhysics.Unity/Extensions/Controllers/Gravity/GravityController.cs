using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Extensions.Controllers.ControllerBase;

namespace VelcroPhysics.Extensions.Controllers.Gravity
{
    public class GravityController : Controller
    {
        public GravityController(Fix64 strength)
            : base(ControllerType.GravityController)
        {
            Strength = strength;
            MaxRadius = Fix64.MaxValue;
            GravityType = GravityType.DistanceSquared;
            Points = new List<FVector2>();
            Bodies = new List<Body>();
        }

        public GravityController(Fix64 strength, Fix64 maxRadius, Fix64 minRadius)
            : base(ControllerType.GravityController)
        {
            MinRadius = minRadius;
            MaxRadius = maxRadius;
            Strength = strength;
            GravityType = GravityType.DistanceSquared;
            Points = new List<FVector2>();
            Bodies = new List<Body>();
        }

        public Fix64 MinRadius { get; set; }
        public Fix64 MaxRadius { get; set; }
        public Fix64 Strength { get; set; }
        public GravityType GravityType { get; set; }
        public List<Body> Bodies { get; set; }
        public List<FVector2> Points { get; set; }

        public override void Update(Fix64 dt)
        {
            var f = FVector2.zero;

            foreach (var worldBody in World.BodyList)
            {
                if (!IsActiveOn(worldBody))
                    continue;

                foreach (var controllerBody in Bodies)
                {
                    if (worldBody == controllerBody || worldBody.IsStatic && controllerBody.IsStatic ||
                        !controllerBody.Enabled)
                        continue;

                    var d = controllerBody.Position - worldBody.Position;
                    var r2 = d.sqrMagnitude;

                    if (r2 <= Settings.Epsilon || r2 > MaxRadius * MaxRadius || r2 < MinRadius * MinRadius)
                        continue;

                    switch (GravityType)
                    {
                        case GravityType.DistanceSquared:
                            f = Strength / r2 * worldBody.Mass * controllerBody.Mass * d;
                            break;
                        case GravityType.Linear:
                            f = Strength / Fix64.Sqrt(r2) * worldBody.Mass * controllerBody.Mass * d;
                            break;
                    }

                    worldBody.ApplyForce(ref f);
                }

                foreach (var point in Points)
                {
                    var d = point - worldBody.Position;
                    var r2 = d.sqrMagnitude;

                    if (r2 <= Settings.Epsilon || r2 > MaxRadius * MaxRadius || r2 < MinRadius * MinRadius)
                        continue;

                    switch (GravityType)
                    {
                        case GravityType.DistanceSquared:
                            f = Strength / r2 * worldBody.Mass * d;
                            break;
                        case GravityType.Linear:
                            f = Strength / Fix64.Sqrt(r2) * worldBody.Mass * d;
                            break;
                    }

                    worldBody.ApplyForce(ref f);
                }
            }
        }

        public void AddBody(Body body)
        {
            Bodies.Add(body);
        }

        public void AddPoint(FVector2 point)
        {
            Points.Add(point);
        }
    }
}