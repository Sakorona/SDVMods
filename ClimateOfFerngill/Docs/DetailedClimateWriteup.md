# Climate Writeup

This is a writeup of how it determines weather per day. This will cover default climates and at the end, how to write your own.

## Engine 

This mod works by normalizing the odds to ensure they always total 1 and then assigning space to them. 

For example, if the debris chance to day is .25, and the rain chance today is .15, then the chances are calculated to be:

>0 <= x < 15 Rain
>
>15 <= x < 40 Debris

And then, you can derive:

>40 <= x < 100 Sunny.

As such, the sunny odds are always derived rather than stated.

It is possible that certain weathers are not allowed in seasons, and the `FerngillClimate` object has flags for that.  (A detailed writeup of that is at the end.) An example might be a Extended Winter day, for example:

>0 <= x < 5 Rain
>
>5 <= x < 65 Snow
>
>65 <= x < 75 Debris

which derives
>75 <= x < 100 Sunny

but a normal Winter day would be
> 0 <= x < 65 Snow 
> 
> 65 <= x < 100 Sunny.

Bear in mind, however, that all default climates apply a variability to this, meaning that odds will often be less or more than assumed, and that the odds often will sum over or under 1. 


### Climate Formulae

In an effort to avoid having people deal with trigonometry, rather than the equation: `y = Asin(2pifx) + B`, this mod uses a partwise standard slope equation series of segments. In other words, you can define a rate of change based on the day of the month and an initial value for spans of the season. An example is:

![Image of Spring Rain Chances Normal or Enhanced Climate](SpringRainChancesNormalEnhanced.png)

This will result in: 

![Image of Spring Rain Chance Graph](SpringRainChanceGraph.png)

It is somewhat advised when creating a custom climate to think about what you want. It is possible to specify only one segment per season, and to use a flat rate as well. 


### Normal Climate

Loosely based off of Seattle, springs under this climate are initially wet but dry off rapidly. Storm chances are relatively flat but increase towards the end of the month, as the wind driven off the Gem Sea picks up before going slack again. 

#### Spring

Rain starts at 81% and rapidly drops to 35% on Day 10, where it drops slower (to 23% on Day 20) before remaining flat at those odds for the rest of the month. Storms vary between 15-30% chance all month, and it is extremely windy in the middle part of the season. This means that the best times for sun are in the end of the season. Temperatures start cold (~3C) but increase to ~26C by the end of the month. Lows can differ as much as 10C towards the end.

![Image of Spring Climate](SpringClimateChart.png)

##### Formulas - Day 1 to 9

* Rain: `f(x) = .85 - day*.04`
* Rain Variability: `±.04`
* Storm: `.25`
* Storm Variability: `±.1`
* Debris: `f(x) = .05 + day*.025`
* Debris Variability: `±.08`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = .02 + day*.015`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = .04 + day*.0125`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low)
* Fog Chance: `.55`
 
#### Formulas - Day 10 to 19

* Rain: `f(x) = .49 - day*.014`
* Rain Variability: `±.02`
* Storm: `.15`
* Storm Variability: `±.1`
* Debris: `f(x) = .275 + day*.0065`
* Debris Variability: `±.08`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = 2 + day*1.05`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = .04 + day*.0115`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low)
* Fog Chance: `.35`

#### Formulas - Day 20 to 28

* Rain: `f(x) = .24 - day*.0005`
* Rain Variability: `±.07`
* Storm: `f(x) = .10 + day*.0065`
* Storm Variability: `±.1`
* Debris: `f(x) = .3925 - day*.0075`
* Debris Variability: `±.14`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = .02 + day*.00805`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = .04 + day*.00875`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low)
* Fog Chance: `.35`

#### Summer

While very dry at the beginning of the season (~20%), it increases slowly until a sudden sharp increase to ~45% at the end of the month. Summers in Stardew Valley are known for being overwhelmingly sunny, with a very good chance of storms if it does rain in the middle of the month. Due to the very flat winds, summers are also rather humid and hot, with temperatures staying a minimum of 23C, but as warm as 32C or more in the middle of it. Lows hover at 5 to 8 degrees below the high.

![Image of Summer Climate](SummerClimateChart.png)

##### Formulas - Day 1 to 9

* Rain: `f(x) = .20 - day*.001`
* Rain Variability: `±.04`
* Storm: `.4`
* Storm Variability: `±.1`
* Debris: `0`
* Debris Variability: `0`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = 24 + day*.1`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = 28 + day*.82`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low)
* Fog Chance: `.001`
 
#### Formulas - Day 10 to 18

* Rain: `f(x) = 20 - day*.0001`
* Rain Variability: `±.02`
* Storm: `.8`
* Storm Variability: `±.1`
* Debris: `0`
* Debris Variability: `0`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = 24 - day*.01`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = 28 + day*.62`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low)
* Fog Chance: `.001`

#### Formulas - Day 19 to 28

* Rain: `f(x) = -12 + day*.018`
* Rain Variability: `±.07`
* Storm: `f(x) = .80 - day*.0065`
* Storm Variability: `±.1`
* Debris: `0`
* Debris Variability: `0`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = 27 - day*.25`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = 57 + day*1.25`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low)
* Fog Chance: `.001`

#### Fall

Rain odds stay flat with minor variances at 45% for the first half of fall before increasing to ~60% by the end of the season. Fall is mostly known for it's wind, and there's almost no day without wind or rain later in the season. The temperatures are cooling rapidly, sometimes with early frosts near the end of the season due to lows.

![Image of Fall Climate](FallNormalClimateChart.png)

##### Formulas - Day 1 to 9

* Rain: `f(x) = .20 - day*.001`
* Rain Variability: `±.04`
* Storm: `.4`
* Storm Variability: `±.1`
* Debris: `0`
* Debris Variability: `0`
* Snow: `0`
* Snow Variability: `0`
* Low Temperature: `f(x) = 24 + day*.1`
* Low Temperature Variability: `±3`
* High Temperature: `f(x) = 28 + day*.82`(clamp so that it's always higher than low)
* High Temperature Variability: `±4`(clamp so that it's always higher than low) 

#### Winter

Winters are often at -1 to -4 C, mediated somewhat by the Gem Sea. However, the jetstream often brings water, to the point where the valley can often receive several feet of snow over the season with snow odds being a high 85% throughout the entire winter. 

### Enhanced Climate

This differs only in that allows several weathers the base game does not. These changes occur only in Fall and Winter.

### Spring

Rain odds fall from 65% instead of 81%. 

### Fall

It often dips below 2C in the end of the season, and as such, there are odds of snow instead of rain. **NB:** If Hazardous Weather is enabled, this **WILL** kill crops. 

### Winter

The most changed season, this reintroduces wind (which looks like small snowflakes), decreases snow odds to add in more wind, and may have some rain at the end of the season, rather than snow. Also rare chances of thundersnow (something like 5%).

## Writing Your Own Climate

This section is largely a documentation section. 