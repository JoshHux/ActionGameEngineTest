using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionTests;
using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities;
using FixMath.NET;
using Spax;
using UnityEngine;

namespace BEPUUnity
{
    [ExecuteInEditMode]
    public abstract class ShapeBase : FixedBehavior
    {
        [HideInInspector]
        public ShapeBase parent;

        protected Entity m_entity = null;

        public BEPUutilities.BepuVector3 position
        {
            get
            {
                return m_entity.position;
            }
            set
            {
                m_entity.position = value;
            }
        }

        public BEPUutilities.BepuQuaternion rotation
        {
            get
            {
                return m_entity.orientation;
            }
            set
            {
                m_entity.orientation = value;
            }
        }

        public BEPUutilities.BepuVector3 velocity
        {
            get
            {
                return m_entity.linearVelocity;
            }
            set
            {
                m_entity.linearVelocity = value;
            }
        }

        private BEPUutilities.BepuVector3 m_localPosition;

        [HideInInspector]
        public BEPUutilities.BepuVector3 localPosition
        {
            get
            {
                return m_localPosition;
            }
            set
            {
                m_localPosition = value;
            }
        }

        private BEPUutilities.BepuQuaternion m_localRotation;

        private BEPUutilities.BepuQuaternion parentStartRot;

        [HideInInspector]
        public BEPUutilities.BepuQuaternion localRotation
        {
            get
            {
                return m_localRotation;
            }
            set
            {
                m_localRotation = value;
            }
        }

        public BEPUutilities.BepuVector3 offset;

        protected BEPUutilities.BepuVector3 m_startPosition;

        protected BEPUutilities.BepuQuaternion m_startOrientation;

        [SerializeField]
        protected Fix64 m_mass;

        public bool isTrigger = false;

        public bool lockXRot = false;

        public bool lockYRot = false;

        public bool lockZRot = false;

        protected virtual void OnBepuAwake()
        {
        }

        protected override void OnAwake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (m_entity == null)
            {
                //assigns world position
                m_startPosition.X = (Fix64)transform.position.x;
                m_startPosition.Y = (Fix64)transform.position.y;
                m_startPosition.Z = (Fix64)transform.position.z;

                //assigns world rotation
                m_startOrientation.X = (Fix64)transform.rotation.x;
                m_startOrientation.Y = (Fix64)transform.rotation.y;
                m_startOrientation.Z = (Fix64)transform.rotation.z;
                m_startOrientation.W = (Fix64)transform.rotation.w;

                //initializes whatever shape we make
                OnBepuAwake();

                //lock x-axis rotation
                if (lockXRot)
                {
                    m_entity.localInertiaTensorInverse.M11 = Fix64.Zero;
                    m_entity.localInertiaTensorInverse.M12 = Fix64.Zero;
                    m_entity.localInertiaTensorInverse.M13 = Fix64.Zero;
                }

                //lock y-axis rotation
                if (lockYRot)
                {
                    m_entity.localInertiaTensorInverse.M21 = Fix64.Zero;
                    m_entity.localInertiaTensorInverse.M22 = Fix64.Zero;
                    m_entity.localInertiaTensorInverse.M23 = Fix64.Zero;
                }

                //lock z-axis rotation
                if (lockZRot)
                {
                    m_entity.localInertiaTensorInverse.M31 = Fix64.Zero;
                    m_entity.localInertiaTensorInverse.M32 = Fix64.Zero;
                    m_entity.localInertiaTensorInverse.M33 = Fix64.Zero;
                }

                //prevents solver (thing that assigns forces if overlapped) from firing
                if (isTrigger)
                {
                    m_entity.CollisionInformation.collisionRules.personal |=
                        CollisionRule.NoSolver;
                }

                m_entity.CollisionInformation.gameObject = this.gameObject;
            }
        }

        protected override void OnStart()
        {
            //if there is a parent
            if (this.transform.parent != null)
            {
                ShapeBase possibleParent =
                    this.GetPossibleParent(this.transform);
                if (possibleParent != null)
                {
                    this.parent = possibleParent;

                    //assign local position
                    m_localPosition = m_entity.Position - parent.position;

                    //get local rotation
                    BEPUutilities.BepuQuaternion hold = parent.rotation;
                    BEPUutilities
                        .BepuQuaternion
                        .GetLocalRotation(ref this.m_entity.orientation,
                        ref hold,
                        out this.m_localRotation);
                    parentStartRot = hold;
                }
            }
            this.velocity = BEPUutilities.BepuVector3.Zero;
        }

        public ShapeBase GetPossibleParent(Transform node)
        {
            ShapeBase ret = null;

            if (node.parent != null)
            {
                ret = node.parent.gameObject.GetComponent<ShapeBase>();
                if (ret == null)
                {
                    ret = GetPossibleParent(node.parent);
                }
            }

            return ret;
        }

        public Entity GetEntity()
        {
            return m_entity;
        }

        protected override void SpaxUpdate()
        {
            if (parent != null)
            {
                //calculations to keep child attached to parent
                BEPUutilities.BepuQuaternion hold =
                    parent.GetEntity().orientation;

                //BEPUutilities.BepuQuaternion.GetLocalRotation(ref parentStartRot, ref parent.GetEntity().orientation, out hold);
                //BEPUutilities.BepuQuaternion.Add(ref parent.GetEntity().orientation, ref this.m_localRotation, out hold);
                //m_entity.orientation = hold;
                BEPUutilities.BepuVector3 newPos = m_localPosition;
                BEPUutilities
                    .BepuQuaternion
                    .Transform(ref m_localPosition, ref hold, out newPos);

                //assigns proper position based off of parent
                m_entity.position = parent.GetEntity().position + newPos;
            }
        }

        protected override void RenderUpdate()
        {
            transform.position =
                new Vector3((float)m_entity.position.X,
                    (float)m_entity.position.Y,
                    (float)m_entity.position.Z);
            transform.rotation =
                new Quaternion((float)m_entity.orientation.X,
                    (float)m_entity.orientation.Y,
                    (float)m_entity.orientation.Z,
                    (float)m_entity.orientation.W);
        }

        protected override void PostUpdate()
        {
            base.PostUpdate();
            if (this.gameObject.name == "Aganju2 (1)")
            {
                // Debug.Log(this.velocity);
            }
        }

        public void SetCollisionGroup(CollisionGroup newGroup)
        {
            //Debug.Log(m_entity);
            m_entity.CollisionInformation.collisionRules.group = newGroup;
        }

        public void SetAngVelocity(BEPUutilities.BepuVector3 newVel)
        {
            m_entity.angularVelocity = newVel;
        }
    }
}
