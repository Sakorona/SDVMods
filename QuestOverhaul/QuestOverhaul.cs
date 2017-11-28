using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using TwilightShards.Common;
using StardewValley.Quests;
using TwilightShards.Stardew.Common;
using SObject = StardewValley.Object;
using StardewValley.Menus;
using TwilightShards.QuestOverhaul.QuestTypes;
using System.Collections.Specialized;

namespace TwilightShards.QuestOverhaul
{
    public class QuestOverhaul : Mod      
    {
        private MersenneTwister Dice = new MersenneTwister();

        public override void Entry(IModHelper Helper)
        {
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;

            Helper.ConsoleCommands
                      .Add("debug_dumpquest", "Debug command to dump quest of day stuff.", QuestVariableDump)
                      .Add("debug_playerpos", "Get Player position", GetCurrentLocation);
        }

        private void CheckForDestroyedObjects(object sender, NotifyCollectionChangedEventArgs e)
        {

            for (int i = 0; i < Game1.player.questLog.Count; i++)
            {
                if (Game1.player.questLog[i] is KN_WeedingQuest)
                    Game1.player.questLog[i].checkIfComplete((NPC)null, -1, -1, (Item)null, (string)null);
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            //despite being in the game, WeedingQuests crash the save serializer.
            if (Game1.questOfTheDay is KN_WeedingQuest)
            {
                Game1.questOfTheDay = null;
            }

            for (int i = 0; i < Game1.player.questLog.Count; i++)
            {
                if (Game1.player.questLog[i] is KN_WeedingQuest)
                {
                    if (!Game1.player.questLog[i].completed)
                    {
                        Game1.player.questLog.RemoveAt(i);
                        //you made Lewis sad :( (And everyone else.)
                        foreach (string key in Game1.player.friendships.Keys)
                        {
                            if (Game1.player.friendships[key][0] < 2729)
                                Game1.player.friendships[key][0] -= 10;
                        }
                    }
                    else
                    {
                        Game1.player.money += Game1.player.questLog[i].moneyReward;
                        foreach (string key in Game1.player.friendships.Keys)
                        {
                            if (Game1.player.friendships[key][0] < 2729)
                                Game1.player.friendships[key][0] += 20;
                        }

                        Game1.player.questLog.RemoveAt(i);
                    }
                }
            }
        }

        private void QuestVariableDump(string arg1, string[] arg2)
        {
            string varDump = "";
            varDump += "Title: " + Game1.questOfTheDay.questTitle + Environment.NewLine;
            varDump += "Description: " + Game1.questOfTheDay.questDescription.ToString() + Environment.NewLine;
            varDump += "Accepted: " + Game1.questOfTheDay.accepted + Environment.NewLine;
            varDump += "Completed: " + Game1.questOfTheDay.completed + Environment.NewLine;
            //now dump the x and y of all weeds in the map
            varDump += Environment.NewLine;

            foreach (SObject @object in Game1.getLocationFromName("Town").Objects.Values)
            {
                if (@object.name.Contains("Weed"))
                    varDump += $"Weed detected at {@object.TileLocation.ToString()} {Environment.NewLine}";
            }
            varDump += $"{Environment.NewLine} Weeds Left (ours): {WeedsLeft()}";

            KN_WeedingQuest quest = (KN_WeedingQuest)Game1.questOfTheDay;

            varDump += $"{Environment.NewLine} Weeds Left (internal): {quest.WeedsLeft()}";

            Monitor.Log(Environment.NewLine + varDump + Environment.NewLine);
        }

        private void GetCurrentLocation(string arg1, string[] arg2)
        {
            Monitor.Log($"Current player location is ({Game1.player.getStandingPosition().X / Game1.tileSize}, {Game1.player.getStandingPosition().Y / Game1.tileSize})");
        }

        private void TimeEvents_AfterDayStarted(object sender, System.EventArgs e)
        {

        }

        private void InitWeedingQuest()
        {
            KN_WeedingQuest _quest = new KN_WeedingQuest(Helper.Translation);
            _quest.SetLocation(Game1.getLocationFromName("Town"));
            _quest.dailyQuest = true;
            int createdWeeds = 0;
            createdWeeds += SDVUtilities.CreateWeeds(_quest.targetLocation, Dice.Next(15, 45));
            Monitor.Log($"Created weeds is {createdWeeds}");
            _quest.LoadQuestInfo();
            Game1.questOfTheDay = _quest;

            //hookup events to allow the quest to be completed
            GameLocation loc = Game1.getLocationFromName("Town");
            loc.objects.CollectionChanged += CheckForDestroyedObjects;
        }

        private int WeedsLeft()
        {
            int num = 0;
            foreach (SObject @object in Game1.getLocationFromName("Town").Objects.Values)
            {
                if (@object.name.Contains("Weed"))
                    ++num;
            }
            return num;
        }   
    }
}
