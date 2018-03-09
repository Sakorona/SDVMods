using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using StardewValley.Quests;
using StardewValley.Monsters;
using System.Collections.Generic;
using TwilightShards.Common;

namespace TwilightShards.USDVP
{
    public class USDVP : Mod
    {
        private MersenneTwister Dice = new MersenneTwister();

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
                Console.WriteLine($"Quest ID is {Game1.player.questLog[index].id}");

                //resolve bug 1 (issue 44)
                if (Game1.player.questLog[index].id == 15 && Game1.player.questLog[index] is SlayMonsterQuest ourQuest)
                {
                    //check to see what the monster actually is
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

                //resolve bug 2 (issue 46)
                if (Game1.player.questLog[index].id == 9 && Game1.player.questLog[index] is SocializeQuest socQuest)
                {
                    if (socQuest.whoToGreet == null || socQuest.whoToGreet?.Count == 0)
                    {
                        socQuest.whoToGreet = new List<string>();
                        socQuest.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");

                        if (socQuest.parts == null)
                            socQuest.parts = new List<DescriptionElement>();

                        socQuest.parts.Clear();
                        socQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13786", Dice.NextDouble() < 0.3 ? new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13787") : (Dice.NextDouble() < 0.5 ? new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13788") : new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13789"))));
                        socQuest.parts.Add("Strings\\StringsFromCSFiles:SocializeQuest.cs.13791");

                        foreach (GameLocation location in Game1.locations)
                        {
                            foreach (NPC character in location.characters)
                            {
                                if (!character.isInvisible && !character.name.Contains("Qi") && (!character.name.Contains("???") && !character.name.Equals("Sandy")) && (!character.name.Contains("Dwarf") && !character.name.Contains("Gunther") && (!character.name.Contains("Mariner") && !character.name.Contains("Henchman"))) && (!character.name.Contains("Marlon") && !character.name.Contains("Wizard") && (!character.name.Contains("Bouncer") && !character.name.Contains("Krobus")) && character.isVillager()))
                                    socQuest.whoToGreet.Add(character.name);
                            }
                        }
                        socQuest.objective = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", "2", socQuest.whoToGreet.Count);
                        socQuest.total = socQuest.whoToGreet.Count;
                        socQuest.whoToGreet.Remove("Lewis");
                        socQuest.whoToGreet.Remove("Robin");

                        Console.WriteLine($"Socialize Quest Total: {socQuest.total}, Who To Greet: {socQuest.whoToGreet?.Count}");
                    }
                }
            }
        }
    }
}
