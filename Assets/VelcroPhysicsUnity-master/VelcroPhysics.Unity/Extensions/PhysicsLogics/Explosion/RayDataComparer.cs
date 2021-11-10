using System.Collections.Generic;
using FixMath.NET;

namespace VelcroPhysics.Extensions.PhysicsLogics.Explosion
{
    /// <summary>
    /// This is a comparer used for
    /// detecting angle difference between rays
    /// </summary>
    internal class RayDataComparer : IComparer<Fix64>
    {
        #region IComparer<Fix64> Members

        int IComparer<Fix64>.Compare(Fix64 a, Fix64 b)
        {
            var diff = a - b;
            if (diff > 0)
                return 1;
            if (diff < 0)
                return -1;
            return 0;
        }

        #endregion
    }
}