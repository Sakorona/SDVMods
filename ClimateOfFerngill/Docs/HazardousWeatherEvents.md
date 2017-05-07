# Hazardous Weather Events

The mod has several right now: Frost, Blizzard and Heatwaves. All of these events require `HarshWeather` in the config to be set to true.


## Frost

The mod checks against the low temp, and then kills them at night. In order to ensure that you don't have a full die off (you can edit this to create a full die off), the variable `FrostHardiness` in the configuration controls if things die. It's set to .45, which means 45% of all crops die. To make more crops die, increase it, to make less crops dies, decrease it.

An important note is that spring freezes (which can occur in the early Spring days) work on stage of the crop, rather than the crop itself.

***NB:*** Remember that frosts are disabled Year 1 Spring unless directly opted into. This is to stop you from being put in an unrecoverable position early game. This is controlled by `DangerousFrost`

They are killed under the following tiers

### Light Frost

This occurs at 1.66 C/35 F. The crops killed here are:   
* Corn  
* Wheat 
* Amaranth
* Sunflower
* Pumpkin
* Eggplant
* Yam

### Heavy Frost

This occurs at 0 C/32 F. The crops killed here are any above, and:   
* Artichoke
* Bok Choy

### Light Freeze

This occurs at -.5C/31 F. The crops killed here are any above, and:   
* Grape

### Freeze

This occurs at -2.22C/28 F. The crops killed here are any above, and:   

* Fairy Rose
* Beet
        
### Heavy Freeze

This occurs at -3.33C/26 F. The crops killed here are any above, and:        

* Cranberry
* Ancient
* SweetGemBerry    
 
## Heatwave

Heatwaves occur at temps above a value (configured as `HeatwaveWarning` (default 37C or 98.6F). If you do not have `AllowCropHeatDeath` set to true, this will only dewater the crops. During a heatwave, stamina is drained if the player is outside for over 65% of the ticks. The penalty is 1.5* the configured storm penalty configured to be the rounded up value.

If you have `AllowCropHeatDeath` set to true, you will need to water the plants within 3 hours (180 minutes) or the crops die. The threshold for that is controlled by `DeathTemp`, default set to 41C/101.5F

The stamina penalty is configured to stop at twilight.

## Blizzard

Blizzards are considered similar to snow storms, and will occur at [TODO CHANCE]. They only occur in Winter, no matter what snow is configured, and will carry the same stamina penalty as heatwaves with the same odds of activation.

## Storms

During any stormy day, every 10 minutes, a stamina penalty, configured by `StaminaPenalty` is hit against the player. Drinking a Muscle Tonic will remove the cold. By default, you can only get a cold once a day, but you can turn that safety off by setting `OnlyOneColdADay` to false.