using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ClimateOfFerngill
{
    static class InternalUtility
    {
        internal static int TIMEADD = 9001;
        internal static int TIMESUB = 9002;

        public static string getFestivalName(int dayOfMonth, string currentSeason)
        {
            switch (currentSeason)
            {
                case ("spring"):
                    if (dayOfMonth == 13) return "Egg Festival";
                    if (dayOfMonth == 24) return "Flower Dance";
                    break;
                case ("winter"):
                    if (dayOfMonth == 8) return "Festival of Ice";
                    if (dayOfMonth == 25) return "Feast of the Winter Star";
                    break;
                case ("fall"):
                    if (dayOfMonth == 16) return "Stardew Valley Fair";
                    if (dayOfMonth == 27) return "Spirit's Eve";
                    break;
                case ("summer"):
                    if (dayOfMonth == 11) return "Luau";
                    if (dayOfMonth == 28) return "Dance of the Moonlight Jellies";
                    break;
                default:
                    return "Festival";
            }

            return "Festival";

        }

        public static void showMessage(string msg)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true);
            hudmsg.whatType = 2;
            Game1.addHUDMessage(hudmsg);
        }

        internal static int GetNewValidTime(int originVal, int changeVal, int mode)
        {
            int oHour = 0, oMin = 0, cHour = 0, cMin = 0, retVal = 0;

            oHour = (int)Math.Floor((double)originVal / 100);
            oMin = (int)Math.Floor((double)originVal % 100);

            cHour = (int)Math.Floor((double)changeVal / 100);
            cMin = (int)Math.Floor((double)changeVal % 100);
            
            //check for boundaries, and original value.
            if (mode == TIMEADD)
            {
                retVal = (oHour + cHour) * 100;
                int testMin = oMin + cMin;
                while (testMin > 59)
                {
                    retVal += 100;
                    testMin -= 60;

                    if (testMin < 0) //sanity check.
                        testMin = 0;
                }

                retVal = retVal + testMin;
            }
            else if (mode == TIMESUB)
            {
                retVal = (oHour - cHour) * 100;
                int testMin = oMin - cMin;
                while (testMin < -59)
                {
                    retVal -= 100;
                    testMin += 60;

                    if (testMin > 0)
                        testMin = 0;
                }
            }

            if (retVal < 0600)
                return 0600;
            if (retVal > 2600)
                return 2600;

            return retVal; 
        }

        internal static string GetNewSeason(string currentSeason)
        {
            if (currentSeason == "spring") return "summer";
            if (currentSeason == "summer") return "fall";
            if (currentSeason == "fall") return "winter";
            if (currentSeason == "winter") return "spring";

            return "ERROR";
        }
    }
}
