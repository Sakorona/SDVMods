using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using xTile.ObjectModel;
using xTile.Tiles;
using SObject = StardewValley.Object;

namespace USDVP
{
    class GameLocationFixes
    {
        public virtual void DayUpdate(GameLocation __instance, int dayOfMonth)
        {
            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
            __instance.temporarySprites.Clear();
            List<Vector2> ObjectsOutsideBounds = new List<Vector2>();
            foreach(KeyValuePair<Vector2, TerrainFeature> kvp in __instance.terrainFeatures)
            {
                if (!__instance.isTileOnMap(kvp.Key))
                    ObjectsOutsideBounds.Add(kvp.Key);

                kvp.Value.dayUpdate(__instance, kvp.Key);
            }

            foreach (Vector2 loc in ObjectsOutsideBounds)
                __instance.terrainFeatures.Remove(loc);

            ObjectsOutsideBounds.Clear();

            if (__instance.largeTerrainFeatures != null)
            {
                foreach (LargeTerrainFeature largeTerrainFeature in __instance.largeTerrainFeatures)
                    largeTerrainFeature.dayUpdate(__instance);
            }

            foreach (SObject @object in __instance.objects.Values)
                @object.DayUpdate(__instance);

            if (!(__instance is FarmHouse))
                __instance.debris.Clear();

            if (__instance.isOutdoors)
            {
                if (Game1.dayOfMonth % 7 == 0 && !(__instance is Farm))
                {
                    foreach(KeyValuePair<Vector2, SObject> KVP in __instance.objects)
                    {
                        if (KVP.Value.isSpawnedObject)
                            ObjectsOutsideBounds.Add(KVP.Key);
                    }

                    foreach (Vector2 loc in ObjectsOutsideBounds)
                        __instance.objects.Remove(loc);

                    ObjectsOutsideBounds.Clear();
                    __instance.numberOfSpawnedObjectsOnMap = 0;
                    __instance.spawnObjects();
                    __instance.spawnObjects();
                }

                __instance.spawnObjects();
                if (Game1.dayOfMonth == 1)
                    __instance.spawnObjects();

                if (Game1.stats.DaysPlayed < 4U)
                    __instance.spawnObjects();

                bool flag = false;
                foreach (Component layer in __instance.map.Layers)
                {
                    if (layer.Id.Equals("Paths"))
                    {
                        flag = true;
                        break;
                    }
                }

                //controls seed replacement outside the farm.
                if (flag && !(__instance is Farm))
                {
                    for (int index1 = 0; index1 < __instance.map.Layers[0].LayerWidth; ++index1)
                    {
                        for (int index2 = 0; index2 < __instance.map.Layers[0].LayerHeight; ++index2)
                        {
                            if (__instance.map.GetLayer("Paths").Tiles[index1, index2] != null && Game1.random.NextDouble() < 0.5)
                            {
                                Vector2 key = new Vector2(index1, index2);
                                int which = -1;
                                switch (__instance.map.GetLayer("Paths").Tiles[index1, index2].TileIndex)
                                {
                                    case 9:
                                        which = 1;
                                        if (Game1.currentSeason.Equals("winter"))
                                        {
                                            which += 3;
                                            break;
                                        }
                                        break;
                                    case 10:
                                        which = 2;
                                        if (Game1.currentSeason.Equals("winter"))
                                        {
                                            which += 3;
                                            break;
                                        }
                                        break;
                                    case 11:
                                        which = 3;
                                        break;
                                    case 12:
                                        which = 6;
                                        break;
                                }

                                if (which != -1 && !__instance.terrainFeatures.ContainsKey(key) && !__instance.objects.ContainsKey(key))
                                    __instance.terrainFeatures.Add(key, (TerrainFeature)new Tree(which, 2));
                            }
                        }
                    }
                }
            }

            if (!(__instance is Farm))
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> KVP in __instance.terrainFeatures)
                {
                    if (KVP.Value is HoeDirt curr && (curr.crop == null || curr.crop.forageCrop))
                        ObjectsOutsideBounds.Add(KVP.Key);
                }

                foreach (Vector2 loc in ObjectsOutsideBounds)
                    __instance.terrainFeatures.Remove(loc);

                ObjectsOutsideBounds.Clear();
            }
            
           // __instance.lightLevel = 0.0f; //??

            if (__instance.name.Equals("BugLand"))
            {
                for (int index1 = 0; index1 < __instance.map.Layers[0].LayerWidth; ++index1)
                {
                    for (int index2 = 0; index2 < __instance.map.Layers[0].LayerHeight; ++index2)
                    {
                        if (Game1.random.NextDouble() < 0.33)
                        {
                            Tile tile = __instance.map.GetLayer("Paths").Tiles[index1, index2];
                            if (tile != null)
                            {
                                Vector2 vector2 = new Vector2((float)index1, (float)index2);
                                switch (tile.TileIndex)
                                {
                                    case 13:
                                    case 14:
                                    case 15:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new SObject(vector2, GameLocation.getWeedForSeason(Game1.random, "spring"), 1));
                                            continue;
                                        }
                                        continue;
                                    case 16:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new SObject(vector2, Game1.random.NextDouble() < 0.5 ? 343 : 450, 1));
                                            continue;
                                        }
                                        continue;
                                    case 17:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new SObject(vector2, Game1.random.NextDouble() < 0.5 ? 343 : 450, 1));
                                            continue;
                                        }
                                        continue;
                                    case 18:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new SObject(vector2, Game1.random.NextDouble() < 0.5 ? 294 : 295, 1));
                                            continue;
                                        }
                                        continue;
                                    case 28:
                                        if (__instance.isTileLocationTotallyClearAndPlaceable(vector2) && __instance.characters.Count < 50)
                                        {
                                            __instance.characters.Add((NPC)new Grub(new Vector2(vector2.X * Game1.tileSize, vector2.Y * Game1.tileSize), true));
                                            continue;
                                        }
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                    }
                }
            }
            __instance.addLightGlows();
        }
    }
}
