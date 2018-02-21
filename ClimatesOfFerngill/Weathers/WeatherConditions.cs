using EnumsNET;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using TwilightShards.Common;
using static ClimatesOfFerngillRebuild.Sprites;
using System;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
{
    /// <summary>
    /// This class tracks the current the current weather of the game
    /// </summary>
    public class WeatherConditions
    {
        /// <summary>This Dictionary tracks elements associated with weathers </summary>
        public Dictionary<int, WeatherData> Weathers;

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

        /// <summary> Track the Moon - it may be used for weathers. </summary>
        private SDVMoon Moon;

        /// <summary>Track current conditions</summary>
        private CurrentWeather CurrentConditionsN { get; set; }

        /// <summary>The list of custom weathers </summary>
        internal List<ISDVWeather> CurrentWeathers { get; set; }

        //evening fog details
        private bool HasSetEveningFog {get; set;}
        public bool GenerateEveningFog { get; set; }

        /// <summary> This field is used to block fog generation due to other events (such as a solar eclipse)</summary>
        public bool BlockFog { get; set; }

        /// ******************************************************************************
        /// CONSTRUCTORS
        /// ******************************************************************************

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="Dice">pRNG</param>
        /// <param name="monitor">SMAPI log object</param>
        /// <param name="Config">Game configuration</param>
        public WeatherConditions(Icons Sheets, MersenneTwister Dice, ITranslationHelper Translation, IMonitor monitor, SDVMoon Termina, WeatherConfig Config)
        {
            this.Monitor = monitor;
            this.ModConfig = Config;
            this.Dice = Dice;
            this.Translation = Translation;
            this.Moon = Termina;
            this.Weathers = new Dictionary<int, WeatherData>()
            {
                {(int)CurrentWeather.Sunny, new WeatherData(WeatherIcon.IconSunny, WeatherIcon.IconSunny, "sunny", Translation.Get("weather_sunny_daytime" ), CondDescNight: Translation.Get("weather_sunny_nighttime"))},

                {(int)(CurrentWeather.Sunny | CurrentWeather.Lightning), new WeatherData(WeatherIcon.IconDryLightning, WeatherIcon.IconDryLightning, "drylightning", Translation.Get("weather_drylightning"), IsSpecial: true)},

                {(int)(CurrentWeather.Sunny | CurrentWeather.Frost), new WeatherData(WeatherIcon.IconSunny, WeatherIcon.IconSunny, CondName: "sunnyfrost", CondDesc: Translation.Get("weather_frost", new {condition = Translation.Get("weather_sunny") } ), IsSpecial: true, CondDescNight:Translation.Get("weather_frost_night") ) },

                {(int)(CurrentWeather.Sunny | CurrentWeather.Heatwave), new WeatherData(WeatherIcon.IconSunny, WeatherIcon.IconSunny, CondName: "sunnyheatwave", CondDesc: Translation.Get("weather_heatwave"), IsSpecial: true) },

                {(int)(CurrentWeather.Sunny | CurrentWeather.Fog), new WeatherData(WeatherIcon.IconSunnyFog, WeatherIcon.IconSunny, CondName: "fog", CondDesc: Translation.Get("weather_fog", new { condition = Translation.Get("weather_sunny") }), CondDescNight: Translation.Get("weather_frost_night")) },

            };

            CurrentConditionsN = CurrentWeather.Unset;
            CurrentWeathers = new List<ISDVWeather>
            {
                new FerngillFog(Sheets, Config.Verbose, monitor, Dice, Config, SDVTimePeriods.Morning),
                new FerngillWhiteOut(Dice, Config),
                new FerngillBlizzard(Dice, Config)
            };

            foreach (ISDVWeather weather in CurrentWeathers)
                weather.OnUpdateStatus += ProcessWeatherChanges;
        }

        /// *************************************************************************
        /// ACCESS METHODS
        /// *************************************************************************
        public CurrentWeather GetCurrentConditions()
        {
            return CurrentConditionsN;
        }

        public void DrawWeathers()
        {
            foreach (ISDVWeather weather in CurrentWeathers)
                weather.DrawWeather();
        }

        public void MoveWeathers()
        {
            foreach (ISDVWeather weather in CurrentWeathers)
                weather.MoveWeather();
        }
        
        public string FogDescription(double fogRoll, double fogChance)
        {
            string desc = "";
            foreach (ISDVWeather weather in CurrentWeathers)
            {
               if (weather.WeatherType == "Fog")
                {
                    FerngillFog fog = (FerngillFog)weather;
                    desc += fog.FogDescription(fogRoll, fogChance);
                }
            }

            return desc;
        }

        public void CreateWeather(string Type, bool IsMorningFog = false)
        {
            foreach (ISDVWeather weather in CurrentWeathers)
            {
                if (weather.WeatherType == Type)
                    weather.CreateWeather();
            }
        }

        public void TenMinuteUpdate()
        {
            foreach (ISDVWeather weather in CurrentWeathers)
            {
                weather.UpdateWeather();
            }

            //update fog for the evening
            if (SDVTime.CurrentTimePeriod == SDVTimePeriods.Afternoon && GenerateEveningFog && !HasSetEveningFog && (!IsFestivalToday && !IsWeddingToday))
            {
                //Get fog instance
                FerngillFog ourFog = (FerngillFog)this.GetWeatherMatchingType("Fog").First();
                if (!ourFog.WeatherInProgress)
                {
                    ourFog.SetEveningFog();
                    HasSetEveningFog = true;
                }
            }

            //if it's a blood moon out..
            if (Moon.CurrentPhase == MoonPhase.BloodMoon)
            {
                if (this.GetWeatherMatchingType("Fog").First().IsWeatherVisible)
                {
                    //Get fog instance
                    FerngillFog ourFog = (FerngillFog)this.GetWeatherMatchingType("Fog").First();
                    ourFog.BloodMoon = true;
                }
            }
        }

        internal List<ISDVWeather> GetWeatherMatchingType(string type)
        {
            List<ISDVWeather> Weathers = new List<ISDVWeather>();
            foreach (ISDVWeather weather in CurrentWeathers)
            {
                if (weather.WeatherType == type)
                    Weathers.Add(weather);
            }

            return Weathers;
        }

        /// <summary>Rather than track the weather seprately, always get it from the game.</summary>
        public CurrentWeather TommorowForecast => ConvertToCurrentWeather(Game1.weatherForTomorrow);

        public bool IsTodayTempSet => TodayTemps != null;
        public bool IsTomorrowTempSet => TomorrowTemps != null;
        public bool IsFestivalToday => CurrentConditionsN.HasFlag(CurrentWeather.Festival);
        public bool IsWeddingToday => CurrentConditionsN.HasFlag(CurrentWeather.Wedding);

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
            CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Unset);

            //Some flags are contradictory. Fix that here.
            if (newWeather == CurrentWeather.Rain)
            {
                //unset debris, sunny, snow and blizzard, if it's raining.
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Sunny)
            {
                //unset debris, rain, snow and blizzard, if it's sunny.
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Wind)
            {
                //unset sunny, rain, snow and blizzard, if it's debris.
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Snow)
            {
                //unset debris, sunny, snow and blizzard, if it's raining.
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Frost)
            {
                CurrentConditionsN.RemoveFlags(CurrentWeather.Heatwave);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Heatwave)
            {
                CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Frost);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Wedding || newWeather == CurrentWeather.Festival)
            {
                CurrentConditionsN = newWeather; //Clear *everything else* if it's a wedding or festival.
            }

            else
                CurrentConditionsN |= newWeather;
        }

        internal void ForceEveningFog()
        {
            //Get fog instance
            List<ISDVWeather> fogWeather = this.GetWeatherMatchingType("Fog");
            foreach (ISDVWeather weat in fogWeather)
            {
                SDVTime BeginTime, ExpirTime;
                BeginTime = new SDVTime(Game1.getStartingToGetDarkTime());
                BeginTime.AddTime(Dice.Next(-15, 90));

                ExpirTime = new SDVTime(BeginTime);
                ExpirTime.AddTime(Dice.Next(120, 310));

                BeginTime.ClampToTenMinutes();
                ExpirTime.ClampToTenMinutes();
                weat.SetWeatherTime(BeginTime, ExpirTime);
            }
        }

        /// <summary> Syntatic Sugar for Enum.HasFlag(). Done so if I choose to rewrite how it's accessed, less rewriting of invoking functions is needed. </summary>
        /// <param name="checkWeather">The weather being checked.</param>
        /// <returns>If the weather is present</returns>
        public bool HasWeather(CurrentWeather checkWeather)
        {
            return CurrentConditionsN.HasFlag(checkWeather);
        }

        public bool HasPrecip()
        {
            if (CurrentConditionsN.HasAnyFlags(CurrentWeather.Snow | CurrentWeather.Rain | CurrentWeather.Blizzard))
                return true;

            return false;
        }       
  
        private void ProcessWeatherChanges(object sender, WeatherNotificationArgs e)
        {
            if (e.Weather == "WhiteOut")
            {
                if (e.Present)
                {
                    CurrentConditionsN = CurrentConditionsN | CurrentWeather.WhiteOut;
                }
                else
                {
                    CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.WhiteOut);
                }
            }

            if (e.Weather == "Fog")
            {
                if (e.Present)
                {
                    CurrentConditionsN = CurrentConditionsN | CurrentWeather.Fog;
                }
                else
                {
                    CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);
                }
            }

            if (e.Weather == "Blizzard")
            {
  
                if (e.Present)
                    CurrentConditionsN = CurrentConditionsN | CurrentWeather.Blizzard;
                else
                    CurrentConditionsN = CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
            }
        }

        public bool ContainsCondition(CurrentWeather cond)
        {
            if (CurrentConditionsN.HasFlag(cond))
            {
                return true;
            }

            return false;
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
            foreach (ISDVWeather weather in CurrentWeathers)
                weather.OnNewDay();

            CurrentConditionsN = CurrentWeather.Unset;
            TodayTemps = TomorrowTemps; //If Tomorrow is null, should just allow it to be null.
            TomorrowTemps = null;
        }

        /// <summary> This function resets the weather object to basic. </summary>
        public void Reset()
        {
            foreach (ISDVWeather weather in CurrentWeathers)
                weather.Reset();

            TodayTemps = null;
            TomorrowTemps = null;
            CurrentConditionsN = CurrentWeather.Unset;
        }

        public SDVTime GetFogTime()
        {
            foreach (ISDVWeather weather in CurrentWeathers)
            {
                if (weather is FerngillFog f)
                {
                    if (f.IsWeatherVisible)
                        return f.WeatherExpirationTime;
                    else
                        return new SDVTime(600);
                }
            }

            return new SDVTime(600);
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

            //check current weathers.
            foreach (ISDVWeather weat in CurrentWeathers)
            {
                if (weat.WeatherType == "Fog" && weat.IsWeatherVisible)
                    CurrentConditionsN |= CurrentWeather.Fog;
                if (weat.WeatherType == "Blizzard" && weat.IsWeatherVisible)
                    CurrentConditionsN |= CurrentWeather.Blizzard;
                if (weat.WeatherType == "WhiteOut" && weat.IsWeatherVisible)
                    CurrentConditionsN |= CurrentWeather.WhiteOut;
            }
        }

        /// <summary>
        /// This function returns a description of the object. A very important note that this is meant for debugging, and as such does not do localization.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += $"Low for today is {TodayTemps?.LowerBound.ToString("N3")} with the high being {TodayTemps?.HigherBound.ToString("N3")}. The current conditions are {GetWeatherType()}.";

            foreach (ISDVWeather weather in CurrentWeathers)
                ret += weather.ToString() + Environment.NewLine;
            
            ret += $"Weather set for tommorow is {WeatherConditions.GetWeatherType(WeatherConditions.ConvertToCurrentWeather(Game1.weatherForTomorrow))} with high {TomorrowTemps?.HigherBound.ToString("N3")} and low {TomorrowTemps?.LowerBound.ToString("N3")}. Evening fog generated {GenerateEveningFog} ";

            return ret;
        }
        
        internal bool TestForSpecialWeather(double fogChance)
        {
            bool specialWeatherTriggered = false;
            // Conditions: Blizzard - occurs in weather_snow in "winter"
            //             Dry Lightning - occurs if it's sunny in any season if temps exceed 25C.
            //             Frost and Heatwave check against the configuration.
            //             Thundersnow  - as Blizzard, but really rare.
            //             Fog - per climate, although night fog in winter is double normal chance

            GenerateEveningFog = (Dice.NextDouble() < (Game1.currentSeason == "winter" ? fogChance * 2 : fogChance)) && !this.GetCurrentConditions().HasFlag(CurrentWeather.Wind);

            if (BlockFog)
                GenerateEveningFog = false;
            
            double fogRoll = Dice.NextDoublePositive();

            if (fogRoll < fogChance && !this.GetCurrentConditions().HasFlag(CurrentWeather.Wind) && !BlockFog)
            {
                this.CreateWeather("Fog", true);

                if (ModConfig.Verbose)
                    Monitor.Log($"{FogDescription(fogRoll, fogChance)}");

                specialWeatherTriggered = true;
            }

            if (this.HasWeather(CurrentWeather.Snow))
            {
                double blizRoll = Dice.NextDoublePositive();
                if (blizRoll <= ModConfig.BlizzardOdds)
                {
                    this.CreateWeather("Blizzard");
                    if (ModConfig.Verbose)
                        Monitor.Log($"With roll {blizRoll.ToString("N3")} against {ModConfig.BlizzardOdds}, there will be blizzards today");
                    if (Dice.NextDoublePositive() < .05)
                    {
                        this.CreateWeather("WhiteOut");
                    }
                }

                specialWeatherTriggered = true;
            }

            //Dry Lightning is also here for such like the dry and arid climates 
            //  which have so low rain chances they may never storm.
            if (this.HasWeather(CurrentWeather.Snow))
            {
                double oddsRoll = Dice.NextDoublePositive();

                if (oddsRoll <= ModConfig.ThundersnowOdds)
                {
                    this.AddWeather(CurrentWeather.Lightning);
                    if (ModConfig.Verbose)
                        Monitor.Log($"With roll {oddsRoll.ToString("N3")} against {ModConfig.ThundersnowOdds}, there will be thundersnow today");

                    specialWeatherTriggered = true;
                }
            }

            if (!(this.HasPrecip()))
            {
                double oddsRoll = Dice.NextDoublePositive();

                if (oddsRoll <= ModConfig.DryLightning && this.TodayHigh >= ModConfig.DryLightningMinTemp)
                {
                    this.AddWeather(CurrentWeather.Lightning);
                    if (ModConfig.Verbose)
                        Monitor.Log($"With roll {oddsRoll.ToString("N3")} against {ModConfig.DryLightning}, there will be dry lightning today.");

                    specialWeatherTriggered = true;
                }

                if (this.TodayHigh > ModConfig.TooHotOutside && ModConfig.HazardousWeather)
                {
                    this.AddWeather(CurrentWeather.Heatwave);
                    specialWeatherTriggered = true;
                }
            }

            if (this.TodayLow < ModConfig.TooColdOutside && !Game1.IsWinter)
            {
                if (ModConfig.HazardousWeather)
                {
                    this.AddWeather(CurrentWeather.Frost);
                    specialWeatherTriggered = true;
                }
            }

            //test for spring conversion.- 50% chance
            if (this.HasWeather(CurrentWeather.Rain) && this.HasWeather(CurrentWeather.Frost) && Game1.currentSeason == "spring" && Dice.NextDoublePositive() <= .5)
            {
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN |= CurrentWeather.Snow;
                Game1.isRaining = false;
                Game1.isSnowing = true;
                specialWeatherTriggered = true;
            }


            return specialWeatherTriggered;
        }
    }
}
