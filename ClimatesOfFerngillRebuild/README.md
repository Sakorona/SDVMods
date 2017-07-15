# Climates of Ferngill (Rebuild) You Can (Not) Have Weather

Current Version (5 June 2017): v0.1 alpha

## Overview

This mod does the following:

- Alters the weather via a custom method that can read custom files

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