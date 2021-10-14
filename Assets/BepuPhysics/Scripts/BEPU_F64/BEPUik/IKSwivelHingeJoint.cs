using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    public class IKSwivelHingeJoint : IKJoint
    {
        /// <summary>
        /// Gets or sets the free hinge axis attached to connection A in its local space.
        /// </summary>
        public BepuVector3 LocalHingeAxis;
        /// <summary>
        /// Gets or sets the free twist axis attached to connection B in its local space.
        /// </summary>
        public BepuVector3 LocalTwistAxis;


        /// <summary>
        /// Gets or sets the free hinge axis attached to connection A in world space.
        /// </summary>
        public BepuVector3 WorldHingeAxis
        {
            get { return BepuQuaternion.Transform(LocalHingeAxis, ConnectionA.Orientation); }
            set
            {
                LocalHingeAxis = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionA.Orientation));
            }
        }

        /// <summary>
        /// Gets or sets the free twist axis attached to connection B in world space.
        /// </summary>
        public BepuVector3 WorldTwistAxis
        {
            get { return BepuQuaternion.Transform(LocalTwistAxis, ConnectionB.Orientation); }
            set
            {
                LocalTwistAxis = BepuQuaternion.Transform(value, BepuQuaternion.Conjugate(ConnectionB.Orientation));
            }
        }

        /// <summary>
        /// Constructs a new constraint which allows relative angular motion around a hinge axis and a twist axis.
        /// </summary>
        /// <param name="connectionA">First connection of the pair.</param>
        /// <param name="connectionB">Second connection of the pair.</param>
        /// <param name="worldHingeAxis">Hinge axis attached to connectionA.
        /// The connected bone will be able to rotate around this axis relative to each other.</param>
        /// <param name="worldTwistAxis">Twist axis attached to connectionB.
        /// The connected bones will be able to rotate around this axis relative to each other.</param>
        public IKSwivelHingeJoint(Bone connectionA, Bone connectionB, BepuVector3 worldHingeAxis, BepuVector3 worldTwistAxis)
            : base(connectionA, connectionB)
        {
            WorldHingeAxis = worldHingeAxis;
            WorldTwistAxis = worldTwistAxis;
        }

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobianA = linearJacobianB = new Matrix3x3();


            //There are two free axes and one restricted axis.
            //The constraint attempts to keep the hinge axis attached to connection A and the twist axis attached to connection B perpendicular to each other.
            //The restricted axis is the cross product between the twist and hinge axes.

            BepuVector3 worldTwistAxis, worldHingeAxis;
            BepuQuaternion.Transform(ref LocalHingeAxis, ref ConnectionA.Orientation, out worldHingeAxis);
            BepuQuaternion.Transform(ref LocalTwistAxis, ref ConnectionB.Orientation, out worldTwistAxis);

            BepuVector3 restrictedAxis;
            BepuVector3.Cross(ref worldHingeAxis, ref worldTwistAxis, out restrictedAxis);
            //Attempt to normalize the restricted axis.
            Fix64 lengthSquared = restrictedAxis.LengthSquared();
            if (lengthSquared > Toolbox.Epsilon)
            {
                BepuVector3.Divide(ref restrictedAxis, Fix64.Sqrt(lengthSquared), out restrictedAxis);
            }
            else
            {
                restrictedAxis = new BepuVector3();
            }


            angularJacobianA = new Matrix3x3
              {
                  M11 = restrictedAxis.X,
                  M12 = restrictedAxis.Y,
                  M13 = restrictedAxis.Z,
              };
            Matrix3x3.Negate(ref angularJacobianA, out angularJacobianB);

            Fix64 error;
            BepuVector3.Dot(ref worldHingeAxis, ref worldTwistAxis, out error);
            error = Fix64.Acos(MathHelper.Clamp(error, -1, F64.C1)) - MathHelper.PiOver2;

            velocityBias = new BepuVector3(errorCorrectionFactor * error, F64.C0, F64.C0);


        }
    }
}
