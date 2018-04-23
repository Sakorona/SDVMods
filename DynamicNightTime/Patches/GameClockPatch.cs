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
            if (Game1.timeOfDay < (GetSunrise().ReturnIntTime() - 200))
            {
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * .9875f;
            }
            else
            {
                float num = Math.Min(0.93f, (float)(0.300000011920929 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.6599998474121) - GetSunriseTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.00224999990314245));
                num = 1 - num;
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * (float)num;
            }

            if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.300000011920929 + ((double)((int)((double)(Game1.timeOfDay - Game1.timeOfDay % 100) + (double)(Game1.timeOfDay % 100 / 10) * 16.6599998474121) - Game1.getStartingToGetDarkTime()) + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.00124999990314245));
                Game1.outdoorLight = (Game1.isRaining ? Game1.ambientLight : Game1.eveningColor) * num;
            }

        }

        private static int GetSunriseTime() => GetSunrise().ReturnIntTime();

        private static SDVTime GetSunrise()
        {
            var date = SDate.Now();
            int dayOfYear = date.DaysSinceStart % 112;
            double lat = GeneralFunctions.DegreeToRadians(DynamicNightTime.NightConfig.latitude);

            double solarDeclination = .40927971 * Math.Sin((2 * Math.PI / 112) * (dayOfYear - 1));
            double noon = 720 - 10 * Math.Sin(4 * (Math.PI / 112) * (dayOfYear - 1)) + 8 * Math.Sin(2 * (Math.PI / 112) * dayOfYear);
            double hourAngle = (Math.Sin(0.01163611) - Math.Sin(lat) * Math.Sin(solarDeclination)) / (Math.Cos(lat) * Math.Cos(solarDeclination));
            double procHA = Math.Acos(hourAngle);
            double minHA = (procHA / (2 * Math.PI)) * 1440;
            int astroTwN = (int)Math.Floor(noon - minHA);

            //Conv to an SDV compat time, then clamp it.
            int hr = (int)Math.Floor(astroTwN / 60.0);
            int min = astroTwN - (hr * 60);
            SDVTime calcTime = new SDVTime(hr, min);
            calcTime.ClampToTenMinutes();

            return calcTime;
        }

    }
}
