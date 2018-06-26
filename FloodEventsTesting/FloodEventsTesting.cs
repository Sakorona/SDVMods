using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace FloodEventsTesting
{
    public class FloodEventsTesting : Mod
    {
        protected Dictionary<GameLocation,Dictionary<int,List<Point>>> FloodedTiles;
        protected int CurrentFloodDepth;
        protected int TotalFloodDepth = 4;

        public static List<Point> GenerateFloodMap(GameLocation location, int depth, Func<GameLocation, int, int, bool> IsBlockedTile=null)
        {
            // Variables
            int[,] map = new int[location.map.Layers[0].LayerWidth, location.map.Layers[0].LayerHeight];
            int mx = location.map.Layers[0].LayerWidth, my = location.map.Layers[0].LayerHeight;
            // Recursive flood method
            void PopulateFromOrigin(int x, int y, int level = 0)
            {
                if (level > 0 && map[x, y] == int.MaxValue && (level > depth || IsBlockedTile?.Invoke(location, x, y) == true))
                    map[x, y] = -1;
                else
                {
                    map[x, y] = level;
                    for (int ox = -1; ox < 2; ox++)
                    for (int oy = -1; oy < 2; oy++)
                        if (ox >= 0 && ox < mx && oy >= 0 && oy < my && map[x, y] > level)
                            PopulateFromOrigin(x + ox, y + oy, level + 1);
                }
            }
            // Clear map
            for (int x = 0; x < mx; x++)
            for (int y = 0; y < my; y++)
                map[x, y] = int.MaxValue;
            // Populate map
            for (int x = 0; x < mx; x++)
            for (int y = 0; y < my; y++)
                if (location.waterTiles[x, y])
                    PopulateFromOrigin(x, y, 0);
            // copy flooded tiles
            var output = new List<Point>();
            for (int x = 0; x < mx; x++)
            for (int y = 0; y < my; y++)
            {
                int height = map[x, y];
                if (height < 0 || height == int.MaxValue)
                    continue;
                output.Add(new Point(x, y));
            }
            // return flooded tiles
            return output;
        }

        public bool StopTheWater(GameLocation loc, int x, int y)
        {
            if (loc.objects.ContainsKey(new Vector2(x,y)) && loc.objects[new Vector2(x,y)] is Fence sFence && (sFence.whichType.Value != Fence.wood || sFence.whichType.Value != Fence.steel))
            { 
                return true;
            }
            return false;
        }

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>
        public override void Entry(IModHelper helper)
        {
            FloodedTiles = new Dictionary<GameLocation, Dictionary<int, List<Point>>>();
            CurrentFloodDepth = TotalFloodDepth;
            //do something here, I suppose.
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (CurrentFloodDepth <= 0)
                return;

            //recede the water, spawn junk
            List<int> junkItems = new List<int>(){168,169,170,171,172,167,388,390,372,393};
            foreach (var floodTiles in FloodedTiles)
            {
                GameLocation f = floodTiles.Key;
                for (int i = 0; i < f.waterTiles.GetLength(0); i++)
                {
                    for (int j = 0; j < f.waterTiles.GetLength(1); j++)
                    {
                        foreach (var tileDepths in floodTiles.Value)
                        {
                           
                        }
                    }
                }
            }

            CurrentFloodDepth--;

        }

        private string PrintPointList(List<Point> b)
        {
            string s = "";
            foreach (var v in b)
            {
                s += $"({v.X} , {v.Y})";
            }

            return s;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            Monitor.Log("Initiating flood calculations");
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc is Farm)
                {
                    FloodedTiles.Add(loc, new Dictionary<int, List<Point>>()
                    {
                        {4,GenerateFloodMap(loc,4,StopTheWater) },
                        {3,GenerateFloodMap(loc,3,StopTheWater) },
                        {2,GenerateFloodMap(loc,2,StopTheWater) },
                        {1,GenerateFloodMap(loc,1,StopTheWater) },

                    });
                }
            }

            Monitor.Log("Printing lists..");
            foreach (var kvp in FloodedTiles)
            {
                Monitor.Log($"For {kvp.Key}, these squares:");
                foreach (var kvpB in kvp.Value)
                {
                    Monitor.Log($"Depth {kvpB.Key} has squares {PrintPointList(kvpB.Value)}");
                }
            }

            Monitor.Log("Flooding maps");
            //flood the maps
            foreach (var floodTiles in FloodedTiles)
            {
                GameLocation f = floodTiles.Key;
                for (int i = 0; i < f.waterTiles.GetLength(0); i++)
                {
                    for (int j = 0; j < f.waterTiles.GetLength(1); j++)
                    {
                        foreach (var tileDepths in floodTiles.Value)
                        {
                            Vector2 currTile = new Vector2(i,j);
                            if (tileDepths.Key == TotalFloodDepth && tileDepths.Key > 0 && tileDepths.Value.Contains(new Point(i, j)))
                            {
                                f.waterTiles[i, j] = true;
                                //destroy terrain features
                                if (f.terrainFeatures.ContainsKey(currTile) && (f.terrainFeatures[currTile] is HoeDirt || f.terrainFeatures[currTile] is Grass || (f.terrainFeatures[currTile] is Tree t && (t.growthStage.Value <= 3 || t.health.Value <= 18f)) ||(f.terrainFeatures[currTile] is FruitTree tf && (tf.growthStage.Value <= 3 || tf.health.Value <= 18f))))
                                {
                                    f.terrainFeatures.Remove(currTile);
                                }

                                //now onto objects!!!
                                if (f.objects.ContainsKey(currTile) &&
                                    f.objects[currTile] is StardewValley.Object fObj && (fObj.bigCraftable.Value || fObj.IsSpawnedObject) && !(fObj.canBePlacedInWater()) && !(fObj is Fence ff && (ff.whichType.Value != 2 || ff.whichType.Value != 5)))
                                {
                                    f.objects.Remove(currTile);
                                }
                            }
                        }
                    }
                }
            }
            Monitor.Log("Flooding complete");
        }
    }
}
