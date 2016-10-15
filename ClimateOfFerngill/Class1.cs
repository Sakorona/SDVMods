using System;
using StardewModdingAPI;
using StardewValley;

namespace MoreRain
{
    public class MoreRain : Mod
    {
        public ModConfig ModConfig { get; private set; } 
        bool gameloaded;

        public override void Entry(params object[] objects)
        {
          ModConfig = new ModConfig().InitializeConfig(BaseConfigPath);
          StardewModdingAPI.Events.PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
          StardewModdingAPI.Events.TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
          Log.Info("MoreRain rev20161014, originally by AlphaOmegasis and reworked by KoihimeNakamura");
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
            //sanity check - not entirely neccesary. 
            if (Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                if (!ModConfig.SuppressLog) Log.Info("[MoreRain] There is no Alanis Morissetting here. Enjoy your wedding.");
                return;
            }
         
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            int genNumber = rng.Next(0, 100);

            if (genNumber < ModConfig.RainChance)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                if (!ModConfig.SuppressLog) Log.Info("[MoreRain] It will rain tomorrow.");
                genNumber = rng.Next(0, 100); //roll again!

                if (genNumber <= ModConfig.StormChance)
                {
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                    if (!ModConfig.SuppressLog) Log.Info("[MoreRain] Thunder and lightning, very very frightening me!.");
                }
            }
            else
            {
                if (genNumber < ModConfig.WindyChance)
                {
                    Game1.weatherForTomorrow = Game1.weather_debris;
                    if (!ModConfig.SuppressLog) Log.Info("[More Rain] Please remember to let Dorothy in.");
                }

                Game1.weatherForTomorrow = Game1.weather_sunny; //default weather.
            }

            if (Game1.currentSeason == "winter")
            {
                if (genNumber < ModConfig.SnowyChance)
                {
                    Game1.weatherForTomorrow = Game1.weather_snow;
                    if (!ModConfig.SuppressLog) Log.Info("[MoreRain] The snow falls on the sad catgirl.");
                }

                if (Game1.weatherForTomorrow == Game1.weather_lightning || Game1.weatherForTomorrow == Game1.weather_rain)
                    Game1.weatherForTomorrow = Game1.weather_snow;
            }

            //overrides
            if (Game1.currentSeason == "fall" && Game1.dayOfMonth == 28)
            {
                Game1.weatherForTomorrow = Game1.weather_snow;
                if (!ModConfig.SuppressLog) Log.Info("[MoreRain] Winter is near.");
            }


            if ((Game1.weatherForTomorrow == Game1.weather_sunny) && (!ModConfig.SuppressLog)) 
                Log.Info("[MoreRain] The sunlight grows harsh.");

            if (!ModConfig.SuppressLog) Log.Info("[MoreRain] More Rain has updated.");

        }
    }
}

