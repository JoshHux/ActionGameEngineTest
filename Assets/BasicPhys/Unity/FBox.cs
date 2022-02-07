using UnityEngine;
using FixMath.NET;
using FlatPhysics;
namespace FlatPhysics.Unity
{
    public class FBox : FRigidbody
    {
        [SerializeField] private Fix64 _width;
        [SerializeField] private Fix64 _height;

        public Fix64 Width
        {
            get { return this._rb.Width; }
            set
            {
                this._rb.Width = value;
            }
        }

        public Fix64 Height
        {
            get { return this._rb.Height; }
            set
            {
                this._rb.Height = value;
            }
        }

        protected override void InstantiateBody()
        {
            string thing = "";
            //Debug.Log("done");
            FVector2 pos = new FVector2((Fix64)this.transform.position.x, (Fix64)this.transform.position.y);
            FlatBody.CreateBoxBody(this._width, this._height, this.mass, pos, this.isStatic, this.isTrigger, 0, out this._rb, out thing);
            FlatWorldMono.instance.AddBody(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (Application.isPlaying)
            {
                Vector3 pos = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
                Vector3 dim = new Vector3((float)this._rb.Width, (float)this._rb.Height, 1f);

                Gizmos.matrix = Matrix4x4.TRS(pos, transform.rotation, Vector3.one);

                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                Gizmos.DrawCube(Vector2.zero, dim);
            }
            else
            {
                Vector3 dim = new Vector3((float)this._width, (float)this._height, 1f);

                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH`    
                Gizmos.DrawCube(Vector2.zero, dim);
            }
        }
    }
}
