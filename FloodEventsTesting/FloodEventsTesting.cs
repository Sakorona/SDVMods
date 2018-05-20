using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;


namespace FloodEventsTesting
{
    public class FloodEventsTesting : Mod
    {
        public static Dictionary<int, List<Point>> GenerateFloodMap(GameLocation location)
        {
            // Variables
            int[,] map = new int[location.map.Layers[0].LayerWidth, location.map.Layers[0].LayerHeight];
            int mx = location.map.Layers[0].LayerWidth, my = location.map.Layers[0].LayerHeight;
            // Recursive flood method
            void PopulateFromOrigin(int x, int y, int level = 0)
            {
                if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1) && map[x, y] > level)
                {
                    map[x, y] = level;
                    for (int ox = -1; ox < 2; ox++)
                    for (int oy = -1; oy < 2; oy++)
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
            // convert map
            var dict = new Dictionary<int, List<Point>>();
            for (int x = 0; x < mx; x++)
            for (int y = 0; y < my; y++)
            {
                int height = map[x, y];
                if (!dict.ContainsKey(height))
                    dict.Add(height, new List<Point>());
                dict[height].Add(new Point(x, y));
            }
            // return converted map
            return dict;
        }

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>
        public override void Entry(IModHelper helper)
        {
            //do something here, I suppose.
            //TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            Farm f = Game1.getFarm();
            var floodedTiles = GenerateFloodMap(f);
            var floodedTilesDist1 = floodedTiles[1];
            for (int i = 0; i < f.waterTiles.GetLength(0); i++)
            {
                for (int j = 0; j < f.waterTiles.GetLength(1); j++)
                {
                    foreach (var ft in floodedTilesDist1)
                        if (ft.X == i && ft.Y == j)
                        {
                            f.waterTiles[i, j] = true;
                        }
                }
            }
          
        }

    }
}
