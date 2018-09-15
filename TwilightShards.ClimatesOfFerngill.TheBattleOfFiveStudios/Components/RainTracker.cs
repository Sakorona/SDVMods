using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;

namespace TwilightShards.ClimatesOfFerngill.Components
{
    /// <summary>
    /// This is the rain accumulator and tracker.
    /// </summary>
    public class RainTracker
    {
        /// <summary>
        /// This value tracks how much rain the land can absorb in a day. And a day of normal light rain should be fine. Not that it's super relevant, but it's stored in mm.
        /// </summary>
        private readonly double rainAbsorb = 132;

        /// <summary>
        /// This is the current amount of rain being tracked
        /// </summary>
        protected double RainAmt;

        /// <summary>
        /// This is the number of days since it last rained
        /// </summary>
        protected int NumDaysSinceLastRain;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RainTracker()
        {
            RainAmt = 0;
            NumDaysSinceLastRain = 0;
        }

        /// <summary>
        /// This function handles new day calcs
        /// </summary>
        public void ResetForNewDay()
        {
            //so, after looking into it, the farm shouldn't really support flash-flooding. This makes this easier.
            NumDaysSinceLastRain++;
            if (NumDaysSinceLastRain > 7)
            {
                RainAmt = 0;
            }
        }

        /// <summary>
        /// This function adds rain to the ongoing total.
        /// </summary>
        /// <param name="rainTotal">Amt of rain being added.</param>
        public void UpdateForRainFallTotals(double rainTotal)
        {
            NumDaysSinceLastRain = 0; //it rained whenever this was called.
            RainAmt += rainTotal;
        }
    }
}
