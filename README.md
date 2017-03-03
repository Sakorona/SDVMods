# Climate of Ferngill

This mod alters the weather of Stardew Valley by creating a climate system. It adds temperature and adds hostile weather events. 

The current version is __1.0.0 rc2 (rev 20170303)__ 

## Changelog

1.0.0 rc2
- A lot of minor changes and updates to wording throughout the TV display, including now putting different days on different messages
- more usage of hud messages
- Prototyping of a moon to be added 
- Implements it's own PRNG to help ensure proper randomness of weather and events
- Updates to remove deprecation warnings on SMAPI 1.8

0.8.6
- Frost and Heatwaves descend on the valley.
- Fall temps should work properly now

0.8.4
- bugfixes
- Moved to a mersenne twister implemenatation
- Cleaned out code, moved heatwave temp to the config file.
- Added a second temp gauge display.

0.8.2
- Stamina drain on stormy days.

0.8.0 
- New weather system
- Temperature!

## Features
* Custom TV dialogue (English only)
* A more complex weather system that can be generally configured to match 6 different climates
* A return of the penalty for being outside in a storm
* Hostile weather events (Heatwaves, and Floods)

## Planned Features
* Moving TV text options to a json/txt file for easier customization.
* Morning fog!
* Adding an item to remove exhaustion status
* A moon, including effects on various things

## Maybe features?
* Specifying climate options via json file
* Making all strings able to be translated
* A hotkey to trigger a weather window?

## Requirements
* Stardew Valley: __1.11__ (but not the beta version on Steam)
* SMAPI: __1.8__ or greater

## Configuration Options

* _SuppressLog_ can be set to __false__ or __true__  If true, the game will not log any events to the console. Default: __false__
* _AllowSnowOnFall28_ is a feature that will allow the possiblity of snow on the 28th of Fall. (Currently, 100%.) It can be set to __true__ or __false__ Default: __true__
* _AllowStormsFirstSpring_ allows storms the first spring. In the base game this is disabled. It can be set to __false__ or __true__.  Default: __false__
* _HarshWeather_ allows the hazards. It can be set to __false__ or __true__. Default: __false__        
* _NoLongerDisplayToday_ is a setting that says after this point: don't display today's weather. The valid setting is: 600 - 2600. Default: __1700__.
* _ClimateType_ is a setting that lets you control the type of climate. The options are: __arid__, __dry__, __normal__, __wet__, __monsoon__ and _traditional_. Default: __normal__
* _TempGauge_ is a setting that controls if temps are displayed in Celsius or Farenheit. The options are _celsius, fahrenheit_, default: _celsius_
* _DisplaySecondScale_ is a setting that controls if two gauges are displayed at once. Valid options are __false__ or __true__, default: __false__.
* _SecondScaleGauge_ is a setting that controls what the second gauge is. Only used if DisplaySecondScale is true. Options are _celsius, fahrenheit_.
* _StormyPenalty_ is a setting that enables the disease drain for being outside in a storm. The valid options are __true__ and __false.__. Default: __true__
* _DiseaseChance_ is a setting that controls how likely you are to get a disease from being outside. Valid numbers are (0 to 1), default _.475_
* _HeatwaveTime_ is a setting that controls when the heatwave will hit. Valid is any time. Default is set to _1600_.
* _FrostWarning_ is a setting that controls when you get the Frost Warning. __Recommened you do not alter this setting__. Default is set to _2_ (degrees Celsius).
* _FrostHardiness_ is a setting that controls the likelyhood of crops to die. Valid is any number, but the higher the number, the less likely crops are to die. Default is set to _.45_
* _DeathTemp_ controls when the crops can be killed instead of dewatered. Do not recommened setting this lower than the heatwave warning, or much below 38. Default is _41_.
* _AllowCropHeatDeath_ controls if you CAN kill crops instead of dewater them. Default is _false_.
* _TimeToDie_ controls how long before the game checks if you've rewatered the crops queued to die. Default is _310_ (3 hrs 10 mins)
* _SetLowCap_ controls if you can cap how cold fall temps can get. It is set to _false_ by default. 
* _LowCap_ controls the cap. Only used if _SetLowCap_ is true. Default: _1_  degree Celsius
* _StaminaPenalty_ controls how much stamina is drained per 10 minute tick.
* _HeatwaveWarning_ controls at what temp you give the heatwave warning temp at. Default _37_, recommended anything over _35_. (temps default to Celsius.)
* _ForceHeat_ and _ForceFrost_ are debug options set to false. ONLY ALTER THESE IF YOU KNOW WHAT YOU'RE DOING OR HAVE BEEN DIRECTED TO DO SO.
* _FogDuration_ is the setting for how many hours this lasts. This is specifically an integer. Default _2_
* _FogChance_ is the setting for the odds of fog per 10 minute check.

## Known Issues

* Weddings may cause heatwave and frost HUD messages to not display

## FAQ

Q. I don't want a storm penalty

A. Turn it off then.

Q. Why are you setting the mod to spam the console?

A. Turn off tooMuchInfo if you feel it spams the console.

Q. What happened to the extensive setting options in previous versions?

A. There were too many, honestly. I may set up something in the future to allow for a json parser

Q. I don't want the TV to display today's weather at all.

A. Set it to 600.

Q. I get an error on launch if I include the trailing zero on NoLongerDisplay!

A. Don't.

Q. Are there any easter eggs?

A. One setting may have more valid options than listed. 

Q. How do I turn off the stormy penalty drain?

A. Well, it's off by default, but if you somehow turn it on, turn it off again by setting StormyPenalty to false.

## Thanks

This wouldn't have been possible without the inspiration from the original More Rain Mod and assistance from Entoraox and Pathoschild

## License

GNU LGPLv3
