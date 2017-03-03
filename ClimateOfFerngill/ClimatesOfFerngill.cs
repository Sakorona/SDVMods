using System;
using System.Reflection;

//3P
using NPack;
using StardewModdingAPI;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

//DAMN YOU 1.2
using SFarmer = StardewValley.Farmer;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Monsters;

namespace ClimateOfFerngill
{
    public class ClimatesOfFerngill : Mod
    {
        public ClimateConfig Config { get; private set; }
        int WeatherAtStartOfDay { get; set; }
        FerngillWeather CurrWeather { get; set; }
        FerngillWeather TomorrowWeather { get; set; }
        private bool GameLoaded;
        public SDVMoon Luna { get; set; }

        MersenneTwister dice;
        private bool ModRan { get; set; }

        //event fields
        private List<Vector2> threatenedCrops { get; set; }
        private int deathTime { get; set; }
        public bool isExhausted { get; set; }

        //chances of specific weathers
        private double windChance;
        private double stormChance;
        private double rainChance;

        //fog info
        private bool ambientFog;
        private Vector2 fogPos;
        private int startFogTime;
        private Rectangle fogSource = new Rectangle(640, 0, 64, 64);
        private float fogAlpha;
        private int endFogTime;

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
            dice = new MersenneTwister();
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
            GraphicsEvents.OnPostRenderEvent += GraphicsEvents_OnPostRenderEvent;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;

            //create crop and temp mapping.
            InternalUtility.SetUpCrops(); 
            threatenedCrops = new List<Vector2>();
        }

        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (ambientFog && Game1.currentLocation.isOutdoors)
                CreateFog();
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (ambientFog && e.NewLocation.IsOutdoors)
            {
                if (Config.tooMuchInfo) LogEvent("Spawning fog for new outdoors location");
                CreateFog();
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            //run all night time processing.
            if (Config.HarshWeather)
            {
                if (CurrWeather.todayLow < 2 && Game1.currentSeason == "fall") //run frost event - restrict to fall rn.
                    EarlyFrost();
            }

            //moon processing
            if (SDVMoon.GetLunarPhase() == SDVMoon.FULMOON && Config.MoonEffects)
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
                                if (dice.NextDouble() < .1)
                                {
                                    if (Config.tooMuchInfo) Console.WriteLine("Crop is being boosted by full moon");
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


            if (SDVMoon.GetLunarPhase() == SDVMoon.NEWMOON && Config.MoonEffects)
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
                                if (dice.NextDouble() < .09)
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
                            if (dice.NextDouble() > .65)
                            {
                                if (Config.tooMuchInfo) Console.WriteLine("First Condition for Lethal: " + (CurrWeather.todayHigh >= Config.DeathTemp));
                                if (Config.tooMuchInfo) Console.WriteLine("Second Condition for Lethal:" + Config.AllowCropHeatDeath);
                                if (CurrWeather.todayHigh >= Config.DeathTemp && Config.AllowCropHeatDeath)
                                {
                                    threatenedCrops.Add(tf.Key);
                                    curr.state = 0;
                                    count++;
                                    cropsKilled = true;
                                    if (Config.tooMuchInfo) Console.WriteLine("Triggered: Lethal heatwave");
                                }
                                else if (CurrWeather.todayHigh >= Config.HeatwaveWarning && !Config.AllowCropHeatDeath)
                                {
                                    curr.state = 0; //dewater
                                    count++;
                                    cropsDeWatered = true;
                                    if (Config.tooMuchInfo) Console.WriteLine("Triggered: Non lethal heatwave");
                                }
                            }
                        }
                    }
                }
            }

            if (cropsDeWatered)
                InternalUtility.showMessage("The extreme heat has caused some of your crops to become dry....!");
            if (cropsKilled)
                InternalUtility.showMessage("The extreme heat has caused some of your crops to dry out. If you don't water them, they'll die!");
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
           //run non specific code first
           if (Game1.currentLocation.IsOutdoors && Game1.isLightning)
            {
                double diceChance = dice.NextDouble();
                if (Config.tooMuchInfo) LogEvent("The chance of exhaustion is: " + diceChance);
                if (diceChance < Config.DiseaseChance)
                {
                    isExhausted = true;
                    InternalUtility.showMessage("The storm has caused you to get a cold!");
                }
            }

           //disease code.
           if (isExhausted)
           {
                if (Config.tooMuchInfo) LogEvent("The old stamina is : " + Game1.player.stamina);
                Game1.player.stamina = Game1.player.stamina - Config.StaminaPenalty;
                if (Config.tooMuchInfo) LogEvent("The new stamina is : " + Game1.player.stamina);
           }

           //alert code
           if (isExhausted && dice.NextDouble() < Config.DiseaseChance)
            {
                InternalUtility.showMessage("You have a cold, and feel worn out!");
            }


            //fog stuff.
            if (Config.tooMuchInfo) LogEvent("Checking for fog.");
            if (dice.NextDouble() < Config.FogChance && e.NewInt <= 1000)
            {
                if (Config.tooMuchInfo) LogEvent("Creating fog conditions!");
                ambientFog = true;
                startFogTime = e.NewInt; //woops!
                fogAlpha = .85f;
                endFogTime = e.NewInt + (Config.FogDuration * 100);
            }

            //fog despawn - really hacky. 
            if (ambientFog && e.NewInt == endFogTime)
            {
                if (Config.tooMuchInfo) LogEvent("Ending fog conditions!");
                fogAlpha = 0.0f;
                ambientFog = false;
            }

            //specific time stuff
            if (e.NewInt == 610)
            {
                if (CurrWeather.todayHigh > Config.HeatwaveWarning)
                    CurrWeather.status = FerngillWeather.HEATWAVE;

                if (CurrWeather.todayLow < Config.FrostWarning)
                    CurrWeather.status = FerngillWeather.FROST;

                checkForDangerousWeather(true);
            }

            //night time events
            if (e.NewInt > Game1.getTrulyDarkTime() && Game1.currentLocation.isOutdoors)
            {
                if (SDVMoon.GetLunarPhase() == SDVMoon.FULMOON && Config.MoonEffects)
                {
                    if (dice.NextDouble() > .98) //2% chance
                        spawnGhostOffScreen();
                }
            }

            //heatwave event
            if (e.NewInt == (int)Config.HeatwaveTime)
            {
                if (CurrWeather.todayHigh > (int)Config.HeatwaveWarning && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && (!Game1.isRaining || !Game1.isLightning))
                {
                    deathTime = InternalUtility.GetNewValidTime(e.NewInt, Config.TimeToDie, InternalUtility.TIMEADD);
                    if (Config.tooMuchInfo) LogEvent("Death Time is " + deathTime);
                    if (Config.tooMuchInfo) LogEvent("Heatwave Event Triggered");
                    SummerHeatwave();
                }
            }

            //killer heatwave crop death time
            if (Game1.timeOfDay == deathTime && Config.AllowCropHeatDeath)
            {
                //if it's still de watered - kill it.
                Farm f = Game1.getFarm();
                bool cDead = false;

                foreach (Vector2 v in threatenedCrops)
                {
                    HoeDirt hd = (HoeDirt)f.terrainFeatures[v];
                    if (hd.state == 0)
                    {
                        hd.crop.dead = true;
                        cDead = true;
                    }
                }

                if (cDead)
                    InternalUtility.showMessage("Some of the crops have died due to lack of water!");
            }

            // sanity check if the player hits 0 Stamina ( the game doesn't track this )
            if (Game1.player.Stamina <= 0f)
            {
                InternalUtility.FaintPlayer();
            }
        }

        private void CreateFog()
        {
           Color fogColor = Color.BlueViolet * 1f;
           Vector2 position = new Vector2();
           Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
           float num1 = -64 * Game1.pixelZoom + (int)((double)fogPos.X % (double)(64 * Game1.pixelZoom));
           while ((double)num1 < (double)Game1.graphics.GraphicsDevice.Viewport.Width)
           {
               float num2 = (float)(-64 * Game1.pixelZoom + (int)((double)this.fogPos.Y % (double)(64 * Game1.pixelZoom)));
               while ((double)num2 < (double)Game1.graphics.GraphicsDevice.Viewport.Height)
               {
                   position.X = (float)(int)num1;
                   position.Y = (float)(int)num2;
                   Game1.spriteBatch.Draw(Game1.mouseCursors, position, new Microsoft.Xna.Framework.Rectangle?(fogSource), (double)this.fogAlpha > 0.0 ? fogColor * fogAlpha : Color.Black * 0.95f, 0.0f, Vector2.Zero, (float)Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                   num2 += (float)(64 * Game1.pixelZoom);
               }
               num1 += (float)(64 * Game1.pixelZoom);
           }
            Game1.spriteBatch.End();

            if (fogAlpha == 0.0f)
                this.ambientFog = false;
        }

        public void spawnGhostOffScreen()
        {
            Vector2 zero = Vector2.Zero;
            var ourFarm = Game1.getFarm();

            if (ourFarm != null)
            {
                switch (Game1.random.Next(4))
                {
                    case 0:
                        zero.X = (float)dice.Next(ourFarm.map.Layers[0].LayerWidth);
                        break;
                    case 1:
                        zero.X = (float)(ourFarm.map.Layers[0].LayerWidth - 1);
                        zero.Y = (float)dice.Next(ourFarm.map.Layers[0].LayerHeight);
                        break;
                    case 2:
                        zero.Y = (float)(ourFarm.map.Layers[0].LayerHeight - 1);
                        zero.X = (float)dice.Next(ourFarm.map.Layers[0].LayerWidth);
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

            if (CurrWeather.todayHigh > Config.HeatwaveWarning && Game1.timeOfDay < 1830)
                tvText = tvText + "That it will be unusually hot outside. Stay hydrated and be careful not to stay too long in the sun. ";
            if (CurrWeather.todayHigh < -5)
                tvText = tvText + "There's an extreme cold snap passing through the valley. Stay warm. ";
            if (CurrWeather.todayLow < 2 && Config.HarshWeather)
                tvText = tvText + "Warning. We're getting frost tonight! Be careful what you plant! ";



            if (Game1.timeOfDay < noLonger) //don't display today's weather 
            {
                tvText += "The high for today is ";
                if (!Config.DisplaySecondScale)
                    tvText += WeatherHelper.DisplayTemperature(CurrWeather.todayHigh, Config.TempGauge) + ", with the low being " + WeatherHelper.DisplayTemperature(CurrWeather.todayLow, Config.TempGauge) + ". ";
                else //derp.
                    tvText += WeatherHelper.DisplayTemperature(CurrWeather.todayHigh, Config.TempGauge) + " (" + WeatherHelper.DisplayTemperature(CurrWeather.todayHigh, Config.SecondScaleGauge) + ") , with the low being " + WeatherHelper.DisplayTemperature(CurrWeather.todayLow, Config.TempGauge) + " (" + WeatherHelper.DisplayTemperature(CurrWeather.todayHigh, Config.SecondScaleGauge) + ") . ";

                if (Config.tooMuchInfo) LogEvent(tvText);

                //today weather
                tvText = tvText + WeatherHelper.GetWeatherDesc(dice, WeatherHelper.GetTodayWeather(), true);

                //get WeatherForTommorow and set text
                tvText = tvText + "#Tommorow, ";
            }

            //tommorow weather
            tvText = tvText + WeatherHelper.GetWeatherDesc(dice, (SDVWeather)Game1.weatherForTomorrow, false);

            return tvText;
        }
       
        private void LogEvent(string msg, bool important=false)
        {
            if (!important)
                Monitor.Log(msg, LogLevel.Debug);
            else
                Monitor.Log(msg, LogLevel.Info);            
        }

        public void checkForDangerousWeather(bool hud = true)
        {
            if (CurrWeather.status == FerngillWeather.BLIZZARD) {
                InternalUtility.showMessage("There's a dangerous blizzard out today. Be careful!");
                return;
            }

            if (CurrWeather.status == FerngillWeather.FROST && Game1.currentSeason != "winter")
            {
                InternalUtility.showMessage("The temperature tonight will be dipping below freezing. Your crops may be vulnerable to frost!");
                return;
            }

            if (CurrWeather.status == FerngillWeather.HEATWAVE)
            {
                InternalUtility.showMessage("A massive heatwave is sweeping the valley. Stay hydrated!");
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
                            if (CurrWeather.todayLow <= InternalUtility.CheckCropTolerance(curr.crop.indexOfHarvest) && dice.NextDouble() < Config.FrostHardiness)
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
                InternalUtility.showMessage("During the night, some crops died to the frost...");
                if (Config.tooMuchInfo) LogEvent("Setting frost test via queued message");
            }
        }

        public void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            if (!GameLoaded) //sanity check
                return;

            int[] beachItems = new int[] { 393, 397, 392, 394 };
            int[] moonBeachItems = new int[] { 393, 394, 560, 586, 587, 589, 397 };
            CurrWeather.status = 0; //reset status
            isExhausted = false; //reset disease
            UpdateWeather();    

            //moon processing
            if (SDVMoon.GetLunarPhase() == SDVMoon.NEWMOON && Config.MoonEffects)
            {
                Beach ourBeach = InternalUtility.getBeach();
                foreach (KeyValuePair<Vector2, StardewValley.Object> o in ourBeach.objects)
                {
                    if (beachItems.Contains(o.Value.parentSheetIndex))
                    {
                        if (dice.NextDouble() < .2)
                        {
                            ourBeach.objects.Remove(o.Key);
                        }                           
                    }
                }
            }
            
            //moon processing
            if (SDVMoon.GetLunarPhase() == SDVMoon.FULMOON && Config.MoonEffects)
            {
                int parentSheetIndex = 0;
                Rectangle rectangle = new Rectangle(65, 11, 25, 12);
                Beach ourBeach = InternalUtility.getBeach();
                for (int index = 0; index < 5; ++index)
                {
                    parentSheetIndex = moonBeachItems.GetRandomItem(dice);
                    if (dice.NextDouble() < .0001)
                        parentSheetIndex = 392; //rare chance

                    if (dice.NextDouble() < .8)
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
            if (Game1.weatherForTomorrow == Game1.weather_wedding)
            {
                if (Config.tooMuchInfo) LogEvent("There is no Alanis Morissetting here. Enjoy your wedding.");
                return;
            }

            if (Game1.countdownToWedding == 0 && (Game1.player.spouse != null && Game1.player.spouse.Contains("engaged")))
            {
                Game1.weatherForTomorrow = Game1.weather_wedding;
                if (Config.tooMuchInfo) LogEvent("Detecting the wedding tommorow. Setting weather and returning");
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
                    if (Config.tooMuchInfo) LogEvent("The game will force weather. Aborting.");
                    return;
                }

                if (Config.tooMuchInfo)
                    LogEvent("Change detected not caught by other checks. Debug Info: DAY " + Game1.dayOfMonth + " Season: " + Game1.currentSeason + " with prev weather: " + WeatherHelper.DescWeather(WeatherAtStartOfDay) + " and new weather: " + WeatherHelper.DescWeather(Game1.weatherForTomorrow) + ". Aborting.");

                return;
            }

            //TV forces.
            bool forceTomorrow = WeatherHelper.fixTV();
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
            double chance = dice.NextDouble();
            if (Config.tooMuchInfo)
                LogEvent("Rain Chance is: " + rainChance + " with the rng being " + chance);

            //override for the first spring.
            if (!WeatherHelper.CanWeStorm(Config))
                stormChance = 0;

            //global change - if it rains, drop the temps (and if it's stormy, drop the temps)
            if (Game1.isRaining)
            {
                if (Config.tooMuchInfo) LogEvent("Dropping temp by 4 from " + CurrWeather.todayHigh);
                CurrWeather.todayHigh = CurrWeather.todayHigh - 4;
                CurrWeather.todayLow = CurrWeather.todayLow - 2;
            }

            if (Config.ForceHeat)
            {
                if (Config.tooMuchInfo) LogEvent("Forcing Hazardous Weather: Heatwave conditions");
                CurrWeather.todayHigh = 50;
            }

            if (Config.ForceFrost)
            {
                if (Config.tooMuchInfo) LogEvent("Forcing Hazardous Weather: Frost conditions");
                CurrWeather.todayHigh = 0;
                CurrWeather.todayLow = -1;
            }


            if (forceSet)
                return;

            //sequence - rain (Storm), wind, sun
            //this also contains the notes - certain seasons don't have certain weathers.
            if (chance < rainChance || Game1.currentSeason != "winter")
            {
                chance = dice.NextDouble();
                if (chance < stormChance && stormChance != 0)
                {
                    if (Config.tooMuchInfo) LogEvent("Storm is selected, with roll " + chance + " and target percent " + stormChance);
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                }
                else
                {
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    if (Config.tooMuchInfo) LogEvent("Raining selected");
                }
            }
            else if (Game1.currentSeason != "winter")
            {
                if (chance < (windChance + rainChance) && (Game1.currentSeason == "spring" || Game1.currentSeason == "fall") && windChance != 0)
                {
                    Game1.weatherForTomorrow = Game1.weather_debris;
                    if (Config.tooMuchInfo) LogEvent("It's windy today, with roll " + chance + " and wind odds " + windChance);
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
                CurrWeather.todayHigh = 2;
                CurrWeather.todayLow = -1;
                Game1.weatherForTomorrow = Game1.weather_snow; //it now snows on Fall 28.
            }

            WeatherAtStartOfDay = Game1.weatherForTomorrow;
            Game1.chanceToRainTomorrow = rainChance; //set for various events.
            LogEvent("We've set the weather for tommorow . It is: " + WeatherHelper.DescWeather(Game1.weatherForTomorrow));
            ModRan = true;
        }

        private void HandleSpringWeather()
        {
            if (Config.tooMuchInfo) LogEvent("Executing Spring Weather");
            stormChance = .15;
            windChance = .25;
            rainChance = .3 + (Game1.dayOfMonth * .0278);

            CurrWeather.todayHigh = dice.Next(1, 8) + 8;
            CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);

            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                stormChance = .2;
                windChance = .15 + (Game1.dayOfMonth * .01);
                rainChance = .2 + (Game1.dayOfMonth * .01);            

                CurrWeather.todayHigh = dice.Next(1, 6) + 14;
                CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);
            }

            if (Game1.dayOfMonth > 18)
            {
                stormChance = .3;
                windChance = .05 + (Game1.dayOfMonth * .01);
                rainChance = .2;

                CurrWeather.todayHigh = dice.Next(1, 6) + 20;
                CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);
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
            if (Config.tooMuchInfo) LogEvent("Executing Summer Weather");
            if (Game1.dayOfMonth < 10)
            {
                //rain, snow, windy chances
                stormChance = .45;
                windChance = 0; //cannot wind during summer
                rainChance = .15;

                CurrWeather.todayHigh = dice.Next(1, 8) + 26;
                CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);
            }
            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                //rain, snow, windy chances
                stormChance = .6;
                windChance = 0; //cannot wind during summer
                rainChance = .15;

                CurrWeather.todayHigh = 30 + (int)Math.Floor(Game1.dayOfMonth * .25) + dice.Next(0,5);
                if (dice.NextDouble() > .70)
                {
                    if (Config.tooMuchInfo) LogEvent("Randomly adding to the temp");
                    CurrWeather.todayHigh += dice.Next(0, 3);
                }

                CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);
            }
            if (Game1.dayOfMonth > 18)
            {
                //temperature
                CurrWeather.todayHigh = 42 - (int)Math.Floor(Game1.dayOfMonth * .8) + dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);

                stormChance = .45;
                windChance = 0; 
                rainChance = .3;
            }

            //summer alterations
            CurrWeather.AlterTemps(dice.Next(0, 3));
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
            if (Config.tooMuchInfo) LogEvent("Executing Fall Weather");
            stormChance = .33;
            CurrWeather.todayHigh = 22 - (int)Math.Floor(Game1.dayOfMonth * .667) + dice.Next(0, 2); 

            if (Game1.dayOfMonth < 10)
            {
                windChance = 0 + (Game1.dayOfMonth * .044);
                rainChance = .3 + (Game1.dayOfMonth * .01111);
                CurrWeather.GetLowFromHigh(dice.Next(1, 6) + 4);
            }

            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                windChance = .4 + (Game1.dayOfMonth * .022);
                rainChance = 1 - windChance;
                CurrWeather.GetLowFromHigh(dice.Next(1, 6) + 3);
            }

            if (Game1.dayOfMonth > 18)
            {
                windChance = .1 + Game1.dayOfMonth * .044; 
                rainChance = .5;
                if (!Config.SetLowCap)
                    CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3);
                else
                    CurrWeather.GetLowFromHigh(dice.Next(1, 3) + 3, Config.LowCap);
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
            if (Config.tooMuchInfo) LogEvent("Executing Winter Weather");
            stormChance = 0;
            windChance = 0;
            rainChance = .6;

            if (Game1.dayOfMonth < 10)
            {
                CurrWeather.todayHigh = -2 + (int)Math.Floor(Game1.dayOfMonth * .889) + dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(dice.Next(1, 4));
            }

            if (Game1.dayOfMonth > 9 && Game1.dayOfMonth < 19)
            {
                CurrWeather.todayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.111) + dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(dice.Next(1, 4));
                rainChance = .75;
            }

            if (Game1.dayOfMonth > 18)
            {
                CurrWeather.todayHigh = -12 + (int)Math.Floor(Game1.dayOfMonth * 1.222) + dice.Next(0, 3);
                CurrWeather.GetLowFromHigh(dice.Next(1, 4));
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

