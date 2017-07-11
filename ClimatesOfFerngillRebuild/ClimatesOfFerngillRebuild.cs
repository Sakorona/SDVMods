using System;
using System.IO;

using StardewValley;
using TwilightCore.PRNG;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using TwilightCore;
using TwilightCore.StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ClimatesOfFerngillRebuild
{
    public class ClimatesOfFerngillRebuild : Mod
    {
        public static Dictionary<SDate, int> ForceDays = new Dictionary<SDate, int>
            {
                { new SDate(1,"spring"), Game1.weather_sunny },
                { new SDate(2, "spring"), Game1.weather_sunny },
                { new SDate(4, "spring"), Game1.weather_sunny },
                { new SDate(13, "spring"), Game1.weather_festival },
                { new SDate(24, "spring"), Game1.weather_festival },
                { new SDate(1, "summer"), Game1.weather_sunny },
                { new SDate(11, "summer"), Game1.weather_festival },
                { new SDate(13, "summer"), Game1.weather_lightning },
                { new SDate(25, "summer", 25), Game1.weather_lightning },
                { new SDate(26, "summer", 26), Game1.weather_lightning },
                { new SDate(28, "summer", 28), Game1.weather_festival },
                { new SDate(1,"fall"), Game1.weather_sunny },
                { new SDate(16,"fall"), Game1.weather_festival },
                { new SDate(27,"fall"), Game1.weather_festival },
                { new SDate(1,"winter"), Game1.weather_sunny },
                { new SDate(8, "winter"), Game1.weather_festival },
                { new SDate(25, "winter"), Game1.weather_festival }
            };

        /// <summary> The options file </summary>
        private WeatherConfig WeatherOpt { get; set; }

        /// <summary> The pRNG object </summary>
        private MersenneTwister Dice;

        /// <summary> The current weather conditions </summary>
        private WeatherConditions CurrentWeather;

        /// <summary> The climate for the game </summary>
        private FerngillClimate GameClimate;

        /// <summary>
        /// Our fog object.
        /// </summary>
        private FerngillFog OurFog;

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>

        public override void Entry(IModHelper helper)
        {
            WeatherOpt = helper.ReadConfig<WeatherConfig>();
            Dice = new MersenneTwister();

            if (WeatherOpt.Verbose) Monitor.Log($"Loading climate type: {WeatherOpt.ClimateType} from file", LogLevel.Trace);

            string path = Path.Combine("data", "Weather", WeatherOpt.ClimateType + ".json");
            GameClimate = helper.ReadJsonFile<FerngillClimate>(path);

            CurrentWeather = new WeatherConditions();

            //subscribe to events
            SaveEvents.AfterLoad += InitiateMod;
            TimeEvents.AfterDayStarted += HandleNewDay;
            SaveEvents.AfterReturnToTitle += ResetMod;

            //console commands
            helper.ConsoleCommands
                  .Add("weather_settommorowweather", helper.Translation.Get("console-text.desc_tmrweather"), TmrwWeatherChangeFromConsole)
                  .Add("weather_setweather", helper.Translation.Get("console-text.desc_setweather"), WeatherChangeFromConsole);
        }

        private void ResetMod(object sender, EventArgs e)
        {
            CurrentWeather.Reset();
            OurFog.Reset();
        }

        private void InitiateMod(object sender, EventArgs e)
        {
            UpdateWeatherOnNewDay();
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            UpdateWeatherOnNewDay();
        }

        private void UpdateWeatherOnNewDay()
        {
            //reset for new day
            CurrentWeather.WillFog = false;
            CurrentWeather.UnusualWeather = SpecialWeather.None;
            OurFog.Reset();

            //Set Temperature for today and tommorow. Get today's conditions.
            //   If tomorrow is set, move it to today, and autoregen tomorrow.
            CurrentWeather.GetTodayWeather();
            if (!(CurrentWeather.TomorrowTemps == null))
            {
                CurrentWeather.TodayTemps = GameClimate.GetTemperatures(SDate.Now(), Dice);
            }
            else
                CurrentWeather.TodayTemps = new RangePair(CurrentWeather.TomorrowTemps);

            CurrentWeather.TomorrowTemps = GameClimate.GetTemperatures(SDate.Now().AddDays(1), Dice);
            if (WeatherOpt.Verbose)
                Monitor.Log($"Updated the temperature for tommorow and today. Setting weather for today... ", LogLevel.Trace);

            //if tomorrow is a festival or wedding, we need to set the weather and leave.
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.weatherForTomorrow = Game1.weather_festival;
                if (WeatherOpt.Verbose)
                    Monitor.Log($"Festival tomorrow. Aborting processing.", LogLevel.Trace);

                return;
            }

            if (Game1.countdownToWedding == 1)
            {
                Game1.weatherForTomorrow = Game1.weather_wedding;
                if (WeatherOpt.Verbose)
                    Monitor.Log($"Wedding tomorrow. Aborting processing.", LogLevel.Trace);

                return;
            }

            //now set tomorrow's weather
            var OddsForTheDay = GameClimate.GetClimateForDate(SDate.Now().AddDays(1));

            double rainDays = OddsForTheDay.RetrieveOdds(Dice, "rain", SDate.Now().AddDays(1).Day);
            double windyDays = OddsForTheDay.RetrieveOdds(Dice, "debris", SDate.Now().AddDays(1).Day);
            double stormDays = OddsForTheDay.RetrieveOdds(Dice, "storm", SDate.Now().AddDays(1).Day);
            double fogChance = OddsForTheDay.RetrieveOdds(Dice, "fog", SDate.Now().Day);

            ProbabilityDistribution<string> WeatherDist = new ProbabilityDistribution<string>("sunny");
            WeatherDist.AddNewEndPoint(rainDays, "rain");
            WeatherDist.AddNewCappedEndPoint(windyDays, "debris");

            if (!(WeatherDist.GetEntryFromProb(Dice.NextDoublePositive(), out string Result)))
            {
                Result = "sunny";
                Monitor.Log("The weather has failed to process in some manner. Falling back to [sunny]", LogLevel.Info);
            }

            //now parse the result.
            if (Result == "rain")
            {
                //snow applies first
                double MidPointTemp = CurrentWeather.TodayTemps.HigherBound - ((CurrentWeather.TodayTemps.HigherBound - CurrentWeather.TodayTemps.LowerBound) / 2);

                if (CurrentWeather.TodayTemps.HigherBound <= 2 || MidPointTemp <= 0)
                {
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"Snow is enabled, with the High for the day being: {CurrentWeather.TodayTemps.HigherBound}" +
                                    $" and the calculated midpoint temperature being {MidPointTemp}");

                    Game1.weatherForTomorrow = Game1.weather_snow;
                }
                else
                {
                    Game1.weatherForTomorrow = Game1.weather_rain;
                }

                if (!GameClimate.AllowRainInWinter && Game1.currentSeason == "winter" && Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    Game1.weatherForTomorrow = Game1.weather_snow;
                }

                //apply lightning logic.
                if (Dice.NextDoublePositive() >= stormDays && Game1.weatherForTomorrow == Game1.weather_rain)
                    Game1.weatherForTomorrow = Game1.weather_lightning;

                //now, update today's weather for fog and other special weathers.
                double fogRoll = Dice.NextDoublePositive();

                if (fogRoll > fogChance && CurrentWeather.TodayWeather != Game1.weather_debris)
                {
                    CurrentWeather.WillFog = true;

                    OurFog.CreateFog(FogAlpha: .55f, AmbientFog: true, FogColor: (Color.White * 1.35f));
                    Game1.globalOutdoorLighting = .5f;

                    if (Dice.NextDouble() < .15)
                    {
                        OurFog.IsDarkFog();
                        Game1.outdoorLight = new Color(227, 222, 211);
                    }
                    else
                    {
                        Game1.outdoorLight = new Color(179, 176, 171);
                    }

                    double FogTimer = Dice.NextDouble();
                    SDVTime FogExpirTime = new SDVTime(1200);

                    if (FogTimer > .75 && FogTimer <= .90)
                    {
                        FogExpirTime = new SDVTime(1120);
                    }
                    else if (FogTimer > .55 && FogTimer <= .75)
                    {
                        FogExpirTime = new SDVTime(1030);
                    }
                    else if (FogTimer > .30 && FogTimer <= .55)
                    {
                        FogExpirTime = new SDVTime(930);
                    }
                    else if (FogTimer <= .30)
                    {
                        FogExpirTime = new SDVTime(820);
                    }

                    CurrentWeather.FogTime = FogExpirTime;

                    if (WeatherOpt.Verbose)
                        Monitor.Log($"With roll {fogRoll} against {fogChance}, there will be fog today until {CurrentWeather.FogTime}");
                }
                
                //now special weathers

            }
        }

        private bool CheckForForceDay(SDate Target)
        {           
            if (Game1.year == 1 && Target.Season == "spring" && Target.Day == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return true;
            }



            foreach (KeyValuePair<SDate, int> entry in ForceDays)
            {
                if (entry.Key == Target)
                {
                    Game1.weatherForTomorrow = entry.Value;
                    return true;
                }
            }

            return false;
    }


        /* **************************************************************
         * console commands
         * **************************************************************
         */

        /// <summary>
        /// This function changes the weather (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        private void WeatherChangeFromConsole(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
                return;

            string ChosenWeather = arg2[0];

            switch (ChosenWeather)
            {
                case "rain":
                    Game1.isSnowing = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_rain"), LogLevel.Info);
                    break;
                case "storm":
                    Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.isLightning = Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_storm"), LogLevel.Info);
                    break;
                case "snow":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
                case "debris":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = false;
                    Game1.isDebrisWeather = true;
                    Game1.populateDebrisWeatherArray();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_debris", LogLevel.Info));
                    break;
                case "sunny":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = Game1.isRaining = false;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_sun", LogLevel.Info));
                    break;
            }

            Game1.updateWeatherIcon();
        }

        /// <summary>
        /// This function changes the weather for tomorrow (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        private void TmrwWeatherChangeFromConsole(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
                return;

            string ChosenWeather = arg2[0];
            switch (ChosenWeather)
            {
                case "rain":
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwrain"), LogLevel.Info);
                    break;
                case "storm":
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwstorm"), LogLevel.Info);
                    break;
                case "snow":
                    Game1.weatherForTomorrow = Game1.weather_snow;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwsnow"), LogLevel.Info);
                    break;
                case "debris":
                    Game1.weatherForTomorrow = Game1.weather_debris;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwdebris"), LogLevel.Info);
                    break;
                case "festival":
                    Game1.weatherForTomorrow = Game1.weather_festival;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwfestival"), LogLevel.Info);
                    break;
                case "sun":
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwsun"), LogLevel.Info);
                    break;
                case "wedding":
                    Game1.weatherForTomorrow = Game1.weather_wedding;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwwedding"), LogLevel.Info);
                    break;
            }
        }
    }
}
