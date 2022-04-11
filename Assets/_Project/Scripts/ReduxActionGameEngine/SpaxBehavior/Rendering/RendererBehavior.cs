using UnityEngine;
using ActionGameEngine.Data;
using ActionGameEngine.Rendering;
using Spax;

namespace ActionGameEngine
{
    public abstract class RendererBehavior : SpaxBehavior
    {

        [SerializeField] protected soVFXHolder p_vfxHolder;
        [SerializeField] protected soStateStringHolder p_animNameHolder;
        protected RendererHelper helper;

        protected override void OnStart()
        {
            SpaxManager[] managers = Object.FindObjectsOfType<SpaxManager>();
            if (managers.Length > 0)
            {
                SpaxManager manager = managers[0];
                if (manager != null)
                {
                    manager.PreRender += (() => PreRenderUpdate());
                    manager.RenderUpdate += (() => RenderUpdate());
                    //Debug.Log("starting: " + gameObject.name);
                }
            }
        }

        public void AssignHelper(RendererHelper help)
        {
            helper = help;
        }

        protected abstract void PreRenderUpdate();
        protected abstract void RenderUpdate();
        protected abstract void PostRenderUpdate();
    }
}