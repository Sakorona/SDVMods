using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using System.Linq;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;
using StardewValley.Objects;
using TwilightShards.Common;

namespace CustomizableCartRedux
{
    public class CustomizableCartRedux : Mod
    {
        public CartConfig OurConfig;
        public MersenneTwister Dice;

        public override void Entry(IModHelper helper)
        {
            Dice = new MersenneTwister();
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

                f.travelingMerchantStock = GetTravelingMerchantStock(OurConfig.AmountOfItems);
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

        private Dictionary<Item, int[]> GetTravelingMerchantStock(int numStock)
        {
            Dictionary<Item, int[]> dictionary = new Dictionary<Item, int[]>();
            int maxItemID = Game1.objectInformation.Keys.Max();
            numStock = (numStock <= 2 ? 3 : numStock); //Ensure the stock isn't too low.
            var itemsToBeAdded = new List<int>();

            //get items
            for (int i = 0; i < (numStock - 2); i++)
            {
                int index2 = GetItem(maxItemID);
                string[] strArray = Game1.objectInformation[index2].Split('/');

                if (OurConfig.DisableDuplicates)
                {
                    while (itemsToBeAdded.Contains(index2))
                    {
                        index2 = GetItem(maxItemID);
                    }
                }

                itemsToBeAdded.Add(index2);
            }

            //assign price
            foreach(int i in itemsToBeAdded)
            {
                string[] strArray = Game1.objectInformation[i].Split('/');
                dictionary.Add(new SObject(i, 1), new int[2]
                {
                    (OurConfig.UseCheaperPricing ? (int)Math.Max(Dice.Next(1,6) * 81, Math.Round(Dice.RollInRange(1.87,5.95) * Convert.ToInt32(strArray[1])))
                                : Math.Max(Dice.Next(1, 11) * 100, Convert.ToInt32(strArray[1]) * Dice.Next(3, 6))),
                    Dice.NextDouble() < 0.1 ? 5 : 1
                });
            }

            //hardcoded item add.
            dictionary.Add(GetRandomFurniture(null, 0, 1613), new int[2]
            {
                Dice.Next(1, 11) * 250,
                1
            });

            // if it's less than fall, add a rare seed
            if (Utility.getSeasonNumber(Game1.currentSeason) < 2)
            {
                dictionary.Add((Item)new SObject(347, 1, false, -1, 0), new int[2]
                {
                    1000, Dice.NextDouble() < 0.1 ? 5 : 1
                });

            }
            else if (Dice.NextDouble() < 0.4)
            {
                dictionary.Add((Item)new SObject(Vector2.Zero, 136, false), new int[2]
                {
                    4000, 1
                });
            }

            if (Dice.NextDouble() < 0.25)
            {
                dictionary.Add(key: new SObject(433, 1, false, -1, 0), value: new int[2]
                {
                    1000, 1
                });
            }
            else
            {
                dictionary.Add(key: new SObject(578, 1, false, -1, 0), value: new int[2]
                {
                    1000, 1
                });
            }

            return dictionary;
        }

        private int GetItem(int maxItemID)
        {
            string[] strArray;
            int index2 = Dice.Next(2, maxItemID);
            do
            {
                do //find the nearest one if it doesn't exist
                {
                    index2 = (index2 + 1) % maxItemID;
                }
                while (!Game1.objectInformation.ContainsKey(index2) || Utility.isObjectOffLimitsForSale(index2));

                strArray = Game1.objectInformation[index2].Split('/');
            }
            while (BannedItemsByCondition(index2, strArray));

            return index2;
        }

        private bool CanSellItem(int item)
        {
            bool Allowed = true;

            List<int> RestrictedItems = new List<int>() { 680, 681, 682, 688, 689, 690, 774, 775, 454, 460, 645, 413, 437, 439, 158, 159, 160, 161, 162, 163, 326, 341, 795, 796 };

            if (RestrictedItems.Contains(item))
                Allowed = false;

            if (OurConfig.AllowedItems.Contains(item))
                Allowed = true;

            if (OurConfig.BlacklistedItems.Contains(item))
                Allowed = false;

            return Allowed;
        }

        private bool BannedItemsByCondition(int item, string[] strArray)
        {
            bool CategoryBanned =
                    (!strArray[3].Contains<char>('-') || 
                    Convert.ToInt32(strArray[1]) <= 0 || 
                    (strArray[3].Contains("-13") || strArray[3].Equals("Quest")) || 
                    (strArray[0].Equals("Weeds") || strArray[3].Contains("Minerals") || strArray[3].Contains("Arch")));

            if (OurConfig.AllowedItems.Contains(item))
                CategoryBanned = false;

            return CategoryBanned;
        }

        private Furniture GetRandomFurniture(List<Item> stock, int lowerIndexBound = 0, int upperIndexBound = 1462)
        {
            Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
            int num;
            do
            {
                num = Dice.Next(lowerIndexBound, upperIndexBound);
                if (stock != null)
                {
                    foreach (Item obj in stock)
                    {
                        if (obj is Furniture && obj.parentSheetIndex == num)
                            num = -1;
                    }
                }
            }
            while (IsFurnitureOffLimitsForSale(num) || !dictionary.ContainsKey(num));
            Furniture furniture = new Furniture(num, Vector2.Zero);
            int maxValue = int.MaxValue;
            furniture.stack = maxValue;
            return furniture;
        }

        private static bool IsFurnitureOffLimitsForSale(int index)
        {
            switch (index)
            {
                case 1680:
                case 1733:
                case 1669:
                case 1671:
                case 1541:
                case 1545:
                case 1554:
                case 1402:
                case 1466:
                case 1468:
                case 131:
                case 1226:
                case 1298:
                case 1299:
                case 1300:
                case 1301:
                case 1302:
                case 1303:
                case 1304:
                case 1305:
                case 1306:
                case 1307:
                case 1308:
                    return true;
                default:
                    return false;
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
