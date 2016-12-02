using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ClimatesOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig ModConfig { get; private set; } 
        bool gameloaded { get; set; }
        int weatherAtStartDay { get; set; }
        FerngillWeather currWeather { get; set; } = FerngillWeather.None;

        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
          ModConfig = helper.ReadConfig<ClimateConfig>();
          PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
          TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
          TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
        }

        private void LogEvent(string msg)
        {
            if (ModConfig.SuppressLog) this.Monitor.Log("[Climate] " + msg);
        }

        public string descWeatherHUD()
        {
            //simple description
            if (Game1.isDebrisWeather) return " very windy outside.";
            if (Game1.isRaining && !Game1.isLightning) return " raining outside";
            if (Game1.isSnowing) return " snowing outside";
            if (Game1.isLightning) return " very stormy outside";

            return " sunny outside"; 
        }

        public void checkForDangerousWeather(bool hud = true)
        {
            if (currWeather == FerngillWeather.Blizzard) {
                Game1.hudMessages.Add(new HUDMessage("There's a dangerous blizzard out today. Be careful!"));
                return;
            }

            if (currWeather == FerngillWeather.Heatwave)
            {
                Game1.hudMessages.Add(new HUDMessage("A massive heatwave is sweeping the valley. Stay hydrated!"));
                return;
            }

            Game1.hudMessages.Add(new HUDMessage("No abnormally dangerous weather.",2));
        }

        public void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            //get weather.
            string weatherMsg = "The weather is" + descWeatherHUD();
            if (Game1.timeOfDay == 620 && ModConfig.HUDDescription)
            {
                Game1.hudMessages.Add(new HUDMessage(weatherMsg, 2));
                //SOON: checkForDangerousWeather();  
                Game1.playSound("yoba");           
            }
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
                Game1.weatherForTomorrow = Game1.weather_festival;
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
            fixTV();

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

        private void fixTV()
        {
            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 1)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return;
            }

            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 2)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return;
            }

            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return;
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 25)
            {
                Game1.weatherForTomorrow = Game1.weather_lightning;
                return;
            }
        }

        private void overrideWeather()
        {
            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && ModConfig.AllowSnowOnFall28)
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.
        }

    }
}

