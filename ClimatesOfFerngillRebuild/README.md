# Climates of Ferngill (Rebuild) You Can (Not) Have Weather

Current Version (26 July 2017): v1.0 alpha


## What's New

- Fog!
- Blizzards, Thundersnow

## Overview

This mod does the following:

- Alters the weather via a custom method that can read custom files
- Adds in several custom weathers - Thundersnow! Blizzards! Dry Lightning!
- Changes the rain totem to occasionally spawm storms as well.
- Adds a moon overhead, which will act on the world
- Adds a weather menu option, which will display information about the weather
- Changes the text for the TV weather channel

##Known Issues
- When foggy, swinging a tool or blade will cause the fog to blink.

##To Do
- Stamina drain for storms and blizzards not implemented
- Frost and Heatwaves are not yet implemented

## Wishlist
- Readd an item to remove the stamina drain. (Pending on SMAPI 2.0+)

##Requirements

- Stardew Valley: 1.2.30+
- SMAPI: 1.15.1+
- Platonymous' Custom Element Handler: v1.3.0+ [http://www.nexusmods.com/stardewvalley/mods/1068/?]
- Platonymous' Custom TV: 1.0.5+ [http://www.nexusmods.com/stardewvalley/mods/1139/?]

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

- `SnowOnFall28` - If set to true, this will force snow fall and appropriate temperatures on Fall 28. Default: `false`,
  Valid: `true` or `false`

- `StormTotemChange` - Usage of the rain totem will now spawn storms with the same odds as spawning storms on 
   rainy days. Default: `true`. Valid: `true` or `false`.

- `Verbose` - This makes the mod noisy to provide a lot of debug options. Default: `false`. Valid: `true` or `false`.

- `AllowStormsSpringYear1` - Default: `false`. Normally, you can't get storms in Spring Y1. This keeps that valid. Valid: `true` or 
 `false`

- `DisplayBothScales` - Default: `false`. This will display both known scales. Set to `true`, if you want to see Farenheit as well.

- `HazardousWeather` - Default: `false`. This turns on hazardous weather. It's normally turned off. Right now, it only turns on the heatwave and frost events

- `AllowCropDeath` - Default: `false`. Normally, hazardous weather won't kill crops, just stop them growing. This reenables crop death.'