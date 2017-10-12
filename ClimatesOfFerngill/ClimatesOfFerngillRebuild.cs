using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using TwilightCore;
using TwilightCore.StardewValley;
using TwilightCore.PRNG;

using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;

using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using StardewValley.TerrainFeatures;

namespace ClimatesOfFerngillRebuild
{
    public class ClimatesOfFerngill : Mod
    {
        public static Dictionary<SDate, int> ForceDays = new Dictionary<SDate, int>
            {
                { new SDate(1,"spring"), Game1.weather_sunny },
                { new SDate(2, "spring"), Game1.weather_sunny },
                { new SDate(3, "spring"), Game1.weather_rain },
                { new SDate(4, "spring"), Game1.weather_sunny },
                { new SDate(13, "spring"), Game1.weather_festival },
                { new SDate(24, "spring"), Game1.weather_festival },
                { new SDate(1, "summer"), Game1.weather_sunny },
                { new SDate(11, "summer"), Game1.weather_festival },
                { new SDate(13, "summer"), Game1.weather_lightning },
                { new SDate(25, "summer", 25), Game1.weather_lightning },
                { new SDate(26, "summer", 26), Game1.weather_lightning },
                { new SDate(28, "summer", 28), Game1.weather_festival },
                { new SDate(1,"fall"), Game1.weather_sunny },
                { new SDate(16,"fall"), Game1.weather_festival },
                { new SDate(27,"fall"), Game1.weather_festival },
                { new SDate(1,"winter"), Game1.weather_sunny },
                { new SDate(8, "winter"), Game1.weather_festival },
                { new SDate(25, "winter"), Game1.weather_festival }
            };

        /// <summary> The options file </summary>
        private WeatherConfig WeatherOpt { get; set; }
        public bool RainTotemUsedToday { get; private set; }

        /// <summary> The pRNG object </summary>
        private MersenneTwister Dice;

        /// <summary> The current weather conditions </summary>
        private WeatherConditions CurrentWeather;

        /// <summary> The climate for the game </summary>
        private FerngillClimate GameClimate;

        /// <summary>
        /// Our fog object.
        /// </summary>
        private FerngillFog OurFog;

        /// <summary>
        /// Tracker to track if changes happened to weahter
        /// </summary>
        private int EndWeather;

        /// <summary>
        /// This is used to display icons on the menu
        /// </summary>
        private Sprites.Icons OurIcons { get; set; }

        private StringBuilder DebugOutput;
        private CustomWeather WeatherCntrl;
        private SDVMoon OurMoon;

        //for stamina management
        private StaminaDrain StaminaMngr;
        private int TicksOutside;
        private int TicksTotal;

        //for events
        private int ExpireTime;
        private List<Vector2> CropList;

        //queued string
        private HUDMessage queuedMsg;

        /// <summary>
        /// This is used to allow the menu to revert back to a previous menu
        /// </summary>
        private IClickableMenu PreviousMenu;

        //tv overloading
        private static FieldInfo Field = typeof(GameLocation).GetField("afterQuestion", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo TVChannel = typeof(TV).GetField("currentChannel", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo TVScreen = typeof(TV).GetField("screen", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo TVScreenOverlay = typeof(TV).GetField("screenOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo TVMethod = typeof(TV).GetMethod("getWeatherChannelOpening", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo TVMethodOverlay = typeof(TV).GetMethod("setWeatherOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
        private static GameLocation.afterQuestionBehavior Callback;
        private static TV Target;

        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>
        public override void Entry(IModHelper helper)
        {
            WeatherOpt = helper.ReadConfig<WeatherConfig>();
            Dice = new MersenneTwister();
            OurFog = new FerngillFog();
            WeatherCntrl = new CustomWeather();
            DebugOutput = new StringBuilder();
            OurMoon = new SDVMoon(Dice);
            OurIcons = new Sprites.Icons(Helper.Content);
            CropList = new List<Vector2>();
            StaminaMngr = new StaminaDrain(WeatherOpt, Helper.Translation, Monitor);
            queuedMsg = null;

            TicksOutside = 0;
            TicksTotal = 0;
            ExpireTime = 0;

            if (WeatherOpt.Verbose) Monitor.Log($"Loading climate type: {WeatherOpt.ClimateType} from file", LogLevel.Trace);

            string path = Path.Combine("data", "Weather", WeatherOpt.ClimateType + ".json");
            GameClimate = helper.ReadJsonFile<FerngillClimate>(path);

            CurrentWeather = new WeatherConditions();

            //subscribe to events
            TimeEvents.AfterDayStarted += HandleNewDay;
            SaveEvents.BeforeSave += OnEndOfDay;
            TimeEvents.TimeOfDayChanged += TenMinuteUpdate;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            GameEvents.UpdateTick += CheckForChanges;
            SaveEvents.AfterReturnToTitle += ResetMod;
            GraphicsEvents.OnPostRenderEvent += DrawObjects;
            Vector2 snowPos = Vector2.Zero;

            ControlEvents.KeyPressed += (sender, e) => this.ReceiveKeyPress(e.KeyPressed, this.WeatherOpt.Keyboard);
            MenuEvents.MenuClosed += (sender, e) => this.ReceiveMenuClosed(e.PriorMenu);

            //console commands
            helper.ConsoleCommands
                  .Add("weather_settommorowweather", helper.Translation.Get("console-text.desc_tmrweather"), TmrwWeatherChangeFromConsole)
                  .Add("weather_setweather", helper.Translation.Get("console-text.desc_setweather"), WeatherChangeFromConsole)
                  .Add("debug_changecondt", "Changes conditions. Debug function.", DebugChgCondition);
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            TryHookTelevision();
        }

        /// <summary>
        /// This function handles the end of the day.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndOfDay(object sender, EventArgs e)
        {
            if (CurrentWeather.UnusualWeather == SpecialWeather.Frost)
            {
                Farm f = Game1.getFarm();
                int count = 0, maxCrops = (int)Math.Floor(SDVUtilities.CropCountInFarm(f) * WeatherOpt.DeadCropPercentage);

                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (count >= maxCrops)
                        break;

                    if (tf.Value is HoeDirt curr && curr.crop != null)
                    {
                        if (Dice.NextDouble() <= (WeatherOpt.CropResistance / 2))
                        {
                            CropList.Add(tf.Key);
                            count++;
                        }
                    }
                }             

                if (count > 0)
                {
                    foreach (Vector2 v in CropList)
                    {
                        HoeDirt hd = (HoeDirt)f.terrainFeatures[v];
                        hd.crop.dead = true;
                    }

                    queuedMsg = new HUDMessage(Helper.Translation.Get("hud-text.desc_frost_killed"), Color.SeaGreen, 5250f, true)
                    {
                        whatType = 2
                    };
                }
            }

            //moon works after frost does
            OurMoon.HandleMoonAtSleep(Game1.getFarm(), Helper.Translation);
        }

        #region TVOverride
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
        #endregion

        /// <summary>
        /// This function gets the forecast of the weather for the TV. 
        /// </summary>
        /// <returns>A string describing the weather</returns>
        public string GetWeatherForecast()
        {
            string tvText = " ";

            //The TV should display: Alerts, today's weather, Tomorrow's weather, alerts.

            // Something such as "Today, the high is 12C, with low 8C. It'll be a very windy day. Tomorrow, it'll be rainy."
            // since we don't predict weather in advance yet. (I don't want to rearchitecture it yet.)
            // That said, the TV channel starts with Tomorrow, so we need to keep that in mind.

            tvText = Helper.Translation.Get("tv.opening-desc");

            if (Game1.timeOfDay < 1800) //don't display today's weather 
            {
                if (WeatherOpt.Verbose)
                    Monitor.Log("Triggering weather");

                //why was this not done seperately? Will proc tommorow
                //does.. Game1.isFestival() work the way I think it does...?
                if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
                {
                    tvText += Helper.Translation.Get("tv.desc-festival",
                                  new { festival = SDVUtilities.GetFestivalName(), Temperature = CurrentWeather.GetTemperatureString(WeatherOpt.ShowBothScales, Helper.Translation)});
                }
                else
                {

                    tvText += Helper.Translation.Get("tv.desc-today", new
                    {
                        temperature = CurrentWeather.GetTemperatureString(WeatherOpt.ShowBothScales, Helper.Translation),
                        weathercondition = CurrentWeather.GetDescText(CurrentWeather.TodayWeather, SDate.Now(), Dice, Helper.Translation)
                    });
                }
            }

            //Tomorrow weather
            tvText += Helper.Translation.Get("tv.desc-tomorrow", new
            {
                temperature = CurrentWeather.GetTomorrowTemperatureString(WeatherOpt.ShowBothScales, Helper.Translation),
                weathercondition = CurrentWeather.GetDescText(CurrentWeather.TomorrowWeather, SDate.Now().AddDays(1), Dice, Helper.Translation)
            });

            return tvText;
        }

        /// <summary>
        /// This checks for things every second.
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void CheckForChanges(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame)
                return;

            OurFog.MoveFog();

            if (Game1.isEating)
            { 
                StardewValley.Object obj = Game1.player.itemToEat as StardewValley.Object;

                if (obj.ParentSheetIndex == 351)
                {
                    StaminaMngr.ClearDrain();
                }

                if (WeatherOpt.Verbose)
                {
                    Monitor.Log($"Eating: {Game1.isEating} with object index {obj.ParentSheetIndex}");
                }
            }

            if (WeatherOpt.StormTotemChange)
            {
                if (Game1.weatherForTomorrow != EndWeather && !RainTotemUsedToday)
                {
                    if (WeatherOpt.Verbose)
                    {
                        Monitor.Log($"The current weather for tommorow is {Game1.weatherForTomorrow} with the previously set {EndWeather}");
                        Monitor.Log($"Rain totem set is {RainTotemUsedToday}");
                    }

                    RainTotemUsedToday = true;

                    if (Dice.NextDoublePositive() <= GameClimate.GetStormOdds(SDate.Now().AddDays(1), Dice, DebugOutput))
                    {
                        Game1.weatherForTomorrow = Game1.weather_lightning;
                        SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_stormtotem"));
                    }
                }
            }

            if (Game1.currentLocation.isOutdoors)
            {
                TicksOutside++;
            }

            TicksTotal++;            
        }

        private void TenMinuteUpdate(object sender, EventArgsIntChanged e)
        {
            if (!Game1.hasLoadedGame)
                return;

            OurFog.UpdateFog(e.NewInt, WeatherOpt.Verbose, Monitor);

            if (Game1.currentLocation.isOutdoors &&
                (CurrentWeather.UnusualWeather == SpecialWeather.Thundersnow ||
                 CurrentWeather.UnusualWeather == SpecialWeather.DryLightning)
                 && Game1.timeOfDay < 2400)
                Utility.performLightningUpdate();

            //queued messages clear
            if (Game1.timeOfDay == 610 && queuedMsg != null)
            {
                Game1.hudMessages.Add(queuedMsg);
                queuedMsg = null;
            }

            //frost works at night, heatwave works during the day
            if (Game1.timeOfDay == 1700)
            {
                if (WeatherConditions.IsHeatwave(CurrentWeather.UnusualWeather))
                {
                    ExpireTime = 2000;
                    Farm f = Game1.getFarm();
                    int count = 0, maxCrops = (int)Math.Floor(SDVUtilities.CropCountInFarm(f) * WeatherOpt.DeadCropPercentage);

                    foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                    {

                        if (count >= maxCrops)
                            break;

                        if (tf.Value is HoeDirt curr && curr.crop != null)
                        {
                            if (Dice.NextDouble() <= WeatherOpt.CropResistance)
                            {
                                CropList.Add(tf.Key);
                                curr.state = HoeDirt.dry;
                                count++;
                            }
                        }
                    }

                    if (CropList.Count > 0)
                    {
                        if (!WeatherOpt.AllowCropDeath)
                            SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_heatwave_dry"));
                        else
                            SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_heatwave_kill"));
                    }
                }
            }

            if (Game1.timeOfDay == ExpireTime && WeatherOpt.AllowCropDeath)
            {
                //if it's still de watered - kill it.
                Farm f = Game1.getFarm();
                bool cDead = false;

                foreach (Vector2 v in CropList)
                {
                    HoeDirt hd = (HoeDirt)f.terrainFeatures[v];
                    if (hd.state == HoeDirt.dry)
                    {
                        hd.crop.dead = true;
                        cDead = true;
                    }
                }

                if (cDead)
                    SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_heatwave_cropdeath"));
            }

            if (WeatherOpt.Verbose)
                Monitor.Log("Checking stamina");

            float oldStamina = Game1.player.stamina;
            Game1.player.stamina += StaminaMngr.TenMinuteTick(CurrentWeather.UnusualWeather, TicksOutside, TicksTotal);

            if (WeatherOpt.Verbose)
                Monitor.Log($"The stamina after the check is {Game1.player.stamina}, changed from {oldStamina}");

            if (Game1.player.stamina <= 0)
                SDVUtilities.FaintPlayer();

            TicksTotal = 0;
            TicksOutside = 0;
        }

        /// <summary>
        /// This event handles drawing to the screen.
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void DrawObjects(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
          
            if (Game1.currentLocation.IsOutdoors)
                OurFog.DrawFog();

            if (Game1.currentLocation.isOutdoors && !(Game1.currentLocation is Desert) && 
                CurrentWeather.UnusualWeather == SpecialWeather.Blizzard) 
                WeatherCntrl.DrawBlizzard(); 
        }

        private void ResetMod(object sender, EventArgs e)
        {
            CurrentWeather.Reset();
            ExpireTime = 0;
            CropList.Clear(); 
            DebugOutput.Clear();
            OurFog.Reset();
            StaminaMngr.Reset();
            TicksOutside = 0;
            TicksTotal = 0;
            RainTotemUsedToday = false;
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            CropList.Clear(); //clear the crop list
            DebugOutput.Clear();
            OurMoon.UpdateForNewDay();
            CurrentWeather.OnNewDay();
            UpdateWeatherOnNewDay();
            OurMoon.HandleMoonAfterWake(Helper.Translation);
            StaminaMngr.OnNewDay(CurrentWeather);
            TicksOutside = 0;
            ExpireTime = 0;
            TicksTotal = 0;
        }

        private void UpdateWeatherOnNewDay()
        {
            if (Game1.dayOfMonth == 0) //do not run on day 0.
                return;

            //Get starting value.
            int TmrwWeather = Game1.weatherForTomorrow;

            //reset for new day
            OurFog.Reset();

            //Set Temperature for today and tommorow. Get today's conditions.
            //   If tomorrow is set, move it to today, and autoregen tomorrow.
            CurrentWeather.GetTodayWeather();

            if (CurrentWeather.TomorrowTemps == null)
                CurrentWeather.SetTodayTemps(GameClimate.GetTemperatures(SDate.Now(), Dice, DebugOutput));
            else
                CurrentWeather.SetTodayTemps(CurrentWeather.TomorrowTemps);

            CurrentWeather.SetTmrwTemps(GameClimate.GetTemperatures(SDate.Now().AddDays(1), Dice, DebugOutput));

            if (WeatherOpt.Verbose)
                Monitor.Log($"Updated the temperature for tommorow and today. Setting weather for today... ", LogLevel.Trace);

            //if today is a festival or wedding, do not go further.
            if (Utility.isFestivalDay(SDate.Now().Day, SDate.Now().Season) || CurrentWeather.TodayWeather == Game1.weather_wedding)
            {
                if (WeatherOpt.Verbose)
                    Monitor.Log("It is a wedding or festival today. Not attempting to run special weather or fog.");

                //if (WeatherOpt.Verbose) Monitor.Log(DebugOutput.ToString());
                return;
            }

            //now, update today's weather for fog and other special weathers.
            double fogChance = GameClimate.GetClimateForDate(SDate.Now())
                                          .RetrieveOdds(Dice, "fog", SDate.Now().Day, DebugOutput);

            fogChance = 1; //for testing purposes
            double fogRoll = Dice.NextDoublePositive();
           
            if (fogRoll < fogChance && CurrentWeather.TodayWeather != Game1.weather_debris)
            {
                if (WeatherOpt.Verbose)
                    Monitor.Log("Executing fog analysis.. ");

                CurrentWeather.WillFog = true;

                OurFog.CreateFog(FogAlpha: .55f, AmbientFog: true, FogColor: (Color.White * 1.35f));               

                if (Dice.NextDoublePositive() < .15)
                {
                    OurFog.IsDarkFog();

                    if (WeatherOpt.Verbose)
                        Monitor.Log("Dark fog!");

                    Game1.outdoorLight = new Color(214, 210, 208);
                }
                else
                {
                    Game1.outdoorLight = new Color(180, 155, 110);
                }

                double FogTimer = Dice.NextDoublePositive();
                SDVTime FogExpirTime = new SDVTime(1200);

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

                OurFog.FogExpirTime = FogExpirTime;

                if (WeatherOpt.Verbose)
                    Monitor.Log($"With roll {fogRoll.ToString("N3")} against {fogChance}, there will be fog today until {OurFog.FogExpirTime}");
            }

            //now special weathers
            //there are three main special weathers. Blizard, only during snow; Dry Lightning, which is lightning minus rain; 
            //  Thundersnow

            // Conditions: Blizzard - occurs in weather_snow in "winter"
            //             Dry Lightning - occurs in weather_clear in any season if temps are >24C.
            //             Thundersnow  - as Blizzard, but really rare.

            // And now, with stamina enabled, time to reenable heatwaves and frosts

            if (WeatherOpt.Verbose)
                Monitor.Log("Testing for special weathers - first, blizzard and thundrsnow");

            if (CurrentWeather.TodayWeather == Game1.weather_snow)
            {
                double blizRoll = Dice.NextDoublePositive();
                if (blizRoll <= WeatherOpt.BlizzardOdds)
                {
                    CurrentWeather.UnusualWeather = SpecialWeather.Blizzard;
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"With roll {blizRoll.ToString("N3")} against {WeatherOpt.BlizzardOdds}, there will be blizzards today");
                }
            }

            //Dry Lightning is also here for such like the dry and arid climates 
            //  which have so low rain chances they may never storm.
            if (CurrentWeather.TodayWeather == Game1.weather_snow)
            {
                double oddsRoll = Dice.NextDoublePositive();

                if (oddsRoll <= WeatherOpt.ThundersnowOdds)
                {
                    CurrentWeather.UnusualWeather = SpecialWeather.Thundersnow;
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"With roll {oddsRoll.ToString("N3")} against {WeatherOpt.ThundersnowOdds}, there will be thundersnow today");
                }
            }

            if (WeatherOpt.Verbose)
                Monitor.Log("Testing for special weathers - dry lightning and heatwave");

            if (CurrentWeather.TodayWeather == Game1.weather_sunny)
            {
                double oddsRoll = Dice.NextDoublePositive();

                if (oddsRoll <= WeatherOpt.DryLightning && CurrentWeather.GetTodayHigh() >= WeatherOpt.DryLightningMinTemp)
                {
                    CurrentWeather.UnusualWeather = SpecialWeather.DryLightning;
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"With roll {oddsRoll.ToString("N3")} against {WeatherOpt.DryLightning}, there will be dry lightning today.");
                }

                if (CurrentWeather.GetTodayHigh() > WeatherOpt.TooHotOutside && WeatherOpt.HazardousWeather)
                {
                    if (CurrentWeather.UnusualWeather == SpecialWeather.DryLightning)
                        CurrentWeather.UnusualWeather = SpecialWeather.DryLightningAndHeatwave;
                    else
                        CurrentWeather.UnusualWeather = SpecialWeather.Heatwave;
                }
            }

            if (WeatherOpt.Verbose)
                Monitor.Log("Testing for special weathers - frost.");

            if (CurrentWeather.GetTodayLow() < WeatherOpt.TooColdOutside && !Game1.IsWinter)
            {
                if (WeatherOpt.HazardousWeather)
                {
                    CurrentWeather.UnusualWeather = SpecialWeather.Frost;
                }
            }


            //if tomorrow is a festival or wedding, we need to set the weather and leave.
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.weatherForTomorrow = Game1.weather_festival;
                if (WeatherOpt.Verbose)
                    Monitor.Log($"Festival tomorrow. Aborting processing.", LogLevel.Trace);

                //if (WeatherOpt.Verbose) Monitor.Log(DebugOutput.ToString());
                return;
            }

            if (Game1.countdownToWedding == 1)
            {
                Game1.weatherForTomorrow = Game1.weather_wedding;
                if (WeatherOpt.Verbose)
                    Monitor.Log($"Wedding tomorrow. Aborting processing.", LogLevel.Trace);

                return;
            }

            if (CheckForForceDay(SDate.Now().AddDays(1)))
            {
                if (WeatherOpt.Verbose)
                    Monitor.Log($"The game will force tomorrow. Aborting processing.", LogLevel.Trace);

                //if (WeatherOpt.Verbose) Monitor.Log(DebugOutput.ToString());
                return;
            }

            if (WeatherOpt.Verbose)
                Monitor.Log("Setting weather for tomorrow");

            //now set tomorrow's weather
            var OddsForTheDay = GameClimate.GetClimateForDate(SDate.Now().AddDays(1));

            double rainDays = OddsForTheDay.RetrieveOdds(Dice, "rain", SDate.Now().AddDays(1).Day, DebugOutput);
            double windyDays = OddsForTheDay.RetrieveOdds(Dice, "debris", SDate.Now().AddDays(1).Day, DebugOutput);
            double stormDays = OddsForTheDay.RetrieveOdds(Dice, "storm", SDate.Now().AddDays(1).Day, DebugOutput);

            ProbabilityDistribution<string> WeatherDist = new ProbabilityDistribution<string>("sunny");
            WeatherDist.AddNewEndPoint(rainDays, "rain");
            WeatherDist.AddNewCappedEndPoint(windyDays, "debris");

            if (!(WeatherDist.GetEntryFromProb(Dice.NextDoublePositive(), out string Result)))
            {
                Result = "sunny";
                Monitor.Log("The weather has failed to process in some manner. Falling back to [sunny]", LogLevel.Info);
            }

            if (WeatherOpt.Verbose)
                Monitor.Log($"Weather result is {Result}");

            //now parse the result.
            if (Result == "rain")
            {
                //snow applies first
                double MidPointTemp = CurrentWeather.GetTodayHigh() - 
                    ((CurrentWeather.GetTodayHigh() - CurrentWeather.GetTodayLow()) / 2);

                if ((CurrentWeather.GetTodayHigh() <= 2 || MidPointTemp <= 0 ) && Game1.currentSeason != "spring")
                {
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"Snow is enabled, with the High for the day being: {CurrentWeather.TodayTemps.HigherBound}" +
                                    $" and the calculated midpoint temperature being {MidPointTemp}");

                    Game1.weatherForTomorrow = Game1.weather_snow;
                }
                else
                {
                    Game1.weatherForTomorrow = Game1.weather_rain;
                }

                if (!GameClimate.AllowRainInWinter && Game1.currentSeason == "winter" && Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    Game1.weatherForTomorrow = Game1.weather_snow;
                }

                //apply lightning logic.
                if (Dice.NextDoublePositive() >= stormDays && Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                    if (SDate.Now().Year == 1 && SDate.Now().Season == "spring" && !WeatherOpt.AllowStormsSpringYear1)
                        Game1.weatherForTomorrow = Game1.weather_rain;
                }

                //apply dry lightning check
                if (CurrentWeather.UnusualWeather == SpecialWeather.DryLightningAndHeatwave || CurrentWeather.UnusualWeather == SpecialWeather.DryLightning)
                    Game1.isLightning = true;

                //tracking time!
                //Snow fall on Fall 28, if the flag is set.
                if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && WeatherOpt.SnowOnFall28)
                {
                    CurrentWeather.ResetTodayTemps(2, -1);
                    Game1.weatherForTomorrow = Game1.weather_snow;
                }

            }

            if (Result == "debris")
            {
                Game1.weatherForTomorrow = Game1.weather_debris;
            }

            if (Result == "sunny")
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
            }

            if (WeatherOpt.Verbose)
                Monitor.Log($"We've set the weather for Tomorrow. It is: {Game1.weatherForTomorrow}");

            CurrentWeather.TomorrowWeather = Game1.weatherForTomorrow; //would help if I updated this!

            //set trackers
            EndWeather = Game1.weatherForTomorrow;
        }

        private bool CheckForForceDay(SDate Target)
        {           
            if (Game1.year == 1 && Target.Season == "spring" && Target.Day == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return true;
            }

            foreach (KeyValuePair<SDate, int> entry in ForceDays)
            {
                if (entry.Key.Day == Target.Day && entry.Key.Season == Target.Season)
                {
                    Game1.weatherForTomorrow = entry.Value;
                    EndWeather = entry.Value; 
                    return true;
                }
            }

            return false;
        }

        /* **************************************************************
         * console commands
         * **************************************************************
         */

        /// <summary>
        /// This function changes the weather (Console Command)
        /// </summary>
        /// <param name="arg1">The command used</param>
        /// <param name="arg2">The console command parameters</param>
        private void DebugChgCondition(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
                return;

            string ChosenWeather = arg2[0];

            switch (ChosenWeather)
            {
                case "blizzard":
                    WeatherChangeFromConsole("blah", new string[] { "snow" });
                    CurrentWeather.UnusualWeather = SpecialWeather.Blizzard;
                    break;
                case "reset":
                    WeatherChangeFromConsole("blah", new string[] { "sunny" });
                    CurrentWeather.UnusualWeather = SpecialWeather.None;
                    OurFog.Reset();
                    break;
                case "thundersnow":
                    WeatherChangeFromConsole("blah", new string[] { "snow" });
                    CurrentWeather.UnusualWeather = SpecialWeather.Thundersnow;
                    break;
                case "drylightning":
                    WeatherChangeFromConsole("blah", new string[] { "sunny" });
                    CurrentWeather.UnusualWeather = SpecialWeather.DryLightning;
                    break;
                case "fog":
                    OurFog.CreateFog(FogAlpha: 1f, AmbientFog: true, FogColor: Color.White * 1.35f);
                    OurFog.FogExpirTime = new SDVTime(1900);
                    break;
            }
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
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_rain"), LogLevel.Info);
                    break;
                case "storm":
                    Game1.isSnowing = Game1.isDebrisWeather = false;
                    Game1.isLightning = Game1.isRaining = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_storm"), LogLevel.Info);
                    break;
                case "snow":
                    Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
                    Game1.isSnowing = true;
                    Game1.debrisWeather.Clear();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_snow"), LogLevel.Info);
                    break;
                case "debris":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = false;
                    Game1.isDebrisWeather = true;
                    Game1.populateDebrisWeatherArray();
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_debris", LogLevel.Info));
                    break;
                case "sunny":
                    Game1.isSnowing = Game1.isLightning = Game1.isRaining = Game1.isRaining = false;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset_sun", LogLevel.Info));
                    break;
            }

            Game1.updateWeatherIcon();
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
            switch (ChosenWeather)
            {
                case "rain":
                    Game1.weatherForTomorrow = Game1.weather_rain;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwrain"), LogLevel.Info);
                    break;
                case "storm":
                    Game1.weatherForTomorrow = Game1.weather_lightning;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwstorm"), LogLevel.Info);
                    break;
                case "snow":
                    Game1.weatherForTomorrow = Game1.weather_snow;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwsnow"), LogLevel.Info);
                    break;
                case "debris":
                    Game1.weatherForTomorrow = Game1.weather_debris;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwdebris"), LogLevel.Info);
                    break;
                case "festival":
                    Game1.weatherForTomorrow = Game1.weather_festival;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwfestival"), LogLevel.Info);
                    break;
                case "sun":
                    Game1.weatherForTomorrow = Game1.weather_sunny;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwsun"), LogLevel.Info);
                    break;
                case "wedding":
                    Game1.weatherForTomorrow = Game1.weather_wedding;
                    Monitor.Log(Helper.Translation.Get("console-text.weatherset-tmrwwedding"), LogLevel.Info);
                    break;
            }
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
            Game1.activeClickableMenu = new WeatherMenu(Monitor, this.Helper.Reflection, OurIcons, Helper.Translation, 
                CurrentWeather, OurMoon, WeatherOpt, Dice);
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
