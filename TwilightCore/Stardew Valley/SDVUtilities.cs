using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.Locations;
using StardewValley.Monsters;
using TwilightCore.PRNG;

namespace TwilightCore.StardewValley
{
    public static class SDVUtilities
    {
       public static Dictionary<SDVDate, int> ForceDays = new Dictionary<SDVDate, int>
       {
            { new SDVDate("spring", 1), Game1.weather_sunny },
            { new SDVDate("spring", 2), Game1.weather_sunny },
            { new SDVDate("spring", 4), Game1.weather_sunny },
            { new SDVDate("spring", 13), Game1.weather_festival },
            { new SDVDate("spring", 24), Game1.weather_festival },
            { new SDVDate("summer", 1), Game1.weather_sunny },
            { new SDVDate("summer", 11), Game1.weather_festival },
            { new SDVDate("summer", 13), Game1.weather_lightning },
            { new SDVDate("summer", 25), Game1.weather_lightning },
            { new SDVDate("summer", 26), Game1.weather_lightning },
            { new SDVDate("summer", 28), Game1.weather_festival },
            { new SDVDate("fall",  1), Game1.weather_sunny },
            { new SDVDate("fall", 16), Game1.weather_festival },
            { new SDVDate("fall", 27), Game1.weather_festival },
            { new SDVDate("winter",  1), Game1.weather_sunny },
            { new SDVDate("winter", 8), Game1.weather_festival },
            { new SDVDate("winter", 25), Game1.weather_festival }
       };

        public static void ShakeScreenOnLowStamina()
        {
            Game1.staminaShakeTimer = 1000;
            for (int i = 0; i < 4; i++)
            {
                Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2((float)(Game1.random.Next(Game1.tileSize / 2) + Game1.viewport.Width - (48 + Game1.tileSize / 8)), (float)(Game1.viewport.Height - 224 - Game1.tileSize / 4 - (int)((double)(Game1.player.MaxStamina - 270) * 0.715))), false, 0.012f, Color.SkyBlue)
                {
                    motion = new Vector2(-2f, -10f),
                    acceleration = new Vector2(0f, 0.5f),
                    local = true,
                    scale = (float)(Game1.pixelZoom + Game1.random.Next(-1, 0)),
                    delayBeforeAnimationStart = i * 30
                });
            }
        }

        public static string GetFestivalName() => GetFestivalName(Game1.dayOfMonth, Game1.currentSeason);
        public static string GetTomorrowFestivalName() => GetFestivalName(Game1.dayOfMonth + 1, Game1.currentSeason);
        
        public static string PrintStringArray(string[] array)
        {
            string s = "";
            for (int i = 0; i < array.Length; i++)
            {
                s = s + $"Command {i} is {array[i]}";
            }

            return s;
        }

        public static string PrintCurrentWeatherStatus()
        {
            return $"Printing current weather status:" +
                    $"It is Raining: {Game1.isRaining} {Environment.NewLine}" +
                    $"It is Stormy: {Game1.isLightning} {Environment.NewLine}" +
                    $"It is Snowy: {Game1.isSnowing} {Environment.NewLine}" +
                    $"It is Debris Weather: {Game1.isDebrisWeather} {Environment.NewLine}";
        }

        private static string GetFestivalName(int dayOfMonth, string currentSeason)
        {
            switch (currentSeason)
            {
                case ("spring"):
                    if (dayOfMonth == 13) return "Egg Festival";
                    if (dayOfMonth == 24) return "Flower Dance";
                    break;
                case ("winter"):
                    if (dayOfMonth == 8) return "Festival of Ice";
                    if (dayOfMonth == 25) return "Feast of the Winter Star";
                    break;
                case ("fall"):
                    if (dayOfMonth == 16) return "Stardew Valley Fair";
                    if (dayOfMonth == 27) return "Spirit's Eve";
                    break;
                case ("summer"):
                    if (dayOfMonth == 11) return "Luau";
                    if (dayOfMonth == 28) return "Dance of the Moonlight Jellies";
                    break;
                default:
                    return "Festival";
            }

            return "Festival";

        }

        public static void ShowMessage(string msg)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true)
            {
                whatType = 2
            };
            Game1.addHUDMessage(hudmsg);
        }

        public static void FaintPlayer()
        {
            Game1.player.Stamina = 0;
            Game1.player.doEmote(36);
            Game1.farmerShouldPassOut = true;
        }

        public static string WeatherToString(int weather)
        {
            switch (weather)
            {
                case 0:
                    return "Sunny";
                case 1:
                    return "Rain";
                case 2:
                    return "Debris";
                case 3:
                    return "Lightning";
                case 4:
                    return "Festival";
                case 5:
                    return "Snow";
                case 6:
                    return "Wedding";
                default:
                    return "<ERROR>";
            }
        }

        public static void SpawnGhostOffScreen(MersenneTwister Dice)
        {
            Vector2 zero = Vector2.Zero;

            if (Game1.getFarm() is Farm ourFarm)
            {
                switch (Game1.random.Next(4))
                {
                    case 0:
                        zero.X = (float)Dice.Next(ourFarm.map.Layers[0].LayerWidth);
                        break;
                    case 1:
                        zero.X = (float)(ourFarm.map.Layers[0].LayerWidth - 1);
                        zero.Y = (float)Dice.Next(ourFarm.map.Layers[0].LayerHeight);
                        break;
                    case 2:
                        zero.Y = (float)(ourFarm.map.Layers[0].LayerHeight - 1);
                        zero.X = (float)Dice.Next(ourFarm.map.Layers[0].LayerWidth);
                        break;
                    case 3:
                        zero.Y = (float)Game1.random.Next(ourFarm.map.Layers[0].LayerHeight);
                        break;
                }

                if (Utility.isOnScreen(zero * (float)Game1.tileSize, Game1.tileSize))
                    zero.X -= (float)Game1.viewport.Width;

                List<NPC> characters = ourFarm.characters;
                Ghost bat = new Ghost(zero * Game1.tileSize)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                };
                characters.Add((NPC)bat);
            }
        }
    }
}
