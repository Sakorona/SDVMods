using System;
using System.Reflection;
using System.Collections.Generic;

//3P
using NPack;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Monsters;
using StardewValley.Locations;
using StardewValley.Menus;

using Microsoft.Xna.Framework;

//DAMN YOU 1.2
using SFarmer = StardewValley.Farmer;
using Microsoft.Xna.Framework.Input;


namespace ClimateOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig Config { get; private set; }
        internal SDVWeather WeatherAtStartOfDay { get; set; }
        internal FerngillWeather CurrWeather { get; set; }
        public SDVMoon Luna { get; set; }

        //trackers
        private bool GameLoaded;
        private SDVWeather TmrwWeather;

        //event fields
        private List<Vector2> ThreatenedCrops { get; set; }
        private int DeathTime { get; set; }
        public MersenneTwister Dice;
        private IClickableMenu PreviousMenu;
        private HazardousWeatherEvents BadEvents;

        //chances of specific weathers
        private double windChance;
        private double stormChance;
        private double rainChance;

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
            Dice = new MersenneTwister();
            Config = helper.ReadConfig<ClimateConfig>();
            CurrWeather = new FerngillWeather(Config);

            var ourIcons = new Sprites.Icons(Helper.DirectoryPath);

            Luna = new SDVMoon(Monitor, Config, Dice);
            BadEvents = new HazardousWeatherEvents(Monitor, Config, Dice);

            //set variables
            rainChance = 0;
            stormChance = 0;
            windChance = 0;
  
            //register event handlers
            TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;

            //register keyboard handlers and other menu events
            ControlEvents.KeyPressed += (sender, e) => this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
            MenuEvents.MenuClosed += (sender, e) => this.ReceiveMenuClosed(e.PriorMenu);
        }

        private void ReceiveKeyPress(Keys key, Keys config)
        {
            if (config != key)
                return;

            // perform bound action
            this.ToggleMenu();
        }

        private void ReceiveMenuClosed(IClickableMenu closedMenu)
        {
            // restore the previous menu if it was hidden to show the lookup UI
            if (closedMenu is WeatherMenu && this.PreviousMenu != null)
            {
                 Game1.activeClickableMenu = this.PreviousMenu;
                 this.PreviousMenu = null;
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            //run all night time processing. Events do their own checking if enabled
            if (Config.HarshWeather)
            {
                BadEvents.EarlyFrost(CurrWeather);
            }

            Luna.HandleMoonBeforeSleep(Game1.getFarm()); //run lunar events
        }
       
        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            BadEvents.CatchACold();

            //specific time stuff
            if (e.NewInt == 610)  CurrWeather.CheckForDangerousWeather(); //HUD Messages about status.

            /* /////////////////////////////////////////////////
             * night time events
             * //////////////////////////////////////////////// */

            if (Luna.CheckForGhostSpawn()) SpawnGhostOffScreen();

            //heatwave event
            if (e.NewInt == 1700)
            {
                if (CurrWeather.TodayHigh > (int)Config.HeatwaveWarning && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && (!Game1.isRaining || !Game1.isLightning))
                {
                    DeathTime = InternalUtility.GetNewValidTime(e.NewInt, 300, InternalUtility.TIMEADD); //3 hours.
                    if (Config.TooMuchInfo) Monitor.Log("Death Time is " + DeathTime);
                    if (Config.TooMuchInfo) Monitor.Log("Heatwave Event Triggered");
                    SummerHeatwave();
                }
            }

            //killer heatwave crop death time
            if (Game1.timeOfDay == DeathTime && Config.AllowCropHeatDeath)
            {
                //if it's still de watered - kill it.
                Farm f = Game1.getFarm();
                bool cDead = false;

                foreach (Vector2 v in ThreatenedCrops)
                {
                    HoeDirt hd = (HoeDirt)f.terrainFeatures[v];
                    if (hd.state == 0)
                    {
                        hd.crop.dead = true;
                        cDead = true;
                    }
                }

                if (cDead)
                    InternalUtility.ShowMessage("Some of the crops have died due to lack of water!");
            }

            // sanity check if the player hits 0 Stamina ( the game doesn't track this )
            if (Game1.player.Stamina <= 0f)
            {
                InternalUtility.FaintPlayer();
            }
        }

        public void SpawnGhostOffScreen()
        {
            Vector2 zero = Vector2.Zero;

            if (Game1.getFarm() is Farm ourFarm)
            {
                switch (Game1.random.Next(4))
                {
                    case 0:
                        zero.X = (float)Dice.Next(ourFarm.map.Layers[0].LayerWidth);
                        break;
                    case 1:
                        zero.X = (float)(ourFarm.map.Layers[0].LayerWidth - 1);
                        zero.Y = (float)Dice.Next(ourFarm.map.Layers[0].LayerHeight);
                        break;
                    case 2:
                        zero.Y = (float)(ourFarm.map.Layers[0].LayerHeight - 1);
                        zero.X = (float)Dice.Next(ourFarm.map.Layers[0].LayerWidth);
                        break;
                    case 3:
                        zero.Y = (float)Game1.random.Next(ourFarm.map.Layers[0].LayerHeight);
                        break;
                }

                if (Utility.isOnScreen(zero * (float)Game1.tileSize, Game1.tileSize))
                    zero.X -= (float)Game1.viewport.Width;

                List<NPC> characters = ourFarm.characters;
                Ghost bat = new Ghost(zero * Game1.tileSize)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                };
                characters.Add((NPC)bat);

                if (!Game1.currentLocation.Equals((object)this))
                    return;
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            GameLoaded = true;
            UpdateWeather(CurrWeather);
            Luna.UpdateForNewDay();
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
    
        public void InterceptCallback(SFarmer who, string answer)
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

            //prevent null from being true.
            if (CurrWeather == null)
            {
                CurrWeather = new FerngillWeather(Config);
                UpdateWeather();
            }

            //The TV should display: Alerts, today's weather, tommorow's weather, alerts.

            // Something such as "Today, the high is 12C, with low 8C. It'll be a very windy day. Tommorow, it'll be rainy."
            // since we don't predict weather in advance yet. (I don't want to rearchitecture it yet.)
            // That said, the TV channel starts with Tommorow, so we need to keep that in mind.


            // Alerts for frost/cold snap display all day. Alerts for heatwave last until 1830. 
            tvText = "The forecast for the Valley is: ";

            if (CurrWeather.TodayHigh > Config.HeatwaveWarning && Game1.timeOfDay < 1830)
                tvText = tvText + "That it will be unusually hot outside. Stay hydrated and be careful not to stay too long in the sun. ";
            if (CurrWeather.TodayHigh < -5)
                tvText = tvText + "There's an extreme cold snap passing through the valley. Stay warm. ";
            if (CurrWeather.TodayLow < 2 && Config.HarshWeather)
                tvText = tvText + "Warning. We're getting frost tonight! Be careful what you plant! ";


            //we need to catch wedding nonsense here

            if (WeatherHelper.GetTodayWeather() == SDVWeather.Wedding && Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                //this is a funny thing, since it shouldn't be doing ths.
                Monitor.Log("We've triggered a flag where today AND tommorow are wedding days.", LogLevel.Info);
                UpdateWeather();             
            }

            if (Game1.timeOfDay < 1800) //don't display today's weather 
            {
                tvText += "The high for today is ";
                if (!Config.DisplaySecondScale)
                    tvText += WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.TempGauge) + ", with the low being " + WeatherHelper.DisplayTemperature(CurrWeather.TodayLow, Config.TempGauge) + ". ";
                else //derp.
                    tvText += WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.TempGauge) + " (" + WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.SecondScaleGauge) + ") , with the low being " + WeatherHelper.DisplayTemperature(CurrWeather.TodayLow, Config.TempGauge) + " (" + WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.SecondScaleGauge) + ") . ";

                if (Config.TooMuchInfo) Monitor.Log(tvText);

                //today weather
                tvText = tvText + WeatherHelper.GetWeatherDesc(Dice, WeatherHelper.GetTodayWeather(), true, Monitor);

                //get WeatherForTommorow and set text
                tvText = tvText + "#Tommorow, ";
            }

            //tommorow weather
            tvText = tvText + WeatherHelper.GetWeatherDesc(Dice, (SDVWeather)Game1.weatherForTomorrow, false, Monitor);

            return tvText;
        }
       
        public void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            if (!GameLoaded) //sanity check
                return;

            //update objects for new day.
            BadEvents.UpdateForNewDay();
            CurrWeather.UpdateForNewDay();
            Luna.UpdateForNewDay();
            Luna.HandleMoonAfterWake(InternalUtility.GetBeach());

            //update the weather
            UpdateWeather(CurrWeather);    
        }

        void UpdateWeather(FerngillWeather weatherOutput)
        {
            bool forceSet = false;
            
            SDVSeasons CurSeason = InternalUtility.GetSeason(Game1.currentSeason);

            /* First, we need to check for forced weather changes */
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, CurSeason.ToString()))
            {
                Game1.weatherForTomorrow = (int)SDVWeather.Festival;
                forceSet = true;
            }



            TmrwWeather = (SDVWeather)Game1.weatherForTomorrow;
        }

        #region Menu
        private void ToggleMenu()
        {
            if (Game1.activeClickableMenu is WeatherMenu)
                this.HideMenu();
            else
                this.ShowMenu();
        }

        private void ShowMenu()
        {
            // show menu
            this.PreviousMenu = Game1.activeClickableMenu;
            Game1.activeClickableMenu = new WeatherMenu(Monitor, this.Helper.Reflection);
        }

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

