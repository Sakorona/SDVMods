## Config Options

To configure this mod, edit the config.json in the mod folder. Bear in mind, you will need to run the game at least once with the mod installed.

### BadMoonRising

This controls the chance of a blood moon. Used only on a full moon. 

This is a double, with valid numbers between 0 and 1, with the default being `.004` 

This is used as a percent. (`.004` is .4% for example.)

**Remember:**  This isn't the only thing controlling blood moon apperance. `HazardousMoonEvents` must also be toggled to true.

### EclipseOn

This enables the Solar Eclipse event. Defaults to `true`. Turning this to `false` will disable the event.

Note, even when true, it won't start until at least Spring 2 and it must be a new moon.

### EclipseChance

This controls the frequency of the Solar Eclipse chance. Defaults to `.015` (1.5%) wit h valid ranges being between 0 to 1. This means, on average, the default is that you have a 12% chance per year of a moon.  (This, incidentally means, a `.125` value should ensure a good chance of at least one eclipse a year)

### SpawnMonsters

Controls if monsters spawn during eclipses on a wilderness farm. Defaults to `true`. Note that this also requires that they would normally spawn in game, so if you have turned that off via the Witch's House, this mod will also not spawn them.  Set this to false if you don't want them to spawn during the eclipse.

(Valid: `true` and `false`)

### HazardousMoonEvents

This controls the moon events that can hinder the player, or spawn monsters. No monsters will spawn ( the blood moon won't even happen) while this is false. This also stops the crop deadvancement on the new moon.

This defaults to `false`, to allow opt-in.

(Valid: `true` and `false`)

### Verbose

This controls if the mod is verbose to the content.

This defaults to `false`, to allow opt-in.

(Valid: `true` and `false`)

### ShowMoonPhase

This controls if you see HUD message popups (the popups at the bottom of the screen) aboutthe moon rising or setting. Turn this to `false` if you want to stop the messages.

(Valid: `true` and `false`)

### CropGrowthChance

This controls the chance for a crop to grow an extra day on a full moon. Defaults to `.015` (1.5%). You can set this to higher, but as a warning to the wise: this happens twice a season, across every crop, the higher chances will effectively mean you are guarenteed it to happen rather than a rare chance on default. (Valid range: `0` to `1`)

If you don't want this to trigger at all, set it to `0`.

### CropHaltChance

This controls the chance for a crop to not grow that day on a new moon. Defaults to `.015` (1.5%). You can set this to higher, but as a warning to the wise: this happens twice a season, across every crop, the higher chances will effectively mean you are guarenteed it to happen rather than a rare chance on default. (Valid range: `0` to `1`)

If you don't want this to trigger at all, set it to `0`.

### BeachRemovalChance

 The chance a item is removed from the beach. Default: `.09` (9%) 

Valid range: `0` to `1`

For more information, please see the advanced writeup.

### BeachSpawnChance

Spawn chance for up to 5 items. Default: `.35` (35%) per item placement attempt.

Valid range: `0` to `1`

For more information, please see the advanced writeup.

### GhostSpawnChance

If moon events that hinder the player are turned on, this is the chance a ghost is spawned every 10 minutes on a combat farm while the player is on the farm.    Increased odds make this far more likely.

Default: `.02` (2%). Valid range: `0` to `1`
