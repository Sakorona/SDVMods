using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using TwilightCore.StardewValley;
using TwilightCore.PRNG;

namespace ClimateOfFerngill
{
    internal class HazardousWeatherEvents
    {
        private IMonitor Logger;
        private ClimateConfig Config;
        private MersenneTwister Dice;
        private List<Vector2> ThreatenedCrops { get; set; }
        private SDVTime DeathTime { get; set; }
        private static List<CropInfo> CropTemps { get; set; }

        internal HazardousWeatherEvents(List<CropInfo> data, IMonitor modlogger, ClimateConfig modconfig, MersenneTwister moddice)
        {
            Logger = modlogger;
            Config = modconfig;
            Dice = moddice;
            ThreatenedCrops = new List<Vector2>();
            CropTemps = data;           
        }

        internal void UpdateForNewDay()
        {
            ThreatenedCrops.Clear(); //purge the list
        }

        internal double CheckCropTolerance(int currentCrop)
        {
           foreach (CropInfo c in CropTemps)
            {
                if (c.ParentSheetIndex == currentCrop)
                    return c.FrostLimit;
            }

            return -100;
        }

        public void ProcessHeatwave(Farm f)
        {
            int count = 0;

            if (f != null)
            {
                if (Config.AllowCropHeatDeath)
                    DeathTime = new SDVTime(Game1.timeOfDay) + 180;

                if (Config.TooMuchInfo)
                    Logger.Log("Processing the heatwave");

                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (count >= Config.WiltLimit)
                        break;

                    if (tf.Value is HoeDirt curr && curr.crop != null)
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
                        SDVUtilities.ShowMessage("The extreme heat has caused some of your crops to become dry....!");
                    else
                    {
                        SDVUtilities.ShowMessage("The extreme heat has caused some of your crops to dry out. If you don't water them, they'll die!");
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
                if (hd.state == HoeDirt.dry)
                {
                    hd.crop.dead = true;
                    cDead = true;
                }
            }

            if (cDead)
                SDVUtilities.ShowMessage("Some of the crops have died due to lack of water!");
        }

        public void EarlyFrost(FerngillWeather currWeather)
        {
            if (Config.TooMuchInfo)
                Logger.Log("Invoking Frost.", LogLevel.Trace);

            //iterate through the farm for crops
            Farm f = Game1.getFarm();
            bool cropsKilled = false;

            if (Game1.currentSeason == "spring" && (Game1.year > 1 || Config.DangerousFrost))
            {
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
                SDVUtilities.ShowMessage("During the night, some crops died to the frost...");
        }

        internal void CheckForHazardousWeather(int time, double temp)
        {
            //heatwave event
            if (time == 1700)
            {
                //the heatwave can't happen if it's a festval day, and if it's rainy
                //dry lightning is now a thing, and I see no reason it can't trigger a heatwave
                if (temp > Config.HeatwaveWarning &&
                    !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && 
                    !Game1.isRaining)
                {
                    ProcessHeatwave(Game1.getFarm());
                    if (Config.TooMuchInfo)
                        Logger.Log($"We dried {ThreatenedCrops.Count} crops");
                }
            }

            //killer heatwave crop death time
            if (time == DeathTime.ReturnIntTime() && Config.AllowCropHeatDeath)
            {
                WiltHeatwave();
                if (Config.TooMuchInfo)
                    Logger.Log($"We killed {ThreatenedCrops.Count} crops");
            }
        }
    }
}
