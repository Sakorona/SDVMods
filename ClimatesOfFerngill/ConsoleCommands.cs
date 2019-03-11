using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Linq;

namespace ClimatesOfFerngillRebuild
{
    internal static class ConsoleCommands
    {
        private static ITranslationHelper Translator;
        private static IMonitor Logger;


        public static void Init()
        {
            Translator = ClimatesOfFerngill.Translator;
            Logger = ClimatesOfFerngill.Logger;
        }

        /// <summary>
        /// This function changes the weather (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        public static void WeatherChangeFromConsole(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            string ChosenWeather = arg2[0];

            switch (ChosenWeather)
            {
                case "rain":
                    Game1.isSnowing = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    Logger.Log(Translator.Get("console-text.weatherset_rain"), LogLevel.Info);
                    break;
                case "storm":
                    Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.isLightning = Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    Logger.Log(Translator.Get("console-text.weatherset_storm"), LogLevel.Info);
                    break;
                case "snow":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.debrisWeather.Clear();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    Logger.Log(Translator.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
                case "debris":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = false;
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    Game1.isDebrisWeather = true;
                    Game1.populateDebrisWeatherArray();
                    Logger.Log(Translator.Get("console-text.weatherset_debris", LogLevel.Info));
                    break;
                case "sunny":
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    Game1.debrisWeather.Clear();
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = Game1.isDebrisWeather = false;
                    Logger.Log(Translator.Get("console-text.weatherset_sun", LogLevel.Info));
                    break;
                case "blizzard":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.debrisWeather.Clear();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().CreateWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    Logger.Log(Translator.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
                case "whiteout":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.debrisWeather.Clear();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().CreateWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().CreateWeather();
                    Logger.Log(Translator.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
            }

            Game1.updateWeatherIcon();
            ClimatesOfFerngill.Conditions.SetTodayWeather();
        }

        /// <summary>
        /// This function changes the weather for tomorrow (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        public static void TomorrowWeatherChangeFromConsole(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            string chosenWeather = arg2[0];
            switch (chosenWeather)
            {
                case "rain":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_rain;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwrain"), LogLevel.Info);
                    break;
                case "storm":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_lightning;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwstorm"), LogLevel.Info);
                    break;
                case "snow":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_snow;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwsnow"), LogLevel.Info);
                    break;
                case "debris":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_debris;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwdebris"), LogLevel.Info);
                    break;
                case "festival":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_festival;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwfestival"), LogLevel.Info);
                    break;
                case "sun":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_sunny;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwsun"), LogLevel.Info);
                    break;
                case "wedding":
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_wedding;
                    Logger.Log(Translator.Get("console-text.weatherset-tmrwwedding"), LogLevel.Info);
                    break;
            }
        }

        public static void ClearSpecial(string arg1, string[] arg2)
        {
            ClimatesOfFerngill.Conditions.ClearAllSpecialWeather();
        }

        public static void OutputWeather(string arg1, string[] arg2)
        {
            var retString = $"Weather for {SDate.Now()} is {ClimatesOfFerngill.Conditions.ToString()}. {Environment.NewLine} System flags: isRaining {Game1.isRaining} isSnowing {Game1.isSnowing} isDebrisWeather: {Game1.isDebrisWeather} isLightning {Game1.isLightning}, with tommorow's set weather being {Game1.weatherForTomorrow}";
            Logger.Log(retString);
        }
    }
}
