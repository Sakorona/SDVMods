using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace SolarEclipseEvent
{
    public class SolarEclipseEvent : Mod
    {
        public EclipseConfig Config { get; set; }
        public bool GameLoaded { get; set; }
        public bool IsEclipse { get; set; }

        public SolarEclipseEvent()
        {
            Config = Helper.ReadConfig<EclipseConfig>();

            Command.RegisterCommand("world_solareclipse", "Starts the solar eclipse").CommandFired += SolarEclipseEvent_CommandFired;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (IsEclipse)
                Game1.globalOutdoorLighting = 1f;
        }

        private void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            Random r = new Random();
            if (r.NextDouble() < Config.EclipseChance)
            {
                IsEclipse = true;
            }

            if (IsEclipse)
                Game1.globalOutdoorLighting = 1f; //midnight during the daaaayyy!
        }

        private void SaveEvents_BeforeSave(object sender, System.EventArgs e)
        {
            if (IsEclipse)
                IsEclipse = false;
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            GameLoaded = true;
        }

        private void SolarEclipseEvent_CommandFired(object sender, StardewModdingAPI.Events.EventArgsCommand e)
        {
            throw new System.NotImplementedException();
        }

    }
}
