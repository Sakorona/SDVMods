using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace TwilightShards.WeatherIllnesses
{
    public class WeatherIllnesses : Mod
    {
        private IllnessConfig IllnessConfig { get; set; }
        private MersenneTwister Dice { get; set; }
        private StaminaDrain StaminaMngr { get; set;}

        private int TicksOutside;
        private int TicksTotal;
        private int prevToEatStack = -1;
        private bool wasEating = false;

        private bool UseClimates = false;
        private Integrations.IClimatesOfFerngillAPI climatesAPI;

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>
        public override void Entry(IModHelper helper)
        {
            IllnessConfig = helper.ReadConfig<IllnessConfig>();
            Dice = new MersenneTwister();
            StaminaMngr = new StaminaDrain(IllnessConfig, Helper.Translation, Monitor);
            TicksOutside = TicksTotal = 0;

            SaveEvents.AfterReturnToTitle += HandleResetToMenu;
            TimeEvents.AfterDayStarted += HandleNewDay;
            GameEvents.UpdateTick += HandleChangesPerTick;
            TimeEvents.TimeOfDayChanged += TenMinuteUpdate;
            GameEvents.FirstUpdateTick += HandleIntegrations;
        }

        private void HandleIntegrations(object sender, EventArgs e)
        {
            IManifest manifestCheck = Helper.ModRegistry.Get("KoihimeNakamura.ClimatesOfFerngill");
            if (manifestCheck != null)
            {
                if (!manifestCheck.Version.IsOlderThan("1.3.8"))
                {
                    climatesAPI = Helper.ModRegistry.GetApi<Integrations.IClimatesOfFerngillAPI>("KoihimeNakamura.ClimatesOfFerngill");

                    if (climatesAPI != null)
                    {
                       UseClimates = true;                        
                       Monitor.Log("Climates of Ferngill integration enabled", LogLevel.Info);
                    }
                }
                else
                {
                    Monitor.Log($"Climates of Ferngill detected, but not of a sufficient version. Req:1.3.8 Detected:{manifestCheck.Version.ToString()}. Skipping..");
                }
            }
            else
            {
                Monitor.Log("Climates of Ferngill not present. Skipping Integration.");
            }
        }

        private void TenMinuteUpdate(object sender, EventArgsIntChanged e)
        {
            if (!Game1.hasLoadedGame)
                return;

            string weatherStatus = "";
            //get current weather string
            if (UseClimates)
                weatherStatus = climatesAPI.GetCurrentWeatherName();
            else
                weatherStatus = SDVUtilities.GetWeatherName();


                Game1.player.stamina += StaminaMngr.TenMinuteTick(Game1.player.hat?.which, weatherStatus, TicksOutside, TicksTotal, Dice);

            if (Game1.player.stamina <= 0)
                SDVUtilities.FaintPlayer();

            TicksTotal = 0;
            TicksOutside = 0;
        }

        private void HandleChangesPerTick(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame)
                return;

            if (Game1.isEating != wasEating)
            {
                if (!Game1.isEating)
                {
                    // Apparently this happens when the ask to eat dialog opens, but they pressed no.
                    // So make sure something was actually consumed.
                    if (prevToEatStack != -1 && (prevToEatStack - 1 == Game1.player.itemToEat.Stack))
                    {
                        if (Game1.player.itemToEat.parentSheetIndex == 351)
                            StaminaMngr.ClearDrain();
                    }
                }
                prevToEatStack = (Game1.player.itemToEat != null ? Game1.player.itemToEat.Stack : -1);
            }
            wasEating = Game1.isEating;

            if (Game1.currentLocation.isOutdoors)
            {
                TicksOutside++;
            }

            TicksTotal++;
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            TicksOutside = TicksTotal = 0;
            StaminaMngr.OnNewDay();
        }

        private void HandleResetToMenu(object sender, EventArgs e)
        {
            TicksTotal = TicksOutside = 0;
            StaminaMngr.Reset();
        }
    }
}
