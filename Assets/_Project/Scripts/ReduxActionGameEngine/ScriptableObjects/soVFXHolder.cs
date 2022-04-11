using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ActionGameEngine.Rendering
{
    [CreateAssetMenu(fileName = "VFXHolder", menuName = "ScriptableObjects/Rendering/VFX/VFXHolder", order = 1)]

    public class soVFXHolder : ScriptableObject
    {
        [SerializeField] private GameObject[] _VFX;
        public GameObject GetVFX(int index) { return this._VFX[index]; }
    }
}