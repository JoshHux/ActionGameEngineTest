using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spax.Input;
using System;
using BEPUphysics.CollisionRuleManagement;

namespace Spax
{
    public class SpaxManager : MonoBehaviour
    {
        public delegate void InputUpdateEventHandler();
        public delegate void PreUpdateEventHandler();
        public delegate void StateUpdateEventHandler();
        public delegate void StateCleanUpdateEventHandler();
        public delegate void PostUpdateEventHandler();
        public delegate void HitQueryUpdateEventHandler();
        public delegate void HurtQueryUpdateEventHandler();
        public delegate void RenderUpdateEventHandler();
        public delegate void SpaxUpdateEventHandler();
        public event InputUpdateEventHandler InputUpdate;
        public event StateUpdateEventHandler StateUpdate;
        public event StateCleanUpdateEventHandler StateCleanUpdate;
        public event PreUpdateEventHandler PreUpdate;
        public event HitQueryUpdateEventHandler HitQueryUpdate;

        public event HurtQueryUpdateEventHandler HurtQueryUpdate;
        public event PostUpdateEventHandler PostUpdate;
        public event RenderUpdateEventHandler RenderUpdate;
        public event SpaxUpdateEventHandler SpaxUpdate;

        //for initializing the physics and filtering collisions
        private CollisionGroup[] groups;

        private BEPUphysics.Space m_space;
        void Awake()
        {
            instance = this;

            m_space = new BEPUphysics.Space();


            PhysicsCollisionMatrixLayerMasks2.SaveCollisionMatrix(true);

            int xLen = PhysicsCollisionMatrixLayerMasks2.CollisionMatrix.GetLength(0);
            int yLen = PhysicsCollisionMatrixLayerMasks2.CollisionMatrix.GetLength(0);
            groups = new CollisionGroup[xLen];
            for (int i = 0; i < xLen; i++)
            {
                CollisionGroup newGroup = new CollisionGroup();

                groups[i] = newGroup;
            }


            for (int i = 0; i < xLen; i++)
            {
                for (int j = i; j < yLen; j++)
                {

                    bool hold = PhysicsCollisionMatrixLayerMasks2.CollisionMatrix[i, j];
                    if (!hold && groups[j] != null)
                    {
                        //Debug.Log(i + " " + j);
                        CollisionGroup.DefineCollisionRule(groups[i], groups[j], CollisionRule.NoBroadPhase);
                    }
                }
            }


            Application.targetFrameRate = 60;

            var shapeList = Resources.FindObjectsOfTypeAll<BEPUUnity.ShapeBase>();
            for (int i = 0; i < shapeList.Length; i++)
            {
                shapeList[i].Initialize();
                shapeList[i].SetCollisionGroup(groups[shapeList[i].gameObject.layer]);
                m_space.Add(shapeList[i].GetEntity());
            }

        }

        void FixedUpdate()
        {
            InputUpdate?.Invoke();
            StateUpdate?.Invoke();
            StateCleanUpdate?.Invoke();
            PreUpdate?.Invoke();
            SpaxUpdate?.Invoke();
            m_space.Update();
            HitQueryUpdate?.Invoke();
            HurtQueryUpdate?.Invoke();
            PostUpdate?.Invoke();
            RenderUpdate?.Invoke();
        }


        public void Add(BEPUphysics.Constraints.TwoEntity.Joints.Joint joint)
        {
            m_space.Add(joint);
        }

        public void Add(BEPUphysics.Constraints.TwoEntity.JointLimits.JointLimit joint)
        {
            m_space.Add(joint);
        }


        private static SpaxManager instance;
    }
}