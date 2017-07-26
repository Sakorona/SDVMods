using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using TwilightCore;
using TwilightCore.PRNG;

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

        public bool IsDangerousWeather()
        {
            if (UnusualWeather != SpecialWeather.None)
                return true;
            else
                return false;
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

        public string GetDescText(int weather, SDate Date, MersenneTwister Dice, ITranslationHelper Helper)
        {
            string retString = "";
            switch (Date.Season)
            {
                case "spring":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.spring-sunny" + Dice.Next(1, 1));
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.spring-debris" + Dice.Next(1, 1));
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.spring-rainy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.spring-stormy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.spring-snowy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.spring-wedding" + Dice.Next(1, 1));
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.spring-festival" + Dice.Next(1, 1));
                    break;
                case "summer":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.summer-sunny" + Dice.Next(1, 1));
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.summer-debris" + Dice.Next(1, 1));
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.summer-rainy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.summer-stormy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.summer-snowy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.summer-wedding" + Dice.Next(1, 1));
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.summer-festival" + Dice.Next(1, 1));
                    break;
                case "fall":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.fall-sunny" + Dice.Next(1, 1));
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.fall-debris" + Dice.Next(1, 1));
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.fall-rainy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.fall-stormy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.fall-snowy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.fall-wedding" + Dice.Next(1, 1));
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.fall-festival" + Dice.Next(1, 1));
                    break;
                case "winter":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.winter-sunny" + Dice.Next(1, 1));
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.winter-debris" + Dice.Next(1, 1));
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.winter-rainy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.winter-stormy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.winter-snowy" + Dice.Next(1, 1));
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.winter-wedding" + Dice.Next(1, 1));
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.winter-festival" + Dice.Next(1, 1));
                    break;
                default:
                    retString = "";
                    break;
            }

            return retString;
        }

        public string GetHazardousText(ITranslationHelper Helper, SDate Date, MersenneTwister Dice)
        {
            string retString = "";
            switch (UnusualWeather)
            {
                case SpecialWeather.Blizzard:
                    retString = Helper.Get("weather-desc.winter_blizzard" + Dice.Next(1, 1));
                    break;
                case SpecialWeather.Thundersnow:
                    retString = Helper.Get("weather-desc.winter_thundersnow" + Dice.Next(1, 1));
                    break;
                case SpecialWeather.DryLightning:
                    if (Date.Season != "summer")
                    retString = Helper.Get("weather-desc.summer_thundersnow" + Dice.Next(1, 1));
                    else
                    retString = Helper.Get("weather-desc.nonsummer_drylightning1" + Dice.Next(1, 1));
                    break;
                case SpecialWeather.Frost:
                    if (Date.Season == "spring")
                        retString = Helper.Get("weather-desc.spring_frost" + Dice.Next(1, 1));
                    else if (Date.Season == "fall")
                        retString = Helper.Get("weather-desc.fall_frost1" + Dice.Next(1, 1));
                    break;
                case SpecialWeather.Heatwave:
                    retString = Helper.Get("weather-desc.summer_heatwave" + Dice.Next(1, 1));
                    break;
                default:
                    retString = "";
                    break;
            }

            return retString;
        }
    }
}
