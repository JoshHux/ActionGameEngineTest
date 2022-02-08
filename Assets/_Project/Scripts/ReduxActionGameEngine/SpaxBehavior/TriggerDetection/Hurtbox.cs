using UnityEngine;
using ActionGameEngine.Data;
using ActionGameEngine.Enum;
using ActionGameEngine.Interfaces;
using FixMath.NET;

namespace ActionGameEngine.Gameplay
{
    public class Hurtbox : TriggerDetector, IAlligned
    {
        [ReadOnly, SerializeField] private int _allignment;
        //what to send hit signal to when this is hit
        private IDamageable damageable;

        private bool _isActive;
        private HurtboxData data;
        private VulnerableObject owner;

        //protected override void OnStart()
        //{
        //    base.OnStart();
        //}

        public void Initialize()
        {
            owner = this.transform.parent.parent.gameObject.GetComponent<VulnerableObject>();
            damageable = owner;
            _allignment = owner.GetAllignment();
        }

        protected override void OnExitTrigger(GameObject other) { }
        protected override void OnEnterTrigger(GameObject other) { }

        public void ActivateHurtBox(HurtboxData boxData)
        {
            _isActive = true;
            data = boxData;
            //trigger.localPosition = data.localPos;
            //trigger.localRotation = new BepuQuaternion(data.localRot.Z, data.localRot.Y, data.localRot.Z, trigger.localRotation.W);
            FVector2 newPos = data.localPos;
            int facing = owner.GetFacing();
            newPos.x *= facing;

            trigger.SetDimensions(data.localDim);
            trigger.LocalPosition = newPos;
        }

        public HitIndicator HitThisBox(int attackerID, HitboxData boxData)
        {
            //sends signal to owner
            return damageable.GetHit(attackerID, boxData);
        }

        public void SetAllignment(int allignment) { this._allignment = allignment; }
        public int GetAllignment()
        {
            return _allignment;
        }

        public bool IsActive() { return this._isActive; }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
        }
#endif
    }
}