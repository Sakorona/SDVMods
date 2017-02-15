using StardewValley;
using System;

namespace ClimateOfFerngill
{
    public class SDVMoon
    {
        private static int cycleLength = 16;

        public static string GetLunarPhase()
        {
            //calc days since day 1
            int numOfDays = ((Game1.year - 1) * 112) + (Game1.dayOfMonth);
            switch (Game1.currentSeason)
            {
                case "summer":
                    numOfDays += 28;
                    break;
                case "winter":
                    numOfDays += 84;
                    break;
                case "fall":
                    numOfDays += 56;
                    break;
                default:
                    numOfDays += 0;
                    break;
            }

            //divide it by the cycle.
            int currentDay = numOfDays - ((int)(Math.Floor(numOfDays / (double)cycleLength)) * cycleLength);
            

        }
    }
}
