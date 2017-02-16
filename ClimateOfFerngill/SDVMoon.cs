using StardewValley;
using System;

namespace ClimateOfFerngill
{
    public class SDVMoon
    {
        private static int cycleLength = 16;

        public static string GetLunarPhase()
        {

            //divide it by the cycle.
            int currentDay = (int)Game1.stats.daysPlayed - ((int)(Math.Floor(Game1.stats.daysPlayed / (double)cycleLength)) * cycleLength);

            return "";            

        }
    }
}
