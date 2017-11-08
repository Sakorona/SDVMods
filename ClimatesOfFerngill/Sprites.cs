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
                source = helper.Load<Texture2D>(Path.Combine("Assets","climatesheet2.png"));
                source2 = Game1.mouseCursors;
            }

            public Rectangle GetNightMoonSprite(int moonDay)
            {
                switch (moonDay)
                {
                    case 16:
                    case 0:
                        return Icons.BigMoonNewDay0;
                    case 1:
                        return Icons.BigWaxingCrescentDay1;
                    case 2:
                        return Icons.BigWaxingCrescentDay2;
                    case 3:
                        return Icons.BigWaxingCrescentDay3;
                    case 4:
                        return Icons.BigFirstQuarterDay4;
                    case 5:
                        return Icons.BigWaxingGibbeousDay5;
                    case 6:
                        return Icons.BigWaxingGibbeousDay6;
                    case 7:
                        return Icons.BigWaxingGibbeousDay7;
                    case 8:
                        return Icons.BigFullMoonDay8;
                    case 9:
                        return Icons.BigWaningGibbeousDay9;
                    case 10:
                        return Icons.BigWaningGibbeousDay10;
                    case 11:
                        return Icons.BigWaningGibbeousDay11;
                    case 12:
                        return Icons.BigThirdQuarterDay12;
                    case 13:
                        return Icons.BigWaningCrescentDay13;
                    case 14:
                        return Icons.BigWaningCrescentDay14;
                    case 15:
                        return Icons.BigWaningCrescentDay15;
                }

                return Icons.BigMoonNewDay0;
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

            public Rectangle GetWeatherSprite(int weather, SpecialWeather cond)
            {
                if (cond == SpecialWeather.Blizzard)
                    return Icons.WeatherBlizzard;

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

            //big moon
            public static readonly Rectangle BigMoonNewDay0 = new Rectangle(13, 290, 78, 78);
            public static readonly Rectangle BigWaxingCrescentDay1 = new Rectangle(86, 288, 71, 67);
            public static readonly Rectangle BigWaxingCrescentDay2 = new Rectangle(158, 295, 75, 66);
            public static readonly Rectangle BigWaxingCrescentDay3 = new Rectangle(233, 289, 78, 73); 
            public static readonly Rectangle BigFirstQuarterDay4 = new Rectangle(310,287,81,76);
            public static readonly Rectangle BigWaxingGibbeousDay5 = new Rectangle(393, 286, 73, 76);
            public static readonly Rectangle BigWaxingGibbeousDay6 = new Rectangle(466, 286, 79, 75);
            public static readonly Rectangle BigWaxingGibbeousDay7 = new Rectangle(544, 286, 77, 74);
            public static readonly Rectangle BigFullMoonDay8 = new Rectangle(11, 370, 76, 69);
            public static readonly Rectangle BigWaningGibbeousDay9 = new Rectangle(86, 370, 75, 70);
            public static readonly Rectangle BigWaningGibbeousDay10 = new Rectangle(161, 369, 73, 70);
            public static readonly Rectangle BigWaningGibbeousDay11 = new Rectangle(234, 366, 75, 75);
            public static readonly Rectangle BigThirdQuarterDay12 = new Rectangle(311, 367, 72, 73);
            public static readonly Rectangle BigWaningCrescentDay13 = new Rectangle(388, 369, 73, 70);
            public static readonly Rectangle BigWaningCrescentDay14 = new Rectangle(471, 369, 77, 74);
            public static readonly Rectangle BigWaningCrescentDay15 = new Rectangle(550, 369, 75, 74);


            //Weather
            public static readonly Rectangle WeatherSunny = new Rectangle(24, 196, 40, 37);
            public static readonly Rectangle WeatherRainy = new Rectangle(63, 197, 35, 36);
            public static readonly Rectangle WeatherStormy = new Rectangle(96, 196, 41, 42);
            public static readonly Rectangle WeatherSnowy = new Rectangle(137, 196, 39, 35);
            public static readonly Rectangle WeatherWindy = new Rectangle(180, 198, 35, 31);
            public static readonly Rectangle WeatherWedding = new Rectangle(221, 196, 37, 35);
            public static readonly Rectangle WeatherFestival = new Rectangle(260, 198, 42, 38);
            public static readonly Rectangle WeatherBlizzard = new Rectangle(301, 195, 43, 38);

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
