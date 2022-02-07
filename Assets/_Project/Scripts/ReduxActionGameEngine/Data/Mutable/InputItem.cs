using ActionGameEngine.Enum;
using FixMath.NET;
namespace ActionGameEngine.Input
{
    //represents one instance of an input
    [System.Serializable]
    public struct InputItem
    {
        //1<<0 x sign (1)
        //1<<1 x val (2)
        //1<<2 y sign (4)
        //1<<3 y val (8)
        //1<<4 x half (16)
        //1<<5 y half (32)
        //1<<6 btn A (64) face west btn
        //1<<7 btn B (128) face north btn
        //1<<8 btn C (256) r1
        //1<<9 btn D (512) r2
        //1<<10 btn W (1024) face south btn
        //1<<11 btn X (2048) face east btn
        //1<<12 btn Y (4096) l1
        //1<<13 btn Z (8192) l2
        //1<<14 btn start (16384)
        //1<<15 btn select (32768)
        public ushort m_rawValue;

        public InputItem(ushort newRaw)
        {
            m_rawValue = newRaw;
        }

        //for reading from json, so making commands can be more user-readable
        public InputItem(DigitalInput npt)
        {
            m_rawValue = 0;

            //if has a nonzero x value
            if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.X_NONZERO))
            {
                m_rawValue |= 1 << 1;
                //if x value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.X_NEGATIVE))
                {
                    m_rawValue |= 1 << 0;
                }
            }

            //if has a nonzero y value
            if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.Y_NONZERO))
            {
                m_rawValue |= 1 << 3;
                //if y value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.Y_NEGATIVE))
                {
                    m_rawValue |= 1 << 2;
                }
            }
        }

        public DigitalInput ToDigitalInput()
        {
            DigitalInput ret = 0;

            //x is nonzero
            if (this.X() != 0)
            {
                ret = DigitalInput.X_NONZERO;
                //x is positive
                if (this.X() > 0)
                {
                    ret &= DigitalInput.X_POSITIVE;
                }
                else
                {
                    ret &= DigitalInput.X_NEGATIVE;
                }
            }
            else
            {
                ret = DigitalInput.X_ZERO;
            }

            //y is nonzero
            if (this.Y() != 0)
            {
                ret |= DigitalInput.Y_NONZERO;
                //y is positive
                if (this.Y() > 0)
                {
                    ret &= DigitalInput.Y_POSITIVE;
                }
                else
                {
                    ret &= DigitalInput.Y_NEGATIVE;
                }
            }
            else
            {
                ret &= DigitalInput.Y_ZERO;
            }
            return ret;
        }

        public Fix64 X()
        {
            Fix64 ret = Fix64.Zero;
            //x is nonzero
            if ((m_rawValue & (1 << 1)) > 0)
            {
                ret = Fix64.One;
                //halve y?
                if ((m_rawValue & (1 << 4)) > 0)
                {
                    ret *= (Fix64)0.5;
                }
                //negative?
                if ((m_rawValue & (1 << 0)) > 0)
                {
                    ret *= -1;
                }

            }
            return ret;
        }

        public Fix64 Y()
        {
            Fix64 ret = Fix64.Zero;
            //y is nonzero
            if ((m_rawValue & (1 << 3)) > 0)
            {
                ret = Fix64.One;
                //halve y?
                if ((m_rawValue & (1 << 5)) > 0)
                {
                    ret *= (Fix64)0.5;
                }
                //negative?
                if ((m_rawValue & (1 << 2)) > 0)
                {
                    ret *= -1;
                }

            }
            return ret;
        }

        public static InputItem FindReleased(InputItem current, InputItem previous)
        {
            //DigitalInput cur = current.ToDigitalInput();
            int cur = current.m_rawValue;
            //DigitalInput prev = previous.ToDigitalInput();
            int prev = previous.m_rawValue;
            //get differences, then only get the difference from prev
            int part = (cur ^ prev) & prev;

            return new InputItem((ushort)part);
        }


        public static InputItem FindPressed(InputItem current, InputItem previous)
        {
            //DigitalInput cur = current.ToDigitalInput();
            int cur = current.m_rawValue;
            //DigitalInput prev = previous.ToDigitalInput();
            int prev = previous.m_rawValue;
            //get differences, then only get the difference from cur
            int part = (cur ^ prev) & cur;

            return new InputItem((ushort)part);
        }

        public static DigitalInput ToDigInp(short rawVal)
        {
            Fix64 x = 0;
            Fix64 y = 0;
            DigitalInput ret = 0;


            //x is nonzero
            if ((rawVal & (1 << 1)) > 0)
            {
                x = Fix64.One;
                //halve y?
                if ((rawVal & (1 << 4)) > 0)
                {
                    x *= (Fix64)0.5;
                }
                //negative?
                if ((rawVal & (1 << 0)) > 0)
                {
                    x *= -1;
                }

            }

            //y is nonzero
            if ((rawVal & (1 << 3)) > 0)
            {
                y = Fix64.One;
                //halve y?
                if ((rawVal & (1 << 5)) > 0)
                {
                    y *= (Fix64)0.5;
                }
                //negative?
                if ((rawVal & (1 << 2)) > 0)
                {
                    y *= -1;
                }

            }

            DigitalInput xVal = DigitalInput.X_ZERO;
            //x is nonzero
            if (x != 0)
            {
                xVal = DigitalInput.X_NONZERO;
                //x is positive
                if (x > 0)
                {
                    xVal &= DigitalInput.X_POSITIVE;
                }
                else
                {
                    xVal &= DigitalInput.X_NEGATIVE;
                }
            }

            DigitalInput yVal = DigitalInput.Y_ZERO;
            //y is nonzero
            if (y != 0)
            {
                yVal = DigitalInput.Y_NONZERO;
                //y is positive
                if (y > 0)
                {
                    yVal &= DigitalInput.Y_POSITIVE;
                }
                else
                {
                    yVal &= DigitalInput.Y_NEGATIVE;
                }
            }

            ushort btnMask = 0b1111111111000000;
            short btn = (short)((rawVal & btnMask) << 2);

            ret |= yVal & xVal | (DigitalInput)btn;

            return ret;
        }

        public static short DigToRaw(DigitalInput npt)
        {
            short m_rawValue = 0;

            //if has a nonzero x value
            if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.X_NONZERO))
            {
                m_rawValue |= 1 << 1;
                //if x value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.X_NEGATIVE))
                {
                    m_rawValue |= 1 << 0;
                }
            }

            //if has a nonzero y value
            if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.Y_NONZERO))
            {
                m_rawValue |= 1 << 3;
                //if y value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((uint)npt, (int)DigitalInput.Y_NEGATIVE))
                {
                    m_rawValue |= 1 << 2;
                }
            }

            m_rawValue |= (short)((int)(npt & DigitalInput.BUTTONS) >> 2);

            return m_rawValue;
        }


        //for inputs and checking commands
        public bool Check(InputItem other, bool read4way)
        {
            bool dirCheck = false;
            bool btnCheck = false;
            if (read4way)
            {
                dirCheck = ((this.X() * other.X()) > 0) || ((this.Y() * other.Y()) > 0);
            }
            else
            {
                //strict match
                dirCheck = ((other.m_rawValue & this.m_rawValue) & (0b0000000000111111)) == (this.m_rawValue & (0b0000000000111111));
            }

            btnCheck = ((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (this.m_rawValue & (0b1111111111000000));


            return dirCheck && btnCheck;
        }

        public void MultX(int mult)
        {
            //get sign of the integer we want to multiply
            int sign = mult >> 31;
            //get the x value of the input
            int xVal = this.m_rawValue >> 1;
            //and the sign and the xVal to make sure we don't apply a sign change to 0
            //both values should be 1 if we want a potential sign change
            //should be 1 or 0
            ushort newSign = (ushort)(sign & xVal);

            //xor the sign bit, flips 0 to 1 (pos to neg), flips 1 to 0 (neg to pos)
            m_rawValue ^= newSign;
        }

        public bool Check(InputItem other, bool read4way, bool checkNot, bool superStrict = false)
        {
            bool dirCheck = false;
            bool btnCheck = false;
            if (checkNot)
            {

                if (read4way)
                {
                    //UnityEngine.Debug.Log("doing something - " + ((this.X() * other.X()) > 0) + " - " + ((this.Y() * other.Y()) > 0) + " btn - " + (((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (this.m_rawValue & (0b1111111111000000))));
                    dirCheck = ((this.X() * other.X()) <= 0) && ((this.Y() * other.Y()) <= 0);
                    btnCheck = ((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (this.m_rawValue & (0b1111111111000000));
                }
                else if (superStrict)
                {
                    dirCheck = ((other.m_rawValue & 0b0000000000111111) != (this.m_rawValue & 0b0000000000111111)) || ((this.m_rawValue & 0b0000000000111111) == 0);
                    btnCheck = ((other.m_rawValue & 0b1111111111000000) != (this.m_rawValue & 0b1111111111000000)) || ((this.m_rawValue & 0b1111111111000000) == 0);
                }
                else
                {
                    dirCheck = (other.m_rawValue & this.m_rawValue) != this.m_rawValue;
                    btnCheck = (other.m_rawValue & this.m_rawValue) != this.m_rawValue;
                    //UnityEngine.Debug.Log(((other.m_rawValue & this.m_rawValue) != this.m_rawValue) + " | " + (other.m_rawValue & this.m_rawValue) + " " + this.m_rawValue);
                }
            }
            else
            {
                if (read4way)
                {
                    //UnityEngine.Debug.Log("doing something - " + ((this.X() * other.X()) > 0) + " - " + ((this.Y() * other.Y()) > 0) + " btn - " + (((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (this.m_rawValue & (0b1111111111000000))));
                    dirCheck = ((this.X() * other.X()) > 0) || ((this.Y() * other.Y()) > 0);
                    btnCheck = ((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (this.m_rawValue & (0b1111111111000000));
                }
                else if (superStrict)
                {
                    dirCheck = (other.m_rawValue & (0b0000000000111111)) == (this.m_rawValue & (0b0000000000111111));
                    btnCheck = ((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (this.m_rawValue & (0b1111111111000000));
                }
                else
                {
                    //UnityEngine.Debug.Log("lenient input check");
                    //strict match
                    //dirCheck = ((other.m_rawValue & this.m_rawValue) & (0b0000000000111111)) == (this.m_rawValue & (0b0000000000111111));
                    dirCheck = ((other.m_rawValue & this.m_rawValue) & (0b0000000000111111)) == (this.m_rawValue & (0b0000000000111111));

                    //lenient buttons
                    btnCheck = ((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) > 0;
                    UnityEngine.Debug.Log(dirCheck + " " + btnCheck);
                }

            }

            bool ret = dirCheck && btnCheck;

            return ret;

        }

        public static bool operator ==(InputItem x, InputItem y)
        {
            return x.m_rawValue == y.m_rawValue;
        }

        public static bool operator !=(InputItem x, InputItem y)
        {
            return x.m_rawValue != y.m_rawValue;
        }

        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }


            return this.GetHashCode() == obj.GetHashCode();
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return (int)m_rawValue;
        }

        //TODO: add method to convert the information in the short to DigitalInput

    }
}