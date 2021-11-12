
namespace FixedAnimationSystem
{
    [System.Serializable]
    public class FixedAnimation
    {
        //has all the frames of animation, length of the array is the length of the animation
        //index 0 is first frame of animation, index 1 is second frame, index 2 is thurn frame, so on
        private FixedFrame[] frames;

        public FixedAnimation(FixedFrame[] frames)
        {
            this.frames = frames;
        }

        public int GetLength() { return frames.Length; }
        public FixedFrame GetFrameAt(int i) { return frames[i]; }

        //returns the frame with total delta transform for f frames of animation
        public FixedFrame GetTotalDelta(int f) { return this.GetTotalDelta(0, f); }


        //returns the frame with delta transform data starting at frame f for n frames of animation
        public FixedFrame GetTotalDelta(int f, int n)
        {
            //get the frame at f1 and we'll add frames until f2
            //first frame of animation
            FixedFrame ret = frames[f];

            //while loop instead of for loop because why not?
            int i = f;
            //locally saving the number of frames to animate to (hopefully) cut compile time
            int end = f;
            while (i < end)
            {
                //local frame for efficiency
                FixedFrame hold = frames[i];
                //add the deltas
                ret += hold;

                //almost forgot to index
                i++;
            }

            return ret;
        }
    }

}
