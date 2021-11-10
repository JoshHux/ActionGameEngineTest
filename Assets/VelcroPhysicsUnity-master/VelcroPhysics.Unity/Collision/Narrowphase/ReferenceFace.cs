using UnityEngine;
using FixMath.NET;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// Reference face used for clipping
    /// </summary>
    public struct ReferenceFace
    {
        public int i1, i2;

        public FVector2 v1, v2;

        public FVector2 Normal;

        public FVector2 SideNormal1;
        public Fix64 SideOffset1;

        public FVector2 SideNormal2;
        public Fix64 SideOffset2;
    }
}