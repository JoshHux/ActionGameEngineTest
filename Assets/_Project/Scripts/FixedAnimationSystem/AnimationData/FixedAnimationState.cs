
namespace FixedAnimationSystem
{
    [System.Serializable]
    public class FixedAnimationState
    {
        //name of state/animation
        [UnityEngine.SerializeField] private string StateName;
        //the transform data that is used to visual transform the object when animating
        [UnityEngine.SerializeField] private FixedAnimation animationData;
        //whether or not we should loop the animation
        [UnityEngine.SerializeField] private bool looping;
        //what transitions we can take
        [UnityEngine.SerializeField] private FixedTransition[] transitions;

        public string GetName() { return StateName; }
        public FixedAnimation GetAnimation() { return animationData; }
        public bool GetIsLooping() { return looping; }


        //returns a frame with the delta transform data for f frames of animation
        public FixedFrame GetFrame(int f)
        {
            //subtract 1 to get index, f is the number of frames to animate
            int i = f - 1;
            return animationData.GetFrameAt(i);
        }

        //returns a frame with the total delta transform data for f frames of animation
        public FixedFrame GetTotalFrame(int framesToAnimate) { return animationData.GetTotalDelta(framesToAnimate); }

        //returns the frame with delta transform data starting at frame f for n frames of animation
        public FixedFrame GetTotalFrame(int startFrame, int framesToAnimate) { return animationData.GetTotalDelta(startFrame, framesToAnimate); }

    }

}
