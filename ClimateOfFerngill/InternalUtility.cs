using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.Locations;

namespace ClimateOfFerngill
{
    static class InternalUtility
    {
        internal static int TIMEADD = 9001;
        internal static int TIMESUB = 9002;
        private static Dictionary<SDVCrops, double> CropTemps { get; set; }

        public static void SetUpCrops()
        {
            CropTemps = new Dictionary<SDVCrops, double>
            {
                { SDVCrops.Corn, 1.66 },
                { SDVCrops.Wheat, 1.66 },
                { SDVCrops.Amaranth, 1.66 },
                { SDVCrops.Sunflower, 1.66 },
                { SDVCrops.Pumpkin, 1.66 },
                { SDVCrops.Eggplant, 1.66 },
                { SDVCrops.Yam, 1.66 },
                { SDVCrops.Artichoke, 0 },
                { SDVCrops.BokChoy, 0 },
                { SDVCrops.Grape, -.55 },
                { SDVCrops.FairyRose, -2.22 },
                { SDVCrops.Beet, -2.22 },
                { SDVCrops.Cranberry, -3.33 },
                { SDVCrops.Ancient, -3.33 },
                { SDVCrops.SweetGemBerry, -3.33 }
            };
        }

        public static double CheckCropTolerance(int currentCrop)
        {
            return CropTemps[(SDVCrops)currentCrop];
        }

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

        public static string GetFestivalName(int dayOfMonth, string currentSeason)
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

        internal static bool VerifyValidTime(int time)
        {
            //basic bounds first
            if (time >= 0600 && time <= 2600)
                return false;
            if ((time % 100) > 50)
                return false;

            return true;
        }

        internal static int GetNewValidTime(int originVal, int changeVal, int mode)
        {
            int oHour = 0, oMin = 0, cHour = 0, cMin = 0, retVal = 0;

            oHour = (int)Math.Floor((double)originVal / 100);
            oMin = (int)Math.Floor((double)originVal % 100);

            cHour = (int)Math.Floor((double)changeVal / 100);
            cMin = (int)Math.Floor((double)changeVal % 100);
            
            //check for boundaries, and original value.
            if (mode == TIMEADD)
            {
                retVal = (oHour + cHour) * 100;
                int testMin = oMin + cMin;
                while (testMin > 59)
                {
                    retVal += 100;
                    testMin -= 60;

                    if (testMin < 0) //sanity check.
                        testMin = 0;
                }

                retVal = retVal + testMin;
            }
            else if (mode == TIMESUB)
            {
                retVal = (oHour - cHour) * 100;
                int testMin = oMin - cMin;
                while (testMin < -59)
                {
                    retVal -= 100;
                    testMin += 60;

                    if (testMin > 0)
                        testMin = 0;
                }
            }

            if (retVal < 0600)
                return 0600;
            if (retVal > 2600)
                return 2600;

            return retVal; 
        }

        internal static void FaintPlayer()
        {
            Game1.player.Stamina = 0;
            Game1.player.doEmote(36);
            Game1.farmerShouldPassOut = true;
        }

        internal static string GetNewSeason(string currentSeason)
        {
            if (currentSeason == "spring") return "summer";
            if (currentSeason == "summer") return "fall";
            if (currentSeason == "fall") return "winter";
            if (currentSeason == "winter") return "spring";

            return "ERROR";
        }

        public static Beach GetBeach()
        {
            return Game1.getLocationFromName("Beach") as Beach;
        }

        internal static bool IsFallCrop(int crop)
        {
            if (Enum.IsDefined(typeof(SDVCrops), crop))
                return true;
            else
                return false;
        }
    }
}
