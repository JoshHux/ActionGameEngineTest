using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    public class IKRevoluteJoint : IKJoint
    {
        private BepuVector3 localFreeAxisA;
        /// <summary>
        /// Gets or sets the free axis in connection A's local space.
        /// Must be unit length.
        /// </summary>
        public BepuVector3 LocalFreeAxisA
        {
            get { return localFreeAxisA; }
            set
            {
                localFreeAxisA = value;
                ComputeConstrainedAxes();
            }
        }

        private BepuVector3 localFreeAxisB;
        /// <summary>
        /// Gets or sets the free axis in connection B's local space.
        /// Must be unit length.
        /// </summary>
        public BepuVector3 LocalFreeAxisB
        {
            get { return localFreeAxisB; }
            set
            {
                localFreeAxisB = value;
                ComputeConstrainedAxes();
            }
        }



        /// <summary>
        /// Gets or sets the free axis attached to connection A in world space.
        /// This does not change the other connection's free axis.
        /// </summary>
        public BepuVector3 WorldFreeAxisA
        {
            get { return BepuQuaternion.Transform(localFreeAxisA, ConnectionA.Orientation); }
            set
            {
                LocalFreeAxisA = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionA.Orientation));
            }
        }

        /// <summary>
        /// Gets or sets the free axis attached to connection B in world space.
        /// This does not change the other connection's free axis.
        /// </summary>
        public BepuVector3 WorldFreeAxisB
        {
            get { return BepuQuaternion.Transform(localFreeAxisB, ConnectionB.Orientation); }
            set
            {
                LocalFreeAxisB = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionB.Orientation));
            }
        }

        private BepuVector3 localConstrainedAxis1, localConstrainedAxis2;
        void ComputeConstrainedAxes()
        {
            BepuVector3 worldAxisA = WorldFreeAxisA;
            BepuVector3 error = BepuVector3.Cross(worldAxisA, WorldFreeAxisB);
            Fix64 lengthSquared = error.LengthSquared();
            BepuVector3 worldConstrainedAxis1, worldConstrainedAxis2;
            //Find the first constrained axis.
            if (lengthSquared > Toolbox.Epsilon)
            {
                //The error direction can be used as the first axis!
                BepuVector3.Divide(ref error, Fix64.Sqrt(lengthSquared), out worldConstrainedAxis1);
            }
            else
            {
                //There's not enough error for it to be a good constrained axis.
                //We'll need to create the constrained axes arbitrarily.
                BepuVector3.Cross(ref Toolbox.UpVector, ref worldAxisA, out worldConstrainedAxis1);
                lengthSquared = worldConstrainedAxis1.LengthSquared();
                if (lengthSquared > Toolbox.Epsilon)
                {
                    //The up vector worked!
                    BepuVector3.Divide(ref worldConstrainedAxis1, Fix64.Sqrt(lengthSquared), out worldConstrainedAxis1);
                }
                else
                {
                    //The up vector didn't work. Just try the right vector.
                    BepuVector3.Cross(ref Toolbox.RightVector, ref worldAxisA, out worldConstrainedAxis1);
                    worldConstrainedAxis1.Normalize();
                }
            }
            //Don't have to normalize the second constraint axis; it's the cross product of two perpendicular normalized vectors.
            BepuVector3.Cross(ref worldAxisA, ref worldConstrainedAxis1, out worldConstrainedAxis2);

            localConstrainedAxis1 = BepuQuaternion.Transform(worldConstrainedAxis1, BepuQuaternion.Conjugate(ConnectionA.Orientation));
            localConstrainedAxis2 = BepuQuaternion.Transform(worldConstrainedAxis2, BepuQuaternion.Conjugate(ConnectionA.Orientation));
        }

        /// <summary>
        /// Constructs a new orientation joint.
        /// Orientation joints can be used to simulate the angular portion of a hinge.
        /// Orientation joints allow rotation around only a single axis.
        /// </summary>
        /// <param name="connectionA">First entity connected in the orientation joint.</param>
        /// <param name="connectionB">Second entity connected in the orientation joint.</param>
        /// <param name="freeAxis">Axis allowed to rotate freely in world space.</param>
        public IKRevoluteJoint(Bone connectionA, Bone connectionB, BepuVector3 freeAxis)
            : base(connectionA, connectionB)
        {
            WorldFreeAxisA = freeAxis;
            WorldFreeAxisB = freeAxis;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobianA = linearJacobianB = new Matrix3x3();

            //We know the one free axis. We need the two restricted axes. This amounts to completing the orthonormal basis.
            //We can grab one of the restricted axes using a cross product of the two world axes. This is not guaranteed
            //to be nonzero, so the normalization requires protection.

            BepuVector3 worldAxisA, worldAxisB;
            BepuQuaternion.Transform(ref localFreeAxisA, ref ConnectionA.Orientation, out worldAxisA);
            BepuQuaternion.Transform(ref localFreeAxisB, ref ConnectionB.Orientation, out worldAxisB);

            BepuVector3 error;
            BepuVector3.Cross(ref worldAxisA, ref worldAxisB, out error);

            BepuVector3 worldConstrainedAxis1, worldConstrainedAxis2;
            BepuQuaternion.Transform(ref localConstrainedAxis1, ref ConnectionA.Orientation, out worldConstrainedAxis1);
            BepuQuaternion.Transform(ref localConstrainedAxis2, ref ConnectionA.Orientation, out worldConstrainedAxis2);


            angularJacobianA = new Matrix3x3
            {
                M11 = worldConstrainedAxis1.X,
                M12 = worldConstrainedAxis1.Y,
                M13 = worldConstrainedAxis1.Z,
                M21 = worldConstrainedAxis2.X,
                M22 = worldConstrainedAxis2.Y,
                M23 = worldConstrainedAxis2.Z
            };
            Matrix3x3.Negate(ref angularJacobianA, out angularJacobianB);


            BepuVector2 constraintSpaceError;
            BepuVector3.Dot(ref error, ref worldConstrainedAxis1, out constraintSpaceError.X);
            BepuVector3.Dot(ref error, ref worldConstrainedAxis2, out constraintSpaceError.Y);
            velocityBias.X = errorCorrectionFactor * constraintSpaceError.X;
            velocityBias.Y = errorCorrectionFactor * constraintSpaceError.Y;


        }
    }
}
