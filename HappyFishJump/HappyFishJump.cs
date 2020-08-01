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
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
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
                            //private readonly NetEvent0 animateHappyFishEvent = new NetEvent0(false);
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
