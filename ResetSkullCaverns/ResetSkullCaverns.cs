using StardewModdingAPI;
using StardewValley.Locations;

namespace ResetSkullCaverns
{
    public class ResetSkullCaverns : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            foreach (var v in MineShaft.permanentMineChanges)
            {
                if (v.Key > 120)
                {
                   MineShaft.permanentMineChanges[v.Key].coalCartsLeft = 1;
                }
            }
        }
    }
}
