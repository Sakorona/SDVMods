using StardewValley;
using System;

namespace ClimateOfFerngill
{
    public class SDVMoon
    {
        private static int cycleLength = 16;

        internal const int NEWMOON = 1001;
        internal const int WAXCRES = 1002;
        internal const int FIRSTQT = 1003;
        internal const int WAXGIBB = 1004;
        internal const int FULMOON = 1005;
        internal const int WANGIBB = 1006;
        internal const int LASTQRT = 1007;
        internal const int WANCRES = 1008;

        public static int GetLunarPhase()
        {
            //divide it by the cycle.
            int currentDay = (int)Game1.stats.daysPlayed - ((int)(Math.Floor(Game1.stats.daysPlayed / (double)cycleLength)) * cycleLength);

            //Day 0 and 16 are the New Moon, so Day 8 must be the Full Moon. Day 4 is 1Q, Day 12 is 3Q. Coorespondingly..
            switch (currentDay)
            {
                case 0:
                    return NEWMOON;
                case 1:
                case 2:
                case 3:
                    return WAXCRES;
                case 4:
                    return FIRSTQT;
                case 5:
                case 6:
                case 7:
                    return WAXGIBB;
                case 8:
                    return FULMOON;
                case 9:
                case 10:
                case 11:
                    return WANGIBB;
                case 12:
                    return LASTQRT;
                case 13:
                case 14:
                case 15:
                    return WANCRES;
                case 16:
                    return NEWMOON; //sanity check
                default:
                    return -1;             
            }       

        }
    }
}
