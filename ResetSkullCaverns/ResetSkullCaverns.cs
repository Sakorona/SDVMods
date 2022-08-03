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

            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
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
