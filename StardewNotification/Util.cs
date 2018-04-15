using System.Collections.Generic;

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
            item.bigCraftable = pair.Value.First.bigCraftable;
            Game1.addHUDMessage(new HUDMessage(pair.Key, pair.Value.Second, true, Color.OrangeRed, item));
        }

        public static void ShowFarmCaveMessage(GameLocation location, ITranslationHelper Trans)
        {
            var e = location.Objects.GetEnumerator();
            e.MoveNext();
            var item = CopyObject(e.Current.Value);
            item.name = Game1.player.caveChoice == MUSHROOM_CAVE ? Trans.Get("CaveMushroom") : Trans.Get("CaveFruit");
            Game1.addHUDMessage(new HUDMessage(item.type, location.Objects.Count, true, Color.OrangeRed, item));
        }

        public static Object CopyObject(Object source)
        {
			Object dst;
            dst = new Object(source.ParentSheetIndex, source.Stack, false, source.Price, source.quality);
            if (Game1.player.caveChoice == MUSHROOM_CAVE)
            {
                dst.bigCraftable = source.bigCraftable;
                dst.tileLocation = source.tileLocation;
            }
            dst.type = source.Type;
            return dst;
        }
    }
}
