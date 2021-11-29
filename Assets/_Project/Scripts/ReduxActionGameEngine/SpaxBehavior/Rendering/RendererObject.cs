using UnityEngine;
namespace ActionGameEngine.Rendering
{
    public class RendererObject : RendererBehavior
    {
        private Animator animator;
        [SerializeField] private int lastKnownFacing = 1;

        protected override void OnStart()
        {
            base.OnStart();
            GameObject anim = ObjectFinder.FindChildWithTag(this.gameObject, "Rendering");
            animator = anim.GetComponentInChildren<Animator>();
            lastKnownFacing = 1;
            helper.facing = 1;
        }

        protected override void PreRenderUpdate()
        {
            int facingDir = helper.facing * lastKnownFacing;

            Vector3 newScale = this.transform.localScale;
            newScale.x = newScale.x * facingDir;

            //Debug.Log(helper.facing);

            this.transform.localScale = newScale;

            lastKnownFacing = helper.facing;
        }

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

