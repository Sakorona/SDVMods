using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace ClimatesOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig ModConfig { get; private set; } 
        bool gameloaded;

        public override void Entry(params object[] objects)
        {
          ModConfig = new ClimateConfig().InitializeConfig(BaseConfigPath);
          StardewModdingAPI.Events.PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
          StardewModdingAPI.Events.TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
          Log.SyncColour("Climates of Ferngill rev20161014 by KoihimeNakamura but based off of Alpha_Omegasis's More Rain", ConsoleColor.Cyan);
        }

        private void LogEvent(string msg)
        {
            if (ModConfig.SuppressLog) Log.Debug("[Climate] " + msg);
        }

        public void TimeEvents_DayOfMonthChanged(object sender, StardewModdingAPI.Events.EventArgsIntChanged e)
        {
            if (gameloaded == false) return;
            UpdateWeather();
        }

        public void PlayerEvents_LoadedGame(object sender, StardewModdingAPI.Events.EventArgsLoadedGameChanged e)
        {
            gameloaded = true;
            UpdateWeather();
        }


        void UpdateWeather(){
            //sanity check - wedding
            if (Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                LogEvent("There is no Alanis Morissetting here. Enjoy your wedding.");
                return;
            }

            Random rng = new Random(Guid.NewGuid().GetHashCode());
            double genNumber = rng.NextDouble();

           switch (Game1.currentSeason)
            {
                case "spring":
                    if (genNumber < (ModConfig.spgBaseRainChance + (ModConfig.spgRainChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is rainy outside.");
                        Game1.weatherForTomorrow = Game1.weather_rain;
                        genNumber = rng.NextDouble();
                        if (genNumber < ModConfig.spgConvRainToStorm)
                        {
                            LogEvent("Well, it turns out it's quite stormy outside...");
                            Game1.weatherForTomorrow = Game1.weather_lightning;
                        }
                    }

                    if (genNumber < (ModConfig.spgBaseStormChance + (ModConfig.spgStormChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is stormy outside.");
                        Game1.weatherForTomorrow = Game1.weather_lightning;
                        genNumber = rng.NextDouble();
                    }

                    if (genNumber < (ModConfig.spgBaseWindChance + (ModConfig.spgWindChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is windy out. Hey, is that Dorothy?");
                        Game1.weatherForTomorrow = Game1.weather_debris;
                    }

                    break;
                case "summer":
                    if (genNumber < (ModConfig.smrBaseRainChance + (ModConfig.smrRainChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is rainy outside.");
                        Game1.weatherForTomorrow = Game1.weather_rain;
                        genNumber = rng.NextDouble();
                        if (genNumber < ModConfig.smrConvRainToStorm)
                        {
                            LogEvent("Well, it turns out it's quite stormy outside...");
                            Game1.weatherForTomorrow = Game1.weather_lightning;
                        }
                    }


                    if (genNumber < (ModConfig.smrBaseStormChance + (ModConfig.smrStormChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is stormy outside.");
                        Game1.weatherForTomorrow = Game1.weather_lightning;
                        genNumber = rng.NextDouble();
                    }

                    if (genNumber < (ModConfig.smrBaseWindChance + (ModConfig.smrWindChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is windy out. Hey, is that Dorothy?");
                        Game1.weatherForTomorrow = Game1.weather_debris;
                    }

                    break;
                case "fall":
                    if (genNumber < (ModConfig.falBaseRainChance + (ModConfig.falRainChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is rainy outside.");
                        Game1.weatherForTomorrow = Game1.weather_rain;
                        genNumber = rng.NextDouble();
                        if (genNumber < ModConfig.falConvRainToStorm)
                        {
                            LogEvent("Well, it turns out it's quite stormy outside...");
                            Game1.weatherForTomorrow = Game1.weather_lightning;
                        }
                    }


                    if (genNumber < (ModConfig.falBaseStormChance + (ModConfig.falStormChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is stormy outside.");
                        Game1.weatherForTomorrow = Game1.weather_lightning;
                        genNumber = rng.NextDouble();
                    }

                    if (genNumber < (ModConfig.falBaseWindChance + (ModConfig.falWindChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is windy out. Hey, is that Dorothy?");
                        Game1.weatherForTomorrow = Game1.weather_debris;
                    }

                    if (Game1.dayOfMonth == 28)
                        Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.

                    break;
                case "winter":

                    if (genNumber < (ModConfig.winBaseWindChance + (ModConfig.winWindChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is windy out. Hey, is that Dorothy?");
                        Game1.weatherForTomorrow = Game1.weather_debris;
                    }

                    if (genNumber < (ModConfig.winBaseSnowChance + (ModConfig.winSnowChanceIncrease * Game1.dayOfMonth)))
                    {
                        LogEvent("It is quite snowy out. Got hot chocolate?");
                        Game1.weatherForTomorrow = Game1.weather_snow;
                    }

                    break;
                default:
                    Game1.weatherForTomorrow = Game1.weather_sunny; //fail safe.
                    break;
            }

            //overrides
            Game1.weatherForTomorrow = Game1.weather_debris;

           
            

        }
    }
}

