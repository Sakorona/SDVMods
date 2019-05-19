using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace TwilightShards.FerngillTaxes.Patches
{
    static class AnswerDialoguePatch
    {
        static bool Prefix(bool __result)
        {             
            if (this.lastQuestionKey != null && this.afterQuestion == null)
            {
                if (this.lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey == "Bus_Yes")
                {
                    NPC characterFromName = Game1.getCharacterFromName("Pam", false);
                    if (Game1.player.Money >= (Game1.shippingTax ? 50 : 500) && this.characters.Contains(characterFromName) && characterFromName.getTileLocation().Equals(new Vector2(11f, 10f)))
                    {
                        Game1.player.Money -= Game1.shippingTax ? 50 : 500;
                        Game1.freezeControls = true;
                        Game1.viewportFreeze = true;
                        this.forceWarpTimer = 8000;
                        Game1.player.controller = new PathFindController((Character)Game1.player, (GameLocation)this, new Point(12, 9), 0, new PathFindController.endBehavior(this.playerReachedBusDoor));
                        Game1.player.setRunning(true, false);
                        if (Game1.player.mount != null)
                            Game1.player.mount.farmerPassesThrough = true;
                    }
                    else if (Game1.player.Money < (Game1.shippingTax ? 50 : 500))
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
                    else
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NoDriver"));
                    return true;
                }
            }
            __result = base.answerDialogue(answer);
            return false;
        }
    }
}
