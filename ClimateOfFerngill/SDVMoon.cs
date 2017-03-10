using StardewValley;
using System;
using StardewModdingAPI;

namespace ClimateOfFerngill
{
    public enum MoonPhase
    {
        NewMoon,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbeous,
        FullMoon,
        WaningGibbeous,
        ThirdQuarter,
        WaningCrescent,
        ErrorPhase
    }

    public class SDVMoon
    {
        private static int cycleLength = 16;

        public static MoonPhase GetLunarPhase()
        {
            //divide it by the cycle.
            int currentDay = (int)Game1.stats.daysPlayed - ((int)(Math.Floor(Game1.stats.daysPlayed / (double)cycleLength)) * cycleLength);

            //Day 0 and 16 are the New Moon, so Day 8 must be the Full Moon. Day 4 is 1Q, Day 12 is 3Q. Coorespondingly..
            switch (currentDay)
            {
                case 0:
                    return MoonPhase.NewMoon;
                case 1:
                case 2:
                case 3:
                    return MoonPhase.WaxingCrescent;
                case 4:
                    return MoonPhase.FirstQuarter;
                case 5:
                case 6:
                case 7:
                    return MoonPhase.WaxingGibbeous;
                case 8:
                    return MoonPhase.FullMoon;
                case 9:
                case 10:
                case 11:
                    return MoonPhase.WaningGibbeous;
                case 12:
                    return MoonPhase.ThirdQuarter;
                case 13:
                case 14:
                case 15:
                    return MoonPhase.WaningCrescent;
                case 16:
                    return MoonPhase.NewMoon;
                default:
                    return MoonPhase.ErrorPhase;             
            }

        }

        public void HandleMoonBeforeSleep() { 
            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon)
            {
                Farm f = Game1.getFarm();
        HoeDirt curr;

                if (f != null){
                    foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt)
                        {
                            curr = (HoeDirt) TF.Value;
                            if (curr.crop != null)
                            {
                                //20% chance of increased growth.
                                if (Dice.NextDouble() < .1)
                                {
                                    if (Config.TooMuchInfo) Console.WriteLine("Crop is being boosted by full moon");
                                    if (curr.state == 1) //make sure it's watered
                                    {
                                        curr.crop.dayOfCurrentPhase = curr.crop.fullyGrown? curr.crop.dayOfCurrentPhase - 1 : Math.Min(curr.crop.dayOfCurrentPhase + 1, curr.crop.phaseDays.Count > 0 ? curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)] : 0);
                                        if (curr.crop.dayOfCurrentPhase >= (curr.crop.phaseDays.Count > 0 ? curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)] : 0) && curr.crop.currentPhase<curr.crop.phaseDays.Count - 1)
                                        {
                                            curr.crop.currentPhase = curr.crop.currentPhase + 1;
                                            curr.crop.dayOfCurrentPhase = 0;
                                        }
}
                                }
                            }
                        }
                    }
                }
            }


            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon && Config.MoonEffects)
            {
                Farm f = Game1.getFarm();
HoeDirt curr;

                if (f != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt)
                        {
                            curr = (HoeDirt) TF.Value;
                            if (curr.crop != null)
                            {
                                if (Dice.NextDouble() < .09)
                                {
                                    curr.state = 0; //dewater!! BWAHAHAAHAA.
                                }
                            }
                        }
                    }
                }
            }
    }
    }
}
