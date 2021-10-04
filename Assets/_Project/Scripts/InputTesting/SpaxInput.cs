using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spax.Input
{

    [Flags, System.Serializable]
    public enum Direction
    {

        _5 = 1 << 0,//1
        _6 = 1 << 1,//2
        _4 = 1 << 2,//4
        _8 = 1 << 3,//8
        _9 = 1 << 4,//16
        _7 = 1 << 5,//32
        _2 = 1 << 6,//64
        _3 = 1 << 7,//128
        _1 = 1 << 8,//256
    }

    [Flags, System.Serializable]
    public enum Button
    {
        I = 1 << 0,
        J = 1 << 1,
        K = 1 << 2,
        L = 1 << 3,// E instead of D just in case D would get confused with D in Directions
        W = 1 << 4,
        X = 1 << 5,
        Y = 1 << 6,
        Z = 1 << 7,
    }

    [System.Serializable]
    public struct SpaxInput
    {
        public Direction direction;
        public Button buttons;

        public bool IsEqual(SpaxInput other)
        {
            return (this.direction == other.direction) && (this.buttons == other.buttons);
        }

        //integer representation of the x-axis
        public int X()
        {
            if ((direction & (Direction)146) > 0)
            {
                return 1;
            }


            if ((direction & (Direction)292) > 0)
            {
                return -1;
            }

            return 0;
        }

        public static SpaxInput Zero()
        {
            SpaxInput ret;
            ret.buttons = 0;
            ret.direction = 0;
            return ret;
        }

    }
}
