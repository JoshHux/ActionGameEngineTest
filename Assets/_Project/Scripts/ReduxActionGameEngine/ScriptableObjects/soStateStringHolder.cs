using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ActionGameEngine.Data
{
    [CreateAssetMenu(fileName = "StringHolder", menuName = "ScriptableObjects/Statemachine/StringHolder", order = 1)]

    public class soStateStringHolder : ScriptableObject
    {
        private string _noAnimName = "NewState";
        [SerializeField] private string[] _stateNames;
        [SerializeField] private string[] _animNames;

        public void SetStateNames(string[] stateNames) { this._stateNames = stateNames; }
        public void SetAnimNames(string[] animNames) { this._animNames = animNames; }

        public void SetStateName(string stateName, int ind) { this._stateNames[ind] = stateName; }
        public void SetAnimName(string animName, int ind)
        {
            if (ind >= this._animNames.Length)
            {
                List<string> temp = this._animNames.ToList();
                temp.Add("New state");
                this._animNames = temp.ToArray();
            }
            this._animNames[ind] = animName;
        }
        public string GetStateName(int index)
        {
            if (index >= this._stateNames.Length)
            {
                List<string> temp = this._stateNames.ToList();
                temp.Add("New state");
                this._stateNames = temp.ToArray();
            }
            return this._stateNames[index];
        }
        public string GetAnimName(int index)
        {

            if (index >= this._animNames.Length)
            {
                List<string> temp = this._animNames.ToList();
                temp.Add("New state");
                this._animNames = temp.ToArray();
            }
            if (index < 0 || index >= this._animNames.Length)
            {
                return this._noAnimName;
            }
            return this._animNames[index];
        }
    }
}