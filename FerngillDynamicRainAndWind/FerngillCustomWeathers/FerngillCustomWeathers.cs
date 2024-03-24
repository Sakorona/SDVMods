using FerngillCustomWeathers.CustomWeathers;
using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace FerngillCustomWeathers
{
    public class FerngillCustomWeathers: Mod
    {
        internal static IMonitor Logger;
        internal WeatherConfig ModOptions;
        internal Icons OurTextures;

        internal FerngillFog OurFog;
        internal FerngillBlizzard OurBlizzard;
        internal bool FogToday;

        public override void Entry(IModHelper helper)
        {
            ModOptions = Helper.ReadConfig<WeatherConfig>();
            OurTextures = new Icons(helper.ModContent);
            Logger = Monitor;
            FogToday = false;
            OurFog = new FerngillFog(ModOptions, OurTextures);
            OurBlizzard = new FerngillBlizzard();


            Helper.Events.GameLoop.ReturnedToTitle += ReturnedToTitle;
            helper.Events.GameLoop.DayEnding += DayEnding;
            Helper.Events.GameLoop.DayStarted += OnDayStart;
            Helper.Events.GameLoop.TimeChanged += TimeChanged;
            Helper.Events.GameLoop.UpdateTicked += PerTickUpdate;
            Helper.Events.Player.Warped += OnLocationChange;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            Helper.ConsoleCommands.Add("debug_fogoutput", "Show Fog Output", DisplayFogInfo);
            Helper.ConsoleCommands.Add("debug_forcefog", "Foooog!", ForceFog);
            Helper.ConsoleCommands.Add("debug_forceblizz", "Blizzard!", ForceBlizz);
            Helper.ConsoleCommands.Add("debug_endblizz", "End Blizzard", EndBlizz);


            Helper.Events.Multiplayer.ModMessageReceived += MessageReceived;
        }

        private void EndBlizz(string arg1, string[] arg2)
        {
            OurBlizzard.EndWeather();
        }

        private void ForceBlizz(string arg1, string[] arg2)
        {
            Game1.netWorldState.Value.GetWeatherForLocation(Game1.location).isSnowing
                .Value = true;
            Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isRaining
                .Value = false;
            Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isDebrisWeather
                .Value = false;
            Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isLightning
                .Value = false;

            Game1.isRaining = false;
            Game1.isDebrisWeather = false;
            Game1.isLightning = false;
            Game1.isSnowing = true;

            Game1.updateWeather(Game1.currentGameTime);
            Game1.currentLocation.UpdateWhenCurrentLocation(Game1.currentGameTime);
            Game1.updateWeatherIcon();
            OurBlizzard.SetWeatherTime(600,2700);
            OurBlizzard.SetWhiteOut(true);

            Monitor.Log($"Forcing blizzards. Snow spawned, and white out set to true.");
        }

        private void ForceFog(string arg1, string[] arg2)
        {
            OurFog.CreateWeather(FogType.Normal, true);
            if (Game1.timeOfDay < 1200)
                OurFog.SetWeatherTime(600,1200);
            else
                OurFog.SetWeatherTime(1200,2400);
        }

        private void DisplayFogInfo(string arg1, string[] arg2)
        {
            Monitor.Log($"Fog info is type {OurFog.CurrentFogType} with start {OurFog.BeginTime} and {OurFog.ExpirationTime}, current alpha is {OurFog.FogAlpha}");
        }

        private void OnLocationChange(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation is IslandLocation)
            {
                //check if the fog should occur
                if (Game1.dayOfMonth <= 14 && Game1.random.NextDouble() < (ModOptions.FogChanceInEarlySummer / 2))
                    return;
                else if (Game1.dayOfMonth > 14 && Game1.random.NextDouble() < (ModOptions.FogChanceInLateSummer / 2))
                    return;

                OurBlizzard.HideInCurrentLocation = true;
                OurFog.HideInCurrentLocation = true;
            }

            if (e.NewLocation is not IslandLocation && OurFog.HideInCurrentLocation)
            {
                OurFog.HideInCurrentLocation = false;
                OurBlizzard.HideInCurrentLocation = false;
            }
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.currentLocation.IsOutdoors)
            {
                OurFog.DrawWeather();
                OurBlizzard.DrawWeather();
            }
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            FogToday = false;
            OurFog.OnNewDay();
            OurBlizzard.OnNewDay();
        }

        private void ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            FogToday = false;
        }

        private void MessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (Game1.IsMasterGame)
                return;

            if (e.Type == "FogSync")
            {
                var message = e.ReadAs<FogMessage>();
                OurFog.CreateWeather(message.type, true, isEvening: message.isEvening);
                OurFog.SetWeatherTime(message.StartTime, message.EndTime);
            }

            if (e.Type == "BlizzardSync")
            {
                var message = e.ReadAs<BlizzardMessage>();
                OurBlizzard.SetWhiteOut(message.IsWhiteOut);
                OurBlizzard.SetWeatherTime(message.StartTime, message.EndTime);
            }
        }

        private void TimeChanged(object sender, TimeChangedEventArgs e)
        {
            OurFog.UpdateWeather();

            if (!Context.IsMainPlayer) return;
            if (OurFog.IsWeatherVisible) return;
            if (Game1.isFestival()) return;

            if ((FogToday && Game1.timeOfDay <= Utility.ModifyTime(Game1.getStartingToGetDarkTime(Game1.currentLocation),-100) && Game1.random.NextDouble() <= ModOptions.EveningWeatherFogChance) || ((Game1.random.NextDouble() <= ModOptions.EveningWeatherFogChance / 4) && Game1.timeOfDay <= Utility.ModifyTime(Game1.getStartingToGetDarkTime(Game1.currentLocation),-100)) && Game1.timeOfDay > 1500)
            {
               //Monitor.Log("Firing off evening fog chance", LogLevel.Info);

                //evening fog
                var numTime = Game1.random.Next(60, 190);
                numTime = ClampTimeToTens(numTime);

                var startTime = Game1.getStartingToGetDarkTime(Game1.currentLocation);
                var endTime = Utility.ModifyTime(Game1.getTrulyDarkTime(Game1.currentLocation),numTime);
                var todayType = FogType.Normal;

                if (Game1.random.NextDouble() > .95)
                    todayType = FogType.Blinding;

                OurFog.CreateWeather(todayType, true, isEvening: true);
                OurFog.SetWeatherTime(startTime, endTime);
                SendFogMessage(todayType, startTime, endTime, isEvening: true);
                FogToday = false;
            }
        }

        private void OnDayStart(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            if (!Utility.isFestivalDay(Game1.dayOfMonth, Game1.season) && !Game1.weddingToday)
            {
                CheckAndInitFog();
                CheckAndInitBlizzards();
            }
        }

        private void CheckAndInitBlizzards()
        {
            double blizzChance = .01;
            bool whiteOut = false;

            int bTime = 600, eTime = 600;

            if (Game1.random.NextDouble() < blizzChance && Game1.IsSnowingHere())
                eTime = 2600;

            OurBlizzard.SetWeatherTime(bTime, eTime);

            //5% conversion chance
            if (Game1.random.NextDouble() < .05)
                whiteOut = true;

            OurBlizzard.SetWhiteOut(whiteOut);
            SendBlizzardMessage(whiteOut, bTime, eTime);
        }

        private void CheckAndInitFog()
        {
            //get fog chances.
            double fogChance = 0.01f;
            switch (Game1.currentSeason)
            {
                case "spring":
                    fogChance = (Game1.dayOfMonth <= 14
                        ? ModOptions.FogChanceInEarlySpring
                        : ModOptions.FogChanceInLateSpring);
                    break;
                case "summer":
                    fogChance = (Game1.dayOfMonth <= 14
                        ? ModOptions.FogChanceInEarlySummer
                        : ModOptions.FogChanceInLateSummer);
                    break;
                case "fall":
                    fogChance = (Game1.dayOfMonth <= 14
                        ? ModOptions.FogChanceInEarlyFall
                        : ModOptions.FogChanceInLateFall);
                    break;
                case "winter":
                    fogChance = (Game1.dayOfMonth <= 14
                        ? ModOptions.FogChanceInEarlyWinter
                        : ModOptions.FogChanceInLateWinter);
                    break;
            }

            if (!(Game1.random.NextDouble() <= fogChance)) return;
            OurFog.CreateWeather(FogType.Normal, false);

            //get morning fog
            int bTime = 600, eTime = 800;
                
            var timeChance = Game1.random.NextDouble();

            if (timeChance > 0 && timeChance < .25)
                eTime = 830;
            else if (timeChance >= .25 && timeChance < .32)
                eTime = 900;
            else if (timeChance >= .32 && timeChance < .41)
                eTime = 930;
            else if (timeChance >= .41 && timeChance < .55)
                eTime = 950;
            else if (timeChance >= .55 && timeChance < .7)
                eTime = 1040;
            else if (timeChance >= .7 && timeChance < .8)
                eTime = 1120;
            else if (timeChance >= .8 && timeChance < .9)
                eTime = 1200;
            else if (timeChance >= .9 && timeChance < .95)
                eTime = 1220;
            else if (timeChance >= .95 && timeChance < .98)
                eTime = 1300;
            else if (timeChance >= .98 && timeChance < .99)
                eTime = 1910;
            else if (timeChance >= .99)
                eTime = 2400;
                
            OurFog.SetWeatherTime(bTime, eTime);
            SendFogMessage(OurFog.CurrentFogType, bTime, eTime);
            FogToday = true;
        }

        private void PerTickUpdate(object sender, UpdateTickedEventArgs e)
        {
            OurFog.MoveWeather();
            OurBlizzard.UpdateWeather();
        }

        public void SendFogMessage(FogType f, int sTime, int eTime, bool isEvening = false)
        {
            var fMessage = new FogMessage {StartTime = sTime, EndTime = eTime, type = f, isEvening = isEvening};

            Helper.Multiplayer.SendMessage(fMessage, "FogSync");
        }

        public void SendBlizzardMessage(bool iW, int sTime, int eTime)
        {
            var bMessage = new BlizzardMessage {EndTime = eTime, StartTime = sTime, IsWhiteOut = iW};

            Helper.Multiplayer.SendMessage(bMessage, "BlizzardSync");
        }

        public static int ClampTimeToTens(int val)
        {
           if (val % 10 >= 5)
           {
               val = ((val / 10) + 1) * 10;
           }
            
           else if (val % 10 < 5 && (val % 10 != 0))
           {
                val = ((val / 10) - 1) * 10;

                if (val < 0)
                {
                    val = 0;
                }
           }
           return val;
        }
    }
}
