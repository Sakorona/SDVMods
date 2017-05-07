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
        private List<Vector2> ThreatenedCrops { get; set; }
        private SDVTime DeathTime { get; set; }
        private static Dictionary<SDVCrops, double> CropTemps { get; set; }

        internal HazardousWeatherEvents(IMonitor modlogger, ClimateConfig modconfig, MersenneTwister moddice)
        {
            Logger = modlogger;
            Config = modconfig;
            Dice = moddice;
            ThreatenedCrops = new List<Vector2>();

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
            ThreatenedCrops.Clear(); //purge the list
        }

        internal double CheckCropTolerance(int currentCrop)
        {
            if (CropTemps.ContainsKey((SDVCrops)currentCrop))
                return CropTemps[(SDVCrops)currentCrop];
            else
                return -100;
        }

        public void ProcessHeatwave(Farm f)
        {
            int count = 0;

            if (f != null)
            {
                if (Config.AllowCropHeatDeath)
                    DeathTime = new SDVTime(Game1.timeOfDay) + 180;

                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (count >= Config.WiltLimit)
                        break;

                    if (tf.Value is HoeDirt curr)
                    {
                        if (Dice.NextDouble() <= Config.ChanceOfWilting)
                        {
                                ThreatenedCrops.Add(tf.Key);
                                curr.state = 0;
                                count++;
                        }
                    }
                }

                if (ThreatenedCrops.Count > 0)
                {
                    if (!Config.AllowCropHeatDeath)
                        InternalUtility.ShowMessage("The extreme heat has caused some of your crops to become dry....!");
                    else
                    {
                        InternalUtility.ShowMessage("The extreme heat has caused some of your crops to dry out. If you don't water them, they'll die!");
                    }
                }
            }
        }

        public void WiltHeatwave()
        {
            //if it's still de watered - kill it.
            Farm f = Game1.getFarm();
            bool cDead = false;

            foreach (Vector2 v in ThreatenedCrops)
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
            if (Config.TooMuchInfo)
                Logger.Log("Invoking Frost.", LogLevel.Trace);

            //If it's not cold enough or not fall (or potentially spring later), return.
            if (currWeather.GetTodayLow() > 1.8 && (Game1.currentSeason == "fall" || Game1.currentSeason == "spring"))
                return;

            //iterate through the farm for crops
            Farm f = Game1.getFarm();
            bool cropsKilled = false;

            if (Game1.currentSeason == "spring" && (Game1.year > 1 || Config.DangerousFrost))
            {
                if (Config.TooMuchInfo)
                    Logger.Log("Invoking Frost - Spring Version.", LogLevel.Trace);

                //spring frosts operate differnetly
                if (f != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                    {
                        if (tf.Value is HoeDirt curr && curr.crop != null && curr.crop.currentPhase < 2)
                        {
                                cropsKilled = true;
                                curr.crop.dead = true;
                        }
                    }
                }
            }
            else
            {
                if (Config.TooMuchInfo)
                    Logger.Log("Invoking Frost - Fall Version.", LogLevel.Trace);

                if (f != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                    {
                        if (tf.Value is HoeDirt curr && curr.crop != null)
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
            }
            if (cropsKilled)
                InternalUtility.ShowMessage("During the night, some crops died to the frost...");
        }

        internal void CheckForHazardousWeather(int time, double temp)
        {
            //heatwave event
            if (time == 1700)
            {
                //the heatwave can't happen if it's a festval day, and if it's rainy or lightening.
                if (temp > Config.HeatwaveWarning &&
                    !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && (!Game1.isRaining || !Game1.isLightning))
                {
                    ProcessHeatwave(Game1.getFarm());
                }
            }

            //killer heatwave crop death time
            if (time == DeathTime.ReturnIntTime() && Config.AllowCropHeatDeath)
            {
                WiltHeatwave();
            }
        }
    }
}
