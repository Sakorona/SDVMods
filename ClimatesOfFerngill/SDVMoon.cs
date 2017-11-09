using StardewValley;
using System;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using TwilightShards.Common;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using System.Linq;
using StardewModdingAPI.Utilities;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
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

        public SDVMoon(MersenneTwister rng)
        {
            Dice = rng;

            //set chances.
            CropGrowthChance = .09;
            CropNoGrowthChance = .09;
            BeachRemovalChance = .09;
            BeachSpawnChance = .35;
            GhostChance = .02;
            CurrPhase = SDVMoon.GetLunarPhase();
        }

        public void UpdateForNewDay()
        {
            CurrPhase = SDVMoon.GetLunarPhase();
        }

        public void Reset()
        {
            CurrPhase = MoonPhase.ErrorPhase;
        }

        public override string ToString()
        {
            return DescribeMoonPhase() + " on day " + GetDayOfCycle();
        }

        public static MoonPhase GetLunarPhase()
        {
            return SDVMoon.GetLunarPhase(SDVUtilities.GetDayFromDate(SDate.Now()));
        }        

        public int GetDayOfCycle()
        {
            return SDVMoon.GetDayOfCycle(SDate.Now());
        }

        public static int GetDayOfCycle(SDate Today)
        {
            return SDVUtilities.GetDayFromDate(Today) % cycleLength;
        }

        public static MoonPhase GetLunarPhase(SDate Today)
        {
            //divide it by the cycle.
            int currentCycle = (int)Math.Floor(SDVUtilities.GetDayFromDate(Today) / (double)cycleLength);
            int currentDay = GetDayOfCycle(Today);
            Console.Write($"Day is {SDVUtilities.GetDayFromDate(Today)} with current cycle is {currentCycle} and currentDay is {currentDay}");

            return SDVMoon.GetLunarPhase(currentDay);
        }

        private static MoonPhase GetLunarPhase(int day)
        {
            //Day 0 and 16 are the New Moon, so Day 8 must be the Full Moon. Day 4 is 1Q, Day 12 is 3Q. Coorespondingly..
            switch (day)
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
        public void HandleMoonAtSleep(Farm f, ITranslationHelper Helper)
        {
            if (f == null)
                return;

            int cropsAffected = 0;

            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon)
            {
                foreach (var TF in f.terrainFeatures)
                {
                    if (TF.Value is HoeDirt curr && curr.crop != null && Dice.NextDouble() < CropGrowthChance)
                    {
                        if (curr.state == HoeDirt.watered) //make sure it's watered
                        {
                            cropsAffected++;
                            int phaseDays = 0;
                            if (curr.crop.fullyGrown) {
                                curr.crop.dayOfCurrentPhase--;
                            }
                            else
                            {
                                if (curr.crop.phaseDays.Count > 0)
                                    phaseDays = curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)];

                                curr.crop.dayOfCurrentPhase = Math.Min(curr.crop.dayOfCurrentPhase + 1, phaseDays);
                            }

                            
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

                if (cropsAffected > 0)
                    Game1.addHUDMessage(new HUDMessage(Helper.Get("moon-text.fullmoon_eff")));
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
                                cropsAffected++;
                                curr.state = HoeDirt.dry; 
                            }
                        }
                    }
                }

                if (cropsAffected > 0)
                    Game1.addHUDMessage(new HUDMessage(Helper.Get("moon-text.newmoon_eff")));
            }
        }

        public void HandleMoonAfterWake(ITranslationHelper Helper)
        {
            Beach b = Game1.getLocationFromName("Beach") as Beach;
            int itemsChanged = 0;
            
            //new moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon)
            {
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

                if (itemsChanged > 0)
                    Game1.addHUDMessage(new HUDMessage(Helper.Get("moon-text.hud_message_new")));
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
                        itemsChanged++;
                        if (b.isTileLocationTotallyClearAndPlaceable(v))
                            b.dropObject(new StardewValley.Object(parentSheetIndex, 1, false, -1, 0), v * (float)Game1.tileSize, Game1.viewport, true, null);
                    }
                }

                if (itemsChanged > 0)
                    Game1.addHUDMessage(new HUDMessage(Helper.Get("moon-text.hud_message_full")));
            }
        }

        public static string DescribeMoonPhase(MoonPhase mp, ITranslationHelper Helper)
        {
            switch (mp)
            {
                case MoonPhase.ErrorPhase:
                    return Helper.Get("moon-text.error");
                case MoonPhase.FirstQuarter:
                    return Helper.Get("moon-text.phase-firstqrt");
                case MoonPhase.FullMoon:
                    return Helper.Get("moon-text.phase-full");
                case MoonPhase.NewMoon:
                    return Helper.Get("moon-text.phase-new");
                case MoonPhase.ThirdQuarter:
                    return Helper.Get("moon-text.phase-thirdqrt");
                case MoonPhase.WaningCrescent:
                    return Helper.Get("moon-text.phase-waningcres");
                case MoonPhase.WaningGibbeous:
                    return Helper.Get("moon-text.phase-waninggibb");
                case MoonPhase.WaxingCrescent:
                    return Helper.Get("moon-text.phase-waxingcres");
                case MoonPhase.WaxingGibbeous:
                    return Helper.Get("moon-text.phase-waxinggibb");
                default:
                    return Helper.Get("moon-text.error");
            }
        }

        private string DescribeMoonPhase()
        {
            switch (this.CurrPhase)
            {
                case MoonPhase.ErrorPhase:
                    return "Phase Error";
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
                    return "Text Error";
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
