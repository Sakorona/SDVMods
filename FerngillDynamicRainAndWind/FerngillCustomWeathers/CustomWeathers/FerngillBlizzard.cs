using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace FerngillCustomWeathers
{
    internal class FerngillBlizzard
    {
        private Vector2 snowPosA;
        private Vector2 snowPosB;
        private Vector2 snowPosC;

        private bool IsWhiteOut;
        private bool IsBloodMoon;

        public bool HideInCurrentLocation { get; set; }

        public int ExpirationTime { get; private set; }
        public int BeginTime { get; private set; }
        public bool IsWeatherVisible => (Game1.timeOfDay >= BeginTime && Game1.timeOfDay <= ExpirationTime);

        internal FerngillBlizzard()
        {
            HideInCurrentLocation = false;
        }

        public void OnNewDay()
        {
            Reset();
        }

        /// <summary> This function resets the fog. </summary>
        public void Reset()
        {
            HideInCurrentLocation = false;
            IsWhiteOut = false;
            IsBloodMoon = false;
            BeginTime = 600;
            ExpirationTime = 600;
        }

        public void SetWhiteOut(bool whiteout)
        {
            IsWhiteOut = whiteout;
        }
        
        public void UpdateWeather()
        {
            snowPosA = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: snowPosA, previous: Game1.previousViewportPosition, speed: -1f);
            snowPosB = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: snowPosB, previous: Game1.previousViewportPosition, speed: -1f);
            snowPosC = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: snowPosC, previous: Game1.previousViewportPosition, speed: -1f);
        }

        public void SetWeatherTime(int begin, int end)
        {
            FerngillCustomWeathers.Logger.Log($"Firing weather set time for blizzard at for {begin} and {end}", LogLevel.Info);

            BeginTime = begin;
            ExpirationTime = end;
        }

        public void EndWeather()
        {
            ExpirationTime = Game1.timeOfDay - 10;
        }

        public void DrawWeather()
        {
            if (!IsWeatherVisible && !HideInCurrentLocation)
                return;

            Color snowColor = (IsBloodMoon ? Color.Red : Color.White) * .8f * Game1.options.snowTransparency;

            if (Game1.IsSnowingHere() && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert))
            {
                Console.WriteLine("Drawing the blizzard!!!!");
                snowPosA.X %= 64f;
                Vector2 v = default;
                for (float x = -64f + snowPosA.X % 64f; x < Game1.viewport.Width; x += 64f)
                {
                    for (float y = -64f + snowPosA.Y % 64f; y < Game1.viewport.Height; y += 64f)
                    {
                        v.X = (int)x;
                        v.Y = (int)y;
                        Game1.spriteBatch.Draw(Game1.mouseCursors, v, new Rectangle(368 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 150 % 1200.0) / 75 * 16, 192, 16, 16), snowColor, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
                    }
                }
            }

            if (Game1.IsSnowingHere() && Game1.currentLocation.IsOutdoors &&
                !(Game1.currentLocation is Desert) && IsWhiteOut)
            {
                Console.WriteLine("Drawing the whiteout");
                snowPosB.X %= 64f;
                Vector2 v2 = default;
                for (float x = -64f + snowPosB.X % 64f; x < Game1.viewport.Width; x += 64f)
                {
                    for (float y = -64f + snowPosB.Y % 64f; y < Game1.viewport.Height; y += 64f)
                    {
                        v2.X = (int)x;
                        v2.Y = (int)y;
                        Game1.spriteBatch.Draw(Game1.mouseCursors, v2, new Rectangle
                                (368 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 225) % 1200.0) / 75 * 16, 192, 16, 16),
                            snowColor, 0.0f, Vector2.Zero,
                            Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                    }
                }

                snowPosC.X %= 64f;
                Vector2 v3 = default;
                for (float x = -64f + snowPosC.X % 64f; x < Game1.viewport.Width; x += 64f)
                {
                    for (float y = -64f + snowPosC.Y % 64f; y < Game1.viewport.Height; y += 64f)
                    {
                        v3.X = (int)x;
                        v3.Y = (int)y;
                        Game1.spriteBatch.Draw(Game1.mouseCursors,v3, new Rectangle
                                (368 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 707) % 1200.0) / 75 * 16, 192, 16, 16),
                            snowColor, 0.0f, Vector2.Zero,
                            Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                    }
                }
            }
        }
    }

}
