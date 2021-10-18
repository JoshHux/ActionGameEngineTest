using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionGameEngine.Enum
{
    [System.Flags]
    //type of hit hitbox is, IE. unblockable, grab, strike, low, etc.
    //1 byte
    public enum HitType : ushort
    {
        STRIKE = 1 << 0,
        GRAB = 1 << 1,

        LOW = STRIKE | 1 << 2,
        MIDDLE = STRIKE | 1 << 3,
        HIGH = STRIKE | 1 << 4,
        //If paired with GRAB tag: not be able to be blocked with the corresponding position (IE. AIR only hits airborne, GROUND only hits grounded)
        //if paired with STRIKE tag: the move will only be ground OR air unblockable (IE. AIR is only unblockable in the air, but blockable on the ground)
        UNBLOCKABLE_AIR = 1 << 5,
        UNBLOCKABLE_GROUND = 1 << 6,
        UNBLOCKABLE_TRUE = 1 << 7,
        UNBLOCKABLE = UNBLOCKABLE_AIR | UNBLOCKABLE_GROUND | UNBLOCKABLE_TRUE,
    }
}
