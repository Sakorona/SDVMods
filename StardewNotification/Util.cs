using StardewValley;

namespace StardewNotification
{
    public static class Util
    {
        public static void ShowMessage(string msg)
        {
            var hudMsg = new SNHudMessage(msg, timeLeft: StardewNotification.Config.NotificationDuration, true)
            {
                noIcon = true
            };
            Game1.addHUDMessage(hudMsg);
        }           
    }
}
