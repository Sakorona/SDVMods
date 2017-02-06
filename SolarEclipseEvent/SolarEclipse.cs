using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley;
using System;
using Microsoft.Xna.Framework;

namespace SolarEclipseEvent
{
    public class SolarEclipseEvent : Mod
    {
        public EclipseConfig Config { get; set; }
        public bool GameLoaded { get; set; }
        public bool IsEclipse { get; set; }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<EclipseConfig>();

            Command.RegisterCommand("world_solareclipse", "Starts the solar eclipse").CommandFired += SolarEclipseEvent_CommandFired;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            TimeEvents.DayOfMonthChanged += TimeEvents_DayOfMonthChanged;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
            
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (IsEclipse)
            {
                Game1.currentLocation.switchOutNightTiles();
            }
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (IsEclipse)
            {
                Game1.globalOutdoorLighting = .5f;
                Game1.outdoorLight = Game1.eveningColor;
                Game1.currentLocation.switchOutNightTiles();

                if (Game1.spawnMonstersAtNight && Game1.farmEvent == null && Config.SpawnMonsters && Game1.random.NextDouble() < 0.25 - Game1.dailyLuck / 2.0)
                {
                    if (Game1.random.NextDouble() < 0.25)
                    {
                        if (this.Equals(Game1.currentLocation))
                        {
                            Game1.getFarm().spawnFlyingMonstersOffScreen();
                            return;
                        }
                    }
                    else
                    {
                        Game1.getFarm().spawnGroundMonsterOffScreen();
                    }
                }
               
            }
        }

        private void TimeEvents_DayOfMonthChanged(object sender, EventArgsIntChanged e)
        {
            Random r = new Random();
            if (r.NextDouble() < Config.EclipseChance)
            {
                IsEclipse = true;
            }
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
            IsEclipse = true;
            Game1.globalOutdoorLighting = .5f; //force lightning change.
            Game1.currentLocation.switchOutNightTiles();
            Game1.outdoorLight = Game1.eveningColor;
            Monitor.Log("Setting the eclipse event to true");
        }

    }
}
