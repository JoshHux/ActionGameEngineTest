using UnityEngine;
namespace ActionGameEngine.Rendering
{
    public class RendererObject : RendererBehavior
    {
        protected Animator animator;
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
            if (helper.newState)// && (helper.animState != "NewState"))
            {
                //AssignAnimationState(helper.animState);
            }

            //step animation forwards by how many gameplay frames elapsed
            //StepAnimator(helper.renderFrames);
        }

        protected override void PostRenderUpdate() { }



    }
}

