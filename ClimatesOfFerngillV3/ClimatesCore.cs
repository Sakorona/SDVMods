using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClimatesOfFerngillV3.ModelData;
using StardewModdingAPI;
using StardewValley;
using TwilightShards.Common;

namespace ClimatesOfFerngillV3
{
    public class ClimatesCore : Mod
    {
        internal static MersenneTwister Dice;
        internal static IMonitor Logger;
        internal ClimateConfig WeatherOptions;
        private FerngillClimate GameClimate;
        internal ClimatesSaveData SaveData;
        internal WeatherConditions CurrentConditions;

        public override void Entry(IModHelper helper)
        {
            bool Disabled = false;
            var events = Helper.Events;
            WeatherOptions = helper.ReadConfig<ClimateConfig>();
            Logger = Monitor;
            Dice = new MersenneTwister();
            CurrentConditions = new WeatherConditions();

            //Load Weather Data.
            if (WeatherOptions.Verbose) 
                Monitor.Log($"Loading climate type: {WeatherOptions.ClimateType} from file", LogLevel.Trace);

            var path = Path.Combine("data", "weather", WeatherOptions.ClimateType + ".json");
            GameClimate = helper.Data.ReadJsonFile<FerngillClimate>(path);

            if (GameClimate is null)
            {
                Monitor.Log($"The required '{path}' file is missing. Try reinstalling the mod to fix that.", LogLevel.Error);
                Monitor.Log("This mod will now disable itself.", LogLevel.Error);
                Disabled = true;
            }

            if (Disabled) return;

            //Data exists, move on to loading the mod.
            events.GameLoop.DayStarted += StartTheNewDay;
            events.GameLoop.ReturnedToTitle += ResetMod;

            //events.Multiplayer.ModMessageReceived += HandleMessages;
            //handle data being written to save
            events.GameLoop.Saving += UpdateSaveData;
            events.GameLoop.SaveLoaded += OnSaveLoad; 
            
        }

        private void HandleMessages(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpdateSaveData(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            Helper.Data.WriteSaveData<ClimatesSaveData>("climate-ferngill", SaveData);
        }

        private void OnSaveLoad(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            SaveData = this.Helper.Data.ReadSaveData<ClimatesSaveData>("climate-ferngill");
            if (SaveData is null)
            {
                SaveData = new ClimatesSaveData()
                {
                    WeatherSystemInProgress = false,
                    WeatherSystemDaysRemaining = 0,
                    WeatherType = (int)WeatherType.Sunny 
                };
            }

        }

        private void SanityCheckingSaveData()
        {
            //Sanity check.
            if (SaveData.DaysSinceLastRain < 0)
                SaveData.DaysSinceLastRain = 0;
            if (SaveData.RainWithinLastWeek < 0)
                SaveData.RainWithinLastWeek = 0;

            else if (SaveData.WeatherSystemInProgress)
            {
                //Sanity parsing!
                if (!(Enum.TryParse(SaveData.WeatherType.ToString(), true, out WeatherType result) && Enum.IsDefined(typeof(WeatherType), result)))
                {
                    SaveData.WeatherType = (int)WeatherType.Sunny;
                }

                if (SaveData.WeatherSystemDaysRemaining < 0)
                    SaveData.WeatherSystemDaysRemaining = 0;
                if (SaveData.WeatherSystemDaysRemaining > 4)
                    SaveData.WeatherSystemDaysRemaining = 4;
            }

            if (!SaveData.WeatherSystemInProgress)
            {
                SaveData.WeatherType = (int)WeatherType.Sunny;
                SaveData.WeatherSystemDaysRemaining = 0;
            }

        }

        private void ResetMod(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
           CurrentConditions.Reset();
           //reset loaded SaveData
           SaveData = null;
        }

        private void StartTheNewDay(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
           //Read from the model, verify data isn't insane.
           if (Game1.IsMasterGame)
           {

           }
           else
           {

           }
        }
    }
}
