using ActionGameEngine.Data;
using ActionGameEngine.Enum;
using ActionGameEngine.Interfaces;
using Spax;


namespace ActionGameEngine.Gameplay
{
    public class Hurtbox : SpaxBehavior
    {
        private int allignment;
        //what to send hit signal to when this is hit
        private IDamageable damageable;


        protected override void OnStart()
        {
            base.OnStart();
            VulnerableObject root = this.transform.parent.parent.gameObject.GetComponent<VulnerableObject>();
            allignment = root.GetAllignment();
        }

        public void ActivateHurtBox(HurtboxData boxData)
        {
            //TODO: change hurtbox dimensions and local position based on boxData
        }

        public HitIndicator HitThisBox(int attackerID, HitboxData boxData)
        {
            return damageable.GetHit(attackerID, boxData);
        }

        public int GetAllignment()
        {
            return allignment;
        }
    }
}