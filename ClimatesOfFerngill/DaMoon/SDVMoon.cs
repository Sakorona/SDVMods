using StardewValley;
using System;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using TwilightShards.Common;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using System.Linq;
using TwilightShards.Stardew.Common;
using StardewValley.Monsters;

namespace ClimatesOfFerngillRebuild
{
    public class SDVMoon
    {
        //encapsulated members
        private MersenneTwister Dice;
        private WeatherConfig ModConfig;

        //internal trackers
        private static int cycleLength = 14;

        //chances for various things
        private double CropGrowthChance;
        private double CropNoGrowthChance;
        private double GhostChance;
        private double BeachRemovalChance;
        private double BeachSpawnChance;
        public Color BloodMoonWater = Color.Red * 0.8f;

        //is blood moon
        private bool IsBloodMoon;

        //internal arrays
        internal readonly int[] beachItems = new int[] { 393, 397, 392, 394 };
        internal readonly int[] moonBeachItems = new int[] { 393, 394, 560, 586, 587, 589, 397 };

        public SDVMoon(WeatherConfig config, MersenneTwister rng)
        {
            Dice = rng;
            ModConfig = config;
            IsBloodMoon = false;

            //set chances.
            CropGrowthChance = .09;
            CropNoGrowthChance = .09;
            BeachRemovalChance = .09;
            BeachSpawnChance = .35;
            GhostChance = .02;
        }

        public void OnNewDay()
        {
            IsBloodMoon = false;
        }

        public void Reset()
        {
            IsBloodMoon = false;
        }

        public override string ToString()
        {
            return DescribeMoonPhase() + " on day " + GetDayOfCycle();
        }

        public MoonPhase CurrentPhase => (!IsBloodMoon) ? GetLunarPhase() : MoonPhase.BloodMoon;
        
        public MoonPhase GetLunarPhase()
        {
            //divide it by the cycle.
            int currentCycle = (int)Math.Floor(SDate.Now().DaysSinceStart / (double)cycleLength);
            int currentDay = GetDayOfCycle(SDate.Now());

            MoonPhase ret = SDVMoon.GetLunarPhase(currentDay);

            if (ret == MoonPhase.FullMoon)
            {
                if (Dice.NextDoublePositive() <= ModConfig.BadMoonRising)
                    return MoonPhase.BloodMoon;
            }

            return ret;
        }

        private int GetDayOfCycle()
        {
            return SDVMoon.GetDayOfCycle(SDate.Now());
        }

        private static int GetDayOfCycle(SDate Today)
        {
            return Today.DaysSinceStart % cycleLength;
        }

        /// <summary>
        /// This function returns the lunar phase for an arbitary day.
        /// </summary>
        /// <param name="Today">The day you are examining for.</param>
        /// <returns></returns>
        public static MoonPhase GetLunarPhaseForDay(SDate Today)
        {
            //divide it by the cycle.
            int currentCycle = (int)Math.Floor(Today.DaysSinceStart / (double)cycleLength);
            int currentDay = GetDayOfCycle(Today);

            return SDVMoon.GetLunarPhase(currentDay);
        }

        public void UpdateForBloodMoon()
        {
            //So the only phases that can spawn the moon are when the moon is >80%. So.. WaxingGibbeous, Full, WaningGibbeous. 
            //Odds are 1.5% and .375% respectivally.
            if (CurrentPhase == MoonPhase.FullMoon && Dice.NextDoublePositive() <= ModConfig.BadMoonRising && !Game1.isFestival() && !Game1.weddingToday && ModConfig.HazardousMoonEvents)
            {
                IsBloodMoon = true;
                Game1.currentLocation.waterColor = BloodMoonWater;
            }
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
                    return MoonPhase.WaxingGibbeous;
                case 7:
                    return MoonPhase.FullMoon;
                case 8:
                case 9:
                    return MoonPhase.WaningGibbeous;
                case 10:
                    return MoonPhase.ThirdQuarter;
                case 11:
                case 12:
                case 13:
                    return MoonPhase.WaningCrescent;
                case 14:
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

            if (Dice.NextDoublePositive() < .20)
                return;

            int cropsAffected = 0;

            //moon processing
            if (CurrentPhase == MoonPhase.FullMoon)
            {
                foreach (var TF in f.terrainFeatures)
                {
                    if (TF.Value is HoeDirt curr && curr.crop != null && Dice.NextDouble() < CropGrowthChance)
                    {
                        SDVUtilities.AdvanceArbitrarySteps(f, curr, TF.Key);                       
                    }
                }

                if (cropsAffected > 0)
                    Game1.addHUDMessage(new HUDMessage(Helper.Get("moon-text.fullmoon_eff", new { cropsAffected = cropsAffected })));
            }

            if (CurrentPhase == MoonPhase.NewMoon && ModConfig.HazardousMoonEvents)
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
                    Game1.addHUDMessage(new HUDMessage(Helper.Get("moon-text.newmoon_eff", new { cropsAffected = cropsAffected })));
            }
        }

        internal void ForceBloodMoon()
        {
            IsBloodMoon = true;
            Game1.currentLocation.waterColor = Color.PaleVioletRed;
        }

        public void TenMinuteUpdate()
        {
            if (SDVTime.CurrentIntTime == (Game1.getStartingToGetDarkTime() - 100))
            {
                UpdateForBloodMoon();
            }

            if (this.CheckForGhostSpawn() && SDVTime.CurrentIntTime > Game1.getStartingToGetDarkTime() && Game1.currentLocation is Farm && Game1.whichFarm == Farm.combat_layout)
            {
                GameLocation f = Game1.currentLocation;
                Vector2 zero = Vector2.Zero;
                switch (Game1.random.Next(4))
                {
                    case 0:
                        zero.X = (float)Game1.random.Next(f.map.Layers[0].LayerWidth);
                        break;
                    case 1:
                        zero.X = (float)(f.map.Layers[0].LayerWidth - 1);
                        zero.Y = (float)Game1.random.Next(f.map.Layers[0].LayerHeight);
                        break;
                    case 2:
                        zero.Y = (float)(f.map.Layers[0].LayerHeight - 1);
                        zero.X = (float)Game1.random.Next(f.map.Layers[0].LayerWidth);
                        break;
                    case 3:
                        zero.Y = (float)Game1.random.Next(f.map.Layers[0].LayerHeight);
                        break;
                }
                if (Utility.isOnScreen(zero * (float)Game1.tileSize, Game1.tileSize))
                    zero.X -= (float)Game1.viewport.Width;

                List<NPC> characters = f.characters;
                Ghost ghost = new Ghost(zero * (float)Game1.tileSize)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = false,
                    willDestroyObjectsUnderfoot = false,
                };
                characters.Add((NPC)ghost);

                Game1.addHUDMessage(new HUDMessage("DEBUG: Ghost spawned"));
            }
        }

        public void HandleMoonAfterWake(ITranslationHelper Helper)
        {
            if (Game1.getLocationFromName("Beach") is null)
                throw new Exception("... Please reinstall your game");

            Beach b = Game1.getLocationFromName("Beach") as Beach;
            int itemsChanged = 0;

            if (Dice.NextDoublePositive() < .20)
                return;

            //new moon processing
            if (CurrentPhase == MoonPhase.NewMoon && ModConfig.HazardousMoonEvents)
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
            if (CurrentPhase == MoonPhase.FullMoon)
            {

                    int parentSheetIndex = 0;
                    Rectangle rectangle = new Rectangle(65, 11, 25, 12);
                    for (int index = 0; index < 5; ++index)
                    {

                        //get the item ID to spawn
                        parentSheetIndex = moonBeachItems.GetRandomItem(Dice);
                        if (Dice.NextDouble() <= .0001)
                            parentSheetIndex = 392; //rare chance for a Nautlius Shell.

                        else if (Dice.NextDouble() > .0001 && Dice.NextDouble() <= .45)
                            parentSheetIndex = 589;

                        else if (Dice.NextDouble() > .45 && Dice.NextDouble() <= .62)
                            parentSheetIndex = 60;


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
            switch (this.CurrentPhase)
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
                if (CurrentPhase is MoonPhase.FullMoon && Dice.NextDouble() < GhostChance && ModConfig.HazardousMoonEvents)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
