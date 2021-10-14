using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Paths
{
    /// <summary>
    /// Wrapper around an orientation curve that specifies a specific velocity at which to travel.
    /// </summary>
    public class ConstantAngularSpeedCurve : ConstantSpeedCurve<BepuQuaternion>
    {
        /// <summary>
        /// Constructs a new constant speed curve.
        /// </summary>
        /// <param name="speed">Speed to maintain while traveling around a curve.</param>
        /// <param name="curve">Curve to wrap.</param>
        public ConstantAngularSpeedCurve(Fix64 speed, Curve<BepuQuaternion> curve)
            : base(speed, curve)
        {
        }

        /// <summary>
        /// Constructs a new constant speed curve.
        /// </summary>
        /// <param name="speed">Speed to maintain while traveling around a curve.</param>
        /// <param name="curve">Curve to wrap.</param>
        /// <param name="sampleCount">Number of samples to use when constructing the wrapper curve.
        /// More samples increases the accuracy of the speed requirement at the cost of performance.</param>
        public ConstantAngularSpeedCurve(Fix64 speed, Curve<BepuQuaternion> curve, int sampleCount)
            : base(speed, curve, sampleCount)
        {
        }

        protected override Fix64 GetDistance(BepuQuaternion start, BepuQuaternion end)
        {
            BepuQuaternion.Conjugate(ref end, out end);
            BepuQuaternion.Multiply(ref end, ref start, out end);
            return BepuQuaternion.GetAngleFromBepuQuaternion(ref end);
        }
    }
}