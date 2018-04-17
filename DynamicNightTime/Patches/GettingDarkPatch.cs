using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace DynamicNightTime.Patches
{
    class GettingDarkPatch
    {
        public static void Postfix(ref int __result)
        {
            var date = SDate.Now();
            int dayOfYear = date.DaysSinceStart % 112;

            __result = 2150;

            /*
            string currentSeason = Game1.currentSeason;
            if (currentSeason == "spring" || currentSeason == "summer")
                __result = 1810;
            if (currentSeason == "fall")
                __result = 1710;
            __result = currentSeason == "winter" ? 1610 : 1810;
            */
        }
    }
}
