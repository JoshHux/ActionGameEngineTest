using FixMath.NET;
namespace ActionGameEngine.Data.Helpers.Static
{
    public static class GameplayHelper
    {
        public static readonly int InputLeniency = 4;
        public static readonly Fix64 TerminalVel = (Fix64)(15);


        //input the current speed in a linear direction, acceleration in that direction, and maximum speed in the direction
        //returns max velocity if acceleration + current speed exceeds exceeds max speed
        //returns current velocity + acceleration if not
        //returns the positive value
        public static Fix64 ApplyAcceleration(Fix64 curVel, Fix64 accel, Fix64 maxSpd)
        {
            Fix64 ret = curVel;
            if (Fix64.Abs(ret + accel) > Fix64.Abs(maxSpd))
            { ret = maxSpd; }
            else
            { ret += accel; }
            return ret;
        }

        public static Fix64 ApplyAcceleration2(Fix64 curVel, Fix64 accel, Fix64 maxSpd)
        {
            Fix64 ret = curVel;
            if ((ret + accel) < (maxSpd))
            { ret = maxSpd; }
            else
            { ret += accel; }
            return ret;
        }
    }
}