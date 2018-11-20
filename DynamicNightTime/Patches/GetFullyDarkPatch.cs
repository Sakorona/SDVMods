using StardewModdingAPI.Utilities;
using System;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GetFullyDarkPatch
    {
        public static void Postfix(ref int __result)
        {
<<<<<<< HEAD
            SDVTime calcTime = DynamicNightTime.GetNavalTwilight();
            calcTime.ClampToTenMinutes();

            __result = calcTime.ReturnIntTime();
=======
            SDVTime calcTime = DynamicNightTime.GetAstroTwilight();
            calcTime.ClampToTenMinutes();

             __result = calcTime.ReturnIntTime();
>>>>>>> master
        }
    }
}

