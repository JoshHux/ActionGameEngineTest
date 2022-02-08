using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using FlatPhysics;
using FlatPhysics.Unity;
using FlatPhysics.Filter;
using ActionGameEngine;

namespace Spax
{
    public class SpaxManager : MonoBehaviour
    {
        //public FixedAnimationController test;
        public static SpaxManager instance;

        public delegate void InputUpdateEventHandler();
        public delegate void PreUpdateEventHandler();
        public delegate void StateUpdateEventHandler();
        public delegate void StateCleanUpdateEventHandler();
        public delegate void PostUpdateEventHandler();
        public delegate void HitQueryUpdateEventHandler();
        public delegate void HurtQueryUpdateEventHandler();
        public delegate void RenderUpdateEventHandler();
        public delegate void SpaxUpdateEventHandler();
        public delegate void RenderPrepEventHandler();
        public delegate void PreRenderEventHandler();
        public event InputUpdateEventHandler InputUpdate;
        public event StateUpdateEventHandler StateUpdate;
        public event StateCleanUpdateEventHandler StateCleanUpdate;
        public event PreUpdateEventHandler PreUpdate;
        public event HitQueryUpdateEventHandler HitQueryUpdate;

        public event HurtQueryUpdateEventHandler HurtQueryUpdate;
        public event PostUpdateEventHandler PostUpdate;
        public event RenderUpdateEventHandler RenderUpdate;
        public event RenderPrepEventHandler PrepRender;
        public event RenderPrepEventHandler PreRender;
        public event SpaxUpdateEventHandler SpaxUpdate;

        private List<LivingObject> entities;
        private List<ActionCharacterController> players;

        private FlatWorld _world;
        private Fix64 _timeStep;
        private CollisionLayer[] _collisionMatrix;

        //for initializing the physics and filtering collisions
        //private CollisionGroup[] groups;

        void Awake()
        {
            instance = this;
            Application.targetFrameRate = 60;
            //test.Initialize();
            entities = new List<LivingObject>();
            players = new List<ActionCharacterController>();

            //initialize physics world stuff
            //collision layer stuff
            this._collisionMatrix = new CollisionLayer[16];
            int len = 16;
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    bool collides = !Physics.GetIgnoreLayerCollision(i, j);

                    if (collides)
                    {
                        this._collisionMatrix[i] |= (CollisionLayer)(1 << j);
                    }
                }
            }
            //asssign world
            this._world = new FlatWorld();
            this._timeStep = (Fix64)1 / (Fix64)60;
        }


        void FixedUpdate()
        {
            GameplayUpdate();
            RendererUpdate();
        }

        private void GameplayUpdate()
        {
            InputUpdate?.Invoke();
            StateUpdate?.Invoke();
            StateCleanUpdate?.Invoke();
            PreUpdate?.Invoke();
            SpaxUpdate?.Invoke();
            //m_space.Update();
            //UpdatePhysics();
            //world.PhysUpdate();
            this._world.Step(this._timeStep, 128);
            HitQueryUpdate?.Invoke();
            HurtQueryUpdate?.Invoke();
            PostUpdate?.Invoke();
        }
        private void RendererUpdate()
        {
            PrepRender?.Invoke();
            PreRender?.Invoke();
            RenderUpdate?.Invoke();
        }

        public void TrackObject(LivingObject obj)
        {
            entities.Add(obj);
            switch (obj)
            {
                case ActionCharacterController actionChar:
                    players.Add(actionChar);
                    break;
            }

        }

        public int GetTrackingIndexOf(LivingObject obj)
        {
            int ret = -1;
            switch (obj)
            {
                case ActionCharacterController actionChar:
                    ret = players.FindIndex(a => a == actionChar);
                    break;
            }
            return ret;
        }


        public void AddBody(FRigidbody rb)
        {
            this._world.AddBody(rb.Body);
        }

        public CollisionLayer GetCollisions(int layer) { return this._collisionMatrix[layer]; }

    }
}