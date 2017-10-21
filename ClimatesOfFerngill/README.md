# Climates of Ferngill (Rebuild) You Can (Not) Have Weather

Current Version (20 October 2017): v1.1.12p3 beta

## What's New

- Fog!
- Blizzards, Thundersnow
- A more customizable weather system
 
## Overview

This mod does the following:

- Alters the weather via a custom method that can read custom files
- Adds in several custom weathers - Thundersnow! Blizzards! Dry Lightning!
- Changes the rain totem to occasionally spawm storms as well.
- Adds a moon overhead, which will act on the world
- Adds a weather menu option, which will display information about the weather
- Changes the text for the TV weather channel
- Going out in storms, blizzards, frosts and heatwaves is now more perilous, as it drains your stamina. Thankfully, 
    a 'Muscle Remedy' has been found to cure even the hardiest flu

## Known Issues
- None at present

## To Do
- Maybe add icons for some of the new weathers?

## Wishlist

## Changelog
v1.1.12p3 
- Text tweaks to make it flow properly
- The TV and popup will have some lines about fog now
- The popup will scroll.
- The probability of dark fog will be lowered to 8.75% and configurable in the settings option. It will also now default to having day 1 not having dark fog (probably hard coded to prevent option bloat)
- Cleaned up some of the code, removed some debug spam
- Rain Totems override chance isn't just the first one now, although that means if you get it to set a Storm totem, the next use might override it..
- fixed issue where festival name would never appear in the popup, and the wrong text was called for the TV.
- fixed issue where summer drylightning tried to call for thundersnow (!)

v1.1.12p2
- removed fog testing code.

v1.1.12p1
- replaced the totem detection code to make it a bit more tolerant of fault

v1.1.12 beta
- fixed an issue with festivals
- added stamina drains back in
- heatwaves and frosts are back in, and now it triggers during certain times.

##Requirements

- Stardew Valley: 1.2.33+
- SMAPI: 1.15.4+ 

## Config Options

- `ClimateType` - set to weather that has a corresponding file in `data\weather\`. Packaged with the mod is
`normal`, `extended`, `arid`, `dry`, `wet`, `monsoon`. Default: `normal`.

- `ThundersnowOdds` - This controls the odds of thundersnow. (Custom weather available during the snow.) Valid: 0-1, but it's 
recommended that this is kept low. Default: `.001` (.1%)

- `BlizzardOdds` - This controls the odds of blizzards (Custom weather available during the snow.). Valid 0-1, but it's 
recommended that this is kept low. Default: `.08` (8%)

- `DryLightning` - This controls the odds of dry lightning (Custom weather available during any clear day, as long as the temperature is 
over a certain value.). Valid 0-1, but it's recommended that this is kept low. Default: `.1` (10%)

- `DryLightningMinTemp` - This controls the minimum temperature to trigger the DryLightning event. 
  Defaults to `34`. Values are in Celsius. (34 C is 93.2 F)

- 'TooColdOutside' - This controls the temperature required (the *low* temperature required) to trigger the Frost event. Note this is a Spring and Fall event, and will potentially kill crops
  Defaults to '-3'. Values are in Celsius (1 C is 33.8 F). 
  NOTE: Frosts trigger at dark

- 'TooHotOutside' - This controls the temperature required (the *high* temperature required) to trigger the Heatwave event.
  Defaults to '39'. Values are in Celsius (39 C is 102.2 F)
  NOTE : Heatwaves taper off at night.

- `SnowOnFall28` - If set to true, this will force snow fall and appropriate temperatures on Fall 28. Default: `false`,
  Valid: `true` or `false`

- `StormTotemChange` - Usage of the rain totem will now spawn storms with the same odds as spawning storms on 
   rainy days. Default: `true`. Valid: `true` or `false`.

- `Verbose` - This makes the mod noisy to provide a lot of debug options. Default: `false`. Valid: `true` or `false`.

- `AllowStormsSpringYear1` - Default: `false`. Normally, you can't get storms in Spring Y1. This keeps that valid. Valid: `true` or 
 `false`

- `DisplayBothScales` - Default: `false`. This will display both known scales. Set to `true`, if you want to see Farenheit as well.

- `HazardousWeather` - Default: `false`. This turns on hazardous weather. It's normally turned off. Right now, it only turns on the heatwave and frost events
	IMPORTANT NOTE: This only enables the stamina drain on them, and the dewatering of the heatwave. Frost's crop death will remain disabled, 
	as well not watering the plants in time for a heatwave.

- `AllowCropDeath` - Default: `false`. Normally, hazardous weather won't kill crops, just stop them growing. This reenables crop death.'

- 'AffectedOutside' - The percentage outside you need to be within a 10 minute span to be affected by stamina events.
 Defaults to '.65', valid values are between 0 and 1. To turn stamina drains off entirely, set it to 0. 

 - 'SickMoreThanOnce' - By default, the false means you can only get sick once a day. Set it to true to be affected by multiple colds.

 - 'Tier1Drain' - A tier 1 cold drains this much stamina per ten minutes. Default: '2'

 - 'Tier2Drain' - A tier 2 cold drains this much stamina per ten minutes. Default: '4'

 - 'DeadCropPercentage' - The amount of crops that a heatwave and frost can kill. (Note: Frost will kill more than heatwaves). Default: '.1' Valid range is 0 to 1.

 - 'CropResistance' - This represents the resistance an averagecrop has to heatwaves. Frosts have half this resistance. Default: '.4' Valid Range is 0 to 1.

 - 'DarkFogChance' - This controls the chance of the darker fog appearing. Default is set to '.0875' (or a 1/8th chance if it's foggy it'll be dark fog.) Valid Range is 0 to 1.