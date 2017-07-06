using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightCore;

namespace ClimatesOfFerngillRebuild
{
    public class WeatherConditions
    {
        public RangePair TodayTemps;
        public int TodayWeather;

        public RangePair TomorrowTemps;
        public int TomorrowWeather;

        public void GetTodayWeather()
        {
            if (Game1.isRaining && !Game1.isSnowing && !Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_rain;

            if (Game1.isRaining && !Game1.isSnowing && Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_lightning;

            if (!Game1.isRaining && Game1.isSnowing && !Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_snow;

            if (!Game1.isRaining && !Game1.isSnowing && !Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_sunny;

            if (!Game1.isRaining && !Game1.isSnowing && !Game1.isLightning && Game1.isDebrisWeather)
                TodayWeather = Game1.weather_debris;

            //and now the more difficult ones to determine
            if (Utility.isFestivalDay(SDate.Now().Day, SDate.Now().Season))
                TodayWeather = Game1.weather_festival;

            if (Game1.weddingToday)
                TodayWeather = Game1.weather_wedding;


        }
    }
}
