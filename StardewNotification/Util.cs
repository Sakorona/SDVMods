using System.Collections.Generic;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;

namespace StardewNotification
{
    public static class Util
    {
        private const int MUSHROOM_CAVE = 2;

        public static void ShowMessage(string msg)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, StardewNotification.Config.NotificationDuration , true)
            {
                whatType = 2
            };
            Game1.addHUDMessage(hudmsg);
        }

        public static void ShowHarvestableMessage(ITranslationHelper Trans, KeyValuePair<string, Pair<Object, int>> pair)
        {
            Game1.addHUDMessage(new HUDMessage(pair.Key, pair.Value.Second, true, Color.OrangeRed, pair.Value.First));
        }             
    }
}
