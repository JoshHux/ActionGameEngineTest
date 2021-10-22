using ActionGameEngine.Data;
using ActionGameEngine.Gameplay;
using ActionGameEngine.Enum;
using UnityEngine;

namespace ActionGameEngine
{
    public class ActionCharacterController : ControllableObject
    {

        public override int GetHit(HitboxData boxData)
        {
            return OnGetHit(boxData.type);
        }

        public override int ConnectedHit(HitboxData boxData)
        {
            return -1;
        }

        public int OnGetHit(HitType type)
        {
            //check to see if invuln matches
            //checks to see if hit is strike box
            if (EnumHelper.HasEnum((int)type, (int)HitType.STRIKE))
            {
                //checks to see if unblockable or not
                if (EnumHelper.HasEnum((int)type, (int)HitType.UNBLOCKABLE))
                {
                    //blockable hit

                    //replace with block operations like checking if they're blocking the right way
                    bool isBlocking = true;

                    if (isBlocking)
                    {
                        //set chip, blockstun, blockstop, etc.
                        return 2;
                    }
                    else
                    {
                        if (EnumHelper.HasEnum((int)type, (int)HitType.GRAB))
                        {
                            //this is where you would also put your operations for hitgrabs
                        }
                    }
                }
                //set damage, hitstun, hitstop, etc.
                return 1;

            }
            else if (EnumHelper.HasEnum((int)type, (int)HitType.GRAB))
            {
                //grab operations, like setting parent and the such
                return 3;
            }
            else
            {
                Debug.LogError("Invalid HitType, must have GRAB or STRIKE tag");
            }

            return -1;
        }
    }
}