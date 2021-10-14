using System;
using BEPUutilities;

namespace BEPUik
{
    public class SingleBoneRevoluteConstraint : SingleBoneConstraint
    {
        private BepuVector3 freeAxis;
        private BepuVector3 constrainedAxis1;
        private BepuVector3 constrainedAxis2;

        /// <summary>
        /// Gets or sets the direction to constrain the bone free axis to.
        /// </summary>
        public BepuVector3 FreeAxis
        {
            get { return freeAxis; }
            set
            {
                freeAxis = value;
                constrainedAxis1 = BepuVector3.Cross(freeAxis, BepuVector3.Up);
                if (constrainedAxis1.LengthSquared() < Toolbox.Epsilon)
                {
                    constrainedAxis1 = BepuVector3.Cross(freeAxis, BepuVector3.Right);
                }
                constrainedAxis1.Normalize();
                constrainedAxis2 = BepuVector3.Cross(freeAxis, constrainedAxis1);
            }
        }


        /// <summary>
        /// Axis of allowed rotation in the bone's local space.
        /// </summary>
        public BepuVector3 BoneLocalFreeAxis;

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
 

            linearJacobian = new Matrix3x3();

            BepuVector3 boneAxis;
            BepuQuaternion.Transform(ref BoneLocalFreeAxis, ref TargetBone.Orientation, out boneAxis);


            angularJacobian = new Matrix3x3
            {
                M11 = constrainedAxis1.X,
                M12 = constrainedAxis1.Y,
                M13 = constrainedAxis1.Z,
                M21 = constrainedAxis2.X,
                M22 = constrainedAxis2.Y,
                M23 = constrainedAxis2.Z
            };


            BepuVector3 error;
            BepuVector3.Cross(ref boneAxis, ref freeAxis, out error);
            BepuVector2 constraintSpaceError;
            BepuVector3.Dot(ref error, ref constrainedAxis1, out constraintSpaceError.X);
            BepuVector3.Dot(ref error, ref constrainedAxis2, out constraintSpaceError.Y);
            velocityBias.X = errorCorrectionFactor * constraintSpaceError.X;
            velocityBias.Y = errorCorrectionFactor * constraintSpaceError.Y;


        }


    }
}
