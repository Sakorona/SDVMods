using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using TwilightCore;
using TwilightCore.PRNG;
using TwilightCore.StardewValley;

namespace ClimatesOfFerngillRebuild
{
    public class WeatherConditions
    {
        public RangePair TodayTemps { private set; get; }
        public int TodayWeather;

        public RangePair TomorrowTemps { private set; get; }
        public int TomorrowWeather;

        public bool WillFog;
        public SpecialWeather UnusualWeather;

        public WeatherConditions()
        {
            UnusualWeather = SpecialWeather.None;
        }

        public void SetTodayTemps(RangePair a)
        {
            TodayTemps = new RangePair();
            if (a.LowerBound > a.HigherBound)
            {
                TodayTemps.HigherBound = a.LowerBound;
                TodayTemps.LowerBound = a.HigherBound;
            }
            else
            {
                TodayTemps.HigherBound = a.HigherBound;
                TodayTemps.LowerBound = a.LowerBound;
            }
        }

        public void SetTmrwTemps(RangePair a)
        {
            TomorrowTemps = new RangePair();
            if (a.LowerBound > a.HigherBound)
            {
                TomorrowTemps.HigherBound = a.LowerBound;
                TomorrowTemps.LowerBound = a.HigherBound;
            }
            else
            {
                TomorrowTemps.HigherBound = a.HigherBound;
                TomorrowTemps.LowerBound = a.LowerBound;
            }
        }

        public bool IsDangerousWeather()
        {
            if (UnusualWeather != SpecialWeather.None)
                return true;
            else
                return false;
        }

        public void ForceFrost()
        {
            UnusualWeather = SpecialWeather.Frost;
        }

        public void ForceHeatwave()
        {
            UnusualWeather = SpecialWeather.Heatwave;
        }

        public void OnNewDay()
        {
            TodayWeather = 0;
            TomorrowWeather = 0;
            UnusualWeather = SpecialWeather.None;
            WillFog = false;
            TodayTemps = null;
            TomorrowTemps = null;
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
                        retString = Helper.Get("weather-desc.spring_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.spring_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.spring_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.spring_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.spring_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.spring_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.spring_festival1");
                    break;
                case "summer":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.summer_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.summer_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.summer_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.summer_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.summer_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.summer_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.summer_festival1");
                    break;
                case "fall":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.fall_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.fall_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.fall_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.fall_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.fall_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.fall_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.fall_festival1");
                    break;
                case "winter":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.winter_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.winter_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.winter_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.winter_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.winter_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.winter_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.winter_festival1");
                    break;
                default:
                    retString = "";
                    break;
            }

            return retString;
        }

        public bool IsHeatwave()
        {
            if (UnusualWeather == SpecialWeather.DryLightningAndHeatwave || UnusualWeather == SpecialWeather.Heatwave)
                return true;
            else
                return false;
        }
        
        


        public string GetHazardousText(ITranslationHelper Helper, SDate Date, MersenneTwister Dice)
        {
            string retString = "";
            switch (UnusualWeather)
            {
                case SpecialWeather.Blizzard:
                    retString = Helper.Get("weather-desc.winter_blizzard1");
                    break;
                case SpecialWeather.Thundersnow:
                    retString = Helper.Get("weather-desc.winter_thundersnow1");
                    break;
                case SpecialWeather.DryLightning:
                    if (Date.Season != "summer")
                    retString = Helper.Get("weather-desc.summer_thundersnow1");
                    else
                    retString = Helper.Get("weather-desc.nonsummer_drylightning1");
                    break;
                case SpecialWeather.Frost:
                    if (Date.Season == "spring")
                        retString = Helper.Get("weather-desc.spring_frost1");
                    else if (Date.Season == "fall")
                        retString = Helper.Get("weather-desc.fall_frost1");
                    break;
                case SpecialWeather.Heatwave:
                    retString = Helper.Get("weather-desc.summer_heatwave1");
                    break;
                case SpecialWeather.DryLightningAndHeatwave:
                    retString = Helper.Get("weather-desc.summer_litheatwave1");
                    break;
                default:
                    retString = "";
                    break;
            }

            return retString;
        }

        public string GetTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.GetTodayHigh().ToString("N1"),
                    lowTempC = this.GetTodayLow().ToString("N1"),
                    highTempF = this.GetTodayHighF().ToString("N1"),
                    lowTempF = this.GetTodayLowF().ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.GetTodayHigh().ToString("N1"),
                    lowTempC = this.GetTodayLow().ToString("N1")
                });

            return Temperature;
        }

        public string GetTomorrowTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.GetTmrwHigh().ToString("N1"),
                    lowTempC = this.GetTmrwLow().ToString("N1"),
                    highTempF = this.GetTmrwHighF().ToString("N1"),
                    lowTempF = this.GetTmrwLowF().ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.GetTmrwHigh().ToString("N1"),
                    lowTempC = this.GetTmrwLow().ToString("N1")
                });

            return Temperature;
        }
    }
}
