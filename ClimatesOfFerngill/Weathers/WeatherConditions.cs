using EnumsNET;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using TwilightShards.Common;

namespace ClimatesOfFerngillRebuild
{

    /// <summary>
    /// This class tracks the current the current weather of the game
    /// </summary>
    public class WeatherConditions
    {
        ///<summary>Game configuration options</summary>
        private WeatherConfig ModConfig;

        ///<summary>pRNG object</summary>
        private MersenneTwister Dice;

        ///<summary>SMAPI logger</summary>
        private IMonitor Monitor;

        /// <summary> The translation interface </summary>
        private ITranslationHelper Translation;

        /// <summary>Track today's temperature</summary>
        private RangePair TodayTemps;

        /// <summary>Track tomorrow's temperature</summary>
        private RangePair TomorrowTemps;

        /// <summary>Track current conditions</summary>
        private CurrentWeather CurrentConditionsN { get; set; }

        /// <summary>Fog object - handles fog</summary>
        internal FerngillFog OurFog { get; set; }

        /// *************************************************************************
        /// ACCESS METHODS
        /// *************************************************************************
        public CurrentWeather GetCurrentConditions()
        {
            if (CurrentConditionsN.HasFlag(CurrentWeather.Fog) && CurrentConditionsN.HasFlag(CurrentWeather.Wind))
                CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);

            return CurrentConditionsN;
        }

        /// <summary>Rather than track the weather seprately, always get it from the game.</summary>
        public CurrentWeather TommorowForecast => ConvertToCurrentWeather(Game1.weatherForTomorrow);

        public bool IsTodayTempSet => TodayTemps != null;
        public bool IsTomorrowTempSet => TomorrowTemps != null;

        /// <summary> This returns the high for today </summary>
        public double TodayHigh => TodayTemps.HigherBound;

        /// <summary> This returns the high for tomorrow </summary>
        public double TomorrowHigh => TomorrowTemps.HigherBound;

        /// <summary> This returns the low for today </summary>
        public double TodayLow => TodayTemps.LowerBound;

        /// <summary> This returns the low for tomorrow </summary>
        public double TomorrowLow => TomorrowTemps.LowerBound;

        public void AddWeather(CurrentWeather newWeather)
        {
            //sanity remove these once weather is set.
            CurrentConditionsN.RemoveFlags(CurrentWeather.Unset);

            //Some flags are contradictoary. Fix that here.
            if (newWeather == CurrentWeather.Rain)
            {
                //unset debris, sunny, snow and blizzard, if it's raining.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Sunny)
            {
                //unset debris, rain, snow and blizzard, if it's sunny.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Wind)
            {
                //unset sunny, rain, snow and blizzard, if it's debris.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Snow)
            {
                //unset debris, sunny, snow and blizzard, if it's raining.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Frost)
            {
                CurrentConditionsN.RemoveFlags(CurrentWeather.Heatwave);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Heatwave)
            {
                CurrentConditionsN.RemoveFlags(CurrentWeather.Frost);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Wedding || newWeather == CurrentWeather.Festival)
            {
                CurrentConditionsN = newWeather; //Clear *everything else* if it's a wedding or festival.
            }

            else
                CurrentConditionsN |= newWeather;
        }

        internal void UpdateForCurrentMoment()
        {
            // Okay. So. I want to hate myself now.
            if (this.OurFog.IsFogVisible)
                CurrentConditionsN.CombineFlags(CurrentWeather.Fog);
            else
            {
                if (ModConfig.Verbose) Monitor.Log("Removing fog from current conditions");
                CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);
            }
        }

        /// <summary> Syntatic Sugar for Enum.HasFlag(). Done so if I choose to rewrite how it's accessed, less rewriting of invoking functions is needed. </summary>
        /// <param name="checkWeather">The weather being checked.</param>
        /// <returns>If the weather is present</returns>
        public bool HasWeather(CurrentWeather checkWeather)
        {
            if (!this.OurFog.IsFogVisible && CurrentConditionsN.HasFlag(CurrentWeather.Fog))
                CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);
            
            return CurrentConditionsN.HasFlag(checkWeather);
        }

        public void ClearFog() => CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);

        public bool HasPrecip()
        {
            if (CurrentConditionsN.HasAnyFlags(CurrentWeather.Snow | CurrentWeather.Rain | CurrentWeather.Blizzard))
                return true;

            return false;

        }

        public WeatherIcon CurrentWeatherIcon
        {
            get
            {
                if (ModConfig.Verbose)
                    Monitor.Log($"Conditions: {CurrentConditionsN}");

                if (OurFog.IsFogVisible)
                    CurrentConditionsN |= CurrentWeather.Fog;
                else
                    CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);
                
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Rain))
                    return WeatherIcon.IconRain;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Rain | CurrentWeather.Lightning)))
                    return WeatherIcon.IconStorm;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Sunny)))
                    return WeatherIcon.IconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Wind)))
                    return WeatherIcon.IconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Snow)))
                    return WeatherIcon.IconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Snow))
                    return WeatherIcon.IconSnow;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Blizzard)))
                    return WeatherIcon.IconBlizzard;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Festival))
                    return WeatherIcon.IconFestival;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Wedding))
                    return WeatherIcon.IconWedding;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Sunny))
                    return WeatherIcon.IconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Unset))
                    return WeatherIcon.IconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Lightning))
                    return WeatherIcon.IconStorm;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Wind)) {
                    if (SDate.Now().Season == "spring") return WeatherIcon.IconSpringDebris;
                    else return WeatherIcon.IconDebris;
                }

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Lightning)))
                    return WeatherIcon.IconThunderSnow;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Sunny | CurrentWeather.Lightning)))
                    return WeatherIcon.IconDryLightning;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Sunny)))
                    return WeatherIcon.IconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Wind)))
                {
                    if (SDate.Now().Season == "spring") return WeatherIcon.IconSpringDebris;
                    else return WeatherIcon.IconDebris;
                }

                //The more complex ones.
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Sunny | CurrentWeather.Frost)))
                    return WeatherIcon.IconSunny;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Frost)))
                    return WeatherIcon.IconStorm;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Sunny | CurrentWeather.Frost)))
                    return WeatherIcon.IconSunny;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Wind | CurrentWeather.Frost)))
                {
                    if (SDate.Now().Season == "spring") return WeatherIcon.IconSpringDebris;
                    else return WeatherIcon.IconDebris;
                }
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Rain | CurrentWeather.Frost)))
                    return WeatherIcon.IconRain;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Frost)))
                    return WeatherIcon.IconSnow;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Frost)))
                    return WeatherIcon.IconBlizzard;

                //And now for fog.
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Fog | CurrentWeather.Sunny)))
                    return WeatherIcon.IconSunnyFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Fog)))
                    return WeatherIcon.IconStormFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Sunny | CurrentWeather.Fog)))
                    return WeatherIcon.IconSunnyFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Rain | CurrentWeather.Fog)))
                    return WeatherIcon.IconRainFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Fog)))
                    return WeatherIcon.IconSnowFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Fog)))
                    return WeatherIcon.IconSnowFog;

                return WeatherIcon.IconError;
            }
        }

        /*
        /// <summary> Get whether or not it is a heatwave </summary>
        public bool IsHeatwave => (TodayTemps?.HigherBound >= ModConfig.TooHotOutside);

        /// <summary> Get whether or not it is a frost </summary>
        public bool IsFrost => (TodayTemps?.LowerBound <= ModConfig.TooColdOutside && SDate.Now().Season != "winter");
        */
        //pass through methods
        

        /// ******************************************************************************
        /// CONSTRUCTORS
        /// ******************************************************************************

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="Dice">pRNG</param>
        /// <param name="monitor">SMAPI log object</param>
        /// <param name="Config">Game configuration</param>
        public WeatherConditions(MersenneTwister Dice, ITranslationHelper Translation, IMonitor monitor, WeatherConfig Config)
        {
            this.Monitor = monitor;
            this.ModConfig = Config;
            this.Dice = Dice;
            this.Translation = Translation;
            CurrentConditionsN = CurrentWeather.Unset;
            OurFog = new FerngillFog(Config.Verbose, monitor);
        }

        /// ******************************************************************************
        /// PROCESSING
        /// ******************************************************************************
        internal void ForceTodayTemps(double high, double low)
        {
            if (TodayTemps is null)
                TodayTemps = new RangePair();

            TodayTemps.HigherBound = high;
            TodayTemps.LowerBound = low;
        }

        /// <summary>This function resets the weather for a new day.</summary>
        public void OnNewDay()
        {
            OurFog.OnNewDay();
            CurrentConditionsN = CurrentWeather.Unset;
            TodayTemps = TomorrowTemps; //If Tomorrow is null, should just allow it to be null.
            TomorrowTemps = null;
        }

        /// <summary> This function resets the weather object to basic. </summary>
        public void Reset()
        {
            OurFog.Reset();
            TodayTemps = null;
            TomorrowTemps = null;
            CurrentConditionsN = CurrentWeather.Unset;
        }

        /// <summary> This sets the temperatures from outside for today </summary>
        /// <param name="a">The RangePair that contains the generated temperatures</param>
        public void SetTodayTemps(RangePair a) => TodayTemps = new RangePair(a, EnforceHigherOverLower: true);

        /// <summary> This sets the temperatures from outside for tomorrow</summary>
        /// <param name="a">The RangePair that contains the generated temperatures</param>
        public void SetTomorrowTemps(RangePair a) => TomorrowTemps = new RangePair(a, EnforceHigherOverLower: true);

        /// ***************************************************************************
        /// Utility functions
        /// ***************************************************************************
        
        ///<summary> This function converts from the game weather back to the CurrentWeather enum. Intended primarily for use with tommorow's forecasted weather.</summary>
        internal static CurrentWeather ConvertToCurrentWeather(int weather)
        { 
            if (weather == Game1.weather_rain)
                return CurrentWeather.Rain;
            else if (weather == Game1.weather_festival)
                return CurrentWeather.Festival;
            else if (weather == Game1.weather_wedding)
                return CurrentWeather.Wedding;
            else if (weather == Game1.weather_debris)
                return CurrentWeather.Wind;
            else if (weather == Game1.weather_snow)
                return CurrentWeather.Snow;
            else if (weather == Game1.weather_lightning)
                return CurrentWeather.Rain | CurrentWeather.Lightning;

            //default return.
            return CurrentWeather.Sunny;
        }

        internal void SetTodayWeather()
        {
            CurrentConditionsN = CurrentWeather.Unset; //reset the flag.

            if (!Game1.isDebrisWeather && !Game1.isRaining && !Game1.isSnowing)
                AddWeather(CurrentWeather.Sunny);

            if (Game1.isRaining)
                AddWeather(CurrentWeather.Rain);
            if (Game1.isDebrisWeather)
                AddWeather(CurrentWeather.Wind);
            if (Game1.isLightning)
                AddWeather(CurrentWeather.Lightning);
            if (Game1.isSnowing)
                AddWeather(CurrentWeather.Snow);

            if (Utility.isFestivalDay(SDate.Now().Day, SDate.Now().Season))
                AddWeather(CurrentWeather.Festival);

            if (Game1.weddingToday)
                AddWeather(CurrentWeather.Wedding);
        }

        /// <summary> Force for wedding only to match vanilla behavior. </summary>
        internal void ForceWeddingOnly() => CurrentConditionsN = CurrentWeather.Wedding;

        /// <summary> Force for festival only to match vanilla behavior. </summary>
        internal void ForceFestivalOnly() => CurrentConditionsN = CurrentWeather.Festival;

        /// <summary>Gets a quick string describing the weather. Meant primarily for use within the class. </summary>
        /// <returns>A quick ID of the weather</returns>
        private string GetWeatherType()
        {
            return WeatherConditions.GetWeatherType(CurrentConditionsN);
        }

        /// <summary>Gets a quick string describing the weather. Meant primarily for use within the class. </summary>
        /// <returns>A quick ID of the weather</returns>
        private static string GetWeatherType(CurrentWeather CurrentConditions)
        {
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Rain))
                return "rainy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Rain | CurrentWeather.Lightning)))
                return "stormy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Lightning))
                return "drylightning";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Snow))
                return "snowy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Snow | CurrentWeather.Blizzard)))
                return "blizzard";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Festival))
                return "festival";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Wedding))
                return "wedding";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Sunny))
                return "sunny";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Unset))
                return "unset";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Wind))
                return "windy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Snow)))
                return "thundersnow";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Sunny)))
                return "drylightningheatwavesunny";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Wind)))
                return "drylightningheatwavewindy";

            //The more complex ones.
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Frost | CurrentWeather.Sunny)))
                return "drylightningwithfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Frost)))
                return "stormswithfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Sunny | CurrentWeather.Frost)))
                return "sunnyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Wind | CurrentWeather.Frost)))
                return "windyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Rain | CurrentWeather.Frost)))
                return "rainyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Snow | CurrentWeather.Frost)))
                return "snowyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Frost)))
                return "blizzardfrost";

            //And now for fog.
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Fog | CurrentWeather.Sunny)))
                return "drylightningwithfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Fog)))
                return "stormswithfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Sunny | CurrentWeather.Fog)))
                return "sunnyfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Rain | CurrentWeather.Fog)))
                return "rainyfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Snow | CurrentWeather.Fog)))
                return "snowyfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Fog)))
                return "blizzardfog";

            return "ERROR";
        }

        /// ***************************************************************************
        /// Description functions
        /// ***************************************************************************

        ///<summary>This function returns the current condition (pulled from the translation helper.)</summary>
        internal string DescribeConditions()
        {
            switch (GetWeatherType()){
                case "rainy":
                    return Translation.Get("weather_rainy");
                case "stormy":
                    return Translation.Get("weather_lightning");
                case "drylightning":
                    return Translation.Get("weather_drylightning");
                case "snowy":
                    return Translation.Get("weather_snow");
                case "blizzard":
                    return Translation.Get("weather_blizzard");
                case "wedding":
                    return Translation.Get("weather_wedding");
                case "festival":
                    return Translation.Get("weather_festival");
                case "unset":
                    return Translation.Get("weather_unset");
                case "windy":
                    return Translation.Get("weather_wind");
                case "thundersnow":
                    return Translation.Get("weather_thundersnow");
                case "heatwave":
                    return Translation.Get("weather_heatwave");
                case "drylightningheatwave":
                    return Translation.Get("weather_drylightningheatwave");
                case "drylightningheatwavesunny":
                    return Translation.Get("weather_drylightningheatwavesunny");
                case "drylightningheatwavewindy":
                    return Translation.Get("weather_drylightningheatwavewindy");
                case "drylightningwithfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_drylightning") });
                case "stormswithfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_lightning") });
                case "sunnyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_sunny") });
                case "windyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_wind") });
                case "rainyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_rainy") });
                case "snowyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_snow") });
                case "blizzardfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_blizzard") });
                case "drylightningwithfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_drylightning") + " " + Translation.Get("weather_sunny") });
                case "stormswithfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_lightning") });
                case "sunnyfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_sunny") });
                case "rainyfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_rainy") });
                case "snowyfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_snow") });
                case "blizzardfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_blizzard") });
                default:
                    return "ERROR";
            }     
        }

        public string GetTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.TodayHigh.ToString("N1"),
                    lowTempC = this.TodayLow.ToString("N1"),
                    highTempF = GeneralFunctions.ConvCtF(this.TodayHigh).ToString("N1"),
                    lowTempF = GeneralFunctions.ConvCtF(this.TodayLow).ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.TodayHigh.ToString("N1"),
                    lowTempC = this.TodayLow.ToString("N1")
                });

            return Temperature;
        }

        public string GetTomorrowTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.TomorrowHigh.ToString("N1"),
                    lowTempC = this.TomorrowLow.ToString("N1"),
                    highTempF = GeneralFunctions.ConvCtF(this.TomorrowHigh).ToString("N1"),
                    lowTempF = GeneralFunctions.ConvCtF(this.TomorrowLow).ToString("N1")
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.TomorrowHigh.ToString("N1"),
                    lowTempC = this.TomorrowLow.ToString("N1")
                });

            return Temperature;
        }
    

        /// <summary>
        /// This function returns a description of the object. A very important note that this is meant for debugging, and as such does not do localization.
        /// </summary>
        /// <returns>A string describing hte object.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += $"Low for today is {TodayTemps?.LowerBound.ToString("N3")} with the high being {TodayTemps?.HigherBound.ToString("N3")}. The current conditions are {GetWeatherType()}.";

            if (OurFog.IsFogVisible)
            {
                ret += $" Fog is visible until {OurFog.ExpirationTime} and type {FerngillFog.DescFogType(OurFog.CurrentFogType)}.  ";
            }
            else
            {
                ret += "Fog is current unvisible.";
            }

            ret += $"Weather set for tommorow is {WeatherConditions.GetWeatherType(WeatherConditions.ConvertToCurrentWeather(Game1.weatherForTomorrow))} with high {TomorrowTemps?.HigherBound.ToString("N3")} and low {TomorrowTemps?.LowerBound.ToString("N3")} ";

            return ret;
        }
    }
}
