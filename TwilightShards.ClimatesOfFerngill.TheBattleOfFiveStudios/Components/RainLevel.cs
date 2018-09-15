using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;

namespace TwilightShards.ClimatesOfFerngill.Components
{
    /// <summary>
    /// This enum contains the descriptors for various rain levels
    /// </summary>
    public enum RainLevel
    {
        None = 0,
        Sprinkle = 1,
        Sunshower = 2,
        Light = 3,
        Normal = 4,
        Heavy = 5,
        Storm = 6,
        Hurricane = 7,
        Confused = 8
    }

    /// <summary>
    /// This class contains methods for variable rain
    /// </summary>
    public static class RainLibrary
    {
        /// <summary>
        /// This gets the rain total for a provided rain level per ten minutes.
        /// </summary>
        /// <param name="level">The rain level</param>
        /// <param name="isSunny">If it is sunny outside</param>
        /// <returns></returns>
        public static double GetRainTotalFromLevel(RainLevel level, bool isSunny=false)
        {
            return GetRainTotalFromLevel(GetDefaultRainFromLevel(level, isSunny), isSunny);
        }

        /// <summary>
        /// This gets the rain total for a provided number of raindrops on screen per ten minutes
        /// </summary>
        /// <param name="level">Number of raindrops</param>
        /// <param name="isSunny">If it is sunny outside</param>
        /// <returns></returns>
        public static double GetRainTotalFromLevel(int level, bool isSunny=false)
        {
            /* Sprinkle - 1mm per hour
             * Light < 2.5mm
             * Normal 2.5 - 7.6mm
             * Heavy 10mm - 50mm
             * Storms 35-85mm
             * Hurricane 85mm+
             */

            RainLevel baseLevel = GetLevelFromRain(level, isSunny);

            //these are straight cases, that don't require math. Return early on these.
            if (baseLevel == RainLevel.Confused)
                return 0;
            if (baseLevel == RainLevel.Sprinkle)
                return (1.0/6.0);
            if (baseLevel == RainLevel.None)
                return 0;

            double rainPerHour = .00000001 * Math.Pow(level, 4) - .00001 * Math.Pow(level, 3) +
                                 .0024 * Math.Pow(level, 2) - .026 * level + .02367;
            return (rainPerHour / 6.0);

        }

        /// <summary>
        /// Return the default amount of rain for a rainlevel
        /// </summary>
        /// <param name="targetLevel">The rain level</param>
        /// <param name="isSunny">If it is sunny outside</param>
        /// <returns></returns>
        public static int GetDefaultRainFromLevel(RainLevel targetLevel, bool isSunny)
        {
            if (isSunny)
                return 45;

            switch (targetLevel)
            {
                case RainLevel.None:
                    return 0;
                case RainLevel.Sprinkle:
                    return 15;
                case RainLevel.Light:
                    return 35;
                case RainLevel.Normal:
                    return 75;
                case RainLevel.Heavy:
                    return 135;
                case RainLevel.Storm:
                    return 285;
                case RainLevel.Hurricane:
                    return 700;
            }

            return 9999;
        }

        /// <summary>
        /// Returns a rain amount from a given RainLevel.
        /// </summary>
        /// <param name="dice">The MersenneTwister used to generate the value</param>
        /// <param name="targetLevel">The desired rain level</param>
        /// <param name="isSunny">Whether or not it is sunny outside</param>
        /// <returns>A rain level</returns>
        public static int GetRainFromLevel(MersenneTwister dice, RainLevel targetLevel, bool isSunny)
        {
            //handle sunshowers first.
            if (isSunny)
            {
                return dice.Next(7, 85);
            }

            switch (targetLevel)
            {
                case RainLevel.None:
                    return 0;
                case RainLevel.Sprinkle:
                    return dice.Next(0, 30);
                case RainLevel.Light:
                    return dice.Next(30, 55);
                case RainLevel.Normal:
                    return dice.Next(55, 95);
                case RainLevel.Heavy:
                    return dice.Next(95,165);
                case RainLevel.Storm:
                    return dice.Next(165, 445);
                case RainLevel.Hurricane:
                    return dice.Next(445, 1415);
                case RainLevel.Confused:
                    return 9999; 
            }

            return 9999; //effect: return Confused.
        }

        /// <summary>
        /// This function gets the rain level for the amount of rain drops.
        /// </summary>
        /// <param name="rainAmt">The current amount of rain</param>
        /// <param name="isSunny">Is it sunny</param>
        /// <returns>The rain level.</returns>
        public static RainLevel GetLevelFromRain(int rainAmt, bool isSunny)
        {
            //default rain amount in vanilla is 70, which is considered normal

            //sunshowers should be clamped to a certain value. I suppose this would allow torrential sunny rain, though.
            if (isSunny)
                return RainLevel.Sunshower;

            if (rainAmt <= 0)
                return RainLevel.None;
            if (rainAmt > 0 && rainAmt <= 30)
                return RainLevel.Sprinkle;
            if (rainAmt > 30 && rainAmt <= 55)
                return RainLevel.Light;
            if (rainAmt > 55 && rainAmt <= 95)
                return RainLevel.Normal;
            if (rainAmt > 95 && rainAmt <= 165)
                return RainLevel.Heavy;
            if (rainAmt > 165 && rainAmt <= 445)
                return RainLevel.Storm;
            if (rainAmt > 445)
                return RainLevel.Hurricane;

            //Error condition.
            return RainLevel.Confused;
        }
    }
}
