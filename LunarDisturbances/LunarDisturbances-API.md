# API Documentation

This is the API signature for Lunar Disturbances

```cs
    public interface ILunarDisturbancesAPI
    {
        string GetCurrentMoonPhase();
        bool IsSolarEclipse();
        int GetMoonRise();
        int GetMoonSet();
        bool IsMoonUp(int time);
    }
```

## Functions

#### GetCurrentMoonPhase

No Parameters

Returns a `string` of the current phase. This is the description (so localization friendly) one.

#### IsSolarEclipse

No Parameters

Returns a bool describing the solar eclipse status (So if you need to do compatiblity or want to run other events, etc.)

#### GetMoonRise

No Parameters

Will return the time the moon rises for the current in-game day.

#### GetMoonSet

No Parameters

Will return the time the moon sets for the current in-game day.

#### IsMoonUp

int time - This should be the same format as the game uses internally (e.g `930`, `1610` or `2620`)

This will determine if the moon is up in the time zone
