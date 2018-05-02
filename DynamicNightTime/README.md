#Dynamic Night Time

## Requirements
- SMAPI 2.6-beta3+
- Stardew Valley 1.3.3

## This Mod Does:
- Creates a more dynamic night time and sunrise lighting system. Does not alter wakeup time.

## Multiplayer Compatiblity
This mod *should* be MP compatible. 

## Installation Instructions
- Unzip the folder into your Mods/ folder after making sure SMAPI is installed

## Changelog
1.0.5
 - sunset starts at -30 minutes now. This avoids some issues, but can be configured to turn off.
 - more agressive progression
 - now 1.3 compatible
1.0.4
 - refactor check
 - fix in the underlying library for time managment that should properly estimate times
1.0.3
 - Fixed NRE on critter check.
1.0.2
 - Added update key

## Options Config
To configure, open up `config.json` in your mod folder.

-`SunsetTimesAreMinusThirty` - Valid is true or false. This applies a half hour corrective to the generated sunset.

- `latitude` - Valid is anything from -64 to 64. (- for lats S, + for lats N.). Sets the latitude that will determine sunrise and sunset times. Out of range latitudes will be reset. To find the latitude for your city, please consult google.