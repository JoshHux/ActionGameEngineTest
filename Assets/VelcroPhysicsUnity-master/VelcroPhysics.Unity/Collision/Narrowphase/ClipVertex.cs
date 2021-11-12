using UnityEngine;
using VelcroPhysics.Collision.ContactSystem;
using FixMath.NET;

namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// Used for computing contact manifolds.
    /// </summary>
    internal struct ClipVertex
    {
        public ContactID ID;
        public FVector2 V;
    }
}