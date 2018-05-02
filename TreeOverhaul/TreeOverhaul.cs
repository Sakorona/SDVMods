using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

//tree types (default names)
//
//bushyTree = 1;
//leafyTree = 2;
//pineTree = 3;
//winterTree1 = 4;
//winterTree2 = 5;
//palmTree = 6;
//mushroomTree = 7;

namespace TreeOverhaul
{
    public class TreeOverhaul : Mod
    {
        public TreeOverhaulConfig treeOverhaulConfig;

        public override void Entry(IModHelper helper)
        {
            TimeEvents.AfterDayStarted += Events_NewDay;
            treeOverhaulConfig = helper.ReadConfig<TreeOverhaulConfig>();
            this.Monitor.Log(GetType().Name + " has loaded", LogLevel.Trace);
        }

        public void Events_NewDay(object sender, EventArgs e)
        {
            foreach (var location in Game1.locations)
            {
                foreach (var terrainfeature in location.terrainFeatures.Pairs)
                {
                    if (terrainfeature.Value is Tree tree)
                    {
                        CheckTree(tree, location, terrainfeature.Key);
                        StopSapling(tree, location, terrainfeature.Key);
                    }
                    if (terrainfeature.Value is FruitTree fruittree)
                    {
                        CheckFruitTree(fruittree, location, terrainfeature.Key);
                    }
                }
            }
        }

        public void StopSapling(Tree tree, GameLocation location, Vector2 tileLocation)
        {
            if (treeOverhaulConfig.StopShadeSaplingGrowth)
            {
                if (tree.growthStage.Value == 1)
                {
                    Rectangle rectangle = new Rectangle((int)(((double)tileLocation.X - 1.0) * (double)Game1.tileSize), (int)(((double)tileLocation.Y - 1.0) * (double)Game1.tileSize), Game1.tileSize * 3, Game1.tileSize * 3);

                    foreach (KeyValuePair<Vector2, TerrainFeature> keyValuePair in (Dictionary<Vector2, TerrainFeature>)location.terrainFeatures.Pairs)
                    {
                        if (keyValuePair.Value is Tree && !keyValuePair.Value.Equals((object)this) && ((Tree)keyValuePair.Value).growthStage.Value >= 5 && keyValuePair.Value.getBoundingBox(keyValuePair.Key).Intersects(rectangle))
                        {
                            tree.growthStage.Value = 0;
                            return;
                        }
                    }
                }
            }
        }

        public void CheckTree(Tree tree, GameLocation location, Vector2 tileLocation)
        {
            if (Game1.currentSeason.Equals("winter"))
            {
                if (tree.treeType.Value != 6 || location.Name.ToLower().Contains("greenhouse"))
                {
                    if (tree.treeType.Value != 7 && treeOverhaulConfig.NormalTreesGrowInWinter)
                    {
                        GrowTree(tree, location, tileLocation);
                    }
                    if (tree.treeType.Value == 7 && treeOverhaulConfig.MushroomTreesGrowInWinter)
                    {
                        FixMushroomStump(tree, location, tileLocation);
                        GrowTree(tree, location, tileLocation);
                    }
                }
            }

            if (treeOverhaulConfig.FasterNormalTreeGrowth)
            {

                if (Game1.currentSeason.Equals("winter"))
                {
                    if (tree.treeType.Value != 6 || location.Name.ToLower().Contains("greenhouse"))
                    {
                        if (tree.treeType.Value != 7 && treeOverhaulConfig.NormalTreesGrowInWinter)
                        {
                            GrowTree(tree, location, tileLocation);
                        }
                        if (tree.treeType.Value == 7 && treeOverhaulConfig.MushroomTreesGrowInWinter)
                        {
                            FixMushroomStump(tree, location, tileLocation);
                            GrowTree(tree, location, tileLocation);
                        }
                    }
                }
                else
                {
                    if (tree.treeType.Value != 6 || location.Name.ToLower().Contains("greenhouse"))
                    {
                        if (tree.treeType.Value != 7)
                        {
                            GrowTree(tree, location, tileLocation);
                        }
                        if (tree.treeType.Value == 7)
                        {
                            FixMushroomStump(tree, location, tileLocation);
                            GrowTree(tree, location, tileLocation);
                        }
                    }
                }
            }
        }

        public void GrowTree(Tree tree, GameLocation location, Vector2 tileLocation)
        {
            Rectangle value = new Rectangle((int)((tileLocation.X - 1f) * (float)Game1.tileSize), (int)((tileLocation.Y - 1f) * (float)Game1.tileSize), Game1.tileSize * 3, Game1.tileSize * 3);
            string text = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "NoSpawn", "Back");
            if (text != null && (text.Equals("All") || text.Equals("Tree")))
            {
                return;
            }

            if (tree.growthStage.Value == 4)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
                {
                    if (pair.Value is Tree && !pair.Value.Equals(this) && ((Tree)pair.Value).growthStage.Value >= 5 && pair.Value.getBoundingBox(pair.Key).Intersects(value))
                    {
                        return;
                    }
                }
                if (Game1.random.NextDouble() < 0.2)
                {
                    tree.growthStage.Value++;
                }
            }

            if (tree.growthStage.Value != 0 || !location.objects.ContainsKey(tileLocation))
            {
                if (Game1.random.NextDouble() < 0.2)
                {
                    tree.growthStage.Value++;
                }
            }
        }

        public void FixMushroomStump(Tree tree, GameLocation location, Vector2 tileLocation)
        {
            if (Game1.currentSeason.Equals("winter"))
            {
                if (tree.stump.Value == true)
                {
                    tree.stump.Value = false;
                    tree.health.Value = 10f;
                }
            }
        }

        public void CheckFruitTree(FruitTree fruittree, GameLocation location, Vector2 tileLocation)
        {
            if (treeOverhaulConfig.FruitTreesDontGrowInWinter)
            {
                if (Game1.currentSeason.Equals("winter"))
                {
                    if (location.Name.ToLower().Contains("greenhouse"))
                    {
                        CheckGrowthType(fruittree, location, tileLocation);
                    }
                    else
                    {
                        GrowFruitTree(fruittree, location, tileLocation, "plus");
                    }
                }
                else
                {
                    CheckGrowthType(fruittree, location, tileLocation);
                }

            }
            else
            {
                CheckGrowthType(fruittree, location, tileLocation);
            }
        }

        public void CheckGrowthType(FruitTree fruittree, GameLocation location, Vector2 tileLocation)
        {
            if (treeOverhaulConfig.FruitTreeGrowth == 1)
            {
                GrowFruitTree(fruittree, location, tileLocation, "minus");
            }

            if (treeOverhaulConfig.FruitTreeGrowth == 2)
            {
                if (Game1.dayOfMonth % 2 == 1) //odd day
                {
                    GrowFruitTree(fruittree, location, tileLocation, "plus");
                }
            }
        }

        public void CheckGrowthStage(FruitTree fruittree, GameLocation location, Vector2 tileLocation)
        {
            if (fruittree.daysUntilMature.Value > 28)
            {
                fruittree.daysUntilMature.Value = 28;
            }

            if (fruittree.daysUntilMature.Value <= 0)
            {
                fruittree.growthStage.Value = 4;
            }
            else if (fruittree.daysUntilMature.Value <= 7)
            {
                fruittree.growthStage.Value = 3;
            }
            else if (fruittree.daysUntilMature.Value <= 14)
            {
                fruittree.growthStage.Value = 2;
            }
            else if (fruittree.daysUntilMature.Value <= 21)
            {
                fruittree.growthStage.Value = 1;
            }
            else
            {
                fruittree.growthStage.Value = 0;
            }
        }

        public void GrowFruitTree(FruitTree fruittree, GameLocation location, Vector2 tileLocation, string change)
        {
            bool flag = false;
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(tileLocation);
            for (int i = 0; i < surroundingTileLocationsArray.Length; i++)
            {
                Vector2 vector = surroundingTileLocationsArray[i];
                bool flag2 = location.terrainFeatures.ContainsKey(vector) && location.terrainFeatures[vector] is HoeDirt && (location.terrainFeatures[vector] as HoeDirt).crop == null;
                if (location.isTileOccupied(vector, "") && !flag2)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                if (fruittree.daysUntilMature.Value > 28)
                {
                    fruittree.daysUntilMature.Value = 28;
                }
                if (fruittree.daysUntilMature.Value > 0)
                {
                    if (change == "minus")
                    {
                        fruittree.daysUntilMature.Value--;
                    }
                    if (change == "plus")
                    {
                        fruittree.daysUntilMature.Value++;
                    }
                    CheckGrowthStage(fruittree, location, tileLocation);
                }           
            }
        }                
    }

    public class TreeOverhaulConfig
    {
        public bool StopShadeSaplingGrowth { get; set; } = true;
        public bool NormalTreesGrowInWinter { get; set; } = true;
        public bool MushroomTreesGrowInWinter { get; set; } = false;
        public bool FruitTreesDontGrowInWinter { get; set; } = false;
        public bool FasterNormalTreeGrowth { get; set; } = false;
        public int FruitTreeGrowth { get; set; } = 0;
    }
}