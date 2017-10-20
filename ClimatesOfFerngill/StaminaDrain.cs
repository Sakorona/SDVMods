using StardewValley;
using TwilightCore.StardewValley;
using StardewModdingAPI;

namespace ClimatesOfFerngillRebuild
{
    internal enum StaminaStatus
    {
        NoDrain = 0,
        Level1 = 1,
        Level2 = 2
    }

    internal class StaminaDrain
    {
        private StaminaStatus HealthLevel;
        private WeatherConfig Config;
        private StaminaStatus TodayDanger;
        private ITranslationHelper Helper;
        private bool SickToday;
        private IMonitor Monitor;

        public StaminaDrain(WeatherConfig Options, ITranslationHelper SHelper, IMonitor mon)
        {
            SickToday = false;
            Config = Options;
            Helper = SHelper;
            Monitor = mon;
            HealthLevel = StaminaStatus.NoDrain;
            TodayDanger = StaminaStatus.NoDrain;
        }

        public bool IsSick()
        {
            if (HealthLevel != StaminaStatus.NoDrain)
                return true;

            return false;
        }

        public void OnNewDay(WeatherConditions Current)
        {
            SickToday = false; 
            HealthLevel = StaminaStatus.NoDrain;
            if (Current.UnusualWeather == SpecialWeather.Frost || 
                (Current.TodayWeather == Game1.weather_lightning)
                || WeatherConditions.IsHeatwave(Current.UnusualWeather))
            {
                //level 1 drain
                if (Config.Verbose)
                    Monitor.Log("Level 1 drain selected");

                TodayDanger = StaminaStatus.Level1;
            }

            else if (Current.UnusualWeather == SpecialWeather.Blizzard || 
                    Current.UnusualWeather == SpecialWeather.Thundersnow ||
                    (Current.TodayWeather == Game1.weather_lightning && 
                        WeatherConditions.IsHeatwave(Current.UnusualWeather)))
            {
                    //level 2 drain
                    if (Config.Verbose)
                        Monitor.Log("Level 2 drain selected");

                    TodayDanger = StaminaStatus.Level2;
            }

            else
            {
                //why was this NOT IN AN ELSE.
                if (Config.Verbose)
                    Monitor.Log("No drain selected");

                TodayDanger = StaminaStatus.NoDrain;
            }
        }

        public void ClearDrain()
        {
            HealthLevel = StaminaStatus.NoDrain;
            SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_cold_removed"));
        }

        public void Reset()
        {
            SickToday = false;
            HealthLevel = StaminaStatus.NoDrain;
            TodayDanger = StaminaStatus.NoDrain;
        }

        public int TenMinuteTick(SpecialWeather conditions, int ticksOutside, int ticksTotal)
        {
            double amtOutside = ticksOutside / (double)ticksTotal;
            bool processStamina = false;

            /*if (Config.Verbose)
                Monitor.Log($"Ticks: {ticksOutside}/{ticksTotal} with percentage {amtOutside.ToString("N3")} against" +
                    $" target {Config.AffectedOutside}");         */

            //poll for amount.
            // 1. Check to make sure you've been outside enough in this span AND that you've not been sick or can get sick
            //    an unlimited amount of times per day
            // 2. Check specifically for frost and night
            // 3. Check for heatwaves during the day. 
            // 4. Check for lightning and blizzards at all times. 
            if ((amtOutside >= Config.AffectedOutside) && (!SickToday || Config.SickMoreThanOnce) &&
                ((Game1.isStartingToGetDarkOut() && conditions == SpecialWeather.Frost) ||
                 (!Game1.isStartingToGetDarkOut() && WeatherConditions.IsHeatwave(conditions)))
                )
            {
                processStamina = true;
            }

            if ((amtOutside >= Config.AffectedOutside) && (!SickToday || Config.SickMoreThanOnce) &&
                ((Game1.isLightning) || (conditions == SpecialWeather.Blizzard)))
            {
                processStamina = true;
            }

            if (processStamina)
            { 
                //if (Config.Verbose) Monitor.Log("Affected time is valid, altering stamina");

                if (HealthLevel != TodayDanger) {

                    if (TodayDanger == StaminaStatus.Level1)
                        SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_sick"));
                    if (TodayDanger == StaminaStatus.Level2)
                        SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_flu"));
                }

                if (!SickToday || Config.SickMoreThanOnce)
                {
                    HealthLevel = TodayDanger;
                    SickToday = true;
                }

                //so, a bit here.
                //lightning and heatwaves is a thing - this corrects for night.
                if (Game1.isLightning && (Game1.isStartingToGetDarkOut() && WeatherConditions.IsHeatwave(conditions)))
                {
                    HealthLevel = StaminaStatus.Level1;
                    TodayDanger = StaminaStatus.Level1;
                }

                switch (HealthLevel)
                {
                    case StaminaStatus.NoDrain:
                        return 0;
                    case StaminaStatus.Level1:
                        return -1 * Config.Tier1Drain;
                    case StaminaStatus.Level2:
                        return -1 * Config.Tier2Drain;
                    default:
                        return 0;
                }
            }
            return 0;
        }

    }
}
