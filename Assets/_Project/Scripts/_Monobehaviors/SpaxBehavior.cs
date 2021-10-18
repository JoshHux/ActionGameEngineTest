using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spax.Input;

namespace Spax
{
    public class SpaxBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            this.OnAwake();
        }
        void Start()
        {
            SpaxManager[] managers = Object.FindObjectsOfType<SpaxManager>();
            if (managers.Length > 0)
            {
                SpaxManager manager = managers[0];
                if (manager != null)
                {
                    manager.InputUpdate += (() => InputUpdate());
                    manager.StateCleanUpdate += (() => StateCleanUpdate());
                    manager.PreUpdate += (() => PreUpdate());
                    manager.SpaxUpdate += (() => SpaxUpdate());
                    manager.HitQueryUpdate += (() => HitboxQueryUpdate());
                    manager.HurtQueryUpdate += (() => HurtboxQueryUpdate());
                    manager.RenderUpdate += (() => RenderUpdate());
                    manager.PostUpdate += (() => PostUpdate());
                    //Debug.Log("starting: " + gameObject.name);
                }
                this.OnStart();
            }
        }

        protected virtual void OnStart() { }
        protected virtual void OnAwake() { }
        protected virtual void InputUpdate() { }
        //get it? It's a pun!...
        //this naming is gonna bite me later, I just know it...
        protected virtual void StateCleanUpdate() { }
        protected virtual void PreUpdate() { }
        protected virtual void SpaxUpdate() { }
        protected virtual void HitboxQueryUpdate() { }

        protected virtual void HurtboxQueryUpdate() { }
        protected virtual void PostUpdate() { }


        protected virtual void RenderUpdate() { }
    }
}