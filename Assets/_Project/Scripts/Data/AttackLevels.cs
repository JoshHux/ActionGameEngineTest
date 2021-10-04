using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class AttackLevels
{

    [SerializeField] private AttackLevelData[] attackData;

}

[System.Serializable]
public struct AttackLevelData
{
    public short hitstop;
    public short blockstun;
    public short hitstun;

}