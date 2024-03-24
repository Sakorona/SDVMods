using StardewModdingAPI;
using StardewValley.Locations;

namespace ResetSkullCaverns
{
    public class ResetCavernsConfig
    {
        public bool ResetMinesToo = false;
    }

    public class ResetSkullCaverns : Mod
    {
        public ResetCavernsConfig Config { get; set; }

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ResetCavernsConfig>();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var GMCMapi = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (GMCMapi != null)
            {
                GMCMapi.Register(ModManifest, () => Config = new ResetCavernsConfig(), () => Helper.WriteConfig(Config));
                GMCMapi.AddBoolOption(ModManifest, () => Config.ResetMinesToo, (bool val) => Config.ResetMinesToo = val, () => "Reset Mines Too", () => "Normally, it only resets coal deposits in the skull caverns, but this will let it reset mines too.");
            }
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            foreach (var v in MineShaft.permanentMineChanges)
            {
                if (v.Key > 120 || Config.ResetMinesToo)
                {
                   MineShaft.permanentMineChanges[v.Key].coalCartsLeft = 1;
                }
            }
        }
    }
}
