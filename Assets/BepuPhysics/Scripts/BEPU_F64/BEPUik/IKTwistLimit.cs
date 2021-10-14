using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    /// <summary>
    /// Prevents two bones from twisting beyond a certain angle away from each other as measured by the twist between two measurement axes.
    /// </summary>
    public class IKTwistLimit : IKLimit
    {
        /// <summary>
        /// Gets or sets the axis attached to ConnectionA in its local space.
        /// Must be unit length and perpendicular to LocalMeasurementAxisA.
        /// </summary>
        public BepuVector3 LocalAxisA;
        /// <summary>
        /// Gets or sets the axis attached to ConnectionB in its local space.
        /// Must be unit length and perpendicular to LocalMeasurementAxisB.
        /// </summary>
        public BepuVector3 LocalAxisB;

        /// <summary>
        /// Gets or sets the measurement axis attached to connection A.
        /// Must be unit length and perpendicular to LocalAxisA.
        /// </summary>
        public BepuVector3 LocalMeasurementAxisA;
        /// <summary>
        /// Gets or sets the measurement axis attached to connection B.
        /// Must be unit length and perpendicular to LocalAxisB.
        /// </summary>
        public BepuVector3 LocalMeasurementAxisB;

        /// <summary>
        /// Gets or sets the axis attached to ConnectionA in world space.
        /// Must be unit length and perpendicular to MeasurementAxisA.
        /// </summary>
        public BepuVector3 AxisA
        {
            get { return BepuQuaternion.Transform(LocalAxisA, ConnectionA.Orientation); }
            set { LocalAxisA = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the axis attached to ConnectionB in world space.
        /// Must be unit length and perpendicular to MeasurementAxisB.
        /// </summary>
        public BepuVector3 AxisB
        {
            get { return BepuQuaternion.Transform(LocalAxisB, ConnectionB.Orientation); }
            set { LocalAxisB = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the measurement axis attached to ConnectionA in world space.
        /// This axis is compared against the other connection's measurement axis to determine the twist.
        /// Must be unit length and perpendicular to AxisA.
        /// </summary>
        public BepuVector3 MeasurementAxisA
        {
            get { return BepuQuaternion.Transform(LocalMeasurementAxisA, ConnectionA.Orientation); }
            set { LocalMeasurementAxisA = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionA.Orientation)); }
        }

        /// <summary>
        /// Gets or sets the measurement axis attached to ConnectionB in world space.
        /// This axis is compared against the other connection's measurement axis to determine the twist.
        /// Must be unit length and perpendicular to AxisB.
        /// </summary>
        public BepuVector3 MeasurementAxisB
        {
            get { return BepuQuaternion.Transform(LocalMeasurementAxisB, ConnectionB.Orientation); }
            set { LocalMeasurementAxisB = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionB.Orientation)); }
        }

        private Fix64 maximumAngle;
        /// <summary>
        /// Gets or sets the maximum angle between the two axes allowed by the constraint.
        /// </summary>
        public Fix64 MaximumAngle
        {
            get { return maximumAngle; }
            set { maximumAngle = MathHelper.Max(F64.C0, value); }
        }

        /// <summary>
        /// Automatically computes the measurement axes for the current local axes.
        /// The current relative state of the entities will be considered 0 twist angle.
        /// </summary>
        public void ComputeMeasurementAxes()
        {
            BepuVector3 axisA, axisB;
            BepuQuaternion.Transform(ref LocalAxisA, ref ConnectionA.Orientation, out axisA);
            BepuQuaternion.Transform(ref LocalAxisB, ref ConnectionB.Orientation, out axisB);
            //Pick an axis perpendicular to axisA to use as the measurement axis.
            BepuVector3 worldMeasurementAxisA;
            BepuVector3.Cross(ref Toolbox.UpVector, ref axisA, out worldMeasurementAxisA);
            Fix64 lengthSquared = worldMeasurementAxisA.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                BepuVector3.Divide(ref worldMeasurementAxisA, Fix64.Sqrt(lengthSquared), out worldMeasurementAxisA);
            }
            else
            {
                //Oops! It was parallel to the up vector. Just try again with the right vector.
                BepuVector3.Cross(ref Toolbox.RightVector, ref axisA, out worldMeasurementAxisA);
                worldMeasurementAxisA.Normalize();
            }
            //Attach the measurement axis to entity B.
            //'Push' A's axis onto B by taking into account the swing transform.
            BepuQuaternion alignmentRotation;
            BepuQuaternion.GetBepuQuaternionBetweenNormalizedVectors(ref axisA, ref axisB, out alignmentRotation);
            BepuVector3 worldMeasurementAxisB;
            BepuQuaternion.Transform(ref worldMeasurementAxisA, ref alignmentRotation, out worldMeasurementAxisB);
            //Plop them on!
            MeasurementAxisA = worldMeasurementAxisA;
            MeasurementAxisB = worldMeasurementAxisB;

        }


        /// <summary>
        /// Builds a new twist limit. Prevents two bones from rotating beyond a certain angle away from each other as measured by attaching an axis to each connected bone.
        /// </summary>
        /// <param name="connectionA">First connection of the limit.</param>
        /// <param name="connectionB">Second connection of the limit.</param>
        /// <param name="axisA">Axis attached to connectionA in world space.</param>
        /// <param name="axisB">Axis attached to connectionB in world space.</param>
        /// <param name="maximumAngle">Maximum angle allowed between connectionA's axis and connectionB's axis.</param>
        public IKTwistLimit(Bone connectionA, Bone connectionB, BepuVector3 axisA, BepuVector3 axisB, Fix64 maximumAngle)
            : base(connectionA, connectionB)
        {
            AxisA = axisA;
            AxisB = axisB;
            MaximumAngle = maximumAngle;

            ComputeMeasurementAxes();
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {

            //This constraint doesn't consider linear motion.
            linearJacobianA = linearJacobianB = new Matrix3x3();

            //Compute the world axes.
            BepuVector3 axisA, axisB;
            BepuQuaternion.Transform(ref LocalAxisA, ref ConnectionA.Orientation, out axisA);
            BepuQuaternion.Transform(ref LocalAxisB, ref ConnectionB.Orientation, out axisB);

            BepuVector3 twistMeasureAxisA, twistMeasureAxisB;
            BepuQuaternion.Transform(ref LocalMeasurementAxisA, ref ConnectionA.Orientation, out twistMeasureAxisA);
            BepuQuaternion.Transform(ref LocalMeasurementAxisB, ref ConnectionB.Orientation, out twistMeasureAxisB);

            //Compute the shortest rotation to bring axisB into alignment with axisA.
            BepuQuaternion alignmentRotation;
            BepuQuaternion.GetBepuQuaternionBetweenNormalizedVectors(ref axisB, ref axisA, out alignmentRotation);

            //Transform the measurement axis on B by the alignment BepuQuaternion.
            BepuQuaternion.Transform(ref twistMeasureAxisB, ref alignmentRotation, out twistMeasureAxisB);

            //We can now compare the angle between the twist axes.
            Fix64 angle;
            BepuVector3.Dot(ref twistMeasureAxisA, ref twistMeasureAxisB, out angle);
            angle = Fix64.Acos(MathHelper.Clamp(angle, -1, F64.C1));

            //Compute the bias based upon the error.
            if (angle > maximumAngle)
                velocityBias = new BepuVector3(errorCorrectionFactor * (angle - maximumAngle), F64.C0, F64.C0);
            else //If the constraint isn't violated, set up the velocity bias to allow a 'speculative' limit.
                velocityBias = new BepuVector3(angle - maximumAngle, F64.C0, F64.C0);

            //We can't just use the axes directly as jacobians. Consider 'cranking' one object around the other.
            BepuVector3 jacobian;
            BepuVector3.Add(ref axisA, ref axisB, out jacobian);
            Fix64 lengthSquared = jacobian.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                BepuVector3.Divide(ref jacobian, Fix64.Sqrt(lengthSquared), out jacobian);
            }
            else
            {
                //The constraint is in an invalid configuration. Just ignore it.
                jacobian = new BepuVector3();
            }

            //In addition to the absolute angle value, we need to know which side of the limit we're hitting.
            //The jacobian will be negated on one side. This is because limits can only 'push' in one direction;
            //if we didn't flip the direction of the jacobian, it would be trying to push the same direction on both ends of the limit.
            //One side would end up doing nothing!
            BepuVector3 cross;
            BepuVector3.Cross(ref twistMeasureAxisA, ref twistMeasureAxisB, out cross);
            Fix64 limitSide;
            BepuVector3.Dot(ref cross, ref axisA, out limitSide);
            //Negate the jacobian based on what side of the limit we're on.
            if (limitSide < F64.C0)
                BepuVector3.Negate(ref jacobian, out jacobian);

            angularJacobianA = new Matrix3x3 { M11 = jacobian.X, M12 = jacobian.Y, M13 = jacobian.Z };
            angularJacobianB = new Matrix3x3 { M11 = -jacobian.X, M12 = -jacobian.Y, M13 = -jacobian.Z };




        }
    }
}
