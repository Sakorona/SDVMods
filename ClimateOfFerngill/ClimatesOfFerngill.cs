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
using System.Text;
using static ClimateOfFerngill.Sprites;

namespace ClimateOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig Config { get; private set; }
        int WeatherAtStartOfDay { get; set; }
        FerngillWeather CurrWeather { get; set; }
        private bool GameLoaded;
        public SDVMoon Luna { get; set; }

        private bool ModRan { get; set; }

        //event fields
        private List<Vector2> ThreatenedCrops { get; set; }
        private int DeathTime { get; set; }
        public bool IsExhausted { get; set; }
        public MersenneTwister Dice;
        private IClickableMenu PreviousMenu;

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
            CurrWeather = new FerngillWeather();
            ModRan = false;
            Luna = new SDVMoon();

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

            //create crop and temp mapping.
            InternalUtility.SetUpCrops();
            Icons ourIcons = new Icons(Helper.DirectoryPath);
            ThreatenedCrops = new List<Vector2>();
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
            Game1.activeClickableMenu = new WeatherMenu(Monitor);
        }

        private void HideMenu()
        {
            if (Game1.activeClickableMenu is WeatherMenu)
            {
                Game1.playSound("bigDeSelect"); // match default behaviour when closing a menu
                Game1.activeClickableMenu = null;
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            //run all night time processing.
            if (Config.HarshWeather)
            {
                if (CurrWeather.TodayLow < 2 && Game1.currentSeason == "fall") //run frost event - restrict to fall rn.
                    EarlyFrost();
            }

            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon && Config.MoonEffects)
            {
                Farm f = Game1.getFarm();
                HoeDirt curr;

                if (f != null){
                    foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt)
                        {
                            curr = (HoeDirt)TF.Value;
                            if (curr.crop != null)
                            {
                                //20% chance of increased growth.
                                if (Dice.NextDouble() < .1)
                                {
                                    if (Config.TooMuchInfo) Console.WriteLine("Crop is being boosted by full moon");
                                    if (curr.state == 1) //make sure it's watered
                                    {
                                        curr.crop.dayOfCurrentPhase = curr.crop.fullyGrown ? curr.crop.dayOfCurrentPhase - 1 : Math.Min(curr.crop.dayOfCurrentPhase + 1, curr.crop.phaseDays.Count > 0 ? curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)] : 0);
                                        if (curr.crop.dayOfCurrentPhase >= (curr.crop.phaseDays.Count > 0 ? curr.crop.phaseDays[Math.Min(curr.crop.phaseDays.Count - 1, curr.crop.currentPhase)] : 0) && curr.crop.currentPhase < curr.crop.phaseDays.Count - 1)
                                        {
                                            curr.crop.currentPhase = curr.crop.currentPhase + 1;
                                            curr.crop.dayOfCurrentPhase = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon && Config.MoonEffects)
            {
                Farm f = Game1.getFarm();
                HoeDirt curr;

                if (f != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> TF in f.terrainFeatures)
                    {
                        if (TF.Value is HoeDirt)
                        {
                            curr = (HoeDirt)TF.Value;
                            if (curr.crop != null)
                            {
                                if (Dice.NextDouble() < .09)
                                {
                                    curr.state = 0; //dewater!! BWAHAHAAHAA.
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SummerHeatwave()
        {
            Farm f = Game1.getFarm();
            HoeDirt curr;
            bool cropsDeWatered = false;
            bool cropsKilled = false;
            int count = 0;

            if (f != null)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (count >= 15)
                        break;

                    if (tf.Value is HoeDirt)
                    {
                        curr = (HoeDirt)tf.Value;
                        if (curr.crop != null)
                        {
                            if (Dice.NextDouble() > .65)
                            {
                                if (Config.TooMuchInfo) Console.WriteLine("First Condition for Lethal: " + (CurrWeather.TodayHigh >= Config.DeathTemp));
                                if (Config.TooMuchInfo) Console.WriteLine("Second Condition for Lethal:" + Config.AllowCropHeatDeath);
                                if (CurrWeather.TodayHigh >= Config.DeathTemp && Config.AllowCropHeatDeath)
                                {
                                    ThreatenedCrops.Add(tf.Key);
                                    curr.state = 0;
                                    count++;
                                    cropsKilled = true;
                                    if (Config.TooMuchInfo) Console.WriteLine("Triggered: Lethal heatwave");
                                }
                                else if (CurrWeather.TodayHigh >= Config.HeatwaveWarning && !Config.AllowCropHeatDeath)
                                {
                                    curr.state = 0; //dewater
                                    count++;
                                    cropsDeWatered = true;
                                    if (Config.TooMuchInfo) Console.WriteLine("Triggered: Non lethal heatwave");
                                }
                            }
                        }
                    }
                }
            }

            if (cropsDeWatered)
                InternalUtility.ShowMessage("The extreme heat has caused some of your crops to become dry....!");
            if (cropsKilled)
                InternalUtility.ShowMessage("The extreme heat has caused some of your crops to dry out. If you don't water them, they'll die!");
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
           //run non specific code first
           if (Game1.currentLocation.IsOutdoors && Game1.isLightning)
            {
                double diceChance = Dice.NextDouble();
                if (Config.TooMuchInfo) LogEvent("The chance of exhaustion is: " + diceChance);
                if (diceChance < Config.DiseaseChance)
                {
                    IsExhausted = true;
                    InternalUtility.ShowMessage("The storm has caused you to get a cold!");
                }
            }

           //disease code.
           if (IsExhausted)
           {
                if (Config.TooMuchInfo) LogEvent("The old stamina is : " + Game1.player.stamina);
                Game1.player.stamina = Game1.player.stamina - Config.StaminaPenalty;
                if (Config.TooMuchInfo) LogEvent("The new stamina is : " + Game1.player.stamina);
           }

           //alert code
           if (IsExhausted && Dice.NextDouble() < Config.DiseaseChance)
            {
                InternalUtility.ShowMessage("You have a cold, and feel worn out!");
            }

            //specific time stuff
            if (e.NewInt == 610)
            {
                if (CurrWeather.TodayHigh > Config.HeatwaveWarning)
                    CurrWeather.Status = FerngillWeather.HEATWAVE;

                if (CurrWeather.TodayLow < Config.FrostWarning)
                    CurrWeather.Status = FerngillWeather.FROST;

                CheckForDangerousWeather(true);
            }

            //night time events
            if (e.NewInt > Game1.getTrulyDarkTime() && Game1.currentLocation.isOutdoors)
            {
                if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon && Config.MoonEffects)
                {
                    if (Dice.NextDouble() > .98) //2% chance
                        SpawnGhostOffScreen();
                }
            }

            //heatwave event
            if (e.NewInt == (int)Config.HeatwaveTime)
            {
                if (CurrWeather.TodayHigh > (int)Config.HeatwaveWarning && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && (!Game1.isRaining || !Game1.isLightning))
                {
                    DeathTime = InternalUtility.GetNewValidTime(e.NewInt, Config.TimeToDie, InternalUtility.TIMEADD);
                    if (Config.TooMuchInfo) LogEvent("Death Time is " + DeathTime);
                    if (Config.TooMuchInfo) LogEvent("Heatwave Event Triggered");
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
            var ourFarm = Game1.getFarm();

            if (ourFarm != null)
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
                bool flag;

                List<NPC> characters = ourFarm.characters;
                Ghost bat = new Ghost(zero * Game1.tileSize);
                int num1 = 1;
                bat.focusedOnFarmers = num1 != 0;
                int num2 = 1;
                bat.wildernessFarmMonster = num2 != 0;
                characters.Add((NPC)bat);
                flag = true;

                if (!flag || !Game1.currentLocation.Equals((object)this))
                    return;
                foreach (KeyValuePair<Vector2, StardewValley.Object> keyValuePair in (Dictionary<Vector2, StardewValley.Object>)ourFarm.objects)
                {
                    if (keyValuePair.Value != null && keyValuePair.Value.bigCraftable && keyValuePair.Value.parentSheetIndex == 83)
                    {
                        keyValuePair.Value.shakeTimer = 1000;
                        keyValuePair.Value.showNextIndex = true;
                        Game1.currentLightSources.Add(new LightSource(4, keyValuePair.Key * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), 0.0f), 1f, Color.Cyan * 0.75f, (int)((double)keyValuePair.Key.X * 797.0 + (double)keyValuePair.Key.Y * 13.0 + 666.0)));
                    }
                }
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            GameLoaded = true;
            UpdateWeather();
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
                CurrWeather = new FerngillWeather();
                UpdateWeather();
            }

            int noLonger = InternalUtility.VerifyValidTime(Config.NoLongerDisplayToday) ? Config.NoLongerDisplayToday : 1700;

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

            if (Game1.timeOfDay < noLonger) //don't display today's weather 
            {
                tvText += "The high for today is ";
                if (!Config.DisplaySecondScale)
                    tvText += WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.TempGauge) + ", with the low being " + WeatherHelper.DisplayTemperature(CurrWeather.TodayLow, Config.TempGauge) + ". ";
                else //derp.
                    tvText += WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.TempGauge) + " (" + WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.SecondScaleGauge) + ") , with the low being " + WeatherHelper.DisplayTemperature(CurrWeather.TodayLow, Config.TempGauge) + " (" + WeatherHelper.DisplayTemperature(CurrWeather.TodayHigh, Config.SecondScaleGauge) + ") . ";

                if (Config.TooMuchInfo) LogEvent(tvText);

                //today weather
                tvText = tvText + WeatherHelper.GetWeatherDesc(Dice, WeatherHelper.GetTodayWeather(), true, Monitor);

                //get WeatherForTommorow and set text
                tvText = tvText + "#Tommorow, ";
            }

            //tommorow weather
            tvText = tvText + WeatherHelper.GetWeatherDesc(Dice, (SDVWeather)Game1.weatherForTomorrow, false, Monitor);

            return tvText;
        }
       
        private void LogEvent(string msg, bool important=false)
        {
            if (!important)
                Monitor.Log(msg, LogLevel.Debug);
            else
                Monitor.Log(msg, LogLevel.Info);            
        }

        public void CheckForDangerousWeather(bool hud = true)
        {
            if (CurrWeather.Status == FerngillWeather.BLIZZARD) {
                InternalUtility.ShowMessage("There's a dangerous blizzard out today. Be careful!");
                return;
            }

            if (CurrWeather.Status == FerngillWeather.FROST && Game1.currentSeason != "winter")
            {
                InternalUtility.ShowMessage("The temperature tonight will be dipping below freezing. Your crops may be vulnerable to frost!");
                return;
            }

            if (CurrWeather.Status == FerngillWeather.HEATWAVE)
            {
                InternalUtility.ShowMessage("A massive heatwave is sweeping the valley. Stay hydrated!");
                return;
            }
        }

        public void EarlyFrost()
        {
            //this function is called if it's too cold to grow crops. Must be enabled.
            Farm f = Game1.getFarm();
            HoeDirt curr;
            bool cropsKilled = false;

            if (f != null)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (tf.Value is HoeDirt)
                    {
                        curr = (HoeDirt)tf.Value;
                        if (curr.crop != null && InternalUtility.IsFallCrop(curr.crop.indexOfHarvest))
                        {
                            if (CurrWeather.TodayLow <= InternalUtility.CheckCropTolerance(curr.crop.indexOfHarvest) && Dice.NextDouble() < Config.FrostHardiness)
                            {
                                cropsKilled = true;
                                curr.crop.dead = true;
                            }
                        }
                    }
                }
            }

            if (cropsKilled)
            {
                InternalUtility.ShowMessage("During the night, some crops died to the frost...");
                if (Config.TooMuchInfo) LogEvent("Setting frost test via queued message");
            }
        }

        public void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            if (!GameLoaded) //sanity check
                return;

            int[] beachItems = new int[] { 393, 397, 392, 394 };
            int[] moonBeachItems = new int[] { 393, 394, 560, 586, 587, 589, 397 };
            CurrWeather.Status = 0; //reset status
            IsExhausted = false; //reset disease
            UpdateWeather();    

            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.NewMoon && Config.MoonEffects)
            {
                Beach ourBeach = InternalUtility.GetBeach();
                foreach (KeyValuePair<Vector2, StardewValley.Object> o in ourBeach.objects)
                {
                    if (beachItems.Contains(o.Value.parentSheetIndex))
                    {
                        if (Dice.NextDouble() < .2)
                        {
                            ourBeach.objects.Remove(o.Key);
                        }                           
                    }
                }
            }
            
            //moon processing
            if (SDVMoon.GetLunarPhase() == MoonPhase.FullMoon && Config.MoonEffects)
            {
                int parentSheetIndex = 0;
                Rectangle rectangle = new Rectangle(65, 11, 25, 12);
                Beach ourBeach = InternalUtility.GetBeach();
                for (int index = 0; index < 5; ++index)
                {
                    parentSheetIndex = moonBeachItems.GetRandomItem(Dice);
                    if (Dice.NextDouble() < .0001)
                        parentSheetIndex = 392; //rare chance

                    if (Dice.NextDouble() < .8)
                    {
                        Vector2 v = new Vector2((float)Game1.random.Next(rectangle.X, rectangle.Right), (float)Game1.random.Next(rectangle.Y, rectangle.Bottom));
                        if (ourBeach.isTileLocationTotallyClearAndPlaceable(v))
                            ourBeach.dropObject(new StardewValley.Object(parentSheetIndex, 1, false, -1, 0), v * (float)Game1.tileSize, Game1.viewport, true, (Farmer)null);
                    }
                }
            }
        }

        void UpdateWeather(){
            bool forceSet = false;
            //sanity checks.
            
            #region WeatherChecks
            //sanity check - wedding
            if (Game1.player.spouse != null && Game1.player.spouse.Contains("engaged"))
            {
                if (Game1.countdownToWedding == 1)
                {
                    if (Config.TooMuchInfo) LogEvent("There is no Alanis Morissetting here. Enjoy your wedding.");
                    Game1.weatherForTomorrow = Game1.weather_wedding;
                    return;
                }
            }

            if (Game1.countdownToWedding == 0 && (Game1.player.spouse != null && Game1.player.spouse.Contains("engaged")))
            {
                Game1.weatherForTomorrow = Game1.weather_wedding;
                if (Config.TooMuchInfo) LogEvent("Detecting the wedding tommorow. Setting weather and returning");
                return;
            }

            //sanity check - festival
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.weatherForTomorrow = Game1.weather_festival;
                forceSet = true;
            }
            //catch call.
            if (WeatherAtStartOfDay != Game1.weatherForTomorrow && ModRan)
            {
                if (WeatherHelper.WeatherForceDay(Game1.currentSeason, Game1.dayOfMonth + 1, Game1.year))
                {
                    if (Config.TooMuchInfo) LogEvent("The game will force weather. Aborting.");
                    return;
                }

                if (Config.TooMuchInfo)
                    LogEvent("Change detected not caught by other checks. Debug Info: DAY " + Game1.dayOfMonth + " Season: " + Game1.currentSeason + " with prev weather: " + WeatherHelper.DescWeather(WeatherAtStartOfDay) + " and new weather: " + WeatherHelper.DescWeather(Game1.weatherForTomorrow) + ". Aborting.");

                return;
            }

            //TV forces.
            bool forceTomorrow = WeatherHelper.FixTV();
            if (forceTomorrow)
            {
                LogEvent("Tommorow, there will be forced weather.");
                forceSet = true;
            }
            #endregion

            /* Since we have multiple different climates, maybe a more .. clearer? method */
            CurrWeather.Reset();

            //reset the variables
            rainChance = stormChance = windChance = 0;

            //set the variables
            if (Game1.currentSeason == "spring")
                HandleSpringWeather();
            else if (Game1.currentSeason == "summer")
                HandleSummerWeather();
            else if (Game1.currentSeason == "fall")
                HandleAutumnWeather();
            else if (Game1.currentSeason == "winter")
                HandleWinterWeather();

            //handle calcs here for odds.
            double chance = Dice.NextDouble();
            if (Config.TooMuchInfo)
                LogEvent("Rain Chance is: " + rainChance + " with the rng being " + chance);

            //override for the first spring.
            if (!WeatherHelper.CanWeStorm(Config))
                stormChance = 0;

            //global change - if it rains, drop the temps (and if it's stormy, drop the temps)
            if (Game1.isRaining)
            {
                if (Config.TooMuchInfo) LogEvent("Dropping temp by 4 from " + CurrWeather.TodayHigh);
                CurrWeather.TodayHigh = CurrWeather.TodayHigh - 4;
                CurrWeather.TodayLow = CurrWeather.TodayLow - 2;
            }

            if (Config.ForceHeat)
            {
                if (Config.TooMuchInfo) LogEvent("Forcing Hazardous Weather: Heatwave conditions");
                CurrWeather.TodayHigh = 50;
            }

            if (Config.ForceFrost)
            {
                if (Config.TooMuchInfo) LogEvent("Forcing Hazardous Weather: Frost conditions");
                CurrWeather.TodayHigh = 0;
                CurrWeather.TodayLow = -1;
            }


            if (forceSet)
                return;

            //sequence - rain (Storm), wind, sun
            //this also contains the notes - certain seasons don't have certain weathers.
            if (chance < rainChance || Game1.currentSeason != "winter")
            {
                chance = Dice.NextDouble();
                if (chance < stormChance && stormChance != 0)
                {
                    if (Config.TooMuchInfo) LogEvent("Storm is selected, with roll " + chance + " and target percent " + stormChance);
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                }
                else
                {
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    if (Config.TooMuchInfo) LogEvent("Raining selected");
                }
            }
            else if (Game1.currentSeason != "winter")
            {
                if (chance < (windChance + rainChance) && (Game1.currentSeason == "spring" || Game1.currentSeason == "fall") && windChance != 0)
                {
                    Game1.weatherForTomorrow = Game1.weather_debris;
                    if (Config.TooMuchInfo) LogEvent("It's windy today, with roll " + chance + " and wind odds " + windChance);
                }
                else
                    Game1.weatherForTomorrow = Game1.weather_sunny;
            }

            if (Game1.currentSeason == "winter")
            {
                if (chance < rainChance)
                    Game1.weatherForTomorrow = Game1.weather_snow;
                else
                    Game1.weatherForTomorrow = Game1.weather_sunny;
            }

            if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && Config.AllowSnowOnFall28)
            {
                CurrWeather.TodayHigh = 2;
                CurrWeather.TodayLow = -1;
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.
            }

            WeatherAtStartOfDay = Game1.weatherForTomorrow;
            Game1.chanceToRainTomorrow = rainChance; //set for various events.
            LogEvent("We've set the weather for tommorow . It is: " + WeatherHelper.DescWeather(Game1.weatherForTomorrow));
            ModRan = true;
        }

        private void HandleSpringWeather()
        {
            if (Config.TooMuchInfo) LogEvent("Executing Spring Weather");
            stormChance = .15;
            windChance = .25;
            rainChance = .3 + (Game1.dayOfMonth * .0278);

            CurrWeather.TodayHigh = Dice.Next(1, 8) + 8;
            CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);

            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                stormChance = .2;
                windChance = .15 + (Game1.dayOfMonth * .01);
                rainChance = .2 + (Game1.dayOfMonth * .01);            

                CurrWeather.TodayHigh = Dice.Next(1, 6) + 14;
                CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);
            }

            if (Game1.dayOfMonth > 18)
            {
                stormChance = .3;
                windChance = .05 + (Game1.dayOfMonth * .01);
                rainChance = .2;

                CurrWeather.TodayHigh = Dice.Next(1, 6) + 20;
                CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);
            }

            //override the rain chance - it's the same no matter what day, so pulling it out of the if statements.
            switch (Config.ClimateType)
            {
                case "arid":
                    CurrWeather.AlterTemps(5);
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
        }

        private void HandleSummerWeather()
        {
            if (Config.TooMuchInfo) LogEvent("Executing Summer Weather");
            if (Game1.dayOfMonth < 10)
            {
                //rain, snow, windy chances
                stormChance = .45;
                windChance = 0; //cannot wind during summer
                rainChance = .15;

                CurrWeather.TodayHigh = Dice.Next(1, 8) + 26;
                CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);
            }
            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                //rain, snow, windy chances
                stormChance = .6;
                windChance = 0; //cannot wind during summer
                rainChance = .15;

                CurrWeather.TodayHigh = 30 + (int)Math.Floor(Game1.dayOfMonth * .25) + Dice.Next(0,5);
                if (Dice.NextDouble() > .70)
                {
                    if (Config.TooMuchInfo) LogEvent("Randomly adding to the temp");
                    CurrWeather.TodayHigh += Dice.Next(0, 3);
                }

                CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);
            }
            if (Game1.dayOfMonth > 18)
            {
                //temperature
                CurrWeather.TodayHigh = 42 - (int)Math.Floor(Game1.dayOfMonth * .8) + Dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);

                stormChance = .45;
                windChance = 0; 
                rainChance = .3;
            }

            //summer alterations
            CurrWeather.AlterTemps(Dice.Next(0, 3));
            if (Config.ClimateType == "arid")  CurrWeather.AlterTemps(6);

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
        }

        private void HandleAutumnWeather()
        {
            if (Config.TooMuchInfo) LogEvent("Executing Fall Weather");
            stormChance = .33;
            CurrWeather.TodayHigh = 22 - (int)Math.Floor(Game1.dayOfMonth * .667) + Dice.Next(0, 2); 

            if (Game1.dayOfMonth < 10)
            {
                windChance = 0 + (Game1.dayOfMonth * .044);
                rainChance = .3 + (Game1.dayOfMonth * .01111);
                CurrWeather.GetLowFromHigh(Dice.Next(1, 6) + 4);
            }

            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                windChance = .4 + (Game1.dayOfMonth * .022);
                rainChance = 1 - windChance;
                CurrWeather.GetLowFromHigh(Dice.Next(1, 6) + 3);
            }

            if (Game1.dayOfMonth > 18)
            {
                windChance = .1 + Game1.dayOfMonth * .044; 
                rainChance = .5;
                if (!Config.SetLowCap)
                    CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3);
                else
                    CurrWeather.GetLowFromHigh(Dice.Next(1, 3) + 3, Config.LowCap);
            }


            //climate changes to the rain.
            switch (Config.ClimateType)
            {
                case "arid":
                    rainChance = .05;
                    CurrWeather.AlterTemps(2);
                    if (Game1.dayOfMonth < 10 && Game1.dayOfMonth > 18) windChance = .3;
                    break;
                case "dry":
                    if (Game1.dayOfMonth > 10) rainChance = .25;
                    else rainChance = .2;
                    windChance = .3;
                    break;
                case "wet":
                    rainChance = rainChance + .05;
                    break;
                case "monsoon":
                    if (Game1.dayOfMonth > 18) rainChance = .9;
                    else rainChance = .1 + (Game1.dayOfMonth * .08889);
                    break;
                default:
                    break;
            }

    }

        private void HandleWinterWeather()
        {
            if (Config.TooMuchInfo) LogEvent("Executing Winter Weather");
            stormChance = 0;
            windChance = 0;
            rainChance = .6;

            if (Game1.dayOfMonth < 10)
            {
                CurrWeather.TodayHigh = -2 + (int)Math.Floor(Game1.dayOfMonth * .889) + Dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(Dice.Next(1, 4));
            }

            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                CurrWeather.TodayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.111) + Dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(Dice.Next(1, 4));
                rainChance = .75;
            }

            if (Game1.dayOfMonth > 18)
            {
                CurrWeather.TodayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.222) + Dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(Dice.Next(1, 4));
            } 

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

        }
    }    
}

