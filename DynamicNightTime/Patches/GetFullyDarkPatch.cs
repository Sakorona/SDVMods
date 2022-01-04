﻿using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GetFullyDarkPatch
    {
        public static void Postfix(ref int __result)
        {
            SDVTime calcTime = DynamicNightTime.GetNavalTwilight();
            calcTime.ClampToTenMinutes();

            __result = calcTime.ReturnIntTime();
        }
    }
}

