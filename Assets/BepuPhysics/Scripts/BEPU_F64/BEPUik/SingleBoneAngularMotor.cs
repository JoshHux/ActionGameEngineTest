using BEPUutilities;
using FixMath.NET;

namespace BEPUik
{
    public class SingleBoneAngularMotor : SingleBoneConstraint
    {
        /// <summary>
        /// Gets or sets the target orientation to apply to the target bone.
        /// </summary>
        public BepuQuaternion TargetOrientation;

        protected internal override void UpdateJacobiansAndVelocityBias()
        {
            linearJacobian = new Matrix3x3();
            angularJacobian = Matrix3x3.Identity;

            //Error is in world space. It gets projected onto the jacobians later.
            BepuQuaternion errorBepuQuaternion;
            BepuQuaternion.Conjugate(ref TargetBone.Orientation, out errorBepuQuaternion);
            BepuQuaternion.Multiply(ref TargetOrientation, ref errorBepuQuaternion, out errorBepuQuaternion);
            Fix64 angle;
            BepuVector3 angularError;
            BepuQuaternion.GetAxisAngleFromBepuQuaternion(ref errorBepuQuaternion, out angularError, out angle);
            BepuVector3.Multiply(ref angularError, angle, out angularError);

            //This is equivalent to projecting the error onto the angular jacobian. The angular jacobian just happens to be the identity matrix!
            BepuVector3.Multiply(ref angularError, errorCorrectionFactor, out velocityBias);
        }


    }
}
