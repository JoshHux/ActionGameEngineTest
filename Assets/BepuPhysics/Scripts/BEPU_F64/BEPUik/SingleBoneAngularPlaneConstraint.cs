using System;
using BEPUutilities;

namespace BEPUik
{
    public class SingleBoneAngularPlaneConstraint : SingleBoneConstraint
    {
        /// <summary>
        /// Gets or sets normal of the plane which the bone's axis will be constrained to..
        /// </summary>
        public BepuVector3 PlaneNormal;



        /// <summary>
        /// Axis to constrain to the plane in the bone's local space.
        /// </summary>
        public BepuVector3 BoneLocalAxis;

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
 

            linearJacobian = new Matrix3x3();

            BepuVector3 boneAxis;
            BepuQuaternion.Transform(ref BoneLocalAxis, ref TargetBone.Orientation, out boneAxis);

            BepuVector3 jacobian;
            BepuVector3.Cross(ref boneAxis, ref PlaneNormal, out jacobian);

            angularJacobian = new Matrix3x3
            {
                M11 = jacobian.X,
                M12 = jacobian.Y,
                M13 = jacobian.Z,
            };


            BepuVector3.Dot(ref boneAxis, ref PlaneNormal, out velocityBias.X);
            velocityBias.X = -errorCorrectionFactor * velocityBias.X;


        }


    }
}
