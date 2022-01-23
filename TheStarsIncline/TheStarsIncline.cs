using StardewModdingAPI;

namespace TwilightShards.TheStarsIncline
{
    public class TheStarsIncline : Mod
    {
        private readonly int UniqueBaseID = 49134570;

        public override void Entry(IModHelper helper)
        {
            //woo!
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            
            
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            
        }
    }
}
