using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionGameEngine.Enum
{
    [System.Flags]
    //type of hit hitbox is, IE. unblockable, grab, strike, low, etc.
    //1 byte
    public enum HitType : byte
    {
        STRIKE = 1 << 0,
        //If paired with GRAB tag: not be able to be blocked with the corresponding position (IE. AIR only hits airborne, GROUND only hits grounded)
        //if paired with STRIKE tag: the move will only be ground OR air unblockable (IE. AIR is only unblockable in the air, but blockable on the ground)
        UNBLOCKABLE = 1 << 1,
        LOW = STRIKE | 1 << 2,
        MIDDLE = STRIKE | 1 << 3,
        HIGH = STRIKE | 1 << 4,
        AIR = UNBLOCKABLE | 1 << 5,
        GROUND = UNBLOCKABLE | 1 << 6,
        GRAB = 1 << 7
    }
}
