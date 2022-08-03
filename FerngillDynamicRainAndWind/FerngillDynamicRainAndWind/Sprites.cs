using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FerngillDynamicRainAndWind
{
    /// <summary> Sprites used for drawing various weather stuff </summary>
    public class Icons
    {
        public static Texture2D Source2;
        public Texture2D LeafSprites;

        public Icons(IModContentHelper helper)
        {
            LeafSprites = helper.Load<Texture2D>(Path.Combine("assets", "DebrisSpritesFull.png"));
            //LeafSprites = helper.Load<Texture2D>(Path.Combine("assets", "Testing.png"));
            Source2 = Game1.mouseCursors;
        }
    }
}
