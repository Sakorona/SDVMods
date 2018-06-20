using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;

namespace StardewNotification
{
    public static class Util
    {
        private const int MUSHROOM_CAVE = 2;
        private const int FRUIT_CAVE = 1;

        public static void ShowMessage(string msg)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true)
            {
                whatType = 2
            };
            Game1.addHUDMessage(hudmsg);
        }

        public static void ShowHarvestableMessage(ITranslationHelper Trans, KeyValuePair<string, Pair<StardewValley.Object, int>> pair)
        {
            var item = CopyObject(pair.Value.First);
            item.name = Trans.Get("readyHarvest", new { cropName = pair.Key }); 
            item.bigCraftable.Value = pair.Value.First.bigCraftable.Value;
            Game1.addHUDMessage(new HUDMessage(pair.Key, pair.Value.Second, true, Color.OrangeRed, item));
        }

        public static void ShowFarmCaveMessage(GameLocation location, ITranslationHelper Trans)
        {
            int i = 0;
            var item = CopyObject(location.Objects.Pairs.ElementAt(i).Value);
            while (item.Category == StardewValley.Object.FishCategory || item.bigCraftable.Value && i < (location.Objects.Pairs.Count() - 1))
            {
                i++;
                item = CopyObject(location.Objects.Pairs.ElementAt(i).Value);
            }
            item.name = Game1.player.caveChoice.Value == MUSHROOM_CAVE ? Trans.Get("CaveMushroom") : Trans.Get("CaveFruit");
            Game1.addHUDMessage(new HUDMessage(item.Type, location.Objects.Pairs.Count(), true, Color.OrangeRed, item));
        }

        public static Object CopyObject(Object source)
        {
			Object dst;
            dst = new Object(source.ParentSheetIndex, source.Stack, false, source.Price, source.Quality);
            if (Game1.player.caveChoice.Value == MUSHROOM_CAVE)
            {
                dst.bigCraftable.Value = source.bigCraftable.Value;
                dst.TileLocation = source.TileLocation;
            }
            dst.Type = source.Type;
            return dst;
        }
    }
}
