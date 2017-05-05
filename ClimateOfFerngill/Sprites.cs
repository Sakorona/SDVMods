using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using System;
using System.IO;

namespace ClimateOfFerngill
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

        /// <summary>A blank pixel which can be colorised and stretched to draw geometric shapes.</summary>
        public static Texture2D Pixel => LazyPixel.Value;

        private static readonly Lazy<Texture2D> LazyPixel = new Lazy<Texture2D>(() =>
        {
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        });

        /// <summary> Sprites used for drawing various weather stuff </summary>
        public class Icons
        {
           public Texture2D source;

           public Icons(IContentHelper helper)
           {
                source = helper.Load<Texture2D>("climatesheet2.png");
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

            public Rectangle GetWeatherSprite(SDVWeather weather)
            {
                if (weather == SDVWeather.Debris)
                    return Icons.WeatherWindy;
                if (weather == SDVWeather.Festival)
                    return Icons.WeatherFestival;
                if (weather == SDVWeather.Sunny)
                    return Icons.WeatherSunny;
                if (weather == SDVWeather.Wedding)
                    return Icons.WeatherWedding;
                if (weather == SDVWeather.Snow)
                    return Icons.WeatherSnowy;
                if (weather == SDVWeather.Rainy)
                    return Icons.WeatherRainy;

                return Icons.WeatherSunny;
            }

            // These are the positions of each sprite on the sheet.
            /*
             * For Sheet 1:
            public static readonly Rectangle NewMoon = new Rectangle(17, 21, 10, 9);
            public static readonly Rectangle WaxingCrescent = new Rectangle(26, 21, 9, 9);
            public static readonly Rectangle FirstQuarter = new Rectangle(35, 21, 10, 9);
            public static readonly Rectangle WaxingGibbeous = new Rectangle(44, 21, 10, 9);

            public static readonly Rectangle FullMoon = new Rectangle(17, 31, 10, 9);
            public static readonly Rectangle WaningCrescent = new Rectangle(26, 31, 9, 9);
            public static readonly Rectangle ThirdQuarter = new Rectangle(35, 31, 10, 9);
            public static readonly Rectangle WaningGibbeous = new Rectangle(44, 31, 10, 9);

            public static readonly Rectangle WeatherSunny = new Rectangle(17, 48, 10, 10);
            public static readonly Rectangle WeatherRainy = new Rectangle(26, 48, 10, 10);
            public static readonly Rectangle WeatherStormy = new Rectangle(35, 48, 10, 10);
            public static readonly Rectangle WeatherSnowy = new Rectangle(44, 48, 10, 10);
            public static readonly Rectangle WeatherWindy = new Rectangle(53, 48, 9, 10);
            public static readonly Rectangle WeatherWedding = new Rectangle(62, 48, 9, 10);
            public static readonly Rectangle WeatherFestival = new Rectangle(71, 48, 10, 10);
            */

            //for v2!
            // will add the progression ones later. TODO.
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
        }
    }
}
