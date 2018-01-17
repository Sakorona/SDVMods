using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace ForceWateringCheck
{
    public class ForceWateringCheck : Mod
    {
        public override void Entry(IModHelper helper)
        {
            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (Game1.currentLocation is Farm && Game1.player.CurrentTool is WateringCan)
            {
                Farm f = Game1.getFarm();
                foreach (KeyValuePair<Vector2, TerrainFeature> tf in f.terrainFeatures)
                {
                    if (tf.Value is HoeDirt curr && curr.crop != null)
                    {
                        if (curr.state == HoeDirt.dry)
                        {

                        }
                    }
                }
            }
        }
    }
}
