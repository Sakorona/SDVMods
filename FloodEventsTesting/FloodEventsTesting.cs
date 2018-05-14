using System;
using System.Collections.Generic;
using StardewModdingAPI;
using System.Linq;
using System.Runtime.CompilerServices;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;

namespace FloodEventsTesting
{
    /// <summary>Wrapper class for vertices which represents a node in a graph.</summary>
    /// <typeparam name="TVertex">The type of the vertex in the graph.</typeparam>
    public class GraphNode<TVertex>
    {
        public TVertex Value { get; set; }
        public int TotalCost { get; set; }

        public GraphNode(TVertex value, int totalCost)
        {
            this.Value = value;
            this.TotalCost = totalCost;
        }
    }

    public static class OurExtensions
    {
        /// <summary>Performs any number of simultaneous breadth-first traversals over a graph, returning each vertex that was reached.</summary>
        /// <typeparam name="TVertex">The type of the vertex in the graph.</typeparam>
        /// <param name="starts">All vertices that the traversals should begin at.</param>
        /// <param name="getNeighbors">A function returning all vertices the given vertex is connected to.</param>
        /// <param name="equalityComparer">Equality comparer for vertices to avoid the same vertex being checked multiple times.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing each vertex that was reached.</returns>
        public static IEnumerable<GraphNode<TVertex>> Traverse<TVertex>(this IEnumerable<TVertex> starts, Func<GraphNode<TVertex>, IEnumerable<TVertex>> getNeighbors, IEqualityComparer<TVertex> equalityComparer = null)
        {
            Queue<GraphNode<TVertex>> open = new Queue<GraphNode<TVertex>>(starts.Select(s => new GraphNode<TVertex>(s, 0)));
            HashSet<TVertex> closed = equalityComparer != null ? new HashSet<TVertex>(equalityComparer) : new HashSet<TVertex>();

            // As long as there's any open nodes
            while (open.Any())
            {
                // Take the closest one (the first in the queue will always be closest for breadth-first)
                // As a side note, since this is just a traversal, it doesn't matter which one we take since we're going to visit them all
                GraphNode<TVertex> cur = open.Dequeue();
                if (!closed.Add(cur.Value))
                    continue;

                // Return this node
                yield return cur;

                // Enqueue all the neighbors of this node
                foreach (TVertex neighbor in getNeighbors(cur))
                {
                    open.Enqueue(new GraphNode<TVertex>(neighbor, cur.TotalCost + 1));
                }
            }
        }
    }

    public class Sprites
    {
        public Texture2D WhitePixel { get; }

        public Sprites()
        {
            WhitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            WhitePixel.SetData(new[] { Color.White });
        }

        // FillRectangle:
        public void FillRectangle(SpriteBatch batch, Color color, Rectangle bounds, float depth)
        {
            batch.Draw(this.WhitePixel, bounds, null, color, 0F, Vector2.Zero, SpriteEffects.None, depth);
        }
    }

    public class FloodEventsTesting : Mod
    {
        public static IEnumerable<Point> GetFloodedTiles(int? maxDistance)
        {
            // Get a list of all the water tile coordinates
            Farm farm = Game1.getFarm();

            if (farm.waterTiles == null)
                return Enumerable.Empty<Point>();

            int width = farm.waterTiles.GetLength(0);
            int height = farm.waterTiles.GetLength(1);
            List<Point> waterTiles = new List<Point>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (farm.waterTiles[x, y])
                    {
                        waterTiles.Add(new Point(x, y));
                    }
                }
            }

            // Traverse the graph
            return waterTiles.Traverse(GetNeighbors).Select(node => node.Value);

            // Neighbors function
            IEnumerable<Point> GetNeighbors(GraphNode<Point> cur)
            {
                // Check if this is at the max possible distance before adding neighbors
                if (maxDistance != null && cur.TotalCost >= maxDistance)
                    return Enumerable.Empty<Point>();

                // Filter out each neighboring point
                return new[] {
                    new Point(cur.Value.X, cur.Value.Y + 1),
                    new Point(cur.Value.X, cur.Value.Y - 1),
                    new Point(cur.Value.X + 1, cur.Value.Y),
                    new Point(cur.Value.X - 1, cur.Value.Y)
                }.Where(neighbor => {
                    // Make sure it's a valid tile
                    if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
                        return false;

                    // Make sure there's no fence there
                    if (farm.Objects.TryGetValue(new Vector2(neighbor.X, neighbor.Y), out SObject obj) && obj is Fence fence &&
                        (fence.whichType.Value == 2 || fence.whichType.Value == 5))
                        return false;

                    // It's a valid tile
                    return true;
                });
            }
        }

        protected Sprites OurSprites { get; set; }

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>
        public override void Entry(IModHelper helper)
        {
            OurSprites = new Sprites();
            //do something here, I suppose.
            GraphicsEvents.OnPreRenderHudEvent += GraphicsEvents_OnPreRenderHudEvent;
        }

        private void GraphicsEvents_OnPreRenderHudEvent(object sender, EventArgs e)
        {
            foreach (Point floodedTile in GetFloodedTiles(8))
            {
                Rectangle rect = new Rectangle(floodedTile.X * Game1.tileSize - Game1.viewport.X, floodedTile.Y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);
                OurSprites.FillRectangle(Game1.spriteBatch, new Color(0,1f,0f,.25f),rect,1);
            }
        }
    }
}
