using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GameClockPatch
    {
        public static void Postfix()
        {
            if (Game1.timeOfDay < (DynamicNightTime.GetSunrise().ReturnIntTime() - 200))
            {
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * .9875f;
            }
            else
            {
                float num = Math.Min(0.93f, (float)(0.454 + (((int)((Game1.timeOfDay - Game1.timeOfDay % 100) + (Game1.timeOfDay % 100 / 10) * 16.66) - DynamicNightTime.GetSunriseTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6) * 0.0022));
                num = 1 - num;
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * (float)num;
            }

            if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.454 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.66) - Game1.getStartingToGetDarkTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6) * 0.00125));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }

        }

  

    }
}
