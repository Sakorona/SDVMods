using StardewValley;
using StardewValley.Quests;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using StardewModdingAPI;

namespace TwilightShards.QuestOverhaul.QuestTypes
{
    class KN_WeedingQuest : Quest
    {
        public List<DescriptionElement> parts = new List<DescriptionElement>();
        public List<DescriptionElement> dialogueparts = new List<DescriptionElement>();
        public NPC target;
        public string targetMessage;
        public int reward;
        public bool complete;
        public int totalWeeds;
        public DescriptionElement objective;
        public GameLocation targetLocation;

        private ITranslationHelper Helper;

        public KN_WeedingQuest(ITranslationHelper Help)
        {
            this.questType = 11;
            Helper = Help;
        }

        public void LoadQuestInfo()
        {
            this.target = Game1.getCharacterFromName("Lewis", false);
            this.targetLocation = Game1.getLocationFromName("Town");
            this.questTitle = Helper.Get("qTitle.Weeding_1");
            this.reward = 300; //???
            this.moneyReward = 300;
            this.parts.Clear();
            this.parts.Add((DescriptionElement)"Strings\\StringsFromCSFiles:WeedingQuest.cs.13816");
            this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:WeedingQuest.cs.13817", (object)this.reward));
            this.parts.Add((DescriptionElement)"Strings\\StringsFromCSFiles:SocializeQuest.cs.13791");

            this.dialogueparts.Clear();
            this.dialogueparts.Add((DescriptionElement)"Strings\\StringsFromCSFiles:WeedingQuest.cs.13819");
            this.targetMessage = "Strings\\StringsFromCSFiles:WeedingQuest.cs.13819";

            this.totalWeeds = this.WeedsLeft();
        }
        
        public void SetLocation(GameLocation loc)
        {
            this.targetLocation = loc;
        }


        public override void accept()
        {
            base.accept();
            foreach (Object @object in Game1.getLocationFromName("Town").Objects.Values)
            {
                if (@object.name.Contains("Weed"))
                    this.totalWeeds = this.totalWeeds + 1;
            }
            this.checkIfComplete((NPC)null, -1, -1, (Item)null, (string)null);
        }

        public override void reloadDescription()
        {
            if (this._questDescription == "")
                this.LoadQuestInfo();
            if (this.parts.Count == 0 || this.parts == null)
                return;
            string str = "", str2 = "";
            foreach (DescriptionElement part in this.parts)
                str += part.loadDescriptionElement();
            this.questDescription = str;
            foreach (DescriptionElement dialoguepart in this.dialogueparts)
                str2 += dialoguepart.loadDescriptionElement();
            this.targetMessage = str2;
        }

        internal int WeedsLeft()
        {
            int num = 0;
            foreach (Object @object in Game1.getLocationFromName("Town").Objects.Values)
            {
                if (@object.name.Contains("Weed"))
                    ++num;
            }
            return num;
        }

        public override void reloadObjective()
        {
            if (this.WeedsLeft() > 0)
                this.objective = new DescriptionElement("Strings\\StringsFromCSFiles:WeedingQuest.cs.13826", (object)(this.totalWeeds - this.WeedsLeft()), (object)this.totalWeeds);
            if (this.objective == null)
                return;
            this.currentObjective = this.objective.loadDescriptionElement();
        }

        public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null)
        {
            if (n == null && !this.complete)
            {
                if (this.WeedsLeft() == 0)
                {
                    this.complete = true;
                    this.objective = new DescriptionElement("Strings\\StringsFromCSFiles:WeedingQuest.cs.13824");
                    Game1.playSound("jingle1");
                }
                if (Game1.currentLocation.Name.Equals("Town"))
                    Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(220f, 260f), false, false, 1f, 0.01f, Color.White, 4f, 0.3f, 0.0f, 0.0f, false)
                    {
                        scaleChangeChange = -0.015f
                    });
            }
            else if (n != null && n.Equals((object)this.target) && this.complete)
            {
                n.CurrentDialogue.Push(new Dialogue(this.targetMessage, n));
                Game1.player.Money += this.reward;
                foreach (string key in Game1.player.friendships.Keys)
                {
                    if (Game1.player.friendships[key][0] < 2729)
                        Game1.player.friendships[key][0] += 20;
                }
                Game1.drawDialogue(n);
                this.questComplete();
                return true;
            }
            else if ((object)n != null && this.target != null && (!this.target.Equals("null") && this.WeedsLeft() == 0 && (n.name.Equals(this.target) && n.isVillager())))
            {
                n.CurrentDialogue.Push(new Dialogue(this.targetMessage, n));
                this.moneyReward = this.reward;
                foreach (string key in Game1.player.friendships.Keys)
                {
                    if (Game1.player.friendships[key][0] < 2729)
                        Game1.player.friendships[key][0] += 20;
                }
                this.questComplete();
                Game1.drawDialogue(n);
                return true;
            }
            return false;
        }

    }
}
