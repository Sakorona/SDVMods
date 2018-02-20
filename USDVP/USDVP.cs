using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using StardewValley.Quests;
using StardewValley.Monsters;

namespace TwilightShards.USDVP
{
    public class USDVP : Mod
    {
        public override void Entry(IModHelper Helper)
        {
            SaveEvents.AfterLoad += AfterLoadFixes;
        }

        private void AfterLoadFixes(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Monitor.Log("Running fixes....");
            for (int index = 0; index < Game1.player.questLog.Count; index++)
            {
                //resolve bug 1 (issue 44)
                if (Game1.player.questLog[index].id == 15 && Game1.player.questLog[index] is SlayMonsterQuest ourQuest)
                {
                    //check to see what hte monster actually is
                    if (ourQuest.monster.name != "Green Slime")
                    {
                        ourQuest.monsterName = "Green Slime";
                        ourQuest.numberToKill = 10;
                        ourQuest.monster = new Monster("Green Slime", Vector2.Zero);
                        ourQuest.questTitle = "Initation";
                        ourQuest.questDescription = "If you can slay 10 slimes, you'll have earned your place in the Adventurer's Guild.";
                        ourQuest.actualTarget = null;
                        ourQuest.targetMessage = null;

                    }
                }
            }
        }
    }
}
