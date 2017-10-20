using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using TwilightCore;
using TwilightCore.PRNG;
using TwilightCore.StardewValley;

namespace ClimatesOfFerngillRebuild
{
    public class WeatherConditions
    {
        //STATIC MEMBERS
        public static bool IsHeatwave(SpecialWeather cond) => (cond == SpecialWeather.DryLightningAndHeatwave || cond == SpecialWeather.Heatwave);
        public static bool IsFrost(SpecialWeather cond) => (cond == SpecialWeather.Frost);

        //Instance Members
        public RangePair TodayTemps { get; private set; }
        public int TodayWeather;
        public RangePair TomorrowTemps { get; private set; }
        public int TomorrowWeather;
        public SpecialWeather UnusualWeather;

        // Things needed for internal fog stuff.
        private bool AmbientFog { get; set; }
        private Rectangle FogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);
        private bool FogTypeDark { get; set; }
        private Color FogColor { get; set; }
        private float FogAlpha { get; set; }
        private Vector2 FogPosition { get; set; }
        private SDVTime FogExpirTime { get; set; }

        //access control members
        public bool IsFogVisible() => (AmbientFog);
        public void SetTodayTemps(RangePair a) => TodayTemps = new RangePair(a, EnforceHigherOverLower: true);
        public void SetTmrwTemps(RangePair a) => TomorrowTemps = new RangePair(a, EnforceHigherOverLower: true);
        public bool IsDarkFog() => (IsFogVisible() && FogTypeDark);
        public void SetFogExpirTime(SDVTime t) => FogExpirTime = t;
        public SDVTime GetFogEndTime() => (FogExpirTime ?? new SDVTime(0600));
        public bool IsDangerousWeather() => (UnusualWeather != SpecialWeather.None);

        public double GetTodayHigh() => TodayTemps.HigherBound;
        public double GetTodayLow() => TodayTemps.LowerBound;
        public double GetTodayHighF() => ConvCtF(TodayTemps.HigherBound);
        public double GetTodayLowF() => ConvCtF(TodayTemps.LowerBound);
        public double GetTmrwHigh() => TomorrowTemps.HigherBound;
        public double GetTmrwLow() => TomorrowTemps.LowerBound;
        public double GetTmrwHighF() => ConvCtF(TomorrowTemps.HigherBound);
        public double GetTmrwLowF() => ConvCtF(TomorrowTemps.LowerBound);
        private double ConvCtF(double temp) => ((temp * 1.8) + 32);

        public void ResetTodayTemps(double high, double low)
        {
            TodayTemps.HigherBound = high;
            TodayTemps.LowerBound = low;
        }


        public WeatherConditions()
        {
            UnusualWeather = SpecialWeather.None;
            FogExpirTime = new SDVTime(600);
        }

        
        public void OnNewDay()
        {
            TodayWeather = 0;
            TomorrowWeather = 0;
            UnusualWeather = SpecialWeather.None;
            TodayTemps = null;
            TomorrowTemps = null;
            FogExpirTime = new SDVTime(600);
        }

        public void Reset()
        {
            TodayTemps = null;
            TomorrowTemps = null;
            TodayWeather = 0;
            TomorrowWeather = 0;
            UnusualWeather = SpecialWeather.None;
            FogTypeDark = false;
            AmbientFog = false;
            FogAlpha = 0f;
            FogExpirTime = new SDVTime(0600);
        }

        // FOG SECTION.

        /// <summary>
        /// This resets the fog to null.
        /// </summary>
        public void ResetFog()
        {
            FogTypeDark = false;
            AmbientFog = false;
            FogAlpha = 0f;
            FogExpirTime = new SDVTime(0600);
        }

        public void InitFog(MersenneTwister Dice, WeatherConfig WeatherOpt, bool FogTypeDark = false)
        {
            this.FogColor = Color.White * 1.5f;
            this.FogAlpha = 1f;
            this.FogTypeDark = FogTypeDark;
            this.AmbientFog = true;

            if (Dice.NextDoublePositive() < WeatherOpt.DarkFogChance &&
                    (SDate.Now().Day != 1 && SDate.Now().Year != 1 && SDate.Now().Season != "spring"))
            {
                FogTypeDark = true;
                Game1.outdoorLight = new Color(214, 210, 208);
            }
            else
            {
                Game1.outdoorLight = new Color(180, 155, 110);
            }

            double FogTimer = Dice.NextDoublePositive();
            this.FogExpirTime = new SDVTime(1200); //default

            if (FogTimer > .75 && FogTimer <= .90)
            {
                FogExpirTime = new SDVTime(1120);
            }
            else if (FogTimer > .55 && FogTimer <= .75)
            {
                FogExpirTime = new SDVTime(1030);
            }
            else if (FogTimer > .30 && FogTimer <= .55)
            {
                FogExpirTime = new SDVTime(930);
            }
            else if (FogTimer <= .30)
            {
                FogExpirTime = new SDVTime(820);
            }

            this.FogExpirTime = FogExpirTime;
        }

        public void UpdateFog(int time, bool debug, IMonitor Monitor)
        {
            if (IsFogVisible())
            {
                if (FogTypeDark)
                {
                    if (time == (FogExpirTime - 30).ReturnIntTime())
                    {
                        if (debug) Monitor.Log("Now at T-30 minutes");
                        Game1.outdoorLight = new Color(190, 188, 186);
                    }

                    if (time == (FogExpirTime - 20).ReturnIntTime())
                    {
                        if (debug) Monitor.Log("Now at T-20 minutes");
                        Game1.outdoorLight = new Color(159, 156, 151);
                    }

                    if (time == (FogExpirTime - 10).ReturnIntTime())
                    {
                        if (debug) Monitor.Log("Now at T-10 minutes");
                        Game1.outdoorLight = new Color(110, 109, 107);
                    }
                }
                else
                {
                    if (time == (FogExpirTime - 30).ReturnIntTime())
                    {
                        if (debug) Monitor.Log("Now at T-30 minutes");
                        Game1.outdoorLight = new Color(168, 142, 99);
                    }

                    if (time == (FogExpirTime - 20).ReturnIntTime())
                    {
                        if (debug) Monitor.Log("Now at T-20 minutes");
                        Game1.outdoorLight = new Color(117, 142, 99);

                    }

                    if (time == (FogExpirTime - 10).ReturnIntTime())
                    {
                        if (debug) Monitor.Log("Now at T-10 minutes");
                        Game1.outdoorLight = new Color(110, 109, 107);
                    }
                }

                //it helps if you implement the fog cutoff!
                if (time >= FogExpirTime.ReturnIntTime())
                {
                    if (debug) Monitor.Log("Now at T-0 minutes");
                    this.AmbientFog = false;
                    this.FogTypeDark = false;
                    Game1.outdoorLight = Color.White;
                    FogAlpha = 0f; //fixes it lingering.
                }
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

        public void GetTodayWeather()
        {
            if (Game1.isRaining && !Game1.isSnowing && !Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_rain;

            if (Game1.isRaining && !Game1.isSnowing && Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_lightning;

            if (!Game1.isRaining && Game1.isSnowing && !Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_snow;

            if (!Game1.isRaining && !Game1.isSnowing && !Game1.isLightning && !Game1.isDebrisWeather)
                TodayWeather = Game1.weather_sunny;

            if (!Game1.isRaining && !Game1.isSnowing && !Game1.isLightning && Game1.isDebrisWeather)
                TodayWeather = Game1.weather_debris;

            if (Utility.isFestivalDay(SDate.Now().Day, SDate.Now().Season))
                TodayWeather = Game1.weather_festival;

            if (Game1.weddingToday)
                TodayWeather = Game1.weather_wedding;
        }

        ///DESCRIPTION SECTION

        /// <summary>
        /// This returns a description for use in the weather menu
        /// </summary>
        /// <param name="weather">The weather being looked at</param>
        /// <param name="Date">The date of the day</param>
        /// <param name="Dice">Randomizer object</param>
        /// <param name="Helper">Translation Helper</param>
        /// <returns>The string describing the weather</returns>
        public string GetDescText(int weather, SDate Date, MersenneTwister Dice, ITranslationHelper Helper)
        {
            string retString = "";

            switch (Date.Season)
            {
                case "spring":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.spring_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.spring_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.spring_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.spring_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.spring_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.spring_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.spring_festival1");
                    break;
                case "summer":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.summer_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.summer_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.summer_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.summer_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.summer_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.summer_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.summer_festival1");
                    break;
                case "fall":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.fall_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.fall_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.fall_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.fall_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.fall_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.fall_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.fall_festival1");
                    break;
                case "winter":
                    if (weather == Game1.weather_sunny)
                        retString = Helper.Get("weather-desc.winter_sunny1");
                    if (weather == Game1.weather_debris)
                        retString = Helper.Get("weather-desc.winter_debris1");
                    if (weather == Game1.weather_rain)
                        retString = Helper.Get("weather-desc.winter_rainy1");
                    if (weather == Game1.weather_lightning)
                        retString = Helper.Get("weather-desc.winter_stormy1");
                    if (weather == Game1.weather_snow)
                        retString = Helper.Get("weather-desc.winter_snowy1");
                    if (weather == Game1.weather_wedding)
                        retString = Helper.Get("weather-desc.winter_wedding1");
                    if (weather == Game1.weather_festival)
                        retString = Helper.Get("weather-desc.winter_festival1");
                    break;
                default:
                    retString = "";
                    break;
            }

            return retString;
        }

        public string GetHazardousText(ITranslationHelper Helper, SDate Date, MersenneTwister Dice)
        {
            string retString = "";
            switch (UnusualWeather)
            {
                case SpecialWeather.Blizzard:
                    retString = Helper.Get("weather-desc.winter_blizzard1");
                    break;
                case SpecialWeather.Thundersnow:
                    retString = Helper.Get("weather-desc.winter_thundersnow1");
                    break;
                case SpecialWeather.DryLightning:
                    if (Date.Season != "summer")
                    retString = Helper.Get("weather-desc.summer_thundersnow1");
                    else
                    retString = Helper.Get("weather-desc.nonsummer_drylightning1");
                    break;
                case SpecialWeather.Frost:
                    if (Date.Season == "spring")
                        retString = Helper.Get("weather-desc.spring_frost1");
                    else if (Date.Season == "fall")
                        retString = Helper.Get("weather-desc.fall_frost1");
                    break;
                case SpecialWeather.Heatwave:
                    retString = Helper.Get("weather-desc.summer_heatwave1");
                    break;
                case SpecialWeather.DryLightningAndHeatwave:
                    retString = Helper.Get("weather-desc.summer_litheatwave1");
                    break;
                default:
                    retString = "";
                    break;
            }

            return retString;
        }

        public string GetTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.GetTodayHigh().ToString("N1"),
                    lowTempC = this.GetTodayLow().ToString("N1"),
                    highTempF = this.GetTodayHighF().ToString("N1"),
                    lowTempF = this.GetTodayLowF().ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.GetTodayHigh().ToString("N1"),
                    lowTempC = this.GetTodayLow().ToString("N1")
                });

            return Temperature;
        }

        public string GetTomorrowTemperatureString(bool Scales, ITranslationHelper Helper)
        {
            string Temperature = "";

            if (Scales)
                Temperature = Helper.Get("weather-menu.temp_bothscales", new
                {
                    highTempC = this.GetTmrwHigh().ToString("N1"),
                    lowTempC = this.GetTmrwLow().ToString("N1"),
                    highTempF = this.GetTmrwHighF().ToString("N1"),
                    lowTempF = this.GetTmrwLowF().ToString("N1"),
                });
            else
                Temperature = Helper.Get("weather-menu.temp_onlyscales", new
                {
                    highTempC = this.GetTmrwHigh().ToString("N1"),
                    lowTempC = this.GetTmrwLow().ToString("N1")
                });

            return Temperature;
        }
    }
}
