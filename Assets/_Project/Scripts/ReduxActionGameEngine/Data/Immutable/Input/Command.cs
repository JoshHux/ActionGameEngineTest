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
                        //It's strict if we don't have the lenient flag
                        bool isStrict = !EnumHelper.HasEnum((uint)frag.flags, (uint)InputFlags.ANY_IS_OKAY);
                        //UnityEngine.Debug.Log(isStrict + " " + j);

                        //add a flag to the checked flag, makes it so it passes the flag check
                        if (isStrict)
                        {
                            inputFrag.flags |= InputFlags.ANY_IS_OKAY;
                        }

                        //from the actual command, not from player
                        bool _4way = EnumHelper.HasEnum((uint)persisFlags, (uint)InputFlags.DIR_AS_4WAY);
                        //UnityEngine.Debug.Log(isStrict + " " + j);

                        //add a flag to the checked flag, makes it so it passes the flag check
                        if (_4way)
                        {
                            inputFrag.flags |= InputFlags.DIR_AS_4WAY;
                        }

                        //from the actual command, not from player
                        bool isUp = EnumHelper.HasEnum((uint)persisFlags, (uint)InputFlags.CHECK_IS_UP);
                        //UnityEngine.Debug.Log(isStrict + " " + j);

                        //add a flag to the checked flag, makes it so it passes the flag check
                        if (isUp)
                        {
                            inputFrag.flags |= InputFlags.CHECK_IS_UP;
                        }


                        //special case where we only want to look at the currently held inputs by the player
                        //only check the first element if we don't have any flags
                        //in this case, we want to check the current inputs the player is pressing
                        if (i == 0 && j == 0 && EnumHelper.HasEnum((uint)frag.flags, (uint)InputFlags.CHECK_CONTROLLER_STATE, true))
                        {
                            inputFrag.flags |= InputFlags.CHECK_CONTROLLER_STATE;
                        }

                        //how many frames of leniency we have in the motion
                        int leniency = 10;
                        //if it's the first input and we're looking at the first element, apply first input leniency
                        if (i == 0 && j == 0) { leniency = 3; }

                        //the input is not fast enough, held too long or something like that
                        if (input.framesHeld > leniency)
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
                                    inputFrag.inputItem.m_rawValue |= (ushort)(inputFragPrev.inputItem.m_rawValue & (0b1111111111000000));
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
                                bool unbrokenCharge = flaglessFrag.Check(inputFrag, false, isStrict);
                                //remove flags to straight check the direction
                                flaglessFrag.flags = 0;

                                //mask flags and item to make sure it
                                //nested loop to soft go through

                                for (int k = i + 1; k < lenNested; k++)
                                {
                                    RecorderElement charge = playerInputs[k];
                                    InputFragment chargeFrag = input.frag;

                                    //free pass
                                    unbrokenCharge = flaglessFrag.Check(chargeFrag, false, isStrict) && ((chargeFrag.flags & InputFlags.PRESSED) > 0) && ((heldTime + charge.framesHeld) >= minHeldTime);
                                    //charge time completed, break out of loop
                                    if (unbrokenCharge) { break; }

                                    if ((lenNested - k) > 1)
                                    {
                                        RecorderElement chargePrev = playerInputs[k + 1];
                                        InputFragment chargePrevFrag = input.frag;
                                        //if it's the same direction, and it's is going from released to pressed
                                        unbrokenCharge = !(((chargePrevFrag.inputItem.m_rawValue & 0b0000000000111111) == (chargeFrag.inputItem.m_rawValue & 0b0000000000111111)) && ((chargePrevFrag.flags & InputFlags.RELEASED) > 0) && ((chargeFrag.flags & InputFlags.PRESSED) > 0));
                                        //would make the check true if charge frag passes the check and there isn't a weird corner case where you press down->down
                                        unbrokenCharge = flaglessFrag.Check(chargeFrag, false, isStrict) && unbrokenCharge;

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
                        if (frag.Check(inputFrag, isUp, isStrict))
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