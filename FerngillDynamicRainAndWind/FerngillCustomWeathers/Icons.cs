using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FerngillCustomWeathers
{
    /// <summary> Sprites used for drawing various weather stuff </summary>
    public class Icons
    {
        public Texture2D LightFogTexture;
        public Texture2D ThickFogTexture;
        public Texture2D NightFogTexture;
        public Texture2D SherlockHolmesFogTexture;
        public Texture2D ThickestFogTexture;
        public static Texture2D Source2;

        public Icons(IModContentHelper helper)
        {
            LightFogTexture = helper.Load<Texture2D>(Path.Combine("assets", "LighterFog.png"));
            ThickFogTexture = helper.Load<Texture2D>(Path.Combine("assets", "ThickerFog.png"));
            NightFogTexture = helper.Load<Texture2D>(Path.Combine("assets", "BlueThickFog.png"));
            SherlockHolmesFogTexture = helper.Load<Texture2D>(Path.Combine("assets", "DarkBlueThickFog.png"));
            ThickestFogTexture = helper.Load<Texture2D>(Path.Combine("assets", "ThickerFog2.png"));
            Source2 = Game1.mouseCursors;
        }
    }
}
