using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace TimeReminder
{
    public class TimeReminder : Mod
    {
        public TimeConfig Config { get; set; }
        private DateTime PrevDate { get; set; }

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<TimeConfig>();
            PrevDate = DateTime.Now;

            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            int stage = 0, daysLeft = 0;
            int numCropsUnsynced = 0;

            Farm f = Game1.getFarm();
            foreach (KeyValuePair<Vector2,TerrainFeature> tf in f.terrainFeatures)
            {
                if (tf.Value is HoeDirt h && h.crop != null)
                {
                    if (stage != h.crop.currentPhase && daysLeft != h.crop.dayOfCurrentPhase)
                    {
                        stage = h.crop.currentPhase;
                        daysLeft = h.crop.dayOfCurrentPhase;
                        numCropsUnsynced++;
                    }

                }
            }

            numCropsUnsynced--; //subtract 1
            Game1.addHUDMessage(new HUDMessage($"We've got {numCropsUnsynced} unsync'd crops"));
        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            if (PrevDate.Add(new TimeSpan(0,Config.NumOfMinutes,0)) == DateTime.Now){
                Game1.hudMessages.Add(new HUDMessage("The current system time is " + DateTime.Now.ToString("h:mm:ss tt")));
                PrevDate = DateTime.Now;
            }
        }
    }
}
