using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionGameEngine.Data
{
    //data to pass to renderer about what changed and what to display
    public struct RendererHelper
    {

        //how many frames the animator needs to update during the Render update, incremented in the PostUpdate
        //Animator.Update(Time.fixedDeltaTime*(animFrames=1))

        public int animFrames;
        public string animState;

        public bool newState;
    }
}
