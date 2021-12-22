using System;
using ActionGameEngine.Enum;

namespace ActionGameEngine.Input
{
    [System.Serializable]
    //4+3*x bytes
    public struct Command
    {
        public InputFragment[] commandInput;

        public Command(InputFragment[] inf)
        {
            commandInput = inf;
        }

        public bool Check(RecorderElement[] playerInputs, int facing)
        {
            int len = commandInput.Length;
            int lenNested = playerInputs.Length;
            //if there are no elements to check, just pass it
            if (len > 0 && lenNested > 0)
            {
                int pos = 0;
                for (int j = 0; j < len; j++)
                {
                    int totalFramesPassed = 0;
                    InputFragment frag = commandInput[j];
                    InputFlags persisFlags = frag.flags;

                    //we need to do an indexing for loop because we want to save our position as we move to the next frag
                    for (int i = pos; i < lenNested; i++)
                    {
                        RecorderElement input = playerInputs[i];
                        input.frag.inputItem.MultX(facing);
                        InputFragment inputFrag = input.frag;

                        //special case where we only want to look at the currently held inputs by the player
                        //only check the first element if we don't have any flags
                        //in this case, we want to check the current inputs the player is pressing
                        if (i == 0 && j == 0 && (EnumHelper.HasEnum((uint)frag.flags, (uint)InputFlags.CHECK_IS_UP) || (frag.flags == 0)))
                        {
                            //fragment from the command to check against
                            InputFragment toCheck = frag;
                            toCheck.flags = 0;

                            bool checkNot = EnumHelper.HasEnum((uint)frag.flags, (uint)InputFlags.CHECK_IS_UP);
                            bool isStrict = EnumHelper.HasEnum((uint)frag.flags, (uint)InputFlags.STRICT);

                            //UnityEngine.Debug.Log("pass currently held items");
                            //UnityEngine.Debug.Log(toCheck.inputItem.m_rawValue + " " + inputFrag.inputItem.m_rawValue + " | " + toCheck.flags + " " + inputFrag.flags);
                            return toCheck.Check(inputFrag, checkNot, isStrict);
                        }

                        //the input is not fast enough, held too long or something like that
                        if (input.framesHeld > 10)
                        {
                            //UnityEngine.Debug.Log("too many frames passed");
                            return false;
                        }
                        //UnityEngine.Debug.Log(inputFrag.flags + " -- " + inputFrag.inputItem.m_rawValue);


                        //if there are any flags where we would want to rewind back in time to add leniency or make sure is correct
                        if (EnumHelper.HasEnum((uint)persisFlags, (int)InputFlags.NEED_PREV) && ((lenNested - i) > 1))
                        {
                            RecorderElement inputPrev = playerInputs[i + 1];
                            InputFragment inputFragPrev = inputPrev.frag;
                            //if we need to press buttons simultaneously, give it a little leniency
                            if (EnumHelper.HasEnum((uint)persisFlags, (int)InputFlags.BTN_SIMUL_PRESS))
                            {
                                if (Math.Abs(input.framesHeld - inputPrev.framesHeld) <= 3)
                                {
                                    inputFrag.inputItem.m_rawValue |= (short)(inputFragPrev.inputItem.m_rawValue & (0b1111111111000000));
                                }
                                //attatch the appropriate flag
                                inputFrag.flags |= InputFlags.BTN_SIMUL_PRESS;
                            }

                            //if we want to see if there's an interrupt, just move back and compare
                            if (EnumHelper.HasEnum((uint)persisFlags, (int)InputFlags.NO_INTERRUPT))
                            {
                                //only non-interrupt if the flagless items match, since we're only tracking change
                                if (inputFrag.inputItem == inputFragPrev.inputItem)
                                {
                                    //attatch the appropriate flag
                                    inputFrag.flags |= InputFlags.NO_INTERRUPT;
                                }
                            }

                            //if we want to see if there's a hold/charge, loop through and see if we can find it
                            if (EnumHelper.HasEnum((uint)persisFlags, (int)InputFlags.HELD))
                            {
                                //attatch the appropriate flag
                                inputFrag.flags |= InputFlags.DIR_AS_4WAY;
                                //calculate the appropriate charge time
                                int minHeldTime = 0;
                                int heldTime = 0;

                                if (EnumHelper.HasEnum((uint)persisFlags, (int)InputFlags.HELD_30F))
                                { minHeldTime += 30; }

                                //just to go through and check without charge flag to make sure the other inputs match
                                InputFragment flaglessFrag = new InputFragment(frag.inputItem, frag.flags ^ (InputFlags.HELD & frag.flags));
                                bool unbrokenCharge = flaglessFrag.Check(inputFrag, false);
                                //remove flags to straight check the direction
                                flaglessFrag.flags = 0;

                                //mask flags and item to make sure it
                                //nested loop to soft go through

                                for (int k = i + 1; k < lenNested; k++)
                                {
                                    RecorderElement charge = playerInputs[k];
                                    InputFragment chargeFrag = input.frag;

                                    //free pass
                                    unbrokenCharge = flaglessFrag.Check(chargeFrag, false) && ((chargeFrag.flags & InputFlags.PRESSED) > 0) && ((heldTime + charge.framesHeld) >= minHeldTime);
                                    //charge time completed, break out of loop
                                    if (unbrokenCharge) { break; }

                                    if ((lenNested - k) > 1)
                                    {
                                        RecorderElement chargePrev = playerInputs[k + 1];
                                        InputFragment chargePrevFrag = input.frag;
                                        //if it's the same direction, and it's is going from released to pressed
                                        unbrokenCharge = !(((chargePrevFrag.inputItem.m_rawValue & 0b0000000000111111) == (chargeFrag.inputItem.m_rawValue & 0b0000000000111111)) && ((chargePrevFrag.flags & InputFlags.RELEASED) > 0) && ((chargeFrag.flags & InputFlags.PRESSED) > 0));
                                        //would make the check true if charge frag passes the check and there isn't a weird corner case where you press down->down
                                        unbrokenCharge = flaglessFrag.Check(chargeFrag, false) && unbrokenCharge;

                                    }
                                    //check to make sure we didn't mark something like 2->1 as broken
                                    //characteristics of broken charges
                                    //released charge dir to press the same dir: 2->~2->2 look for uninterrupted ~2->2
                                    //we're looking for a pressed/released input from the wrong direction


                                    //only true of charge broken
                                    if (!unbrokenCharge)
                                    {
                                        return false;
                                    }

                                    //reached if passed check
                                    heldTime += charge.framesHeld;

                                }
                                //found the appropriate charge, let's break to skip over the rest of the irrelevant query

                                //next starting position of the player's inputs
                                pos = i;
                                //breaks out of nested loop only
                                break;
                            }

                        }


                        //check the flag to see if it checks out
                        if (frag.Check(inputFrag, false))
                        {
                            //UnityEngine.Debug.Log(inputFrag.inputItem.m_rawValue + " " + frag.inputItem.m_rawValue);

                            //next starting position of the player's inputs
                            pos = i;
                            //breaks out of nested loop only
                            break;
                        }

                        //reached the end of the player's inputs, no reason to continue
                        //only reasched if player's inputs failed to match to command
                        if (i == lenNested - 1)
                        {
                            return false;
                        }

                        //only reached if we didn't find a matching input
                        totalFramesPassed += input.framesHeld;

                    }

                }
            }

            return true;
        }
    }
}