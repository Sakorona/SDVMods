using StardewValley;
using StardewModdingAPI;
using TwilightShards.Stardew.Common;
using TwilightShards.Common;
using System;
using System.Collections.Generic;

namespace TwilightShards.WeatherIllnesses
{
    internal class StaminaDrain
    {
        private IllnessConfig IllOptions;
        private ITranslationHelper Helper;
        private bool FarmerSick;
        public bool FarmerHasBeenSick;
        private IMonitor Monitor;

        private readonly int FROST = 1;
        private readonly int HEATWAVE = 2;

        public StaminaDrain(IllnessConfig Options, ITranslationHelper SHelper, IMonitor mon)
        {
            IllOptions = Options;
            Helper = SHelper;
            Monitor = mon;
        }

        public bool IsSick()
        {
            return this.FarmerSick;
        }

        public void MakeSick(int reason = 0)
        {
            FarmerSick = true;
            FarmerHasBeenSick = true;
            if (reason == FROST)
            {
                SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_freeze"));
            }
            else if (reason == HEATWAVE)
            {
                SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_exhaust"));
            }
            else
                SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_sick"));
        }

        public void OnNewDay()
        {
            FarmerSick = false;
        }

        public void ClearDrain()
        {
            FarmerSick = false;
            SDVUtilities.ShowMessage(Helper.Get("hud-text.desc_cold_removed"));
        }

        public void Reset()
        {
            FarmerSick = false;
        }

        public bool FarmerCanGetSick()
        {
            if (FarmerSick && !IllOptions.SickMoreThanOnce)
                return false;

            if (!IllOptions.SickMoreThanOnce && FarmerHasBeenSick)
                return false;

            return true;
        }

        public int TenMinuteTick(int? hatID, string conditions, int ticksOutside, int ticksTotal, MersenneTwister Dice)
        {
            double amtOutside = ticksOutside / (double)ticksTotal, totalMulti = 0;
            int staminaAffect = 0;
            int sickReason = 0;
            var condList = new List<string>();

            if (IllOptions.Verbose)
                 Monitor.Log($"Ticks: {ticksOutside}/{ticksTotal} with percentage {amtOutside.ToString("N3")} against" +
                     $" target {IllOptions.PercentageOutside}"); 

            //Logic: At all times, if the today danger is not null, we should consider processing.
            //However: If it's frost, only at night. If it's a heatwave, only during the day.
            //So this means: if it's storming, you can get sick. If it's a blizzard or thundersnow.. you can get sick
            //If it's frost or heatwave during the appropriate time.. you can get sick

            //First, update the sick status
            bool farmerCaughtCold = false;
            double sickOdds = IllOptions.ChanceOfGettingSick - Game1.dailyLuck;

            //weee.
            if (hatID == 28 && (conditions.Contains("lightning") || conditions.Contains("stormy") || conditions.Contains("thundersnow")))
                sickOdds -= (Dice.NextDoublePositive() / 5.0) - .1;

            if (hatID == 25 && conditions.Contains("blizzard") || conditions.Contains("whiteout"))
                sickOdds -= .22;

            if (hatID == 4 && conditions.Contains("heatwave") && !SDVTime.IsNight)
                sickOdds -= .11;

            farmerCaughtCold = (Dice.NextDoublePositive() <= sickOdds) && (IllOptions.StaminaDrain > 0);

            if (amtOutside >= IllOptions.PercentageOutside && farmerCaughtCold || this.FarmerSick)
            {
                //check if it's a valid condition
                if (FarmerCanGetSick())
                {
                    //rewrite time..
                    if (conditions.Contains("blizzard") || conditions.Contains("lightning") || conditions.Contains("stormy") || conditions.Contains("thundersnow") || (conditions.Contains("frost") && SDVTime.IsNight) ||
                        (conditions.Contains("heatwave") && !SDVTime.IsNight))
                    {
                        if ((conditions.Contains("heatwave") && !SDVTime.IsNight))
                            sickReason = HEATWAVE;
                        else if (conditions.Contains("frost") && SDVTime.IsNight)
                            sickReason = FROST;

                        this.MakeSick(sickReason);
                    }
                }

                //now that we've done that, go through the various conditions
                if (this.FarmerSick && (conditions.Contains("lightning") || conditions.Contains("stormy") || conditions.Contains("thundersnow")))
                {
                    totalMulti += 1;
                    condList.Add("Lightning or Thundersnow");
                }

                if (this.FarmerSick && conditions.Contains("fog"))
                {
                    totalMulti += .5;
                    condList.Add("Fog");
                }

                if (this.FarmerSick && conditions.Contains("fog") && SDVTime.IsNight)
                {
                    totalMulti += .25;
                    condList.Add("Night Fog");
                }

                if (this.FarmerSick && conditions.Contains("blizzard") && !conditions.Contains("whiteout"))
                {
                    totalMulti += 1.25;
                    condList.Add("Blizzard");
                }

                if (this.FarmerSick && conditions.Contains("blizzard") && conditions.Contains("whiteout"))
                {
                    totalMulti += 2.25;
                    condList.Add("White Out");
                }

                if (this.FarmerSick && conditions.GetCurrentConditions().HasFlag(CurrentWeather.Frost) && SDVTime.IsNight)
                {
                    totalMulti += 1.25;
                    condList.Add("Night Frost");
                }

                if (this.FarmerSick && conditions.GetCurrentConditions().HasAllFlags(CurrentWeather.Lightning | CurrentWeather.Snow) && SDVTime.IsNight)
                {
                    totalMulti += .5;
                    condList.Add("Night Thundersnow");
                }

                if (this.FarmerSick && conditions.GetCurrentConditions().HasFlag(CurrentWeather.Blizzard) && SDVTime.IsNight)
                {
                    totalMulti += .5;
                    condList.Add("Night Blizzard");
                }

                if (this.FarmerSick && conditions.GetCurrentConditions().HasFlag(CurrentWeather.Heatwave) && !SDVTime.IsNight)
                {
                    totalMulti += 1.25;
                    condList.Add("Day Heatwave");
                }
            }

            staminaAffect -= (int)Math.Floor(IllOptions.StaminaDrain * totalMulti);

            if (IllOptions.Verbose && this.FarmerSick)
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
    }
}
