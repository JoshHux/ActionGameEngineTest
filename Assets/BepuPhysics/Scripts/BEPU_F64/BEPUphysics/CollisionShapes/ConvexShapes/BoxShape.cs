using System;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.CollisionShapes.ConvexShapes
{
    ///<summary>
    /// Convex shape with width, length, and height.
    ///</summary>
    public class BoxShape : ConvexShape
    {
        internal Fix64 halfWidth;
        internal Fix64 halfHeight;
        internal Fix64 halfLength;


        /// <summary>
        /// Width of the box divided by two.
        /// </summary>
        public Fix64 HalfWidth
        {
            get { return halfWidth; }
            set { halfWidth = value; OnShapeChanged(); }
        }

        /// <summary>
        /// Height of the box divided by two.
        /// </summary>
        public Fix64 HalfHeight
        {
            get { return halfHeight; }
            set { halfHeight = value; OnShapeChanged(); }
        }

        /// <summary>
        /// Length of the box divided by two.
        /// </summary>
        public Fix64 HalfLength
        {
            get { return halfLength; }
            set { halfLength = value; OnShapeChanged(); }
        }

        /// <summary>
        /// Width of the box.
        /// </summary>
        public Fix64 Width
        {
            get { return halfWidth * F64.C2; }
            set { halfWidth = value * F64.C0p5; OnShapeChanged(); }
        }

        /// <summary>
        /// Height of the box.
        /// </summary>
        public Fix64 Height
        {
            get { return halfHeight * F64.C2; }
            set { halfHeight = value * F64.C0p5; OnShapeChanged(); }
        }

        /// <summary>
        /// Length of the box.
        /// </summary>
        public Fix64 Length
        {
            get { return halfLength * F64.C2; }
            set { halfLength = value * F64.C0p5; OnShapeChanged(); }
        }


        ///<summary>
        /// Constructs a new box shape.
        ///</summary>
        ///<param name="width">Width of the box.</param>
        ///<param name="height">Height of the box.</param>
        ///<param name="length">Length of the box.</param>
        public BoxShape(Fix64 width, Fix64 height, Fix64 length)
        {
            halfWidth = width * F64.C0p5;
            halfHeight = height * F64.C0p5;
            halfLength = length * F64.C0p5;

            UpdateConvexShapeInfo(ComputeDescription(width, height, length, collisionMargin));
        }

        ///<summary>
        /// Constructs a new box shape from cached information.
        ///</summary>
        ///<param name="width">Width of the box.</param>
        ///<param name="height">Height of the box.</param>
        ///<param name="length">Length of the box.</param>
        /// <param name="description">Cached information about the shape. Assumed to be correct; no extra processing or validation is performed.</param>
        public BoxShape(Fix64 width, Fix64 height, Fix64 length, ConvexShapeDescription description)
        {
            halfWidth = width * F64.C0p5;
            halfHeight = height * F64.C0p5;
            halfLength = length * F64.C0p5;

            UpdateConvexShapeInfo(description);
        }

        protected override void OnShapeChanged()
        {
            UpdateConvexShapeInfo(ComputeDescription(halfWidth, halfHeight, halfLength, collisionMargin));
            base.OnShapeChanged();
        }

        /// <summary>
        /// Computes a convex shape description for a BoxShape.
        /// </summary>
        ///<param name="width">Width of the box.</param>
        ///<param name="height">Height of the box.</param>
        ///<param name="length">Length of the box.</param>
        /// <param name="collisionMargin">Collision margin of the shape.</param>
        /// <returns>Description required to define a convex shape.</returns>
        public static ConvexShapeDescription ComputeDescription(Fix64 width, Fix64 height, Fix64 length, Fix64 collisionMargin)
        {
            ConvexShapeDescription description;
            description.EntityShapeVolume.Volume = width * height * length;

            Fix64 widthSquared = width * width;
            Fix64 heightSquared = height * height;
            Fix64 lengthSquared = length * length;
            Fix64 inv12 = F64.OneTwelfth;

            description.EntityShapeVolume.VolumeDistribution = new Matrix3x3();
            description.EntityShapeVolume.VolumeDistribution.M11 = (heightSquared + lengthSquared) * inv12;
            description.EntityShapeVolume.VolumeDistribution.M22 = (widthSquared + lengthSquared) * inv12;
            description.EntityShapeVolume.VolumeDistribution.M33 = (widthSquared + heightSquared) * inv12;

            description.MaximumRadius = F64.C0p5 * Fix64.Sqrt(width * width + height * height + length * length);
            description.MinimumRadius = F64.C0p5 * MathHelper.Min(width, MathHelper.Min(height, length));

            description.CollisionMargin = collisionMargin;
            return description;
        }





        /// <summary>
        /// Gets the bounding box of the shape given a transform.
        /// </summary>
        /// <param name="shapeTransform">Transform to use.</param>
        /// <param name="boundingBox">Bounding box of the transformed shape.</param>
        public override void GetBoundingBox(ref RigidTransform shapeTransform, out BoundingBox boundingBox)
        {
#if !WINDOWS
            boundingBox = new BoundingBox();
#endif

            Matrix3x3 o;
            Matrix3x3.CreateFromBepuQuaternion(ref shapeTransform.Orientation, out o);
            //Sample the local directions from the orientation matrix, implicitly transposed.
            //Notice only three directions are used.  Due to box symmetry, 'left' is just -right.
            var right = new BepuVector3(Fix64.Sign(o.M11) * halfWidth, Fix64.Sign(o.M21) * halfHeight, Fix64.Sign(o.M31) * halfLength);

            var up = new BepuVector3(Fix64.Sign(o.M12) * halfWidth, Fix64.Sign(o.M22) * halfHeight, Fix64.Sign(o.M32) * halfLength);

            var backward = new BepuVector3(Fix64.Sign(o.M13) * halfWidth, Fix64.Sign(o.M23) * halfHeight, Fix64.Sign(o.M33) * halfLength);


            //Rather than transforming each axis independently (and doing three times as many operations as required), just get the 3 required values directly.
            BepuVector3 offset;
            TransformLocalExtremePoints(ref right, ref up, ref backward, ref o, out offset);

            //The positive and negative vectors represent the X, Y and Z coordinates of the extreme points in world space along the world space axes.
            BepuVector3.Add(ref shapeTransform.Position, ref offset, out boundingBox.Max);
            BepuVector3.Subtract(ref shapeTransform.Position, ref offset, out boundingBox.Min);

        }


        ///<summary>
        /// Gets the extreme point of the shape in local space in a given direction.
        ///</summary>
        ///<param name="direction">Direction to find the extreme point in.</param>
        ///<param name="extremePoint">Extreme point on the shape.</param>
        public override void GetLocalExtremePointWithoutMargin(ref BepuVector3 direction, out BepuVector3 extremePoint)
        {
            extremePoint = new BepuVector3(Fix64.Sign(direction.X) * (halfWidth - collisionMargin), Fix64.Sign(direction.Y) * (halfHeight - collisionMargin), Fix64.Sign(direction.Z) * (halfLength - collisionMargin));
        }




        /// <summary>
        /// Gets the intersection between the box and the ray.
        /// </summary>
        /// <param name="ray">Ray to test against the box.</param>
        /// <param name="transform">Transform of the shape.</param>
        /// <param name="maximumLength">Maximum distance to travel in units of the direction vector's length.</param>
        /// <param name="hit">Hit data for the raycast, if any.</param>
        /// <returns>Whether or not the ray hit the target.</returns>
        public override bool RayTest(ref Ray ray, ref RigidTransform transform, Fix64 maximumLength, out RayHit hit)
        {

            BepuVector3.Subtract(ref ray.Position, ref transform.Position, out var offset);
            Matrix3x3.CreateFromBepuQuaternion(ref transform.Orientation, out var orientation);
            Matrix3x3.TransformTranspose(ref offset, ref orientation, out var localOffset);
            Matrix3x3.TransformTranspose(ref ray.Direction, ref orientation, out var localDirection);
            //Note that this division has two odd properties:
            //1) If the local direction has a near zero component, it is clamped to a nonzero but extremely small value. This is a hack, but it works reasonably well.
            //The idea is that any interval computed using such an inverse would be enormous. Those values will not be exactly accurate, but they will never appear as a result
            //because a parallel ray will never actually intersect the surface. The resulting intervals are practical approximations of the 'true' infinite intervals.
            //2) To compensate for the clamp and abs, we reintroduce the sign in the numerator. Note that it has the reverse sign since it will be applied to the offset to get the T value.
            var offsetToTScale = new BepuVector3(
                (localDirection.X < 0 ? 1 : -1) / Fix64.Max((Fix64)1e-15, Fix64.Abs(localDirection.X)),
                (localDirection.Y < 0 ? 1 : -1) / Fix64.Max((Fix64)1e-15, Fix64.Abs(localDirection.Y)),
                (localDirection.Z < 0 ? 1 : -1) / Fix64.Max((Fix64)1e-15, Fix64.Abs(localDirection.Z)));

            //Compute impact times for each pair of planes in local space.
            var halfExtent = new BepuVector3(HalfWidth, HalfHeight, HalfLength);
            BepuVector3.Subtract(ref localOffset, ref halfExtent, out var negativeTNumerator);
            BepuVector3.Add(ref localOffset, ref halfExtent, out var positiveTNumerator);
            BepuVector3.Multiply(ref negativeTNumerator, ref offsetToTScale, out var negativeT);
            BepuVector3.Multiply(ref positiveTNumerator, ref offsetToTScale, out var positiveT);
            BepuVector3.Min(ref negativeT, ref positiveT, out var entryT);
            BepuVector3.Max(ref negativeT, ref positiveT, out var exitT);

            //In order for an impact to occur, the ray must enter all three slabs formed by the axis planes before exiting any of them.
            //In other words, the first exit must occur after the last entry.
            var earliestExit = exitT.X < exitT.Y ? exitT.X : exitT.Y;
            if (exitT.Z < earliestExit)
                earliestExit = exitT.Z;
            if (earliestExit > maximumLength)
                earliestExit = maximumLength;
            //The interval of ray-box intersection goes from latestEntry to earliestExit. If earliestExit is negative, then the ray is pointing away from the box.
            if (earliestExit < 0)
            {
                hit = default;
                return false;
            }
            Fix64 latestEntry;
            if (entryT.X > entryT.Y)
            {
                if (entryT.X > entryT.Z)
                {
                    latestEntry = entryT.X;
                    hit.Normal = new BepuVector3(orientation.M11, orientation.M12, orientation.M13);
                }
                else
                {
                    latestEntry = entryT.Z;
                    hit.Normal = new BepuVector3(orientation.M31, orientation.M32, orientation.M33);
                }
            }
            else
            {
                if (entryT.Y > entryT.Z)
                {
                    latestEntry = entryT.Y;
                    hit.Normal = new BepuVector3(orientation.M21, orientation.M22, orientation.M23);
                }
                else
                {
                    latestEntry = entryT.Z;
                    hit.Normal = new BepuVector3(orientation.M31, orientation.M32, orientation.M33);
                }
            }

            if (earliestExit < latestEntry)
            {
                //At no point is the ray in all three slabs at once.
                hit = default;
                return false;
            }
            hit.T = latestEntry < 0 ? 0 : latestEntry;
            //The normal should point away from the center of the box.
            if (BepuVector3.Dot(hit.Normal, offset) < 0)
            {
                BepuVector3.Negate(ref hit.Normal, out hit.Normal);
            }
            BepuVector3.Multiply(ref ray.Direction, hit.T, out var offsetFromOrigin);
            BepuVector3.Add(ref ray.Position, ref offsetFromOrigin, out hit.Location);
            return true;
        }

        /// <summary>
        /// Retrieves an instance of an EntityCollidable that uses this EntityShape.  Mainly used by compound bodies.
        /// </summary>
        /// <returns>EntityCollidable that uses this shape.</returns>
        public override EntityCollidable GetCollidableInstance()
        {
            return new ConvexCollidable<BoxShape>(this);
        }

    }
}
