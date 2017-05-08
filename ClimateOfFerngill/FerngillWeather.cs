using NPack;
using StardewModdingAPI;
using StardewValley;
using System;

namespace ClimateOfFerngill
{
    public class FerngillWeather
    {
        private double TodayHigh { get; set; }
        private double TodayLow { get; set; }
        private SDVWeather CurrentWeather { get; set; }
        private ClimateConfig Config { get; set; }
        private MersenneTwister pRNG;
        private IMonitor Logger;
        private bool IsExhausted;
        private bool HasGottenColdToday;
        public bool IsBlizzard { get; private set; }
        public bool IsHeatwave { get; private set; }
        public bool IsFrost { get; private set; }

        public FerngillWeather(ClimateConfig config, MersenneTwister Dice, IMonitor log)
        {
            IsBlizzard = false;
            IsHeatwave = false;
            IsFrost = false;
            HasGottenColdToday = false;
            IsExhausted = false;
            Config = config;
            Logger = log;
            pRNG = Dice;
        }

        public void CheckForHazardConditions(MersenneTwister Dice)
        {
            CheckHeatwave();
            CheckFrost();
        }

        public double GetTodayHighInScale() => TempInScale(TodayHigh);
        public double GetTodayLowInScale() => TempInScale(TodayLow);

        public double TempInScale(double temp)
        {
            if (Config.TempGauge == "celsius") return temp;
            if (Config.TempGauge == "kelvin") return (temp + 273.15);
            if (Config.TempGauge == "rankine") return ((temp + 273.15) * 1.8);
            if (Config.TempGauge == "fahrenheit") return ((temp * 1.8) + 32);
            if (Config.TempGauge == "romer") return ((temp * 1.904761905) + 7.5);
            if (Config.TempGauge == "delisle") return ((100 - temp) * 1.5);
            if (Config.TempGauge == "reaumur") return (temp *.8);

            return temp;
        }

        public string DisplayHighTemperature() => DisplayTemperature(Config.TempGauge, TodayHigh);
        public string DisplayLowTemperature() => DisplayTemperature(Config.TempGauge, TodayLow);

        public string DisplayHighTemperatureSG() => DisplayTemperature(Config.SecondScaleGauge, TodayHigh);
        public string DisplayLowTemperatureSG() => DisplayTemperature(Config.SecondScaleGauge, TodayLow);

        private string DisplayTemperature(string tempGauge, double temp)
        {
            //base temps are always in celsius
            if (tempGauge == "celsius")
            {
                return temp + " C";
            }

            if (tempGauge == "kelvin")
            {
                return (temp + 273.15) + " K";
            }

            if (tempGauge == "rankine")
            {
                double tmpTemp = (temp + 273.15) * 1.8;
                return string.Format("{0:0.00}", tmpTemp) + " Ra";
            }

            if (tempGauge == "fahrenheit")
            {
                double tmpTemp = (temp * 1.8) + 32;
                return string.Format("{0:0.00}", tmpTemp) + " F";
            }

            if (tempGauge == "romer")
            {
                return string.Format("{0:0.00}", (temp * 1.904761905) + 7.5) + " Ro";
            }

            if (tempGauge == "delisle")
            {
                return string.Format("{0:0.00}", ((100 - temp) * 1.5)) + " De";
            }

            if (tempGauge == "reaumur")
            {
                return string.Format("{0:0.00}", temp * .8) + " Re";
            }

            return "ERROR";
        }

        public string GetTempScale()
        {
            switch (Config.TempGauge)
            {
                case "reaumur":
                    return "Re";
                case "delisle":
                    return "De";
                case "romer":
                    return "Ro";
                case "celsius":
                    return "C";
                case "kelvin":
                    return "K";
                case "rankine":
                    return "Ra";
                case "fahrenheit":
                    return "F";
            }
            return "C";
        }

        public bool HasACold()
        {
            return this.IsExhausted;
        }

        public void RemoveCold()
        {
            IsExhausted = false;
            Game1.addHUDMessage(new HUDMessage("You are no longer exhausted!"));
        }

        public void CatchACold()
        {
            //run non specific code first
            if (Game1.currentLocation.IsOutdoors && Game1.isLightning && !HasGottenColdToday)
            {
                double diceChance = pRNG.NextDouble();
                if (Config.TooMuchInfo)
                    Logger.Log($"The chance of exhaustion is: {diceChance} with the configured chance of {Config.DiseaseChance}");

                if (diceChance < Config.DiseaseChance)
                {
                    IsExhausted = true;
                    InternalUtility.ShowMessage("The storm has caused you to get a cold!");
                    HasGottenColdToday = true;
                }
            }
        }

        public void HandleStaminaChanges(bool passedThresholdOutside)
        {
            int HighStaminaPenalty = (int)Math.Ceiling(Config.StaminaPenalty * 1.5);

            //disease code.
            if (IsExhausted)
            {
                Game1.player.stamina = Game1.player.stamina - Config.StaminaPenalty;
            }

            //heatwave or blizzard code.
            if (IsHeatwave && passedThresholdOutside)
            {
                if (Config.TooMuchInfo) {
                    Logger.Log($"Running the heatwave stamina penalty:{HighStaminaPenalty} until" +
                        $" {Game1.getStartingToGetDarkTime()}");
                }

                if (Game1.timeOfDay < Game1.getStartingToGetDarkTime())
                {
                    Game1.player.stamina -= HighStaminaPenalty;
                }
            }

            if (IsBlizzard && passedThresholdOutside)
            {     
                Game1.player.stamina -= HighStaminaPenalty;
            }

            //alert code - 30% chance of appearing
            // configured to properly appear now
            // Fix: 15%
            if (IsExhausted && pRNG.NextDouble() < .15)
            {
                InternalUtility.ShowMessage("You have a cold, and feel worn out!");
            }

            if ((IsBlizzard || IsHeatwave) && pRNG.NextDouble() < .15)
            {
                InternalUtility.ShowMessage("The harsh weather conditions have tired you out!");
            }
        }

        public bool IsDangerousWeather()
        {
            if (IsBlizzard || IsFrost || IsHeatwave)
                return true;
            else
                return false;
        }

        public void ForceHeatwave()
        {
            IsHeatwave = true;
        }

        public string GetHazardMessage()
        {
            string areasAffected = " Areas affected include Zuzu City, Pelican Town...";
            if (IsBlizzard)
                return "FRWS Warning: There's a dangerous blizzard out today." + areasAffected;
            if (IsHeatwave)
                return "FRWS Warning: An unnatural heatwave is affecting the region." + areasAffected;
            if (IsFrost)
                return "FRAS Warning: Temepratures in your region will be dipping below the frost threshold. Your crops will be vulnerable.";

            return "";
        }
        public string TestHazardMessage()
        {
            string areasAffected = " Areas affected include Zuzu City, Pelican Town...";
            return "FRWS Warning: An unnatural heatwave is affecting the region." + areasAffected;
        }

        public void MessageForDangerousWeather()
        {
            if (IsBlizzard) InternalUtility.ShowMessage("There's a dangerous blizzard out today. Be careful!");
            if (IsFrost) InternalUtility.ShowMessage("The temperature tonight will be dipping below freezing. Your crops may be vulnerable to frost!");
            if (IsHeatwave) InternalUtility.ShowMessage("A massive heatwave is sweeping the valley. Stay hydrated!");
        }

        public void SetTemperatures(double high, double low)
        {
            TodayHigh = high;
            TodayLow = low;
        }

        public void SetTodayHigh(double high)
        {
            TodayHigh = high;
        }

        public void SetTodayLow(double low)
        {
            TodayLow = low;
        }

        public double GetTodayHigh()
        {
            return this.TodayHigh;
        }

        public double GetTodayLow()
        {
            return this.TodayLow;
        }

        public bool CheckBlizzard()
        {            
            if (CurrentWeather == SDVWeather.Snow)
            { 
                IsBlizzard = true;
                return true;
            } 
            return false;            
        }

        public bool CheckHeatwave()
        {
            if (TodayHigh > Config.HeatwaveWarning)
            {
                IsHeatwave = true;
                return true;
            }
            return false;
        }

        public bool CheckFrost()
        {
            if (TodayLow < Config.FrostWarning)
            {
                IsFrost = true;
                return true;
            }
            return false;
        }

        public void AlterTemps(double temp)
        {
            TodayHigh = TodayHigh + temp;
            TodayLow = TodayLow + temp;
        }

        public void GetLowFromHigh(double temp)
        {
            TodayLow = TodayHigh - temp;
        }

        public SDVWeather CurrentConditions()
        {
            return CurrentWeather;
        }

        public void UpdateForNewDay()
        {
            Reset();
        }

        public void Reset()
        {
            HasGottenColdToday = false;
            IsBlizzard = false;
            IsHeatwave = false;
            IsFrost = false;
            IsExhausted = false;
            CurrentWeather = SDVWeather.None;
            TodayHigh = -1000;
            TodayLow = -1000;
        }


        public bool IsFog(string season, MersenneTwister Dice)
        {
            //set up fog.
            double FogChance = 0;
            switch (season)
            {
                case "spring":
                    FogChance = Config.SpringFogChance;
                    break;
                case "summer":
                    FogChance = Config.SummerFogChance;
                    break;
                case "fall":
                    FogChance = Config.AutumnFogChance;
                    break;
                case "winter":
                    FogChance = Config.WinterFogChance;
                    break;
                default:
                    FogChance = 0;
                    break;
            }

            //move these out of the main loop.
            if (CurrentConditions() == SDVWeather.Rainy || CurrentConditions() == SDVWeather.Debris)
                return false;
            
            if (Dice.NextDouble() < FogChance)
            {
                return true;
            }
            else
                return false;
        }

        public void SetCurrentWeather()
        {
            if (Game1.isRaining)
            {
                if (Game1.isLightning)
                    CurrentWeather = SDVWeather.Stormy;
                else
                    CurrentWeather = SDVWeather.Rainy;
            }

            else if (Game1.isSnowing)
                CurrentWeather = SDVWeather.Snow;
            else if (Game1.isDebrisWeather)
                CurrentWeather = SDVWeather.Debris;

            else if (Game1.weddingToday == true)
                CurrentWeather = SDVWeather.Wedding;

            else if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
                CurrentWeather = SDVWeather.Festival;
            else
                CurrentWeather = SDVWeather.Sunny;
        }

        public SDVTime GetFogExpireTime(MersenneTwister dice)
        {
            double FogTimer = dice.NextDouble();
            SDVTime FogExpirTime = new SDVTime(1200);

            if (FogTimer > .75 && FogTimer <= .90)
            {
                FogExpirTime = new SDVTime(1120);
            }
            else if (FogTimer > .55 && FogTimer <= .75)
            {
                FogExpirTime = new SDVTime(1030);
            }
            else if (FogTimer > .30 && FogTimer <= .55)
            {
                FogExpirTime = new SDVTime(930);
            }
            else if (FogTimer <= .30)
            {
                FogExpirTime = new SDVTime(820);
            }

            return FogExpirTime;
        }

        public override string ToString()
        {
            string s = $"High: {TodayHigh} C and Low: {TodayLow} C, with status {CurrentWeather.ToString()}";

            if (IsBlizzard)
                s += " . There's a blizzard out";

            if (IsFrost)
                s += " . There's a high chance of a frost tonight";

            if (IsHeatwave)
                s += " . It's very hot outside, expect a good chance of a heatwave.";

            return s;
        }


    }
}
