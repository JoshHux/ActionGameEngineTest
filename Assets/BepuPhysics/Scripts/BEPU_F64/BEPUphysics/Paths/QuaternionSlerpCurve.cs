﻿

using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Paths
{
    /// <summary>
    /// Defines a BepuQuaternion curve using spherical linear interpolation.
    /// </summary>
    public class BepuQuaternionSlerpCurve : Curve<BepuQuaternion>
    {
        /// <summary>
        /// Evaluates the curve section starting at the control point index using
        /// the weight value.
        /// </summary>
        /// <param name="controlPointIndex">Index of the starting control point of the subinterval.</param>
        /// <param name="weight">Location to evaluate on the subinterval from 0 to 1.</param>
        /// <param name="value">Value at the given location.</param>
        public override void Evaluate(int controlPointIndex, Fix64 weight, out BepuQuaternion value)
        {
            value = BepuQuaternion.Slerp(ControlPoints[controlPointIndex].Value, ControlPoints[controlPointIndex + 1].Value, weight);
        }

        /// <summary>
        /// Gets the curve's bounding index information.
        /// </summary>
        /// <param name="minIndex">Index of the minimum control point in the active curve segment.</param>
        /// <param name="maxIndex">Index of the maximum control point in the active curve segment.</param>
        public override void GetCurveIndexBoundsInformation(out int minIndex, out int maxIndex)
        {
            maxIndex = ControlPoints.Count - 1;
            if (maxIndex < 0)
                minIndex = -1;
            else
                minIndex = 0;
        }

        /// <summary>
        /// Called when a control point is added.
        /// </summary>
        /// <param name="curveControlPoint">New control point.</param>
        /// <param name="index">Index of the control point.</param>
        protected internal override void ControlPointAdded(CurveControlPoint<BepuQuaternion> curveControlPoint, int index)
        {
        }

        /// <summary>
        /// Called when a control point is removed.
        /// </summary>
        /// <param name="curveControlPoint">Removed control point.</param>
        /// <param name="oldIndex">Index of the control point before it was removed.</param>
        protected internal override void ControlPointRemoved(CurveControlPoint<BepuQuaternion> curveControlPoint, int oldIndex)
        {
        }

        /// <summary>
        /// Called when a control point belonging to the curve has its time changed.
        /// </summary>
        /// <param name="curveControlPoint">Changed control point.</param>
        /// <param name="oldIndex">Old index of the control point.</param>
        /// <param name="newIndex">New index of the control point.</param>
        protected internal override void ControlPointTimeChanged(CurveControlPoint<BepuQuaternion> curveControlPoint, int oldIndex, int newIndex)
        {
        }

        /// <summary>
        /// Called when a control point belonging to the curve has its value changed.
        /// </summary>
        /// <param name="curveControlPoint">Changed control point.</param>
        protected internal override void ControlPointValueChanged(CurveControlPoint<BepuQuaternion> curveControlPoint)
        {
        }
    }
}