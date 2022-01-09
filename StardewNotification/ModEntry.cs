using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewNotification
{
    /// <summary>
    /// The mod entry point
    /// </summary>
    public class StardewNotification : Mod
    {
        private HarvestNotification harvestableNotification;
        private GeneralNotification generalNotification;
        private ProductionNotification productionNotification;
        public static SNConfiguration Config { get; set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<SNConfiguration>();
            harvestableNotification = new HarvestNotification(Helper.Translation);
            generalNotification = new GeneralNotification();
            productionNotification = new ProductionNotification();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<Integrations.GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api != null)
            {
                Monitor.Log("Accessed mod-provided API for Generic Mod Config Menu.", LogLevel.Trace);
                api.RegisterModConfig(ModManifest, () => Config = new SNConfiguration(), () => Helper.WriteConfig(Config));
               
                api.RegisterClampedOption(ModManifest, Helper.Translation.Get("gmcmNotDurTitle"),Helper.Translation.Get("gmcmNotDurDesc"),() => (float)Config.NotificationDuration, (float val) => Config.NotificationDuration = val, 0f,14000f);
                api.RegisterClampedOption(ModManifest, Helper.Translation.Get("gmcmNotDurTitle"), Helper.Translation.Get("gmcmNotDurDesc"), () => Config.RunNotificationsTime, (int val) => Config.RunNotificationsTime = val, 600, 1400);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnBirthTitle"), Helper.Translation.Get("gmcmNotifOnBirthDesc"), () => Config.NotifyBirthdays, (bool val) => Config.NotifyBirthdays = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnBirthRemindTitle"), Helper.Translation.Get("gmcmNotifOnBirthRemindDesc"), () => Config.NotifyBirthdayReminder, (bool val) => Config.NotifyBirthdayReminder = val);   
                api.RegisterClampedOption(ModManifest, Helper.Translation.Get("gmcmNotifOnBirthRemindTimeTitle"), Helper.Translation.Get("gmcmNotifOnBirthRemindTimeDesc"), () => Config.RunNotificationsTime, (int val) => Config.RunNotificationsTime = val, 900, 1900);
                
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnFestivalTitle"), Helper.Translation.Get("gmcmNotifOnFestivalDesc"), () => Config.NotifyFestivals, (bool val) => Config.NotifyFestivals = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnMerchantTitle"), Helper.Translation.Get("gmcmNotifOnMerchantDesc"), () => Config.NotifyTravelingMerchant, (bool val) => Config.NotifyTravelingMerchant = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnToolTitle"), Helper.Translation.Get("gmcmNotifOnToolDesc"), () => Config.NotifyToolUpgrade, (bool val) => Config.NotifyToolUpgrade = val);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnGLuckTitle"), Helper.Translation.Get("gmcmNotifOnGLuckDesc"), () => Config.NotifyMaxLuck, (bool val) => Config.NotifyMaxLuck = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifOnBLuckTitle"), Helper.Translation.Get("gmcmNotifOnBLuckDesc"), () => Config.NotifyMinLuck, (bool val) => Config.NotifyMinLuck = val);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifShowHayCountTitle"), Helper.Translation.Get("gmcmNotifShowHayCountDesc"), () => Config.NotifyHay, (bool val) => Config.NotifyHay = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifEmptyHayTitle"), Helper.Translation.Get("gmcmNotifEmptyHayDesc"), () => Config.ShowEmptyhay, (bool val) => Config.ShowEmptyhay = val);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifWeatNDTitle"), Helper.Translation.Get("gmcmNotifWeatNDDesc"), () => Config.ShowWeatherNextDay, (bool val) => Config.ShowWeatherNextDay = val);
                api.RegisterClampedOption(ModManifest, Helper.Translation.Get("gmcmRemindTimeForNWDTitle"), Helper.Translation.Get("gmcmRemindTimeForNWDDesc"), () => Config.WeatherNextDayTime, (int val) => Config.WeatherNextDayTime = val, 900, 2600);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifTVChanTitle"), Helper.Translation.Get("gmcmNotifTVChanDesc"), () => Config.NotifyTVChannels, (bool val) => Config.NotifyTVChannels = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifSpringOnionTitle"), Helper.Translation.Get("gmcmNotifSpringOnionDesc"), () => Config.ShowSpringOnionCount, (bool val) => Config.ShowSpringOnionCount = val);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifFarmCaveTitle"), Helper.Translation.Get("gmcmNotifFarmCaveDesc"), () => Config.NotifyFarmCave, (bool val) => Config.NotifyFarmCave = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifGreenCropTitle"), Helper.Translation.Get("gmcmNotifGreenCropDesc"), () => Config.NotifyGreenhouseCrops, (bool val) => Config.NotifyGreenhouseCrops = val);

                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifShedProdTitle"), Helper.Translation.Get("gmcmNotifShedProdDesc"), () => Config.NotifyShed, (bool val) => Config.NotifyShed = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifGreenProdTitle"), Helper.Translation.Get("gmcmNotifGreenProdDesc"), () => Config.NotifyGreenhouse, (bool val) => Config.NotifyGreenhouse = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifCellarProdTitle"), Helper.Translation.Get("gmcmNotifCellarProdDesc"), () => Config.NotifyCellar, (bool val) => Config.NotifyCellar = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("gmcmNotifBarnProdTitle"), Helper.Translation.Get("gmcmNotifBarnProdDesc"), () => Config.NotifyBarn, (bool val) => Config.NotifyBarn = val);
            }
        }

        /// <summary>Raised after the in-game clock time changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // send daily notifications
            if (Config.RunNotificationsTime != 600 && Config.RunNotificationsTime == e.NewTime)
            {
                generalNotification.DoNewDayNotifications(Helper.Translation);
                harvestableNotification.CheckHarvestsAroundFarm();
            }

            if (Config.NotifyBirthdayReminder && e.NewTime == Config.BirthdayReminderTime)
                GeneralNotification.DoBirthdayReminder(Helper.Translation);

            if (Config.ShowWeatherNextDay && e.NewTime == Config.WeatherNextDayTime)
                GeneralNotification.DoWeatherReminder(Helper.Translation);
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && e.NewLocation is Farm && Game1.timeOfDay < 2400 && Context.IsWorldReady)
            {
                harvestableNotification.CheckHarvestsOnFarm();
                productionNotification.CheckProductionAroundFarm(Helper.Translation);
            }
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.currentSeason.Equals("Spring") && Game1.dayOfMonth == 0 && Game1.year == 1)
                return;

            // send daily notifications
            if (Config.RunNotificationsTime == 600) { 
                generalNotification.DoNewDayNotifications(Helper.Translation);
                harvestableNotification.CheckHarvestsAroundFarm();
            }
        }
    }
}
