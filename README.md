# Climate of Ferngill

This mod alters the weather of Stardew Valley by creating a climate system. It adds temperature and may add hostile weather events down the line.  

The current version is __0.7.26 rev 20170105 (beta)__ 

## Features
* Custom TV dialogue (English only)
* A more complex weather system that can be generally configured to match 5 different climates

## Planned Features
* Hostile weather events (Heatwaves, Floods, Frosts)
* Creating a 'traditional' option for the weather
* Moving TV text options to a json/txt file for easier customization.
* Adding in Farenheit as a temp option.

## Maybe features?
* Specifying climate options via json file
* Making all strings able to be translated

## Requirements
* Stardew Valley: __1.11__ or greater
* SMAPI: __1.2__ or greater

## Configuration Options

* _SuppressLog_ can be set to __false__ or __true__  If true, the game will not log any events to the console. Default: __false__
* _AllowSnowOnFall28_ is a feature that will allow the possiblity of snow on the 28th of Fall. (Currently, 100%.) It can be set to __true__ or __false__ Default: __true__
* _AllowStormsFirstSpring_ allows storms the first spring. In the base game this is disabled. It can be set to __false__ or __true__.  Default: __false__
* _HarshWeather_ allows the hazards. It can be set to __false__ or __true__. Default: __false__        
* _NoLongerDisplayToday_ is a setting that says after this point: don't display today's weather. The valid setting is: 0600 - 2600. Default: __1700__.
* _ClimateType_ is a setting that lets you control the type of climate. The options are: __arid__, __dry__, __normal__, __wet__, __monsoon__. Default: __normal__

## FAQ

Q. Why are you setting the mod to spam the console?

A. I am currently targetting .9 to change the default behavior of this option, or if I feel I have sufficently tested .8

Q. What happened to the extensive setting options in previous versions?

A. There were too many, honestly. I may set up something in the future to allow for a json parser

Q. I don't want the TV to display today's weather at all.

A. Set it to 0600.

Q. How do I get it to display today's temperature in Farenheit?

A. Upcoming. Probably soon. 

## Thanks

This wouldn't have been possible without the inspiration from the original More Rain Mod and assistance from Entoraox and Pathoschild

## License

GNU LGPLv3
