using StardewValley;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using System.Linq;
using StardewValley.Monsters;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;
using System;

namespace TwilightShards.LunarDisturbances
{
    public class SDVMoon
    {
        //encapsulated members
        private readonly Random Dice;
        private readonly MoonConfig ModConfig;
        private readonly ITranslationHelper Translations;

        //internal trackers
        private const int cycleLengthA = 14;
        private const int cycleLengthB = 25;

        //chances for various things
        public Color BloodMoonWater = Color.Red * 0.8f;
        private readonly IMonitor Monitor;
        internal float EclipseMods;
        internal LunarInfo MoonTracker;

        internal bool IsEclipse;

        private bool IsSuperMoon; //a relative of superman
        private bool IsBlueMoon;
        private bool IsBloodMoon;
        private bool IsHarvestMoon;

        //internal arrays
        internal readonly string[] beachItems = new string[] { "(O)393", "(O)397", "(O)392", "(O)394" };
        internal readonly string[] moonBeachItems = new string[] { "(O)393", "(O)394", "(O)560", "(O)586", "(O)587", "(O)589", "(O)397" };

        public SDVMoon(MoonConfig config, Random rng, ITranslationHelper Trans, IMonitor Logger)
        {
            Dice = rng;
            ModConfig = config;
            Monitor = Logger;
            IsBloodMoon = false;
            IsSuperMoon = false;
            IsBlueMoon = false;
            IsHarvestMoon = false;
            Translations = Trans;
        }

        public void OnNewDay()
        {
            IsBloodMoon = false;
            IsSuperMoon = false;
            IsHarvestMoon = false;
            IsBlueMoon = false;

            if (CurrentPhase() == MoonPhase.FullMoon || CurrentPhase() == MoonPhase.BloodMoon)
            {
                if (Game1.currentSeason == "fall")
                {
                    if (Game1.dayOfMonth - GetMoonCycleLength < 1)
                    {
                        IsHarvestMoon = true;
                        Game1.addHUDMessage(new TCHUDMessage(Translations.Get("moon-text.harvestmoon"), CurrentPhase()));
                    }
                }
            }

            if (Game1.currentSeason == "fall" && Game1.dayOfMonth == 27)
            {
                Game1.addHUDMessage(new TCHUDMessage(Translations.Get("moon-text.spiritmoon"), MoonPhase.SpiritsMoon));
            }

            if (Dice.NextDouble() < ModConfig.SuperMoonChances)
            {
                IsSuperMoon = true;
                Game1.addHUDMessage(new TCHUDMessage(Translations.Get("moon-text.supermoon"), CurrentPhase()));
            }

            if (MoonTracker is null)
            {
                Monitor.Log("Error: Moon Tracker is null", LogLevel.Info);
            }
            else
            {
                if (MoonTracker.FullMoonThisSeason && CurrentPhase() == MoonPhase.FullMoon && CurrentPhase() != MoonPhase.BloodMoon)
                {
                    IsBlueMoon = true;
                    Game1.addHUDMessage(new TCHUDMessage(Translations.Get("moon-text.bluemoon"), CurrentPhase()));
                    Game1.player.team.sharedDailyLuck.Value += .025;
                }
                else
                {
                    MoonTracker.FullMoonThisSeason = true;
                }
            }

            if (MoonTracker is not null)
                MoonTracker.IsEclipseTomorrow = SetEclipseTomorrow();
            else
            {
                Monitor.Log("MoonTracker is null! Eclipse tomorrow not set", LogLevel.Error);
            }
        }

        public void TurnEclipseOn()
        {
            Monitor.Log("Turning the eclipse on!");
            IsEclipse = true;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void TurnEclipseOn(string arg1, string[] arg2)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            TurnEclipseOn();
        }

        public float GetBrightnessQuotient()
        {
            return CurrentPhase() switch
            {
                MoonPhase.BloodMoon or MoonPhase.BlueMoon => 2f,
                MoonPhase.HarvestMoon => 1.55f,
                MoonPhase.SpiritsMoon => 1.15f,
                MoonPhase.FullMoon => 1f,
                MoonPhase.ThirdQuarter or MoonPhase.FirstQuarter => .5f,
                MoonPhase.WaxingCrescent or MoonPhase.WaningCrescent => .15f,
                MoonPhase.WaningGibbeous or MoonPhase.WaxingGibbeous => .65f,
                MoonPhase.NewMoon => 0.02f,
                _ => 0.0f,
            };
        }

        public void Reset()
        {
            MoonTracker = null;
            IsBloodMoon = false;
            IsBlueMoon = false;
            IsHarvestMoon = false;
            IsSuperMoon = false;
            IsEclipse = false;
        }

        public override string ToString()
        {
            return DescribeMoonPhase() + " on day " + GetDayOfCycle();
        }

        public MoonPhase CurrentPhase()
        {
            MoonPhase def = GetLunarPhase();

            if (IsBloodMoon)
                return MoonPhase.BloodMoon;
            if (IsBlueMoon)
                return MoonPhase.BlueMoon;
            if (IsHarvestMoon)
                return MoonPhase.HarvestMoon;
            if (Game1.currentSeason == "fall" && Game1.dayOfMonth == 27)
                return MoonPhase.SpiritsMoon;

            return def;
        }

        public MoonPhase GetLunarPhase()
        {
            //divide it by the cycle.
            int currentDay = GetDayOfCycle(SDate.Now());

            MoonPhase ret = SDVMoon.GetLunarPhase(currentDay, GetMoonCycleLength);

            if (IsBloodMoon) //restructuring.
                return MoonPhase.BloodMoon;
            if (IsBlueMoon)
                return MoonPhase.BlueMoon;
            if (IsHarvestMoon)
                return MoonPhase.HarvestMoon;

            return ret;
        }

        private int GetDayOfCycle()
        {
            return GetDayOfCycle(SDate.Now());
        }

        public int GetMoonCycleLength => (ModConfig.UseMoreMonthlyCycle ? cycleLengthB : cycleLengthA);

        private int GetDayOfCycle(SDate Today)
        {
            return Today.DaysSinceStart % GetMoonCycleLength;
        }

        private bool SetEclipseTomorrow()
        {
            bool validEclipseDate = (SDate.Now().DaysSinceStart > 2 && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.season));
            bool validEclipsePhase = (this.CurrentPhase() == MoonPhase.NewMoon);
                    
            if (validEclipsePhase && validEclipseDate)
            {
                if (Dice.NextDouble() < (ModConfig.EclipseChance + EclipseMods))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This function returns the lunar phase for an arbitrary day.
        /// </summary>
        /// <param name="Today">The day you are examining for.</param>
        /// <returns></returns>
        public MoonPhase GetLunarPhaseForDay(SDate Today)
        {
            int currentDay = GetDayOfCycle(Today);
            return GetLunarPhase(currentDay, GetMoonCycleLength);
        }

        public void ForceBloodMoon()
        {
            IsBloodMoon = true;
            DoBloodMoonAlert();
            Game1.currentLocation.waterColor.Value = BloodMoonWater;
        }

        public void UpdateForBloodMoon()
        {
            if (CurrentPhase() == MoonPhase.FullMoon && Dice.NextDouble() <= ModConfig.BadMoonRising && !Game1.isFestival() && !Game1.weddingToday && ModConfig.HazardousMoonEvents && !IsBlueMoon)
            {
                IsBloodMoon = true;
                DoBloodMoonAlert();
                Game1.currentLocation.waterColor.Value = BloodMoonWater;
            }
        }

        internal void DoBloodMoonAlert()
        {
            Game1.addHUDMessage(new TCHUDMessage(LunarDisturbances.Translation.Get("moon-text.hud_message_bloodMoon"),CurrentPhase()));
        }

        private static MoonPhase GetLunarPhase(int day, int cycleLength)
        {
            if (cycleLength == 14)
            {
                return day switch
                {
                    0 => MoonPhase.NewMoon,
                    1 or 2 or 3 => MoonPhase.WaxingCrescent,
                    4 => MoonPhase.FirstQuarter,
                    5 or 6 => MoonPhase.WaxingGibbeous,
                    7 => MoonPhase.FullMoon,
                    8 or 9 => MoonPhase.WaningGibbeous,
                    10 => MoonPhase.ThirdQuarter,
                    11 or 12 or 13 => MoonPhase.WaningCrescent,
                    14 => MoonPhase.NewMoon,
                    _ => MoonPhase.ErrorPhase,
                };
            }
            else
            {
                return day switch
                {
                    0 => MoonPhase.NewMoon,
                    int n when (n >= 1 && n <= 5) => MoonPhase.WaxingCrescent,
                    int n when (n == 6 || n == 7) => MoonPhase.FirstQuarter,
                    int n when (n >= 8 && n <= 12) => MoonPhase.WaxingGibbeous,
                    13 => MoonPhase.FullMoon,
                    int n when (n >= 14 && n <= 18) => MoonPhase.WaningGibbeous,
                    19 => MoonPhase.ThirdQuarter,
                    int n when (n >= 20 && n <= 24) => MoonPhase.WaningCrescent,
                    25 => MoonPhase.NewMoon,
                    _ => MoonPhase.ErrorPhase,
                };
            }            
        }

        /// <summary>
        /// Handles events that fire at sleep.
        /// </summary>
        /// <param name="f"></param>
        public int HandleMoonAtSleep(Farm f, IMonitor Logger)
        {
            if (f == null)
                return 0;

            int cropsAffected = 0;

            if (CurrentPhase() == MoonPhase.FullMoon || CurrentPhase() == MoonPhase.HarvestMoon || CurrentPhase() == MoonPhase.BlueMoon)
            {
                foreach (var TF in f.terrainFeatures.Pairs)
                {
                    double diceRoll = ModConfig.CropGrowthChance;

                    if (CurrentPhase() == MoonPhase.HarvestMoon)
                        diceRoll *= 3.5;
                    if (IsSuperMoon)
                        diceRoll *= 2;

                    if (TF.Value is HoeDirt curr && curr.crop != null && Dice.NextDouble() < diceRoll)
                    {
                        if (ModConfig.Verbose)
                            Logger.Log($"Advancing crop at {TF.Key}", LogLevel.Trace);
                        SDVUtilities.AdvanceArbitrarySteps(f, curr, TF.Key);   

                        if (Dice.NextDouble() < ModConfig.HarvestMoonDoubleGrowChance)
                        {
                            if (ModConfig.Verbose)
                                Logger.Log($"Advancing crop at {TF.Key} for harvest moon", LogLevel.Trace);
                            SDVUtilities.AdvanceArbitrarySteps(f, curr, TF.Key);
                        }

                    }
                }
                return cropsAffected;
            }

            if (CurrentPhase() == MoonPhase.NewMoon && ModConfig.HazardousMoonEvents)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures.Pairs)
                {
                    double diceRoll = ModConfig.CropHaltChance;

                    if (IsSuperMoon)
                        diceRoll *= 2;

                    if (TF.Value is HoeDirt current && current.crop != null)
                    {
                        if (Dice.NextDouble() < diceRoll)
                        {
                            SDVUtilities.DeAdvanceCrop(f, current, TF.Key, 1, Logger);
                            current.state.Value = 0;
                            cropsAffected++;
                            if (ModConfig.Verbose)
                                Logger.Log($"Deadvancing crop at {TF.Key}", LogLevel.Trace);
                        }
                    }
                }

                return cropsAffected;
            }        
            return cropsAffected;
        }

        internal void TurnBloodMoonOff()
        {
            IsBloodMoon = false;
        }

        public void TenMinuteUpdate()
        {
            if (CheckForGhostSpawn() && SDVTime.CurrentIntTime > Game1.getStartingToGetDarkTime(Game1.currentLocation) && Game1.currentLocation is Farm && Game1.whichFarm == Farm.combat_layout)
            {
                GameLocation f = Game1.currentLocation;
                Vector2 zero = Vector2.Zero;
                switch (Game1.random.Next(4))
                {
                    case 0:
                        zero.X = Game1.random.Next(f.map.Layers[0].LayerWidth);
                        break;
                    case 1:
                        zero.X = (f.map.Layers[0].LayerWidth - 1);
                        zero.Y = Game1.random.Next(f.map.Layers[0].LayerHeight);
                        break;
                    case 2:
                        zero.Y = (f.map.Layers[0].LayerHeight - 1);
                        zero.X = Game1.random.Next(f.map.Layers[0].LayerWidth);
                        break;
                    case 3:
                        zero.Y = Game1.random.Next(f.map.Layers[0].LayerHeight);
                        break;
                }
                if (Utility.isOnScreen(zero * Game1.tileSize, Game1.tileSize))
                    zero.X -= Game1.viewport.Width;

                List<NPC> characters = f.characters.ToList();
                Ghost ghost = new(zero * Game1.tileSize)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = false,
                    willDestroyObjectsUnderfoot = false,
                };
                characters.Add(ghost);
            }
        }

        public string GetMenuString()
        {
            return Translations.Get("moon-desc.desc_moonphase", new { moonPhase = SDVMoon.DescribeMoonPhase(CurrentPhase(), Translations) });
        }

        public float GetTrackPosition()
        {
            int moonDuration = SDVTime.MinutesBetweenTwoIntTimes(GetMoonSetTime(), GetMoonRiseTime());
            int timeSinceRise = SDVTime.MinutesBetweenTwoIntTimes(Game1.timeOfDay, GetMoonRiseTime());

            if (Game1.timeOfDay < GetMoonRiseTime() || Game1.timeOfDay > GetMoonSetTime())
                return 0f;

            return timeSinceRise / moonDuration;            
        }

        public int GetMoonZenith()
        {
            int moonDuration = SDVTime.MinutesBetweenTwoIntTimes(GetMoonSetTime(), GetMoonRiseTime());
            SDVTime mr = new(GetMoonRiseTime());
            mr.AddTime(moonDuration / 2);
            return mr.ReturnIntTime();
        }

        public void DayEnding()
        {
            IsEclipse = false;
        }

        public void HandleMoonAfterWake()
        {
            Beach b = Game1.getLocationFromName("Beach") as Beach;
            int itemsChanged = 0;

            if (Dice.NextDouble() < .20)
                return;

            //new moon processing
            if (CurrentPhase() == MoonPhase.NewMoon && ModConfig.HazardousMoonEvents && b is not null)
            {
                List<KeyValuePair<Vector2, StardewValley.Object>> entries = (from o in b.objects.Pairs
                    where Array.Exists(beachItems, element => element == o.Value.QualifiedItemId)
                    select o).ToList();

                foreach (KeyValuePair<Vector2, StardewValley.Object> rem in entries)
                {
                    double diceRoll = ModConfig.BeachRemovalChance;
                    if (IsSuperMoon)
                        diceRoll *= 2;

                    if (Dice.NextDouble() < diceRoll)
                    {
                        itemsChanged++;
                        b.objects.Remove(rem.Key);
                    }
                }

                if (itemsChanged > 0)
                    Game1.addHUDMessage(new HUDMessage(Translations.Get("moon-text.hud_message_new")));
            }

            //full moon processing
            if (CurrentPhase() == MoonPhase.FullMoon)
            {
                Rectangle rectangle = new(65, 11, 25, 12);
                for (int index = 0; index < 8; ++index)
                {
                    //get the item ID to spawn
                    string parentSheetIndex = moonBeachItems.GetRandomItem(Dice);
                    if (Dice.NextDouble() <= .0001)
                        parentSheetIndex = "(O)392"; //rare chance for a Nautlius Shell.

                    double emeraldChance = .2001;
                    if (IsSuperMoon)
                        emeraldChance += .10;

                    else if (Dice.NextDouble() > .0001 && Dice.NextDouble() <= emeraldChance)
                        parentSheetIndex = "(O)60";

                    if (Dice.NextDouble() < ModConfig.BeachSpawnChance)
                    {
                        Vector2 v = new(Game1.random.Next(rectangle.X, rectangle.Right), Game1.random.Next(rectangle.Y, rectangle.Bottom));
                        itemsChanged++;
                        if (b.isTilePlaceable(v))
                            b.dropObject(new StardewValley.Object(parentSheetIndex, 1, false, -1, 0), v * Game1.tileSize, Game1.viewport, true, null);
                    }
                }

                if (IsSuperMoon)
                {
                    for (int j = 0; j < 20; ++j)
                    {
                        double driftWoodChance = .25;
                        string parentSheetIndex = "(O)388";
                        if (Dice.NextDouble() < driftWoodChance)
                        {
                            Vector2 v = new(Game1.random.Next(rectangle.X, rectangle.Right), Game1.random.Next(rectangle.Y, rectangle.Bottom));
                            itemsChanged++;
                            if (b.isTilePlaceable(v))
                                b.dropObject(new StardewValley.Object(parentSheetIndex, 1, false, -1, 0), v * Game1.tileSize, Game1.viewport, true, null);
                        }
                    }
                }

                if (itemsChanged > 0)
                    Game1.addHUDMessage(new HUDMessage(Translations.Get("moon-text.hud_message_full")));
            }

            //check for eclipse tomorrow.
        }

        public string SimpleMoonPhase()
        {
            return DescribeMoonPhase(this.CurrentPhase());
        }

        public static string DescribeMoonPhase(MoonPhase mp, ITranslationHelper Helper)
        {
            return mp switch
            {
                MoonPhase.ErrorPhase => (string)Helper.Get("moon-text.error"),
                MoonPhase.FirstQuarter => (string)Helper.Get("moon-text.phase-firstqrt"),
                MoonPhase.FullMoon => (string)Helper.Get("moon-text.phase-full"),
                MoonPhase.NewMoon => (string)Helper.Get("moon-text.phase-new"),
                MoonPhase.ThirdQuarter => (string)Helper.Get("moon-text.phase-thirdqrt"),
                MoonPhase.WaningCrescent => (string)Helper.Get("moon-text.phase-waningcres"),
                MoonPhase.WaningGibbeous => (string)Helper.Get("moon-text.phase-waninggibb"),
                MoonPhase.WaxingCrescent => (string)Helper.Get("moon-text.phase-waxingcres"),
                MoonPhase.WaxingGibbeous => (string)Helper.Get("moon-text.phase-waxinggibb"),
                MoonPhase.BloodMoon => (string)Helper.Get("moon-text.phase-blood"),
                MoonPhase.BlueMoon => (string)Helper.Get("moon-text.blue-moon"),
                MoonPhase.HarvestMoon => (string)Helper.Get("moon-text.harvest-moon"),
                MoonPhase.SpiritsMoon => (string)Helper.Get("moon-text.spirits-moon"),
                _ => (string)Helper.Get("moon-text.error"),
            };
        }

        public static string DescribeMoonPhase(MoonPhase mp)
        {
            return mp switch
            {
                MoonPhase.ErrorPhase => "ErrorPhase",
                MoonPhase.FirstQuarter => "FirstQuarter",
                MoonPhase.FullMoon => "FullMoon",
                MoonPhase.NewMoon => "NewMoon",
                MoonPhase.ThirdQuarter => "ThirdQuarter",
                MoonPhase.WaningCrescent => "WaningCrescent",
                MoonPhase.WaningGibbeous => "WaningGibbous",
                MoonPhase.WaxingCrescent => "WaxingCrescent",
                MoonPhase.WaxingGibbeous => "WaxingGibbous",
                MoonPhase.BloodMoon => "BloodMoon",
                MoonPhase.BlueMoon => "BlueMoon",
                MoonPhase.HarvestMoon => "HarvestMoon",
                MoonPhase.SpiritsMoon => "SpiritsMoon",
                _ => "ErrorMoon",
            };
        }

        public string DescribeMoonPhase()
        {
            return DescribeMoonPhase(this.CurrentPhase(), LunarDisturbances.Translation);
        }
        public int GetMoonRiseDisplayTime()
        {
            int MoonRise = GetMoonRiseTime();
            if (MoonRise >= 2400)
                return (MoonRise - 2400);
            else
                return MoonRise;
        }
        public int GetMoonRiseTime()
        {
            return CurrentPhase() switch
            {
                MoonPhase.BloodMoon or MoonPhase.HarvestMoon => 0600,
                MoonPhase.FullMoon or MoonPhase.BlueMoon => 2040,
                MoonPhase.WaningGibbeous => 2200,
                MoonPhase.ThirdQuarter => 2310,
                MoonPhase.WaningCrescent => 2430,
                MoonPhase.NewMoon or MoonPhase.SpiritsMoon => 0600,
                MoonPhase.WaxingCrescent => 1130,
                MoonPhase.FirstQuarter => 1500,
                MoonPhase.WaxingGibbeous => 1340,
                _ => 2700,
            };
        }

        public bool IsMoonUp(int time)
        {
            if (time >= GetMoonRiseTime() && time <= GetMoonSetTime())
                return true;

            return false;
        }

        public int GetMoonSetTime()
        {
            //Blood Moons don't set. More's the pity, I guess..
            return this.CurrentPhase() switch
            {
                MoonPhase.BloodMoon or MoonPhase.HarvestMoon or MoonPhase.SpiritsMoon => 2700,
                MoonPhase.FullMoon or MoonPhase.BlueMoon => 2830,
                MoonPhase.WaningGibbeous => 1020,
                MoonPhase.ThirdQuarter => 1420,
                MoonPhase.WaningCrescent => 1800,
                MoonPhase.NewMoon => 2020,
                MoonPhase.WaxingCrescent => 2130,
                MoonPhase.FirstQuarter => 2250,
                MoonPhase.WaxingGibbeous => 2320,
                _ => 0700,
            };
        }    

        public bool CheckForGhostSpawn()
        {
			if (CurrentPhase() is MoonPhase.FullMoon && Dice.NextDouble() < ModConfig.GhostSpawnChance && ModConfig.HazardousMoonEvents)
			{
				return true;
			}
			
            return false;
        }

        public void MoonTrackerUpdate()
        {
            if (MoonTracker.FullMoonThisSeason && this.CurrentPhase() == MoonPhase.FullMoon && !IsBloodMoon)
            {
                IsBlueMoon = true;
                Game1.addHUDMessage(new TCHUDMessage(Translations.Get("moon-text.bluemoon"), CurrentPhase()));
                Game1.player.team.sharedDailyLuck.Value += .025;
            }
            else
            {
                MoonTracker.FullMoonThisSeason = true;
            }
        }
    }
}
