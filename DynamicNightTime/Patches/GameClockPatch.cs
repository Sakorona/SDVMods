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

            else if (Game1.isRaining)
                Game1.outdoorLight = Game1.ambientLight * 0.3f;
            else
                Game1.outdoorLight = Color.White;
        }

  

    }
}
