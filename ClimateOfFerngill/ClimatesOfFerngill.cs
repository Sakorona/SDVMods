using System;
using System.Reflection;

// ???
using StardewModdingAPI;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ClimatesOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig Config { get; private set; }
        bool GameLoaded { get; set; }
        int WeatherAtStartOfDay { get; set; }
        FerngillWeather CurrWeather { get; set; }
        FerngillWeather TomorrowWeather { get; set; }

        //tv overloading
        private static FieldInfo Field = typeof(GameLocation).GetField("afterQuestion", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo TVChannel = typeof(TV).GetField("currentChannel", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo TVScreen = typeof(TV).GetField("screen", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo TVScreenOverlay = typeof(TV).GetField("screenOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo TVMethod = typeof(TV).GetMethod("getWeatherChannelOpening", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo TVMethodOverlay = typeof(TV).GetMethod("setWeatherOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
        private static GameLocation.afterQuestionBehavior Callback;
        private static TV Target;

        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ClimateConfig>();
            PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
            TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            TryHookTelevision();
        }

        public void TryHookTelevision()
        {
            if (Game1.currentLocation != null && Game1.currentLocation is DecoratableLocation && Game1.activeClickableMenu != null && Game1.activeClickableMenu is DialogueBox)
            {
                Callback = (GameLocation.afterQuestionBehavior)Field.GetValue(Game1.currentLocation);
                if (Callback != null && Callback.Target.GetType() == typeof(TV))
                {
                    Field.SetValue(Game1.currentLocation, new GameLocation.afterQuestionBehavior(InterceptCallback));
                    Target = (TV)Callback.Target;
                }
            }
        }
    
        public void InterceptCallback(Farmer who, string answer)
        {
            if (answer != "Weather")
            {
                Callback(who, answer);
                return;
            }
            TVChannel.SetValue(Target, 2);
            TVScreen.SetValue(Target, new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(413, 305, 42, 28), 150f, 2, 999999, Target.getScreenPosition(), false, false, (float)((double)(Target.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, Target.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false));
            Game1.drawObjectDialogue(Game1.parseText((string)TVMethod.Invoke(Target, null)));
            Game1.afterDialogues = NextScene;
        }
        public void NextScene()
        {
            TVScreen.SetValue(Target, new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(497, 305, 42, 28), 9999f, 1, 999999, Target.getScreenPosition(), false, false, (float)((double)(Target.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, Target.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false));
            Game1.drawObjectDialogue(Game1.parseText(GetWeatherForecast()));
            TVMethodOverlay.Invoke(Target, null);
            Game1.afterDialogues = Target.proceedToNextScene;
        }

        public string DisplayTemperature(int temp, string tempGauge)
        {
            //base temps are always in celsius
            if (tempGauge == "celsius")
            {
                return temp + " C";
            }

            if (tempGauge == "kelvin")
            {
                return (temp + 273.15) + " K";
            }

            if (tempGauge == "rankine")
            {
                double tmpTemp = (temp + 273.15) * (9 / 5);
                return string.Format("{0:0.00}", tmpTemp) + " Ra";
            }

            if (tempGauge == "farenheit")
            {
                double tmpTemp = temp * (9 / 5) + 32;
                return string.Format("{0:0.00}", tmpTemp) + " F";
            }

            if (tempGauge == "romer")
            {
                return string.Format("{0:0.00}", (temp * (40 / 21) + 7.5)) + " Ro";
            }

            if (tempGauge == "delisle")
            {
                return string.Format("{0:0.00}", ((100 - temp) * 1.5)) + " De";
            }

            if (tempGauge == "reaumur")
            {
                return string.Format("{0:0.00}", temp * .8) + " Re";
            }

            return "ERROR";
        }

        public string GetWeatherForecast()
        {
            string tvText = " ";
            Random rng = new Random();

            if (CurrWeather == null)
            {
                CurrWeather = new FerngillWeather();
                SetTemperature(rng);
            }



            // Your custom weather channel string is created by this method
            int noLonger = VerifyValidTime(Config.NoLongerDisplayToday) ? Config.NoLongerDisplayToday : 1700;

            //BUG #10 - >_>
            if (Game1.timeOfDay < noLonger) //don't display today's weather 
            {
                tvText = "The high for today is " + DisplayTemperature(CurrWeather.todayHigh, Config.TempGauge) + ", with the low being " + DisplayTemperature(CurrWeather.todayLow, Config.TempGauge) + ". ";

                //temp warnings
                if (CurrWeather.todayHigh > 36 && Game1.timeOfDay < 1900)
                    tvText = tvText + "It will be unusually hot outside. Stay hydrated. ";
                if (CurrWeather.todayHigh < -5)
                    tvText = tvText + "There's a cold snap passing through. Stay warm. ";

                //today weather
                tvText = tvText + GetWeatherDesc(rng, GetTodayWeather());

                //get WeatherForTommorow and set text
                tvText = tvText + "Tommorow, ";
            }

            if (CurrWeather.todayHigh > 36 && Game1.timeOfDay < 1900)
                tvText = "It will be unusually hot outside. Stay hydrated. ";
            if (CurrWeather.todayHigh < -5)
                tvText = "There's a cold snap passing through. Stay warm. ";

            //tommorow weather
            tvText = tvText + GetWeatherDesc(rng, Game1.weatherForTomorrow);


            return tvText;
        }

        public int GetTodayWeather()
        {
            if (Game1.isRaining)
            {
                if (Game1.isLightning) return Game1.weather_lightning;
                else return Game1.weather_rain;
            }

            if (Game1.isSnowing) return Game1.weather_snow;
            if (Game1.isDebrisWeather) return Game1.weather_debris;

            if (Game1.weddingToday == true)
                return Game1.weather_wedding;

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
                return Game1.weather_festival;

            return Game1.weather_sunny;
        }

        public string GetWeatherDesc(Random rng, int weather)
        {

            string[] springRainText = new string[] { "It'll be a rainy day outside! Make sure to bring your coat. ", "It'll be a wet day outside. ", "It'll be a misty, wet day - make sure to pause when you can and enjoy it! " };
            string[] springStormText = new string[] { "Early showers bring summer flowers! It'll be stormy outside. ", "Expect some lightning  outside -  be careful! ", "A storm front is blowing over the region, bringing rain and lightning. " };
            string[] springWindText = new string[] { "It'll be a blustery day outside. ", "A cold front is blowing through - if you have allergies, be careful. ", "The wind will be blowing through . " };
            string[] springClearWeather = new string[] { "A nice spring day, perfect for all those outside chores! ", "Clear and warm, it should be a perfect day. " };

            string[] summerRainText = new string[] { "A warm rain is expected. ", "There will be a warm refreshing rain as a front passes by. " };
            string[] summerStormText = new string[] { "Expect storms throughout the day. ", "A cold front is expected to pass through, bringing through a squall line. " };
            string[] summerClearWeather = new string[] { "It'll be a sweltering day. ", "Another perfect sunny day, perfect for hitting the beach.", "A hot and clear day dawns over the Valley. " };

            string[] fallRainText = new string[] { "Expect a cold rain as a low pressure goes overhead. ", "Moisture off the Gem Sea will make for a cold windy rain. " };
            string[] fallStormText = new string[] { "Expect storms throughout the day. ", "It'll be a cold and stormy day . " };
            string[] fallWindText = new string[] { "It'll be a blustry cold day outside . ", "Expect blowing leaves - a cold front will be passing through. " };
            string[] fallClearWeather = new string[] { "A cold day in the morning, with a warmer afternoon - clear. ", "Another autumn day in eastern Ferngill, expect a chilly and clear day. " };

            string[] winterSnowText = new string[] { "Winter continues it's relentless assualt - expect snow. ", "Moisture blowing off the Gem Sea - expecting snowfall for the Stardew Valley, more in the mountains. ", "A curtain of white will descend on the valley. " };
            string[] winterClearWeather = new string[] { "It'll be a clear cold day . ", "A cold winter day - keep warm!", "Another chilly clear day over the Valley as a High pressure moves overhead. " };

            string[] festivalWeather = new string[] { "It'll be good weather for the festival! Sunny and clear. " };
            string[] weddingWeather = new string[] { "It'll be good weather for a Pelican Town Wedding! Congratuatlions to the newlyweds. " };
            if (weather == Game1.weather_festival)
                return festivalWeather.GetRandomItem(rng);

            if (weather == Game1.weather_wedding)
                return festivalWeather.GetRandomItem(rng);

            //spring
            if (Game1.currentSeason == "spring" && weather == Game1.weather_debris)
                return springWindText.GetRandomItem(rng);

            if (Game1.currentSeason == "spring" && weather == Game1.weather_sunny)
                return springClearWeather.GetRandomItem(rng);

            if (Game1.currentSeason == "spring" && weather == Game1.weather_lightning)
                return springStormText.GetRandomItem(rng);

            if (Game1.currentSeason == "spring" && weather == Game1.weather_rain)
                return springRainText.GetRandomItem(rng);

            //summer
            if (Game1.currentSeason == "summer" && weather == Game1.weather_sunny)
                return summerClearWeather.GetRandomItem(rng);

            if (Game1.currentSeason == "summer" && weather == Game1.weather_lightning)
                return summerStormText.GetRandomItem(rng);

            if (Game1.currentSeason == "summer" && weather == Game1.weather_rain)
                return summerRainText.GetRandomItem(rng);

            //fall
            if (Game1.currentSeason == "fall" && weather == Game1.weather_debris)
                return fallWindText.GetRandomItem(rng);

            if (Game1.currentSeason == "fall" && weather == Game1.weather_sunny)
                return fallClearWeather.GetRandomItem(rng);

            if (Game1.currentSeason == "fall" && weather == Game1.weather_lightning)
                return fallStormText.GetRandomItem(rng);

            if (Game1.currentSeason == "fall" && weather == Game1.weather_rain)
                return fallRainText.GetRandomItem(rng);

            if (Game1.currentSeason == "fall" && weather == Game1.weather_snow && Game1.dayOfMonth == 27)
                return "Winter is just around the bend, with snow predicted for tommorow!";

            //winter
            if (Game1.currentSeason == "winter" && weather == Game1.weather_sunny)
                return winterClearWeather.GetRandomItem(rng);

            if (Game1.currentSeason == "winter" && weather == Game1.weather_snow)
                return winterSnowText.GetRandomItem(rng);

            return "Angry suns descend on us! Run! (ERROR)";
        }

        private bool VerifyValidTime(int time)
        {
            //basic bounds first
            if (time >= 0600 && time <= 2600)
                return false;
            if ((time % 100) > 50)
                return false;

            return true;
        }
        

        private void LogEvent(string msg)
        {
           this.Monitor.Log("[Climate] " + msg);
        }

        public void checkForDangerousWeather(bool hud = true)
        {
            if (CurrWeather.status == FerngillWeather.BLIZZARD) {
                Game1.hudMessages.Add(new HUDMessage("There's a dangerous blizzard out today. Be careful!"));
                return;
            }

            if (CurrWeather.status == FerngillWeather.HEATWAVE)
            {
                Game1.hudMessages.Add(new HUDMessage("A massive heatwave is sweeping the valley. Stay hydrated!"));
                return;
            }
        }

        public void EarlyFrost()
        {
            //this function is called if it's too cold to grow crops. Must be enabled.
        }

        public void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            if (GameLoaded == false) return;
            UpdateWeather();
        }

        public void PlayerEvents_LoadedGame(object sender, EventArgsLoadedGameChanged e)
        {
            GameLoaded = true;
        }

        void UpdateWeather(){
            #region WeatherChecks
            //sanity check - wedding
            if (Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                LogEvent("There is no Alanis Morissetting here. Enjoy your wedding.");
                return;
            }

            //sanity check - festival
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.weatherForTomorrow = Game1.weather_festival;
                Game1.questOfTheDay = null;
                return;
            }

            if (Game1.weatherForTomorrow == Game1.weather_festival)
            {
                LogEvent("A festival!");
                return;
            }

            //rain totem
            if (WeatherAtStartOfDay != Game1.weatherForTomorrow)
            {
                if (Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    LogEvent("Rain totem used, aborting weather change");
                    return;
                }
            }

            //TV forces.
            fixTV();
            #endregion

            //now on to the main program
            CurrWeather = new FerngillWeather();
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            double genNumber = rng.NextDouble();
            double rainChance = 0, stormChance = 0, windChance = 0;
            SetTemperature(rng);

            //first: spring
            #region SpringWeather
            if (Game1.currentSeason == "spring")
            {
                if (Game1.dayOfMonth < 10)
                {
                    //rain, snow, windy chances
                    stormChance = .15;
                    windChance = .25;
                    rainChance = .3 + (Game1.dayOfMonth * .0278);

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .25;
                            break;
                        case "dry":
                            rainChance = .3;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = .95;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }
                    else if (rng.NextDouble() < windChance) Game1.weatherForTomorrow = Game1.weather_debris;

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
                {
                    //rain, snow, windy chances
                    stormChance = .2;
                    windChance = .15 + (Game1.dayOfMonth * .01);
                    rainChance = .2 + (Game1.dayOfMonth * .01);

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .25;
                            break;
                        case "dry":
                            rainChance = .3;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = .95;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }
                    else if (rng.NextDouble() < windChance) Game1.weatherForTomorrow = Game1.weather_debris;

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 18)
                {
                    //rain, snow, windy chances
                    stormChance = .3;
                    windChance = .05 + (Game1.dayOfMonth * .01);
                    rainChance = .2;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .25;
                            break;
                        case "dry":
                            rainChance = .3;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = .95;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }
                    else if (rng.NextDouble() < windChance) Game1.weatherForTomorrow = Game1.weather_debris;

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
            }
            #endregion
            #region SummerWeather
            if (Game1.currentSeason == "summer")
            {
                if (Game1.dayOfMonth < 10)
                {
                    //rain, snow, windy chances
                    stormChance = .45;
                    windChance = 0; //cannot wind during summer
                    rainChance = .15;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .05;
                            break;
                        case "dry":
                            rainChance = .20;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = rainChance - .1;
                            stormChance = .8;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
                {
                    //rain, snow, windy chances
                    stormChance = .6;
                    windChance = 0; //cannot wind during summer
                    rainChance = .15;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .05;
                            break;
                        case "dry":
                            rainChance = .10;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = rainChance - .1;
                            stormChance = .8;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 18)
                {
                    //temperature
                    //currWeather.todayHigh = rng.Next(1, 14) + 22;
                    CurrWeather.todayHigh = 22 + (int)Math.Floor(Game1.dayOfMonth * 1.56) + rng.Next(0,3);
                    CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    //summer adjustment
                    CurrWeather.AlterTemps(rng.Next(0, 6));

                    if (Config.ClimateType == "arid" || Config.ClimateType == "monsoon") CurrWeather.AlterTemps(6);

                    //rain, snow, windy chances
                    stormChance = .45;
                    windChance = 0; //cannot wind during summer
                    rainChance = .3;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .05;
                            break;
                        case "dry":
                            rainChance = .15;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = rainChance - .1;
                            stormChance = .8;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
            }
            #endregion
            #region AutumnWeather
            if (Game1.currentSeason == "fall")
            {
                if (Game1.dayOfMonth < 10)
                {
                    //rain, snow, windy chances
                    stormChance = .33;
                    windChance = 0 + (Game1.dayOfMonth * .044); 
                    rainChance = .3 + (Game1.dayOfMonth * .01111);

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .05;
                            windChance = .3;
                            break;
                        case "dry":
                            rainChance = .20;
                            windChance = .3;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = .1 + (Game1.dayOfMonth * .08889);
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }
                    else if (rng.NextDouble() < windChance) Game1.weatherForTomorrow = Game1.weather_debris;

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
                {
                    //rain, snow, windy chances
                    stormChance = .33;
                    windChance = .4 + (Game1.dayOfMonth * .022);
                    rainChance =  1- windChance;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .05;
                            break;
                        case "dry":
                            rainChance = .25;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = .9;
                            windChance = .1;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }

                    Game1.weatherForTomorrow = Game1.weather_debris;

                    //Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 18)
                {
                    //rain, snow, windy chances
                    stormChance = .33;
                    windChance = .1 + Game1.dayOfMonth * .044; //cannot wind during summer
                    rainChance = .5;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .05;
                            windChance = .3;
                            break;
                        case "dry":
                            rainChance = .25;
                            windChance = .3;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = .9;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance)
                    {
                        if (rng.NextDouble() < stormChance) Game1.weatherForTomorrow = Game1.weather_lightning;
                        else Game1.weatherForTomorrow = Game1.weather_rain;
                    }
                    else if (rng.NextDouble() < windChance) Game1.weatherForTomorrow = Game1.weather_debris;

                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
            }
            #endregion
            #region WinterWeather
            if (Game1.currentSeason == "winter")
            {
                if (Game1.dayOfMonth < 10)
                {
                    //rain, snow, windy chances
                    rainChance = .6;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .25;
                            break;
                        case "dry":
                            rainChance = .30;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = 1;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance) Game1.weatherForTomorrow = Game1.weather_snow;
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }

                if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
                {
                    //rain, snow, windy chances
                    rainChance = .75;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .25;
                            break;
                        case "dry":
                            rainChance = .30;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = 1;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance) Game1.weatherForTomorrow = Game1.weather_snow;
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
                if (Game1.dayOfMonth > 18)
                {
                    //rain, snow, windy chances
                    rainChance = .6;

                    //climate changes to the rain.
                    switch (Config.ClimateType)
                    {
                        case "arid":
                            rainChance = .25;
                            break;
                        case "dry":
                            rainChance = .30;
                            break;
                        case "wet":
                            rainChance = rainChance + .05;
                            break;
                        case "monsoon":
                            rainChance = 1;
                            break;
                        default:
                            break;
                    }

                    //sequence - rain (Storm), wind, sun
                    if (rng.NextDouble() < rainChance) Game1.weatherForTomorrow = Game1.weather_snow;
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                }
            }
            #endregion
            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && Config.AllowSnowOnFall28)
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.

            WeatherAtStartOfDay = Game1.weatherForTomorrow;
        }

        public void SetTemperature(Random rng)
        {
            if (Game1.currentSeason == "spring" && Game1.dayOfMonth < 10)
            {
                CurrWeather.todayHigh = rng.Next(1, 8) + 8;
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                if (Config.ClimateType == "arid") CurrWeather.AlterTemps(5);
            }

            if (Game1.currentSeason == "spring" && Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                CurrWeather.todayHigh = rng.Next(1, 6) + 14;
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                if (Config.ClimateType == "arid") CurrWeather.AlterTemps(5);
            }
            if (Game1.currentSeason == "spring" && Game1.dayOfMonth > 18)
            {
                CurrWeather.todayHigh = rng.Next(1, 6) + 20;
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                if (Config.ClimateType == "arid") CurrWeather.AlterTemps(5);
            }

            //SUMMER
            if (Game1.currentSeason == "summer" && Game1.dayOfMonth < 10)
            {
                CurrWeather.todayHigh = rng.Next(1, 8) + 26;
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                CurrWeather.AlterTemps(rng.Next(0, 6));
                if (Config.ClimateType == "arid" || Config.ClimateType == "monsoon")
                    CurrWeather.AlterTemps(6);
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                CurrWeather.todayHigh = rng.Next(1, 6) + 31;
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                CurrWeather.AlterTemps(rng.Next(0, 6));
                if (Config.ClimateType == "arid" || Config.ClimateType == "monsoon")
                    CurrWeather.AlterTemps(6);
            }
            if (Game1.currentSeason == "summer" && Game1.dayOfMonth > 18)
            {
                CurrWeather.todayHigh = 22 + (int)Math.Floor(Game1.dayOfMonth * 1.56) + rng.Next(0, 3);
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                CurrWeather.AlterTemps(rng.Next(0, 6));
                if (Config.ClimateType == "arid" || Config.ClimateType == "monsoon")
                    CurrWeather.AlterTemps(6);
            }

            //AUTUMN
            if (Game1.currentSeason == "fall" && Game1.dayOfMonth < 10)
            {
                CurrWeather.todayHigh = 16 + (int)Math.Floor(Game1.dayOfMonth * .667) + rng.Next(0, 2);
                CurrWeather.GetLowFromHigh(rng.Next(1, 6) + 4);

                if (Config.ClimateType == "arid") CurrWeather.AlterTemps(2);
            }

            if (Game1.currentSeason == "fall" && Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                CurrWeather.todayHigh = 9 + (int)Math.Floor(Game1.dayOfMonth * .778) + rng.Next(0, 2);
                CurrWeather.GetLowFromHigh(rng.Next(1, 6) + 3);

                if (Config.ClimateType == "arid") CurrWeather.AlterTemps(2);
            }

            if (Game1.currentSeason == "fall" && Game1.dayOfMonth > 18)
            {
                CurrWeather.todayHigh = 2 + (int)Math.Floor(Game1.dayOfMonth * .667) + rng.Next(0, 2);
                CurrWeather.GetLowFromHigh(rng.Next(1, 3) + 3, 1);

                if (Config.ClimateType == "arid") CurrWeather.AlterTemps(2);

                if (Game1.dayOfMonth == 28 && Config.AllowSnowOnFall28)
                {
                    CurrWeather.todayHigh = 2;
                    CurrWeather.todayLow = -1;
                }
            }

            //WINTER
            if (Game1.currentSeason == "winter" && Game1.dayOfMonth < 10)
            {
                CurrWeather.todayHigh = -2 + (int)Math.Floor(Game1.dayOfMonth * .889) + rng.Next(0, 3);
                CurrWeather.GetLowFromHigh(rng.Next(1, 4));
            }

            if (Game1.currentSeason == "winter" && Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                CurrWeather.todayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.111) + rng.Next(0, 3);
                CurrWeather.GetLowFromHigh(rng.Next(1, 4));
            }

            if (Game1.currentSeason == "winter" && Game1.dayOfMonth > 18)
            {
                CurrWeather.todayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.222) + rng.Next(0, 3);
                CurrWeather.GetLowFromHigh(rng.Next(1, 4));
            }
        }

        private bool CanWeStorm()
        {
            if (Game1.year == 1 && Game1.currentSeason == "spring") return Config.AllowStormsFirstSpring;
            else return true;
        }

        private void fixTV()
        {
            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 1)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return;
            }

            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 2)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return;
            }

            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return;
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 25)
            {
                Game1.weatherForTomorrow = Game1.weather_lightning;
                return;
            }
        }

    }

    public static class OurExtensions
    {
        public static string GetRandomItem(this string[] array, Random r)
        {
            int l = array.Length;

            return array[r.Next(l)];
        }
    }
}

