using StardewValley;
using System;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using NPack;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using System.Linq;

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
        //encapsulated members
        private MersenneTwister Dice;
        private ClimateConfig Config;
        private IMonitor Monitor;

        //internal trackers
        internal MoonPhase CurrPhase;
        private static int cycleLength = 16;

        //chances for various things
        private double CropGrowthChance;
        private double CropNoGrowthChance;
        private double GhostChance;
        private double BeachRemovalChance;
        private double BeachSpawnChance;

        //internal arrays
        internal readonly int[] beachItems = new int[] { 393, 397, 392, 394 };
        internal readonly int[] moonBeachItems = new int[] { 393, 394, 560, 586, 587, 589, 397 };

        public SDVMoon(IMonitor mon, ClimateConfig config, MersenneTwister rng)
        {
            Monitor = mon;
            Dice = rng;
            Config = config;

            //set chances.
            CropGrowthChance = .09;
            CropNoGrowthChance = .09;
            BeachRemovalChance = .09;
            BeachSpawnChance = .35;
            GhostChance = .02;
        }

        public void UpdateForNewDay()
        {
            CurrPhase = SDVMoon.GetLunarPhase();
        }

        public void Reset()
        {
            CurrPhase = MoonPhase.ErrorPhase;
        }

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

        public void HandleMoonBeforeSleep(Farm f) { 
            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon)
            {
                if(f != null) {
                    foreach (var TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt curr && curr.crop != null)
                        {
                            //20% chance of increased growth.
                            if (Dice.NextDouble() < CropGrowthChance)
                            {
                                if (Config.TooMuchInfo)
                                    Monitor.Log("Crop is being boosted by full moon");
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


            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon)
            {
                if (f != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt curr && curr.crop != null)
                        {
                            if (Dice.NextDouble() < CropNoGrowthChance)
                            {
                                curr.state = 0; //dewater!! BWAHAHAAHAA.
                            }
                        }
                    }
                }
            }
        }

        public void HandleMoonAfterWake(Beach b)
        {
            //new moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon || Config.ForceMoonRemoval)
            {
                if (Config.TooMuchInfo)
                    Monitor.Log($"It is a new moon with removal chance {BeachRemovalChance}");

                List<KeyValuePair<Vector2, StardewValley.Object>> entries = (from o in b.objects
                                                                             where beachItems.Contains(o.Value.parentSheetIndex)
                                                                             select o).ToList();

                foreach (KeyValuePair<Vector2, StardewValley.Object> rem in entries)
                {
                        if (Dice.NextDouble() < BeachRemovalChance)
                        {
                            b.objects.Remove(rem.Key);
                        }
                }
            }

            //full moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon)
            {
                int parentSheetIndex = 0;
                Rectangle rectangle = new Rectangle(65, 11, 25, 12);
                for (int index = 0; index < 5; ++index)
                {
                    //get the item ID to spawn
                    parentSheetIndex = moonBeachItems.GetRandomItem(Dice);
                    if (Dice.NextDouble() < .0001)
                        parentSheetIndex = 392; //rare chance

                    if (Dice.NextDouble() < BeachSpawnChance)
                    {
                        Vector2 v = new Vector2((float)Game1.random.Next(rectangle.X, rectangle.Right), (float)Game1.random.Next(rectangle.Y, rectangle.Bottom));
                        if (b.isTileLocationTotallyClearAndPlaceable(v))
                            b.dropObject(new StardewValley.Object(parentSheetIndex, 1, false, -1, 0), v * (float)Game1.tileSize, Game1.viewport, true, null);
                    }
                }
            }
        }

        public static string DescribeMoonPhase(MoonPhase mp)
        {
            switch (mp)
            {
                case MoonPhase.ErrorPhase:
                    return "error";
                case MoonPhase.FirstQuarter:
                    return "First Quarter";
                case MoonPhase.FullMoon:
                    return "Full Moon";
                case MoonPhase.NewMoon:
                    return "New Moon";
                case MoonPhase.ThirdQuarter:
                    return "Third Quarter";
                case MoonPhase.WaningCrescent:
                    return "Waning Crescent";
                case MoonPhase.WaningGibbeous:
                    return "Waning Gibbeous";
                case MoonPhase.WaxingCrescent:
                    return "Waxing Crescent";
                case MoonPhase.WaxingGibbeous:
                    return "Waxing Gibbeous";
                default:
                    return "ERRROR";
            }
        }

        public bool CheckForGhostSpawn()
        {
            if (Game1.timeOfDay > Game1.getTrulyDarkTime() && Game1.currentLocation.isOutdoors && Game1.currentLocation is Farm)
            {
                if (CurrPhase is MoonPhase.FullMoon && Dice.NextDouble() < GhostChance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
