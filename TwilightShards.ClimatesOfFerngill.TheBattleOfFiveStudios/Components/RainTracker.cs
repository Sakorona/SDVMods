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
            //so, after looking into it, the farm shouldn't really support flash-flooding. This makes this easier
            NumDaysSinceLastRain++;
            if (NumDaysSinceLastRain > 7)
            {
                RainAmt = 0;
            }
        }

        /// <summary>
        /// This function determines the effective rain
        /// </summary>
        /// <param name="dice"></param>
        /// <returns></returns>
        public double GetRainForFlooding(MersenneTwister dice)
        {
            double calcAmt = RainAmt;

            //if there's no rain stored, don't even bother doing this logic
            if (calcAmt <= 0.0)
                return 0;

            //So, essentially, every day removes 4.5 units of rain +/- a random amt, which is why we just don't purge it
            for (int i = NumDaysSinceLastRain; i > 0; i--)
            {
                calcAmt -= 3.75 + dice.RollInRange(0, 1.5);
            }

            if (calcAmt <= 0) 
                return 0;
            return calcAmt;
        }
    }
}
