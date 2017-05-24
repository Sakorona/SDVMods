1.1
- Updated for SMAPI 1.13
- Refactored to make it easier to add things in the future. 
- Cleaned up some of the debug spam
- Added new custom weathers: Thundersnow, Dry Lightning, Blizzards
- Added fog in the mornings, and it should now fade out
- Changed the weather system to run off of a .json, which also allows for custom user climates
- Fixed an issue with the menu popping up when the key was type din a box
- Heatwaves and Blizards now drain stamina.
- Fixed the issue where drinking the muscle relaxer didn't stop the cold
- TV now uses a mod to allow for other mods altering TV channels to work with this one
- Tweaked the penalty for colds
- Collection Enumeration error resolved
- Fixed: Debris weather via console didn't trigger the debris. 
- Moved TV strings to a .json file, and rewrote a few to flow better, to allow for customization down the line
- Spellchecked the mod in places
- Moved crops into it's own file for future consideration
- Updated the documentation

1.0
- Final release for SDV 1.2
- Updated for new functionality in 1.2
- Added in weather commands, to use type in help in the command base
- Some tweaks

1.0rc11
- added in a way to clear exhaustion
- fix so the weather system won't try to set weather on force days. Instead, it'll just output the weather from the force days

1.0rc10
- Added in an option to allow storm changes to change the rain to stormy

1.0rc9
- Fixed a logic error that would make it rain far too often
- Made the 'dry' setting more detailed during Spring.
- Refined the stamina shaker
- Attempted to help clean up the events vs load time
- Some tweaks to dialogue
- Resolved a display error that would cause festivals to display for both the day of and the next

1.0rc1
- Added in stamina drain on stormy days. See the documents folder for more information
- Implented a debug option to allow detailed logging to become viable
- Added a dual temperature display
- Added a temperature menu option
- Moved RNG rolls to a custom PRNG to allow for better randomization
- Added hazardous events such as frosts and heatwaves
- Added a moon, which has special events on a new moon and full moon. See the documents folder for more information.
- Refactored for easier updating later
- Better usage of HUD messages
- Updated to remove deprecation warnings on SMAPI 1.8 and earlier.


0.8.0
- Updated to a new more fluid method of generating weather patterns
- Overhauled the options list
- Added temperature

v0.7.0
- Added in some logic to check for festivals
- Added in some checks so that it doesn't overwrite TV forecast

v0.6.1
- fixed a logic error that would have caused weather to behave oddly