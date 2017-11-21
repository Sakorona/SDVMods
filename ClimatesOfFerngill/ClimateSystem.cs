using EnumsNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
{
    /************************************************************************
     * Weather enums. 
     ******************************************************************************/
    /// <summary>
    /// This enum is for what fog is currently on screen.
    /// </summary>
    internal enum FogType
    {
        None = 0,
        Normal = 1,
        Dark = 2,
        Blinding = 3
    }

    /*Notes on changes from last SpecialWeather enum
     Thundersnow is now both Snow and Lightning
     DryLightning is Thunder but not Rain
     DryLightningHeatwave is now DryLighting and if temp returns heatwave conditions. (They've been intentionally moved to a seperate section.)
     Default flag is now unset.
    */

    /// <summary>
    /// This enum tracks weathers added to the system as well as the current weather.
    /// </summary>
    [Flags]
    public enum CurrentWeather
    {
        Unset = 0,
        Sunny = 2,
        Rain = 4,
        Snow = 8,
        Wind = 16,
        Festival = 32,
        Wedding = 64,
        Lightning = 128,
        Blizzard = 256,
        Fog = 512,
        Frost = 1024,
        Heatwave = 2048
    }

    /// <summary>This tracks blizzard details</summary>
    internal class FerngillBlizzard
    {
        /// <summary> This is for the second snow overlay.</summary>
        private Vector2 snowPos; 

        /* work on creating various buff code here */

        internal void DrawBlizzard()
        {
            snowPos = Game1.updateFloatingObjectPositionForMovement(snowPos, new Vector2(Game1.viewport.X, Game1.viewport.Y),
                        Game1.previousViewportPosition, -1f);
            snowPos.X = snowPos.X % (16 * Game1.pixelZoom);
            Vector2 position = new Vector2();
            float num1 = -16 * Game1.pixelZoom + snowPos.X % (16 * Game1.pixelZoom);
            while ((double)num1 < Game1.viewport.Width)
            {
                float num2 = -16 * Game1.pixelZoom + snowPos.Y % (16 * Game1.pixelZoom);
                while (num2 < (double)Game1.viewport.Height)
                {
                    position.X = (int)num1;
                    position.Y = (int)num2;
                    Game1.spriteBatch.Draw(Game1.mouseCursors, position, new Microsoft.Xna.Framework.Rectangle?
                        (new Microsoft.Xna.Framework.Rectangle
                        (368 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 150) % 1200.0) / 75 * 16, 192, 16, 16)),
                        Color.White * Game1.options.snowTransparency, 0.0f, Vector2.Zero,
                        Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                    num2 += 16 * Game1.pixelZoom;
                }
                num1 += 16 * Game1.pixelZoom;
            }
        }
    }

    // This was a class, but honestly, we should probably just move all of the fog things into one struct. 
    /// <summary> This tracks fog details </summary>
    internal class FerngillFog
    {
        //private bool AmbientFog { get; set; }
        private readonly static Rectangle FogSource = new Rectangle(640, 0, 64, 64);
        private readonly static Color DarkOutdoor = new Color(180, 175, 105);
        private readonly static Color NormalOutdoor = new Color(135, 120, 145);

        internal Color fogLight = Color.White;
        internal Color endLight = Color.White;
        internal Color beginLight = Color.White;

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

        /// Status members
        
        /// <summary> This checks if the fog is visible. </summary>
        internal bool IsFogVisible => (CurrentFogType != FogType.None);

        /// <summary> Returns the expiration time of fog. Note that this doesn't sanity check if the fog is even visible. </summary>
        internal SDVTime ExpirationTime => (ExpirTime ?? new SDVTime(0600));

        /// <summary> Sets the fog expiration time. </summary>
        /// <param name="t">The time for the fog to expire</param>
        internal void SetFogExpirationTime(SDVTime t) => ExpirTime = t;

        /// <summary> Default constructor. </summary>
        internal FerngillFog(bool Verbose, IMonitor Monitor)
        {
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
            /*if (SetFog == FogType.None)
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
            } */
            CurrentFogType = FogType.Dark;

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
                endLight = Color.White;


    }

        public void UpdateFog()
        {
            if (!IsFogVisible)
                return;

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
                Console.WriteLine($"Diff count. Red: {redDiff}, Green: {greenDiff}, Blue: {blueDiff}, Alpha: {alphaDiff}");
                Console.WriteLine($"End Color is {endLight.ToString()} with the calced time remaining as {timeRemain} with {timeTotal} between start and end");
                Console.WriteLine($"This returns: {percentage}");
                Console.WriteLine();
                */

                if (beginLight.R > endLight.R)
                    fogLight.R = (byte)Math.Floor(beginLight.R - (redDiff * percentage));
                else if (beginLight.R == endLight.R)
                    fogLight.R = beginLight.R;
                else
                    fogLight.R = (byte)Math.Floor(endLight.R + (redDiff * percentage));

                if (beginLight.G > endLight.G)
                    fogLight.G = (byte)Math.Floor(beginLight.G - (greenDiff * percentage));
                else if (beginLight.G == endLight.G)
                    fogLight.G = beginLight.G;
                else
                    fogLight.G = (byte)Math.Floor(endLight.G + (greenDiff * percentage));

                if (beginLight.B > endLight.B)
                    fogLight.B = (byte)Math.Floor(beginLight.B - (blueDiff * percentage));
                else if (beginLight.B == endLight.B)
                    fogLight.B = beginLight.B;
                else
                    fogLight.B = (byte)Math.Floor(endLight.B + (blueDiff * percentage));

                if (beginLight.A > endLight.A)
                    fogLight.A = (byte)Math.Floor(beginLight.A - (alphaDiff * percentage));
                else if (beginLight.A == endLight.A)
                    fogLight.A = beginLight.A;
                else
                    fogLight.A = (byte)Math.Floor(endLight.A + (alphaDiff * percentage));

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
            }
        }

        public void DrawFog()
        {
            if (IsFogVisible)
            {
                if (CurrentFogType != FogType.Blinding) { 
                Game1.outdoorLight = fogLight;
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

    /// <summary>
    /// This class tracks the current the current weather of the game
    /// </summary>
    public class WeatherConditions
    {
        ///<summary>Game configuration options</summary>
        private WeatherConfig ModConfig;

        ///<summary>pRNG object</summary>
        private MersenneTwister Dice;

        ///<summary>SMAPI logger</summary>
        private IMonitor Monitor;

        /// <summary> The translation interface </summary>
        private ITranslationHelper Translation;

        /// <summary>Track today's temperature</summary>
        private RangePair TodayTemps;

        /// <summary>Track tomorrow's temperature</summary>
        private RangePair TomorrowTemps;

        /// <summary>Track current conditions</summary>
        private CurrentWeather CurrentConditionsN { get; set; }

        /// <summary>Fog object - handles fog</summary>
        internal FerngillFog OurFog { get; set; }

        //Heatwaves and Frosts don't have icons..
        private int iconWedding = 0;
        private int iconFestival = 1;
        private int iconSunny = 2;
        private int iconSpringDebris = 3;
        private int iconRain = 4;
        private int iconStorm = 5;
        private int iconDebris = 6;
        private int iconSnow = 7;
        private int iconBlizzard = 8;
        private int iconSunnyFog = 9;
        private int iconRainFog = 10;
        private int iconSnowFog = 11;
        private int iconError = 12;
        private int iconStormFog = 13;
        private int iconThunderSnow = 14;
        private int iconDryLightning = 15;

        /// *************************************************************************
        /// ACCESS METHODS
        /// *************************************************************************
        public CurrentWeather GetCurrentConditions()
        {
            if (CurrentConditionsN.HasFlag(CurrentWeather.Fog) && CurrentConditionsN.HasFlag(CurrentWeather.Wind))
                CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);

            return CurrentConditionsN;
        }

        /// <summary>Rather than track the weather seprately, always get it from the game.</summary>
        public CurrentWeather TommorowForecast => ConvertToCurrentWeather(Game1.weatherForTomorrow);

        public bool IsTodayTempSet => TodayTemps != null;
        public bool IsTomorrowTempSet => TomorrowTemps != null;

        /// <summary> This returns the high for today </summary>
        public double TodayHigh => TodayTemps.HigherBound;

        /// <summary> This returns the high for tomorrow </summary>
        public double TomorrowHigh => TomorrowTemps.HigherBound;

        /// <summary> This returns the low for today </summary>
        public double TodayLow => TodayTemps.LowerBound;

        /// <summary> This returns the low for tomorrow </summary>
        public double TomorrowLow => TomorrowTemps.LowerBound;

        public void AddWeather(CurrentWeather newWeather)
        {
            //sanity remove these once weather is set.
            CurrentConditionsN.RemoveFlags(CurrentWeather.Unset);

            //Some flags are contradictoary. Fix that here.
            if (newWeather == CurrentWeather.Rain)
            {
                //unset debris, sunny, snow and blizzard, if it's raining.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Sunny)
            {
                //unset debris, rain, snow and blizzard, if it's sunny.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Wind)
            {
                //unset sunny, rain, snow and blizzard, if it's debris.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Snow);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Blizzard);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Snow)
            {
                //unset debris, sunny, snow and blizzard, if it's raining.
                CurrentConditionsN.RemoveFlags(CurrentWeather.Sunny);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Rain);
                CurrentConditionsN.RemoveFlags(CurrentWeather.Wind);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Frost)
            {
                CurrentConditionsN.RemoveFlags(CurrentWeather.Heatwave);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Heatwave)
            {
                CurrentConditionsN.RemoveFlags(CurrentWeather.Frost);

                CurrentConditionsN |= newWeather;
            }

            else if (newWeather == CurrentWeather.Wedding || newWeather == CurrentWeather.Festival)
            {
                CurrentConditionsN = newWeather; //Clear *everything else* if it's a wedding or festival.
            }

            else
                CurrentConditionsN |= newWeather;
        }

        internal void UpdateForCurrentMoment()
        {
            // Okay. So. I want to hate myself now.
            if (this.OurFog.IsFogVisible)
                CurrentConditionsN.CombineFlags(CurrentWeather.Fog);
            else
                CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);
        }

        /// <summary> Syntatic Sugar for Enum.HasFlag(). Done so if I choose to rewrite how it's accessed, less rewriting of invoking functions is needed. </summary>
        /// <param name="checkWeather">The weather being checked.</param>
        /// <returns>If the weather is present</returns>
        public bool HasWeather(CurrentWeather checkWeather)
        {
            if (!this.OurFog.IsFogVisible && CurrentConditionsN.HasFlag(CurrentWeather.Fog))
                CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);
            
            return CurrentConditionsN.HasFlag(checkWeather);
        }

        public void ClearFog() => CurrentConditionsN.RemoveFlags(CurrentWeather.Fog);

        public bool HasPrecip()
        {
            if (CurrentConditionsN.HasAnyFlags(CurrentWeather.Snow | CurrentWeather.Rain | CurrentWeather.Blizzard))
                return true;

            return false;

        }

        public int CurrentWeatherIcon
        {
            get
            {
                //if (ModConfig.Verbose)
                //    Monitor.Log($"Conditions: {CurrentConditions}");

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Rain))
                    return iconRain;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Rain | CurrentWeather.Lightning)))
                    return iconStorm;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Sunny)))
                    return iconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Wind)))
                    return iconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Snow)))
                    return iconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Snow))
                    return iconSnow;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Blizzard)))
                    return iconBlizzard;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Festival))
                    return iconFestival;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Wedding))
                    return iconWedding;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Sunny))
                    return iconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Unset))
                    return iconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Lightning))
                    return iconStorm;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)CurrentWeather.Wind)) {
                    if (SDate.Now().Season == "spring") return iconSpringDebris;
                    else return iconDebris;
                }

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Lightning)))
                    return iconThunderSnow;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Sunny | CurrentWeather.Lightning)))
                    return iconDryLightning;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Sunny)))
                    return iconSunny;

                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Wind)))
                {
                    if (SDate.Now().Season == "spring") return 3;
                    else return 6;
                }

                //The more complex ones.
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Sunny | CurrentWeather.Frost)))
                    return iconSunny;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Frost)))
                    return iconStorm;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Sunny | CurrentWeather.Frost)))
                    return iconSunny;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Wind | CurrentWeather.Frost)))
                {
                    if (SDate.Now().Season == "spring") return iconSpringDebris;
                    else return iconDebris;
                }
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Rain | CurrentWeather.Frost)))
                    return iconRain;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Frost)))
                    return iconSnow;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Frost)))
                    return iconBlizzard;

                //And now for fog.
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Fog | CurrentWeather.Sunny)))
                    return iconSunnyFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Fog)))
                    return iconStormFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Sunny | CurrentWeather.Fog)))
                    return iconSunnyFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Rain | CurrentWeather.Fog)))
                    return iconRainFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Snow | CurrentWeather.Fog)))
                    return iconSnowFog;
                if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditionsN, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Fog)))
                    return iconSnowFog;

                return iconError;
            }
        }

        /*
        /// <summary> Get whether or not it is a heatwave </summary>
        public bool IsHeatwave => (TodayTemps?.HigherBound >= ModConfig.TooHotOutside);

        /// <summary> Get whether or not it is a frost </summary>
        public bool IsFrost => (TodayTemps?.LowerBound <= ModConfig.TooColdOutside && SDate.Now().Season != "winter");
        */
        //pass through methods
        

        /// ******************************************************************************
        /// CONSTRUCTORS
        /// ******************************************************************************

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="Dice">pRNG</param>
        /// <param name="monitor">SMAPI log object</param>
        /// <param name="Config">Game configuration</param>
        public WeatherConditions(MersenneTwister Dice, ITranslationHelper Translation, IMonitor monitor, WeatherConfig Config)
        {
            this.Monitor = monitor;
            this.ModConfig = Config;
            this.Dice = Dice;
            this.Translation = Translation;
            CurrentConditionsN = CurrentWeather.Unset;
            OurFog = new FerngillFog(Config.Verbose, monitor);
        }

        /// ******************************************************************************
        /// PROCESSING
        /// ******************************************************************************
        internal void ForceTodayTemps(double high, double low)
        {
            if (TodayTemps is null)
                TodayTemps = new RangePair();

            TodayTemps.HigherBound = high;
            TodayTemps.LowerBound = low;
        }

        /// <summary>This function resets the weather for a new day.</summary>
        public void OnNewDay()
        {
            OurFog.OnNewDay();
            CurrentConditionsN = CurrentWeather.Unset;
            TodayTemps = TomorrowTemps; //If Tomorrow is null, should just allow it to be null.
            TomorrowTemps = null;
        }

        /// <summary> This function resets the weather object to basic. </summary>
        public void Reset()
        {
            OurFog.Reset();
            TodayTemps = null;
            TomorrowTemps = null;
            CurrentConditionsN = CurrentWeather.Unset;
        }

        /// <summary> This sets the temperatures from outside for today </summary>
        /// <param name="a">The RangePair that contains the generated temperatures</param>
        public void SetTodayTemps(RangePair a) => TodayTemps = new RangePair(a, EnforceHigherOverLower: true);

        /// <summary> This sets the temperatures from outside for tomorrow</summary>
        /// <param name="a">The RangePair that contains the generated temperatures</param>
        public void SetTomorrowTemps(RangePair a) => TomorrowTemps = new RangePair(a, EnforceHigherOverLower: true);

        /// ***************************************************************************
        /// Utility functions
        /// ***************************************************************************
        
        ///<summary> This function converts from the game weather back to the CurrentWeather enum. Intended primarily for use with tommorow's forecasted weather.</summary>
        internal static CurrentWeather ConvertToCurrentWeather(int weather)
        { 
            if (weather == Game1.weather_rain)
                return CurrentWeather.Rain;
            else if (weather == Game1.weather_festival)
                return CurrentWeather.Festival;
            else if (weather == Game1.weather_wedding)
                return CurrentWeather.Wedding;
            else if (weather == Game1.weather_debris)
                return CurrentWeather.Wind;
            else if (weather == Game1.weather_snow)
                return CurrentWeather.Snow;
            else if (weather == Game1.weather_lightning)
                return CurrentWeather.Rain | CurrentWeather.Lightning;

            //default return.
            return CurrentWeather.Sunny;
        }

        internal void SetTodayWeather()
        {
            CurrentConditionsN = CurrentWeather.Unset; //reset the flag.

            if (!Game1.isDebrisWeather && !Game1.isRaining && !Game1.isSnowing)
                AddWeather(CurrentWeather.Sunny);

            if (Game1.isRaining)
                AddWeather(CurrentWeather.Rain);
            if (Game1.isDebrisWeather)
                AddWeather(CurrentWeather.Wind);
            if (Game1.isLightning)
                AddWeather(CurrentWeather.Lightning);
            if (Game1.isSnowing)
                AddWeather(CurrentWeather.Snow);

            if (Utility.isFestivalDay(SDate.Now().Day, SDate.Now().Season))
                AddWeather(CurrentWeather.Festival);

            if (Game1.weddingToday)
                AddWeather(CurrentWeather.Wedding);
        }

        /// <summary> Force for wedding only to match vanilla behavior. </summary>
        internal void ForceWeddingOnly() => CurrentConditionsN = CurrentWeather.Wedding;

        /// <summary> Force for festival only to match vanilla behavior. </summary>
        internal void ForceFestivalOnly() => CurrentConditionsN = CurrentWeather.Festival;

        /// <summary>Gets a quick string describing the weather. Meant primarily for use within the class. </summary>
        /// <returns>A quick ID of the weather</returns>
        private string GetWeatherType()
        {
            return WeatherConditions.GetWeatherType(CurrentConditionsN);
        }

        /// <summary>Gets a quick string describing the weather. Meant primarily for use within the class. </summary>
        /// <returns>A quick ID of the weather</returns>
        private static string GetWeatherType(CurrentWeather CurrentConditions)
        {
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Rain))
                return "rainy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Rain | CurrentWeather.Lightning)))
                return "stormy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Lightning))
                return "drylightning";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Snow))
                return "snowy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Snow | CurrentWeather.Blizzard)))
                return "blizzard";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Festival))
                return "festival";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Wedding))
                return "wedding";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Sunny))
                return "sunny";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Unset))
                return "unset";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)CurrentWeather.Wind))
                return "windy";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Snow)))
                return "thundersnow";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Sunny)))
                return "drylightningheatwavesunny";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Heatwave | CurrentWeather.Wind)))
                return "drylightningheatwavewindy";

            //The more complex ones.
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Frost | CurrentWeather.Sunny)))
                return "drylightningwithfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Frost)))
                return "stormswithfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Sunny | CurrentWeather.Frost)))
                return "sunnyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Wind | CurrentWeather.Frost)))
                return "windyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Rain | CurrentWeather.Frost)))
                return "rainyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Snow | CurrentWeather.Frost)))
                return "snowyfrost";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Frost)))
                return "blizzardfrost";

            //And now for fog.
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Fog | CurrentWeather.Sunny)))
                return "drylightningwithfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Lightning | CurrentWeather.Rain | CurrentWeather.Fog)))
                return "stormswithfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Sunny | CurrentWeather.Fog)))
                return "sunnyfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Rain | CurrentWeather.Fog)))
                return "rainyfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Snow | CurrentWeather.Fog)))
                return "snowyfog";
            if (GeneralFunctions.ContainsOnlyMatchingFlags(CurrentConditions, (int)(CurrentWeather.Blizzard | CurrentWeather.Snow | CurrentWeather.Fog)))
                return "blizzardfog";

            return "ERROR";
        }

        /// ***************************************************************************
        /// Description functions
        /// ***************************************************************************

        ///<summary>This function returns the current condition (pulled from the translation helper.)</summary>
        internal string DescribeConditions()
        {
            switch (GetWeatherType()){
                case "rainy":
                    return Translation.Get("weather_rainy");
                case "stormy":
                    return Translation.Get("weather_lightning");
                case "drylightning":
                    return Translation.Get("weather_drylightning");
                case "snowy":
                    return Translation.Get("weather_snow");
                case "blizzard":
                    return Translation.Get("weather_blizzard");
                case "wedding":
                    return Translation.Get("weather_wedding");
                case "festival":
                    return Translation.Get("weather_festival");
                case "unset":
                    return Translation.Get("weather_unset");
                case "windy":
                    return Translation.Get("weather_wind");
                case "thundersnow":
                    return Translation.Get("weather_thundersnow");
                case "heatwave":
                    return Translation.Get("weather_heatwave");
                case "drylightningheatwave":
                    return Translation.Get("weather_drylightningheatwave");
                case "drylightningheatwavesunny":
                    return Translation.Get("weather_drylightningheatwavesunny");
                case "drylightningheatwavewindy":
                    return Translation.Get("weather_drylightningheatwavewindy");
                case "drylightningwithfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_drylightning") });
                case "stormswithfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_lightning") });
                case "sunnyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_sunny") });
                case "windyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_wind") });
                case "rainyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_rainy") });
                case "snowyfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_snow") });
                case "blizzardfrost":
                    return Translation.Get("weather_frost", new { condition = Translation.Get("weather_blizzard") });
                case "drylightningwithfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_drylightning") + " " + Translation.Get("weather_sunny") });
                case "stormswithfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_lightning") });
                case "sunnyfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_sunny") });
                case "rainyfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_rainy") });
                case "snowyfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_snow") });
                case "blizzardfog":
                    return Translation.Get("weather_fog", new { condition = Translation.Get("weather_blizzard") });
                default:
                    return "ERROR";
            }     
        }

        public string GetTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.TodayHigh.ToString("N1"),
                    lowTempC = this.TodayLow.ToString("N1"),
                    highTempF = GeneralFunctions.ConvCtF(this.TodayHigh).ToString("N1"),
                    lowTempF = GeneralFunctions.ConvCtF(this.TodayLow).ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.TodayHigh.ToString("N1"),
                    lowTempC = this.TodayLow.ToString("N1")
                });

            return Temperature;
        }

        public string GetTomorrowTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.TomorrowHigh.ToString("N1"),
                    lowTempC = this.TomorrowLow.ToString("N1"),
                    highTempF = GeneralFunctions.ConvCtF(this.TomorrowHigh).ToString("N1"),
                    lowTempF = GeneralFunctions.ConvCtF(this.TomorrowLow).ToString("N1")
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.TomorrowHigh.ToString("N1"),
                    lowTempC = this.TomorrowLow.ToString("N1")
                });

            return Temperature;
        }
    

        /// <summary>
        /// This function returns a description of the object. A very important note that this is meant for debugging, and as such does not do localization.
        /// </summary>
        /// <returns>A string describing hte object.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += $"Low for today is {TodayTemps?.LowerBound.ToString("N3")} with the high being {TodayTemps?.HigherBound.ToString("N3")}. The current conditions are {GetWeatherType()}.";

            if (OurFog.IsFogVisible)
            {
                ret += $" Fog is visible until {OurFog.ExpirationTime} and type {FerngillFog.DescFogType(OurFog.CurrentFogType)}. ";
            }

            ret += $"Weather set for tommorow is {WeatherConditions.GetWeatherType(WeatherConditions.ConvertToCurrentWeather(Game1.weatherForTomorrow))} with high {TomorrowTemps?.HigherBound.ToString("N3")} and low {TomorrowTemps?.LowerBound.ToString("N3")} ";

            return ret;
        }
    }
}
