#Climates of Ferngill

## Basics

This mod does the following:

- Adds a moon to give it events on certain phases (full and new)
- Adds overworld fog
- Expands the TV dialogue
- Adds a weather menu popup
- Expands weathers to add blizzards and thundersnow in winter
- Adds hazardous weather to summers and winters
- Adds a temperature gauge to give a detailed sense of realism
- Makes storms, blizzards and heatwaves hazardous by draining stamina while outside

## Overview of Functionality

This mod works the same way most SMAPI mods do, by subscribing to events, mainly `UpdateTick`, `OnPostRenderEvent`, `AfterLoad`, `AfterDayStarted`, `AfterReturnToTitle`, `BeforeSave`, and `TimeOfDayChanged`.

At base, it needs the `AfterDayStarted` event to run, but all of the other events handle various functionalities. 


 