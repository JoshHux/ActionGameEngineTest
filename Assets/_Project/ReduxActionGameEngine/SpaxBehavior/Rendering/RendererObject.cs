using UnityEngine;
namespace ActionGameEngine.Rendering
{
    public class RendererObject : RendererBehavior
    {
        private Animator animator;

        protected override void OnStart()
        {
            base.OnStart();
            GameObject anim = ObjectFinder.FindChildWithTag(this.gameObject, "Rendering");
            animator = anim.GetComponentInChildren<Animator>();
        }

        protected override void PreRenderUpdate() { }

        protected override void RenderUpdate()
        {
            if (helper.newState && (helper.animState != "NewState"))
            {
                //if 1 frame to animate, start at frame 0 for the animation, we're updating by 1 frame later anyway
                AssignAnimationState(helper.animState, helper.animFrames - 1);
            }

            //step animation forwards by how many gameplay frames elapsed
            StepAnimator(helper.animFrames);
        }

        protected override void PostRenderUpdate() { }

        //call to assign a new animation to our animator
        //starts at frame vf
        protected void AssignAnimationState(string stateName, int f)
        {
            float startTime = f * Time.fixedDeltaTime;
            animator.PlayInFixedTime(stateName, 0, startTime);
        }


        //call to update animator by f frames
        protected void StepAnimator(int f)
        {
            //updates the animator componenet
            animator.speed = 1.0f;
            animator.Update(Time.fixedDeltaTime * f);
            animator.speed = 0f;
        }
    }
}

