using System;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley;

/// <summary>
/// This file handles storm penalties.
/// </summary>
namespace ClimateOfFerngill
{
    public static class StormyWeather
    {
        private static double PercentOutside { get; set; }
        public static double TickPerSpan { get; set; }
        public static double TicksOutside { get; set; }
        private static double PenaltyThres { get; set; }
        private static int PenaltyAmt { get; set; }
        private static bool Enabled { get; set; }
        private static bool Initiated { get; set; } = false;

        public static void InitiateVariables(ClimateConfig c)
        {
            if (!Initiated)
            {
                PenaltyThres = .65;
                PenaltyAmt = c.StaminaPenalty;
                Enabled = c.StormyPenalty;
                Initiated = true;
            }
        }

        public static void CheckForStaminaPenalty(Action<string, bool> log, bool debugEnabled)
        {
            //safety check
            if (TickPerSpan == 0 || !Initiated)
                return;

            PercentOutside = TickPerSpan / TickPerSpan;
            if (debugEnabled) log("Ticks Outside was: " + PercentOutside + " with " + TickPerSpan + " ticks per span and " + TicksOutside + " ticks outside.", false);

            if (PercentOutside > PenaltyThres && Game1.isLightning)
            {
               
            }

            //reset the counters
            TickPerSpan = 0;
            TicksOutside = 0;
        }

    }
}
