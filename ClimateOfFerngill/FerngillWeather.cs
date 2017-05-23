using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TwilightCore.StardewValley;
using TwilightCore;
using TwilightCore.PRNG;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClimateOfFerngill
{
    /// <summary>
    /// This class controls the weather of the mod as well as many function to set it.
    /// </summary>
    public class FerngillWeather
    {
        public bool IsBlizzard { get; private set; }
        public bool IsHeatwave { get; private set; }
        public bool IsFrost { get; private set; }

        private double TodayHigh { get; set; }
        private double TodayLow { get; set; }
        private SDVWeather CurrentWeather { get; set; }
        private ClimateConfig Config { get; set; }
        private MersenneTwister pRNG;
        private FerngillClimate GameClimate;
        public ProbabilityDistribution<string> WeatherOdds { get; set; }
        private IMonitor Logger;
        private Vector2 snowPos; //snow elements

        /// <summary>
        /// This contains the climate data used to generate chances.
        /// </summary>
        public FerngillClimate WeatherModel { get; private set; }

        public FerngillWeather(ClimateConfig config, FerngillClimate climate, MersenneTwister Dice, IMonitor log)
        {
            IsBlizzard = false;
            IsHeatwave = false;
            IsFrost = false;
            GameClimate = climate;
            Config = config;
            Logger = log;
            pRNG = Dice;

            WeatherOdds = new ProbabilityDistribution<string>();
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
            if (tempGauge == "celsius") return temp + " C";
            if (tempGauge == "kelvin") return (temp + 273.15) + " K";
            
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

        //climate access functions
        public FerngillClimateTimeSpan GetClimateForDate(SDVDate Target)
        {
            return this.GameClimate.ClimateSequences.Where(c => WeatherHelper.SeasonIsWithinRange(Target.Season, c.BeginSeason, c.EndSeason))
                                                    .Where(c => Target.Day >= c.BeginDay && Target.Day <= c.EndDay)
                                                    .First();
        }


        public double GetStormOdds(SDVDate Target)
        {
                return GetClimateForDate(Target).RetrieveOdds(pRNG, "storm", Target.Day);
        }

        public void HandleStaminaChanges(bool passedThresholdOutside)
        {
            int HighStaminaPenalty = (int)Math.Ceiling(Config.StaminaPenalty * 1.5);

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

            if ((IsBlizzard || IsHeatwave) && pRNG.NextDouble() < .15)
            {
                SDVUtilities.ShowMessage("The harsh weather conditions have tired you out!");
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
            if (IsBlizzard) SDVUtilities.ShowMessage("There's a dangerous blizzard out today. Be careful!");
            if (IsFrost) SDVUtilities.ShowMessage("The temperature tonight will be dipping below freezing. Your crops may be vulnerable to frost!");
            if (IsHeatwave) SDVUtilities.ShowMessage("A massive heatwave is sweeping the valley. Stay hydrated!");
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
            GenerateWeatherOdds(SDVDate.Today);
        }

        public void Reset()
        {
            IsBlizzard = false;
            IsHeatwave = false;
            IsFrost = false;
            CurrentWeather = SDVWeather.None;
            TodayHigh = -1000;
            TodayLow = -1000;
        }

        public void GenerateWeatherOdds(SDVDate Target)
        {
            //first, pull the climate time span for this
            FerngillClimateTimeSpan CurrentConditions = GetClimateForDate(Target);

            //Now check for weathers. We will check in the pattern:
            // Rain - debris. 
            // Storms and Snow are treated as sub weathers, and as such occupy a 
            // second track

            double rainOdds = CurrentConditions.RetrieveOdds(pRNG, "rain", Target.Day);
            double debrisOdds = CurrentConditions.RetrieveOdds(pRNG, "debris", Target.Day);
            double stormOdds = CurrentConditions.RetrieveOdds(pRNG, "storm", Target.Day);
            double snowOdds = CurrentConditions.RetrieveOdds(pRNG, "snow", Target.Day);

            //config overflow
            WeatherOdds.SetOverflowResult("sunny");

            //weathers
            WeatherOdds.AddNewEndPoint(rainOdds, "rain");

            if (debrisOdds + rainOdds < 1)
                WeatherOdds.AddNewEndPoint(debrisOdds, "debris");
            else
            {
                double newOdds = debrisOdds - ((debrisOdds + rainOdds) - 1);
                WeatherOdds.AddNewEndPoint(newOdds, "debris");
            }



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

            if (Game1.isLightning && !Game1.isRaining)
                CurrentWeather = SDVWeather.DryLightning;

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

        public void SetForThunderSnow()
        {
            Game1.isRaining = false;
            Game1.isLightning = true;
            Game1.isSnowing = true;
            Game1.isDebrisWeather = false;

            Game1.debrisWeather.Clear();
            this.CurrentWeather = SDVWeather.Thundersnow;
        }

        public void SetForBlizzard()
        {
            Game1.isRaining = false;
            Game1.isLightning = false;
            Game1.isSnowing = true;
            Game1.isDebrisWeather = false;

            Game1.debrisWeather.Clear();
            this.CurrentWeather = SDVWeather.Blizzard;
        }

        public void DrawBlizzard()
        {
            snowPos = Game1.updateFloatingObjectPositionForMovement(snowPos, new Vector2(Game1.viewport.X, Game1.viewport.Y),
                        Game1.previousViewportPosition, -1f);
            snowPos.X = snowPos.X % (16 * Game1.pixelZoom);
            Vector2 position = new Vector2();
            float num1 = -16 * Game1.pixelZoom + snowPos.X % (16 * Game1.pixelZoom);
            while ((double)num1 < Game1.viewport.Width)
            {
                float num2 = -16 * Game1.pixelZoom + snowPos.Y % (16 * Game1.pixelZoom);
                while (num2 < (double)Game1.viewport.Height)
                {
                    position.X = (int)num1;
                    position.Y = (int)num2;
                    Game1.spriteBatch.Draw(Game1.mouseCursors, position, new Microsoft.Xna.Framework.Rectangle?
                        (new Microsoft.Xna.Framework.Rectangle
                        (368 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 150) % 1200.0) / 75 * 16, 192, 16, 16)),
                        Color.White * Game1.options.snowTransparency, 0.0f, Vector2.Zero,
                        Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                    num2 += 16 * Game1.pixelZoom;
                }
                num1 += 16 * Game1.pixelZoom;
            }
        }
    }
}
