# Weather Overview

## Spring:

Weather through Spring tends to be dry. As is drying, temps hover between 11 C and warm up to 26 C. Lows hover between 4-16 C. Some rain should occur during the first third, but it should dry out. It's windy throughout, that said, storms are uncommon.

### Details:

* In the first nine days, temperatures are rising from 8-11C to ~16C. Lows should stay between 3-6C of this temp, and rain chance should be decreasing from 55% to 30%. Storm conversions are ~15%. Windy flat 25% throughout.
* In the second nine days, ~14C to ~20C highs, lows same variance. Rain chances decreasing from 30% to 20%.  Storm conversions are 20%. Windy dropping from 25% to 15%.
* In the third nine days, ~20C to ~26C highs, rain chances hovering at 20%. Storm conversions are 30%, due to the ramp up to Summer.

### Variants:
* Arid - Rain chances at a flat 25% all month. Temperatures are +5C throught out the month.
* Dry - Rain chances are a flat 30% all month. Temps are the same.
* Wet - Increase rain chances by 5%
* Monsoon - Rain chances are a flat 95%. Monsoon season is Winter-Spring. 

## Summer: 

Dry. Hot. 26 C to 36 C to 22 C at the end, with 36-39C not unreachable during the hottest bit. (Heatwaves should make that top out at 41C). Rain should be scarse but ramping up towards the end slowly. Storm conversions should be likely.

### Details:
* In summer, you can have +6C randomly - so an average of .5 to 6 should be added to the day.

* In the first nine days, temperatures are rising from ~26C to 31C. Lows the same spread as spring. Rain chances hover at 15%. Storm conversions are 45%
* In the second nine days, temperature are rising from 31C to 36C. Rain chances the same, but storm conversions are now 60%.
* In the third nine days, temperature dropping from 36C to 22C. Cooling rapidly. Rain chances now at 30%, storm conversions are 45%.

### Variants:
* Arid - In Arid climates, the summer is dry. Rain 5%. Temperatures are +6C throught out the month. (this means Arid climates will peak at 47C.)
* Dry - Rain chances are 20, 10, 15%. Temperatures are the same. 
* Wet - Increase rain chances by 5%
* Monsoon - Rain odds are -10%, thunderstorm conversions 80%, temps are +6C. 

## Autumn:

Wet. Cooling. Windy, but it should essentially be windy or rainy. Highs start at 22 C but drop to 8C. Lows are 11C to 0C. If it snows, the temp should cap at 2C with low -1 C.

Storm conversions are still likely.

### Details:
* In autumn, have storm conversions flat 33%.

* In the first nine days, temperatures are falling from 22C to 16C. Lows can have a larger variance, so we should use rand(4,10). Rain chances are increasing from 30% to 40%. Windy 0% rising to 40%.
* In the second nine days, windy - 40% rising to 50%. Rainy the rest of the time. Temps falling from 16C to 9C. Lows should strictly stay above 2C. 
* In the third nine days, wind falling from 50% to 10%. Rain steady at 50%. Temps now at 9C to 6C. Lows capping at 1C. If snow is enabled on Fall 28, set the temp to 2C, low -1C.

### Variants:

* Arid - Dry fall. Rain 5%. Wind 30%. Temps are +2C.
* Dry - Rain chances at 25%, 30%, 30%. Wind 30%. No temp changes.
* Wet - Increase rain chances by 5%
* Monsoon - Rain chances should increase from 10% to 90%, wind decreasing in proportion. No temp changes.

## Winter: 

Wet. Very wet. Cold. High capped at 6C, and minimally -1C, with lows no more than 6C under that. Should dry out a bit towards the end of Winter.

### Details:
* It cannot storm, rain or wind in winter. As such, we only have to worry about sunny and snowy!

* In the first nine days, temperatures drop from 6C to -2C high. Snow chance is 60%.

* In the second nine days, temperatures drop from -2C to -12C. Snow chance is 75%. 

* In the third nine days, temperatures rise from -12C to -1C high. Snow chance is 60%.


### Variants:

* Arid - Snow chance is 25%. No temp changes.
* Dry - Rain chance is 30%. No temp changes
* Wet - Increase rain chances by 5%
* Monsoon - Snow chance is 100%. No temp changes.