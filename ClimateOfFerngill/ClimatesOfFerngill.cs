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
        public static ClimateConfig config { get; private set; }
        bool gameloaded { get; set; }
        int weatherAtStartDay { get; set; }
        FerngillWeather currWeather { get; set; }
        FerngillWeather tmrwWeather { get; set; }

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
            ClimateConfig config = helper.ReadConfig<ClimateConfig>();
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
        public string GetWeatherForecast()
        {
            string tvText = " ";

            if (tvText != null)
                Monitor.Log("AAAAAAH");
            else
                Monitor.Log("Why is this null?");

            string[] springRainText = new string[] { "It'll be a rainy day outside! Make sure to bring your coat. ", "It'll be a wet day outside. ", "It'll be a misty, wet day - make sure to pause when you can and enjoy it! "};
            string[] springStormText = new string[] { "Early showers bring summer flowers! It'll be stormy outside tommorow. ", "Expect some lightning tomorrow! ", "A storm front is blowing over the region, and it'll be here tommorrow. " };
            string[] springWindText = new string[] { "It'll be a blustery day outside tommorow. ", "A cold front is blowing through - if you have allergies, be careful. ", "The wind will be blowing through tommorow. " };
            string[] springClearWeather = new string[] { "A nice spring day, perfect for all those outside chores! ", "Clear and warm, tomorrow should be a perfect day. " };

            string[] summerRainText = new string[] { "A warm rain is expected tommorow. ", "There will be a warm refreshing rain as a front passes by tommorow. " };
            string[] summerStormText = new string[] { "Expect storms throughout the day. ", "A cold front is expected to pass through tommorow, bringing through a squall line. " };
            string[] summerClearWeather = new string[] { "It'll be a sweltering day tommorow. ", "Another perfect sunny day tommorow, perfect for hitting the beach.", "A hot and clear day dawns over the Valley. " };

            string[] fallRainText = new string[] { "Expect a cold rain tommorow as a low pressure goes overhead. ", "Moisture off the Gem Sea will make for a cold windy rain tommorow. " };
            string[] fallStormText = new string[] { "Expect storms throughout the day. ", "It'll be a cold and stormy day tomorrow. "};
            string[] fallWindText = new string[] { "It'll be a blustry cold day outside tommorow. ", "Expect blowing leaves - a cold front will be passing through tommorrow. "};
            string[] fallClearWeather = new string[] { "A cold day in the morning, with a warmer afternoon - clear. ", "Another autumn day in eastern Ferngill, expect a chilly and clear day. " };

            string[] winterSnowText = new string[] { "Winter continues it's relentless assualt - expect snow tommorow. ", "Moisture blowing off the Gem Sea tommorow - expecting snowfall for the Stardew Valley, more in the mountains. ", "A curtain of white will descend on the valley tommorow. " };
            string[] winterClearWeather = new string[] { "It'll be a clear cold day tommorow. ", "A cold winter day tommorow - keep warm!", "Another chilly clear day over the Valley as a High pressure moves overhead. " };

            string[] festivalWeather = new string[] { "It'll be good weather tommorow for the festival! Sunny and clear. " };
            string[] weddingWeather = new string[] { "It'll be good weather tommorow for a Pelican Town Wedding! Congratuatlions to the newlyweds. " };

            // Your custom weather channel string is created by this method
            int noLonger = VerifyValidTime(config.NoLongerDisplayToday) ? config.NoLongerDisplayToday : 1700;
            if (Game1.timeOfDay> noLonger) //don't display today's weather
                tvText = "The high for today is " + currWeather.todayHigh + "C, with the low being " + currWeather.todayLow +  "C. ";

            //temp warnings
            if (currWeather.todayHigh > 36)
                tvText = tvText + "It will be unusually hot outside. Stay hydrated. ";
            if (currWeather.todayHigh < -5)
                tvText = tvText + "There's a cold snap passing through. Stay warm. ";

            //get WeatherForTommorow and set text

            //generics
            if (Game1.weatherForTomorrow == Game1.weather_festival)
                tvText = tvText + festivalWeather.GetRandomItem(new Random());

            if (Game1.weatherForTomorrow == Game1.weather_wedding)
                tvText = tvText + festivalWeather.GetRandomItem(new Random());

            //spring
            if (Game1.currentSeason == "spring" && Game1.weatherForTomorrow == Game1.weather_debris)
                tvText = tvText + springWindText.GetRandomItem(new Random());

            if (Game1.currentSeason == "spring" && Game1.weatherForTomorrow == Game1.weather_sunny)
                tvText = tvText + springClearWeather.GetRandomItem(new Random());

            if (Game1.currentSeason == "spring" && Game1.weatherForTomorrow == Game1.weather_lightning)
                tvText = tvText + springStormText.GetRandomItem(new Random());

            if (Game1.currentSeason == "spring" && Game1.weatherForTomorrow == Game1.weather_rain)
                tvText = tvText + springRainText.GetRandomItem(new Random());

            //summer
            if (Game1.currentSeason == "summer" && Game1.weatherForTomorrow == Game1.weather_sunny)
                tvText = tvText + summerClearWeather.GetRandomItem(new Random());

            if (Game1.currentSeason == "summer" && Game1.weatherForTomorrow == Game1.weather_lightning)
                tvText = tvText + summerStormText.GetRandomItem(new Random());

            if (Game1.currentSeason == "summer" && Game1.weatherForTomorrow == Game1.weather_rain)
                tvText = tvText + summerRainText.GetRandomItem(new Random());

            //fall
            if (Game1.currentSeason == "fall" && Game1.weatherForTomorrow == Game1.weather_debris)
                tvText = tvText + fallWindText.GetRandomItem(new Random());

            if (Game1.currentSeason == "fall" && Game1.weatherForTomorrow == Game1.weather_sunny)
                tvText = tvText + fallClearWeather.GetRandomItem(new Random());

            if (Game1.currentSeason == "fall" && Game1.weatherForTomorrow == Game1.weather_lightning)
                tvText = tvText + fallStormText.GetRandomItem(new Random());

            if (Game1.currentSeason == "fall" && Game1.weatherForTomorrow == Game1.weather_rain)
                tvText = tvText + fallRainText.GetRandomItem(new Random());

            if (Game1.currentSeason == "fall" && Game1.weatherForTomorrow == Game1.weather_snow)
                tvText = tvText + "Winter is just around the bend, with snow predicted for tommorow!";

            //winter
            if (Game1.currentSeason == "winter" && Game1.weatherForTomorrow == Game1.weather_sunny)
                tvText = tvText + winterClearWeather.GetRandomItem(new Random());

            if (Game1.currentSeason == "winter" && Game1.weatherForTomorrow == Game1.weather_snow)
                tvText = tvText + winterSnowText.GetRandomItem(new Random());

            return tvText;
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
            if (currWeather.status == FerngillWeather.BLIZZARD) {
                Game1.hudMessages.Add(new HUDMessage("There's a dangerous blizzard out today. Be careful!"));
                return;
            }

            if (currWeather.status == FerngillWeather.HEATWAVE)
            {
                Game1.hudMessages.Add(new HUDMessage("A massive heatwave is sweeping the valley. Stay hydrated!"));
                return;
            }
        }

        public void EarlyFrost()
        {
            //this function is called if it's too cold to grow crops. Must be enabled.
        }

        public void TimeEvents_DayOfMonthChanged(object sender, StardewModdingAPI.Events.EventArgsIntChanged e)
        {
            if (gameloaded == false) return;
            UpdateWeather();
        }

        public void PlayerEvents_LoadedGame(object sender, StardewModdingAPI.Events.EventArgsLoadedGameChanged e)
        {
            gameloaded = true;
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
            if (weatherAtStartDay != Game1.weatherForTomorrow)
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

            Random rng = new Random(Guid.NewGuid().GetHashCode());
            double genNumber = rng.NextDouble();
            double rainChance = 0, stormChance = 0, windChance = 0;

            //first: spring
            #region SpringWeather
            if (Game1.currentSeason == "spring")
            {
                if (Game1.dayOfMonth < 10)
                {
                    //temperature
                    currWeather.todayHigh = rng.Next(1,8) + 8;
                    currWeather.GetLowFromHigh(rng.Next(1,3) + 3);

                    if (config.ClimateType == "arid") currWeather.AlterTemps(5);

                    //rain, snow, windy chances
                    stormChance = .15;
                    windChance = .25;
                    rainChance = .3 + (Game1.dayOfMonth * .0278);

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = rng.Next(1, 6) + 14;
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    if (config.ClimateType == "arid") currWeather.AlterTemps(5);

                    //rain, snow, windy chances
                    stormChance = .2;
                    windChance = .15 + (Game1.dayOfMonth * .01);
                    rainChance = .2 + (Game1.dayOfMonth * .01);

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = rng.Next(1, 6) + 20;
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    if (config.ClimateType == "arid") currWeather.AlterTemps(5);

                    //rain, snow, windy chances
                    stormChance = .3;
                    windChance = .05 + (Game1.dayOfMonth * .01);
                    rainChance = .2;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = rng.Next(1, 5) + 26;
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    //summer adjustment
                    currWeather.AlterTemps(rng.Next(0, 6));

                    if (config.ClimateType == "arid" || config.ClimateType == "monsoon") currWeather.AlterTemps(6);

                    //rain, snow, windy chances
                    stormChance = .45;
                    windChance = 0; //cannot wind during summer
                    rainChance = .15;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = rng.Next(1, 5) + 31;
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    //summer adjustment
                    currWeather.AlterTemps(rng.Next(0, 6));

                    if (config.ClimateType == "arid" || config.ClimateType == "monsoon") currWeather.AlterTemps(6);

                    //rain, snow, windy chances
                    stormChance = .6;
                    windChance = 0; //cannot wind during summer
                    rainChance = .15;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    currWeather.todayHigh = 22 + (int)Math.Floor(Game1.dayOfMonth * 1.56) + rng.Next(0,3);
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    //summer adjustment
                    currWeather.AlterTemps(rng.Next(0, 6));

                    if (config.ClimateType == "arid" || config.ClimateType == "monsoon") currWeather.AlterTemps(6);

                    //rain, snow, windy chances
                    stormChance = .45;
                    windChance = 0; //cannot wind during summer
                    rainChance = .3;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = 16 + (int)Math.Floor(Game1.dayOfMonth * .667) + rng.Next(0, 2);
                    currWeather.GetLowFromHigh(rng.Next(1, 6) + 4);

                    if (config.ClimateType == "arid") currWeather.AlterTemps(2);

                    //rain, snow, windy chances
                    stormChance = .33;
                    windChance = 0 + (Game1.dayOfMonth * .044); 
                    rainChance = .3 + (Game1.dayOfMonth * .01111);

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = 9 + (int)Math.Floor(Game1.dayOfMonth * .778) + rng.Next(0, 2);
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3);

                    if (config.ClimateType == "arid") currWeather.AlterTemps(2);

                    //rain, snow, windy chances
                    stormChance = .33;
                    windChance = .4 + (Game1.dayOfMonth * .022);
                    rainChance =  1- windChance;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = 2 + (int)Math.Floor(Game1.dayOfMonth * .333) + rng.Next(0, 4);
                    currWeather.GetLowFromHigh(rng.Next(1, 3) + 3, 1);

                    if (Game1.dayOfMonth == 28 && config.AllowSnowOnFall28)
                    {
                        currWeather.todayHigh = 2;
                        currWeather.todayLow = -1;
                    }

                    if (config.ClimateType == "arid") currWeather.AlterTemps(2);

                    //rain, snow, windy chances
                    stormChance = .33;
                    windChance = .1 + Game1.dayOfMonth * .044; //cannot wind during summer
                    rainChance = .5;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = -2 + (int)Math.Floor(Game1.dayOfMonth * .889) + rng.Next(0, 3);
                    currWeather.GetLowFromHigh(rng.Next(1, 4));

                    //rain, snow, windy chances
                    rainChance = .6;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.111) + rng.Next(0, 3);
                    currWeather.GetLowFromHigh(rng.Next(1, 4));

                    //rain, snow, windy chances
                    rainChance = .75;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
                    //temperature
                    currWeather.todayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.222) + rng.Next(0, 3);
                    currWeather.GetLowFromHigh(rng.Next(1, 4));

                    //rain, snow, windy chances
                    rainChance = .6;

                    //climate changes to the rain.
                    switch (config.ClimateType)
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
            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && config.AllowSnowOnFall28)
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.

            weatherAtStartDay = Game1.weatherForTomorrow;
        }

        private bool CanWeStorm()
        {
            if (Game1.year == 1 && Game1.currentSeason == "spring") return config.AllowStormsFirstSpring;
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

