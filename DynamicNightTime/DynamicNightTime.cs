using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Linq;
using System.Reflection;

namespace DynamicNightTime
{
    public class DynamicNightTime : Mod
    {
        public override void Entry(IModHelper helper)
        { /*
            var harmony = HarmonyInstance.Create("koihimenakamura.dynamicnighttime");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //patch getStartingToGetDarkTime
            MethodInfo setStartingToGetDarkTime = typeof(Game1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "getStartingToGetDarkTime");
            MethodInfo postfix = typeof(Patches.GettingDarkPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(setStartingToGetDarkTime, null, new HarmonyMethod(postfix));

            //and now events!
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;*/
            TimeEvents.TimeOfDayChanged += TenMinuteUpdate;
        }

        private void TenMinuteUpdate(object sender, EventArgsIntChanged e)
        {
            Console.WriteLine($"Outdoor Light: {Game1.outdoorLight.ToString()}, Evening Light: {Game1.eveningColor.ToString()}");
            Console.WriteLine($"Ambient Light: {Game1.ambientLight.ToString()}, Beginning Light: {Color.White.ToString()}");
        }

        private void TimeEvents_AfterDayStarted(object sender, System.EventArgs e)
        {
            Console.WriteLine($"End of Day is: {Game1.getStartingToGetDarkTime()}");
        }
    }
}
