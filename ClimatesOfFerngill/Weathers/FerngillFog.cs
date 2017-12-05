using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;
using static ClimatesOfFerngillRebuild.Sprites;

namespace ClimatesOfFerngillRebuild
{
    // This was a class, but honestly, we should probably just move all of the fog things into one struct. 
    /// <summary> This tracks fog details </summary>
    internal class FerngillFog
    {
        //private bool AmbientFog { get; set; }
        //private readonly static Rectangle FogSource = new Rectangle(640, 0, 64, 64);
        public static Rectangle FogSource = new Rectangle(0, 0, 64, 64);
        private readonly static Color DarkOutdoor = new Color(180, 175, 105);
        private readonly static Color NormalOutdoor = new Color(135, 120, 145);

        internal Color fogLight = Color.White;
        internal Color endLight = Color.White;
        internal Color beginLight = Color.White;

        internal Icons Sheet;

        /// <summary> The Current Fog Type </summary>
        internal FogType CurrentFogType { get; set; }

        /// <summary> The color of the fog. Set to LightGreen * 1.5f </summary>
        private readonly Color FogColor = Color.LightGreen * 1.5f;

        private bool VerboseDebug { get; set; }
        private IMonitor Monitor { get; set; }

        /// <summary>  The alpha attribute of the fog. </summary>
        private float FogAlpha { get; set; }

        /// <summary> Fog Position. For drawing. </summary>
        private Vector2 FogPosition { get; set; }

        /// <summary> Sets the expiration time of the fog </summary>
        private SDVTime ExpirTime { get; set; }

        internal bool IsFogVisible {
            get
            {
                   return false;
            }
          }


        /// <summary> Returns the expiration time of fog. Note that this doesn't sanity check if the fog is even visible. </summary>
        internal SDVTime ExpirationTime => (ExpirTime ?? new SDVTime(0600));

        /// <summary> Sets the fog expiration time. </summary>
        /// <param name="t">The time for the fog to expire</param>
        internal void SetFogExpirationTime(SDVTime t) => ExpirTime = t;

        /// <summary> Default constructor. </summary>
        internal FerngillFog(Icons Sheet, bool Verbose, IMonitor Monitor)
        {
            this.Sheet = Sheet;
            CurrentFogType = FogType.None;
            ExpirTime = null;
            VerboseDebug = Verbose;
            this.Monitor = Monitor;
        }

        /// <summary> This function resets the fog for a new day. </summary>
        internal void OnNewDay()
        {
            Reset();
        }

        /// <summary> This function resets the fog. </summary>
        internal void Reset()
        {
            CurrentFogType = FogType.None;
            ExpirTime = null;
            FogAlpha = 0f;
        }

        /// <summary>Returns a string describing the fog type. </summary>
        /// <param name="CurrentFogType">The type of the fog being looked at.</param>
        /// <returns>The fog type</returns>
        internal static string DescFogType(FogType CurrentFogType)
        {
            switch (CurrentFogType)
            {
                case FogType.None:
                    return "None";
                case FogType.Blinding:
                    return "Blinding";
                case FogType.Dark:
                    return "Dark";
                case FogType.Normal:
                    return "Normal";
                default:
                    return "ERROR";
            }
        }

        public void SetColor(int red, int green, int blue)
        {
            Game1.outdoorLight = new Color(red, green, blue);
        }

        /// <summary>This function creates the fog
        /// </summary>
        /// <param name="Dice">pRNG </param>
        /// <param name="WeatherOpt">Mod options</param>
        /// <param name="SetFog">Preset fog type. Defaults to None</param>
        public void CreateFog(MersenneTwister Dice, WeatherConfig WeatherOpt, FogType SetFog = FogType.None)
        {
            this.FogAlpha = 1f;
            //First, let's determine the type.
            //... I am a dumb foxgirl. A really dumb one. 
            if (SetFog == FogType.None)
            {
                if (Dice.NextDoublePositive() < WeatherOpt.DarkFogChance && SDate.Now().Day != 1 && SDate.Now().Year != 1 && SDate.Now().Season != "spring")
                    CurrentFogType = FogType.Dark;
                else if (Dice.NextDoublePositive() <= .001)
                    CurrentFogType = FogType.Blinding;
                else
                    CurrentFogType = FogType.Normal;
            }
            else
            {
                CurrentFogType = SetFog;
            }

            //IsFogVisible = true;
            //CurrentFogType = FogType.Dark; //testing

            //Set the outdoorlight depending on the type.
            switch (CurrentFogType)
            {
                case FogType.Normal:
                case FogType.Blinding:
                    Game1.outdoorLight = FerngillFog.NormalOutdoor;
                    //Game1.ambientLight = FerngillFog.NormalOutdoor * 3f;
                    fogLight = FerngillFog.NormalOutdoor;
                    break;
                case FogType.Dark:
                    Game1.outdoorLight = FerngillFog.DarkOutdoor;
                    //Game1.ambientLight = FerngillFog.DarkOutdoor * 3f;
                    fogLight = FerngillFog.DarkOutdoor;
                    break;
                case FogType.None:
                    return;
            }

            //now determine the fog expiration time
            double FogChance = Dice.NextDoublePositive();

            /*
             * So we should rarely have full day fog, and it should on average burn off around 9am. 
             * So, the strongest odds should be 820 to 930, with sharply falling off odds until 1200. And then
             * so, extremely rare odds for until 7pm and even rarer than midnight.
             */

            if (FogChance > 0 && FogChance < .25)
                this.ExpirTime = new SDVTime(830);
            else if (FogChance >= .25 && FogChance < .32)
                this.ExpirTime = new SDVTime(900);
            else if (FogChance >= .32 && FogChance < .41)
                this.ExpirTime = new SDVTime(930);
            else if (FogChance >= .41 && FogChance < .55)
                this.ExpirTime = new SDVTime(950);
            else if (FogChance >= .55 && FogChance < .7)
                this.ExpirTime = new SDVTime(1040);
            else if (FogChance >= .7 && FogChance < .8)
                this.ExpirTime = new SDVTime(1120);
            else if (FogChance >= .8 && FogChance < .9)
                this.ExpirTime = new SDVTime(1200);
            else if (FogChance >= .9 && FogChance < .95)
                this.ExpirTime = new SDVTime(1220);
            else if (FogChance >= .95 && FogChance < .98)
                this.ExpirTime = new SDVTime(1300);
            else if (FogChance >= .98 && FogChance < .99)
                this.ExpirTime = new SDVTime(1910);
            else if (FogChance >= .99)
                this.ExpirTime = new SDVTime(2400);

            int endTime = this.ExpirTime.ReturnIntTime();

            beginLight = fogLight;

            //calculate the ending light color
            if (endTime >= Game1.getTrulyDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.75 + ((int)(endTime - endTime % 100 + endTime % 100 / 10 * 16.6599998474121) - Game1.getTrulyDarkTime() + Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.000624999986030161));
                endLight = (Game1.isRaining ? DarkOutdoor : Game1.eveningColor) * num;
            }
            else if (endTime >= Game1.getStartingToGetDarkTime())
            {
                float num = Math.Min(0.93f, (float)(0.300000011920929 + ((int)(endTime - endTime % 100 + endTime % 100 / 10 * 16.6599998474121) - Game1.getStartingToGetDarkTime() + (double)Game1.gameTimeInterval / 7000.0 * 16.6000003814697) * 0.00224999990314245));
                endLight = (Game1.isRaining ? DarkOutdoor : Game1.eveningColor) * num;
            }
            else if (Game1.isRaining)
                endLight = Game1.ambientLight * 0.3f;
            else
                endLight = new Color(0, 0, 0); // I have a sneaking suspcion
        }

        public void UpdateFog()
        {
            if (!IsFogVisible)
            {
                return;
            }

            //Console.WriteLine($"Fog end time is {ExpirationTime} with current time being {Game1.timeOfDay}");
            //Console.WriteLine($"Begin light is {beginLight.ToString()}");
            //Console.WriteLine();

            if (CurrentFogType == FogType.Dark || CurrentFogType == FogType.Normal && ExpirationTime >= SDVTime.CurrentTime)
            {
                int timeRemain = ExpirationTime.ReturnIntTime() - Game1.timeOfDay;
                int timeTotal = ExpirationTime.ReturnIntTime() - 0600;
                double percentage = 1 - (timeRemain / (double)timeTotal);


                byte redDiff = (byte)Math.Abs(beginLight.R - endLight.R);
                byte greenDiff = (byte)Math.Abs(beginLight.G - endLight.G);
                byte blueDiff = (byte)Math.Abs(beginLight.B - endLight.B);
                byte alphaDiff = (byte)Math.Abs(beginLight.A - endLight.A);

                /*
                Console.WriteLine($"Fog end time is {ExpirationTime} with current time being {Game1.timeOfDay}");
                Console.WriteLine($"Begin light is {beginLight.ToString()}");
                Console.WriteLine($"End Color is {endLight.ToString()} with the calced time remaining as {timeRemain} with {timeTotal} between start and end");
                Console.WriteLine($"Diff count. Red: {redDiff}, Green: {greenDiff}, Blue: {blueDiff}, Alpha: {alphaDiff}");
                Console.WriteLine($"This returns: {percentage}");
                Console.WriteLine();
                Console.WriteLine($"Diff calculated is R:{Math.Floor(redDiff * percentage)}, G: {Math.Floor(greenDiff * percentage)}, B: {Math.Floor(blueDiff * percentage)}, A: {Math.Floor(alphaDiff * percentage)}");
                Console.WriteLine();*/

                if (beginLight.R > endLight.R)
                    fogLight.R = (byte)Math.Floor(beginLight.R - (redDiff * percentage));
                else if (beginLight.R == endLight.R)
                    fogLight.R = beginLight.R;
                else
                    fogLight.R = (byte)Math.Floor(beginLight.R + (redDiff * percentage));

                if (beginLight.G > endLight.G)
                    fogLight.G = (byte)Math.Floor(beginLight.G - (greenDiff * percentage));
                else if (beginLight.G == endLight.G)
                    fogLight.G = beginLight.G;
                else
                    fogLight.G = (byte)Math.Floor(beginLight.G + (greenDiff * percentage));

                if (beginLight.B > endLight.B)
                    fogLight.B = (byte)Math.Floor(beginLight.B - (blueDiff * percentage));
                else if (beginLight.B == endLight.B)
                    fogLight.B = beginLight.B;
                else
                    fogLight.B = (byte)Math.Floor(beginLight.B + (blueDiff * percentage));

                if (beginLight.A > endLight.A)
                    fogLight.A = (byte)Math.Floor(beginLight.A - (alphaDiff * percentage));
                else if (beginLight.A == endLight.A)
                    fogLight.A = beginLight.A;
                else
                    fogLight.A = (byte)Math.Floor(beginLight.A + (alphaDiff * percentage));

                //Console.WriteLine(value: $"FogLight is {fogLight.ToString()}");            
            }

            if (ExpirationTime <= SDVTime.CurrentTime)
            {
                if (VerboseDebug)
                    Monitor.Log("Now at 0 minutes or fog has expired");

                CurrentFogType = FogType.None;
                Game1.outdoorLight = Color.White;
                fogLight = Color.White;
                FogAlpha = 0f;
                //IsFogVisible = false;
            }
        }

        public void DrawFog()
        {
            if (IsFogVisible)
            {
                if (CurrentFogType != FogType.Blinding)
                {
                    Game1.outdoorLight = fogLight;
                    Vector2 position = new Vector2();
                    float num1 = -64* Game1.pixelZoom + (int)(FogPosition.X % (double)(64 * Game1.pixelZoom));
                    while (num1 < (double)Game1.graphics.GraphicsDevice.Viewport.Width)
                    {
                        float num2 = -64 * Game1.pixelZoom + (int)(FogPosition.Y % (double)(64 * Game1.pixelZoom));
                        while ((double)num2 < Game1.graphics.GraphicsDevice.Viewport.Height)
                        {
                            position.X = (int)num1;
                            position.Y = (int)num2;
                            Game1.spriteBatch.Draw(Sheet.FogTexture, position, new Microsoft.Xna.Framework.Rectangle?
                                (FogSource), FogAlpha > 0.0 ? FogColor * FogAlpha : Color.Black * 0.95f, 0.0f, Vector2.Zero, Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                            num2 += 64 * Game1.pixelZoom;
                        }
                        num1 += 64 * Game1.pixelZoom;
                    }
                }
            }
        }

        public void MoveFog()
        {
            if (IsFogVisible)
            {
                Game1.outdoorLight = fogLight;
                this.FogPosition = Game1.updateFloatingObjectPositionForMovement(FogPosition,
                    new Vector2(Game1.viewport.X, Game1.viewport.Y), Game1.previousViewportPosition, -1f);
                FogPosition = new Vector2((FogPosition.X + 0.5f) % (64 * Game1.pixelZoom),
                    (FogPosition.Y + 0.5f) % (64 * Game1.pixelZoom));
            }
        }
    }
}