using ActionGameEngine.Enum;
using Spax;
using UnityEngine;

namespace ActionGameEngine
{
    //object that can move around and detect physics
    public class LivingObject : SpaxBehavior
    {
        public void OnHitConnect(HitType type)
        {
            //check to see if invuln matches
            //checks to see if hit is strike box
            if (EnumHelper.HasEnum((int) type, (int) HitType.STRIKE))
            {
                //checks to see if unblockable or not
                if (EnumHelper.HasEnum((int) type, (int) HitType.UNBLOCKABLE))
                {
                }
                else
                //blockable hit
                {
                    //replace with block operations like checking if they're blocking the right way
                    bool isBlocking = true;

                    if (isBlocking)
                    {
                        //set chip, blockstun, blockstop, etc.
                    }
                    else
                    {
                        //set damage, hitstun, hitstop, etc.
                        if (EnumHelper.HasEnum((int) type, (int) HitType.GRAB))
                        {
                            //this is where you would also put your operations for hitgrabs
                        }
                    }
                }
            }
            else if (EnumHelper.HasEnum((int) type, (int) HitType.GRAB))
            {
                //grab operations, like setting parent and the such
            }
            else
            {
                Debug
                    .LogError("Invalid HitType, must have GRAB or STRIKE tag");
            }
        }
    }
}
