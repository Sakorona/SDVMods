using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.IO;

namespace TwilightShards.YouShouldRest
{
    //public class YouShouldRest : Mod, IAssetEditor
    public class YouShouldRest : Mod
    {
        internal Dictionary<string, string> ModDialogues;
        private readonly IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            ModDialogues = new Dictionary<string, string>();

            helper.Events.GameLoop.SaveLoaded += LoadPacksOnLoad;
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        private void LoadPacksOnLoad(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            foreach (IContentPack contentPack in helper.ContentPacks.GetOwned())
            {
                if (File.Exists(Path.Combine(contentPack.DirectoryPath, "dialogue.json")))
                {
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
                    var rawData = contentPack.ReadJsonFile<List<RestModel>>("dialogue.json");
                    foreach (var r in rawData)
                    {
                        ModDialogues.Add(r.Conditions, r.Dialogue);
                    }
                }
                else
                {
                    Monitor.Log($"Ignoring content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}\nIt does not have an dialogue.json file.", LogLevel.Warn);
                }
            }
        }

        private bool StaminaCheck()
        {
            if (Game1.player.Stamina / Game1.player.MaxStamina <= .825)
                return true;
            else if (Game1.player.health / Game1.player.maxHealth <= .825)
                return true;

            return false;
        }

        /// <summary>
        /// This function returns the key for a given dialogue. 
        /// </summary>
        /// <param name="character">NPC for the dialogue</param>
        /// <returns>The key for the dialogue</returns>
        private string GetDialogueForConditions(NPC character)
        {
            //json format ex: abigail[10]summer_4_4_spouse
            // name[heartlevel]season_healthstatus_staminastatus_timeofday_spousestatus
            int HeartLevel = Game1.player.getFriendshipHeartLevelForNPC(character.Name);
            int HealthStatus = GetHealthLevel(Game1.player.health, Game1.player.maxHealth);
            int StaminaStatus = GetStaminaLevel(Game1.player.Stamina, Game1.player.MaxStamina);
            int TimeOfDay = GetTimeOfDay(Game1.timeOfDay);
            string SpouseStatus = Game1.player.friendshipData[character.Name].IsMarried() ? "spouse" : "";


            //fallback to disposition text
            //age, manners?,social anxiety, optimism
            //format is:
            //age_manners_social_timeofday_optimism
            // it tries to find the longest one first, then works backwards


            //fallback to default if EVERYTHING's missing.
            return "default";
        }

        private void OnMenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.NewMenu is DialogueBox dBox && !Game1.eventUp && StaminaCheck())
            {
                var cDBU = Helper.Reflection.GetField<Stack<string>>(dBox, "characterDialoguesBrokenUp").GetValue();
                Dialogue diag = Helper.Reflection.GetField<Dialogue>(dBox, "characterDialogue").GetValue();
                cDBU.Push(Helper.Translation.Get(GetDialogueForConditions(diag.speaker)));
                Helper.Reflection.GetField<Stack<string>>(dBox, "characterDialoguesBrokenUp").SetValue(cDBU);
            }
        }
    }
}
