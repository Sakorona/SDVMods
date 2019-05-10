using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using TwilightShards.Stardew.Common;
using TwilightShards.Common;
using Microsoft.Xna.Framework.Graphics;
using EnumsNET;
using PyTK.CustomTV;
using Harmony;
using System.Reflection;
using ClimatesOfFerngillRebuild.Patches; 

namespace ClimatesOfFerngillRebuild
{
    public class ClimatesOfFerngill : Mod
    {
        /// <summary> The options file </summary>
        internal static WeatherConfig WeatherOpt { get; set; }

        /// <summary> The pRNG object </summary>
        internal static MersenneTwister Dice;

        /// <summary> The current weather conditions </summary>
        internal static WeatherConditions Conditions;

        //provide common interfaces for logging
        internal static IMonitor Logger;
        internal static IReflectionHelper Reflection;
        internal static IMultiplayerHelper MPHandler;
        internal static ITranslationHelper Translator;

        /// <summary> The climate for the game </summary>
        private static FerngillClimate GameClimate;

        /// <summary> This is used to display icons on the menu </summary>
        internal static Sprites.Icons OurIcons { get; set; }

        private List<Vector2> CropList;
        private HUDMessage queuedMsg;
        private int ExpireTime;
        internal static ClimateTracker trackerModel;

        /// <summary> This is used to allow the menu to revert back to a previous menu </summary>
        private IClickableMenu PreviousMenu;
        private Descriptions DescriptionEngine;
        private Rectangle RWeatherIcon;
        private bool Disabled = false;
        private bool HasGottenSync = false;
        private bool HasRequestedSync = false;
        private static bool IsBloodMoon = false;
        private float weatherX;
        internal static bool IsVariableRain { get; private set;}
        private int TenMCounter;
        internal static int AmtOfRainDrops { get; private set; }
        private bool SummitRebornLoaded;

        //Integrations
        internal static bool UseLunarDisturbancesApi = false;
        internal static Integrations.ILunarDisturbancesAPI MoonAPI;
        internal static bool UseSafeLightningApi = false;
        internal static Integrations.ISafeLightningAPI SafeLightningAPI;
        internal static bool UseDynamicNightApi = false;
        internal static Integrations.IDynamicNightAPI DynamicNightAPI;

        /// <summary> Provide an API interface </summary>
        private IClimatesOfFerngillAPI API;
        public override object GetApi()
        {
            if (API == null)
                API = new ClimatesOfFerngillAPI(Conditions);

            return API;
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            RWeatherIcon = new Rectangle();
            WeatherOpt = helper.ReadConfig<WeatherConfig>();
            Logger = Monitor;
            Translator = Helper.Translation;
            Reflection = Helper.Reflection;
            MPHandler = Helper.Multiplayer;
            Dice = new MersenneTwister();
            OurIcons = new Sprites.Icons(Helper.Content);
            CropList = new List<Vector2>();
            Conditions = new WeatherConditions();
            DescriptionEngine = new Descriptions();
            queuedMsg = null;
            SummitRebornLoaded = false;
            TenMCounter = 0;
            ExpireTime = 0;
            IsVariableRain = false;
            //RainAmt = 0.0;

            if (WeatherOpt.Verbose) Monitor.Log($"Loading climate type: {WeatherOpt.ClimateType} from file", LogLevel.Trace);

            var path = Path.Combine("data", "weather", WeatherOpt.ClimateType + ".json");
            GameClimate = helper.Data.ReadJsonFile<FerngillClimate>(path); 
            
            if (GameClimate is null)
            {
                this.Monitor.Log($"The required '{path}' file is missing. Try reinstalling the mod to fix that.", LogLevel.Error);
                this.Monitor.Log("This mod will now disable itself.", LogLevel.Error);
                this.Disabled = true;
            }

            if (Disabled) return;

            var harmony = HarmonyInstance.Create("koihimenakamura.climatesofferngill");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            ConsoleCommands.Init();

            //patch SGame::DarkImpl
            //patch GameLocation::drawAboveAlwaysFrontLayer
            MethodInfo GameLocationDAAFL = AccessTools.Method(typeof(StardewValley.GameLocation), "drawAboveAlwaysFrontLayer");
            HarmonyMethod DAAFLTranspiler = new HarmonyMethod(AccessTools.Method(typeof(GameLocationPatches), "Transpiler"));
            Monitor.Log($"Patching {GameLocationDAAFL} with Transpiler: {DAAFLTranspiler}", LogLevel.Trace); ;
            harmony.Patch(GameLocationDAAFL, transpiler: DAAFLTranspiler);

            Type t = AccessTools.TypeByName("StardewModdingAPI.Framework.SGame");
            MethodInfo SGameDrawImpl = AccessTools.Method(t, "DrawImpl");
            HarmonyMethod DrawTrans = new HarmonyMethod(AccessTools.Method(typeof(SGamePatches), "Transpiler"));
            Monitor.Log($"Patching {SGameDrawImpl} with Transpiler: {DrawTrans}", LogLevel.Trace);
            harmony.Patch(SGameDrawImpl,transpiler: DrawTrans);

            t = AccessTools.TypeByName("StardewValley.WeatherDebris");
            var DebrisConstructor = AccessTools.Constructor(t, new[] { typeof(Vector2), typeof (int), typeof(float), typeof(float), typeof(float)});
            HarmonyMethod CtorPatch = new HarmonyMethod(AccessTools.Method(typeof(WeatherDebrisPatches), "CtorPostfix"));
            Monitor.Log($"Patching {DebrisConstructor} with Postfix: {CtorPatch}", LogLevel.Trace);
            harmony.Patch(DebrisConstructor, postfix: CtorPatch);

            MethodInfo DebrisDraw = AccessTools.Method(typeof(WeatherDebris), "draw");
            HarmonyMethod DebrisPatch = new HarmonyMethod(AccessTools.Method(typeof(WeatherDebrisPatches), "DrawPrefix"));
            Monitor.Log($"Patching {DebrisDraw} with Prefix: {DebrisDraw}", LogLevel.Trace);
            harmony.Patch(DebrisDraw,prefix: DebrisPatch);

            MethodInfo DebrisUpdate = AccessTools.Method(typeof(WeatherDebris), "update", new[] { typeof(bool) });
            HarmonyMethod DebrisUpdatePatch = new HarmonyMethod(AccessTools.Method(typeof(WeatherDebrisPatches), "UpdatePrefix"));
            Monitor.Log($"Patching {DebrisUpdate} with Prefix: {DebrisUpdatePatch}", LogLevel.Trace);
            harmony.Patch(DebrisUpdate, prefix: DebrisUpdatePatch);

            //subscribe to events
            var events = helper.Events;
            events.GameLoop.DayStarted += OnDayStarted;
            events.GameLoop.Saving += OnSaving;
            events.GameLoop.TimeChanged += OnTimeChanged;
            events.Display.MenuChanged += OnMenuChanged;
            events.GameLoop.UpdateTicked += OnUpdateTicked;
            events.GameLoop.GameLaunched += OnGameLaunched;
            events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            events.GameLoop.SaveLoaded += OnSaveLoaded;
            events.Display.RenderingHud += OnRenderingHud;
            events.Display.RenderedHud += OnRenderedHud;
            events.Player.Warped += OnWarped;
            events.Input.ButtonPressed += OnButtonPressed;
            events.Multiplayer.ModMessageReceived += OnModMessageRecieved;

            //console commands
            helper.ConsoleCommands
                .Add("world_tmrwweather", helper.Translation.Get("console-text.desc_tmrweather"), ConsoleCommands.TomorrowWeatherChangeFromConsole)
                .Add("world_setweather", helper.Translation.Get("console-text.desc_setweather"), ConsoleCommands.WeatherChangeFromConsole)
                .Add("debug_clearspecial", "debug command to clear special weathers", ConsoleCommands.ClearSpecial)
                .Add("debug_weatherstatus","!", ConsoleCommands.OutputWeather)
                .Add("debug_sswa","!", ConsoleCommands.ShowSpecialWeather)
                .Add("debug_vrainc","Set Rain Amt.", ConsoleCommands.SetRainAmt);
        }

        private static int GetPixelZoom()
        {
            FieldInfo field = typeof(Game1).GetField(nameof(Game1.pixelZoom), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new InvalidOperationException($"The {nameof(Game1)}.{nameof(Game1.pixelZoom)} field could not be found.");
            return (int)field.GetValue(null);
        }

        /// <summary>Raised once per second after the game state is updated.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            Conditions.SecondUpdate();
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var modManifest = Helper.ModRegistry.Get("KoihimeNakamura.summitreborn");
            if (modManifest != null)
            {
                SummitRebornLoaded = true;
                Logger.Log("Summit Reborn loaded. Disabling summit redraw.");
            }

            //testing for ZA MOON, YOUR HIGHNESS.
            MoonAPI = SDVUtilities.GetModApi<Integrations.ILunarDisturbancesAPI>(Monitor, Helper, "KoihimeNakamura.LunarDisturbances", "1.0");
            SafeLightningAPI = SDVUtilities.GetModApi<Integrations.ISafeLightningAPI>(Monitor, Helper, "cat.safelightning", "1.0");
            DynamicNightAPI = SDVUtilities.GetModApi<Integrations.IDynamicNightAPI>(Monitor, Helper, "knakamura.dynamicnighttime", "1.1-rc3");

            if (MoonAPI != null)
                UseLunarDisturbancesApi = true;
                    
            if (SafeLightningAPI != null)
                UseSafeLightningApi = true;

            if (DynamicNightAPI != null)
                UseDynamicNightApi = true;
        }
        
        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            CustomTVMod.changeAction("weather", DisplayWeather);

            if (Context.IsMainPlayer)
            {
                trackerModel = this.Helper.Data.ReadSaveData<ClimateTracker>("climate-tracker");
            }
        } 

        public static void SetVariableRain(bool isVariable)
        {
            IsVariableRain = isVariable;
        }

        public static void SetRainAmt(int rainAmt)
        {
            AmtOfRainDrops = rainAmt;
            Array.Resize(ref Game1.rainDrops, AmtOfRainDrops);
            if (Game1.IsMasterGame)
                Logger.Log($"Setting rain to {AmtOfRainDrops}");
            else
            {
                Logger.Log($"Setting from master: rain to {AmtOfRainDrops}");
            }
                
        }
        
        public void DisplayWeather(TV tv, TemporaryAnimatedSprite sprite, Farmer who, string answer)
        {
            TemporaryAnimatedSprite BackgroundSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(497, 305, 42, 28), 9999f, 1, 999999, tv.getScreenPosition(), false, false, (float)((tv.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);
            TemporaryAnimatedSprite WeatherSprite = DescriptionEngine.GetWeatherOverlay(Conditions, tv);

            string OnScreenText = "";

            if (BackgroundSprite is null)
                Monitor.Log("Background Sprite is null");
            if (WeatherSprite is null)
                Monitor.Log("Weather Sprite is null");

            string MoonPhase = "";
            if (UseLunarDisturbancesApi)
                MoonPhase = MoonAPI.GetCurrentMoonPhase();

            OnScreenText += DescriptionEngine.GenerateTVForecast(Conditions, MoonPhase);

            CustomTVMod.showProgram(BackgroundSprite, OnScreenText, CustomTVMod.endProgram, WeatherSprite);
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.currentLocation.IsOutdoors)
                Conditions.DrawWeathers();

            if (Game1.isRaining && Game1.currentLocation.IsOutdoors && (Game1.currentLocation is Summit) && !SummitRebornLoaded &&
                (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / Game1.tileSize), (float)(Game1.viewport.Y / Game1.tileSize)))))
            {
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[index].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[index].frame, -1, -1)), Color.White);
                }

                if (WeatherOpt.ShowSummitClouds)
                {
                    int num2 = -61 * GetPixelZoom();
                    while (num2 < Game1.viewport.Width + 61 * GetPixelZoom())
                    {
                        Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)num2 + this.weatherX % (float)(61 * GetPixelZoom()), (float)(-Game1.tileSize / 2)), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.DarkSlateGray * 1f, 0.0f, Vector2.Zero, (float)GetPixelZoom(), SpriteEffects.None, 1f);
                        num2 += 61 * GetPixelZoom();
                    }
                }
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && Conditions.HasWeather(CurrentWeather.Fog))
            {
                if (Game1.currentLocation is DecoratableLocation loc && !loc.IsOutdoors && WeatherOpt.DarkenLightInFog)
                {
                    foreach (var f in loc.furniture)
                    {
                        if (f.furniture_type.Value == Furniture.window)
                        {
                            //if (WeatherOpt.Verbose) Monitor.Log($"Attempting to remove the light for {f.name}");
                            Helper.Reflection.GetMethod(f, "addLights").Invoke(Game1.currentLocation);
                        }
                    }
                }
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // restore previous menu on close
            if (e.OldMenu is WeatherMenu && this.PreviousMenu != null)
            {
                Game1.activeClickableMenu = this.PreviousMenu;
                this.PreviousMenu = null;
            }

            // bandle new dialogue box
            else if (e.NewMenu is DialogueBox box)
            {
                bool stormDialogue = false;
                double odds = Dice.NextDoublePositive(), stormOdds = GameClimate.GetStormOdds(SDate.Now().AddDays(1), Dice);
                List<string> lines = Helper.Reflection.GetField<List<string>>(box, "dialogues").GetValue();
                if (lines.FirstOrDefault() == Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"))
                {
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"Rain totem interception firing with roll {odds:N3} vs. odds {stormOdds:N3}");

                    // rain totem used, do your thing
                    if (WeatherOpt.StormTotemChange)
                    {
                        if (odds <= stormOdds)
                        {
                            if (WeatherOpt.Verbose)
                                Monitor.Log("Replacing rain with storm..");

                            Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_lightning;
                            stormDialogue = true;
                        }
                    }

                    // change dialogue text
                    lines.Clear();
                    lines.Add(stormDialogue
                        ? Helper.Translation.Get("hud-text.desc_stormtotem")
                        : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"));
                }
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            this.Helper.Data.WriteSaveData("climate-tracker", trackerModel);

            if (Conditions.HasWeather(CurrentWeather.Frost) && WeatherOpt.AllowCropDeath)
            {
                Farm f = Game1.getFarm();
                int count = 0,
                    maxCrops = (int) Math.Floor(SDVUtilities.CropCountInFarm(f) * WeatherOpt.DeadCropPercentage);

                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures.Pairs)
                {
                    if (count >= maxCrops)
                        break;

                    if (tf.Value is HoeDirt curr && curr.crop != null)
                    {
                        if (Dice.NextDouble() > WeatherOpt.CropResistance)
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
                        HoeDirt hd = (HoeDirt) f.terrainFeatures[v];
                        hd.crop.dead.Value = true;
                    }

                    queuedMsg = new HUDMessage(
                        Helper.Translation.Get("hud-text.desc_frost_killed", new {deadCrops = count}),
                        Color.SeaGreen, 5250f, true)
                    {
                        whatType = 2
                    };
                }
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Context.IsMultiplayer && !Context.IsMainPlayer && Context.IsPlayerFree && !HasRequestedSync)
            {
                Monitor.Log("Requesting climate information");
                this.Helper.Multiplayer.SendMessage<long>(Game1.player.UniqueMultiplayerID, "NewFarmHandJoin", new[] {"KoihimeNakamura.ClimatesOfFerngill" });
                HasRequestedSync = true;
            }

            if (Game1.currentGameTime != null)
            {
                this.weatherX += (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.03f;
            }

            // check for changes
            Conditions.MoveWeathers();  
            
            if (UseLunarDisturbancesApi)
            {
                if (MoonAPI.GetCurrentMoonPhase() == "Blood Moon") 
                { 
                    IsBloodMoon = true;
                }
            }
            
        }

        /// <summary>Raised after the in-game clock time changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!Game1.hasLoadedGame)
                return;

            if (!Context.IsMainPlayer)
                return;

            Conditions.TenMinuteUpdate();

            if (Conditions.HasWeather(CurrentWeather.Fog)) 
            {
                if (!Game1.currentLocation.IsOutdoors && Game1.currentLocation is DecoratableLocation && 
                    WeatherOpt.DarkenLightInFog)
                {
                    var loc = (DecoratableLocation) Game1.currentLocation;
                    foreach (Furniture f in loc.furniture)
                    {
                        //Yes, *add* lights removes them. No, don't ask me why.
                        if (f.furniture_type.Value == Furniture.window)
                        {
                            //if (WeatherOpt.Verbose) Monitor.Log($"Attempting to remove the light for {f.name}");
                            Helper.Reflection.GetMethod(f, "addLights").Invoke(Game1.currentLocation);
                        }
                    }
                }
            }

            TenMCounter++;

            if (TenMCounter == 3)
            {
                if (IsVariableRain)
                {
                    if (Dice.NextDouble() < WeatherOpt.VRChangeChance)
                    {
                        double chance = WeatherOpt.VRMassiveStepChance;
                        if (AmtOfRainDrops > 200)
                            chance /= 2; //cut this chance in half.
                        if (AmtOfRainDrops > 600)
                            chance /= 2; //cut it in half *again*

                        if (Dice.NextDouble() < chance)                      
                            AmtOfRainDrops = WeatherUtilities.GetNextHighestRainCategoryBeginning(AmtOfRainDrops);
                        else
                            AmtOfRainDrops = (int)Math.Floor(AmtOfRainDrops * (1.0 + Dice.RollInRange(-1.0 * WeatherOpt.VRStepPercent, WeatherOpt.VRStepPercent)));

                        SetRainAmt(AmtOfRainDrops);
                        Conditions.GenerateWeatherSync();
                    }

                }
                TenMCounter = 0;
            }

            if (Game1.currentLocation.IsOutdoors && Conditions.HasWeather(CurrentWeather.Lightning) && !Conditions.HasWeather(CurrentWeather.Rain) && Game1.timeOfDay < 2400)
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
                if (Conditions.HasWeather(CurrentWeather.Heatwave) || Conditions.HasWeather(CurrentWeather.Sandstorm))
                {
                    ExpireTime = 2000;
                    Farm f = Game1.getFarm();
                    int count = 0, maxCrops = (int)Math.Floor(SDVUtilities.CropCountInFarm(f) * WeatherOpt.DeadCropPercentage);

                    foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures.Pairs)
                    {
                        if (count >= maxCrops)
                            break;

                        if (tf.Value is HoeDirt curr && curr.crop != null)
                        {
                            if (Dice.NextDouble() <= WeatherOpt.CropResistance)
                            {
                                CropList.Add(tf.Key);
                                curr.state.Value = HoeDirt.dry;
                                count++;
                            }
                        }
                    }

                    if (CropList.Count > 0)
                    {
                        if (WeatherOpt.AllowCropDeath) {
                            if (Conditions.HasWeather(CurrentWeather.Heatwave) && !Conditions.HasWeather(CurrentWeather.Sandstorm))
                                SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_heatwave_kill", new { crops = count }), 3);
                            if (Conditions.HasWeather(CurrentWeather.Sandstorm))
                                SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_sandstorm_kill", new { crops = count }), 3);
                        }
                        else {
                            if (Conditions.HasWeather(CurrentWeather.Heatwave) && !Conditions.HasWeather(CurrentWeather.Sandstorm))
                                SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_heatwave_dry", new { crops = count }),3);
                            if (Conditions.HasWeather(CurrentWeather.Sandstorm))
                                SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_sandstorm_dry", new { crops = count }), 3);
                        }
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
                    if (hd.state.Value == HoeDirt.dry)
                    {
                        hd.crop.dead.Value = true;
                        cDead = true;
                    }
                }

                if (cDead) { 
                    SDVUtilities.ShowMessage(Helper.Translation.Get("hud-text.desc_heatwave_cropdeath"),3);
                }
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            //float shadowMult = 0f;
            if (!Context.IsWorldReady)
                return;
          
            var weatherMenu = Game1.onScreenMenus.OfType<DayTimeMoneyBox>().FirstOrDefault();

            if (weatherMenu == null)
                return;
            // abort abort abort (maybe another mod replaced it?)

            if (WeatherOpt.EnableCustomWeatherIcon) 
            { 
                //determine icon offset  
                if (!Game1.eventUp)
                {
                    if ((int)Conditions.CurrentWeatherIcon != (int)WeatherIcon.IconError)
                    {
                        RWeatherIcon = new Rectangle(0 + 12 * (int)Conditions.CurrentWeatherIcon, Game1.isDarkOut() ? 8 : 0, 12, 8);
                    }

                    if ((int)Conditions.CurrentWeatherIcon == (int)WeatherIcon.IconBloodMoon)
                    {
                        RWeatherIcon = new Rectangle(144, 8, 12, 8);
                    }

                    if ((int)Conditions.CurrentWeatherIcon == (int)WeatherIcon.IconError)
                    {
                        RWeatherIcon = new Rectangle(144, 0, 12, 8);
                    }

                    Game1.spriteBatch.Draw(OurIcons.WeatherSource, weatherMenu.position + new Vector2(116f, 68f), RWeatherIcon, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, .1f);
                }

                //redraw mouse cursor
                if (WeatherOpt.RedrawCursor)
                    SDVUtilities.RedrawMouseCursor();
            }
        }       
        
        private void OnModMessageRecieved(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == "KoihimeNakamura.ClimatesOfFerngill" && e.Type == "NewFarmHandJoin" && Context.IsMainPlayer && !HasGottenSync)
            {
                WeatherSync message = Conditions.GenerateWeatherSyncMessage();
                MPHandler.SendMessage<WeatherSync>(message,"WeatherSync",new[] {"KoihimeNakamura.ClimatesOfFerngill" },new[] {e.FromPlayerID });
                HasGottenSync = true;
            }

            if (e.FromModID == "KoihimeNakamura.ClimatesOfFerngill" && e.Type == "WeatherSync")
            {
                WeatherSync message = e.ReadAs<WeatherSync>();
                if (WeatherOpt.Verbose)
                {
                    Monitor.Log($"Message contents: {GenSyncMessageString(message)}");
                }                
                Conditions.SetSync(message);
            }
        }

        /// <summary>Raised after the game returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            TenMCounter = 0;
            Conditions.Reset();
            IsVariableRain = false;
            IsBloodMoon = false;
            ExpireTime = 0;
            AmtOfRainDrops = 0;
            //RainAmt = 0.0;
            CropList.Clear(); 
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            TenMCounter = 0;
            Conditions.OnNewDay();
            IsBloodMoon = false;
            IsVariableRain = false;

            Conditions.SetTodayWeather(); //run this automatically
            if (!Context.IsMainPlayer) return;

            if (trackerModel is null) { 
                  trackerModel = new ClimateTracker();
            }

            AmtOfRainDrops = 0;

            if (Conditions.ContainsCondition(CurrentWeather.Rain))
                trackerModel.DaysSinceRainedLast = 0;
            else
                trackerModel.DaysSinceRainedLast++;

            CropList.Clear(); //clear the crop list
            UpdateWeatherOnNewDay();
            SetTommorowWeather();
            ExpireTime = 0;
                       
            WeatherSync message = Conditions.GenerateWeatherSyncMessage();
            MPHandler.SendMessage(message, "WeatherSync", modIDs: new[] { this.ModManifest.UniqueID });
        }

        private string GenSyncMessageString(WeatherSync ws)
        {
            string s = $"WeatherType: {ws.weatherType}, with today temps ({ws.todayLow},{ws.todayHigh}), tomorrow temps ({ws.tommorowLow},{ws.tommorowHigh}).";
            s += $"{Environment.NewLine}Variable Rain: {ws.isVariableRain}, overcast: {ws.isOvercast}, rainAmt: {ws.rainAmt}";
            s += $"{Environment.NewLine}Blizzard: {ws.isBlizzard}, time: ({ws.blizzWeatherBeginTime},{ws.blizzWeatherEndTime}).";
            s += $"{Environment.NewLine}Fog: {ws.isFoggy}, time: ({ws.fogWeatherBeginTime},{ws.fogWeatherEndTime}).";
            s += $"{Environment.NewLine}WhiteOut: {ws.isWhiteOut}, time: ({ws.whiteWeatherBeginTime},{ws.whiteWeatherEndTime}).";
            s += $"{Environment.NewLine}Thunder: {ws.isThunderFrenzy}, time: ({ws.thunWeatherBeginTime},{ws.thunWeatherEndTime}).";
            s += $"{Environment.NewLine}Sandstorm: {ws.isSandstorm}, time: ({ws.sandstormWeatherBeginTime},{ws.sandstormWeatherEndTime}).";

            return s;
        }

        private void SetTommorowWeather()
        {
            //if tomorrow is a festival or wedding, we need to set the weather and leave.
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
            {
                Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_festival;
                if (WeatherOpt.Verbose)
                    Monitor.Log($"Festival tomorrow. Aborting processing.", LogLevel.Trace);

                //if (WeatherOpt.Verbose) Monitor.Log(DebugOutput.ToString());
                return;
            }

            if (Game1.player.spouse != null && Game1.player.isEngaged() && Game1.player.friendshipData[Game1.player.spouse].CountdownToWedding == 1)
            {
                Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_wedding;
                if (WeatherOpt.Verbose)
                    Monitor.Log($"Wedding tomorrow. Aborting processing.", LogLevel.Trace);

                return;
            }

            if (ForceDays.CheckForForceDay(DescriptionEngine, SDate.Now().AddDays(1),Monitor, WeatherOpt.Verbose))
            {
                if (WeatherOpt.Verbose)
                    Monitor.Log($"The game will force tomorrow. Aborting processing.", LogLevel.Trace);
                return;
            }

            if (WeatherOpt.Verbose)
                Monitor.Log("Setting weather for tomorrow");

            //now set tomorrow's weather
            var OddsForTheDay = GameClimate.GetClimateForDate(SDate.Now().AddDays(1));

            double rainDays = OddsForTheDay.RetrieveOdds(Dice, "rain", SDate.Now().AddDays(1).Day);
            double windyDays = OddsForTheDay.RetrieveOdds(Dice, "debris", SDate.Now().AddDays(1).Day);
            double stormDays = OddsForTheDay.RetrieveOdds(Dice, "storm", SDate.Now().AddDays(1).Day);

            if (WeatherOpt.Verbose)
            {
                Monitor.Log($"Odds are Rain: {rainDays.ToString("N3")}, Windy: {windyDays.ToString("N3")}, and Storm {stormDays.ToString("N3")}");
            }

            ProbabilityDistribution<string> WeatherDist = new ProbabilityDistribution<string>("sunny");
            WeatherDist.AddNewEndPoint(rainDays, "rain");
            
            if (WeatherOpt.DisableHighRainWind)
                WeatherDist.AddNewCappedEndPoint(windyDays, "debris");

            double distOdd = Dice.NextDoublePositive();

            if (WeatherOpt.Verbose)
            {
                Monitor.Log(WeatherDist.ToString());
                Monitor.Log($"Distribution odds is {distOdd}");
            }

            if (!(WeatherDist.GetEntryFromProb(distOdd, out string Result)))
            {
                Result = "sunny";
                Monitor.Log("The weather has failed to process in some manner. Falling back to [sunny]", LogLevel.Info);
            }

            if (WeatherOpt.Verbose)
                Monitor.Log($"Weather result is {Result}");

            if (!Conditions.IsTodayTempSet)
                throw new NullReferenceException("Today's temperatures have not been set!");

            //now parse the result.
            if (Result == "rain")
            {
                //snow applies first
                double MidPointTemp = Conditions.TodayHigh -
                    ((Conditions.TodayHigh - Conditions.TodayLow) / 2);

                if ((Conditions.TodayHigh <= 2 || MidPointTemp <= 0) && Game1.currentSeason != "spring")
                {
                    if (WeatherOpt.Verbose)
                        Monitor.Log($"Snow is enabled, with the High for the day being: {Conditions.TodayHigh}" +
                                    $" and the calculated midpoint temperature being {MidPointTemp}");

                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_snow;
                }
                else
                {
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_rain;
                }

                if (!GameClimate.AllowRainInWinter && Game1.currentSeason == "winter" && Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_snow;
                }

                //apply lightning logic.
                if (Dice.NextDoublePositive() >= stormDays && Game1.weatherForTomorrow == Game1.weather_rain)
                {
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_lightning;
                    if (SDate.Now().Year == 1 && SDate.Now().Season == "spring" && !WeatherOpt.AllowStormsSpringYear1)
                        Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_rain;
                }

                //tracking time! - Snow fall on Fall 28, if the flag is set.
                if (Game1.dayOfMonth == 28 && Game1.currentSeason == "fall" && WeatherOpt.SnowOnFall28)
                {
                    Conditions.ForceTodayTemps(2, -1);
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_snow;
                }
            }

            if (Result == "debris")
            {
                Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_debris;
            }

            if (Result == "sunny")
            {
                Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = Game1.weather_sunny;
            }

            if (WeatherOpt.Verbose)
                Monitor.Log($"We've set the weather for Tomorrow. It is: {DescriptionEngine.DescribeInGameWeather(Game1.weatherForTomorrow)}");
        }

        private void UpdateWeatherOnNewDay()
        {
            if (!(Context.IsMainPlayer))
            {
                return;
            }

            if (Game1.dayOfMonth == 0) //do not run on day 0.
                return;

            //RainAmt = 0; //reset for recalc.
            //Set Temperature for today and tommorow. Get today's conditions.
            //   If tomorrow is set, move it to today, and autoregen tomorrow.
            //   *201711 Due to changes in the object, it auto attempts to update today from tomorrow.

            if (!Conditions.IsTomorrowTempSet)
                Conditions.SetTodayTemps(GameClimate.GetTemperatures(SDate.Now(), Dice));

            Conditions.SetTomorrowTemps(GameClimate.GetTemperatures(SDate.Now().AddDays(1), Dice));

            if (WeatherOpt.Verbose)
                Monitor.Log($"Updated the temperature for tommorow and today. Setting weather for today... ", LogLevel.Trace);

            //if today is a festival or wedding, do not go further.
            if (Conditions.GetCurrentConditions().HasAnyFlags(CurrentWeather.Festival | CurrentWeather.Wedding))
            {
                if (WeatherOpt.Verbose)
                    Monitor.Log("It is a wedding or festival today. Not attempting to run special weather or fog.");

                return;
            }

            if (Conditions.HasWeather(CurrentWeather.Rain))
               AmtOfRainDrops = 70;

            //variable rain conditions 
            //only roll this if it's actually raining. :|
            double roll = Dice.NextDouble();
            if (roll <= WeatherOpt.VariableRainChance && Game1.isRaining)
            {
                Monitor.Log($"With {roll}, we are setting for variable rain against {WeatherOpt.VariableRainChance}");
                IsVariableRain = true;
             
                if (Dice.NextDouble() < WeatherOpt.OvercastChance) { 
                    IsVariableRain = false;
                    Conditions.AddWeather(CurrentWeather.Overcast);
                    Monitor.Log("Resizing rain array - settting weather to ovecast");
                    Array.Resize(ref Game1.rainDrops,0);
                    AmtOfRainDrops = 0;
                    IsVariableRain = false;
                }
                else if (Dice.NextDouble() < WeatherOpt.VRChangeChance)
                {
                    double chance = WeatherOpt.VRMassiveStepChance;
                    if (AmtOfRainDrops > 200)
                        chance /= 2; //cut this chance in half.
                    if (AmtOfRainDrops > 600)
                        chance /= 2; //cut it in half *again*

                    if (Dice.NextDouble() < chance)
                        AmtOfRainDrops = WeatherUtilities.GetNextHighestRainCategoryBeginning(AmtOfRainDrops);
                    else
                        AmtOfRainDrops = (int)Math.Floor(AmtOfRainDrops * (1.0 + Dice.RollInRange(-1.0 * WeatherOpt.VRStepPercent, WeatherOpt.VRStepPercent)));

                    Monitor.Log($"Resizing rain array - settting rain to {AmtOfRainDrops}");
                    Array.Resize(ref Game1.rainDrops, AmtOfRainDrops);
                }

                //TODO calc the amount of water it's been since 3am
               
            }
            //TODO water check
            
            if (Conditions.TestForSpecialWeather(GameClimate.GetClimateForDate(SDate.Now()), trackerModel))
            {           
                if (WeatherOpt.Verbose)
                    Monitor.Log("Special weather created!");
            }
        }

        internal static void ForceVariableRain()
        {
            IsVariableRain = true;
        }

        public static bool ShouldPrecipInLocation()
        {
            return true;
        }

        public static Color GetRainColor()
        {
            if (ClimatesOfFerngill.IsBloodMoon)
                return Color.Red;
            else
               return Color.White;
        }
        public static Color GetRainBackColor()
        {
            return Color.Blue;
        }

        public static Color GetSnowColor()
        {
            if (ClimatesOfFerngill.IsBloodMoon)
                return Color.Red;
            else
                return Color.White;
        }

        #region Menu
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (WeatherOpt.WeatherMenuToggle != e.Button)  //sanity force this to exit!
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
            string MoonPhase = "";
            if (UseLunarDisturbancesApi)
                MoonPhase = MoonAPI.GetCurrentMoonPhase();

            string NightTime = "";
            if (UseDynamicNightApi)
                NightTime = Helper.Translation.Get("nightTime",
                    new
                    {
                        sunrise = new SDVTime(DynamicNightAPI.GetSunriseTime()),
                        sunset = new SDVTime(DynamicNightAPI.GetSunsetTime()),
                        nighttime = new SDVTime(DynamicNightAPI.GetAstroTwilightTime())
                    });

            string MenuText = DescriptionEngine.GenerateMenuPopup(Conditions, MoonPhase, NightTime);

            // show menu
            this.PreviousMenu = Game1.activeClickableMenu;
            Game1.activeClickableMenu = new WeatherMenu(OurIcons, Conditions, MenuText);
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

        internal static void GetTodayTemps()
        {
            Conditions.SetTodayTemps(GameClimate.GetTemperatures(SDate.Now(), Dice));
        }

        internal static void GetTomorrowTemps()
        {
            Conditions.SetTomorrowTemps(GameClimate.GetTemperatures(SDate.Now().AddDays(1), Dice));
        }
     
    }
}
