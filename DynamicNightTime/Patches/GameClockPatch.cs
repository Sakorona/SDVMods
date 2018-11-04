using System;
using Microsoft.Xna.Framework;
using StardewValley;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GameClockPatch
    {
        public static void Postfix()
        {
            int sunriseTime = DynamicNightTime.GetSunrise().ReturnIntTime();
            int astronTime = DynamicNightTime.GetMorningAstroTwilight().ReturnIntTime();
            int endOfEarlyMorning = DynamicNightTime.GetEndOfEarlyMorning().ReturnIntTime();
            int beginOfLateAfternoon = DynamicNightTime.GetBeginningOfLateAfternoon().ReturnIntTime();
            int sunsetTime = DynamicNightTime.GetSunset().ReturnIntTime();

            if (Game1.timeOfDay < sunriseTime - astronTime)
            {
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * .15f;
            }
            else if (Game1.timeOfDay < sunriseTime)
            {
                float v = ((float)(SDVTime.ConvertIntTimeToMinutes(Game1.timeOfDay) - SDVTime.ConvertIntTimeToMinutes(astronTime)) / (SDVTime.ConvertIntTimeToMinutes(sunriseTime) - SDVTime.ConvertIntTimeToMinutes(astronTime)));
                float num = Math.Max(0.001f, .93f - (.6f * v));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
            else if (Game1.timeOfDay >= sunriseTime && Game1.timeOfDay <= Game1.getStartingToGetDarkTime())
            {
                if (Game1.isRaining)
                    Game1.outdoorLight = Game1.ambientLight * 0.3f;
                else
                    Game1.outdoorLight = Color.White;
            }
            else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                //So, the num increases as we get closer to astronomical twilight
                // So we know that at astronomical twilight, we should be near .94
                // And at naval twilight .7, and at civil twilight .5



                /*
                float num = Math.Min(0.93f, (float)(0.15 + (((int)(Game1.timeOfDay - Game1.timeOfDay % 100 + Game1.timeOfDay % 100 / 10 * 16.66) - Game1.getStartingToGetDarkTime()) + Game1.gameTimeInterval / 7000.0 * 16.6) * 0.006347));
                //now time to determine the time.
                */
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
        }

  

    }
}
