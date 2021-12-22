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

        public HitIndicator HitThisBox(int attackerID, HitboxData boxData, int dir)
        {
            //sends signal to owner
            return damageable.GetHit(attackerID, boxData, dir);
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
            //Gizmos.color = Color.green;

            Gizmos.color = new Color(0, 1, 0, 0.5f);

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
            if (trigger != null)
                Gizmos.DrawCube(Vector3.zero, new Vector2((float)(trigger as VelcroBox).Width * 0.8f, (float)(trigger as VelcroBox).Height * 0.8f));
        }
#endif
    }
}