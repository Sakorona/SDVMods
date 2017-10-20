using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using System;
using System.IO;

namespace ClimatesOfFerngillRebuild
{
    internal class Sprites
    {
        /// <summary>Sprites used to draw a letter.</summary>
        public static class Letter
        {
            /// <summary>The sprite sheet containing the letter sprites.</summary>
            public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\letterBG");

            /// <summary>The letter background (including edges and corners).</summary>
            public static readonly Rectangle Sprite = new Rectangle(0, 0, 320, 180);
        }

        /// <summary> Sprites used for drawing various weather stuff </summary>
        public class Icons
        {
            public Texture2D source;
            public static Texture2D source2;

            public Icons(IContentHelper helper)
            {
                source = helper.Load<Texture2D>("Assets\\climatesheet2.png");
                source2 = Game1.mouseCursors;
            }

            public Rectangle GetMoonSprite(MoonPhase moon)
            {
                if (moon == MoonPhase.FirstQuarter)
                    return Icons.FirstQuarter;
                if (moon == MoonPhase.FullMoon)
                    return Icons.FullMoon;
                if (moon == MoonPhase.NewMoon)
                    return Icons.NewMoon;
                if (moon == MoonPhase.ThirdQuarter)
                    return Icons.ThirdQuarter;
                if (moon == MoonPhase.WaningCrescent)
                    return Icons.WaningCrescent;
                if (moon == MoonPhase.WaningGibbeous)
                    return Icons.WaningGibbeous;
                if (moon == MoonPhase.WaxingCrescent)
                    return Icons.WaxingCrescent;
                if (moon == MoonPhase.WaxingGibbeous)
                    return Icons.WaxingGibbeous;

                return Icons.NewMoon;
            }

            public Rectangle GetWeatherSprite(int weather)
            {
                if (weather == Game1.weather_debris)
                    return Icons.WeatherWindy;
                if (weather == Game1.weather_festival)
                    return Icons.WeatherFestival;
                if (weather == Game1.weather_sunny)
                    return Icons.WeatherSunny;
                if (weather == Game1.weather_wedding)
                    return Icons.WeatherWedding;
                if (weather == Game1.weather_snow)
                    return Icons.WeatherSnowy;
                if (weather == Game1.weather_rain)
                    return Icons.WeatherRainy;

                return Icons.WeatherSunny;
            }

            // These are the positions of each sprite on the sheet.
            public static readonly Rectangle NewMoon = new Rectangle(24, 120, 37, 35);
            public static readonly Rectangle WaxingCrescent = new Rectangle(60, 120, 37, 34);
            public static readonly Rectangle FirstQuarter = new Rectangle(174, 117, 38, 36);
            public static readonly Rectangle WaxingGibbeous = new Rectangle(254, 118, 36, 33);

            public static readonly Rectangle FullMoon = new Rectangle(25, 156, 37, 33);
            public static readonly Rectangle WaningCrescent = new Rectangle(255, 156, 36, 33);
            public static readonly Rectangle ThirdQuarter = new Rectangle(174, 157, 37, 30);
            public static readonly Rectangle WaningGibbeous = new Rectangle(99, 153, 37, 35);

            public static readonly Rectangle WeatherSunny = new Rectangle(24, 196, 40, 37);
            public static readonly Rectangle WeatherRainy = new Rectangle(63, 197, 35, 36);
            public static readonly Rectangle WeatherStormy = new Rectangle(96, 196, 41, 42);
            public static readonly Rectangle WeatherSnowy = new Rectangle(137, 196, 39, 35);
            public static readonly Rectangle WeatherWindy = new Rectangle(180, 198, 35, 31);
            public static readonly Rectangle WeatherWedding = new Rectangle(221, 196, 37, 35);
            public static readonly Rectangle WeatherFestival = new Rectangle(260, 198, 42, 38);

            /// <summary>A down arrow for scrolling content.</summary>
            public static readonly Rectangle DownArrow = new Rectangle(12, 76, 40, 44);

            /// <summary>An up arrow for scrolling content.</summary>
            public static readonly Rectangle UpArrow = new Rectangle(76, 72, 40, 44);
            
        }
    }
}
