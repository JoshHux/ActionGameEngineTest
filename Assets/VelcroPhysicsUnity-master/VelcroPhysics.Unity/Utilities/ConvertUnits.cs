/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using FixMath.NET;

namespace VelcroPhysics.Utilities
{
    /// <summary>
    /// Convert units between display and simulation units.
    /// </summary>
    public static class ConvertUnits
    {
        private static Fix64 _displayUnitsToSimUnitsRatio = 100;
        private static Fix64 _simUnitsToDisplayUnitsRatio = Fix64.One / _displayUnitsToSimUnitsRatio;

        public static void SetDisplayUnitToSimUnitRatio(Fix64 displayUnitsPerSimUnit)
        {
            _displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
            _simUnitsToDisplayUnitsRatio = Fix64.One / displayUnitsPerSimUnit;
        }

        public static Fix64 ToDisplayUnits(Fix64 simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static Fix64 ToDisplayUnits(int simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static FVector2 ToDisplayUnits(FVector2 simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static void ToDisplayUnits(ref FVector2 simUnits, out FVector2 displayUnits)
        {
            displayUnits = simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static FVector3 ToDisplayUnits(FVector3 simUnits)
        {
            return simUnits * _displayUnitsToSimUnitsRatio;
        }

        public static FVector2 ToDisplayUnits(Fix64 x, Fix64 y)
        {
            return new FVector2(x, y) * _displayUnitsToSimUnitsRatio;
        }

        public static void ToDisplayUnits(Fix64 x, Fix64 y, out FVector2 displayUnits)
        {
            //displayUnits = FVector2.zero;
            //displayUnits.x = x * _displayUnitsToSimUnitsRatio;
            //displayUnits.y = y * _displayUnitsToSimUnitsRatio;
            displayUnits = new FVector2(x * _displayUnitsToSimUnitsRatio, y * _displayUnitsToSimUnitsRatio);
        }

        public static Fix64 ToSimUnits(Fix64 displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static Fix64 ToSimUnits(int displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static FVector2 ToSimUnits(FVector2 displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static FVector3 ToSimUnits(FVector3 displayUnits)
        {
            return displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static void ToSimUnits(ref FVector2 displayUnits, out FVector2 simUnits)
        {
            simUnits = displayUnits * _simUnitsToDisplayUnitsRatio;
        }

        public static FVector2 ToSimUnits(Fix64 x, Fix64 y)
        {
            return new FVector2(x, y) * _simUnitsToDisplayUnitsRatio;
        }

        public static void ToSimUnits(Fix64 x, Fix64 y, out FVector2 simUnits)
        {
            //simUnits = FVector2.zero;
            //simUnits.x = x * _simUnitsToDisplayUnitsRatio;
            //simUnits.y = y * _simUnitsToDisplayUnitsRatio;
            simUnits = new FVector2(x * _simUnitsToDisplayUnitsRatio, y * _simUnitsToDisplayUnitsRatio);
        }
    }
}