namespace StardewNotification
{
    public class SNConfiguration
    {
        // General Notifications
        public float NotificationDuration { get; set; } = 6150;
        public bool NotifyBirthdays { get; set; } = true;
        public bool NotifyBirthdayReminder { get; set; } = true;
        public int RunNotificationsTime { get ; set; } = 0600; // 6:00 am
        public int BirthdayReminderTime { get; set; } = 1700; // 5:00 pm
        public bool NotifyFestivals { get; set; } = true;
        public bool NotifyFestivalReminders { get; set; } = false;
        public bool NotifyBookseller { get; set; } = true;
        public bool NotifyTravelingMerchant { get; set; } = true;
        public bool NotifyToolUpgrade { get; set; } = false;
        public bool NotifyMaxLuck { get; set; } = true;
        public bool NotifyMinLuck { get; set; } = true;
        public bool HideUnknownNPCBirthday { get; set; } = false;
        public bool NotifySeasonalForage { get; set; } = true;
        public bool ShowEmptyhay { get; set;} = true;
        public bool NotifyHay { get ; set; } = true;
        public bool ShowWeatherNextDay { get; set; } = true;

        public bool IncludeGingerIsland { get; set; } = true;
        public int WeatherNextDayTime { get; set; } = 1700;  //5pm
        public bool NotifyTVChannels { get; set; } = true;
        public bool ShowSpringOnionCount { get; set; } = true;

        // Harvest Notifications
        public bool NotifyFarmCave { get; set; } = true;
        public bool NotifyGreenhouseCrops { get; set; } = true;

        // Production Notifications
        public bool NotifyShed { get; set; } = true;
        public bool NotifyFarm { get; set; } = true;
        public bool NotifyGreenhouse { get; set; } = true;
        public bool NotifyCellar { get; set; } = true;
        public bool NotifyBarn { get; set; } = true;
    }
}
