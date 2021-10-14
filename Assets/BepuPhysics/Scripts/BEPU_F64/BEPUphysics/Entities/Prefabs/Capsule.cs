using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.EntityStateManagement;
 
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Entities.Prefabs
{
    /// <summary>
    /// Pill-shaped object that can collide and move.  After making an entity, add it to a Space so that the engine can manage it.
    /// </summary>
    public class Capsule : Entity<ConvexCollidable<CapsuleShape>>
    {
        /// <summary>
        /// Gets or sets the length of the capsule.
        /// </summary>
        public Fix64 Length
        {
            get
            {
                return CollisionInformation.Shape.Length;
            }
            set
            {
                CollisionInformation.Shape.Length = value;
            }
        }

        /// <summary>
        /// Gets or sets the radius of the capsule.
        /// </summary>
        public Fix64 Radius
        {
            get
            {
                return CollisionInformation.Shape.Radius;
            }
            set
            {
                CollisionInformation.Shape.Radius = value;
            }
        }

        private Capsule(Fix64 len, Fix64 rad)
            : base(new ConvexCollidable<CapsuleShape>(new CapsuleShape(len, rad)))
        {
        }

        private Capsule(Fix64 len, Fix64 rad, Fix64 mass)
            : base(new ConvexCollidable<CapsuleShape>(new CapsuleShape(len, rad)), mass)
        {
        }



        ///<summary>
        /// Computes an orientation and length from a line segment.
        ///</summary>
        ///<param name="start">Starting point of the line segment.</param>
        ///<param name="end">Endpoint of the line segment.</param>
        ///<param name="orientation">Orientation of a line that fits the line segment.</param>
        ///<param name="length">Length of the line segment.</param>
        public static void GetCapsuleInformation(ref BepuVector3 start, ref BepuVector3 end, out BepuQuaternion orientation, out Fix64 length)
        {
            BepuVector3 segmentDirection;
            BepuVector3.Subtract(ref end, ref start, out segmentDirection);
            length = segmentDirection.Length();
            if (length > F64.C0)
            {
                BepuVector3.Divide(ref segmentDirection, length, out segmentDirection);
                BepuQuaternion.GetBepuQuaternionBetweenNormalizedVectors(ref Toolbox.UpVector, ref segmentDirection, out orientation);
            }
            else
                orientation = BepuQuaternion.Identity;
        }

        ///<summary>
        /// Constructs a new kinematic capsule.
        ///</summary>
        ///<param name="start">Line segment start point.</param>
        ///<param name="end">Line segment end point.</param>
        ///<param name="radius">Radius of the capsule to expand the line segment by.</param>
        public Capsule(BepuVector3 start, BepuVector3 end, Fix64 radius)
            : this((end - start).Length(), radius)
        {
            Fix64 length;
            BepuQuaternion orientation;
            GetCapsuleInformation(ref start, ref end, out orientation, out length);
            this.Orientation = orientation;
            BepuVector3 position;
            BepuVector3.Add(ref start, ref end, out position);
            BepuVector3.Multiply(ref position, F64.C0p5, out position);
            this.Position = position;
        }


        ///<summary>
        /// Constructs a new dynamic capsule.
        ///</summary>
        ///<param name="start">Line segment start point.</param>
        ///<param name="end">Line segment end point.</param>
        ///<param name="radius">Radius of the capsule to expand the line segment by.</param>
        /// <param name="mass">Mass of the entity.</param>
        public Capsule(BepuVector3 start, BepuVector3 end, Fix64 radius, Fix64 mass)
            : this((end - start).Length(), radius, mass)
        {
            Fix64 length;
            BepuQuaternion orientation;
            GetCapsuleInformation(ref start, ref end, out orientation, out length);
            this.Orientation = orientation;
            BepuVector3 position;
            BepuVector3.Add(ref start, ref end, out position);
            BepuVector3.Multiply(ref position, F64.C0p5, out position);
            this.Position = position;
        }

        /// <summary>
        /// Constructs a physically simulated capsule.
        /// </summary>
        /// <param name="position">Position of the capsule.</param>
        /// <param name="length">Length of the capsule.</param>
        /// <param name="radius">Radius of the capsule.</param>
        /// <param name="mass">Mass of the object.</param>
        public Capsule(BepuVector3 position, Fix64 length, Fix64 radius, Fix64 mass)
            : this(length, radius, mass)
        {
            Position = position;
        }

        /// <summary>
        /// Constructs a nondynamic capsule.
        /// </summary>
        /// <param name="position">Position of the capsule.</param>
        /// <param name="length">Length of the capsule.</param>
        /// <param name="radius">Radius of the capsule.</param>
        public Capsule(BepuVector3 position, Fix64 length, Fix64 radius)
            : this(length, radius)
        {
            Position = position;
        }

        /// <summary>
        /// Constructs a dynamic capsule.
        /// </summary>
        /// <param name="motionState">Motion state specifying the entity's initial state.</param>
        /// <param name="length">Length of the capsule.</param>
        /// <param name="radius">Radius of the capsule.</param>
        /// <param name="mass">Mass of the object.</param>
        public Capsule(MotionState motionState, Fix64 length, Fix64 radius, Fix64 mass)
            : this(length, radius, mass)
        {
            MotionState = motionState;
        }

        /// <summary>
        /// Constructs a nondynamic capsule.
        /// </summary>
        /// <param name="motionState">Motion state specifying the entity's initial state.</param>
        /// <param name="length">Length of the capsule.</param>
        /// <param name="radius">Radius of the capsule.</param>
        public Capsule(MotionState motionState, Fix64 length, Fix64 radius)
            : this(length, radius)
        {
            MotionState = motionState;
        }

    }
}