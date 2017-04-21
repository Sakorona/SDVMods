using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using NPack;
using System;

namespace ClimateOfFerngill
{
    internal class HazardousWeatherEvents
    {
        private IMonitor Logger;
        private ClimateConfig Config;
        private MersenneTwister Dice;

        private static Dictionary<SDVCrops, double> CropTemps { get; set; }

        //internal trackers
        private bool IsExhausted;

        internal HazardousWeatherEvents(IMonitor modlogger, ClimateConfig modconfig, MersenneTwister moddice)
        {
            Logger = modlogger;
            Config = modconfig;
            Dice = moddice;

            CropTemps = new Dictionary<SDVCrops, double>
            {
                { SDVCrops.Corn, 1.66 },
                { SDVCrops.Wheat, 1.66 },
                { SDVCrops.Amaranth, 1.66 },
                { SDVCrops.Sunflower, 1.66 },
                { SDVCrops.Pumpkin, 1.66 },
                { SDVCrops.Eggplant, 1.66 },
                { SDVCrops.Yam, 1.66 },
                { SDVCrops.Artichoke, 0 },
                { SDVCrops.BokChoy, 0 },
                { SDVCrops.Grape, -.55 },
                { SDVCrops.FairyRose, -2.22 },
                { SDVCrops.Beet, -2.22 },
                { SDVCrops.Cranberry, -3.33 },
                { SDVCrops.Ancient, -3.33 },
                { SDVCrops.SweetGemBerry, -3.33 }
            };
        }

        internal void UpdateForNewDay()
        {
            IsExhausted = false;
        }

        internal bool IsFallCrop(int crop)
        {
            if (Enum.IsDefined(typeof(SDVCrops), crop))
                return true;
            else
                return false;
        }

        internal double CheckCropTolerance(int currentCrop)
        {
            return CropTemps[(SDVCrops)currentCrop];
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
            if (Game1.currentLocation.IsOutdoors && Game1.isLightning)
            {
                double diceChance = Dice.NextDouble();
                if (Config.TooMuchInfo)
                    Logger.Log($"The chance of exhaustion is: {diceChance} with the configured chance of {Config.DiseaseChance}");

                if (diceChance < Config.DiseaseChance)
                {
                    IsExhausted = true;
                    InternalUtility.ShowMessage("The storm has caused you to get a cold!");
                }
            }

            //disease code.
            if (IsExhausted)
            {
                Game1.player.stamina = Game1.player.stamina - Config.StaminaPenalty;
            }

            //alert code - 30% chance of appearing
            // configured to properly appear now
            if (IsExhausted && Dice.NextDouble() < 0.3)
            {
                InternalUtility.ShowMessage("You have a cold, and feel worn out!");
            }          
        }

        public List<Vector2> ProcessHeatwave(Farm f, FerngillWeather CurrWeather)
        {
            var ThreatenedCrops = new List<Vector2>();
            int count = 0;

            if (f != null)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (count >= 15)
                        return ThreatenedCrops;

                    if (tf.Value is HoeDirt curr)
                    {
                        if (Dice.NextDouble() > .65)
                        {
                            if (CurrWeather.GetTodayHigh() >= Config.DeathTemp)
                            {
                                ThreatenedCrops.Add(tf.Key);
                                curr.state = 0;
                                count++;
                            }
                        }
                    }
                }
            }

            return ThreatenedCrops;
        }

        public void WiltHeatwave(List<Vector2> UnluckyCrops)
        {
            //if it's still de watered - kill it.
            Farm f = Game1.getFarm();
            bool cDead = false;

            foreach (Vector2 v in UnluckyCrops)
            {
                HoeDirt hd = (HoeDirt)f.terrainFeatures[v];
                if (hd.state == 0)
                {
                    hd.crop.dead = true;
                    cDead = true;
                }
            }

            if (cDead)
                InternalUtility.ShowMessage("Some of the crops have died due to lack of water!");
        }

        public void EarlyFrost(FerngillWeather currWeather)
        {
            //If it's not cold enough or not fall (or potentially spring later), return.
            if (currWeather.GetTodayLow() > 2 && Game1.currentSeason != "fall")
                return;
            
            //iterate through the farm for crops
            Farm f = Game1.getFarm();
            bool cropsKilled = false;

            if (f != null)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (tf.Value is HoeDirt curr && curr.crop != null && IsFallCrop(curr.crop.indexOfHarvest)) 
                    {
                        if (currWeather.GetTodayLow() <= CheckCropTolerance(curr.crop.indexOfHarvest) && 
                            Dice.NextDouble() < Config.FrostHardiness)
                        {
                            cropsKilled = true;
                            curr.crop.dead = true;
                        }
                    }
                }
            }

            if (cropsKilled)
            {
                InternalUtility.ShowMessage("During the night, some crops died to the frost...");
                if (Config.TooMuchInfo)
                    Logger.Log("Setting frost test via queued message");
            }
        }

     
    }
}
