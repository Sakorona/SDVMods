using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Graphics;
using System;
using static StardewValley.Menus.CharacterCustomization;
using System.IO;

namespace FoggyWeather
{
    public class FoggyWeatherConfig
    {
        public float FadeThreshold = 1f;
        public int MinFadeRate = 0;
        public int MaxFadeRate = 5;
        public bool AllowRandomFadeRate = true;
        public bool HeavyFogFadeRate = false;
        public string DefaultFogThickness = "normal";

        public string FogFadeMethod = "time";
    }


    public class FoggyWeather : Mod
    {
        private static readonly float NormalFogThickness = .61f;
        private static readonly float HeavyFogThickness = .84f;
        private static readonly float LightFogThickness = .41f;
        private static readonly float DenseFogThickness = .92f;

        private static IMonitor Logger;
        private static FoggyWeatherConfig FogConfig;

        protected bool isDrawingFog;
        protected float alphaFog;
        protected bool fogAlphaShouldFade;
        protected Vector2 fogPos;
        protected Color fogColor;

        //protected Texture2D FogTexture;
        //protected Texture2D TestTexture;
        //protected Texture2D BlindingFogTexture;
        //protected Texture2D LightFogTexture;

        private Rectangle FogSource = new(640, 0, 64, 64);
        //private static Rectangle FogSource = new(0, 0, 128, 128);

        protected int fogTime = 0;
        protected int timeElapsed = 0;

        public override void Entry(IModHelper helper)
        {
            FogConfig = Helper.ReadConfig<FoggyWeatherConfig>();

            //load icons
            LoadTextures(helper);

            isDrawingFog = false;
            fogColor = Color.Black;
            alphaFog = 0f;
            Logger = Monitor;
            Helper.Events.Display.RenderedWorld += On_RenderedWorld;
            Helper.Events.GameLoop.UpdateTicked += UpdateTicked;

            Helper.Events.GameLoop.TimeChanged += On_TimeChanged;
            Helper.Events.GameLoop.ReturnedToTitle += On_ReturningToTitle;
            Helper.Events.GameLoop.DayEnding += On_DayEnding;

            helper.ConsoleCommands
               .Add("togglefog", helper.Translation.Get("console-text.togglefog"), ToggleFog)
               .Add("spawnfogfortime", helper.Translation.Get("console-text.spawnfortime"), SpawnForTime)
               .Add("setfogcolor", helper.Translation.Get("console-text.setfogcolor"), SetFogColor)
               .Add("foginfo", "Fog Info", GetFogInfo);

        }

        private void LoadTextures(IModHelper helper)
        {
            //LeafSprites = helper.ModContent.Load<Texture2D>(Path.Combine("assets", "DebrisSpritesFull.png"));
            //WeatherSource = helper.Load<Texture2D>(Path.Combine("assets", "WeatherIcons.png"));
           // FogTexture = helper.ModContent.Load<Texture2D>(Path.Combine("assets", "ThickerFog.png"));
            //TestTexture = helper.ModContent.Load<Texture2D>(Path.Combine("assets", "Test.png"));
           // BlindingFogTexture = helper.ModContent.Load<Texture2D>(Path.Combine("assets", "ThickerFog2.png"));
           // LightFogTexture = helper.ModContent.Load<Texture2D>(Path.Combine("assets", "LighterFog.png"));
            //DarudeTexture = helper.Load<Texture2D>(Path.Combine("assets", "low_sand.png"));
            //source2 = Game1.mouseCursors;
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (isDrawingFog && Game1.shouldTimePass() && alphaFog > 0f)
            {
                this.fogPos = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: this.fogPos, previous: Game1.previousViewportPosition, speed: -1f);
                this.fogPos.X = (this.fogPos.X + 0.5f) % 256f;
                this.fogPos.Y = (this.fogPos.Y + 0.5f) % 256f;
            }
        }

        private void On_ReturningToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Reset();
        }

        private void On_DayEnding(object sender, DayEndingEventArgs e)
        {
            Reset();

        }

        private void Reset()
        {
            fogTime = 0;
            timeElapsed = 0;
            alphaFog = 0f;
            isDrawingFog = false;
        }

        private void On_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            timeElapsed += 10;

            //if we are using set time for fog fade out, we should check.
            if (timeElapsed == fogTime)
            {
                alphaFog = 0f;
            }

            // deal with fade out. Set to a random variable to produce uneven fade rates.
            if (alphaFog > 0f && fogAlphaShouldFade && Game1.random.NextDouble() < FogConfig.FadeThreshold) 
            {
                if (FogConfig.AllowRandomFadeRate)
                {
                    alphaFog = -0.01f * Game1.random.Next(FogConfig.MinFadeRate, FogConfig.MaxFadeRate); //set to allow uneven fade rate
                }
                else
                {
                    if (!FogConfig.HeavyFogFadeRate) 
                        alphaFog = -0.01f;
                    else
                        alphaFog = -0.03f;
                }
                
            }

            //tell it to stop if it reaches 0.
            if (alphaFog <= 0f)
            {
                isDrawingFog = false;
            }
        }

        public static float GetThicknessFromValue(string val)
        {
            return val switch
            {
                "normal" => FoggyWeather.NormalFogThickness,
                "dense" => FoggyWeather.DenseFogThickness,
                "light" => FoggyWeather.LightFogThickness,
                "heavy" => FoggyWeather.HeavyFogThickness,
                _ => .1337f,
            };
        }

        public void SpawnForTime(string arg1, string[] arg2)
        {
            isDrawingFog = true;
            fogTime = Convert.ToInt16(arg2[0]);
            alphaFog = GetThicknessFromValue(FogConfig.DefaultFogThickness);
        }

        public void ToggleFog(string arg1, string[] arg2)
        {
            isDrawingFog = !isDrawingFog;
            alphaFog = GetThicknessFromValue(FogConfig.DefaultFogThickness);

            Logger.Log($"Drawing Fog is {isDrawingFog}, with color {fogColor}, and thickness {alphaFog}", LogLevel.Info);
        }

        public void GetFogInfo(string arg1, string[] arg2)
        {
           Logger.Log($"Drawing Fog is {isDrawingFog}, with color {fogColor}, and thickness {alphaFog} with position {fogPos}", LogLevel.Info);
        }

        public void SetFogColor(string arg1, string[] arg2)
        {
            fogColor = arg2[0] switch
            {
                "BlueViolet" => Color.BlueViolet,
                "White" => Color.White,
                "Khaki" => Color.Khaki,
                "Danger" => new Color(255, 150, 0),
                "Cyan" => (Color.Cyan),
                "Blue" => Color.Blue,
                "Red" => Color.Red,
                _ => Color.Turquoise,
            };
        }

        private void On_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.currentLocation.IsOutdoors && isDrawingFog)
                DrawFog();
        }

        public void DrawFog()
        {
            if (alphaFog > 0f || isDrawingFog)
            {
                Vector2 v = default;
                for (float x = -256f + (int)(fogPos.X % 256f); x < Game1.graphics.GraphicsDevice.Viewport.Width; x += 256f)
                {
                    for (float y = -256f + (int)(fogPos.Y % 256f); y < Game1.graphics.GraphicsDevice.Viewport.Height; y += 256f)
                    {
                        v.X = (int)x;
                        v.Y = (int)y;
                        Game1.spriteBatch.Draw(Game1.mouseCursors, v, FogSource, (alphaFog > 0f) ? (fogColor * alphaFog) : fogColor, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
                    }
                }
            }
  
        }

    }
}