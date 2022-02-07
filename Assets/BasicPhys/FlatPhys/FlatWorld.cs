using System.Collections.Generic;
using FixMath.NET;

namespace FlatPhysics
{
    public sealed class FlatWorld
    {
        public static readonly Fix64 MinBodySize = FixedMath.C0p01 * FixedMath.C0p01;
        public static readonly Fix64 MaxBodySize = 64 * 64;

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 128;

        private FVector2 gravity;
        private List<FlatBody> bodyList;

        public int BodyCount
        {
            get { return this.bodyList.Count; }
        }

        public FlatWorld()
        {
            this.gravity = new FVector2(0, 0);
            this.bodyList = new List<FlatBody>();
        }

        public void AddBody(FlatBody body)
        {
            this.bodyList.Add(body);
        }

        public bool RemoveBody(FlatBody body)
        {
            return this.bodyList.Remove(body);
        }

        public bool GetBody(int index, out FlatBody body)
        {
            body = null;

            if (index < 0 || index >= this.bodyList.Count)
            {
                return false;
            }

            body = this.bodyList[index];
            return true;
        }

        public void Step(Fix64 time, int iterations)
        {
            iterations = UnityEngine.Mathf.Clamp(iterations, FlatWorld.MinIterations, FlatWorld.MaxIterations);

            for (int it = 0; it < iterations; it++)
            {
                // Movement step
                for (int i = 0; i < this.bodyList.Count; i++)
                {
                    this.bodyList[i].Step(time, this.gravity, iterations);
                }

                // collision step
                for (int i = 0; i < this.bodyList.Count - 1; i++)
                {
                    FlatBody bodyA = this.bodyList[i];

                    for (int j = i + 1; j < this.bodyList.Count; j++)
                    {
                        FlatBody bodyB = this.bodyList[j];

                        if (bodyA.IsStatic && bodyB.IsStatic)
                        {
                            continue;
                        }

                        if (this.Collide(bodyA, bodyB, out FVector2 normal, out Fix64 depth))
                        {
                            if (!(bodyA.IsTrigger || bodyB.IsTrigger))
                            {
                                if (bodyA.IsStatic)
                                {
                                    bodyB.Move(normal * depth);
                                }
                                else if (bodyB.IsStatic)
                                {
                                    bodyA.Move(-normal * depth);
                                }
                                else
                                {
                                    bodyA.Move(-normal * depth / 2);
                                    bodyB.Move(normal * depth / 2);
                                }

                                this.ResolveCollision(bodyA, bodyB, normal, depth);
                            }
                        }
                    }
                }
            }
        }

        public void ResolveCollision(FlatBody bodyA, FlatBody bodyB, FVector2 normal, Fix64 depth)
        {
            FVector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (FVector2.Dot(relativeVelocity, normal) > 0)
            {
                return;
            }

            Fix64 e = Fix64.Min(bodyA.Restitution, bodyB.Restitution);

            Fix64 j = -(1 + e) * FVector2.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;

            FVector2 impulse = j * normal;

            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }

        public bool Collide(FlatBody bodyA, FlatBody bodyB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    bool result = Collisions.IntersectCirclePolygon(
                        bodyB.Position, bodyB.Radius,
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectCirclePolygon(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    return Collisions.IntersectCircles(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }

            return false;
        }
    }
}
