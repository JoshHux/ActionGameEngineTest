using UnityEngine;

namespace Spax
{
    public class SpaxBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            this.OnAwake();
        }

        void Start()
        {
            this.OnStart();
        }

        protected virtual void OnStart() { }

        protected virtual void OnAwake() { }
    }
}
