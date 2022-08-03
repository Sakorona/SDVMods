using System;
using StardewModdingAPI;
using StardewValley;

namespace FerngillDynamicRainAndWind
{
   public partial class RainAndWind
    {
        internal void SetRainAmt(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            int rainAmt = Convert.ToInt32(arg2[0]);

            Array.Resize(ref Game1.rainDrops, rainAmt);
            CurrentRainAmt = rainAmt;
            Console.WriteLine($"Testing: resize of array is now {Game1.rainDrops.Length}");
        }

        internal void ShowRainAmt(string arg1, string[] arg2)
        { 
            Monitor.Log($"Current rain amount is {CurrentRainAmt}", LogLevel.Info);
        }

        /*
        internal static void SetRainDef(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            float x = Convert.ToInt32(arg2[0]);
            float y = Convert.ToInt32(arg2[1]);

            RainX.Value = x;
            RainY.Value = y;
        }

        internal static void SetWindChance(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            WindChance = (float)Convert.ToDouble(arg2[0]);
        }

        internal static void SetWindThreshold(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            WindThreshold = (float)Convert.ToDouble(arg2[0]);
        }

        internal static void SetWindRange(string arg1, string[] arg2)
        {
            if (!Context.IsMainPlayer) return;

            if (arg2.Length < 1)
                return;

            float x = (float)Convert.ToDouble(arg2[0]);
            float y = (float)Convert.ToDouble(arg2[1]);

            WindMin = x;
            WindCap = y;
        }

        private void SetWindToGo(string arg1, string[] arg2)
        {
            Game1.isDebrisWeather = true;
        }*/
    }
}
