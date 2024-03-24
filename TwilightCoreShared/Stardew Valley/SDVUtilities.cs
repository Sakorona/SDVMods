using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Linq;
using System.Collections.Generic;
using TwilightShards.Common;
using StardewModdingAPI;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System.Xml.Linq;
using xTile.Dimensions;
using StardewValley.GameData.Crops;

namespace TwilightShards.Stardew.Common
{
    public static class SDVUtilities
    {
        public static string PrintStringArray(string[] array)
        {
            string s = "";
            for (int i = 0; i < array.Length; i++)
            {
                s += $"Command {i} is {array[i]}";
            }

            return s;
        }

        public static bool TileIsClearForSpawning(GameLocation checkLoc, Vector2 tileVector, StardewValley.Object tile)
        {
            if (tile == null && checkLoc.doesTileHaveProperty((int)tileVector.X, (int)tileVector.Y, "Diggable", "Back") != null && checkLoc.isTileLocationOpen(new Location((int)tileVector.X * Game1.tileSize, (int)tileVector.Y * Game1.tileSize)) && !checkLoc.IsTileOccupiedBy(tileVector) && checkLoc.doesTileHaveProperty((int)tileVector.X, (int)tileVector.Y, "Water", "Back") == null)
            {
                string PropCheck = checkLoc.doesTileHaveProperty((int)tileVector.X, (int)tileVector.Y, "NoSpawn", "Back");

                if (PropCheck == null || !PropCheck.Equals("Grass") && !PropCheck.Equals("All") && !PropCheck.Equals("True"))
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetFestivalName(int dayOfMonth, string currentSeason)
        {
            switch (currentSeason)
            {
                case ("spring"):
                    if (dayOfMonth == 13) return "Egg Festival";
                    if (dayOfMonth == 24) return "Flower Dance";
                    break;
                case ("winter"):
                    if (dayOfMonth == 8) return "Festival of Ice";
                    if (dayOfMonth == 25) return "Feast of the Winter Star";
                    if (dayOfMonth == 15) return "Night Festival";
                    if (dayOfMonth == 16) return "Night Festival";
                    if (dayOfMonth == 17) return "Night Festival";
                    break;
                case ("fall"):
                    if (dayOfMonth == 16) return "Stardew Valley Fair";
                    if (dayOfMonth == 27) return "Spirit's Eve";
                    break;
                case ("summer"):
                    if (dayOfMonth == 11) return "Luau";
                    if (dayOfMonth == 28) return "Dance of the Moonlight Jellies";
                    break;
                default:
                    return $"";
            }
            return $"";
        }

        public static Vector2 SpawnRandomMonster(GameLocation location)
        {
            for (int index = 0; index < 15; ++index)
            {
                Vector2 randomTile = location.getRandomTile();
                if (Utility.isOnScreen(Utility.Vector2ToPoint(randomTile), 64, location))
                    randomTile.X -= Game1.viewport.Width / 64;
                if (location.isTileLocationOpen(randomTile))
                {
                    if (Game1.player.CombatLevel >= 10 && Game1.MasterPlayer.deepestMineLevel >= 145 && Game1.random.NextDouble() <= .001 && Game1.MasterPlayer.stats.getMonstersKilled("Pepper Rex") > 0)
                    {
                        DinoMonster squidKid = new(randomTile * Game1.tileSize)
                        {
                            focusedOnFarmers = true
                        };
                        location.characters.Add(squidKid);

                        return (randomTile);
                    }

                    else if (Game1.player.CombatLevel >= 10 && Game1.MasterPlayer.deepestMineLevel >= 145 && Game1.random.NextDouble() <= .05)
                    {
                        MetalHead squidKid = new(randomTile * Game1.tileSize, 145)
                        {
                            focusedOnFarmers = true
                        };
                        location.characters.Add(squidKid);

                        return (randomTile);
                    }

                    else if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() <= .25)
                    {
                        Skeleton skeleton = new(randomTile * Game1.tileSize)
                        {
                            focusedOnFarmers = true
                        };
                        location.characters.Add(skeleton);

                        return (randomTile);
                    }

                    else if (Game1.player.CombatLevel >= 8 && Game1.random.NextDouble() <= 0.15)
                    {
                        ShadowBrute shadowBrute = new(randomTile * Game1.tileSize)
                        {
                            focusedOnFarmers = true
                        };
                        location.characters.Add(shadowBrute);

                        return (randomTile);
                    }
                    else if (Game1.random.NextDouble() < 0.65 && location.isTileLocationOpen(randomTile))
                    {
                        RockGolem rockGolem = new(randomTile * Game1.tileSize, Game1.player.CombatLevel)
                        {
                            focusedOnFarmers = true
                        };
                        rockGolem.Sprite.currentFrame = 16;
                        rockGolem.Sprite.loop = false;
                        rockGolem.Sprite.UpdateSourceRect();
                        location.characters.Add(rockGolem);
                        return (randomTile);
                    }
                    else
                    {
                        int mineLevel;
                        if (Game1.player.CombatLevel > 1 && Game1.player.CombatLevel <= 4)
                            mineLevel = 41;
                        else if (Game1.player.CombatLevel > 4 && Game1.player.CombatLevel <= 8)
                            mineLevel = 100;
                        else if (Game1.player.CombatLevel > 8 && Game1.player.CombatLevel <= 10)
                            mineLevel = 140;
                        else
                            mineLevel = 200;

                        GreenSlime greenSlime = new(randomTile * Game1.tileSize, mineLevel)
                        {
                            focusedOnFarmers = true,
                        };
                        greenSlime.color.Value = Color.IndianRed;
                        location.characters.Add(greenSlime);

                        return (randomTile);
                    }
                }
            }

            return Vector2.Zero;
        }


        public static T GetModApi<T>(IMonitor Monitor, IModHelper Helper, string name, string minVersion, string friendlyName="") where T : class
        {
            var modManifest = Helper.ModRegistry.Get(name);
            if (modManifest != null)
            {
                if (!modManifest.Manifest.Version.IsOlderThan(minVersion))
                {
                    T api = Helper.ModRegistry.GetApi<T>(name);
                    if (api == null)
                    {
                        Monitor.Log($"{(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)}'s API returned null. ", LogLevel.Info);
                    }

                    if (api != null)
                    {
                        Monitor.Log($"{(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)} {modManifest.Manifest.Version} integration feature enabled", LogLevel.Info);
                    }
                    return api;

                }
                else
                    Monitor.Log($"{(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)} detected, but not of a sufficient version. Req:{minVersion} Detected:{modManifest.Manifest.Version}. Update the other mod if you want to use the integration feature. Skipping..", LogLevel.Debug);
            }
            else
                Monitor.Log($"Didn't find mod {(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)}; you can optionally install it for extra features!", LogLevel.Debug);
            return null;
        }

        public static void ShowMessage(string msg, int whatType)
        {
            var hudmsg = new HUDMessage(msg, whatType);
            Game1.addHUDMessage(hudmsg);
        }

            /// <summary>
        /// This function prints crop data in detail.
        /// </summary>
        /// <param name="loc">Location of the crop</param>
        /// <param name="h">Hoe Dirt of the crop</param>
        /// <param name="position">The position of the crop</param>
        /// <returns>A string description of the crop</returns>
        public static string PrintCropData(GameLocation loc, HoeDirt h, Vector2 position)
        {
            Crop currentCrop = h.crop;
            CropData cropInfo = currentCrop.GetData();
            string cropName = Game1.objectData[cropInfo.HarvestItemId].DisplayName;
            string desc = "";

            if (currentCrop is null)
                return $"This is not a crop at {loc} and {position}";

            //describe crop data.
            desc += $"This crop has harvest index: {currentCrop.indexOfHarvest.Value} ({cropName}).{Environment.NewLine} Current Phase: {currentCrop.currentPhase}, phase calendar: {currentCrop.phaseDays}, final phase: {(currentCrop.phaseDays.Count)-1}";

            desc +=
                $"{Environment.NewLine} Day of Current Phase: {currentCrop.dayOfCurrentPhase}, Valid Seasons: {cropInfo.Seasons}. Regrowth after Harvest: {currentCrop.RegrowsAfterHarvest}";

            return desc;
        }

        /// <summary>
        /// This function regresses crops
        /// </summary>
        /// <param name="loc">Location of the crop</param>
        /// <param name="h">Hoe Dirt of the crop</param>
        /// <param name="position">The position of the crop</param>
        /// <param name="numSteps">The number of steps the crop is being regressed by</param>
        /// <param name="giantCropRequiredSteps">The number of minimum steps to affect the giant crop (default: 4)</param>
        /// <param name="giantCropDestructionOdds">The chance of the giant crop being affected (default: 50%)</param>
        /// <exception cref="">Throws a generic exception if it finds a giant crop that doesn't have an actual crop backing.</exception>
        public static void DeAdvanceCrop(GameLocation loc, HoeDirt h, Vector2 position, int numSteps, IMonitor Logger, int giantCropRequiredSteps = 4, double giantCropDestructionOdds = .5)
        {
            //determine the phase of the crop
            Crop currentCrop = h.crop;

            //data on the crop. Outputting to debug.
            Logger.Log($"BEFORE CROP DEADVANCEMENT: {PrintCropData(loc, h, position)}", LogLevel.Debug);

            if (currentCrop is not null)
            {
                int countPhases = currentCrop.phaseDays.Count;
                int finalPhase = countPhases - 1;

                for (int i = 0; i < numSteps; i++)
                {
                    //now, check the phase - handle the final phase.
                    if (currentCrop.currentPhase.Value == finalPhase && currentCrop.RegrowsAfterHarvest() == false)
                    {
                        if (currentCrop.dayOfCurrentPhase.Value > 0)
                        {
                            currentCrop.dayOfCurrentPhase.Value--;
                        }
                        else if (currentCrop.dayOfCurrentPhase.Value == 0)
                        {
                            currentCrop.fullyGrown.Value = false;
                            currentCrop.currentPhase.Value--;
                            currentCrop.dayOfCurrentPhase.Value = currentCrop.phaseDays[currentCrop.currentPhase.Value];
                        }
                        Logger.Log($"AFTER CROP DEADVANCEMENT: {PrintCropData(loc, h, position)}", LogLevel.Debug);
                        continue;
                    }

                    //handle regrowth crops.
                    if (currentCrop.RegrowsAfterHarvest() == true && currentCrop.currentPhase.Value == finalPhase)
                    {
                        currentCrop.dayOfCurrentPhase.Value++;
                        Logger.Log($"AFTER CROP DEADVANCEMENT: {PrintCropData(loc, h, position)}", LogLevel.Debug);
                        continue;
                    }

                    //now handle it being any phase but 0.
                    if (currentCrop.currentPhase.Value != finalPhase || currentCrop.currentPhase.Value != 0)
                    {
                        if (currentCrop.dayOfCurrentPhase.Value >= currentCrop.phaseDays[currentCrop.currentPhase.Value] && currentCrop.currentPhase.Value > 0)
                        {
                            currentCrop.currentPhase.Value--;
                            currentCrop.dayOfCurrentPhase.Value = currentCrop.phaseDays[currentCrop.currentPhase.Value];
                        }
                        else
                        {
                            currentCrop.dayOfCurrentPhase.Value++;
                        }
                        Logger.Log($"AFTER CROP DEADVANCEMENT: {PrintCropData(loc, h, position)}", LogLevel.Debug);
                        continue;
                    }

                    //final check. Phase 0.
                    if (currentCrop.currentPhase.Value == 0)
                    {
                        if (currentCrop.dayOfCurrentPhase.Value != 0 && currentCrop.dayOfCurrentPhase.Value > 0)
                        {
                            currentCrop.dayOfCurrentPhase.Value--;
                        }
                        Logger.Log($"AFTER CROP DEADVANCEMENT: {PrintCropData(loc, h, position)}", LogLevel.Debug);
                        continue;
                    }

                    //Sanity check here.
                    if (currentCrop.currentPhase.Value < 0)
                    {
                        currentCrop.currentPhase.Value = 0;
                    }
                }
            }

            //check for giant crop.
            if (loc is Farm f)
            {
                foreach (ResourceClump rc in f.resourceClumps)
                {
                    if (rc is GiantCrop gc && CheckIfPositionIsWithinGiantCrop(position, gc))
                    {
                        //This breaks my heart, given the requirements...
                        if (numSteps > giantCropRequiredSteps && Game1.random.NextDouble() < giantCropDestructionOdds)
                        {
                            numSteps -= giantCropRequiredSteps;
                            Vector2 upperLeft = gc.Tile;
                            string cropReplacement = gc.Id;
                            
                            int width = gc.width.Value, height = gc.height.Value;

                            string cropSeed = gc.GetData().FromItemId ?? throw new Exception($"Somehow, this giant crop has no valid seed from it's stored parent index. This needs to be troubleshooted. Parent seed index is {cropReplacement}");

                            f.resourceClumps.Remove(gc);
                            for (int i = 0; i < width; i++)
                            {
                                for (int j = 0; j < height; j++)
                                {
                                    Vector2 currPos = new(upperLeft.X + i, upperLeft.Y + i);
                                    HoeDirt hd = new(1)
                                    {
                                        crop = new Crop(cropSeed, (int)currPos.X, (int)currPos.Y, loc)
                                    };
                                    hd.crop.growCompletely();
                                    loc.terrainFeatures.Add(currPos, hd);
                                }
                            }
                        }
                    }
                }
            }
            Logger.Log($"AFTER CROP DEADVANCEMENT: {PrintCropData(loc, h, position)}", LogLevel.Debug);
            //we aren't handling forage crops here.
        }

        public static bool CheckIfPositionIsWithinGiantCrop(Vector2 position, GiantCrop g)
        {
            if (position.X >= g.Tile.X && position.X <= g.Tile.X + g.width.Value)
            {
                if (position.Y >= g.Tile.Y && position.Y < g.Tile.Y + g.height.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AdvanceCropOneStep(GameLocation loc, HoeDirt h, Vector2 position)
        {
            Crop currentCrop = h.crop;
            CropData cCropData = currentCrop.GetData();
            int xPos = (int)position.X;
            int yPos = (int)position.Y;

            if (currentCrop is null || cCropData is null)
                return;

            //due to how this will be called, we do need to some checking
            if (!loc.IsGreenhouse && (currentCrop.dead.Value || !cCropData.Seasons.Contains(Game1.season)))
            {
                currentCrop.dead.Value = true;
            }
            else
            {
                if (h.state.Value == HoeDirt.watered)
                {
                    //get the day of the current phase - if it's fully grown, we can just leave it here.
                    if (currentCrop.fullyGrown.Value)
                        currentCrop.dayOfCurrentPhase.Value -= 1;
                    else
                    {
                        //check to sere what the count of current days is

                        int phaseCount; //get the count of days in the current phase
                        if (currentCrop.phaseDays.Count > 0)
                            phaseCount = currentCrop.phaseDays[Math.Min(currentCrop.phaseDays.Count - 1, currentCrop.currentPhase.Value)];
                        else
                            phaseCount = 0;

                        currentCrop.dayOfCurrentPhase.Value = Math.Min(currentCrop.dayOfCurrentPhase.Value + 1, phaseCount);

                        //check phases
                        if (currentCrop.dayOfCurrentPhase.Value >= phaseCount && currentCrop.currentPhase.Value < currentCrop.phaseDays.Count - 1)
                        {
                            currentCrop.currentPhase.Value++;
                            currentCrop.dayOfCurrentPhase.Value = 0;
                        }

                        //skip negative day or 0 day crops.
                        while (currentCrop.currentPhase.Value < currentCrop.phaseDays.Count - 1 && currentCrop.phaseDays.Count > 0 && currentCrop.phaseDays[currentCrop.currentPhase.Value] <= 0)
                        {
                            currentCrop.currentPhase.Value++;
                        }

                        //handle wild crops
                        if (currentCrop.isWildSeedCrop() && currentCrop.phaseToShow.Value == -1 && currentCrop.currentPhase.Value > 0)
                            currentCrop.phaseToShow.Value = Game1.random.Next(1, 7);

                        //and now giant crops
                        double giantChance = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + xPos * 2000 + yPos).NextDouble();

                        if (loc is Farm && currentCrop.currentPhase.Value == currentCrop.phaseDays.Count - 1 && IsValidGiantCrop(currentCrop.indexOfHarvest.Value) &&
                            giantChance <= 0.01)
                        {
                            for (int i = xPos - 1; i <= xPos + 1; i++)
                            {
                                for (int j = yPos - 1; j <= yPos + 1; j++)
                                {
                                    Vector2 tile = new(i, j);
                                    if (!loc.terrainFeatures.ContainsKey(tile) || loc.terrainFeatures[tile] is not HoeDirt hDirt ||
                                        hDirt?.crop?.indexOfHarvest == currentCrop.indexOfHarvest)
                                    {
                                        return; //no longer needs to process.
                                    }
                                }
                            }


                            //replace for giant crops.
                            for (int i = xPos - 1; i <= xPos + 1; i++)
                            {
                                for (int j = yPos - 1; j <= yPos + 1; j++)
                                {
                                    Vector2 tile = new(i, j);
                                    if (loc.terrainFeatures[tile] is not null && loc.terrainFeatures[tile] is HoeDirt hDirt)
                                        hDirt.crop = null;
                                }
                            }

                            if (loc is Farm f)
                                f.resourceClumps.Add(new GiantCrop(currentCrop.indexOfHarvest.Value, new Vector2(xPos - 1, yPos - 1)));

                        }
                    }
                }
                //process some edge cases for non watered crops.
                if (currentCrop.fullyGrown.Value && currentCrop.dayOfCurrentPhase.Value > 0 ||
                    currentCrop.currentPhase.Value < currentCrop.phaseDays.Count - 1 ||
                    !currentCrop.isWildSeedCrop())

                    return; //stop processing

                //replace wild crops**

                //remove any object here. o.O
                loc.objects.Remove(position);

                Season season = Game1.season;
                switch (currentCrop.whichForageCrop.Value)
                {
                    case "(O)495":
                        season = Season.Spring;
                        break;
                    case "(O)496":
                        season = Season.Summer;
                        break;
                    case "(O)497":
                        season = Season.Fall;
                        break;
                    case "(O)498":
                        season = Season.Winter;
                        break;
                }
                loc.objects.Add(position, new StardewValley.Object(position, currentCrop.getRandomWildCropForSeason(season), false)
                {
                    IsSpawnedObject = true,
                    CanBeGrabbed = true
                });

                //the normal iteration has a safe-call that isn't neded here               
            }
        }

        private static bool IsValidGiantCrop(string cropID)
        {
            var GCrops = DataLoader.GiantCrops(Game1.content);
            if (GCrops.ContainsKey(cropID))
                return true;

            return false;

        }

        public static void AdvanceArbitrarySteps(GameLocation loc, HoeDirt h, Vector2 position, int numDays = 1)
        {
            for (int i = 0; i < numDays; i++)
                AdvanceCropOneStep(loc, h, position);
        }

        public static int CreateWeeds(GameLocation spawnLoc, int numOfWeeds)
        {
            if (spawnLoc == null)
                throw new Exception("The passed spawn location cannot be null!");

            int CreatedWeeds = 0;

            for (int i = 0; i <= numOfWeeds; i++)
            {
                //limit number of attempts per attempt to 10.
                int numberOfAttempts = 0;
                while (numberOfAttempts < 3)
                {
                    //get a random tile.
                    int xTile = Game1.random.Next(spawnLoc.map.DisplayWidth / Game1.tileSize);
                    int yTile = Game1.random.Next(spawnLoc.map.DisplayHeight / Game1.tileSize);
                    Vector2 randomVector = new(xTile, yTile);
                    spawnLoc.objects.TryGetValue(randomVector, out StardewValley.Object @object);

                    if (SDVUtilities.TileIsClearForSpawning(spawnLoc, randomVector, @object))
                    {
                        //for now, don't spawn in winter.
                        if (Game1.currentSeason != "winter")
                        {
                            //spawn the weed
                            spawnLoc.objects.Add(randomVector, new StardewValley.Object(randomVector, GameLocation.getWeedForSeason(Game1.random, Game1.season), false));
                            CreatedWeeds++;
                        }
                    }
                    numberOfAttempts++; // this might have been more useful INSIDE the while loop.
                }
            }
            return CreatedWeeds;
        }
    }
}
