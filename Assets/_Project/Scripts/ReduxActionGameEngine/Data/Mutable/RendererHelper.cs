using UnityEngine;
using ActionGameEngine.Enum;

namespace ActionGameEngine.Data
{
    //data to pass to renderer about what changed and what to display
    public struct RendererHelper
    {

        //how many frames the animator needs to update during the Render update, incremented in the PostUpdate
        //Animator.Update(Time.fixedDeltaTime*(animFrames=1))
        public int renderFrames;
        public int facing;
        public int damageTaken;
        public int comboHits;

        public int meterChange;
        public int installChange;

        public HitIndicator hitIndicator;
        public HitType hitType;
        public TransitionFlag transitionFlags;
        public Vector3 hitPos;
        public int hitVfx;

        public int animState;
        public bool newState;
    }
}
