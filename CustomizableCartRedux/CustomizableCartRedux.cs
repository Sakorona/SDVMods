using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework;

namespace CustomizableCartRedux
{
    public class CustomizableCartRedux : Mod
    {
        public CartConfig OurConfig;

        public override void Entry(IModHelper helper)
        {
            OurConfig = helper.ReadConfig<CartConfig>();
            TimeEvents.AfterDayStarted += SetCartSpawn;
        }

         private void SetCartSpawn(object Sender, EventArgs e)
        {
            Random r = new Random();
            double randChance = r.NextDouble(), dayChance = 0;
            Forest f = Game1.getLocationFromName("Forest") as Forest;

            if (f is null)
                throw new Exception("The Forest is not loaded. Please verify your game is properly installed.");

            //get the day
            DayOfWeek day = GetDayOfWeek(SDate.Now());
            switch (day)
            {
                case DayOfWeek.Monday:
                    dayChance = OurConfig.MondayChance;
                    break;
                case DayOfWeek.Tuesday:
                    dayChance = OurConfig.TuesdayChance;
                    break;
                case DayOfWeek.Wednesday:
                    dayChance = OurConfig.WednesdayChance;
                    break;
                case DayOfWeek.Thursday:
                    dayChance = OurConfig.ThursdayChance;
                    break;
                case DayOfWeek.Friday:
                    dayChance = OurConfig.FridayChance;
                    break;
                case DayOfWeek.Saturday:
                    dayChance = OurConfig.SaturdayChance;
                    break;
                case DayOfWeek.Sunday:
                    dayChance = OurConfig.SundayChance;
                    break;
                default:
                    dayChance = 0;
                    break;
            }

            /* Start of the Season - Day 1. End of the Season - Day 28. Both is obviously day 1 and 28 
               Every other week is only on days 8-14 and 22-28) */

            bool SetCartToOn = false;
            if (OurConfig.AppearOnlyAtEndOfSeason)
            {
                if (Game1.dayOfMonth == 28)
                    SetCartToOn = true;
            }

            else if (OurConfig.AppearOnlyAtStartAndEndOfSeason)
            {
                if (Game1.dayOfMonth == 28 || Game1.dayOfMonth == 1)
                    SetCartToOn = true;
            }

            else if (OurConfig.AppearOnlyAtStartAndEndOfSeason)
            {
                if (Game1.dayOfMonth == 28 || Game1.dayOfMonth == 1)
                    SetCartToOn = true;
            }

            else if (OurConfig.AppearOnlyAtStartOfSeason)
            {
                if (Game1.dayOfMonth == 1)
                    SetCartToOn = true;
            }

            else if (OurConfig.AppearOnlyEveryOtherWeek)
            {
                if ((Game1.dayOfMonth >= 8 && Game1.dayOfMonth <= 14) || (Game1.dayOfMonth >= 22 && Game1.dayOfMonth <= 28))
                {
                    if (dayChance > randChance)
                    {
                        SetCartToOn = true;
                    }
                }
            }

            else
            {
                if (dayChance > randChance)
                {
                    SetCartToOn = true;
                }
            }

            if (SetCartToOn)
            {
                f.travelingMerchantDay = true;
                f.travelingMerchantBounds = new List<Rectangle>
                {
                    new Rectangle(23 * Game1.tileSize, 10 * Game1.tileSize, 123 * Game1.pixelZoom, 28 * Game1.pixelZoom),
                    new Rectangle(23 * Game1.tileSize + 45 * Game1.pixelZoom, 10 * Game1.tileSize + 26 * Game1.pixelZoom, 19 * Game1.pixelZoom, 12 * Game1.pixelZoom),
                    new Rectangle(23 * Game1.tileSize + 85 * Game1.pixelZoom, 10 * Game1.tileSize + 26 * Game1.pixelZoom, 26 * Game1.pixelZoom, 12 * Game1.pixelZoom)
                };

                f.travelingMerchantStock = Utility.getTravelingMerchantStock();
                foreach (Rectangle travelingMerchantBound in f.travelingMerchantBounds)
                {
                    Utility.clearObjectsInArea(travelingMerchantBound, f);                 
                }
            }
            else
            {
                //clear other values
                f.travelingMerchantBounds = null;
                f.travelingMerchantDay = false;
                f.travelingMerchantStock = null;
            }
        }

        private DayOfWeek GetDayOfWeek(SDate Target)
        {
            switch (Target.Day % 7)
            {
                case 0:
                    return DayOfWeek.Sunday;
                case 1:
                    return DayOfWeek.Monday;
                case 2:
                    return DayOfWeek.Tuesday;
                case 3:
                    return DayOfWeek.Wednesday;
                case 4:
                    return DayOfWeek.Thursday;
                case 5:
                    return DayOfWeek.Friday;
                case 6:
                    return DayOfWeek.Saturday;
                default:
                    return 0;
            }
        }
    }
}
