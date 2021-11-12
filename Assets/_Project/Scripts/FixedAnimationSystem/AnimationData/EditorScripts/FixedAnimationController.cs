using UnityEngine;
namespace FixedAnimationSystem
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]

    public class FixedAnimationController : ScriptableObject
    {
        public FixedAnimationState[] states;

        public void Initialize()
        {
            
        }
    }
}