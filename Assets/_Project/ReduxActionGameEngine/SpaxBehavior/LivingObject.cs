using ActionGameEngine.Enum;
using BEPUUnity;
using BEPUutilities;
using Spax;
using UnityEngine;

namespace ActionGameEngine
{
    //object that can move around and detect physics
    public abstract class LivingObject : SpaxBehavior
    {
        //rigidbody that we will use to move around and collide with the environment
        protected ShapeBase rb;

        //velocity calculated that we will apply to our rigidbody
        protected BepuVector3 calcVel;

        //for things such as setting velocity, makes sure that that velocity is always being applied
        protected BepuVector4 storedVel;

        public int OnHitConnect(HitType type)
        {
            //check to see if invuln matches
            //checks to see if hit is strike box
            if (EnumHelper.HasEnum((int) type, (int) HitType.STRIKE))
            {
                //checks to see if unblockable or not
                if (EnumHelper.HasEnum((int) type, (int) HitType.UNBLOCKABLE))
                {
                    return 1;
                }

                //blockable hit
                {
                    //replace with block operations like checking if they're blocking the right way
                    bool isBlocking = true;

                    if (isBlocking)
                    {
                        //set chip, blockstun, blockstop, etc.
                        return 2;
                    }
                    else
                    {
                        //set damage, hitstun, hitstop, etc.
                        if (EnumHelper.HasEnum((int) type, (int) HitType.GRAB))
                        {
                            //this is where you would also put your operations for hitgrabs
                        }
                        return 1;
                    }
                }
            }
            else if (EnumHelper.HasEnum((int) type, (int) HitType.GRAB))
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
