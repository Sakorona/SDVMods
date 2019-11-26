(Contains some spoilers for new surprises added in the most recent release.)

## Eclipse

On a new moon, as long as eclipses are turned on in the config file, and the rolled number is under the chance, it'll flip to 'dark all day'. On combat farms, monsters will spawn all day.

## Blood Moon Mechanics

Blood moon can only happen right now on a full moon, and if the odds are less than the configured odds in the configuration. Chances occur at `0930 (9:30 AM)`, `0950 (9:50 AM)`, `1010 (10:10 AM)` on a full moon day.

Also, it'll only happen if `HazardousMoonEvents` is toggled on in the config. 

For sanity sake, it won't happen on festivals or wedding days. During a blood moon, the following effects happen:

- The worth of all goods sold is down 25%
- The worth of all goods bought is up 85%
- Monsters spawn every 10 real-world seconds as long as your outside.
- Water color changes.
- UPCOMING: Talking to villagers inflicts a -40 friendship penalty for the day.

If Climates of Ferngill is also installed, the fog, or snow, or rain will be blood red during this time.

### Monster Spawn Chances

* If the player has a combat level of at least 10, and has reached at least the skull cavern level of 25, and with a chance of .1%, and **if a Pepper Rex has been killed at least once before**, a Pepper Rex monster is spawned
* Else, if the player has a combat level of at least 10, and has reached at least the skull cavern level of 25, and with a chance of 5%, a Metal Head monster is spawned
* Else, if the player has a combat level of at least 10, and with a chance of 25%, a Skeleton monster is spawned
* Else, if the player has a combat level of at least 8, and with a chance of 15%, a ShadowBrute monster is spawned
* Else, with a chance of 65%, spawn a RockGolem scaled up to the players combat level

Finally, if we have spawned none of the above:
Spawn a green slime, set to mine level 1 if the combat level is under 4, mine level 100, if it is under 8, and 140 if it is under 10, and 200 if it is over 10. 

## Moon Cycle

The moon orbits on a 14 day cycle. The first day of the game is a Waxing Crescent. What this means is that cycle goes this way:

| Day 1           | Day 2          | Day 3         | Day 4         | Day 5          | Day 6          | Day 7     |
| --------------- | -------------- | ------------- | ------------- | -------------- | -------------- | --------- |
| Waxing Crescent | Waxing Cresent | Waxing Cresen | First Quarter | Waxing Gibbous | Waxing Gibbous | Full Moon |

| Day 8          | Day 9          | Day 10        | Day 11          | Day 12          | Day 13          | Day 14   |
| -------------- | -------------- | ------------- | --------------- | --------------- | --------------- | -------- |
| Waning Gibbous | Waning Gibbous | Third Quarter | Waning Crescent | Waning Crescent | Waning Crescent | New Moon |

| Day 15          | Day 16         | Day 17        | Day 18        | Day 19         | Day 20         | Day 21    |
| --------------- | -------------- | ------------- | ------------- | -------------- | -------------- | --------- |
| Waxing Crescent | Waxing Cresent | Waxing Cresen | First Quarter | Waxing Gibbous | Waxing Gibbous | Full Moon |

| Day 22         | Day 23         | Day 24        | Day 25          | Day 26          | Day 27          | Day 28   |
| -------------- | -------------- | ------------- | --------------- | --------------- | --------------- | -------- |
| Waning Gibbous | Waning Gibbous | Third Quarter | Waning Crescent | Waning Crescent | Waning Crescent | New Moon |

This means that new moon events will occur on the 14th and 28th of each month, and full moon events on the 7th and 21st of every month.

The moon rises and sets at certain times. The table contains normalized times to a 24-hour cycle. These times are based off of the Earth's moon.

| Moon Phase      | Moon Rise       | Moon Set        |
| --------------- | --------------- | --------------- |
| New Moon        | 0640 (6:40 AM)  | 1800 (6:00 PM)  |
| Waxing Crescent | 1040 (10:40 AM) | 2100 (9:00 PM)  |
| First Quarter   | 1330 (1:30 PM)  | 0030 (12:30 AM) |
| Waxing Gibbous  | 1510 (3:10 PM)  | 0320 (3:20 am)  |
| Full Moon       | 1650 (4:50 PM)  | 0620 (6:20 AM)  |
| Waning Gibbous  | 2000 (8:00 PM)  | 1030 (10:30 AM) |
| Third Quarter   | 2320 (11:20 PM) | 1300 (1:00 PM)  |
| Waning Crescent | 0230 (2:30 AM)  | 1540 (3:40 PM)  |

### Full Moon Events

These events will occur any day there is a full moon that day.

At the beginning of the day, it will try to add up to 5 items. The default item list is:

- `393` - Coral

- `394` - Rainbow Shell

- `560` - Ocean Stone

- `586` - Nautilus Fossil (the artifact)

- `587` -  Ampibian Fossil

- `589` - Trilobite

- `397` - Sea Urchin

Once it's done that, you then have a .01% chance to select `392` (Nautlius Shell), and then a 20% chance to select`60` (Emerald). 

Once this is done, there is a chance set by `BeachSpawnChance` (default: 35%)) to spawn the item as long as the random tile chosen is clear and placeable.

Crops have a random chance, set by `CropGrowthChance` (Default: 1.5%) to advance an extra day at the end of the day.

Due to limitations, ghosts may spawn all over the farm as long as it is a combat farm every 10 minutes. The odds of this are controlled by `GhostSpawnChance` (default: 2%) and requires `HazardousMoonEvents` to be true. 

### New Moon Events

These events will occur any day there is a full moon that day.

At the beginning of the day, the various items on the beach may be removed as long as `HazardousMoonEvents` is true. The chance each item being removed is controlled by `BeachRemovalChance` (default: 9%).     

Crops have a random chance, set by `CropHaltChance`  (Default:1.5%) to not advance at the end of the day
