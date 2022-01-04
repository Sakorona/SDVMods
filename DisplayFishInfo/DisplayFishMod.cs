using StardewModdingAPI;

namespace DisplayFishInfo
{
    public class DisplayFishMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            //ha!
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
