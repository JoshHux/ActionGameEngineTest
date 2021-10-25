using Spax;
using UnityEngine;

namespace ActionGameEngine
{
    public class RendererBehavior : SpaxBehavior
    {
        protected override void OnStart()
        {
            SpaxManager[] managers = Object.FindObjectsOfType<SpaxManager>();
            if (managers.Length > 0)
            {
                SpaxManager manager = managers[0];
                if (manager != null)
                {
                    manager.RenderUpdate += (() => RenderUpdate());
                    //Debug.Log("starting: " + gameObject.name);
                }
            }
        }

        protected virtual void RenderUpdate() { }
    }
}