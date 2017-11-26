using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;
using xTile.Dimensions;
using SObject = StardewValley.Object;
using System.Collections.Specialized;
using StardewValley.Menus;

namespace QuestOverhaul
{
    public class QuestOverhaul : Mod      
    {
        private MersenneTwister Dice = new MersenneTwister();

        public override void Entry(IModHelper Helper)
        {
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            GameEvents.OneSecondTick += GameEvents_SecondUpdateTick;

            Helper.ConsoleCommands
                      .Add("debug_dumpquest", "Debug command to dump quest of day stuff.", QuestVariableDump)
                      .Add("debug_playerpos", "Get Player position", GetCurrentLocation);
        }

        private void GameEvents_SecondUpdateTick(object sender, EventArgs e)
        {
            foreach (Quest q in Game1.player.questLog)
            {
                if (q is WeedingQuest && q.completed)
                {
                    GameLocation loc = Game1.getLocationFromName("Town");
                    loc.objects.CollectionChanged -= CheckForDestroyedObjects;
                }
            }
        }

        private void CheckForDestroyedObjects(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Game1.player.questLog.Count; i++)
                Game1.player.questLog[i].checkIfComplete((NPC)null, -1, -1, (Item)null, (string)null);
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            //despite being in the game, WeedingQuests crash the save serializer.
            if (Game1.questOfTheDay is WeedingQuest)
            {
                Game1.questOfTheDay = null;
            }

            for (int i = 0; i < Game1.player.questLog.Count; i++)
            {
                if (Game1.player.questLog[i] is WeedingQuest)
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

            WeedingQuest quest = (WeedingQuest)Game1.questOfTheDay;

            int numWeeds = Helper.Reflection.GetPrivateMethod(quest, "weedsLeft").Invoke<int>();
            varDump += $"{Environment.NewLine} Weeds Left (internal): {numWeeds}";

            Monitor.Log(Environment.NewLine + varDump + Environment.NewLine);
        }

        private void GetCurrentLocation(string arg1, string[] arg2)
        {
            Monitor.Log($"Current player location is ({Game1.player.getStandingPosition().X / Game1.tileSize}, {Game1.player.getStandingPosition().Y / Game1.tileSize})");
        }

        private void TimeEvents_AfterDayStarted(object sender, System.EventArgs e)
        {
            WeedingQuest testQuest = CreateWeedingQuest();
           
            Game1.questOfTheDay = testQuest;
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


        private WeedingQuest CreateWeedingQuest()
        {
            WeedingQuest _quest = new WeedingQuest()
            {
                questTitle = Helper.Translation.Get("qTitle.Weeding_1"),
                target = Game1.getCharacterFromName("Lewis")
            };

            //code from source game
            _quest.parts.Clear();
            _quest.parts.Clear();
            _quest.parts.Add((DescriptionElement)"Strings\\StringsFromCSFiles:WeedingQuest.cs.13816");
            _quest.parts.Add((DescriptionElement)"Strings\\StringsFromCSFiles:WeedingQuest.cs.13817");
            _quest.parts.Add((DescriptionElement)"Strings\\StringsFromCSFiles:SocializeQuest.cs.13791");
            _quest.dialogue = (DescriptionElement)"Strings\\StringsFromCSFiles:WeedingQuest.cs.13819";

            _quest.currentObjective = "";
            //create the weeds

            _quest.totalWeeds = Dice.Next(5, 25);
            GameLocation spawnLoc = Game1.getLocationFromName("Town");
            int weedsCreated = CreateWeeds(spawnLoc, _quest.totalWeeds);
            _quest.moneyReward = weedsCreated * 25;
            _quest.totalWeeds = WeedsLeft();

            //set duration
            _quest.daysLeft = 1; 
            _quest.completionString = "Strings\\StringsFromCSFiles:WeedingQuest.cs.13819";

            //hookup events to allow the quest to be completed
            GameLocation loc = Game1.getLocationFromName("Town");
            loc.objects.CollectionChanged += CheckForDestroyedObjects;

            return _quest;
        }

        private int CreateWeeds(GameLocation spawnLoc, int numOfWeeds)
        {
            int CreatedWeeds = 0;

            Monitor.Log($"Map dimensions are {spawnLoc.map.DisplayWidth} and {spawnLoc.map.DisplayHeight}");

            for (int i = 0; i <= numOfWeeds; i++)
            {
                //limit number of attempts per attempt to 10.
                int numberOfAttempts = 0;
                while (numberOfAttempts < 3)
                { 
                    //get a random tile.
                    int xTile = Game1.random.Next(spawnLoc.map.DisplayWidth / Game1.tileSize);
                    int yTile = Game1.random.Next(spawnLoc.map.DisplayHeight / Game1.tileSize);
                    Vector2 randomVector = new Vector2((float)xTile, (float)yTile);
                    spawnLoc.objects.TryGetValue(randomVector, out SObject @object);

                    if (SDVUtilities.TileIsClearForSpawning(spawnLoc, randomVector, @object))
                    {
                        //for now, don't spawn in winter.
                        if (Game1.currentSeason != "winter")
                        {
                            //spawn the weed
                            Monitor.Log($"Spawning weed at {randomVector.ToString()}");
                            spawnLoc.objects.Add(randomVector, new SObject(randomVector, GameLocation.getWeedForSeason(Game1.random, Game1.currentSeason), 1));
                            CreatedWeeds++;
                        }
                    }
                    numberOfAttempts++; // this might have been more useful INSIDE the while loop.
                }
            }
            return CreatedWeeds;
        }

    }
}
