using System;
using StardewModdingAPI;
using StardewValley;

namespace ClimatesOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ModConfig ModConfig { get; private set; } 
        bool gameloaded;

        public override void Entry(params object[] objects)
        {
          ModConfig = new ModConfig().InitializeConfig(BaseConfigPath);
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
            //sanity check - not entirely neccesary. 
            if (Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                LogEvent("[MoreRain] There is no Alanis Morissetting here. Enjoy your wedding.");
                return;
            }
         
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            int genNumber = rng.Next(0, 100);



        }
    }
}

