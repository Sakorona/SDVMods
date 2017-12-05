using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;

namespace ClimatesOfFerngillRebuild
{
    class Descriptions
    {
        private List<string> NonTownLocations = new List<string>()
        {
            "Castle Village",
            "Basket Town",
            "Pine Mesa City",
            "Point Break",
            "Minister Valley",
            "Grampleton",
            "Zuzu City",
            "Fort Josa",
            "Chestervale",
            "Fern Island",
            "Tanker Grove",
            "Pathos Isle"
        };

        private ITranslationHelper Helper;

        public Descriptions(ITranslationHelper Translaton)
        {
            Helper = Translaton;
        }


        internal string GetDescOfDay(SDate date)
        {
            return Helper.Get("date" + GeneralFunctions.FirstLetterToUpper(date.Season) + date.Day);
        }

        internal TemporaryAnimatedSprite GetWeatherOverlay(TV tv)
        {
            Rectangle placement = new Rectangle(413, 333, 13, 33);

            switch (Game1.weatherForTomorrow)
            {
                case 0:
                case 6:
                    placement = new Rectangle(413, 333, 13, 33);
                    break;
                case 1:
                    placement = new Rectangle(465, 333, 13, 13);
                    break;
                case 2:
                    placement = Game1.currentSeason.Equals("spring") ? new Rectangle(465, 359, 13, 13) : (Game1.currentSeason.Equals("fall") ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13));
                    break;
                case 3:
                    placement = new Rectangle(413, 346, 13, 13);
                    break;
                case 4:
                    placement = new Rectangle(413, 372, 13, 13);
                    break;
                case 5:
                    placement = new Rectangle(465, 346, 13, 13);
                    break;
            }

            return new TemporaryAnimatedSprite(Game1.mouseCursors, placement, 100f, 4, 999999, tv.getScreenPosition() + new Vector2(3f, 3f) * tv.getScreenSizeModifier(), false, false, (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 1.99999994947575E-05), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);
        }
    }
}
