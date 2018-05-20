using System;
using StardewValley;

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

            else if (Game1.timeOfDay >= Game1.getTrulyDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.75 + ((int)(Game1.timeOfDay - Game1.timeOfDay % 100 + Game1.timeOfDay % 100 / 10 * 16.66) - Game1.getTrulyDarkTime() + Game1.gameTimeInterval / 7000.0 * 16.6) * 0.00062));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
            
            if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.30 + (((int)(Game1.timeOfDay - Game1.timeOfDay % 100 + Game1.timeOfDay % 100 / 10 * 16.66) - Game1.getStartingToGetDarkTime()) + Game1.gameTimeInterval / 7000.0 * 16.6) * 0.00225));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }
        }

  

    }
}
