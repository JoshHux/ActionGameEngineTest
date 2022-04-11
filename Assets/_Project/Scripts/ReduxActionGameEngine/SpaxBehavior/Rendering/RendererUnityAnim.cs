using UnityEngine;
using ActionGameEngine.Enum;
namespace ActionGameEngine.Rendering
{
    public class RendererUnityAnim : RendererObject
    {

        private string _noNewState = "NewState";
        private TransitionFlag prev;
        protected override void RenderUpdate()
        {
            if (((int)(helper.hitType & HitType.ENUM_MASK)) > 0)
            {
                //Debug.Log((helper.hitType & HitType.ENUM_MASK));
                animator.SetInteger("Hittype", (int)(helper.hitType & HitType.ENUM_MASK));
            }

            if (((int)helper.transitionFlags) > 0)
            {
                if (EnumHelper.HasEnum((uint)prev, (uint)TransitionFlag.AIRBORNE) && EnumHelper.HasEnum((uint)helper.transitionFlags, (uint)TransitionFlag.GROUNDED)) { PlayVFX(transform.position, 6); }

            }
            if (((int)helper.hitIndicator) > 0)
            {
                //Debug.Log("vfx play " + (((int)helper.hitIndicator) > 0) + " " + (helper.hitIndicator));
                if (EnumHelper.HasEnum((uint)helper.hitIndicator, (uint)HitIndicator.BLOCKED)) { PlayVFX(helper.hitPos, 5); }
                else
                {

                    PlayVFX(helper.hitPos, helper.hitVfx);
                }
            }

            var animName = this.p_animNameHolder.GetAnimName(helper.animState);
            if (helper.newState && (animName != this._noNewState))
            {
                AssignAnimationState(animName);
            }

            //step animation forwards by how many gameplay frames elapsed
            StepAnimator(helper.renderFrames);


            helper.animState = -1;
            helper.renderFrames = 0;
            helper.damageTaken = 0;
            helper.meterChange = 0;
            helper.installChange = 0;
            helper.hitIndicator = HitIndicator.WHIFFED;
            helper.hitType = 0;
            this.prev = helper.transitionFlags;
            helper.transitionFlags = 0;

        }

        protected void PlayVFX(Vector3 pos, int vfxIndex)
        {
            var vfx = this.p_vfxHolder.GetVFX(vfxIndex);
            Instantiate(vfx, pos, Quaternion.identity);
        }
        protected void PlayVFX(int vfxIndex)
        {
            var vfx = this.p_vfxHolder.GetVFX(vfxIndex);
            Instantiate(vfx, this.transform);
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