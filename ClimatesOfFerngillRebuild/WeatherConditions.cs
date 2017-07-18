using StardewModdingAPI.Utilities;
using StardewValley;
using TwilightCore;

namespace ClimatesOfFerngillRebuild
{
    public class WeatherConditions
    {
        public RangePair TodayTemps;
        public int TodayWeather;

        public RangePair TomorrowTemps;
        public int TomorrowWeather;

        public bool WillFog;
        public SpecialWeather UnusualWeather;

        public WeatherConditions()
        {
            UnusualWeather = SpecialWeather.None;
        }

        public void Reset()
        {
            TodayTemps = null;
            TomorrowTemps = null;
            TodayWeather = 0;
            TomorrowWeather = 0;
            WillFog = false;
            UnusualWeather = SpecialWeather.None;
        }

        public void ResetTodayTemps(double high, double low)
        {
            TodayTemps.HigherBound = high;
            TodayTemps.LowerBound = low;
        }

        public double GetTodayHigh() => TodayTemps.HigherBound;
        public double GetTodayLow() => TodayTemps.LowerBound;
        public double GetTodayHighF() => ConvCtF(TodayTemps.HigherBound);
        public double GetTodayLowF() => ConvCtF(TodayTemps.LowerBound);

        public double GetTmrwHigh() => TomorrowTemps.HigherBound;
        public double GetTmrwLow() => TomorrowTemps.LowerBound;
        public double GetTmrwHighF() => ConvCtF(TomorrowTemps.HigherBound);
        public double GetTmrwLowF() => ConvCtF(TomorrowTemps.LowerBound);

        private double ConvCtF(double temp) => ((temp * 1.8) + 32);

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

            if (Utility.isFestivalDay(SDate.Now().Day, SDate.Now().Season))
                TodayWeather = Game1.weather_festival;

            if (Game1.weddingToday)
                TodayWeather = Game1.weather_wedding;
        }
    }
}
