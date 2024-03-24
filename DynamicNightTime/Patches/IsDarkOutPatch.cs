using StardewValley;

namespace DynamicNightTime.Patches
{
    class IsDarkOutPatch
    {
        public static void Postfix(ref bool __result)
        {
            bool IsPastSunset = Game1.timeOfDay > Game1.getModeratelyDarkTime(Game1.currentLocation);

            __result = IsPastSunset;
        }
    }
}
