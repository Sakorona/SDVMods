using System;
using StardewModdingAPI;
using StardewValley;

namespace ClimatesOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig ModConfig { get; private set; } 
        bool gameloaded { get; set; }
        public int weatherAtStartDay { get; set; }

        public string Name { get; } = "Climates of Ferngill";
        public string Author { get; } = "KoihimeNakamura";
        public string Version { get; } = "0.7.0";
        public string Description { get; } = "Creates a more complex weather system";
        
        public override void Entry(params object[] objects)
        {
          ModConfig = new ClimateConfig().InitializeConfig(BaseConfigPath);
          StardewModdingAPI.Events.PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
          StardewModdingAPI.Events.TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
          
          Log.SyncColour(Name + " " + Version+ " by KoihimeNakamura but based off of Alpha_Omegasis's More Rain", ConsoleColor.Cyan);
        }

        private void LogEvent(string msg)
        {
            if (ModConfig.SuppressLog) Log.Info("[Climate] " + msg);
        }

        public void TimeEvents_DayOfMonthChanged(object sender, StardewModdingAPI.Events.EventArgsIntChanged e)
        {
            if (gameloaded == false) return;
            UpdateWeather();
        }

        public void PlayerEvents_LoadedGame(object sender, StardewModdingAPI.Events.EventArgsLoadedGameChanged e)
        {
            gameloaded = true;
        }

        void UpdateWeather(){
            //sanity check - wedding
            if (Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                LogEvent("There is no Alanis Morissetting here. Enjoy your wedding.");
                return;
            }

            //sanity check - festival
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.weatherForTomorrow = 4;
                Game1.questOfTheDay = null;
                return;
            }

            if (Game1.weatherForTomorrow == Game1.weather_festival)
            {
                LogEvent("A festival!");
                return;
            }

            //rain totem
            if (weatherAtStartDay != Game1.weatherForTomorrow)
            {
                if (Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    LogEvent("Rain totem used, aborting weather change");
                    return;
                }
            }

            //TV forces.
            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return;
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 12)
            {
                Game1.weatherForTomorrow = Game1.weather_lightning;
                return;
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 24)
            {
                Game1.weatherForTomorrow = Game1.weather_lightning;
                return;
            }

            //now on to the main program

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
                        if (genNumber < ModConfig.spgConvRainToStorm && CanWeStorm())
                        {
                            LogEvent("Well, it turns out it's quite stormy outside...");
                            Game1.weatherForTomorrow = Game1.weather_lightning;
                        }
                    }

                    if (genNumber < (ModConfig.spgBaseStormChance + (ModConfig.spgStormChanceIncrease * Game1.dayOfMonth)) && CanWeStorm())
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

            overrideWeather();
            weatherAtStartDay = Game1.weatherForTomorrow;
        }

        private bool CanWeStorm()
        {
            if (Game1.year == 1 && Game1.currentSeason == "spring") return ModConfig.AllowStormsFirstSpring;
            else return true;
        }

        private void overrideWeather()
        {
            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && ModConfig.AllowSnowOnFall28)
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.
        }

    }
}

