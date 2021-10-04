using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spax.Input
{

    public enum Moves
    {
        Attacking, LBlazAcc, _4L, Punch, Dash, Jump,
        AirdashB,
        AirdashF,
        AirdashL,
        AirdashNode,
        AirdashR,
        DashB,
        DashF,
        DashL,
        DashNode,
        DashR,
        Airborne,
        BackingUp,
        Idle,
        JumpBack,
        JumpForward,
        JumpLeft,
        JumpNeutral,
        JumpRight,
        Landing,
        Prejump,
        Running,
        StrafeL,
        StrafeR,
        Walking
    }


    [System.Serializable]
    public class MoveList
    {
        [SerializeField]
        private CommandMove[] moveList;

        public CommandMove[] MoveListData
        {
            get => moveList;
            set => moveList = value;
        }

        public void Initialize()
        {
            for (int i = 0; i < moveList.Length; i++)
            {
                moveList[i].Initialize();
            }
        }

        public int FindCommand(ulong[] inputs, bool facingRight, CancelCondition cond)
        {
            for (int i = 0; i < moveList.Length; i++)
            {
                //checks if the move can be cancelled into
                if ((cond & moveList[i].condition) == moveList[i].condition)
                {
                    //Debug.Log("CONDITIONS PASSED " + moveList[i].moveName);
                    int ret = moveList[i].CheckCommand(inputs, facingRight);
                    if (ret > -1)
                    {
                        //Debug.Log("index :: " + i + " , " + ret);
                        return ret;
                    }
                }
            }
            return -1;
        }

        public MoveList DeepCopy()
        {
            MoveList ret = new MoveList();

            //there has to be a better way of doing this...
            int len = this.MoveListData.Length;
            CommandMove[] newArray = new CommandMove[len];
            for (int i = 0; i < len; i++)
            {
                newArray[i] = MoveListData[i].DeepCopy();
            }
            ret.MoveListData = newArray;

            return ret;
        }
    }

    [System.Serializable]
    public class CommandMove
    {
        public Moves Move;
        public InputCodeFlags[] command;
        public int state;
        public CancelCondition condition;

        public void Initialize()
        {
            for (int i = 0; i < command.Length; i++)
            {
                Debug.Log(command[i]);
            }
        }

        public int CheckCommand(ulong[] inputs, bool facingRight)
        {
            //string print = "";
            // i is the index of the command
            int i = 0;
            //for checking how long an input has been charged for
            int chargeTime = 0;
            //how many extra motions you're allowed before you mess up your charge motion
            int chargeTimerLeniancy = 3;
            //string debugMsg = "";
            int len = inputs.Length;
            /*if (len > 0)
            {
                Debug.Log("name :: " + state);

            }*/
            //j is the index of the inputs from the player
            for (int j = 0; j < len; j++)
            {
                //debug += ((InputCodeFlags)inputs[j]) + " | ";
                int fromInput = (int)inputs[j];
                int fromCommand = (int)command[i];
                //right shift to get the smaller value, the one we want
                int inputDuration = (int)(inputs[j] >> 32);
                //Debug.Log("input duration :: " + (int)(inputs[j] >> 32));

                //if the input is 0, then it is invalid
                if (fromInput != 0)
                {

                    //checking charge duration
                    //the way we check for charge duration is by checking for the release fisrt, then counting how long the corresponbding direction has been released for
                    if ((fromCommand & (int)InputCodeFlags.CHARGE) > 0)
                    {
                        //Debug.Log("charge :: " + moveName);
                        //Debug.Log("charge command found :: " + (InputCodeFlags)(fromInput) + " \n " + (InputCodeFlags)(fromCommand ^ (int)InputCodeFlags._30F));
                        fromCommand ^= (int)InputCodeFlags._30F;

                        fromCommand |= (int)InputCodeFlags.RELEASED;
                        //found release, either, start charge timer (if not started) or end charge check
                        bool chargeReleaseFound = ((fromCommand & fromInput) == fromCommand) || InputCode.WorksAs4Way(fromInput, fromCommand);
                        bool freePass = false;
                        //we only care about directional presses, if it doesn't have a direction, it gets a free pass
                        if ((fromInput & (int)(InputCodeFlags._1 | InputCodeFlags._2 | InputCodeFlags._3 | InputCodeFlags._4 | InputCodeFlags._6 | InputCodeFlags._7 | InputCodeFlags._8 | InputCodeFlags._9)) == 0)
                        {
                            freePass = true;
                        }

                        //if timer is not running, then start timer


                        //we only add time if a corresponding direction is pressed
                        if (chargeTime > 0)
                        {
                            if (chargeReleaseFound && (inputDuration > 0))
                            {

                                Debug.Log("charge broken -- released too long :: " + inputDuration);
                                //charge has been released for too long, charge broken
                                return -1;
                            }

                            fromCommand ^= (int)(InputCodeFlags.RELEASED | InputCodeFlags.PRESSED);

                            if (((fromCommand & fromInput) == fromCommand) || InputCode.WorksAs4Way(fromInput, fromCommand) || chargeReleaseFound || freePass)
                            {
                                chargeTime += inputDuration;
                            }
                            else
                            {
                                Debug.Log((InputCodeFlags)fromCommand + " \n" + (InputCodeFlags)fromInput);
                                Debug.Log("charge broken -- released too soon :: " + chargeTime);
                                //different cardinal direction is found, charge broken
                                return -1;
                            }

                            //fully charged
                            if (chargeTime >= 60)
                            {
                                Debug.Log("charged -- " + chargeTime);
                                i++;
                                //this is if the motion and button are pressed on the same frame, stalls by one input
                                j--;
                                if (i == command.Length)
                                {
                                    //Debug.Log(debug);
                                    return state;
                                }
                            }

                        }
                        else if (chargeReleaseFound)
                        {
                            if ((inputDuration > 7))
                            {
                                Debug.Log("charge broken -- released too long :: " + inputDuration);

                                //charge has been released for too long, charge broken
                                return -1;
                            }
                            chargeTime = 1;
                        }
                        //on.y if the charge timer hasn't started yet
                        else
                        {
                            //if there are too many messed up motions or it took too mong to release the charge, then break the charge
                            if (inputDuration > 7 || chargeTimerLeniancy < 0)
                            {
                                Debug.Log("charge motion broken");
                                return -1;
                            }
                            chargeTimerLeniancy--;
                        }




                    }
                    else if (inputDuration > 7)
                    {
                        return -1;
                    }
                    else if ((((fromInput & fromCommand) == fromCommand) || InputCode.WorksAs4Way(fromInput, fromCommand)))
                    {

                        //Debug.Log(inputs[j] + " :: " + command[i] + " || " + i);
                        i++;
                        //this is if the motion and button are pressed on the same frame, stalls by one input
                        j--;
                        if (i == command.Length)
                        {
                            //Debug.Log(debug);
                            return state;
                        }
                    }
                    //makes sure that the most recent inputs match, prevents a scenario where a directional input comes after the button press and triggers the command
                    //since the first element of the input is the current state of the controller
                    else if (j == 1 && i == 0)
                    {

                        //Debug.Log(debug);

                        return -1;
                    }
                    //print += inputs[j] + " || ";

                }
            }
            //Debug.Log(debug);

            //Debug.Log(print);
            return -1;
        }

        public CommandMove DeepCopy()
        {
            CommandMove ret = new CommandMove();
            ret.Move = this.Move;
            ret.state = this.state;
            ret.condition = this.condition;

            //I really am starting to hate doing this
            int len = this.command.Length;
            InputCodeFlags[] newArray = new InputCodeFlags[len];
            for (int i = 0; i < len; i++)
            {
                newArray[i] = command[i];
            }
            ret.command = newArray;

            return ret;
        }
    }

    [Flags]
    public enum CancelCondition
    {
        //move cancelables
        NORMAL = 1 << 0,
        COMMAND_NORMAL = 1 << 1,
        SPECIAL = 1 << 2,
        EX_SPECIAL = 1 << 3,
        SUPER = 1 << 4,
        JUMP = 1 << 5,
        MOVEMENT = 1 << 6,
        GUARD = 1 << 7,
        GROUNDED = 1 << 8,
        NORM_LV1 = 1 << 9,
        NORM_LV2 = 1 << 10,
        NORM_LV3 = 1 << 11,
        NORM_LV4 = 1 << 12,
        NORM_LV5 = 1 << 13,
        NORM_LV6 = 1 << 14,
        DASH = 1 << 15,

        AIRBORNE = 1 << 16,

        CAUSE_FOR_BLOCK = 1 << 17,

        GRAB = 1 << 18,

        UNIQUE = 1 << 19,
    }



}