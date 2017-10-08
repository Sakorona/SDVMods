using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public StaminaDrain(WeatherConfig Options, ITranslationHelper SHelper)
        {
            SickToday = false;
            Config = Options;
            Helper = SHelper;
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
            if (Current.UnusualWeather == SpecialWeather.Frost || (Current.TodayWeather == Game1.weather_lightning && Current.IsHeatwave())
                || Current.IsHeatwave())
            {
                //level 1 drain
                TodayDanger = StaminaStatus.Level1;
            }

            if (Current.UnusualWeather == SpecialWeather.Blizzard || Current.UnusualWeather == SpecialWeather.Thundersnow || 
                (Current.TodayWeather == Game1.weather_lightning && Current.IsHeatwave()))
            {
                //level 2 drain
                TodayDanger = StaminaStatus.Level2;
            }

            TodayDanger = StaminaStatus.NoDrain;
        }

        public void ClearDrain()
        {
            HealthLevel = StaminaStatus.NoDrain;
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
            bool quickeningTime = false;

            if (((Game1.timeOfDay >= Game1.getStartingToGetDarkTime()) && (conditions == SpecialWeather.Frost))
                || ((Game1.timeOfDay < Game1.getStartingToGetDarkTime()) && (conditions == SpecialWeather.DryLightningAndHeatwave || conditions == SpecialWeather.Heatwave))                )
                quickeningTime = true;
            
            //poll for amount.
                if (amtOutside >= Config.AffectedOutside && quickeningTime)
            {
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
