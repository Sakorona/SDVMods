using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwilightCore;
using TwilightCore.PRNG;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using System.IO;

namespace ClimatesOfFerngillRebuild
{
    public class ClimatesOfFerngillRebuild : Mod
    {
        /// <summary> The options file </summary>
        private WeatherConfig WeatherOpt { get; set; }

        /// <summary> The pRNG object </summary>
        private MersenneTwister Dice;

        public override void Entry(IModHelper helper)
        {
            WeatherOpt = helper.ReadConfig<WeatherConfig>();
            Dice = new MersenneTwister();
            string path = Path.Combine("data", "Weather", WeatherOpt.ClimateType + ".json");
            FerngillClimate GameClimate = helper.ReadJsonFile<FerngillClimate>(path);

            //subscribe to events
            SaveEvents.AfterLoad += InitiateMod;
            TimeEvents.AfterDayStarted += HandleNewDay;
        }

        private void InitiateMod(object sender, EventArgs e)
        {
            
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpdateWeather()
        {

        }
    }
}
