using UnityEngine;
using FixMath.NET;
namespace FlatPhysics.Unity
{
    public class FlatWorldMono : MonoBehaviour
    {

        public static FlatWorldMono instance;
        private FlatWorld _world;
        private Fix64 _timeStep;
        void Awake()
        {
            instance = this;
            this._world = new FlatWorld();
            this._timeStep = (Fix64)1 / (Fix64)60;
        }

        void FixedUpdate()
        {
            this._world.Step(this._timeStep, 128);
        }

        public void AddBody(FRigidbody rb)
        {
            this._world.AddBody(rb.Body);
        }
    }
}
