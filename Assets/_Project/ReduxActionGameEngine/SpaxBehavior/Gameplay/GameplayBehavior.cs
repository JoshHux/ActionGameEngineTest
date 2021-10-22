using UnityEngine;
using Spax;
namespace ActionGameEngine
{
    public class GameplayBehavior : SpaxBehavior
    {

        protected override void OnStart()
        {
            SpaxManager[] managers = Object.FindObjectsOfType<SpaxManager>();
            if (managers.Length > 0)
            {
                SpaxManager manager = managers[0];
                if (manager != null)
                {
                    manager.InputUpdate += (() => InputUpdate());
                    manager.StateUpdate += (() => StateUpdate());
                    manager.StateCleanUpdate += (() => StateCleanUpdate());
                    manager.PreUpdate += (() => PreUpdate());
                    manager.SpaxUpdate += (() => SpaxUpdate());
                    manager.HitQueryUpdate += (() => HitboxQueryUpdate());
                    manager.HurtQueryUpdate += (() => HurtboxQueryUpdate());
                    manager.PostUpdate += (() => PostUpdate());
                    //Debug.Log("starting: " + gameObject.name);
                }
            }
        }
        protected virtual void InputUpdate() { }

        protected virtual void StateUpdate() { }
        //get it? It's a pun!...
        //this naming is gonna bite me later, I just know it...
        protected virtual void StateCleanUpdate() { }
        protected virtual void PreUpdate() { }
        protected virtual void SpaxUpdate() { }
        protected virtual void HitboxQueryUpdate() { }

        protected virtual void HurtboxQueryUpdate() { }
        protected virtual void PostUpdate() { }
    }
}