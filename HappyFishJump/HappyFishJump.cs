using System;
using System.Collections.Generic;
using System.Reflection;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Harmony;
using Microsoft.Xna.Framework;
using TwilightShards.Stardew.Common;

namespace HappyFishJump
{
    public class FishConfig
    {
        public float JumpChance = .30f;
        public bool SplashSound = true;
        public int JumpingFish = 18;
    }

    public class HappyFishJump : Mod
    {
        internal static FishConfig ModConfig;
        private List<JumpFish> _jumpingFish;
        private List<Vector2> _validFishLocations;

        public override void Entry(IModHelper helper)
        {
            _jumpingFish = new List<JumpFish>();
            _validFishLocations = new List<Vector2>();

            var harmony = HarmonyInstance.Create("koihimenakamura.happyfishjump");
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
                GMCMapi.RegisterModConfig(ModManifest, () => ModConfig = new FishConfig(), () => Helper.WriteConfig(ModConfig));
                GMCMapi.RegisterSimpleOption(ModManifest,"Splash Sound","Disable to stop the splash sound on jump", () => ModConfig.SplashSound, (bool val) => ModConfig.SplashSound = val);
                GMCMapi.RegisterClampedOption(ModManifest,"Jumping Fish","The number of jumping fish. Minimum 2. Note that large numbers may lag a computer.", () => ModConfig.JumpingFish, (int val) => ModConfig.JumpingFish = val,2,1000);
                GMCMapi.RegisterSimpleOption(ModManifest,"Jump Chance","Controls the jump chance per pond every 10 minutes.", () => ModConfig.JumpChance,
                    (float val) => ModConfig.JumpChance = val);
            }
        }

        public static void PlaySound(string cueName)
        {
            if (ModConfig.SplashSound)
            {
                Game1.playSound("dropItemInWater");
            }
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (Game1.currentLocation != null && Game1.currentLocation is Farm f)
            {
                foreach (var v in f.buildings)
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

            if (Game1.currentLocation != null && Game1.currentLocation.IsOutdoors && _validFishLocations.Count >= ModConfig.JumpingFish)
            {
                for (int i = 0; i < ModConfig.JumpingFish; i++)
                {
                    if (Game1.random.NextDouble() > ModConfig.JumpChance)
                        continue;
                    StardewValley.Object fish = SDVUtilities.GetRandomFish(Game1.currentLocation);
                    
                    var startPosition = _validFishLocations[Game1.random.Next(0, _validFishLocations.Count - 1)];
                    var endPosition = new Vector2(startPosition.X + 1.5f,startPosition.Y + 1.5f);
                    if (ValidFishForJumping(fish.ParentSheetIndex))
                        this._jumpingFish.Add(new JumpFish(fish, startPosition * Game1.tileSize,
                            endPosition * Game1.tileSize));
                }
            }
        }

        private void PopulateValidFishLocations()
        {
            //get starting position
            for (int j = 0; j < Game1.currentLocation.waterTiles.GetLength(0); j++)
            {
                for (int k = 0; k < Game1.currentLocation.waterTiles.GetLength(1); k++)
                {
                    if (Game1.currentLocation.waterTiles[j, k])
                    {
                        if ((j + 2) >= Game1.currentLocation.waterTiles.GetLength(0) ||
                            (k + 2) >= Game1.currentLocation.waterTiles.GetLength(1))
                            continue;

                        if (Game1.currentLocation.waterTiles[j + 2, k + 2])
                        {
                            _validFishLocations.Add(new Vector2(j, k));
                        }
                    }
                }
            }
        }
        
        private bool ValidFishForJumping(int index)
        {
            switch (index)
            {
                case 372:
                case 718:
                case 719:
                case 721:
                case 152:
                case 153:
                case 157:
                case 723:
                    return false;
                default:
                    return true;
            }
        }
    }
}