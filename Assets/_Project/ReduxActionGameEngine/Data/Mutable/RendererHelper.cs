using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionGameEngine.Data
{
    //data to pass to renderer about what changed and what to display
    public struct RendererHelper
    {
        public string animState;

        public bool newState;
    }
}
