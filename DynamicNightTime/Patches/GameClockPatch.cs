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

            if (Game1.timeOfDay < sunriseTime - astronTime)
            {
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * .5f;
            }
            else if (Game1.timeOfDay < sunriseTime)
            {
                float v = ((float)(SDVTime.ConvertIntTimeToMinutes(Game1.timeOfDay) - SDVTime.ConvertIntTimeToMinutes(astronTime)) / (SDVTime.ConvertIntTimeToMinutes(sunriseTime) - SDVTime.ConvertIntTimeToMinutes(astronTime)));
                float num = Math.Max(0.001f, (1f - v));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
            else if (Game1.timeOfDay >= sunriseTime && Game1.timeOfDay <= Game1.getStartingToGetDarkTime())
            {
                if (Game1.isRaining)
                    Game1.outdoorLight = Game1.ambientLight * 0.3f;
                else
                    Game1.outdoorLight = Color.White;
            }

            else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime() && Game1.timeOfDay <= Game1.getTrulyDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.15 + (((int)(Game1.timeOfDay - Game1.timeOfDay % 100 + Game1.timeOfDay % 100 / 10 * 16.66) - Game1.getStartingToGetDarkTime()) + Game1.gameTimeInterval / 7000.0 * 16.6) * 0.00225));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }

            else if (Game1.timeOfDay >= Game1.getTrulyDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.75 + ((int)(Game1.timeOfDay - Game1.timeOfDay % 100 + Game1.timeOfDay % 100 / 10 * 16.66) - Game1.getTrulyDarkTime() + Game1.gameTimeInterval / 7000.0 * 16.6) * 0.00062));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
            else if (Game1.isRaining)
                Game1.outdoorLight = Game1.ambientLight * 0.3f;
            else
                Game1.outdoorLight = Color.White;
        }

  

    }
}
