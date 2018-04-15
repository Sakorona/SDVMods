This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation, licenses, and release notes. May have other related projects that are here for tracking.

## Mods
 
* **[Customizable Cart Redux](https://rd.nexusmods.com/stardewvalley/mods/1402)**<small> ([source](CustomizableCartRedux))</small>
  _Lets you customize when the cart appears, and how many items it has. Contains an API for other mods to hook into._
  
* **LunarDisturbances**<small>([source](LunarDisturbances))</small>
_This mod adds a moon, which adds events, such as the Blood Moon, to the world. Also includes a solar eclipse!_

* **WeatherIllnesses**<small>([source](WeatherIllnesses))</small>
_This adds a stamina system to the game. In hazardous weather, the player will drain stamina per 10 minutes.__

* **USDVP** <small>([source](USDVP))</small>
  _A in progress patch mod for Stardew Valley. On hiatus until after the SDV 1.3 beta ends._

* **ArtifactsInOmniGeodes**<small> ([source](ArtifactsInOmniGeodes))</small>
  _Baby port of a XNB mod that no longer works with SDV 1.2 for personal use._

* **[Climates of Ferngill (Rebuild)](http://www.nexusmods.com/stardewvalley/mods/604)** <small>([source](ClimatesOfFerngill))</small>  
  _Creates a different climate system, with custom weather, including fog and thunder frenzy. Also stamina changes. Follow the link for more details._

* **[Time Reminder](http://www.nexusmods.com/stardewvalley/mods/1000)** <small>([source](TimeReminder))</small>  
  _Alerts you of system time after a configurable number of minutes._
  
## Depricated Mods
* **[Solar Eclipse Event](http://www.nexusmods.com/stardewvalley/mods/897)** <small>([source](SolarEclipseEvent))</small>  
  _This adds the possibility of a solar eclipse to your game! Configurable chance. Also will spawn monsters if you have a wilderness farm in line with the same percentages as the base game. Now implemented into Lunar Disturbances, and as of SDV 1.3, no longer maintained.._
  
## Patches for SMAPI 2.0

* **[Better Shipping Box](https://community.playstarbound.com/threads/better-shipping-box.126235/#post-3228667)**<small> ([source](BetterShippingBox))</small>
  _This is a SMAPI 2.0 port of Kthio's Better Shipping Box, which gives you a menu. I recommend using [Chests Anywhere](https://www.nexusmods.com/stardewvalley/mods/518) by Pathoschild instead. _
  
* **[Tree Overhaul](https://github.com/JohnsonNicholas/SDVMods/releases/download/1.2.0/TreeOverhaul.zip)**<small>([source](TreeOverhaul))</small>
  _This is the source code for the unofficial patch I am maintaining to make it compatibile with 2.0. Original author is Goldenrevolver._
  
* **[StardewNotification](https://github.com/JohnsonNicholas/SDVMods/releases/download/stardew-notifcation/StardewNotification.1.7.2-kylindraUpdate.zip)**<small>([source](StardewNotification))</small>
_Source code for Stardew Notification, currently maintained by me. MIT License. Original author is monopandora_

## Compiling the mods
Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, read on.

These mods use the [crossplatform build config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
so they can be built on Linux, Mac, and Windows without changes. See [the build config documentation](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
for troubleshooting.

### Compiling a mod for testing
To compile a mod and add it to your game's `Mods` directory:

1. Rebuild the project in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).  
   <small>This will compile the code and package it into the mod directory.</small>
2. Launch the project with debugging.  
   <small>This will start the game through SMAPI and attach the Visual Studio debugger.</small>

### Compiling a mod for release
To package a mod for release:

1. Delete the mod's directory in `Mods`.  
   <small>(This ensures the package is clean and has default configuration.)</small>
2. Recompile the mod per the previous section.
3. Create a zip file of the mod's folder in the `Mods` folder. The zip name should include the
   mod name and version (like `ClimateOfFerngill-1.0.zip`).
