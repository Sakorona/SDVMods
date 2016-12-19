using System;
using StardewModdingAPI;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using StardewValley;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Menus;

namespace ClimatesOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig ModConfig { get; private set; }
        bool gameloaded { get; set; }
        int weatherAtStartDay { get; set; }
        FerngillWeather currWeather { get; set; } = FerngillWeather.None;

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
            ModConfig = helper.ReadConfig<ClimateConfig>();
            PlayerEvents.LoadedGame += PlayerEvents_LoadedGame;
            TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            TryHookTelevision();
        }

        public static void TryHookTelevision()
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
    
        public static void InterceptCallback(Farmer who, string answer)
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
        public static void NextScene()
        {
            TVScreen.SetValue(Target, new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(497, 305, 42, 28), 9999f, 1, 999999, Target.getScreenPosition(), false, false, (float)((double)(Target.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, Target.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false));
            Game1.drawObjectDialogue(Game1.parseText(GetWeatherForecast()));
            TVMethodOverlay.Invoke(Target, null);
            Game1.afterDialogues = Target.proceedToNextScene;
        }
        public static string GetWeatherForecast()
        {
            // Your custom weather channel string is created by this method
            return "";
        }
        

        private void LogEvent(string msg)
        {
           this.Monitor.Log("[Climate] " + msg);
        }

        public void checkForDangerousWeather(bool hud = true)
        {
            if (currWeather == FerngillWeather.Blizzard) {
                Game1.hudMessages.Add(new HUDMessage("There's a dangerous blizzard out today. Be careful!"));
                return;
            }

            if (currWeather == FerngillWeather.Heatwave)
            {
                Game1.hudMessages.Add(new HUDMessage("A massive heatwave is sweeping the valley. Stay hydrated!"));
                return;
            }

            //Game1.hudMessages.Add(new HUDMessage("No abnormally dangerous weather.",2));
        }

        public void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            //get weather.

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

            switch (Game1.currentSeason)
            {
                case "spring":

  
                default:
                    Game1.weatherForTomorrow = Game1.weather_sunny; //fail safe.
                    break;
            }

            overrideWeather();
            weatherAtStartDay = Game1.weatherForTomorrow;
        }

        private bool CanWeStorm()
        {
            if (Game1.year == 1 && Game1.currentSeason == "spring") return ModConfig.AllowStormsFirstSpring;
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

        private void overrideWeather()
        {
            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && ModConfig.AllowSnowOnFall28)
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.
        }


    }
}

