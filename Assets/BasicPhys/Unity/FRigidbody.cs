using UnityEngine;
using FixMath.NET;
using FlatPhysics;
namespace FlatPhysics.Unity
{
    public abstract class FRigidbody : MonoBehaviour
    {
        protected FlatBody _rb;

        [SerializeField] protected bool isStatic;
        [SerializeField] protected bool isTrigger;
        [SerializeField] protected Fix64 mass;

        public FlatBody Body
        {
            get { return this._rb; }
        }

        public FVector2 Velocity
        {
            get { return this._rb.LinearVelocity; }
            internal set { this._rb.LinearVelocity = value; }
        }


        void Start()
        {
            this.InstantiateBody();
        }

        void Update()
        {
            this.transform.position = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
        }

        protected abstract void InstantiateBody();
    }
}