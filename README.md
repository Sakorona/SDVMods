# Climate of Ferngill

This mod alters the weather of Stardew Valley by creating a climate system. It adds temperature and may add hostile weather events down the line.  

The current version is __0.8.6 rev 20170203__ 

## Changelog

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

## Planned Features
* More hostile weather options
* Moving TV text options to a json/txt file for easier customization.

## Maybe features?
* Specifying climate options via json file
* Making all strings able to be translated

## Requirements
* Stardew Valley: __1.11__ or greater
* SMAPI: __1.7__ or greater

## Configuration Options

* _SuppressLog_ can be set to __false__ or __true__  If true, the game will not log any events to the console. Default: __false__
* _AllowSnowOnFall28_ is a feature that will allow the possiblity of snow on the 28th of Fall. (Currently, 100%.) It can be set to __true__ or __false__ Default: __true__
* _AllowStormsFirstSpring_ allows storms the first spring. In the base game this is disabled. It can be set to __false__ or __true__.  Default: __false__
* _HarshWeather_ allows the hazards. It can be set to __false__ or __true__. Default: __false__        
* _NoLongerDisplayToday_ is a setting that says after this point: don't display today's weather. The valid setting is: 0600 - 2600. Default: __1700__.
* _ClimateType_ is a setting that lets you control the type of climate. The options are: __arid__, __dry__, __normal__, __wet__, __monsoon__ and _traditional_. Default: __normal__
* _TempGauge_ is a setting that controls if temps are displayed in Celsius or Farenheit. The options are _celsius, fahrenheit_, default: _celsius_
* _DisplaySecondScale_ is a setting that controls if two gauges are displayed at once. Valid options are __false__ or __true__, default: __false__.
* _SecondScaleGauge_ is a setting that controls what the second gauge is. Only used if DisplaySecondScale is true. Options are _celsius, fahrenheit_.
* _StormyPenalty_ is a setting that enables the stamina drain for being outside in a storm. The valid options are __true__ and __false.__. Default: __true__
* _StaminaPenalty_ controls how much stamina is drained per 10 minute tick.
* _HeatwaveWarning_ controls at what temp you give the heatwave warning temp at. Default _37_, recommended anything over _35_. (temps default to Celsius.)

## FAQ

Q. I don't want a storm penalty

A. Turn it off then.

Q. Why are you setting the mod to spam the console?

A. I am currently targetting .9 to change the default behavior of this option, or if I feel I have sufficently tested .8

Q. What happened to the extensive setting options in previous versions?

A. There were too many, honestly. I may set up something in the future to allow for a json parser

Q. I don't want the TV to display today's weather at all.

A. Set it to 0600.

Q. Are there any easter eggs?

A. One setting may have more valid options than listed. 

Q. How do I turn off the stormy penalty drain?

A. Well, it's off by default, but if you somehow turn it on, turn it off again by setting StormyPenalty to false.

## Thanks

This wouldn't have been possible without the inspiration from the original More Rain Mod and assistance from Entoraox and Pathoschild

## License

GNU LGPLv3
