using System;
using System.IO;

using StardewValley;
using TwilightCore.PRNG;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using TwilightCore;

namespace ClimatesOfFerngillRebuild
{
    public class ClimatesOfFerngillRebuild : Mod
    {
        /// <summary> The options file </summary>
        private WeatherConfig WeatherOpt { get; set; }

        /// <summary> The pRNG object </summary>
        private MersenneTwister Dice;

        /// <summary> The current weather conditions </summary>
        private WeatherConditions CurrentWeather;

        /// <summary> The climate for the game </summary>
        private FerngillClimate GameClimate;

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>

        public override void Entry(IModHelper helper)
        {
            WeatherOpt = helper.ReadConfig<WeatherConfig>();
            Dice = new MersenneTwister();
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
            CurrentWeather = new WeatherConditions(); //reset to a null weather
        }

        private void InitiateMod(object sender, EventArgs e)
        {
            //no information loaded, so
            CurrentWeather.TodayTemps = GameClimate.GetTemperatures(SDate.Now(), Dice);
            CurrentWeather.GetTodayWeather();
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpdateWeather()
        {

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
