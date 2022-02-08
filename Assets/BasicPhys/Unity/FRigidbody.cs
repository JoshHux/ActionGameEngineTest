using UnityEngine;
using FixMath.NET;
using FlatPhysics.Filter;
using Spax;
namespace FlatPhysics.Unity
{
    public abstract class FRigidbody : MonoBehaviour
    {
        protected FlatBody _rb;

        [SerializeField] protected bool isStatic;
        [SerializeField] protected bool isTrigger;
        [SerializeField] protected bool isPushbox;
        [SerializeField] protected Fix64 mass;

        public FlatBody Body
        {
            get
            {
                if (this._rb == null)
                {
                    this.StartPhys();
                }
                return this._rb;
            }
        }

        public FVector2 Velocity
        {
            get { return this._rb.LinearVelocity; }
            set { this._rb.LinearVelocity = value; }
        }

        public FVector2 LocalPosition
        {
            get { return this._rb.LocalPosition; }
            set { this._rb.LocalPosition = value; }
        }

        public bool Awake
        {
            get { return this._rb.Awake; }
            set { this._rb.Awake = value; }
        }

        void Start()
        {
            if (this._rb == null)
            {
                this.StartPhys();
            }
        }

        private void StartPhys()
        {
            //CollisionLayer layer = FlatWorldMono.instance.GetCollisions(this.gameObject.layer);
            CollisionLayer layer = SpaxManager.instance.GetCollisions(this.gameObject.layer);
            this.InstantiateBody(layer);

            this.ResolveStart();
        }

        private void ResolveStart()
        {
            FRigidbody hold = null;
            Transform possibleParent = this.transform.parent;

            if (possibleParent != null)
            {
                possibleParent = possibleParent.parent;
                if (possibleParent != null)
                {
                    possibleParent.TryGetComponent<FRigidbody>(out hold);
                }
            }

            if (hold != null)
            {
                var toSet = hold.Body;
                this._rb.SetParent(toSet);
                var offset = this._rb.Position - hold.Body.Position;
                this._rb.LocalPosition = offset;
            }
        }

        void Update()
        {
            if (this._rb != null)
                this.transform.position = new Vector3((float)this._rb.Position.x, (float)this._rb.Position.y, 0f);
        }

        protected abstract void InstantiateBody(CollisionLayer collidesWith);
        public virtual void SetDimensions(FVector2 newDim) { }
    }
}