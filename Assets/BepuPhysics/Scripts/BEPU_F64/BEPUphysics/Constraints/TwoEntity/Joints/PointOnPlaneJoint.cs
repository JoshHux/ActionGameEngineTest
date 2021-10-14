using BEPUphysics.Entities;

using BEPUutilities;
using FixMath.NET;

namespace BEPUphysics.Constraints.TwoEntity.Joints
{
    /// <summary>
    /// Constrains a point on one body to be on a plane defined by another body.
    /// </summary>
    public class PointOnPlaneJoint : Joint, I1DImpulseConstraintWithError, I1DJacobianConstraint
    {
        private Fix64 accumulatedImpulse;
        private Fix64 biasVelocity;
        private Fix64 error;

        private BepuVector3 localPlaneAnchor;
        private BepuVector3 localPlaneNormal;
        private BepuVector3 localPointAnchor;

        private BepuVector3 worldPlaneAnchor;
        private BepuVector3 worldPlaneNormal;
        private BepuVector3 worldPointAnchor;
        private Fix64 negativeEffectiveMass;
        private BepuVector3 rA;
        private BepuVector3 rAcrossN;
        private BepuVector3 rB;
        private BepuVector3 rBcrossN;

        /// <summary>
        /// Constructs a new point on plane constraint.
        /// To finish the initialization, specify the connections (ConnectionA and ConnectionB) 
        /// as well as the PlaneAnchor, PlaneNormal, and PointAnchor (or their entity-local versions).
        /// This constructor sets the constraint's IsActive property to false by default.
        /// </summary>
        public PointOnPlaneJoint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a new point on plane constraint.
        /// </summary>
        /// <param name="connectionA">Entity to which the constraint's plane is attached.</param>
        /// <param name="connectionB">Entity to which the constraint's point is attached.</param>
        /// <param name="planeAnchor">A point on the plane.</param>
        /// <param name="normal">Direction, attached to the first connected entity, defining the plane's normal</param>
        /// <param name="pointAnchor">The point to constrain to the plane, attached to the second connected object.</param>
        public PointOnPlaneJoint(Entity connectionA, Entity connectionB, BepuVector3 planeAnchor, BepuVector3 normal, BepuVector3 pointAnchor)
        {
            ConnectionA = connectionA;
            ConnectionB = connectionB;

            PointAnchor = pointAnchor;
            PlaneAnchor = planeAnchor;
            PlaneNormal = normal;
        }

        /// <summary>
        /// Gets or sets the plane's anchor in entity A's local space.
        /// </summary>
        public BepuVector3 LocalPlaneAnchor
        {
            get { return localPlaneAnchor; }
            set
            {
                localPlaneAnchor = value;
                Matrix3x3.Transform(ref localPlaneAnchor, ref connectionA.orientationMatrix, out worldPlaneAnchor);
                BepuVector3.Add(ref connectionA.position, ref worldPlaneAnchor, out worldPlaneAnchor);
            }
        }

        /// <summary>
        /// Gets or sets the plane's normal in entity A's local space.
        /// </summary>
        public BepuVector3 LocalPlaneNormal
        {
            get { return localPlaneNormal; }
            set
            {
                localPlaneNormal = BepuVector3.Normalize(value);
                Matrix3x3.Transform(ref localPlaneNormal, ref connectionA.orientationMatrix, out worldPlaneNormal);
            }
        }

        /// <summary>
        /// Gets or sets the point anchor in entity B's local space.
        /// </summary>
        public BepuVector3 LocalPointAnchor
        {
            get { return localPointAnchor; }
            set
            {
                localPointAnchor = value;
                Matrix3x3.Transform(ref localPointAnchor, ref connectionB.orientationMatrix, out worldPointAnchor);
                BepuVector3.Add(ref worldPointAnchor, ref connectionB.position, out worldPointAnchor);
            }
        }

        /// <summary>
        /// Gets the offset from A to the connection point between the entities.
        /// </summary>
        public BepuVector3 OffsetA
        {
            get { return rA; }
        }

        /// <summary>
        /// Gets the offset from B to the connection point between the entities.
        /// </summary>
        public BepuVector3 OffsetB
        {
            get { return rB; }
        }

        /// <summary>
        /// Gets or sets the plane anchor in world space.
        /// </summary>
        public BepuVector3 PlaneAnchor
        {
            get { return worldPlaneAnchor; }
            set
            {
                worldPlaneAnchor = value;
                localPlaneAnchor = value - connectionA.position;
                Matrix3x3.TransformTranspose(ref localPlaneAnchor, ref connectionA.orientationMatrix, out localPlaneAnchor);

            }
        }

        /// <summary>
        /// Gets or sets the plane's normal in world space.
        /// </summary>
        public BepuVector3 PlaneNormal
        {
            get { return worldPlaneNormal; }
            set
            {
                worldPlaneNormal = BepuVector3.Normalize(value);
                Matrix3x3.TransformTranspose(ref worldPlaneNormal, ref connectionA.orientationMatrix, out localPlaneNormal);
            }
        }

        /// <summary>
        /// Gets or sets the point anchor in world space.
        /// </summary>
        public BepuVector3 PointAnchor
        {
            get { return worldPointAnchor; }
            set
            {
                worldPointAnchor = value;
                localPointAnchor = value - connectionB.position;
                Matrix3x3.TransformTranspose(ref localPointAnchor, ref connectionB.orientationMatrix, out localPointAnchor);

            }
        }

        #region I1DImpulseConstraintWithError Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        public Fix64 RelativeVelocity
        {
            get
            {
                BepuVector3 dv;
                BepuVector3 aVel, bVel;
                BepuVector3.Cross(ref connectionA.angularVelocity, ref rA, out aVel);
                BepuVector3.Add(ref aVel, ref connectionA.linearVelocity, out aVel);
                BepuVector3.Cross(ref connectionB.angularVelocity, ref rB, out bVel);
                BepuVector3.Add(ref bVel, ref connectionB.linearVelocity, out bVel);
                BepuVector3.Subtract(ref aVel, ref bVel, out dv);
                Fix64 velocityDifference;
                BepuVector3.Dot(ref dv, ref worldPlaneNormal, out velocityDifference);
                return velocityDifference;
            }
        }


        /// <summary>
        /// Gets the total impulse applied by this constraint.
        /// </summary>
        public Fix64 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the current constraint error.
        /// </summary>
        public Fix64 Error
        {
            get { return error; }
        }

        #endregion

        #region I1DJacobianConstraint Members

        /// <summary>
        /// Gets the linear jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the first connected entity.</param>
        public void GetLinearJacobianA(out BepuVector3 jacobian)
        {
            jacobian = worldPlaneNormal;
        }

        /// <summary>
        /// Gets the linear jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Linear jacobian entry for the second connected entity.</param>
        public void GetLinearJacobianB(out BepuVector3 jacobian)
        {
            jacobian = -worldPlaneNormal;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the first connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the first connected entity.</param>
        public void GetAngularJacobianA(out BepuVector3 jacobian)
        {
            jacobian = rAcrossN;
        }

        /// <summary>
        /// Gets the angular jacobian entry for the second connected entity.
        /// </summary>
        /// <param name="jacobian">Angular jacobian entry for the second connected entity.</param>
        public void GetAngularJacobianB(out BepuVector3 jacobian)
        {
            jacobian = -rBcrossN;
        }

        /// <summary>
        /// Gets the mass matrix of the constraint.
        /// </summary>
        /// <param name="outputMassMatrix">Constraint's mass matrix.</param>
        public void GetMassMatrix(out Fix64 outputMassMatrix)
        {
            outputMassMatrix = -negativeEffectiveMass;
        }

        #endregion

        /// <summary>
        /// Computes one iteration of the constraint to meet the solver updateable's goal.
        /// </summary>
        /// <returns>The rough applied impulse magnitude.</returns>
        public override Fix64 SolveIteration()
        {
            //TODO: This could technically be faster.
            //Form the jacobian explicitly.
            //Cross cross add add subtract dot
            //vs
            //dot dot dot dot and then scalar adds
            BepuVector3 dv;
            BepuVector3 aVel, bVel;
            BepuVector3.Cross(ref connectionA.angularVelocity, ref rA, out aVel);
            BepuVector3.Add(ref aVel, ref connectionA.linearVelocity, out aVel);
            BepuVector3.Cross(ref connectionB.angularVelocity, ref rB, out bVel);
            BepuVector3.Add(ref bVel, ref connectionB.linearVelocity, out bVel);
            BepuVector3.Subtract(ref aVel, ref bVel, out dv);
            Fix64 velocityDifference;
            BepuVector3.Dot(ref dv, ref worldPlaneNormal, out velocityDifference);
            //if(velocityDifference > 0)
            //    Debug.WriteLine("Velocity difference: " + velocityDifference);
            //Debug.WriteLine("softness velocity: " + softness * accumulatedImpulse);
            Fix64 lambda = negativeEffectiveMass * (velocityDifference + biasVelocity + softness * accumulatedImpulse);
            accumulatedImpulse += lambda;

            BepuVector3 impulse;
            BepuVector3 torque;
            BepuVector3.Multiply(ref worldPlaneNormal, lambda, out impulse);
            if (connectionA.isDynamic)
            {
                BepuVector3.Multiply(ref rAcrossN, lambda, out torque);
                connectionA.ApplyLinearImpulse(ref impulse);
                connectionA.ApplyAngularImpulse(ref torque);
            }
            if (connectionB.isDynamic)
            {
                BepuVector3.Negate(ref impulse, out impulse);
                BepuVector3.Multiply(ref rBcrossN, lambda, out torque);
                connectionB.ApplyLinearImpulse(ref impulse);
                connectionB.ApplyAngularImpulse(ref torque);
            }

            return lambda;
        }

        ///<summary>
        /// Performs the frame's configuration step.
        ///</summary>
        ///<param name="dt">Timestep duration.</param>
        public override void Update(Fix64 dt)
        {
            Matrix3x3.Transform(ref localPlaneNormal, ref connectionA.orientationMatrix, out worldPlaneNormal);
            Matrix3x3.Transform(ref localPlaneAnchor, ref connectionA.orientationMatrix, out worldPlaneAnchor);
            BepuVector3.Add(ref worldPlaneAnchor, ref connectionA.position, out worldPlaneAnchor);

            Matrix3x3.Transform(ref localPointAnchor, ref connectionB.orientationMatrix, out rB);
            BepuVector3.Add(ref rB, ref connectionB.position, out worldPointAnchor);

            //Find rA and rB.
            //So find the closest point on the plane to worldPointAnchor.
            Fix64 pointDistance, planeDistance;
            BepuVector3.Dot(ref worldPointAnchor, ref worldPlaneNormal, out pointDistance);
            BepuVector3.Dot(ref worldPlaneAnchor, ref worldPlaneNormal, out planeDistance);
            Fix64 distanceChange = planeDistance - pointDistance;
            BepuVector3 closestPointOnPlane;
            BepuVector3.Multiply(ref worldPlaneNormal, distanceChange, out closestPointOnPlane);
            BepuVector3.Add(ref closestPointOnPlane, ref worldPointAnchor, out closestPointOnPlane);

            BepuVector3.Subtract(ref closestPointOnPlane, ref connectionA.position, out rA);

            BepuVector3.Cross(ref rA, ref worldPlaneNormal, out rAcrossN);
            BepuVector3.Cross(ref rB, ref worldPlaneNormal, out rBcrossN);
            BepuVector3.Negate(ref rBcrossN, out rBcrossN);

            BepuVector3 offset;
            BepuVector3.Subtract(ref worldPointAnchor, ref closestPointOnPlane, out offset);
            BepuVector3.Dot(ref offset, ref worldPlaneNormal, out error);
            Fix64 errorReduction;
            springSettings.ComputeErrorReductionAndSoftness(dt, F64.C1 / dt, out errorReduction, out softness);
            biasVelocity = MathHelper.Clamp(-errorReduction * error, -maxCorrectiveVelocity, maxCorrectiveVelocity);

            if (connectionA.IsDynamic && connectionB.IsDynamic)
            {
                BepuVector3 IrACrossN, IrBCrossN;
                Matrix3x3.Transform(ref rAcrossN, ref connectionA.inertiaTensorInverse, out IrACrossN);
                Matrix3x3.Transform(ref rBcrossN, ref connectionB.inertiaTensorInverse, out IrBCrossN);
                Fix64 angularA, angularB;
                BepuVector3.Dot(ref rAcrossN, ref IrACrossN, out angularA);
                BepuVector3.Dot(ref rBcrossN, ref IrBCrossN, out angularB);
                negativeEffectiveMass = connectionA.inverseMass + connectionB.inverseMass + angularA + angularB;
                negativeEffectiveMass = -1 / (negativeEffectiveMass + softness);
            }
            else if (connectionA.IsDynamic && !connectionB.IsDynamic)
            {
                BepuVector3 IrACrossN;
                Matrix3x3.Transform(ref rAcrossN, ref connectionA.inertiaTensorInverse, out IrACrossN);
                Fix64 angularA;
                BepuVector3.Dot(ref rAcrossN, ref IrACrossN, out angularA);
                negativeEffectiveMass = connectionA.inverseMass + angularA;
                negativeEffectiveMass = -1 / (negativeEffectiveMass + softness);
            }
            else if (!connectionA.IsDynamic && connectionB.IsDynamic)
            {
                BepuVector3 IrBCrossN;
                Matrix3x3.Transform(ref rBcrossN, ref connectionB.inertiaTensorInverse, out IrBCrossN);
                Fix64 angularB;
                BepuVector3.Dot(ref rBcrossN, ref IrBCrossN, out angularB);
                negativeEffectiveMass = connectionB.inverseMass + angularB;
                negativeEffectiveMass = -1 / (negativeEffectiveMass + softness);
            }
            else
                negativeEffectiveMass = F64.C0;


        }

        /// <summary>
        /// Performs any pre-solve iteration work that needs exclusive
        /// access to the members of the solver updateable.
        /// Usually, this is used for applying warmstarting impulses.
        /// </summary>
        public override void ExclusiveUpdate()
        {
            //Warm Starting
            BepuVector3 impulse;
            BepuVector3 torque;
            BepuVector3.Multiply(ref worldPlaneNormal, accumulatedImpulse, out impulse);
            if (connectionA.isDynamic)
            {
                BepuVector3.Multiply(ref rAcrossN, accumulatedImpulse, out torque);
                connectionA.ApplyLinearImpulse(ref impulse);
                connectionA.ApplyAngularImpulse(ref torque);
            }
            if (connectionB.isDynamic)
            {
                BepuVector3.Negate(ref impulse, out impulse);
                BepuVector3.Multiply(ref rBcrossN, accumulatedImpulse, out torque);
                connectionB.ApplyLinearImpulse(ref impulse);
                connectionB.ApplyAngularImpulse(ref torque);
            }
        }
    }
}