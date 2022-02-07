using UnityEngine;
using FixMath.NET;
using FlatPhysics;
namespace FlatPhysics.Unity
{
    public class FCircle : FRigidbody
    {
        [SerializeField] private Fix64 _radius;
        public Fix64 Radius
        {
            get { return this._rb.Radius; }
            set
            {
                this._rb.Radius = value;
            }
        }

        protected override void InstantiateBody()
        {
            string thing = "";
            //Debug.Log("done");
            FVector2 pos = new FVector2((Fix64)this.transform.position.x, (Fix64)this.transform.position.y);
            FlatBody.CreateCircleBody(this._radius, this.mass, pos, this.isStatic, this.isTrigger, 0, out this._rb, out thing);
            FlatWorldMono.instance.AddBody(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (Application.isPlaying)
            {
                Vector3 pos = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
                Gizmos.matrix = Matrix4x4.TRS(pos, transform.rotation, Vector3.one);
                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                Gizmos.DrawWireSphere(Vector2.zero, (float)this._rb.Radius);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                Gizmos.DrawWireSphere(Vector2.zero, (float)_radius);
            }
        }
    }
}