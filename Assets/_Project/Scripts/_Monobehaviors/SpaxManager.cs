using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedAnimationSystem;
namespace Spax
{
    public class SpaxManager : MonoBehaviour
    {
        public FixedAnimationController test;
        private static SpaxManager SpaxInstance;

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

        //for initializing the physics and filtering collisions
        //private CollisionGroup[] groups;

        void Awake()
        {
            SpaxInstance = this;
            Application.targetFrameRate = 60;
            test.Initialize();
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

    }
}