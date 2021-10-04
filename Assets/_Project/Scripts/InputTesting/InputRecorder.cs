using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spax.Input
{
    [System.Serializable]
    public class InputRecorder
    {
        public InputStorage prevInput;

        public void Initialize()
        {
            prevInput = new InputStorage();
        }

        public bool RecordInput(SpaxInput input, bool inStop = false)
        {
            return prevInput.StoreInput(input, inStop);
        }

        public ulong[] GetInputCodes()
        {
            return prevInput.GetReverseCodeArray();
        }

        public Button GetPressedButtons()
        {
            return prevInput.GetChangedPressed();
        }

        public InputCodeFlags GetLatestCode()
        {
            return prevInput.GetLatestCode();
        }
    }

    [System.Serializable]
    public class InputStorage
    {
        [SerializeField]
        private SpaxInput[] prevInputs;
        [SerializeField]
        private InputCode[] prevInputsCode;
        [SerializeField]
        private int arrayPos;
        [SerializeField]
        private int codeArrayPos;
        private SpaxInput changedPressed;

        private SpaxInput changedReleased;

        public SpaxInput[] PrevInputs
        {
            get => prevInputs;
            set => prevInputs = value;
        }

        public InputCode[] PrevInputsCode
        {
            get => prevInputsCode;
            set => prevInputsCode = value;
        }

        public int ArrayPos
        {
            get => arrayPos;
            set => arrayPos = value;
        }

        public int CodeArrayPos
        {
            get => codeArrayPos;
            set => codeArrayPos = value;
        }

        public InputStorage()
        {
            prevInputs = new SpaxInput[10];
            prevInputsCode = new InputCode[127];
            for (int i = 0; i < 20; i++)
            {
                prevInputsCode[i].framesHeld = 0;
                prevInputsCode[i].code = "";
            }
            arrayPos = 0;
            codeArrayPos = 0;

        }



        private int IncrementByPos(int index, bool byCodeArray)
        {

            int bound = 0;
            if (byCodeArray)
            {
                bound = prevInputsCode.Length - 1;
            }
            else
            {
                bound = prevInputs.Length - 1;
            }

            if (++index > bound)
            {
                index = 0;
            }
            return index;
        }

        private int DecrementByPos(int index, bool byCodeArray)
        {

            int bound = 0;
            if (byCodeArray)
            {
                bound = prevInputsCode.Length - 1;
            }
            else
            {
                bound = prevInputs.Length - 1;
            }

            if (--index < 0)
            {
                index = bound;
            }
            return index;
        }

        //returns true if input was successfully recorded
        public bool StoreInput(SpaxInput input, bool inStop = false)
        {
            SpaxInput curInput = prevInputs[arrayPos];
            bool ret = false;

            if (!curInput.IsEqual(input))
            {

                changedPressed.buttons = (curInput.buttons ^ input.buttons) & input.buttons;
                changedPressed.direction = (curInput.direction ^ input.direction) & input.direction;


                changedReleased.buttons = (curInput.buttons ^ input.buttons) & curInput.buttons;
                changedReleased.direction = (curInput.direction ^ input.direction) & curInput.direction;


                //Debug.Log(input.direction + " " + input.buttons);
                arrayPos = this.IncrementByPos(arrayPos, false);
                prevInputs[arrayPos] = input;
                this.SetInputCode();
                ret = true;
            }

            if (prevInputsCode[codeArrayPos].framesHeld < 128)
            {
                prevInputsCode[codeArrayPos].framesHeld++;
            }
            return ret;
        }

        private void SetInputCode()
        {
            SpaxInput curInput = prevInputs[arrayPos];

            //records released inputs
            int change = 0;
            if (changedReleased.direction > Direction._5 || changedReleased.buttons != 0)
            {

                change = (((int)changedReleased.direction & 510) << 2) | ((int)changedReleased.buttons << 11);

                //Debug.Log("release :: " + (InputCodeFlags)change);


                int checkPrev = ((int)prevInputsCode[codeArrayPos].inputCode >> 3);
                int checkCode = change >> 3;

                if (checkPrev == checkCode)
                {
                    change |= 1;
                }

                change |= 2;


                codeArrayPos = this.IncrementByPos(codeArrayPos, true);
                prevInputsCode[codeArrayPos].inputCode = change;
                prevInputsCode[codeArrayPos].framesHeld = 0;
            }

            //records pressed inputs
            change = 0;

            if (changedPressed.direction > Direction._5 || changedPressed.buttons != 0)
            {

                change = (((int)changedPressed.direction & 510) << 2) | ((int)changedPressed.buttons << 11);

                //Debug.Log("press :: " + (InputCodeFlags)change);

                int checkPrev = ((int)prevInputsCode[codeArrayPos].inputCode >> 3);
                int checkCode = change >> 3;

                if ((checkPrev ^ checkCode) == 0)
                {
                    change |= 1;
                }

                change |= 4;


                codeArrayPos = this.IncrementByPos(codeArrayPos, true);
                prevInputsCode[codeArrayPos].inputCode = change;
                prevInputsCode[codeArrayPos].framesHeld = 0;
            }

        }

        public ulong[] GetReverseCodeArray()
        {
            List<ulong> ret = new List<ulong>();

            int index = codeArrayPos;
            int endInd = this.IncrementByPos(codeArrayPos, true);

            string debugMsg = "";

            //add the current controller condition purely for command normals
            ulong toAdd = (ulong)((((int)prevInputs[arrayPos].direction & 510) << 2) | ((int)prevInputs[arrayPos].buttons << 11));
            debugMsg += (InputCodeFlags)toAdd + " | ";

            ret.Add(toAdd);


            //guarentees at least 2 elements
            //do
            //records all inputs to send (for charge inputs)
            while (index != endInd) //&& prevInputsCode[index].framesHeld <= 7)

            {
                //first 4 bytes is the input code, next 4 bytes is the duration held
                //left shift makes it bigger, right makes it smaller, big endian
                toAdd = ((ulong)prevInputsCode[index].framesHeld << 32) | ((uint)prevInputsCode[index].inputCode);
                //ulong msgDebug = (((ulong)prevInputsCode[index].framesHeld));
                //Debug.Log("recording :: " + (msgDebug) + "\n came out as :: " +  (toAdd >> 32));
                if (toAdd < 1)
                {
                    //Debug.Log("broken");
                    break;
                }

                debugMsg += (InputCodeFlags)toAdd + " | ";

                ret.Add(toAdd);
                //Debug.Log("making array: " + toAdd.Length);

                index = this.DecrementByPos(index, true);
            }
            //Debug.Log(debugMsg);
            return ret.ToArray();
        }

        public Button GetChangedPressed()
        {
            return changedPressed.buttons;
        }

        public InputCodeFlags GetLatestCode()
        {
            return (InputCodeFlags)prevInputsCode[codeArrayPos].inputCode;
        }

    }

    [System.Serializable]
    public struct InputCode
    {
        public uint framesHeld;
        public string code;
        public int inputCode;

        //call this to flip backwards and forwards for the inputcode
        public static int FlipBackForth(int code)
        {
            //exact integer value to mask every left/right combination
            int mask = 1752;
            //Debug.Log(mask);

            //has the left/right directional input from the command
            int maskHelper = (mask & code);

            //removes the left/right directional input
            code -= maskHelper;

            mask = (maskHelper << 1 | maskHelper >> 1) & mask;
            code |= mask;
            //Debug.Log("facing left :: " + (InputCodeFlags)mask);
            return code;
        }

        //assumes the direction in command only has a cardinal
        //checks if the input relative to the 4 cardinal directions is correct
        //checks if _1 is good for D
        public static bool WorksAs4Way(int code, int command)
        {
            if ((command & 1 << 19) > 0)
            {
                //removes the 4-way condition flag
                command ^= 1 << 19;
                //removes the direction
                int mask = 2040;
                //Debug.Log("pre check :: " + (InputCodeFlags)mask+" "+(InputCodeFlags) code);

                //Debug.Log((command & code) + " || " + (command & ~mask)+ " || " + (int.MaxValue ^mask)+ " || " + ( ~mask)+ " || " + ((InputCodeFlags)command)+ " || " + ((InputCodeFlags)code));
                //Debug.Log("pre check :: "+(InputCodeFlags) code);
                //Debug.Log("pre check :: "+(InputCodeFlags) (command & ~mask)+" || "+(InputCodeFlags)(command & code));
                if (((command & code) & ~mask) == (command & ~mask))
                {
                    //Debug.Log("passed :: " + (InputCodeFlags)code);
                    //Debug.Log(mask);
                    //when using, make sure from command has a cardinal direction
                    //for up and down
                    if ((command & 288) > 0)
                    {
                        //mask for all up/down positions
                        mask = command & 288;
                        mask |= mask | mask << 1 | mask << 2;
                    }
                    //for left and right
                    else if ((command & 24) > 0)
                    {
                        mask = command & 24;
                        mask |= mask | mask << 3 | mask << 6;

                    }
                    else
                    {
                        return false;
                    }


                    //Debug.Log("facing left :: " + (InputCodeFlags)mask);
                    //Debug.Log(((InputCodeFlags) mask)+" || "+((InputCodeFlags) code)+" || "+( code));
                    return (code & mask) > 0;
                }
            }
            return false;
        }
    }
}

//using neither RELEASED not PRESSED flag indicates command normal
[Flags]
public enum InputCodeFlags
{
    NO_INTERRUPTS = 1 << 0,
    RELEASED = 1 << 1,
    PRESSED = 1 << 2,
    _6 = 1 << 3,//2
    _4 = 1 << 4,//4
    _8 = 1 << 5,//8
    _9 = 1 << 6,//16
    _7 = 1 << 7,//32
    _2 = 1 << 8,//64
    _3 = 1 << 9,//128
    _1 = 1 << 10,//256
    I = 1 << 11,
    J = 1 << 12,
    K = 1 << 13,
    L = 1 << 14,// E instead of _2 just in case _2 would get confused with _2 in Directions
    W = 1 << 15,
    X = 1 << 16,
    Y = 1 << 17,
    Z = 1 << 18,
    READ_AS_4WAY = 1 << 19,
    //only for transitioning states, doesn't have to do with input motions
    CURRENTLY_HELD = 1 << 20,
    CHARGE = 1 << 21,
    _30F = 1 << 21 | 1 << 22,

}
