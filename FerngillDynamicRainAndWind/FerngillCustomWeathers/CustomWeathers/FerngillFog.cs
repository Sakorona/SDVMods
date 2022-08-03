using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using System.Diagnostics;
using StardewModdingAPI;

namespace FerngillCustomWeathers.CustomWeathers
{
    /// <summary> This tracks fog details </summary>
    internal class FerngillFog
    {
        private readonly WeatherConfig config;
        private readonly Icons TextureData;

        private const int TextureSize = 128;
        public static Rectangle FogSource = new Rectangle(0, 0, TextureSize, TextureSize);
        private Color _fogColor = Color.White * 1.25f;
        internal FogType CurrentFogType { get; set; }
        internal FogType EveningFog { get; set; }
        public bool BloodMoon { get; set; }
        private float FogTargetAlpha { get; set; }
        public float FogAlpha { get; private set; }
        private Vector2 FogPosition { get; set; }
        public int ExpirationTime { get; private set; }
        public int BeginTime { get; private set; }
        public bool HideInCurrentLocation { get; set; }
        public bool IsWeatherVisible => (!Game1.isFestival() && CurrentFogType != FogType.None);
        private bool FadeOutFog { get; set; }
        private bool FadeInFog { get; set; }
        private bool FogStarted { get; set; }
        private Stopwatch FogElapsed { get; }
        internal Texture2D FogTexture;

        internal FerngillFog(WeatherConfig c, Icons texture)
        {
            TextureData = texture;
            config = c;

            CurrentFogType = FogType.None;
            EveningFog = FogType.None;
            BloodMoon = false;
            FogStarted = false;
            FogElapsed = new Stopwatch();
            HideInCurrentLocation = false;
        }

        public void SetFogTargetAlpha()
        {
            if (config.UseLighterFog)
            {
                this.FogTargetAlpha = Game1.isRaining ? .25f : .35f;
            }
            else
            {
                this.FogTargetAlpha = .7f;
            }

            switch (CurrentFogType)
            {
                case FogType.Light:
                    this.FogTargetAlpha = .6f; //.4f is barely visible
                    break;
                case FogType.Blinding:
                    this.FogTargetAlpha = .95f;
                    break;
            }
        }

        /// <summary> This function resets the fog for a new day. </summary>
        public void OnNewDay()
        {
            Reset();
        }

        /// <summary> This function resets the fog. </summary>
        public void Reset()
        {
            CurrentFogType = FogType.None;
            EveningFog = FogType.None;
            BloodMoon = false;
            FogAlpha = 0f;
            FogTargetAlpha = 0f;
            FadeOutFog = false;
            FogStarted = false;
            FadeInFog = false;
            BeginTime = 600;
            ExpirationTime = 600;
            FogElapsed.Reset();
            FogTexture = null;
        }

        /// <summary>Returns a string describing the fog type. </summary>
        /// <param name="currentFogType">The type of the fog being looked at.</param>
        /// <returns>The fog type</returns>
        internal static string DescFogType(FogType currentFogType)
        {
            switch (currentFogType)
            {
                case FogType.None:
                    return "None";
                case FogType.Light:
                    return "Light";
                case FogType.Blinding:
                    return "Blinding";
                case FogType.Normal:
                    return "Normal";
                default:
                    return "ERROR";
            }
        }

        /// <summary>This function creates the fog </summary>
        public void CreateWeather(FogType fType, bool force, bool isEvening = false)
        {
            FogElapsed.Reset();

            if (!isEvening)
            {
                CurrentFogType = FogType.Normal;

                if (Game1.random.NextDouble() < .25)
                    CurrentFogType = FogType.Light;

                else if (Game1.random.NextDouble() > .95)
                    CurrentFogType = FogType.Blinding;

                if (force)
                    CurrentFogType = fType;
            }
            else
            {
                EveningFog = fType;
            }
            

            SetFogTargetAlpha();

            switch (CurrentFogType)
            {
                case FogType.Normal:
                    FogTexture = TextureData.LightFogTexture;
                    break;
                case FogType.Light:
                    FogTexture = TextureData.ThickFogTexture;
                    break;
                case FogType.Blinding:
                    FogTexture = TextureData.ThickestFogTexture;
                    break;
                default:
                    FogTexture = TextureData.ThickFogTexture;
                    break;
            }

            FogAlpha = FogTargetAlpha;
        }

        public void EndWeather()
        {
            if (IsWeatherVisible)
            {
                ExpirationTime = Utility.ModifyTime(Game1.timeOfDay, - 10);
                CurrentFogType = FogType.None;
                FadeOutFog = true;
                SetFogTargetAlpha();
                FogElapsed.Stop();
            }
        }

        public void SetWeatherTime(int begin, int end)
        {
            FerngillCustomWeathers.Logger.Log($"Firing weather set time for fog at {begin} and {end}", LogLevel.Info);

            BeginTime = begin;
            ExpirationTime = end;
        }

        public override string ToString()
        {
            return $"Fog Weather from {BeginTime} to {ExpirationTime}. Visible: {IsWeatherVisible}.  Alpha: {FogAlpha}.";
        }

        public void UpdateWeather()
        {
            //Console.WriteLine($"Fog Alpha at this tick is {FogAlpha} with target set to {FogTargetAlpha}");

            if (Game1.timeOfDay > BeginTime && Game1.timeOfDay < ExpirationTime && !FogStarted && EveningFog != FogType.None)
            {
                SetFogTargetAlpha();
                CurrentFogType = EveningFog;
                FogElapsed.Start();
                EveningFog = FogType.None;
                FadeInFog = true;
                FogStarted = true;
            }

            if (Game1.timeOfDay >= BeginTime && Game1.timeOfDay < ExpirationTime && !FogStarted && !IsWeatherVisible)
            {
                //FogElapsed.Reset();
                SetFogTargetAlpha();
                CurrentFogType = FogType.Normal;
                FogElapsed.Start();
                FadeInFog = true;
                FogStarted = true;
            }

            if (Game1.timeOfDay >= ExpirationTime && IsWeatherVisible)
            {
                //FogElapsed.Reset();
                SetFogTargetAlpha();
                FadeOutFog = true;
                FogElapsed.Start();
                FogStarted = false;
            }

            if (Game1.timeOfDay >= Game1.getModeratelyDarkTime())
            {
                FogTexture = TextureData.NightFogTexture;

                if (CurrentFogType == FogType.Blinding)
                {
                    FogTexture = TextureData.SherlockHolmesFogTexture;
                }
            }

            if (!(Game1.currentLocation is null) && (Game1.currentLocation is Woods))
            {
                FogTexture = TextureData.ThickFogTexture;
                if (Game1.timeOfDay >= Game1.getModeratelyDarkTime())
                    FogTexture = TextureData.SherlockHolmesFogTexture;
            }
        }
        
        public void DrawWeather()
        {
            if (IsWeatherVisible && !HideInCurrentLocation && FogTexture != null)
            {
                if (!(Game1.currentLocation is Desert) ||
                    (config.DisplayFogInTheDesert && Game1.currentLocation is Desert))
                {
                    Vector2 v = default;

                    if (BloodMoon)
                    {
                        _fogColor = Color.DarkRed;
                    }

                    for (float x = -1 * TextureSize + (int) (FogPosition.X % 128f);
                        x < (float) Game1.graphics.GraphicsDevice.Viewport.Width;
                        x += 128f)
                    {
                        for (float y = -1 * TextureSize + (int) (FogPosition.Y % 128f);
                            y < (float) Game1.graphics.GraphicsDevice.Viewport.Height;
                            y += 128f)
                        {
                            v.X = (int) x;
                            v.Y = (int) y;
                            Game1.spriteBatch.Draw(FogTexture, v, FogSource,
                                FogAlpha > 0.0 ? _fogColor * FogAlpha : Color.Black, 0f,
                                Vector2.Zero, 1.001f, SpriteEffects.None, 1f);
                        }
                    }
                }
            }
        }

        public void MoveWeather()
        {
            const float fogFadeTime = 3120f;
            if (FadeOutFog)
            {
                // we want to fade out the fog over 3 or so seconds, so we need to process a fade from 100% to 45%
                // So, 3000ms for 55% or 54.45 repeating. But this is super fast....
                // let's try 955ms.. or 1345..
                // or 2690.. so no longer 3s. 
                FogAlpha = FogTargetAlpha - (FogTargetAlpha * (FogElapsed.ElapsedMilliseconds / fogFadeTime));
                //Console.WriteLine($"Fade out code: {FogAlpha:N3}, for elapsed time {FogElapsed.ElapsedMilliseconds}");
                if (FogAlpha <= 0)
                {
                    FogAlpha = 0;
                    FogTargetAlpha = 0;
                    FogElapsed.Stop();
                    FogElapsed.Reset();
                    CurrentFogType = FogType.None;
                    FadeOutFog = false;
                }
            }

            if (FadeInFog)
            {
                //as above, but the reverse.
                FogAlpha = FogTargetAlpha * (FogElapsed.ElapsedMilliseconds / fogFadeTime);

                if (FogAlpha >= FogTargetAlpha)
                {
                    FogAlpha = FogTargetAlpha;
                    FadeInFog = false;
                    FogElapsed.Stop();
                    FogElapsed.Reset();
                }
            }

            if (IsWeatherVisible && Game1.shouldTimePass() )
            {
                if (Game1.IsDebrisWeatherHere()) {
                    this.FogPosition = Game1.updateFloatingObjectPositionForMovement(FogPosition,
                        new Vector2(Game1.viewport.X, Game1.viewport.Y), Game1.previousViewportPosition, -1f);
                    FogPosition = new Vector2((FogPosition.X + 0.5f) % (TextureSize) + WeatherDebris.globalWind,
                        (FogPosition.Y + 0.5f) % (TextureSize));
                }
                else {
                    //Game1.outdoorLight = fogLight;
                    this.FogPosition = Game1.updateFloatingObjectPositionForMovement(FogPosition,
                        new Vector2(Game1.viewport.X, Game1.viewport.Y), Game1.previousViewportPosition, -1f);
                    FogPosition = new Vector2((FogPosition.X + 0.5f) % (TextureSize),
                        (FogPosition.Y + 0.5f) % (TextureSize));
                }
            }
        }
    }
}