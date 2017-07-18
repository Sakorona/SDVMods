using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TwilightCore.StardewValley;
using StardewValley;
using StardewModdingAPI;

namespace ClimatesOfFerngillRebuild
{
    public class FerngillFog
    {
        private Rectangle FogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);
        private Color FogColor { get; set; }
        private float FogAlpha { get; set; }
        public SDVTime FogExpirTime { get; set; }
        public bool FogTypeDark { get; set; }
        public bool AmbientFog { get; set; }
        private Vector2 FogPosition { get; set; }

        public FerngillFog()
        {
            FogExpirTime = new SDVTime(600);
        }

        public FerngillFog(Color FogColor, float FogAlpha, SDVTime FogExpirTime, bool FogTypeDark, 
            bool AmbientFog, Vector2 FogPosition)
        {
            this.FogColor = FogColor;
            this.FogAlpha = FogAlpha;
            this.FogExpirTime = FogExpirTime;
            this.FogTypeDark = FogTypeDark;
            this.AmbientFog = AmbientFog;
            this.FogPosition = FogPosition;
        }

        public void Reset()
        {
            FogTypeDark = false;
            AmbientFog = false;
            FogAlpha = 0f;
        }

        public void IsDarkFog()
        {
            FogTypeDark = true;
        }

        public bool IsFogVisible()
        {
            if (FogAlpha > 0 || AmbientFog)
                return true;
            else
                return false;
        }
        public void UpdateFog(int time, bool debug, IMonitor Monitor)
        {
            if (FogTypeDark)
            {
                if (time == (FogExpirTime - 30).ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-30 minutes");

                    Game1.globalOutdoorLighting = .98f;
                    Game1.outdoorLight = new Color(200, 198, 196);
                }

                if (time == (FogExpirTime - 20).ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-20 minutes");
                    Game1.globalOutdoorLighting = .99f;
                    Game1.outdoorLight = new Color(179, 176, 171);
                }

                if (time == (FogExpirTime - 10).ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-10 minutes");
                    Game1.globalOutdoorLighting = 1f;
                    Game1.outdoorLight = new Color(110, 109, 107);
                }
            }
            else
            {
                if (time == (FogExpirTime - 30).ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-30 minutes");
                    Game1.globalOutdoorLighting = .80f;
                    Game1.outdoorLight = new Color(168, 142, 99);
                }

                if (time == (FogExpirTime - 20).ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-20 minutes");
                    Game1.globalOutdoorLighting = .92f;
                    Game1.outdoorLight = new Color(117, 142, 99);

                }

                if (time == (FogExpirTime - 10).ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-10 minutes");
                    Game1.globalOutdoorLighting = .96f;
                    Game1.outdoorLight = new Color(110, 109, 107);
                }
            }

            //it helps if you implement the fog cutoff!
            if (time == FogExpirTime.ReturnIntTime())
            {
                if (debug) Monitor.Log("Now at T-0 minutes");
                this.AmbientFog = false;
                this.FogTypeDark = false;
                Game1.globalOutdoorLighting = 1f;
                Game1.outdoorLight = Color.White;
                FogAlpha = 0f; //fixes it lingering.
            }

        }

        public void DrawFog()
        {
            if (IsFogVisible())
            {
                Vector2 position = new Vector2();
                float num1 = -64 * Game1.pixelZoom + (int)(FogPosition.X % (double)(64 * Game1.pixelZoom));
                while (num1 < (double)Game1.graphics.GraphicsDevice.Viewport.Width)
                {
                    float num2 = -64 * Game1.pixelZoom + (int)(FogPosition.Y % (double)(64 * Game1.pixelZoom));
                    while ((double)num2 < Game1.graphics.GraphicsDevice.Viewport.Height)
                    {
                        position.X = (int)num1;
                        position.Y = (int)num2;
                        Game1.spriteBatch.Draw(Game1.mouseCursors, position, new Microsoft.Xna.Framework.Rectangle?
                            (FogSource), FogAlpha > 0.0 ? FogColor * FogAlpha : Color.Black * 0.95f, 0.0f, Vector2.Zero, Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                        num2 += 64 * Game1.pixelZoom;
                    }
                    num1 += 64 * Game1.pixelZoom;
                }
            }
        }

        public void MoveFog()
        {
            if (AmbientFog)
            {
                this.FogPosition = Game1.updateFloatingObjectPositionForMovement(FogPosition,
                    new Vector2(Game1.viewport.X, Game1.viewport.Y), Game1.previousViewportPosition, -1f);
                FogPosition = new Vector2((FogPosition.X + 0.5f) % (64 * Game1.pixelZoom), 
                    (FogPosition.Y + 0.5f) % (64 * Game1.pixelZoom));
            }
        }

        public void CreateFog(float FogAlpha, bool AmbientFog, Color FogColor)
        {
            this.FogAlpha = FogAlpha;
            this.AmbientFog = AmbientFog;
            this.FogColor = FogColor;
        }
    }
}
