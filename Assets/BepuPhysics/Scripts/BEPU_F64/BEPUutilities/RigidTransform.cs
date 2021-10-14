 

namespace BEPUutilities
{
    ///<summary>
    /// Transform composed of a rotation and translation.
    ///</summary>
    public struct RigidTransform
    {
        ///<summary>
        /// Translation component of the transform.
        ///</summary>
        public BepuVector3 Position;
        ///<summary>
        /// Rotation component of the transform.
        ///</summary>
        public BepuQuaternion Orientation;

        ///<summary>
        /// Constructs a new rigid transform.
        ///</summary>
        ///<param name="position">Translation component of the transform.</param>
        ///<param name="orientation">Rotation component of the transform.</param>
        public RigidTransform(BepuVector3 position, BepuQuaternion orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        ///<summary>
        /// Constructs a new rigid transform.
        ///</summary>
        ///<param name="position">Translation component of the transform.</param>
        public RigidTransform(BepuVector3 position)
        {
            Position = position;
            Orientation = BepuQuaternion.Identity;
        }

        ///<summary>
        /// Constructs a new rigid transform.
        ///</summary>
        ///<param name="orienation">Rotation component of the transform.</param>
        public RigidTransform(BepuQuaternion orienation)
        {
            Position = new BepuVector3();
            Orientation = orienation;
        }

        /// <summary>
        /// Gets the orientation matrix created from the orientation of the rigid transform.
        /// </summary>
        public Matrix OrientationMatrix
        {
            get
            {
                Matrix toReturn;
                Matrix.CreateFromBepuQuaternion(ref Orientation, out toReturn);
                return toReturn;
            }
        }
        ///<summary>
        /// Gets the 4x4 matrix created from the rigid transform.
        ///</summary>
        public Matrix Matrix
        {
            get
            {
                Matrix toReturn;
                Matrix.CreateFromBepuQuaternion(ref Orientation, out toReturn);
                toReturn.Translation = Position;
                return toReturn;
            }
        }

      

        ///<summary>
        /// Gets the identity rigid transform.
        ///</summary>
        public static RigidTransform Identity
        {
            get
            {
                var t = new RigidTransform {Orientation = BepuQuaternion.Identity, Position = new BepuVector3()};
                return t;
            }
        }

        /// <summary>
        /// Inverts a rigid transform.
        /// </summary>
        /// <param name="transform">Transform to invert.</param>
        /// <param name="inverse">Inverse of the transform.</param>
        public static void Invert(ref RigidTransform transform, out RigidTransform inverse)
        {
            BepuQuaternion.Conjugate(ref transform.Orientation, out inverse.Orientation);
            BepuQuaternion.Transform(ref transform.Position, ref inverse.Orientation, out inverse.Position);
            BepuVector3.Negate(ref inverse.Position, out inverse.Position);
        }

        ///<summary>
        /// Concatenates a rigid transform with another rigid transform.
        ///</summary>
        ///<param name="a">The first rigid transform.</param>
        ///<param name="b">The second rigid transform.</param>
        ///<param name="combined">Concatenated rigid transform.</param>
        public static void Multiply(ref RigidTransform a, ref RigidTransform b, out RigidTransform combined)
        {
            BepuVector3 intermediate;
            BepuQuaternion.Transform(ref a.Position, ref b.Orientation, out intermediate);
            BepuVector3.Add(ref intermediate, ref b.Position, out combined.Position);
            BepuQuaternion.Concatenate(ref a.Orientation, ref b.Orientation, out combined.Orientation);

        }

        ///<summary>
        /// Concatenates a rigid transform with another rigid transform's inverse.
        ///</summary>
        ///<param name="a">The first rigid transform.</param>
        ///<param name="b">The second rigid transform whose inverse will be concatenated to the first.</param>
        ///<param name="combinedTransform">Combined rigid transform.</param>
        public static void MultiplyByInverse(ref RigidTransform a, ref RigidTransform b, out RigidTransform combinedTransform)
        {
            Invert(ref b, out combinedTransform);
            Multiply(ref a, ref combinedTransform, out combinedTransform);
        }

        ///<summary>
        /// Transforms a position by a rigid transform.
        ///</summary>
        ///<param name="position">Position to transform.</param>
        ///<param name="transform">Transform to apply.</param>
        ///<param name="result">Transformed position.</param>
        public static void Transform(ref BepuVector3 position, ref RigidTransform transform, out BepuVector3 result)
        {
            BepuVector3 intermediate;
            BepuQuaternion.Transform(ref position, ref transform.Orientation, out intermediate);
            BepuVector3.Add(ref intermediate, ref transform.Position, out result);
        }


        ///<summary>
        /// Transforms a position by a rigid transform's inverse.
        ///</summary>
        ///<param name="position">Position to transform.</param>
        ///<param name="transform">Transform to invert and apply.</param>
        ///<param name="result">Transformed position.</param>
        public static void TransformByInverse(ref BepuVector3 position, ref RigidTransform transform, out BepuVector3 result)
        {
            BepuQuaternion orientation;
            BepuVector3 intermediate;
            BepuVector3.Subtract(ref position, ref transform.Position, out intermediate);
            BepuQuaternion.Conjugate(ref transform.Orientation, out orientation);
            BepuQuaternion.Transform(ref intermediate, ref orientation, out result);
        }


    }
}
