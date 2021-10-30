using ActionGameEngine.Enum;
using FixMath.NET;
namespace ActionGameEngine.Input
{
    //represents one instance of an input
    [System.Serializable]
    public struct InputItem
    {
        //1<<0 x sign
        //1<<1 x val
        //1<<2 y sign
        //1<<3 y val
        //1<<4 x half
        //1<<5 y half
        //1<<6 btn A
        //1<<7 btn B
        //1<<8 btn C
        //1<<9 btn D
        //1<<10 btn E
        //1<<11 btn F
        //1<<12 btn G
        //1<<13 btn H
        //1<<14 btn start
        //1<<15 btn select
        public short m_rawValue;

        public InputItem(short newRaw)
        {
            m_rawValue = newRaw;
        }

        //for reading from json, so making commands can be more user-readable
        public InputItem(DigitalInput npt)
        {
            m_rawValue = 0;

            //if has a nonzero x value
            if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.X_NONZERO))
            {
                m_rawValue |= 1 << 1;
                //if x value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.X_NEGATIVE))
                {
                    m_rawValue |= 1 << 0;
                }
            }

            //if has a nonzero y value
            if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.Y_NONZERO))
            {
                m_rawValue |= 1 << 3;
                //if y value is negative
                //don't need else since we only change the bit when it's negative
                if (EnumHelper.HasEnum((int)npt, (int)DigitalInput.Y_NEGATIVE))
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
            if ((m_rawValue & (1 << 3)) > 0)
            {
                ret = Fix64.One;
                //halve y?
                if ((m_rawValue & (1 << 5)) > 0)
                {
                    ret *=  (Fix64)0.5;
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
                if ((m_rawValue & (1 << 4)) > 0)
                {
                    ret *=  (Fix64)0.5;
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
            DigitalInput cur = current.ToDigitalInput();
            DigitalInput prev = previous.ToDigitalInput();
            //get differences, then only get the difference from prev
            DigitalInput part = (cur ^ prev) & prev;

            return new InputItem(part);
        }


        public static InputItem FindPressed(InputItem current, InputItem previous)
        {
            DigitalInput cur = current.ToDigitalInput();
            DigitalInput prev = previous.ToDigitalInput();
            //get differences, then only get the difference from cur
            DigitalInput part = (cur ^ prev) & cur;

            return new InputItem(part);
        }

        public static DigitalInput ToDigInp(short rawVal)
        {
            Fix64 x = 0;
            Fix64 y = 0;
            DigitalInput ret = 0;


            //x is nonzero
            if ((rawVal & (1 << 3)) > 0)
            {
                x = Fix64.One;
                //halve y?
                if ((rawVal & (1 << 5)) > 0)
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
                if ((rawVal & (1 << 4)) > 0)
                {
                    y *=  (Fix64)0.5;
                }
                //negative?
                if ((rawVal & (1 << 2)) > 0)
                {
                    y *= -1;
                }

            }

            //x is nonzero
            if (x != 0)
            {
                ret = DigitalInput.X_NONZERO;
                //x is positive
                if (x > 0)
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
            if (x != 0)
            {
                ret |= DigitalInput.Y_NONZERO;
                //y is positive
                if (x > 0)
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
                dirCheck = ((other.m_rawValue & this.m_rawValue) & (0b0000000000111111)) == (other.m_rawValue & (0b0000000000111111));
            }

            btnCheck = ((other.m_rawValue & this.m_rawValue) & (0b1111111111000000)) == (other.m_rawValue & (0b1111111111000000));


            return dirCheck && btnCheck;
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
            return m_rawValue;
        }

        //TODO: add method to convert the information in the short to DigitalInput

    }
}