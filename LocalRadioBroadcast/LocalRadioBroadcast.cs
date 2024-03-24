using System.Collections.Generic;
using ResetSkullCaverns;
using StardewModdingAPI;
using StardewValley;

namespace LocalRadioBroadcast
{
    public class RadioConfig
    {
        public bool EchoBroadcastAtChanges = false;
        public bool AutoBroadcastAtNight = false;
        public int AutoBroadcastTime = 600;
        public bool Display24HTime = true;
        public SButton RadioTriggerKey = SButton.R;
        public bool EnableMaliciousDJ = true;
        public bool EnableBeneficalDJ = true;
        public float MaliciousThreshold = -0.05f;
        public float BeneficialThreshold = 0.05f;
        public int MaxAmount = 1000;        
    }

    public class LocalRadioBroadcast : Mod
    {
        RadioConfig ModConfig;
        public Dictionary<string, string> rapidCachedValues = new();
        public string cachedDMName;

        public override void Entry(IModHelper helper)
        {
            ModConfig = Helper.ReadConfig<RadioConfig>();
            cachedDMName = "";

            Helper.Events.GameLoop.ReturnedToTitle += OnTitleReturn;
            Helper.Events.GameLoop.TimeChanged += OnTimeChange;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunch;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnTitleReturn(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            cachedDMName = "";
            rapidCachedValues.Clear();
        }

        private void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
           if (Game1.IsGreenRainingHere())
            {
                //no signal.

            }

        }

        private void OnTimeChange(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            rapidCachedValues.Clear(); //clear the cache.
            if (ModConfig.EchoBroadcastAtChanges)
            {

            }
            else if (ModConfig.AutoBroadcastAtNight)
            {

            }

            else if (e.NewTime == ModConfig.AutoBroadcastTime && e.NewTime != 600) 
            { 
            
            }
        }

        private void OnGameLaunch(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var GMCMapi = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (GMCMapi != null)
            {
                GMCMapi.Register(ModManifest, () => ModConfig = new RadioConfig(), () => Helper.WriteConfig(ModConfig));
                GMCMapi.AddBoolOption(ModManifest, () => ModConfig.EchoBroadcastAtChanges, (bool val) => ModConfig.EchoBroadcastAtChanges = val, () => Helper.Translation.Get("gmcmEBACTitle"), () => Helper.Translation.Get("gmcmEBACDesc"));
                GMCMapi.AddBoolOption(ModManifest, () => ModConfig.AutoBroadcastAtNight, (bool val) => ModConfig.AutoBroadcastAtNight = val, () => Helper.Translation.Get("gmcmABANTitle"), () => Helper.Translation.Get("gmcmABANDesc"));
                GMCMapi.AddBoolOption(ModManifest, () => ModConfig.Display24HTime, (bool val) => ModConfig.Display24HTime = val, () => Helper.Translation.Get("gmcmD2HTitle"), () => Helper.Translation.Get("gmcmD2HDesc"));
                GMCMapi.AddNumberOption(ModManifest, () => ModConfig.AutoBroadcastTime, (int val) => ModConfig.AutoBroadcastTime = val, () => Helper.Translation.Get("gmcmABTTitle"), () => Helper.Translation.Get("gmcmABTDesc"), min: 600, max: 2600);
                GMCMapi.AddKeybind(ModManifest, () => ModConfig.RadioTriggerKey, (SButton val) => ModConfig.RadioTriggerKey = val, () => Helper.Translation.Get("gmcmRTKTitle"), () => Helper.Translation.Get("gmcmRTKDesc"));
            }
        }
    }
}