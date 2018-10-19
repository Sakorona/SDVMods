using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime
{
    public class DynamicNightConfig
    {
        public double Latitude = 38.25;
        public bool SunsetTimesAreMinusThirty = true;
    }

    public class DynamicNightTime : Mod
    {
        public static DynamicNightConfig NightConfig;
        public static IMonitor Logger;
        private bool resetOnWakeup;
        private bool isNightOut;

        private static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";

            return Type.GetType(prefix + type + ", Stardew Valley") ?? Type.GetType(prefix + type + ", StardewValley");
        }

        /// <summary> Provide an API interface </summary>
        private IDynamicNightAPI API;
        public override object GetApi()
        {
            return API ?? (API = new DynamicNightAPI());
        }

        public override void Entry(IModHelper helper)
        {
            isNightOut = false;
            Logger = Monitor;
            NightConfig = Helper.ReadConfig<DynamicNightConfig>();
            resetOnWakeup = false;

            //sanity check lat
            if (NightConfig.Latitude > 64)
                NightConfig.Latitude = 64;
            if (NightConfig.Latitude < -64)
                NightConfig.Latitude = -64;

            var harmony = HarmonyInstance.Create("koihimenakamura.dynamicnighttime");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //patch getStartingToGetDarkTime
            MethodInfo setStartingToGetDarkTime = GetSDVType("Game1").GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "getStartingToGetDarkTime");
            MethodInfo postfix = typeof(Patches.GettingDarkPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(setStartingToGetDarkTime, null, new HarmonyMethod(postfix));

            //patch getTrulyDarkTime
            MethodInfo setTrulyDarkTime = GetSDVType("Game1").GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "getTrulyDarkTime");
            MethodInfo postfixDark = typeof(Patches.GetFullyDarkPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(setTrulyDarkTime, null, new HarmonyMethod(postfixDark));

            //patch isDarkOut
            MethodInfo isDarkOut = GetSDVType("Game1").GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "isDarkOut");
            MethodInfo postfixIsDarkOut = typeof(Patches.IsDarkOutPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(isDarkOut, null, new HarmonyMethod(postfixIsDarkOut));

            //patch UpdateGameClock
            MethodInfo UpdateGameClock = helper.Reflection.GetMethod(GetSDVType("Game1"), "UpdateGameClock").MethodInfo;
            MethodInfo postfixClock = helper.Reflection.GetMethod(typeof(Patches.GameClockPatch), "Postfix").MethodInfo;
            harmony.Patch(UpdateGameClock, null, new HarmonyMethod(postfixClock));

            TimeEvents.AfterDayStarted += HandleNewDay;
            SaveEvents.AfterReturnToTitle += HandleReturn;
            TimeEvents.TimeOfDayChanged += HandleTimeChanges;

            Helper.ConsoleCommands.Add("debug_cycleinfo", "Outputs the cycle information", OutputInformation);
            Helper.ConsoleCommands.Add("debug_outdoorlight", "Outputs the outdoor light information", OutputLight);
        }


        private void HandleReturn(object sender, EventArgs e)
        {
            resetOnWakeup = false;
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            if (Game1.isDarkOut() && !Game1.currentLocation.IsOutdoors && Game1.currentLocation is DecoratableLocation loc && !resetOnWakeup)
            {
                //we need to handle the spouse's room
                if (loc is FarmHouse)
                {
                    if (Game1.timeOfDay < GetSunriseTime())
                        Game1.currentLocation.switchOutNightTiles();
                }
            }
            resetOnWakeup = false;
        }

        private Color GetRGBFromTemp(double temp)
        {
            float r,g,b = 0.0f;
            float effTemp = (float)Math.Min(Math.Max(temp, 1000), 40000);
            effTemp = effTemp / 100;

            //calc red
            if (effTemp <= 66)
                r = 255f;
            else
            { 
              float x = effTemp -55f;   
              r = 351.97690566805693f + (0.114206453784165f*x) - (40.25366309332127f * (float)Math.Log(x));
            }

            //clamp red
            r = (float)Math.Min(Math.Max(r, 0), 255);

            //calc green
            if (effTemp <= 66)
            {
                float x = effTemp - 2f;
                g = -155.25485562709179f - (.44596950469579133f * x) + (104.49216199393888f * (float)Math.Log(x));
            }
            else
            {
                float x = effTemp - 50f;
                g = 325.4494125711974f + (.07943456536662342f * x) - (28.0852963507957f * (float)Math.Log(x));
            }

            //clamp green
            g = (float)Math.Min(Math.Max(g,0), 255);

            //calc blue
            if (effTemp <= 19) 
                b = 0;
            else if (effTemp >= 66)
                b = 255;
            else
            {
                float x = effTemp - 10f;
                b = -254.76935184120902f + (.8274096064007395f * x) + (115.67994401066147f * (float)Math.Log(x));
            }
                    

            //clamp blue
            b = (float)Math.Min(Math.Max(b, 0), 255);
            Color ourColor = new Color((uint)r, (uint)g, (uint)b);
            ourColor.R = (byte)r;
            ourColor.G = (byte)g;
            ourColor.B = (byte)b;
            return ourColor;

        }

        private static Color CalculateMaskFromColor(Color target)
        {
           return new Color(255 - target.R, 255 - target.G, 255 - target.B);
        }

        private void HandleTimeChanges(object sender, EventArgsIntChanged e)
        {
            //handle ambient light changes.
            if (!Game1.currentLocation.IsOutdoors && Game1.currentLocation is DecoratableLocation locB)
            {
                Game1.ambientLight = Game1.isDarkOut() || locB.LightLevel > 0.0 ? new Color(180, 180, 0) : Color.White;
            }
            
            //handle the game being bad at night->day :|
            if (Game1.timeOfDay < GetSunriseTime())
            {
                Game1.currentLocation.switchOutNightTiles();
                isNightOut = true;
            }

            if (Game1.timeOfDay >= GetSunriseTime() && isNightOut)
            {
                Game1.currentLocation.addLightGlows();
                isNightOut = false;
            }
        }

        private void OutputLight(string arg1, string[] arg2)
        {
            Monitor.Log($"The outdoor light is {Game1.outdoorLight.ToString()}. The ambient light is {Game1.ambientLight.ToString()}");
        }

        private void OutputInformation(string arg1, string[] arg2)
        {
            Monitor.Log($"Sunrise : {GetSunrise().ToString()}, Sunset: {GetSunset().ToString()}. Solar Noon {GetSolarNoon().ToString()}");
            Monitor.Log($"Early Morning ends at {GetEndOfEarlyMorning().ToString()}, Late Afternoon begins at {GetBeginningOfLateAfternoon().ToString()}");
            Monitor.Log($"Morning Twilight: {GetMorningAstroTwilight().ToString()}, Evening Twilight: {GetAstroTwilight().ToString()}");
        }

        public static int GetSunriseTime() => GetSunrise().ReturnIntTime();
        public static SDVTime GetMorningAstroTwilight() => GetTimeAtHourAngle(-0.314159265);
        public static SDVTime GetAstroTwilight() => GetTimeAtHourAngle(-0.314159265, false);
        public static SDVTime GetMorningNavalTwilight() => GetTimeAtHourAngle(-0.20944);
        public static SDVTime GetNavalTwilight() => GetTimeAtHourAngle(-0.20944, false);
        public static SDVTime GetCivilTwilight() => GetTimeAtHourAngle(-0.104719755, false);
        public static SDVTime GetMorningCivilTwilight() => GetTimeAtHourAngle(-0.104719755);
        public static SDVTime GetSunrise() => GetTimeAtHourAngle(0.01163611);
        public static SDVTime GetSunset()
        {
            SDVTime s =  GetTimeAtHourAngle(0.01163611, false);
            if (NightConfig.SunsetTimesAreMinusThirty) s.AddTime(-30);
            return s;
        }

        protected internal static SDVTime GetSolarNoon()
        {
            var date = SDate.Now();
            int dayOfYear = date.DaysSinceStart % 112;
            double lat = GeneralFunctions.DegreeToRadians(NightConfig.Latitude);

            double solarDeclination = .40927971 * Math.Sin((2 * Math.PI / 112) * (dayOfYear - 1));
            double noon = 720 - 10 * Math.Sin(4 * (Math.PI / 112) * (dayOfYear - 1)) + 8 * Math.Sin(2 * (Math.PI / 112) * dayOfYear);
            
            int noonTime = (int)Math.Floor(noon);
            int hr = (int)Math.Floor(noonTime / 60.0);
            SDVTime calcTime = new SDVTime(hr, noonTime - (hr * 60));
            calcTime.ClampToTenMinutes();
            return calcTime;
        }

        protected internal static SDVTime GetEndOfEarlyMorning()
        {
            SDVTime Noon = GetSolarNoon();
            SDVTime Sunrise = GetSunrise();

            int numMinutes = Noon.GetNumberOfMinutesFromMidnight() - Sunrise.GetNumberOfMinutesFromMidnight();
            int endOfEarlyMorning = (int)Math.Floor(numMinutes * .38);

            SDVTime EndOfEarlyMorning = new SDVTime(Sunrise);
            EndOfEarlyMorning.AddTime(endOfEarlyMorning);
            EndOfEarlyMorning.ClampToTenMinutes();

            return EndOfEarlyMorning;
        }

        protected internal static SDVTime GetBeginningOfLateAfternoon()
        {
            SDVTime Noon = GetSolarNoon();
            SDVTime Sunset = GetSunset();

            int numMinutes = Sunset.GetNumberOfMinutesFromMidnight() - Noon.GetNumberOfMinutesFromMidnight();
            int lateAfternoon = (int)Math.Floor(numMinutes * .62);

            SDVTime LateAfternoon  = new SDVTime(Noon);
            LateAfternoon.AddTime(lateAfternoon);
            LateAfternoon.ClampToTenMinutes();

            return LateAfternoon;
        }

        protected internal static SDVTime GetTimeAtHourAngle(double angle, bool morning = true)
        {
            var date = SDate.Now();
            int dayOfYear = date.DaysSinceStart % 112;
            double lat = GeneralFunctions.DegreeToRadians(NightConfig.Latitude);

            double solarDeclination = .40927971 * Math.Sin((2 * Math.PI / 112) * (dayOfYear - 1));
            double noon = 720 - 10 * Math.Sin(4 * (Math.PI / 112) * (dayOfYear - 1)) + 8 * Math.Sin(2 * (Math.PI / 112) * dayOfYear);
            double astroHA = Math.Acos((Math.Sin(angle) - Math.Sin(lat) * Math.Sin(solarDeclination)) / (Math.Cos(lat) * Math.Cos(solarDeclination)));
            double minHA = (astroHA / (2 * Math.PI)) * 1440;
            int astroTwN = 0;

            if (!morning)
                astroTwN = (int)Math.Floor(noon + minHA);
            else
                astroTwN = (int)Math.Floor(noon - minHA);

            //Conv to an SDV compat time, then clamp it.
            int hr = (int)Math.Floor(astroTwN / 60.0);
            int min = astroTwN - (hr * 60);
            SDVTime calcTime = new SDVTime(hr, min);
            calcTime.ClampToTenMinutes();
            return calcTime;
        }
    }
}
