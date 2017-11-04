using StardewValley;
using StardewModdingAPI;
using TwilightShards.Stardew.Common;
using TwilightShards.Common;
using System;
using System.Collections.Generic;

namespace ClimatesOfFerngillRebuild
{
    internal class StaminaDrain
    {
        private WeatherConfig Config;
        private ITranslationHelper Helper;
        private bool FarmerSick;
        private bool SickToday;
        private IMonitor Monitor;

        public StaminaDrain(WeatherConfig Options, ITranslationHelper SHelper, IMonitor mon)
        {
            SickToday = false;
            Config = Options;
            Helper = SHelper;
            Monitor = mon;
        }

        public bool IsSick()
        {
            return this.FarmerSick;
        }

       public void MakeSick()
        {
            FarmerSick = true;
            SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_sick"));
        }

        public void OnNewDay()
        {
            SickToday = false;
            FarmerSick = false;
        }

        public void ClearDrain()
        {
            FarmerSick = false;
            SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_cold_removed"));
        }

        public void Reset()
        {
            SickToday = false;
            FarmerSick = false;
        }

        public bool FarmerCanGetSick()
        {
            if (FarmerSick)
            {
                if (Config.Verbose)
                    Monitor.Log("Farmer is already sick, returning false");
                return false;
            }

            if (SickToday && !Config.SickMoreThanOnce)
                return false;

            return true;
        }

        public int TenMinuteTick(SpecialWeather conditions, int ticksOutside, int ticksTotal, MersenneTwister Dice, int weather, bool IsFoggy)
        {
            double amtOutside = ticksOutside / (double)ticksTotal, totalMulti = 0;
            int staminaAffect = 0;
            var condList = new List<string>();

            if (Config.Verbose)
                Monitor.Log($"Ticks: {ticksOutside}/{ticksTotal} with percentage {amtOutside.ToString("N3")} against" +
                    $" target {Config.AffectedOutside}");

            //Logic: At all times, if the today danger is not null, we should consider processing.
            //However: If it's frost, only at night. If it's a heatwave, only during the day.
            //So this means: if it's storming, you can get sick. If it's a blizzard or thundersnow.. you can get sick
            //If it's frost or heatwave during the appropriate time.. you can get sick

            //First, update the sick status
            if (amtOutside >= Config.AffectedOutside && ((Dice.NextDoublePositive() >= Config.ChanceOfGettingSick) || this.FarmerSick))
            {
                //check if it's a valid condition
                if (ValidConditions(weather, conditions) && FarmerCanGetSick())
                {
                    this.MakeSick();

                    if (Config.Verbose)
                        Monitor.Log("Making the farmer sick");
                }

                //test status
                if (Config.Verbose)              
                    Monitor.Log($"Status update. Farmer Sick: {FarmerSick} and Valid Conditions: {ValidConditions(weather, conditions)}");

                //now that we've done that, go through the various conditions
                if (this.FarmerSick && (weather == Game1.weather_lightning || conditions == SpecialWeather.Thundersnow))
                {
                    totalMulti += 1;
                    condList.Add("Lightning or Thundersnow");
                }

                if (this.FarmerSick && IsFoggy)
                {
                    totalMulti += .5;
                    condList.Add("Fog");
                }

                if (this.FarmerSick && IsFoggy && SDVTime.IsNight)
                {
                    totalMulti += .25;
                    condList.Add("Night Fog");
                }

                if (this.FarmerSick && conditions == SpecialWeather.Blizzard)
                {
                    totalMulti += 1.25;
                    condList.Add("Blizzard");
                }

                if (this.FarmerSick && WeatherConditions.IsFrost(conditions) && SDVTime.IsNight)
                {
                    totalMulti += 1.25;
                    condList.Add("Night Frost");
                }

                if (this.FarmerSick && conditions == SpecialWeather.Thundersnow && SDVTime.IsNight)
                {
                    totalMulti += .5;
                    condList.Add("Night Thundersnow");
                }

                if (this.FarmerSick && conditions == SpecialWeather.Blizzard && SDVTime.IsNight)
                {
                    totalMulti += .5;
                    condList.Add("Night Blizzard");
                }

                if (this.FarmerSick && WeatherConditions.IsHeatwave(conditions) && !SDVTime.IsNight)
                {
                    totalMulti += 1.25;
                    condList.Add("Day Heatwave");
                }
            }


            staminaAffect -= (int)Math.Floor(Config.StaminaDrain * totalMulti);
            
            if (Config.Verbose && this.FarmerSick)
            {
                string condString = "[ ";
                for (int i = 0; i < condList.Count; i++)
                {
                    if (i != condList.Count - 1)
                    {
                        condString += condList[i] + ", ";
                    }
                    else
                    {
                        condString += condList[i];
                    }
                }
                condString += " ]";

                Monitor.Log($"[{Game1.timeOfDay}] Conditions for the drain are {condString} for a total multipler of {totalMulti} for a total drain of {staminaAffect}");
            }
            
            return staminaAffect;
        }

        private bool ValidConditions(int weather, SpecialWeather conditions)
        {
            return weather == Game1.weather_lightning || conditions == SpecialWeather.Blizzard || WeatherConditions.IsFrost(conditions) || WeatherConditions.IsHeatwave(conditions) || conditions == SpecialWeather.Thundersnow;
        }
    }
}
