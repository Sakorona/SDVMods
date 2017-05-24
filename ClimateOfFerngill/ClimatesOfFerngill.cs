using System;
using System.IO;
using System.Collections.Generic;
using TwilightCore.PRNG;
using TwilightCore.StardewValley;
using TwilightCore;

//3P
using CustomTV;

//XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

//Stardew Valley Imports
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ClimateOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        /// <summary>
        /// This handles hazardous events (frosts, heatwaves, etc.) and the various stamina penalties
        /// </summary>
        private HazardousWeatherEvents BadEvents;

        /// <summary>
        /// This controls and draws fog to the screen.
        /// </summary>
        private FerngillFog OurFog; 

        /// <summary>
        /// This is used to allow the menu to revert back to a previous menu
        /// </summary>
        private IClickableMenu PreviousMenu;
        
        /// <summary>
        /// This is used to display icons on the menu
        /// </summary>
        private Sprites.Icons OurIcons { get; set; }

        /// <summary>
        /// This flag is if the rain totem was used today.
        /// </summary>
        private bool RainTotemUsedToday;

        /// <summary>
        /// This tracks what the weather is at the end of the mod update loop
        /// </summary>
        private SDVWeather EndWeather;

        /// <summary>
        /// This is the weather conditions in game. 
        /// </summary>
        private FerngillWeather CurrWeather { get; set; }

        /// <summary>
        /// This is the config container for the mod.
        /// </summary>
        private ClimateConfig Config { get; set; }
        
        /// <summary>
        /// This is the moon of the mod, which has a few events. Deliberatly marked public for another mod of mine.
        /// </summary>
        public SDVMoon Luna { get; set; }
        
        /// <summary>
        /// This contains the text for all strings. 
        /// </summary>
        public TVStrings OurText;

        /// <summary>
        /// This tracks if the farmer has any buffs or debuffs from the weather.
        /// </summary>
        private FarmerStatus FarmerHealth;

        /// <summary>
        /// Our PRNG.
        /// </summary>
        public MersenneTwister Dice;
    
        /// <summary>
        /// Tracking for if you were eating
        /// </summary>
        private bool wasEating = false;

        /// <summary>
        /// The ID for what was being eaten
        /// </summary>
        private int prevToEatStack = -1;

        /// <summary>
        /// The number of ticks per 10 minute span
        /// </summary>
        private int numberOfTicksPerSpan;

        /// <summary>
        /// The number of ticks outside.
        /// </summary>
        private int numberOfTicksOutside;

        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            bool IsFailed = false;

            Dice = new MersenneTwister();
            Config = helper.ReadConfig<ClimateConfig>();

            FarmerHealth = new FarmerStatus(Config, Monitor, Dice);
            OurFog = new FerngillFog();
            Luna = new SDVMoon(Monitor, Config, Dice);
            BadEvents = new HazardousWeatherEvents(Monitor, Config, Dice);
            OurText = helper.ReadJsonFile<TVStrings>("TvStrings.en.json");

            VerifyBundledWeatherFiles(); 

            string ClimateFileName = "weather\\" + Config.ClimateType + ".json";
            string ClimateFilePath = Path.Combine(Path.Combine(Helper.DirectoryPath, ClimateFileName));

            if (!File.Exists(ClimateFilePath))
            {
                Monitor.Log("The specified climate does not exist! Please verify your climate settings or reinstall the mod. Falling back to the base climate",
                    LogLevel.Error);
                ClimateFilePath = Path.Combine(Path.Combine(Helper.DirectoryPath, "weather\normal.json"));
                if (!File.Exists(ClimateFilePath))
                {
                    Monitor.Log("The default climate does not exist! Please reinstall the mod.", LogLevel.Error);
                    IsFailed = true;
                }
            }

            if (!IsFailed)
            {
                FerngillClimate gameClimate = helper.ReadJsonFile<FerngillClimate>(ClimateFilePath);
                CurrWeather = new FerngillWeather(Config, gameClimate, Dice, Monitor);
                //set flags
                RainTotemUsedToday = false;

                //register event handlers
                SaveEvents.AfterLoad += SaveEvents_AfterLoad;
                TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
                SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
                GameEvents.UpdateTick += GameEvents_UpdateTick;
                SaveEvents.BeforeSave += SaveEvents_BeforeSave;
                TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
                GraphicsEvents.OnPostRenderEvent += GraphicsEvents_OnPostRenderEvent;

                //siiigh.
                helper.ConsoleCommands
                        .Add("world_changetmrweather", "Changes tomorrow's weather.\"rain,storm,snow,debris,festival,wedding,sun\" ", TmrwWeatherChangeFromConsole)
                        .Add("world_changeweather", "Changes CURRENT weather. \"rain,storm,snow,debris,sun\"", WeatherChangeFromConsole)
                        .Add("player_removecold", "Removes the cold from the player.", RemovePlayerCold);

                //register keyboard handlers and other menu events
                ControlEvents.KeyPressed += (sender, e) => this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
                MenuEvents.MenuClosed += (sender, e) => this.ReceiveMenuClosed(e.PriorMenu);
            }
            else
            {
                Monitor.Log("The mod is not loaded.", LogLevel.Error);
            }
        }

        // *******************************
        // Events 
        // *******************************

        /// <summary>
        /// This function handles setting up the weather system for a new run. It also contains one-time run code.
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            CustomTVMod.changeAction("weather", HandleWeather); //replace weather
            OurIcons = new Sprites.Icons(Helper.Content);
            //UpdateWeather(CurrWeather);
            Luna.UpdateForNewDay();
            BadEvents.UpdateForNewDay();
            RainTotemUsedToday = false;
        }

        /// <summary>
        /// This handles the day flipping over, updating various objects and events
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame) //sanity check
                return;
            
            //update objects for new day.
            BadEvents.UpdateForNewDay();
            CurrWeather.UpdateForNewDay();
            RainTotemUsedToday = false;
            Luna.UpdateForNewDay();
            Luna.HandleMoonAfterWake();
            FarmerHealth.UpdateForNewDay();

            //update the weather
            UpdateWeather(CurrWeather);         
        }

        /// <summary>
        ///  Handle the player returning to the main window.
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            BadEvents.UpdateForNewDay();
            CurrWeather.UpdateForNewDay();
            FarmerHealth.UpdateForNewDay();
            OurFog.Reset();
            Luna.Reset();
            RainTotemUsedToday = false;
        }

        /// <summary>
        /// This event handles drawing to the screen
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame)
              return;

            if (Game1.currentLocation.IsOutdoors)
                OurFog.DrawFog();

            if (Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert) && CurrWeather.IsBlizzard)
                CurrWeather.DrawBlizzard();
        }
   
        /// <summary>
        /// This controls what is done every tick of the game. 
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame)
                return;

            OurFog.MoveFog();

            numberOfTicksPerSpan++;
            if (Game1.currentLocation.isOutdoors)
                numberOfTicksOutside++;

            //handle eating events
            if (Game1.isEating != wasEating)
            {
                if (!Game1.isEating)
                {
                    // Apparently this happens when the ask to eat dialog opens, but they pressed no.
                    // So make sure something was actually consumed.
                    if (prevToEatStack != -1 && (prevToEatStack - 1 == Game1.player.itemToEat.Stack))
                    {
                        FarmerHealth.CheckToRemoveCold();
                    }
                }

                prevToEatStack = (Game1.player.itemToEat != null ? Game1.player.itemToEat.Stack : -1);
            }

            wasEating = Game1.isEating;

            if (Config.StormTotem)
            {
                if (Game1.weatherForTomorrow != (int)EndWeather && !RainTotemUsedToday)
                {
                    RainTotemUsedToday = true;

                    if (Dice.NextDoublePositive() <= CurrWeather.GetStormOdds(SDVDate.Tomorrow))
                    {
                        Game1.weatherForTomorrow = Game1.weather_lightning;
                        Game1.addHUDMessage(new HUDMessage("You hear a roll of thunder..."));
                    }
                }
            }
        }

        /// <summary>
        /// This handles all post sleep before save events (night time processing)
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            if (Config.HarshWeather)
                BadEvents.EarlyFrost(CurrWeather);

            Luna.HandleMoonAtSleep(Game1.getFarm());          
        }

        /// <summary>
        /// This function handles the TV interception, putting up the sprite and outputting the text.
        /// </summary>
        /// <param name="tv">The TV being intercepted</param>
        /// <param name="sprite">The sprite being used</param>
        /// <param name="who">The Farmer being intercepted for</param>
        /// <param name="answer">The string being answered with</param>
        public void HandleWeather(TV tv, TemporaryAnimatedSprite sprite, StardewValley.Farmer who, string answer)
        {
            TemporaryAnimatedSprite WeatherCaster = new TemporaryAnimatedSprite(
                Game1.mouseCursors, new Rectangle(497, 305, 42, 28), 9999f, 1, 999999,
                tv.getScreenPosition(), false, false,
                (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06),
                0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);

            CustomTVMod.showProgram(WeatherCaster, Game1.parseText(GetWeatherForecast()), CustomTVMod.endProgram);
        }
        
        /// <summary>
        /// This function verifies the weather file!
        /// </summary>
        private bool VerifyBundledWeatherFiles()
        {
            bool FilesVerified = false;
            if (!File.Exists(Path.Combine(Helper.DirectoryPath, "weather/normal.json")))
            {
                FerngillClimate NormalClimate = new FerngillClimate();
                var Weathers = new List<WeatherParameters>
                {
                    new WeatherParameters("rain", .85, -.04, -.04, .04),
                    new WeatherParameters("storm", .25, 0, -.1, .1),
                    new WeatherParameters("debris", .05, .025, -.08, .08),
                    new WeatherParameters("lowtemp", 2, 1.5, -3,3),
                    new WeatherParameters("highTemp", 4,1.25,-4,4),
                    new WeatherParameters("fog", .55, 0, -.04,.04)
                };
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("spring", "spring", 1, 9, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.49,-.14,-.02,.02),
                    new WeatherParameters("storm",.15,0,-.1,.1),
                    new WeatherParameters("debris", .275,.0065,-.08,.08),
                    new WeatherParameters("lowtemp",2,.95,-3,3),
                    new WeatherParameters("hightemp",4,1.05,-4,4),
                    new WeatherParameters("fog",.35,0,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("spring", "spring", 10, 19, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.24,-.0005,-.07,.07),
                    new WeatherParameters("storm",.10,.0065,-.1,.1),
                    new WeatherParameters("debris", .3925,-.0075,-.14,.14),
                    new WeatherParameters("lowtemp",2,.805,-3,3),
                    new WeatherParameters("hightemp",4,.875,-4,4),
                    new WeatherParameters("fog",.35,0,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("spring", "spring", 20, 28, Weathers));

                //Summer.
                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.2,-.001,-.04,.04),
                    new WeatherParameters("storm",.4,0,-.1,.1),
                    new WeatherParameters("lowtemp",24,.1,-3,3),
                    new WeatherParameters("hightemp",28,.82,-4,4),
                    new WeatherParameters("fog",.001,0,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("summer", "summer", 1, 9, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.2,-.001,-.04,.04),
                    new WeatherParameters("storm",.8,0,-.1,.1),
                    new WeatherParameters("lowtemp",24,.1,-3,3),
                    new WeatherParameters("hightemp",28,.62,-4,4),
                    new WeatherParameters("fog",.001,0,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("summer", "summer", 10, 18, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",-.12,.0183,-.07,.07),
                    new WeatherParameters("storm",.8,-.0065,-.1,.1),
                    new WeatherParameters("lowtemp",27,-.25,-3,3),
                    new WeatherParameters("hightemp",57,-1.25,-4,4),
                    new WeatherParameters("fog",.001,0,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("summer", "summer", 19, 28, Weathers));


                //Fall
                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.44,-.001,-.04,.04),
                    new WeatherParameters("storm",.62,-.056,-.1,.1),
                    new WeatherParameters("debris", .05,.026,0,.1),
                    new WeatherParameters("lowtemp",21,-.78,-3,3),
                    new WeatherParameters("hightemp",23,-.64,-4,4),
                    new WeatherParameters("fog",.001,.02,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("fall", "fall", 1, 9, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.35,0,-.02,.02),
                    new WeatherParameters("storm",.1,0,-.1,.1),
                    new WeatherParameters("debris", .3,.035,0,.1),
                    new WeatherParameters("lowtemp",25,-1.11,-3,3),
                    new WeatherParameters("hightemp",29,-1.3522,-4,4),
                    new WeatherParameters("fog",.2,.05,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("fall", "fall", 10, 19, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.494,.0444,-.07,.07),
                    new WeatherParameters("storm",.1,0,-.1,.1),
                    new WeatherParameters("debris", 2.0222,.0722,0,.1),
                    new WeatherParameters("lowtemp",25,-1.11,-3,3),
                    new WeatherParameters("hightemp",29,-1.3522,-4,4),
                    new WeatherParameters("fog",.55,-1.85,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("fall", "fall", 20, 28, Weathers));

                //Winter
                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.75,0,-.04,.04),
                    new WeatherParameters("snow",1,0,0,0),
                    new WeatherParameters("lowtemp",-2.5,-.05,-3,3),
                    new WeatherParameters("hightemp",0,-.1579,-4,0),
                    new WeatherParameters("fog",.03,.0051,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("winter", "winter", 1, 9, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.58,.006,-.07,.07),
                    new WeatherParameters("snow",1,0,0,0),
                    new WeatherParameters("lowtemp",-24,1,-3,3),
                    new WeatherParameters("hightemp",0,-.1579,-4,0),
                    new WeatherParameters("fog",-.2,.028,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("winter", "winter", 10, 18, Weathers));

                Weathers.Clear();
                Weathers.AddRange(new List<WeatherParameters>()
                {
                    new WeatherParameters("rain",.58,.006,-.07,.07),
                    new WeatherParameters("snow",1,0,0,0),
                    new WeatherParameters("lowtemp",-24,1,-3,3),
                    new WeatherParameters("hightemp",0,-.1579,-4,0),
                    new WeatherParameters("fog",-.2,.028,-.04,.04)
                });
                NormalClimate.ClimateSequences.Add(new FerngillClimateTimeSpan("winter", "winter", 19, 28, Weathers));

                Helper.WriteJsonFile<FerngillClimate>(Path.Combine(Helper.DirectoryPath, "weather/normal.json"), NormalClimate);
            }



            return FilesVerified;
        }

        /// <summary>
        /// This function removes the cold (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        private void RemovePlayerCold(string arg1, string[] arg2)
        {
            FarmerHealth.RemoveCold();
            Monitor.Log("The cold has been removed", LogLevel.Info);
        }

        /// <summary>
        /// This function changes the weather (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        private void WeatherChangeFromConsole(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
                return;

            string ChosenWeather = arg2[0];

            switch (ChosenWeather)
            {
                case "rain":
                    Game1.isSnowing = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log("The weather is now rain",LogLevel.Info);
                    break;
                case "storm":
                    Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.isLightning = Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log("The weather is now storm", LogLevel.Info);
                    break;
                case "snow":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log("The weather is now snow", LogLevel.Info);
                    break;
                case "debris":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = false;
                    Game1.isDebrisWeather = true;
                    Game1.populateDebrisWeatherArray();
                    Monitor.Log("The weather is now debris", LogLevel.Info);
                    break;
                case "sunny":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = Game1.isRaining = false;
                    Monitor.Log("The weather is now sunny", LogLevel.Info);
                    break;
            }

            Game1.updateWeatherIcon();
            CurrWeather.SetCurrentWeather(); //update mod internals
        }

        /// <summary>
        /// This function changes the weather for tomorrow (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        private void TmrwWeatherChangeFromConsole(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
                return;

            string ChosenWeather = arg2[0];
            switch(ChosenWeather)
            {
                case "rain":
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    Monitor.Log("The weather Tomorrow is now rain", LogLevel.Info);
                    break;
                case "storm":
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                    Monitor.Log("The weather Tomorrow is now storm", LogLevel.Info);
                    break;
                case "snow":
                    Game1.weatherForTomorrow = Game1.weather_snow;
                    Monitor.Log("The weather Tomorrow is now snow", LogLevel.Info);
                    break;
                case "debris":
                    Game1.weatherForTomorrow = Game1.weather_debris;
                    Monitor.Log("The weather Tomorrow is now debris", LogLevel.Info);
                    break;
                case "festival":
                    Game1.weatherForTomorrow = Game1.weather_festival;
                    Monitor.Log("The weather Tomorrow is now festival", LogLevel.Info);
                    break;
                case "sun":
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                    Monitor.Log("The weather Tomorrow is now sun", LogLevel.Info);
                    break;
                case "wedding":
                    Game1.weatherForTomorrow = Game1.weather_wedding;
                    Monitor.Log("The weather Tomorrow is now wedding", LogLevel.Info);
                    break;
            }
        }
       
        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (!Game1.hasLoadedGame)
                return;

            OurFog.UpdateFog(e.NewInt);

            if (Config.StormyPenalty)
                FarmerHealth.CatchACold();

            double outsidePercentage = (double)numberOfTicksOutside / numberOfTicksPerSpan;

            if (Config.TooMuchInfo)
                Monitor.Log($"The outside percentage is {outsidePercentage} for {numberOfTicksPerSpan} ticks per span and {numberOfTicksOutside} ticks outside");

            if (outsidePercentage > .586)
                CurrWeather.HandleStaminaChanges(true);
            else
                CurrWeather.HandleStaminaChanges(false);

            //reset the numbers
            numberOfTicksOutside = 0;
            numberOfTicksPerSpan = 0;

            //specific time stuff
            if (e.NewInt == 610)
                CurrWeather.MessageForDangerousWeather();

            BadEvents.CheckForHazardousWeather(e.NewInt, CurrWeather.GetTodayHigh());

            /* /////////////////////////////////////////////////
             * night time events
             * //////////////////////////////////////////////// */

            if (Luna.CheckForGhostSpawn() && Game1.currentLocation is Farm)
                SDVUtilities.SpawnGhostOffScreen(Dice);

            // sanity check if the player hits 0 Stamina ( the game doesn't track this )
            if (Game1.player.Stamina <= 0f)
                SDVUtilities.FaintPlayer();
        }

        /// <summary>
        /// This function gets the forecast of the weather for the TV. 
        /// </summary>
        /// <returns>A string describing the weather</returns>
        public string GetWeatherForecast()
        {
            if (Game1.weddingToday) // sanity check
            {
                if (Config.TooMuchInfo)
                    Monitor.Log("There was a wedding today. Regenerating the weather.");

                UpdateWeather(CurrWeather, weddingOverride: true);
            }

            string tvText = " ";

            //The TV should display: Alerts, today's weather, Tomorrow's weather, alerts.

            // Something such as "Today, the high is 12C, with low 8C. It'll be a very windy day. Tomorrow, it'll be rainy."
            // since we don't predict weather in advance yet. (I don't want to rearchitecture it yet.)
            // That said, the TV channel starts with Tomorrow, so we need to keep that in mind.

            tvText = OurText.TVOpening;

            if (Game1.timeOfDay < 1800) //don't display today's weather 
            {
                tvText += OurText.TVHigh;
                if (!Config.DisplaySecondScale)
                    tvText += $"{CurrWeather.DisplayHighTemperature()} {OurText.TVLow} {CurrWeather.DisplayLowTemperature()} ";
                else 
                    tvText += $"{CurrWeather.DisplayHighTemperature()} ({CurrWeather.DisplayHighTemperatureSG()}), " +
                        $"{OurText.TVLow} {CurrWeather.DisplayLowTemperature()} ({CurrWeather.DisplayLowTemperatureSG()}) . ";

                if (Config.TooMuchInfo) Monitor.Log(tvText);

                //today weather
                tvText = tvText + WeatherHelper.GetWeatherDesc(OurText,Dice, CurrWeather.CurrentConditions(), 
                    CurrWeather, true, Monitor, Config.TooMuchInfo);

                //get WeatherForTomorrow and set text
                tvText = tvText + OurText.TVTomorrowForecast;
            }

            //Tomorrow weather
            tvText = tvText + WeatherHelper.GetWeatherDesc(OurText, Dice, (SDVWeather)Game1.weatherForTomorrow, CurrWeather, 
                false, Monitor, Config.TooMuchInfo);

            return tvText;
        }       

        void UpdateWeather(FerngillWeather weatherOutput, bool BREAKME, bool weddingOverride = false)
        {
            //get start values
            SDVWeather TmrwWeather = (SDVWeather)Game1.weatherForTomorrow;
            bool forceSet = false;

            string logMessage = $"The weather set for tomorrow at start is: " +
                $"{WeatherHelper.DescWeather(TmrwWeather, Game1.currentSeason)}";

            // The mod executes after the main loop and should only execute at the beginning of the
            //  day. This really means we have to make sure it runs or we'll have an issue with the tv
            //  description.

            // So, essentially, if it's already set to wedding or festival, we can go ahead and 
            //  just not run. If you use a rain totem, that should run after this, and before the 
            //  game's own weather processing.
            if (!weddingOverride)
            {

                if (Config.TooMuchInfo && Game1.player.spouse != null)
                    Monitor.Log($"Wedding flags: {Game1.countdownToWedding == 1} and {Game1.player.spouse.Contains("engaged")} with" +
                        $"count down to wedding being {Game1.countdownToWedding}");
                else if (Config.TooMuchInfo)
                    Monitor.Log($"Spouse is null. No wedding.");

                if (Game1.countdownToWedding == 1 && Game1.player.spouse != null && Game1.player.spouse.Contains("engaged"))
                {
                    if (Config.TooMuchInfo)
                        Monitor.Log("Wedding tommorrow");
                    forceSet = true;
                    Game1.weatherForTomorrow = Game1.weather_wedding;
                }        
            }

            if (TmrwWeather == SDVWeather.Festival)
            {
                if (Config.TooMuchInfo)
                    Monitor.Log("The weather Tomorrow is a festival.", LogLevel.Warn);
                forceSet = true;
            }

            //handle calcs here for odds.
            double chance = Dice.NextDouble();

            //construct the odds.


            //global change - if it rains, drop the temps (and if it's stormy, drop the temps)
            if (Game1.isRaining)
            {
                if (Config.TooMuchInfo) Monitor.Log($"Dropping temp by 4 from {CurrWeather.GetTodayHigh()}");
                CurrWeather.SetTodayHigh(CurrWeather.GetTodayHigh() - 4);
                CurrWeather.SetTodayLow(CurrWeather.GetTodayLow() - 2);
            }

            //handle forced weather from the game - this function will actually set the weather itself, if true.
            if (GameWillForceTomorrow(SDVDate.Tomorrow))
            {
                forceSet = true;
                if (Config.TooMuchInfo) Monitor.Log("The game is forcing weather Tomorrow. Setting flag.");

            }

            if (Game1.currentSeason == "winter" && (Game1.weatherForTomorrow == Game1.weather_rain || Game1.weatherForTomorrow == Game1.weather_lightning ) && !Config.AllowRainInWinter)
            {
                if (Config.TooMuchInfo)
                    Monitor.Log($"Fixing {SDVUtilities.WeatherToString(Game1.weatherForTomorrow)} in winter. Force setting to snow");

                Game1.weatherForTomorrow = Game1.weather_snow;
            }


            if (forceSet)
            {
                if (Config.TooMuchInfo) Monitor.Log("Detecting Force Set. Exiting.");
                return;
            }
 
            
            //Snow fall on Fall 28, if the flag is set.
            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && Config.AllowSnowOnFall28)
            {
                CurrWeather.SetTodayHigh(2);
                CurrWeather.SetTodayLow(-1);
                TmrwWeather = (SDVWeather)Game1.weather_snow; //it now snows on Fall 28.
            }
            
            if (Config.TooMuchInfo)
                Monitor.Log($"We've set the weather for Tomorrow. It is: {WeatherHelper.DescWeather(TmrwWeather, Game1.currentSeason)}");

            //set trackers
            EndWeather = TmrwWeather;
            // Game1.chanceToRainTomorrow = rainChance; //set for various events.
            Game1.weatherForTomorrow = (int)TmrwWeather;

            //Fire off the events to say 'we've set weather, update trackers'
            CurrWeather.SetCurrentWeather();
            OurFog.CheckForFog(Dice, CurrWeather);

            if (Config.TooMuchInfo)
                Monitor.Log($"Checking if set. Generated Weather: {WeatherHelper.DescWeather(TmrwWeather, Game1.currentSeason)} and set weather is: {WeatherHelper.DescWeather((SDVWeather)Game1.weatherForTomorrow, Game1.currentSeason)}");
        }


        private bool GameWillForceTomorrow(SDVDate Tomorrow)
        {
            if (Game1.year == 1 && Tomorrow.Season == "spring" && Tomorrow.Day == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return true;
            }

            foreach(KeyValuePair<SDVDate, int> entry in SDVUtilities.ForceDays)
            {
                if (entry.Key == Tomorrow)
                {
                    Game1.weatherForTomorrow = entry.Value;
                    return true;
                }
            }

            return false;
        }

        #region Menu
        /// <summary>
        /// This checks the keys being pressed if one of them is the weather menu, toggles it.
        /// </summary>
        /// <param name="key">The key being pressed</param>
        /// <param name="config">The keys we're listening to.</param>
        private void ReceiveKeyPress(Keys key, Keys config)
        {
            if (config != key)  //sanity force this to exit!
                return;

            if (!Game1.hasLoadedGame)
                return;

            // perform bound action ONLY if there is no menu OR if the menu is a WeatherMenu
            if (Game1.activeClickableMenu == null || Game1.activeClickableMenu is WeatherMenu)
            {
                this.ToggleMenu();
            }
        }

        /// <summary>
        /// This function closes the menu. Will reopen the previous menu if it exists
        /// </summary>
        /// <param name="closedMenu">The menu being closed.</param>
        private void ReceiveMenuClosed(IClickableMenu closedMenu)
        {
            // restore the previous menu if it was hidden to show the lookup UI
            if (closedMenu is WeatherMenu && this.PreviousMenu != null)
            {
                Game1.activeClickableMenu = this.PreviousMenu;
                this.PreviousMenu = null;
            }
        }

        /// <summary>
        /// Toggle the menu visiblity
        /// </summary>
        private void ToggleMenu()
        {
            if (Game1.activeClickableMenu is WeatherMenu)
                this.HideMenu();
            else
                this.ShowMenu();
        }

        /// <summary>
        /// Show the menu
        /// </summary>
        private void ShowMenu()
        {
            // show menu
            this.PreviousMenu = Game1.activeClickableMenu;
            Game1.activeClickableMenu = new WeatherMenu(Monitor, this.Helper.Reflection, OurIcons, OurText, CurrWeather, Luna, Config);
        }

        /// <summary>
        /// Hide the menu.
        /// </summary>
        private void HideMenu()
        {
            if (Game1.activeClickableMenu is WeatherMenu)
            {
                Game1.playSound("bigDeSelect"); // match default behaviour when closing a menu
                Game1.activeClickableMenu = null;
            }
        }
        #endregion
    }    
}

