using System;
using FerngillDynamicRainAndWind.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace FerngillDynamicRainAndWind
{
    public partial class RainAndWind : Mod
    {
        public static Icons OurIcons;
        public static IReflectionHelper Reflection;
        public static IMonitor Logger;
        
        //rain tracking stuff
        internal int CurrentRainAmt;
        internal int TimeElapsed;
        internal FDRAWConfig ModOptions;
        internal bool IsVariableRain;

        public override void Entry(IModHelper helper)
        {
            ModOptions = Helper.ReadConfig<FDRAWConfig>();
            OurIcons = new Icons(Helper.ModContent);
            Logger = Monitor;
            Reflection = Helper.Reflection;
            TimeElapsed = 0;

            var harmony =  new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), "drawAboveAlwaysFrontLayer"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(GameLocationPatches), "DAAFLTranspiler")));
            Monitor.Log("Patching GameLocation::drawAboveAlwaysFrontLayer with a Transpiler.");

            harmony.Patch(
                AccessTools.Method(typeof(Game1), "drawWeather"),
                new HarmonyMethod(AccessTools.Method(typeof(Game1Patches), "DrawWeatherPrefix")));
            Monitor.Log("Patching Game1::drawWeather with a prefix method.");

            harmony.Patch(
                AccessTools.Method(typeof(Game1), "updateWeather"),
                new HarmonyMethod(AccessTools.Method(typeof(Game1Patches), "UpdateWeatherPrefix")));
            Monitor.Log("Patching Game1::updateWeather with a prefix method."); 

            harmony.Patch(
                AccessTools.Constructor(AccessTools.TypeByName("StardewValley.WeatherDebris"), new[] { typeof(Vector2), typeof(int), typeof(float), typeof(float), typeof(float) }),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(WeatherDebrisPatches), "CtorPostfix")));
            Monitor.Log("Patching WeatherDebris's constructor method with a postfix method.");

            harmony.Patch(
                AccessTools.Method(typeof(WeatherDebris), "draw"),
                new HarmonyMethod(AccessTools.Method(typeof(WeatherDebrisPatches), "DrawPrefix")));
            Monitor.Log("Patching WeatherDebris::draw with a prefix method.");

            harmony.Patch(
                AccessTools.Method(typeof(WeatherDebris), "update", new[] { typeof(bool) }),
                new HarmonyMethod(AccessTools.Method(typeof(WeatherDebrisPatches), "UpdatePrefix")));
            Monitor.Log("Patching WeatherDebris::update with a prefix method.");

            harmony.Patch(
                AccessTools.Method(typeof(Game1), "randomizeRainPositions"),
                    new HarmonyMethod(AccessTools.Method(typeof(Game1Patches), "randomizeRainPositionsPrefix")));
            Monitor.Log("Patching Game1::RandomizeRainPositions with a prefix method");

            helper.ConsoleCommands
                .Add("debug_vrainc", "Set Rain Amt.", SetRainAmt)
                .Add("debug_showrain", "Show Rain Amt", ShowRainAmt)
                .Add("world_setweather", "This sets the current weather for the default location", SetWeather);
                

            //let's get started
            helper.Events.GameLoop.TimeChanged += Every10Minutes;
            helper.Events.GameLoop.DayStarted += OnNewDay;
            helper.Events.GameLoop.DayEnding += OnEndDay;
            Helper.Events.GameLoop.ReturnedToTitle += ClosingGame;

            //MP sync
            Helper.Events.Multiplayer.ModMessageReceived += ParseMessage;
        }

        private void SetWeather(string arg1, string[] arg2)
        {
            switch (arg2[0])
            {
                case "sunny":
                    Game1.isRaining = Game1.isLightning = Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isRaining.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isLightning.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isSnowing.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isDebrisWeather.Value = false;
                    Game1.debrisWeather.Clear();
                    Game1.updateWeather(Game1.currentGameTime);
                    Game1.currentLocation.UpdateWhenCurrentLocation(Game1.currentGameTime);
                    Console.WriteLine("Weather updated to sunny");
                    break;
                case "rainy":
                    Game1.isLightning = Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.isRaining = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isRaining.Value = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isLightning.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isSnowing.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isDebrisWeather.Value = false;
                    Game1.debrisWeather.Clear();
                    Game1.updateWeather(Game1.currentGameTime);
                    Game1.currentLocation.UpdateWhenCurrentLocation(Game1.currentGameTime);
                    Console.WriteLine("Weather updated to rainy");
                    break;
                case "snowy":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isRaining.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isLightning.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isSnowing.Value = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isDebrisWeather.Value = false;
                    Game1.debrisWeather.Clear();
                    Game1.updateWeather(Game1.currentGameTime);
                    Game1.currentLocation.UpdateWhenCurrentLocation(Game1.currentGameTime);
                    Console.WriteLine("Weather updated to snowy");
                    break;
                case "windy":
                    Game1.isRaining = Game1.isLightning = Game1.isSnowing = false;
                    Game1.isDebrisWeather = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isRaining.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isLightning.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isSnowing.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isDebrisWeather.Value = true;
                    Game1.debrisWeather.Clear();
                    Game1.populateDebrisWeatherArray();
                    Game1.updateWeather(Game1.currentGameTime);
                    Game1.currentLocation.UpdateWhenCurrentLocation(Game1.currentGameTime);
                    Console.WriteLine("Weather updated to windy");
                    break;
                case "stormy":
                    Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.isRaining = Game1.isLightning = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isRaining.Value = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isLightning.Value = true;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isSnowing.Value = false;
                    Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext())
                        .isDebrisWeather.Value = false;
                    Game1.debrisWeather.Clear();
                    Game1.updateWeather(Game1.currentGameTime);
                    Game1.currentLocation.UpdateWhenCurrentLocation(Game1.currentGameTime);
                    Console.WriteLine("Weather updated to stormy");
                    break;
            }

            Game1.updateWeatherIcon();
        }

        private void OnEndDay(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            IsVariableRain = false;
            TimeElapsed = 0;
            CurrentRainAmt = 0;

            //resize the primary array
            Array.Resize(ref Game1.rainDrops, 70);
        }

        private void ParseMessage(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (Game1.IsMasterGame)
                return;

            if (e.Type == "UpdateRain")
                Array.Resize(ref Game1.rainDrops, e.ReadAs<DRWMessage>().RainAmt);
        }

        private void ClosingGame(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            TimeElapsed = 0;
            CurrentRainAmt = 0;
            IsVariableRain = false;
        }

        private void OnNewDay(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            //new day reset
            TimeElapsed = 0;
            CurrentRainAmt = (Game1.isRaining ? 70 : 0);

            if (Game1.random.NextDouble() <= ModOptions.VariableRainChance && Game1.isRaining)
            {
                Game1.hudMessages.Add(new HUDMessage("The rain today will be variable"));
                IsVariableRain = true;
            }

        }

        private void Every10Minutes(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            TimeElapsed++;
            if (TimeElapsed == ModOptions.NumberOfTenMinutes && IsVariableRain && Game1.isRaining)
            {
                TimeElapsed = 0;
                UpdateDynamicRain();
            }
        }

        private void UpdateDynamicRain()
        {
            if (!Context.IsMainPlayer)
                return;

            //let's do some basic rules.
            // Massive change chance (defined as 2.5* configured option) is 1/4th the default chance

            if (Game1.random.NextDouble() <= ModOptions.RainChangeChance)
            {
                double changeAmt = ModOptions.RainChangeAmt;
                if (Game1.random.NextDouble() <= (ModOptions.RainChangeChance / 4.0))
                    changeAmt *= 2.5;

                if (changeAmt <= 0)
                    changeAmt = 0;

                if (Game1.random.NextDouble() <= ModOptions.ChanceOfIncrease)
                {
                    CurrentRainAmt = (int)Math.Floor(CurrentRainAmt * (1.0 + changeAmt));
                }
                else
                {
                    CurrentRainAmt = (int)Math.Floor(CurrentRainAmt * (1.0 - changeAmt));
                }
            }

            //sanity bound.
            if (CurrentRainAmt > 2000)
                CurrentRainAmt = 2000;
            if (CurrentRainAmt < 10)
                CurrentRainAmt = 10;
            
            //send out update
            if (Context.IsMultiplayer)
                SendRainUpdate(CurrentRainAmt);

            Array.Resize(ref Game1.rainDrops, CurrentRainAmt);
        }

        public void SendRainUpdate(int newAmt)
        {
            var newMessage = new DRWMessage {RainAmt = newAmt};

            Helper.Multiplayer.SendMessage(newMessage,"UpdateRain");
        }

        //harmony reference methods
        public static Color GetSnowColor()
        {
            //for future compatibility
            return Color.White;
        }

    }
}
