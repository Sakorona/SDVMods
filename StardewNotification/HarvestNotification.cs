using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewModdingAPI;

namespace StardewNotification
{
    public class HarvestNotification
    {
        private ITranslationHelper Trans;

        public HarvestNotification(ITranslationHelper Helper)
        {
            Trans = Helper;
        }

        private const int MUSHROOM_CAVE = 2;
        private const int FRUIT_CAVE = 1;

        public void CheckHarvestsAroundFarm()
        {
            CheckSeasonalHarvests();
        }

        public void CheckHarvestsOnFarm()
        {
            CheckFarmCaveHarvests(Game1.getLocationFromName("FarmCave"));
            CheckGreenhouseCrops(Game1.getLocationFromName("Greenhouse"));
        }

        public void CheckFarmCaveHarvests(GameLocation farmcave)
        {
            if (!StardewNotification.Config.NotifyFarmCave) return;
            if (Game1.player.caveChoice.Value == MUSHROOM_CAVE)
            {
                var numReadyForHarvest = 0;
                foreach (var pair in farmcave.Objects.Pairs)
                {
                    if (pair.Value.readyForHarvest.Value) numReadyForHarvest++;
                }
                if (numReadyForHarvest > 0)
                    Util.ShowFarmCaveMessage(farmcave, Trans);
            }

            else if (Game1.player.caveChoice.Value == FRUIT_CAVE && farmcave.Objects.Any())
            {
                int count = 0;

                foreach (StardewValley.Object o in farmcave.Objects.Values)
                {
                    if (!o.bigCraftable.Value)
                        count++;
                }

                if (count > 0)
                    Util.ShowFarmCaveMessage(farmcave, Trans);
            }
        }

        public void CheckSeasonalHarvests()
        {
            if (!StardewNotification.Config.NotifySeasonalForage) return;
            string seasonal = null;
            var dayOfMonth = Game1.dayOfMonth;
            switch (Game1.currentSeason)
            {
                case "spring":
                    if (dayOfMonth > 14 && dayOfMonth < 19) seasonal = Trans.Get("Salmonberry");
                    break;
                case "summer":
                    if (dayOfMonth > 11 && dayOfMonth < 15) seasonal = Trans.Get("Seashells");
                    break;
                case "fall":
                    if (dayOfMonth > 7 && dayOfMonth < 12) seasonal = Trans.Get("Blackberry");
                    break;
                default:
                    break;
            }
            if (!(seasonal is null))
            {
                Util.ShowMessage($"{seasonal}");
            }
        }

        public void CheckGreenhouseCrops(GameLocation greenhouse)
        {
            if (!StardewNotification.Config.NotifyGreenhouseCrops) return;
            //var counter = new Dictionary<string, Pair<StardewValley.TerrainFeatures.HoeDirt, int>>();
            foreach (var pair in greenhouse.terrainFeatures.Pairs)
            {
                if (pair.Value is StardewValley.TerrainFeatures.HoeDirt hoeDirt)
                {
                    if (!hoeDirt.readyForHarvest()) continue;
                    Util.ShowMessage(Trans.Get("greenhouse_crops"));
                    break;
                }
            }
        }
    }
}
