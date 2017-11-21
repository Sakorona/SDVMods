using EnumsNET;
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
                source = helper.Load<Texture2D>(Path.Combine("Assets","climatesheet4.png"));
                source2 = Game1.mouseCursors;
            }

            public Rectangle GetNightMoonSprite(MoonPhase currPhase)
            {
                switch (currPhase)
                {
                    case MoonPhase.BloodMoon:
                        return Icons.BloodMoon;
                    case MoonPhase.NewMoon:
                        return Icons.NewMoon;
                    case MoonPhase.WaxingCrescent:
                        return Icons.WaxingCrescent2;
                    case MoonPhase.FirstQuarter:
                        return Icons.FirstQuarter;
                    case MoonPhase.WaxingGibbeous:
                        return Icons.WaxingGibbeous;
                    case MoonPhase.FullMoon:
                        return Icons.FullMoon;
                    case MoonPhase.WaningGibbeous:
                        return Icons.WaningGibbeous;
                    case MoonPhase.ThirdQuarter:
                        return Icons.ThirdQuarter;
                    case MoonPhase.WaningCrescent:
                        return Icons.WaningCrescent2;
                }

                return Icons.NewMoon;
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
                    return Icons.WaningCrescent1;
                if (moon == MoonPhase.WaxingCrescent)
                    return Icons.WaxingCrescent1;

                /*
                 *  if (moon == MoonPhase.WaningGibbeous)
                    return Icons.WaningGibbeous;
                    
                if (moon == MoonPhase.WaxingGibbeous)
                    return Icons.WaxingGibbeous; */

                return Icons.NewMoon;
            }

            public Rectangle GetWeatherSprite(CurrentWeather condition)
            {
                if (condition.HasFlag(CurrentWeather.Blizzard))
                    return Icons.WeatherBlizzard;

                if (condition.HasFlag(CurrentWeather.Wind))
                    return Icons.WeatherWindy;

                if (condition.HasFlag(CurrentWeather.Festival))
                    return Icons.WeatherFestival;

                if (condition.HasFlag(CurrentWeather.Sunny) && !condition.HasFlag(CurrentWeather.Lightning))
                    return Icons.WeatherSunny;

                if (condition.HasFlag(CurrentWeather.Sunny) && condition.HasFlag(CurrentWeather.Lightning))
                    return Icons.WeatherDryLightning;

                if (condition.HasFlag(CurrentWeather.Wedding))
                    return Icons.WeatherWedding;

                if (condition.HasFlag(CurrentWeather.Snow) && !condition.HasFlag(CurrentWeather.Lightning))
                    return Icons.WeatherSnowy;

                if (condition.HasFlag(CurrentWeather.Snow) && condition.HasFlag(CurrentWeather.Lightning))
                    return Icons.WeatherThundersnow;

                if (condition.HasFlag(CurrentWeather.Rain))
                    return Icons.WeatherRainy;

                return Icons.WeatherSunny;
            }

            // These are the positions of each sprite on the sheet.
            public static readonly Rectangle NewMoon = new Rectangle(0, 16, 41, 41);
            public static readonly Rectangle WaxingCrescent1 = new Rectangle(42, 20, 32, 29);
            public static readonly Rectangle WaxingCrescent2 = new Rectangle(77, 19, 32, 21);
            public static readonly Rectangle WaxingCrescent3 = new Rectangle(109, 18, 32, 31);
            public static readonly Rectangle FirstQuarter = new Rectangle(143, 17, 34, 34);
            public static readonly Rectangle FullMoon = new Rectangle(217, 13, 36, 38);
            public static readonly Rectangle ThirdQuarter = new Rectangle(174, 157, 37, 30);
            public static readonly Rectangle WaningCrescent1 = new Rectangle(287, 12, 34, 36);
            public static readonly Rectangle WaningCrescent2 = new Rectangle(322, 12, 32, 37);
            public static readonly Rectangle WaningCrescent3 = new Rectangle(356, 14, 29, 32);
            public static readonly Rectangle WaxingGibbeous = new Rectangle(5, 60, 36, 40);
            public static readonly Rectangle WaningGibbeous = new Rectangle(40, 58, 40, 36);
            public static readonly Rectangle BloodMoon = new Rectangle(385, 9, 38, 38);

            //Weather
            public static readonly Rectangle WeatherSunny = new Rectangle(1, 104, 39, 38);
            public static readonly Rectangle WeatherRainy = new Rectangle(40, 104, 35, 40);
            public static readonly Rectangle WeatherStormy = new Rectangle(77, 104, 38, 43);
            public static readonly Rectangle WeatherSnowy = new Rectangle(115, 104, 37, 40);
            public static readonly Rectangle WeatherWindy = new Rectangle(115, 104, 41, 41);
            public static readonly Rectangle WeatherWedding = new Rectangle(198, 104, 37, 38);
            public static readonly Rectangle WeatherFestival = new Rectangle(235, 104, 47, 45);
            public static readonly Rectangle WeatherBlizzard = new Rectangle(281, 104, 40, 42);
            public static readonly Rectangle WeatherDryLightning = new Rectangle(321,105,33,39);
            public static readonly Rectangle WeatherThundersnow = new Rectangle(356,103,42,39);

            /// <summary>A down arrow for scrolling content.</summary>
            public static readonly Rectangle DownArrow = new Rectangle(12, 76, 40, 44);

            /// <summary>An up arrow for scrolling content.</summary>
            public static readonly Rectangle UpArrow = new Rectangle(76, 72, 40, 44);
            
        }

        public static Texture2D Pixel => LazyPixel.Value;

        private static readonly Lazy<Texture2D> LazyPixel = new Lazy<Texture2D>(() =>
        {
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        });
    }
}
