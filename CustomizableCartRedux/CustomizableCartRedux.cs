using System;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework;
using HarmonyLib;
using System.Reflection;
using CustomizableTravelingCart.Patches;

namespace CustomizableTravelingCart
{
    public class CustomizableCartRedux : Mod
    {
        internal static Mod instance;
        internal static IMonitor Logger;
        internal static CartConfig OurConfig;
        public Random Dice;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Dice = new Xoshiro.PRNG64.XoShiRo256starstar();
            OurConfig = helper.ReadConfig<CartConfig>();
            Logger = Monitor;

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.GameLaunched += OnGameLanuched;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            MethodInfo CheckAction = AccessTools.Method(typeof(Forest), "checkAction");
            HarmonyMethod CATranspiler = new(AccessTools.Method(typeof(ForestPatches), "CheckActionTranspiler"));
            Monitor.Log($"Patching {CheckAction} with Transpiler: {CATranspiler}", LogLevel.Trace); ;
            harmony.Patch(CheckAction, transpiler: CATranspiler);        
        }

        private void OnGameLanuched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<Integrations.GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu"); 
            if (api != null)
            {
                Monitor.Log("Accessed mod-provided API for Generic Mod Config Menu.",LogLevel.Trace);
                api.RegisterModConfig(ModManifest, () => OurConfig = new CartConfig(), () => Helper.WriteConfig(OurConfig));
                api.RegisterClampedOption(ModManifest, "Monday Apparence", "The chance for the cart to appear on Monday", () => (float)OurConfig.MondayChance, (float val) => OurConfig.MondayChance = val, 0f, 1f);
                api.RegisterClampedOption(ModManifest, "Tuesday Apparence", "The chance for the cart to appear on Tuesday", () => (float)OurConfig.TuesdayChance, (float val) => OurConfig.TuesdayChance = val, 0f, 1f);
                api.RegisterClampedOption(ModManifest, "Wednesday Apparence", "The chance for the cart to appear on Wednesday", () => (float)OurConfig.WednesdayChance, (float val) => OurConfig.WednesdayChance = val, 0f, 1f);
                api.RegisterClampedOption(ModManifest, "Thursday Apparence", "The chance for the cart to appear on Thursday", () => (float)OurConfig.ThursdayChance, (float val) => OurConfig.ThursdayChance = val, 0f, 1f);
                api.RegisterClampedOption(ModManifest, "Friday Apparence", "The chance for the cart to appear on Friday", () => (float)OurConfig.FridayChance, (float val) => OurConfig.FridayChance = val, 0f, 1f);
                api.RegisterClampedOption(ModManifest, "Saturday Apparence", "The chance for the cart to appear on Saturday", () => (float)OurConfig.SaturdayChance, (float val) => OurConfig.SaturdayChance = val, 0f, 1f);
                api.RegisterClampedOption(ModManifest, "Sunday Apparence", "The chance for the cart to appear on Sunday", () => (float)OurConfig.SundayChance, (float val) => OurConfig.SundayChance = val, 0f, 1f);
                api.RegisterSimpleOption(ModManifest, "Appear Only At Start Of Season", "If selected, the cart only appears at the beginning of the season", () => OurConfig.AppearOnlyAtStartOfSeason, (bool val) => OurConfig.AppearOnlyAtStartOfSeason = val);
                api.RegisterSimpleOption(ModManifest, "Appear Only At End Of Season", "If selected, the cart only appears at the end of the season", () => OurConfig.AppearOnlyAtEndOfSeason, (bool val) => OurConfig.AppearOnlyAtEndOfSeason = val);
                api.RegisterSimpleOption(ModManifest, "Appear Only At Start and End Of Season", "If selected, the cart only appears at the beginning and end of the season", () => OurConfig.AppearOnlyAtStartAndEndOfSeason, (bool val) => OurConfig.AppearOnlyAtStartAndEndOfSeason = val);
                api.RegisterSimpleOption(ModManifest, "Appear Only Every Other Week", "If selected, the cart only appears every other week", () => OurConfig.AppearOnlyEveryOtherWeek, (bool val) => OurConfig.AppearOnlyEveryOtherWeek = val);
                api.RegisterSimpleOption(ModManifest, "Use Vanilla Max", "The game defaults to a max of 790. Turning this off allows PPJA assets to appear in the cart.", () => OurConfig.UseVanillaMax, (bool val) => OurConfig.UseVanillaMax = val);
                api.RegisterClampedOption(ModManifest, "Opening Time", "The time the cart opens. Please select a 10-minute time.", () => OurConfig.OpeningTime, (int val) => OurConfig.OpeningTime = val, 600, 2600);
                api.RegisterClampedOption(ModManifest, "Closing Time", "The time the cart closes for the night. Please select a 10-minute time. You don't have to go home, but you can't stay here.", () => OurConfig.ClosingTime, (int val) => OurConfig.ClosingTime = val, 600, 2600);
            }
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            Random r = new();
            double randChance = r.NextDouble();

            if (Game1.getLocationFromName("Forest") is not Forest f)
                throw new Exception("The Forest is not loaded. Please verify your game is properly installed.");
            
            //get the day
            DayOfWeek day = GetDayOfWeek(SDate.Now());
            var dayChance = day switch
            {
                DayOfWeek.Monday => OurConfig.MondayChance,
                DayOfWeek.Tuesday => OurConfig.TuesdayChance,
                DayOfWeek.Wednesday => OurConfig.WednesdayChance,
                DayOfWeek.Thursday => OurConfig.ThursdayChance,
                DayOfWeek.Friday => OurConfig.FridayChance,
                DayOfWeek.Saturday => OurConfig.SaturdayChance,
                DayOfWeek.Sunday => OurConfig.SundayChance,
                _ => 0,
            };

            /* Start of the Season - Day 1. End of the Season - Day 28. Both is obviously day 1 and 28 
               Every other week is only on days 8-14 and 22-28) */

            bool setCartToOn = false;
            if (OurConfig.AppearOnlyAtEndOfSeason)
            {
                if (Game1.dayOfMonth == 28)
                    setCartToOn = true;
            }

            else if (OurConfig.AppearOnlyAtStartAndEndOfSeason)
            {
                if (Game1.dayOfMonth == 28 || Game1.dayOfMonth == 1)
                    setCartToOn = true;
            }

            else if (OurConfig.AppearOnlyAtStartAndEndOfSeason)
            {
                if (Game1.dayOfMonth == 28 || Game1.dayOfMonth == 1)
                    setCartToOn = true;
            }

            else if (OurConfig.AppearOnlyAtStartOfSeason)
            {
                if (Game1.dayOfMonth == 1)
                    setCartToOn = true;
            }

            else if (OurConfig.AppearOnlyEveryOtherWeek)
            {
                if ((Game1.dayOfMonth >= 8 && Game1.dayOfMonth <= 14) || (Game1.dayOfMonth >= 22 && Game1.dayOfMonth <= 28))
                {
                    if (dayChance > randChance)
                    {
                        setCartToOn = true;
                    }
                }
            }

            else
            {
                if (dayChance > randChance)
                {
                    setCartToOn = true;
                }
            }

            if (setCartToOn)
            {
                f.travelingMerchantDay = true;
                f.travelingMerchantBounds.Add(new Rectangle(1472, 640, 492, 116));
                f.travelingMerchantBounds.Add(new Rectangle(1652, 744, 76, 48));
                f.travelingMerchantBounds.Add(new Rectangle(1812, 744, 104, 48));
                foreach (Rectangle travelingMerchantBound in f.travelingMerchantBounds)
                    Utility.clearObjectsInArea(travelingMerchantBound, f);       
            }
            else
            {
                //clear other values
                f.travelingMerchantBounds.Clear();
                f.travelingMerchantDay = false;
                
            }
        }

        private static DayOfWeek GetDayOfWeek(SDate Target)
        {
            return (Target.Day % 7) switch
            {
                0 => DayOfWeek.Sunday,
                1 => DayOfWeek.Monday,
                2 => DayOfWeek.Tuesday,
                3 => DayOfWeek.Wednesday,
                4 => DayOfWeek.Thursday,
                5 => DayOfWeek.Friday,
                6 => DayOfWeek.Saturday,
                _ => 0,
            };
        }

        public static bool IsValidHours()
        {
            int TimeOfDay = Game1.timeOfDay;
            if (TimeOfDay >= OurConfig.OpeningTime && TimeOfDay <= OurConfig.ClosingTime)
                return true;

            return false;
        }
    }
}   
