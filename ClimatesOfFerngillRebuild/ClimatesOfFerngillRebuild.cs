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

namespace ClimatesOfFerngillRebuild
{
    public class ClimatesOfFerngillRebuild : Mod
    {
        /// <summary> The options file </summary>
        private WeatherConfig WeatherOpt { get; set; }

        /// <summary> The pRNG object </summary>
        private MersenneTwister Dice;

        /// <summary> Fixed path for Weather Files </summary>
        private static string WeatherPath = "data/Weather/";

        public override void Entry(IModHelper helper)
        {
            WeatherOpt = helper.ReadConfig<WeatherConfig>();
            Dice = new MersenneTwister();

            //subscribe to events
            SaveEvents.AfterLoad += InitiateMod;
            TimeEvents.AfterDayStarted += HandleNewDay;
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void InitiateMod(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
