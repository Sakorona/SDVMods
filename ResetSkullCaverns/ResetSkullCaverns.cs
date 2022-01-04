using System.Collections.Generic;
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
            List<int> KeyValues = new List<int>();

            foreach (var v in MineShaft.permanentMineChanges)
            {
                if (v.Key > 120)
                {
                    KeyValues.Add(v.Key);
                }
            }

            foreach (var i in KeyValues)
            {
                MineShaft.permanentMineChanges.Remove(i);
            }
        }
    }
}
