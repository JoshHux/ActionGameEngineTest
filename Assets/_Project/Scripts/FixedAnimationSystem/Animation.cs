
namespace FixedAnimationSystem
{
    [System.Serializable]
    public struct FixedAnimation
    {
        public AnimFrame[] frames;

        public FixedAnimation(AnimFrame[] frames)
        {
            this.frames = frames;
        }
    }

}
