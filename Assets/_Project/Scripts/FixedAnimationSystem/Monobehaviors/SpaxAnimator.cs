using System.Collections.Generic;
using UnityEngine;

namespace FixedAnimationSystem
{
    public class SpaxAnimator : MonoBehaviour
    {
        //list of all animations for a character
        private FixedAnimation[] allAnimations;
        //animation name -> index in the list of all animations
        //used to assign animation by the name of the animation
        private Dictionary<string, int> nameToAnimIndex;

        //the current animation that we are playing
        private FixedAnimation currentAnimation;

        //number of frames that have passed since the start of the animation
        private int framesElapsed;
        void Awake()
        {
            framesElapsed = 0;

        }
        public int GetFramesElapsed() { return framesElapsed; }
    }
}