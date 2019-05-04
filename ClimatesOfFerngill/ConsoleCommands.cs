using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Linq;
using TwilightShards.Stardew.Common;

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
                    WeatherUtilities.SetWeatherRain();
                    Game1.updateWeatherIcon();
                    Logger.Log(Translator.Get("console-text.weatherset_rain"), LogLevel.Info);
                    break;
                case "vrain":
                    WeatherUtilities.SetWeatherRain();
                    ClimatesOfFerngill.ForceVariableRain();
                    Game1.updateWeatherIcon();
                    Logger.Log(Translator.Get("console-text.weatherset_rain"), LogLevel.Info);
                    break;
                case "storm":
                    WeatherUtilities.SetWeatherStorm();
                    Game1.updateWeatherIcon();
                    Logger.Log(Translator.Get("console-text.weatherset_storm"), LogLevel.Info);
                    break;
                case "snow":
                    WeatherUtilities.SetWeatherSnow();
                    Game1.updateWeatherIcon();
                    Logger.Log(Translator.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
                case "debris":
                    WeatherUtilities.SetWeatherDebris();
                    Game1.updateWeatherIcon();
                    Logger.Log(Translator.Get("console-text.weatherset_debris", LogLevel.Info));
                    break;
                case "sunny":
                    WeatherUtilities.SetWeatherSunny();
                    Game1.updateWeatherIcon();
                    Logger.Log(Translator.Get("console-text.weatherset_sun", LogLevel.Info));
                    break;
                case "blizzard":
                    WeatherUtilities.SetWeatherSnow();
                    Game1.updateWeatherIcon();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().CreateWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().SetWeatherBeginTime(new SDVTime(0600));
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().SetWeatherExpirationTime(new SDVTime(2800));
                    Logger.Log(Translator.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
                case "fog":
                    WeatherUtilities.SetWeatherSunny();
                    Game1.updateWeatherIcon();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().CreateWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().SetWeatherBeginTime(new SDVTime(0600));
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().SetWeatherBeginTime(new SDVTime(2800));
                    break;
                case "whiteout":
                    WeatherUtilities.SetWeatherSnow();
                    Game1.updateWeatherIcon();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().CreateWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().CreateWeather();
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().SetWeatherBeginTime(new SDVTime(0600));
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().SetWeatherBeginTime(new SDVTime(0600));
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().SetWeatherExpirationTime(new SDVTime(2800));
                    ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().SetWeatherExpirationTime(new SDVTime(2800));
                    Logger.Log(Translator.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
            }

            Game1.updateWeatherIcon();
            ClimatesOfFerngill.Conditions.SetTodayWeather();
            ClimatesOfFerngill.Conditions.GenerateWeatherSync();
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

        internal static void ShowSpecialWeather(string arg1, string[] arg2)
        {
           Logger.Log(ClimatesOfFerngill.Conditions.PrintWeather());
        }
    }
}
