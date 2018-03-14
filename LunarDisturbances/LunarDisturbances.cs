using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using TwilightShards.Common;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace TwilightShards.LunarDisturbances
{
    public class LunarDisturbances : Mod
    {
        private SDVMoon OurMoon;
        private MersenneTwister Dice;
        private MoonConfig ModConfig;
        private Sprites.Icons OurIcons { get; set; }
        public bool IsEclipse { get; set; }
        private bool UseJsonAssetsApi = false;
        private Color nightColor = new Color((int)byte.MaxValue, (int)byte.MaxValue, 0);
        private Integrations.IJsonAssetsApi JAAPi;
        public int ResetTicker { get; set; }

        private ILunarDisturbancesAPI API;

        public override object GetApi()
        {
            if (API == null)
                API = new LunarDisturbancesAPI(OurMoon, IsEclipse);

            return API;
        }
        /// <summary> Main mod function. </summary>
        /// <param name="helper">The helper. </param>
        public override void Entry(IModHelper helper)
        {
            Dice = new MersenneTwister();
            ModConfig = Helper.ReadConfig<MoonConfig>();
            OurMoon = new SDVMoon(ModConfig, Dice, Helper.Translation);

            GameEvents.FirstUpdateTick += GameEvents_FirstUpdateTick;
            GraphicsEvents.OnPostRenderGuiEvent += DrawOverMenus;
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
            TimeEvents.TimeOfDayChanged += TenMinuteUpdate;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            SaveEvents.BeforeSave += OnEndOfDay;
            GameEvents.UpdateTick += CheckForChanges;
            SaveEvents.AfterReturnToTitle += ResetMod;
        }

        private void GameEvents_FirstUpdateTick(object sender, EventArgs e)
        {
            IManifest manifestCheck = Helper.ModRegistry.Get("spacechase0.JsonAssets");
            if (manifestCheck != null)
            {
                if (!manifestCheck.Version.IsOlderThan("1.1"))
                {
                    JAAPi = Helper.ModRegistry.GetApi<Integrations.IJsonAssetsApi>("spacechase0.JsonAssets");

                    if (JAAPi != null)
                    {
                        UseJsonAssetsApi = true;
                        JAAPi.AddedItemsToShop += JAAPi_AddedItemsToShop;
                        Monitor.Log("JsonAssets Integration enabled", LogLevel.Info);
                    }
                }
                else
                {
                    Monitor.Log($"JsonAssets detected, but not of a sufficient version. Req:1.1.0. Detected:{manifestCheck.Version.ToString()}. Skipping..");
                }
            }
            else
            {
                Monitor.Log("JsonAssets not present. Skipping Integration.");
            }
        }

        private void DrawOverMenus(object sender, EventArgs e)
        {
            bool outro = false;
            //revised this so it properly draws over the canon moon. :v
            if (Game1.activeClickableMenu is ShippingMenu ourMenu)
            {
                outro = Helper.Reflection.GetField<bool>(ourMenu, "outro").GetValue();
            }

            if (Game1.showingEndOfNightStuff && !Game1.wasRainingYesterday && !outro && Game1.activeClickableMenu is ShippingMenu currMenu)
            {
                Game1.spriteBatch.Draw(OurIcons.MoonSource, new Vector2(Game1.viewport.Width - 65 * Game1.pixelZoom, Game1.pixelZoom), OurIcons.GetNightMoonSprite(SDVMoon.GetLunarPhaseForDay(SDate.Now().AddDays(-1))), Color.LightBlue, 0.0f, Vector2.Zero, Game1.pixelZoom * 1.5f, SpriteEffects.None, 1f);
            }
        }

        /// <summary>
        /// This function handles the end of the day.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndOfDay(object sender, EventArgs e)
        {
            //cleanup any spawned monsters
            foreach (GameLocation l in Game1.locations)
            {
                if (l is Farm)
                    continue;

                for (int index = l.characters.Count - 1; index >= 0; --index)
                {
                    if (l.characters[index] is Monster)
                        l.characters.RemoveAt(index);
                }
            }

            if (IsEclipse)
                IsEclipse = false;

            //moon works after frost does
            OurMoon.HandleMoonAtSleep(Game1.getFarm());
        }

        /// <summary>
        /// This handles location changes
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Parameters</param>
        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (IsEclipse)
            {
                Game1.globalOutdoorLighting = .5f;
                Game1.currentLocation.switchOutNightTiles();
                Game1.ambientLight = nightColor;

                if (!Game1.currentLocation.isOutdoors && Game1.currentLocation is DecoratableLocation)
                {
                    var loc = Game1.currentLocation as DecoratableLocation;
                    foreach (Furniture f in loc.furniture)
                    {
                        if (f.furniture_type == Furniture.window)
                            Helper.Reflection.GetMethod(f, "addLights").Invoke(new object[] { Game1.currentLocation });
                    }
                }
            }

            if (OurMoon.CurrentPhase == MoonPhase.BloodMoon)
            {
                Game1.currentLocation.waterColor = OurMoon.BloodMoonWater;
            }
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is null)
                Monitor.Log("e.NewMenu is null");
            if (!UseJsonAssetsApi)
            {
                if (e.NewMenu is ShopMenu menu && menu.portraitPerson != null)
                {
                    if (OurMoon.CurrentPhase == MoonPhase.BloodMoon)
                    {
                        Helper.Reflection.GetField<float>(menu, "sellPercentage").SetValue(.75f);
                        var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();
                        foreach (KeyValuePair<Item, int[]> kvp in itemPriceAndStock)
                        {
                            kvp.Value[0] = (int)Math.Floor(kvp.Value[0] * 1.85);
                        }
                    }
                    else
                    {
                        if (Helper.Reflection.GetField<float>(menu, "sellPercentage").GetValue() != 1f)
                        {
                            Helper.Reflection.GetField<float>(menu, "sellPercentage").SetValue(1f);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This checks for things every second.
        /// </summary>
        /// <param name="sender">Object sending</param>
        /// <param name="e">event params</param>
        private void CheckForChanges(object sender, EventArgs e)
        {
            if (IsEclipse && ResetTicker > 0)
            {
                Game1.globalOutdoorLighting = .5f;
                Game1.ambientLight = nightColor;
                Game1.currentLocation.switchOutNightTiles();
                ResetTicker = 0;
            }
        }

        /// <summary>
        /// Handles the ten minute update tick
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Parameters</param>
        private void TenMinuteUpdate(object sender, EventArgsIntChanged e)
        {
            if (IsEclipse)
            {
                Game1.globalOutdoorLighting = .5f;
                Game1.ambientLight = nightColor;
                Game1.currentLocation.switchOutNightTiles();
                ResetTicker = 1;

                if (!Game1.currentLocation.isOutdoors && Game1.currentLocation is DecoratableLocation)
                {
                    var loc = Game1.currentLocation as DecoratableLocation;
                    foreach (Furniture f in loc.furniture)
                    {
                        if (f.furniture_type == Furniture.window)
                            Helper.Reflection.GetMethod(f, "addLights").Invoke(new object[] { Game1.currentLocation });
                    }
                }

                if ((Game1.farmEvent == null && Game1.random.NextDouble() < (0.25 - Game1.dailyLuck / 2.0))
                    && ((ModConfig.SpawnMonsters && Game1.spawnMonstersAtNight) || (ModConfig.SpawnMonstersAllFarms)))
                {
                    Monitor.Log("Spawning a monster, or attempting to.", LogLevel.Debug);
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

            //moon 10-minute
            OurMoon.TenMinuteUpdate();
        }

        private void ResetMod(object sender, EventArgs e)
        {
            OurMoon.Reset();
        }

        private void HandleNewDay(object sender, EventArgs e)
        {
            if (OurMoon == null)
                Monitor.Log("OurMoon is null");

            if (Dice.NextDouble() < ModConfig.EclipseChance && ModConfig.EclipseOn && OurMoon.CurrentPhase == MoonPhase.FullMoon &&
                SDate.Now().DaysSinceStart > 2)
            {
                IsEclipse = true;
                Game1.addHUDMessage(new HUDMessage("It looks like a rare solar eclipse will darken the sky all day!"));
            }

            OurMoon.OnNewDay();
            OurMoon.HandleMoonAfterWake();
        }

        private void JAAPi_AddedItemsToShop(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu menu)
            {
                if (OurMoon.CurrentPhase == MoonPhase.BloodMoon)
                {
                    Monitor.Log("Firing off replacement...");
                    Helper.Reflection.GetField<float>(menu, "sellPercentage").SetValue(.75f);
                    var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();
                    foreach (KeyValuePair<Item, int[]> kvp in itemPriceAndStock)
                    {
                        kvp.Value[0] = (int)Math.Floor(kvp.Value[0] * 1.85);
                    }
                }
                else
                {
                    if (Helper.Reflection.GetField<float>(menu, "sellPercentage").GetValue() != 1f)
                    {
                        Helper.Reflection.GetField<float>(menu, "sellPercentage").SetValue(1f);
                    }
                }
            }
        }
    }
}
