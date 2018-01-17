using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
{
    class Descriptions
    {
        private ITranslationHelper Helper;
        private MersenneTwister OurDice;

        public Descriptions(ITranslationHelper Translaton, MersenneTwister mDice)
        {
            Helper = Translaton;
            OurDice = mDice;
        }

        internal string GetDescOfDay(SDate date)
        {
            return Helper.Get("date" + GeneralFunctions.FirstLetterToUpper(date.Season) + date.Day);
        }

        public string GetTemperatureString(bool Scales, ITranslationHelper Helper, WeatherConditions cond)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = cond.TodayHigh.ToString("N1"),
                    lowTempC = cond.TodayLow.ToString("N1"),
                    highTempF = GeneralFunctions.ConvCtF(cond.TodayHigh).ToString("N1"),
                    lowTempF = GeneralFunctions.ConvCtF(cond.TodayLow).ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = cond.TodayHigh.ToString("N1"),
                    lowTempC = cond.TodayLow.ToString("N1")
                });

            return Temperature;
        }

        public string GetTomorrowTemperatureString(bool Scales, ITranslationHelper Helper, WeatherConditions cond)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = cond.TomorrowHigh.ToString("N1"),
                    lowTempC = cond.TomorrowLow.ToString("N1"),
                    highTempF = GeneralFunctions.ConvCtF(cond.TomorrowHigh).ToString("N1"),
                    lowTempF = GeneralFunctions.ConvCtF(cond.TomorrowLow).ToString("N1")
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = cond.TomorrowHigh.ToString("N1"),
                    lowTempC = cond.TomorrowLow.ToString("N1")
                });

            return Temperature;
        }

        internal string DescribeInGameWeather(int weather)
        {
            if (weather == Game1.weather_debris)
                return Helper.Get("weather_wind");
            if (weather == Game1.weather_festival)
                return Helper.Get("weather_festival");
            if (weather == Game1.weather_lightning)
                return Helper.Get("weather_lightning");
            if (weather == Game1.weather_rain)
                return Helper.Get("weather_rainy");
            if (weather == Game1.weather_snow)
                return Helper.Get("weather_snow");
            if (weather == Game1.weather_sunny)
                return Helper.Get("weather_sunny");
            if (weather == Game1.weather_wedding)
                return Helper.Get("weather_wedding");

            return "ERROR";
        }

        internal string GenerateTVForecast(WeatherConditions Current, SDVMoon Moon)
        {
            string text = "";
            string newSpacer = "#";
            

            if (SDVTime.CurrentTimePeriod == SDVTimePeriods.Morning)
                


            return "";
        }

        internal TemporaryAnimatedSprite GetWeatherOverlay(TV tv)
        {
            Rectangle placement = new Rectangle(413, 333, 13, 33);

            switch (Game1.weatherForTomorrow)
            {
                case 0:
                case 6:
                    placement = new Rectangle(413, 333, 13, 33);
                    break;
                case 1:
                    placement = new Rectangle(465, 333, 13, 13);
                    break;
                case 2:
                    placement = Game1.currentSeason.Equals("spring") ? new Rectangle(465, 359, 13, 13) : (Game1.currentSeason.Equals("fall") ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13));
                    break;
                case 3:
                    placement = new Rectangle(413, 346, 13, 13);
                    break;
                case 4:
                    placement = new Rectangle(413, 372, 13, 13);
                    break;
                case 5:
                    placement = new Rectangle(465, 346, 13, 13);
                    break;
            }

            return new TemporaryAnimatedSprite(Game1.mouseCursors, placement, 100f, 4, 999999, tv.getScreenPosition() + new Vector2(3f, 3f) * tv.getScreenSizeModifier(), false, false, (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 1.99999994947575E-05), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);
        }
    }
}
