using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using TwilightShards.Stardew.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using System.Reflection;
using Harmony;

namespace TwilightShards.FerngillTaxes
{
    public class FerngillTaxes : Mod
    {
        internal static TaxConfig Options;
        private bool UseJsonAssetsApi = false;
        private Integrations.IJsonAssetsApi JAAPi;

        private static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";

            return Type.GetType(prefix + type + ", Stardew Valley") ?? Type.GetType(prefix + type + ", StardewValley");
        }

        public override void Entry(IModHelper helper)
        {
            Options = helper.ReadConfig<TaxConfig>();

            /* var harmony = HarmonyInstance.Create("koihimenakamura.ferngilltaxes");
            harmony.PatchAll(Assembly.GetExecutingAssembly()); */

            helper.Events.Display.MenuChanged += OnMenuChanged;            
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JAAPi = SDVUtilities.GetModApi<Integrations.IJsonAssetsApi>(Monitor, Helper, "spacechase0.JsonAssets", "1.1");

            if (JAAPi != null)
            {
                UseJsonAssetsApi = true;
                JAAPi.AddedItemsToShop += JAAPi_AddedItemsToShop;
                Monitor.Log("JsonAssets Integration enabled", LogLevel.Info);
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!UseJsonAssetsApi)
            {
                if (e.NewMenu is ShopMenu menu && menu.portraitPerson != null)
                {
                    double sellValue = 1 - Options.taxPercentage;
                    double buyValue = 1 + Options.taxPercentage;

                    Helper.Reflection.GetField<float>(menu, "sellPercentage").SetValue((float)sellValue);
                    var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();
                    foreach (KeyValuePair<Item, int[]> kvp in itemPriceAndStock)
                    {
                        kvp.Value[0] = (int)Math.Floor(kvp.Value[0] * buyValue);
                    }
                }
            }
        }

        private void JAAPi_AddedItemsToShop(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu menu)
            {
                double sellValue = 1 - Options.taxPercentage;
                double buyValue = 1 + Options.taxPercentage;

                Helper.Reflection.GetField<float>(menu, "sellPercentage").SetValue((float)sellValue);
                var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();
                foreach (KeyValuePair<Item, int[]> kvp in itemPriceAndStock)
                {
                    kvp.Value[0] = (int)Math.Floor(kvp.Value[0] * buyValue);
                }
            }
        }
    }
}
