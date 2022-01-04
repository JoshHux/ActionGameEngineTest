using UnityEngine;
namespace ActionGameEngine.Rendering
{
    public class RendererUnityAnim : RendererObject
    {

        protected override void RenderUpdate()
        {
            if (helper.newState && (helper.animState != "NewState"))
            {
                AssignAnimationState(helper.animState);
            }

            //step animation forwards by how many gameplay frames elapsed
            StepAnimator(helper.renderFrames);
        }

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