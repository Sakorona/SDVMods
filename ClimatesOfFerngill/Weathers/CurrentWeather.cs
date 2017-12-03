using System;


namespace ClimatesOfFerngillRebuild
{
    /*Notes on changes from last SpecialWeather enum
   Thundersnow is now both Snow and Lightning
   DryLightning is Thunder but not Rain
   DryLightningHeatwave is now DryLighting and if temp returns heatwave conditions. (They've been intentionally moved to a seperate section.)
   Default flag is now unset.
  */

    /// <summary>
    /// This enum tracks weathers added to the system as well as the current weather.
    /// </summary>
    [Flags]
    public enum CurrentWeather
    {
        Unset = 0,
        Sunny = 2,
        Rain = 4,
        Snow = 8,
        Wind = 16,
        Festival = 32,
        Wedding = 64,
        Lightning = 128,
        Blizzard = 256,
        Fog = 512,
        Frost = 1024,
        Heatwave = 2048
    }
}
