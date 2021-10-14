using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;

namespace ActionGameEngine.Data
{
    //static struct to keep all of our character's stats, move list, and other immutable data
    [System.Serializable]
    public struct CharacterData
    {
        public int maxHealth;

        public Fix64 mass;

        public StateData[] stateList;
    }
}
