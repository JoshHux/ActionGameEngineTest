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
                //AssignAnimationState(helper.animState);
            }

            //step animation forwards by how many gameplay frames elapsed
            //StepAnimator(helper.renderFrames);
        }

        protected override void PostRenderUpdate() { }

        //call to assign a new animation to our animator
        //starts at frame vf
        protected void AssignAnimationState(string stateName)
        {
            animator.PlayInFixedTime(stateName, 0, 0f);
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

