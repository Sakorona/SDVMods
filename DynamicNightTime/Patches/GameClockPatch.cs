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
                float minEff = SDVTime.MinutesBetweenTwoIntTimes(astronTime, Game1.timeOfDay) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                float lightMulti = Math.Max(0.001f, 1f - (.83f * (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunriseTime, astronTime))));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * lightMulti;
            }
            else if (Game1.timeOfDay >= sunriseTime && Game1.timeOfDay <= Game1.getStartingToGetDarkTime())
            {
                if (Game1.isRaining)
                    Game1.outdoorLight = Game1.ambientLight * 0.3f;
                else 
                { 
                    Game1.outdoorLight = Color.White;
                }
            }
            else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                //So, the num increases as we get closer to astronomical twilight
                // So we know that at astronomical twilight, we should be near .94
                // And at naval twilight .7, and at civil twilight .5
                int sunset = DynamicNightTime.GetSunset().ReturnIntTime();
                int navalTwilight = DynamicNightTime.GetNavalTwilight().ReturnIntTime();
                int astroTwilight = DynamicNightTime.GetAstroTwilight().ReturnIntTime();

                float lightMulti = 0.0f;
                if (Game1.timeOfDay >= sunset && Game1.timeOfDay <= navalTwilight) //civil
                {
                    float minEff = SDVTime.MinutesBetweenTwoIntTimes(sunset, Game1.timeOfDay) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                    lightMulti = .3f + (.35f * (minEff / SDVTime.MinutesBetweenTwoIntTimes(sunset, navalTwilight)));
                }
                if (Game1.timeOfDay >= navalTwilight && Game1.timeOfDay <= astroTwilight) //naval
                {
                    float minEff = SDVTime.MinutesBetweenTwoIntTimes(navalTwilight, Game1.timeOfDay) + (float)Math.Min(10.0, Game1.gameTimeInterval / 700);
                    lightMulti = .65f + (.29f * (minEff / SDVTime.MinutesBetweenTwoIntTimes(navalTwilight, astroTwilight)));
                }
                if (Game1.timeOfDay >= astroTwilight)
                    lightMulti = .94f;

                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * lightMulti;
            }
        }

  

    }
}
