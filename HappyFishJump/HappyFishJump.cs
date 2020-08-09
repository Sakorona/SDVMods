using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace HappyFishJump
{
    public class FishConfig
    {
        public float JumpChance = .30f;
    }

    public class HappyFishJump : Mod
    {
        private FishConfig ModConfig;

        public override void Entry(IModHelper helper)
        {
            ModConfig = Helper.ReadConfig<FishConfig>();
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var GMCMapi = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (GMCMapi != null)
            {
                GMCMapi.RegisterModConfig(ModManifest, () => ModConfig = new FishConfig(), () => Helper.WriteConfig(ModConfig));
                GMCMapi.RegisterSimpleOption(ModManifest,"Jump Chance","Controls the jump chance per pond every 10 minutes.", () => ModConfig.JumpChance,
                    (float val) => ModConfig.JumpChance = val);
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
                            Monitor.VerboseLog("ATTEMPTING TO FIRE EVENT");
                            NetEvent0 animateHappyFishEvent = this.Helper.Reflection
                                .GetField<NetEvent0>(fp, "animateHappyFishEvent").GetValue();
                            animateHappyFishEvent.Fire();
                        }
                    }
                }
            }
        }
    }
}
