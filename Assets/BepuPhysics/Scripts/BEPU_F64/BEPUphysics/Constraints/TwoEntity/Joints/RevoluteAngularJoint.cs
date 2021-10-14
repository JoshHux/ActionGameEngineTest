﻿using System;
using BEPUphysics.Entities;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Constrains two entities to rotate only around a single axis.
    /// Acts like the angular portion of a hinge joint.
    /// </summary>
    public class RevoluteAngularJoint : Joint, I2DImpulseConstraintWithError, I2DJacobianConstraint
    {
        private BepuVector2 accumulatedImpulse;
        private BepuVector2 biasVelocity;
        private Matrix2x2 effectiveMassMatrix;
        private BepuVector3 localAxisA, localAxisB;
        private BepuVector3 localConstrainedAxis1, localConstrainedAxis2; //Not a and b because they are both based on a...
        private BepuVector2 error;
        private BepuVector3 worldAxisA, worldAxisB;
        private BepuVector3 worldConstrainedAxis1, worldConstrainedAxis2;

        /// <summary>
        /// Constructs a new orientation joint.
        /// Orientation joints can be used to simulate the angular portion of a hinge.
        /// Orientation joints allow rotation around only a single axis.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the WorldFreeAxisA and WorldFreeAxisB (or their entity-local versions).
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public RevoluteAngularJoint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new orientation joint.
        /// Orientation joints can be used to simulate the angular portion of a hinge.
        /// Orientation joints allow rotation around only a single axis.
        /// </summary>
        /// <param name="connectionA">First entity connected in the orientation joint.</param>
        /// <param name="connectionB">Second entity connected in the orientation joint.</param>
        /// <param name="freeAxis">Axis allowed to rotate freely in world space.</param>
        public RevoluteAngularJoint(Entity connectionA, Entity connectionB, BepuVector3 freeAxis)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            //rA and rB store the local version of the axis.
            WorldFreeAxisA = freeAxis;
            WorldFreeAxisB = freeAxis;
        }

        /// <summary>
        /// Gets or sets the free axis in connection A's local space.
        /// Updates the internal restricted axes.
        /// </summary>
        public BepuVector3 LocalFreeAxisA
        {
            get { return localAxisA; }
            set
            {
                localAxisA = BepuVector3.Normalize(value);
                Matrix3x3.Transform(ref localAxisA, ref connectionA.orientationMatrix, out worldAxisA);
                UpdateRestrictedAxes();
            }
        }

        /// <summary>
        /// Gets or sets the free axis in connection B's local space.
        /// </summary>
        public BepuVector3 LocalFreeAxisB
        {
            get { return localAxisB; }
            set
            {
                localAxisB = BepuVector3.Normalize(value);
                Matrix3x3.Transform(ref localAxisB, ref connectionB.orientationMatrix, out worldAxisB);
            }
        }

        /// <summary>
        /// Gets or sets the free axis attached to connection A in world space.
        /// This does not change the other connection's free axis.
        /// Updates the internal restricted axes.
        /// </summary>
        public BepuVector3 WorldFreeAxisA
        {
            get { return worldAxisA; }
            set
            {
                worldAxisA = BepuVector3.Normalize(value);
                Matrix3x3.TransformTranspose(ref worldAxisA, ref connectionA.orientationMatrix, out localAxisA);
                UpdateRestrictedAxes();
            }
        }

        /// <summary>
        /// Gets or sets the free axis attached to connection A in world space.
        /// This does not change the other connection's free axis.
        /// </summary>
        public BepuVector3 WorldFreeAxisB
        {
            get { return worldAxisB; }
            set
            {
                worldAxisB = BepuVector3.Normalize(value);
                Matrix3x3.TransformTranspose(ref worldAxisB, ref connectionB.orientationMatrix, out localAxisB);
            }
        }

        private void UpdateRestrictedAxes()
        {
            localConstrainedAxis1 = BepuVector3.Cross(BepuVector3.Up, localAxisA);
            if (localConstrainedAxis1.LengthSquared() < F64.C0p001)
            {
                localConstrainedAxis1 = BepuVector3.Cross(BepuVector3.Right, localAxisA);
            }
            localConstrainedAxis2 = BepuVector3.Cross(localAxisA, localConstrainedAxis1);
            localConstrainedAxis1.Normalize();
            localConstrainedAxis2.Normalize();
        }

        #region I2DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public BepuVector2 RelativeVelocity
        {
            get
            {
                BepuVector3 velocity;
                BepuVector3.Subtract(ref connectionA.angularVelocity, ref connectionB.angularVelocity, out velocity);

#if !WINDOWS
                BepuVector2 lambda = new BepuVector2();
#else
                BepuVector2 lambda;
#endif
                BepuVector3.Dot(ref worldConstrainedAxis1, ref velocity, out lambda.X);
                BepuVector3.Dot(ref worldConstrainedAxis2, ref velocity, out lambda.Y);
                return lambda;
            }
        }

        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public BepuVector2 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// </summary>
        public BepuVector2 Error
        {
            get { return error; }
        }

        #endregion

        #region I2DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out BepuVector3 jacobianX, out BepuVector3 jacobianY)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First linear jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out BepuVector3 jacobianX, out BepuVector3 jacobianY)
        {
            jacobianX = Toolbox.ZeroVector;
            jacobianY = Toolbox.ZeroVector;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the first connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out BepuVector3 jacobianX, out BepuVector3 jacobianY)
        {
            jacobianX = worldConstrainedAxis1;
            jacobianY = worldConstrainedAxis2;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobianX">First angular jacobian entry for the second connected entity.</param>
        /// <param name="jacobianY">Second angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out BepuVector3 jacobianX, out BepuVector3 jacobianY)
        {
            jacobianX = -worldConstrainedAxis1;
            jacobianY = -worldConstrainedAxis2;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="massMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out Matrix2x2 massMatrix)
        {
            massMatrix = effectiveMassMatrix;
        }

        #endregion

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(Fix64 dt)
        {
            Matrix3x3.Transform(ref localAxisA, ref connectionA.orientationMatrix, out worldAxisA);
            Matrix3x3.Transform(ref localAxisB, ref connectionB.orientationMatrix, out worldAxisB);


            Matrix3x3.Transform(ref localConstrainedAxis1, ref connectionA.orientationMatrix, out worldConstrainedAxis1);
            Matrix3x3.Transform(ref localConstrainedAxis2, ref connectionA.orientationMatrix, out worldConstrainedAxis2);

            BepuVector3 error;
            BepuVector3.Cross(ref worldAxisA, ref worldAxisB, out error);

            BepuVector3.Dot(ref error, ref worldConstrainedAxis1, out this.error.X);
            BepuVector3.Dot(ref error, ref worldConstrainedAxis2, out this.error.Y);
            Fix64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);
            errorReduction = -errorReduction;
            biasVelocity.X = errorReduction * this.error.X;
            biasVelocity.Y = errorReduction * this.error.Y;


            //Ensure that the corrective velocity doesn't exceed the max.
            Fix64 length = biasVelocity.LengthSquared();
            if (length > maxCorrectiveVelocitySquared)
            {
                Fix64 multiplier = maxCorrectiveVelocity / Fix64.Sqrt(length);
                biasVelocity.X *= multiplier;
                biasVelocity.Y *= multiplier;
            }

            BepuVector3 axis1I, axis2I;
            if (connectionA.isDynamic && connectionB.isDynamic)
            {
                Matrix3x3 inertiaTensorSum;
                Matrix3x3.Add(ref connectionA.inertiaTensorInverse, ref connectionB.inertiaTensorInverse, out inertiaTensorSum);

                Matrix3x3.Transform(ref worldConstrainedAxis1, ref inertiaTensorSum, out axis1I);
                Matrix3x3.Transform(ref worldConstrainedAxis2, ref inertiaTensorSum, out axis2I);
            }
            else if (connectionA.isDynamic && !connectionB.isDynamic)
            {
                Matrix3x3.Transform(ref worldConstrainedAxis1, ref connectionA.inertiaTensorInverse, out axis1I);
                Matrix3x3.Transform(ref worldConstrainedAxis2, ref connectionA.inertiaTensorInverse, out axis2I);
            }
            else if (!connectionA.isDynamic && connectionB.isDynamic)
            {
                Matrix3x3.Transform(ref worldConstrainedAxis1, ref connectionB.inertiaTensorInverse, out axis1I);
                Matrix3x3.Transform(ref worldConstrainedAxis2, ref connectionB.inertiaTensorInverse, out axis2I);
            }
            else
            {
                throw new InvalidOperationException("Cannot constrain two kinematic bodies.");
            }

            BepuVector3.Dot(ref axis1I, ref worldConstrainedAxis1, out effectiveMassMatrix.M11);
            BepuVector3.Dot(ref axis1I, ref worldConstrainedAxis2, out effectiveMassMatrix.M12);
            BepuVector3.Dot(ref axis2I, ref worldConstrainedAxis1, out effectiveMassMatrix.M21);
            BepuVector3.Dot(ref axis2I, ref worldConstrainedAxis2, out effectiveMassMatrix.M22);
            effectiveMassMatrix.M11 += softness;
            effectiveMassMatrix.M22 += softness;
            Matrix2x2.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);
            Matrix2x2.Negate(ref effectiveMassMatrix, out effectiveMassMatrix);


        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //Warm Starting
#if !WINDOWS
            BepuVector3 impulse = new BepuVector3();
#else
            BepuVector3 impulse;
#endif
            impulse.X = worldConstrainedAxis1.X * accumulatedImpulse.X + worldConstrainedAxis2.X * accumulatedImpulse.Y;
            impulse.Y = worldConstrainedAxis1.Y * accumulatedImpulse.X + worldConstrainedAxis2.Y * accumulatedImpulse.Y;
            impulse.Z = worldConstrainedAxis1.Z * accumulatedImpulse.X + worldConstrainedAxis2.Z * accumulatedImpulse.Y;
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                BepuVector3.Negate(ref impulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }
        }


        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fix64 SolveIteration()
        {
            // lambda = -mc * (Jv + b)
            // P = JT * lambda
            BepuVector3 velocity;
            BepuVector3.Subtract(ref connectionA.angularVelocity, ref connectionB.angularVelocity, out velocity);

#if !WINDOWS
            BepuVector2 lambda = new BepuVector2();
#else
            BepuVector2 lambda;
#endif
            BepuVector3.Dot(ref worldConstrainedAxis1, ref velocity, out lambda.X);
            BepuVector3.Dot(ref worldConstrainedAxis2, ref velocity, out lambda.Y);
            BepuVector2.Add(ref lambda, ref biasVelocity, out lambda);
            BepuVector2 softnessImpulse;
            BepuVector2.Multiply(ref accumulatedImpulse, softness, out softnessImpulse);
            BepuVector2.Add(ref lambda, ref softnessImpulse, out lambda);
            Matrix2x2.Transform(ref lambda, ref effectiveMassMatrix, out lambda);
            BepuVector2.Add(ref accumulatedImpulse, ref lambda, out accumulatedImpulse);


#if !WINDOWS
            BepuVector3 impulse = new BepuVector3();
#else
            BepuVector3 impulse;
#endif
            impulse.X = worldConstrainedAxis1.X * lambda.X + worldConstrainedAxis2.X * lambda.Y;
            impulse.Y = worldConstrainedAxis1.Y * lambda.X + worldConstrainedAxis2.Y * lambda.Y;
            impulse.Z = worldConstrainedAxis1.Z * lambda.X + worldConstrainedAxis2.Z * lambda.Y;
            if (connectionA.isDynamic)
            {
                connectionA.ApplyAngularImpulse(ref impulse);
            }
            if (connectionB.isDynamic)
            {
                BepuVector3.Negate(ref impulse, out impulse);
                connectionB.ApplyAngularImpulse(ref impulse);
            }

            return (Fix64.Abs(lambda.X) + Fix64.Abs(lambda.Y));
        }


    }
}