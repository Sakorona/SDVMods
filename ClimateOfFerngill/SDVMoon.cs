using StardewValley;
using System;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using TwilightCore.PRNG;
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
            return SDVMoon.GetLunarPhase((int)Game1.stats.daysPlayed);
        }        

        public static MoonPhase GetLunarPhase(int day)
        {
            //divide it by the cycle.
            int currentDay = day - ((int)(Math.Floor(day / (double)cycleLength)) * cycleLength);

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

        /// <summary>
        /// Handles events that fire at sleep.
        /// </summary>
        /// <param name="f"></param>
        public void HandleMoonAtSleep(Farm f)
        {
            if (f == null)
                return;

            int cropsAffected = 0;
            string debugMessage = "";

            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon)
            {
                debugMessage = "Moon Event: Affecting crops on a full moon";
                foreach (var TF in f.terrainFeatures)
                {
                    if (TF.Value is HoeDirt curr && curr.crop != null && Dice.NextDouble() < CropGrowthChance)
                    {
                        if (curr.state == HoeDirt.watered) //make sure it's watered
                        {
                            cropsAffected++;
                            int phaseDays = 0;
                            /* Get the current day of phase
                             *  1] Is it fully grown? 
                             *  1a] If so, subtract a day from the current phase
                             *  1b] If not, take the minimum of the day of phase +1 or if the count of the phase days is past 0, then
                             *  1bi] pull the phase day count of the minimum of the current phase days count -1 or the the current phase
                             *  1bii] or 0.
                             */
                            if (curr.crop.fullyGrown) {
                                curr.crop.dayOfCurrentPhase--;
                            }
                            else
                            {
                                if (curr.crop.phaseDays.Count > 0)
                                    phaseDays = curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)];

                                curr.crop.dayOfCurrentPhase = Math.Min(curr.crop.dayOfCurrentPhase + 1, phaseDays);
                            }

                            /* 1] if the day of the current phase is greater than or equal to
                             * 1a] if the current count of the phase days is greater than 0, then
                             * 1ai] Get the count of the phase days of either this phase or the phase before.
                             * 1aii] Else, return 0
                             * 1b] and the current phase is less than the current crop phase day -1.
                             * 
                             * then, advance the phase.
                             */
                            int phaseDayCount = (curr.crop.phaseDays.Count > 0 ? 
                                curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)] : 0);

                            if (curr.crop.dayOfCurrentPhase >= phaseDayCount && 
                                curr.crop.currentPhase < curr.crop.phaseDays.Count - 1)
                            {
                                curr.crop.currentPhase = curr.crop.currentPhase + 1;
                                curr.crop.dayOfCurrentPhase = 0;
                            }
                        }
                    }
                }
            }
            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon)
            {
                debugMessage = $"Moon Events: Dewatering on a new moon, with {CropNoGrowthChance}";
                if (f != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt curr && curr.crop != null)
                        {
                            if (Dice.NextDouble() < CropNoGrowthChance)
                            {
                                cropsAffected++;
                                curr.state = HoeDirt.dry; 
                            }
                        }
                    }
                }
            }

            //output debug message
            if (Config.TooMuchInfo)
                Monitor.Log($"{debugMessage} with {cropsAffected} crops affected.");
        }

        public void HandleMoonAfterWake()
        {
            Beach b = Game1.getLocationFromName("Beach") as Beach;
            int itemsChanged = 0;
            string debugMessage = "";
            
            //new moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon)
            {
                debugMessage = $"Moon Events: It is a new moon with removal chance {BeachRemovalChance} ";
                List<KeyValuePair<Vector2, StardewValley.Object>> entries = (from o in b.objects
                                                                             where beachItems.Contains(o.Value.parentSheetIndex)
                                                                             select o).ToList();

                foreach (KeyValuePair<Vector2, StardewValley.Object> rem in entries)
                {
                        if (Dice.NextDouble() < BeachRemovalChance)
                        {
                            itemsChanged++;
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
                    debugMessage = "Moon Event: A Full moon is spawning items on the beach ";

                    //get the item ID to spawn
                    parentSheetIndex = moonBeachItems.GetRandomItem(Dice);
                    if (Dice.NextDouble() < .0001)
                        parentSheetIndex = 392; //rare chance

                    if (Dice.NextDouble() < BeachSpawnChance)
                    {
                        Vector2 v = new Vector2((float)Game1.random.Next(rectangle.X, rectangle.Right), (float)Game1.random.Next(rectangle.Y, rectangle.Bottom));
                        itemsChanged++;
                        if (b.isTileLocationTotallyClearAndPlaceable(v))
                            b.dropObject(new StardewValley.Object(parentSheetIndex, 1, false, -1, 0), v * (float)Game1.tileSize, Game1.viewport, true, null);
                    }
                }
            }

            //output what the function did
            if (debugMessage.Length > 0 && Config.TooMuchInfo)
                Monitor.Log($"{debugMessage} with {itemsChanged}", LogLevel.Trace);
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
