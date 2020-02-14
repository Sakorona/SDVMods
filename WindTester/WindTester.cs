using System;

namespace WindTester
{
    public class WindTester
    {
    }
}

//    public class ClimatesOfFerngill : Mod
    {
        /// <summary> The options file </summary>
        internal static WeatherConfig WeatherOpt { get; set; }

/// <summary> The pRNG object </summary>
internal static MersenneTwister Dice;

/// <summary> The current weather conditions </summary>
internal static WeatherConditions Conditions;

//provide common interfaces for logging, and access to SMAPI APIs
internal static IMonitor Logger;
internal static IReflectionHelper Reflection;
internal static IMultiplayerHelper MPHandler;
internal static ITranslationHelper Translator;

/// <summary> The climate for the game </summary>
private static FerngillClimate GameClimate;

/// <summary> This is used to display icons on the menu </summary>
internal static Sprites.Icons OurIcons { get; set; }
private HUDMessage queuedMsg;

/// <summary> This is used to allow the menu to revert back to a previous menu </summary>
private IClickableMenu PreviousMenu;
private Descriptions DescriptionEngine;
private Rectangle RWeatherIcon;
private bool Disabled = false;
private bool HasGottenSync = false;
private bool HasRequestedSync = false;
private static bool IsBloodMoon = false;
private float weatherX;
private bool SummitRebornLoaded;

//Integrations
internal static bool UseLunarDisturbancesApi = false;
internal static Integrations.ILunarDisturbancesAPI MoonAPI;
internal static bool UseSafeLightningApi = false;
internal static Integrations.ISafeLightningAPI SafeLightningAPI;
internal static bool UseDynamicNightApi = false;
internal static Integrations.IDynamicNightAPI DynamicNightAPI;

/// <summary> Provide an API interface </summary>
private IClimatesOfFerngillAPI API;
public override object GetApi()
{
    if (API == null)
        API = new ClimatesOfFerngillAPI(Conditions, WeatherOpt);

    return API;
}

/// <summary>The mod entry point, called after the mod is first loaded.</summary>
/// <param name="helper">Provides simplified APIs for writing mods.</param>
public override void Entry(IModHelper helper)
{
