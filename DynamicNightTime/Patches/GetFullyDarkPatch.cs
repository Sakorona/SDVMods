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
                var date = SDate.Now();
                int dayOfYear = date.DaysSinceStart % 112;
                double lat = GeneralFunctions.DegreeToRadians(DynamicNightTime.NightConfig.latitude);

                double solarDeclination = .40927971 * Math.Sin((2 * Math.PI / 112) * (dayOfYear - 1));
                double noon = 720 - 10 * Math.Sin(4 * (Math.PI / 112) * (dayOfYear - 1)) + 8 * Math.Sin(2 * (Math.PI / 112) * dayOfYear);
                double hourAngle = (Math.Sin(-0.314159265) - Math.Sin(lat) * Math.Sin(solarDeclination)) / (Math.Cos(lat) * Math.Cos(solarDeclination));
                double procHA = Math.Acos(hourAngle);
                double minHA = (procHA / (2 * Math.PI)) * 1440;
                int astroTwN = (int)Math.Floor(noon + minHA);

                //Conv to an SDV compat time, then clamp it.
                int hr = (int)Math.Floor(astroTwN / 60.0);
                int min = astroTwN - (hr * 60);
                SDVTime calcTime = new SDVTime(hr, min);
                calcTime.ClampToTenMinutes();

                __result = calcTime.ReturnIntTime();
        }
    }
}

