using System.Collections.Generic;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Linq;
using StardewValley.GameData.Locations;
using System;
using StardewValley.Audio;
using StardewValley.Tools;

namespace HappyFishJump
{
    public class FishConfig
    {
        public float JumpChance = .30f;
        public bool SplashSound = true;
        public int NumberOfJumpingFish = 18;     
    }

    public class HappyFishJump : Mod
    {
        internal static FishConfig ModConfig;
        private List<JumpFish> _jumpingFish;
        private Dictionary<Vector2, string> _validFishLocations;
        internal IMonitor Logger;
        internal bool debug = false;

        public override void Entry(IModHelper helper)
        {
            _jumpingFish = new List<JumpFish>();
            _validFishLocations = new Dictionary<Vector2, string>();
            Logger = Monitor;

            var harmony = new Harmony("koihimenakamura.happyfishjump");
            harmony.PatchAll();
            Monitor.Log("Patching JumpingFish.Splash with a transpiler", LogLevel.Trace);

            ModConfig = Helper.ReadConfig<FishConfig>();

            Helper.Events.Player.Warped += Player_Warped;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            foreach (var t in this._jumpingFish)
            {
                t.Draw(Game1.spriteBatch);
            }
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (Game1.currentLocation != null)
            {
                _validFishLocations.Clear();
                PopulateValidFishLocations();
            }
        }
        
        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            for (int index = 0; index < this._jumpingFish.Count; ++index)
            {
                JumpFish jumpingFish = this._jumpingFish[index];
                var elapsedGameTime = Game1.currentGameTime.ElapsedGameTime;
                double totalSeconds = elapsedGameTime.TotalSeconds;
                if (jumpingFish.Update((float)totalSeconds))
                {
                   this._jumpingFish.RemoveAt(index);
                    --index;
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var GMCMapi = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (GMCMapi != null)
            {
                GMCMapi.Register(ModManifest, () => ModConfig = new FishConfig(), () => Helper.WriteConfig(ModConfig));
                GMCMapi.AddBoolOption(ModManifest, () => ModConfig.SplashSound, (bool val) => ModConfig.SplashSound = val, () => "Splash Sound", () => "Disable to stop the splash sound on jump" );
                GMCMapi.AddNumberOption(ModManifest,() => ModConfig.NumberOfJumpingFish, (int val) => ModConfig.NumberOfJumpingFish = val,() => "Jumping Fish", () => "The number of jumping fish. Minimum 2. Note that large numbers may lag a computer.", min:2, max:1000);
                GMCMapi.AddNumberOption(ModManifest,() => ModConfig.JumpChance, (float val) => ModConfig.JumpChance = val, () => "Jump Chance", () => "Controls the jump chance per pond every 10 minutes.");
            }
        }

        public static bool PlaySound(string cueName, int? pitch=null)
        {
            if (ModConfig.SplashSound)
            {
                return Game1.sounds.PlayLocal(cueName, null, null, pitch, SoundContext.Default, out ICue cue);
            }
            return false;
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (Game1.currentLocation is GameLocation Loc)
            {
                foreach (var v in Loc.buildings)
                {
                    if (v is FishPond fp)
                    {
                        if (Game1.random.NextDouble() <= ModConfig.JumpChance)
                        {
                            NetEvent0 animateHappyFishEvent = this.Helper.Reflection
                                .GetField<NetEvent0>(fp, "animateHappyFishEvent").GetValue();
                            animateHappyFishEvent.Fire();
                        }
                    }
                }
            }

            if (Game1.currentLocation != null && Game1.currentLocation.IsOutdoors && _validFishLocations.Count >= ModConfig.NumberOfJumpingFish)
            {
                Vector2[] validLocs = _validFishLocations.Keys.ToArray();

                for (int i = 0; i < ModConfig.NumberOfJumpingFish; i++)
                {
                    int rndLoc = Game1.random.Next(0, validLocs.Length - 1);
                    var startPosition = validLocs[rndLoc];
                    Vector2 endPosition = new(startPosition.X + 1.5f, startPosition.Y + 1.5f);

                    var clearWaterDistance = FishingRod.distanceToLand((int)(startPosition.X / 64f), (int)(startPosition.Y / 64f), Game1.currentLocation);
                    var randFish = StardewValley.GameLocation.GetFishFromLocationData(Game1.currentLocation.Name, startPosition, clearWaterDistance, Game1.player, false, false);

                    if (this.debug)
                        this.Monitor.Log($"Log ID: {randFish.QualifiedItemId}, Display Name: {randFish.DisplayName}");

                    if (ValidFishForJumping(randFish.QualifiedItemId))
                        this._jumpingFish.Add(new JumpFish(randFish, startPosition * Game1.tileSize, endPosition * Game1.tileSize));
                }
            }
        }

        private void PopulateValidFishLocations()
        {
            //get starting position
            if (Game1.currentLocation.waterTiles is null)
                return;

            //iterate through array
            for (int j = 0; j < Game1.currentLocation.map.Layers[0].LayerWidth; j++)
            {
                for (int k = 0; k < Game1.currentLocation.map.Layers[0].LayerHeight; k++)
                {
                    if (Game1.currentLocation.waterTiles.waterTiles[j,k].isWater)
                    {
                        if ((j + 2) >= Game1.currentLocation.map.Layers[0].LayerWidth ||
                            (k + 2) >= Game1.currentLocation.map.Layers[0].LayerHeight)
                            continue;

                        if (Game1.currentLocation.waterTiles.waterTiles[j+2, k+2].isWater)
                        {
                            Vector2 pos = new(j, k);
                            bool validArea = Game1.currentLocation.TryGetFishAreaForTile(pos, out string fishID, out FishAreaData data);

                            if (validArea)  _validFishLocations.Add(pos, data?.DisplayName);
                        }
                    }
                }
            }
        }

        private bool ValidFishForJumping(string index)
        {
            List<string> invalidFishJump = new() {"(O)172", "(O)153", "(O)169", "(O)170", "(O)167", "(O)168", "(O)158", "(O)171", "(O)152", 
                "(O)2","(O)4","(O)6","(O)8","(O)10","(O)12","(O)14"};

            if (this.debug)
                Logger.Log($"Checking index {index}");

            if (Game1.random.NextDouble() < .00001)
            {
                Logger.Log("Random Jump Detected");
                return true;
            }
            else
            { 
                if (index.StartsWith("(F)")) return false;
                return !invalidFishJump.Contains(index);
                
            }
        }
    }
}