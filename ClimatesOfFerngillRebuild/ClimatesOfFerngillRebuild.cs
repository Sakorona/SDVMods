using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwilightCore;
using TwilightCore.PRNG;
using StardewModdingAPI;

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
        }

    }
}
