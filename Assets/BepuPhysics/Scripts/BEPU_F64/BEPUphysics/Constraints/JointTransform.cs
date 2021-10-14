using System;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Constraints
{
    /// <summary>
    /// Defines a three dimensional orthonormal basis used by a constraint.
    /// </summary>
    public class JointBasis3D
    {
        internal BepuVector3 localPrimaryAxis = BepuVector3.Backward;
        internal BepuVector3 localXAxis = BepuVector3.Right;
        internal BepuVector3 localYAxis = BepuVector3.Up;
        internal BepuVector3 primaryAxis = BepuVector3.Backward;
        internal Matrix3x3 rotationMatrix = Matrix3x3.Identity;
        internal BepuVector3 xAxis = BepuVector3.Right;
        internal BepuVector3 yAxis = BepuVector3.Up;

        /// <summary>
        /// Gets the primary axis of the transform in local space.
        /// </summary>
        public BepuVector3 LocalPrimaryAxis
        {
            get { return localPrimaryAxis; }
        }

        /// <summary>
        /// Gets or sets the local transform of the basis.
        /// </summary>
        public Matrix3x3 LocalTransform
        {
            get
            {
                var toReturn = new Matrix3x3 {Right = localXAxis, Up = localYAxis, Backward = localPrimaryAxis};
                return toReturn;
            }
            set { SetLocalAxes(value); }
        }

        /// <summary>
        /// Gets the X axis of the transform in local space.
        /// </summary>
        public BepuVector3 LocalXAxis
        {
            get { return localXAxis; }
        }

        /// <summary>
        /// Gets the Y axis of the transform in local space.
        /// </summary>
        public BepuVector3 LocalYAxis
        {
            get { return localYAxis; }
        }

        /// <summary>
        /// Gets the primary axis of the transform.
        /// </summary>
        public BepuVector3 PrimaryAxis
        {
            get { return primaryAxis; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix used by the joint transform to convert local space axes to world space.
        /// </summary>
        public Matrix3x3 RotationMatrix
        {
            get { return rotationMatrix; }
            set
            {
                rotationMatrix = value;
                ComputeWorldSpaceAxes();
            }
        }

        /// <summary>
        /// Gets or sets the world transform of the basis.
        /// </summary>
        public Matrix3x3 WorldTransform
        {
            get
            {
                var toReturn = new Matrix3x3 {Right = xAxis, Up = yAxis, Backward = primaryAxis};
                return toReturn;
            }
            set { SetWorldAxes(value); }
        }

        /// <summary>
        /// Gets the X axis of the transform.
        /// </summary>
        public BepuVector3 XAxis
        {
            get { return xAxis; }
        }

        /// <summary>
        /// Gets the Y axis of the transform.
        /// </summary>
        public BepuVector3 YAxis
        {
            get { return yAxis; }
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetLocalAxes(BepuVector3 primaryAxis, BepuVector3 xAxis, BepuVector3 yAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetLocalAxes(primaryAxis, xAxis, yAxis);
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        public void SetLocalAxes(BepuVector3 primaryAxis, BepuVector3 xAxis, BepuVector3 yAxis)
        {
            if (Fix64.Abs(BepuVector3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(primaryAxis, yAxis)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(xAxis, yAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            localPrimaryAxis = BepuVector3.Normalize(primaryAxis);
            localXAxis = BepuVector3.Normalize(xAxis);
            localYAxis = BepuVector3.Normalize(yAxis);
            ComputeWorldSpaceAxes();
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.
        /// The matrix's up vector is used as the y axis.</param>
        public void SetLocalAxes(Matrix3x3 matrix)
        {
            if (Fix64.Abs(BepuVector3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(matrix.Backward, matrix.Up)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(matrix.Right, matrix.Up)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            localPrimaryAxis = BepuVector3.Normalize(matrix.Backward);
            localXAxis = BepuVector3.Normalize(matrix.Right);
            localYAxis = BepuVector3.Normalize(matrix.Up);
            ComputeWorldSpaceAxes();
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetWorldAxes(BepuVector3 primaryAxis, BepuVector3 xAxis, BepuVector3 yAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetWorldAxes(primaryAxis, xAxis, yAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="yAxis">Third axis in the transform.</param>
        public void SetWorldAxes(BepuVector3 primaryAxis, BepuVector3 xAxis, BepuVector3 yAxis)
        {
            if (Fix64.Abs(BepuVector3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(primaryAxis, yAxis)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(xAxis, yAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            this.primaryAxis = BepuVector3.Normalize(primaryAxis);
            this.xAxis = BepuVector3.Normalize(xAxis);
            this.yAxis = BepuVector3.Normalize(yAxis);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
            Matrix3x3.TransformTranspose(ref this.yAxis, ref rotationMatrix, out localYAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.
        /// The matrix's up vector is used as the y axis.</param>
        public void SetWorldAxes(Matrix3x3 matrix)
        {
            if (Fix64.Abs(BepuVector3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(matrix.Backward, matrix.Up)) > Toolbox.BigEpsilon ||
				Fix64.Abs(BepuVector3.Dot(matrix.Right, matrix.Up)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform do not form an orthonormal basis.  Ensure that each axis is perpendicular to the other two.");

            primaryAxis = BepuVector3.Normalize(matrix.Backward);
            xAxis = BepuVector3.Normalize(matrix.Right);
            yAxis = BepuVector3.Normalize(matrix.Up);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
            Matrix3x3.TransformTranspose(ref this.yAxis, ref rotationMatrix, out localYAxis);
        }

        internal void ComputeWorldSpaceAxes()
        {
            Matrix3x3.Transform(ref localPrimaryAxis, ref rotationMatrix, out primaryAxis);
            Matrix3x3.Transform(ref localXAxis, ref rotationMatrix, out xAxis);
            Matrix3x3.Transform(ref localYAxis, ref rotationMatrix, out yAxis);
        }
    }

    /// <summary>
    /// Defines a two axes which are perpendicular to each other used by a constraint.
    /// </summary>
    public class JointBasis2D
    {
        internal BepuVector3 localPrimaryAxis = BepuVector3.Backward;
        internal BepuVector3 localXAxis = BepuVector3.Right;
        internal BepuVector3 primaryAxis = BepuVector3.Backward;
        internal Matrix3x3 rotationMatrix = Matrix3x3.Identity;
        internal BepuVector3 xAxis = BepuVector3.Right;

        /// <summary>
        /// Gets the primary axis of the transform in local space.
        /// </summary>
        public BepuVector3 LocalPrimaryAxis
        {
            get { return localPrimaryAxis; }
        }

        /// <summary>
        /// Gets the X axis of the transform in local space.
        /// </summary>
        public BepuVector3 LocalXAxis
        {
            get { return localXAxis; }
        }

        /// <summary>
        /// Gets the primary axis of the transform.
        /// </summary>
        public BepuVector3 PrimaryAxis
        {
            get { return primaryAxis; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix used by the joint transform to convert local space axes to world space.
        /// </summary>
        public Matrix3x3 RotationMatrix
        {
            get { return rotationMatrix; }
            set
            {
                rotationMatrix = value;
                ComputeWorldSpaceAxes();
            }
        }

        /// <summary>
        /// Gets the X axis of the transform.
        /// </summary>
        public BepuVector3 XAxis
        {
            get { return xAxis; }
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetLocalAxes(BepuVector3 primaryAxis, BepuVector3 xAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetLocalAxes(primaryAxis, xAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        public void SetLocalAxes(BepuVector3 primaryAxis, BepuVector3 xAxis)
        {
            if (Fix64.Abs(BepuVector3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");

            localPrimaryAxis = BepuVector3.Normalize(primaryAxis);
            localXAxis = BepuVector3.Normalize(xAxis);
            ComputeWorldSpaceAxes();
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.</param>
        public void SetLocalAxes(Matrix3x3 matrix)
        {
            if (Fix64.Abs(BepuVector3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            localPrimaryAxis = BepuVector3.Normalize(matrix.Backward);
            localXAxis = BepuVector3.Normalize(matrix.Right);
            ComputeWorldSpaceAxes();
        }


        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        /// <param name="rotationMatrix">Matrix to use to transform the local axes into world space.</param>
        public void SetWorldAxes(BepuVector3 primaryAxis, BepuVector3 xAxis, Matrix3x3 rotationMatrix)
        {
            this.rotationMatrix = rotationMatrix;
            SetWorldAxes(primaryAxis, xAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="primaryAxis">First axis in the transform.  Usually aligned along the main axis of a joint, like the twist axis of a TwistLimit.</param>
        /// <param name="xAxis">Second axis in the transform.</param>
        public void SetWorldAxes(BepuVector3 primaryAxis, BepuVector3 xAxis)
        {
            if (Fix64.Abs(BepuVector3.Dot(primaryAxis, xAxis)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            this.primaryAxis = BepuVector3.Normalize(primaryAxis);
            this.xAxis = BepuVector3.Normalize(xAxis);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
        }

        /// <summary>
        /// Sets up the axes of the transform and ensures that it is an orthonormal basis.
        /// </summary>
        /// <param name="matrix">Rotation matrix representing the three axes.
        /// The matrix's backward vector is used as the primary axis.  
        /// The matrix's right vector is used as the x axis.</param>
        public void SetWorldAxes(Matrix3x3 matrix)
        {
            if (Fix64.Abs(BepuVector3.Dot(matrix.Backward, matrix.Right)) > Toolbox.BigEpsilon)
                throw new ArgumentException("The axes provided to the joint transform are not perpendicular.  Ensure that the specified axes form a valid constraint.");
            primaryAxis = BepuVector3.Normalize(matrix.Backward);
            xAxis = BepuVector3.Normalize(matrix.Right);
            Matrix3x3.TransformTranspose(ref this.primaryAxis, ref rotationMatrix, out localPrimaryAxis);
            Matrix3x3.TransformTranspose(ref this.xAxis, ref rotationMatrix, out localXAxis);
        }

        internal void ComputeWorldSpaceAxes()
        {
            Matrix3x3.Transform(ref localPrimaryAxis, ref rotationMatrix, out primaryAxis);
            Matrix3x3.Transform(ref localXAxis, ref rotationMatrix, out xAxis);
        }
    }
}