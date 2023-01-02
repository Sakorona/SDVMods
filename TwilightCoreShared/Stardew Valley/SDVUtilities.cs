using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Linq;
using System.Collections.Generic;
using TwilightShards.Common;
using StardewModdingAPI;


namespace TwilightShards.Stardew.Common
{
    public static class SDVUtilities
    {
        public static string PrintStringArray(string[] array)
        {
            string s = "";
            for (int i = 0; i < array.Length; i++)
            {
                s += $"Command {i} is {array[i]}";
            }

            return s;
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
                    if (dayOfMonth == 15) return "Night Festival";
                    if (dayOfMonth == 16) return "Night Festival";
                    if (dayOfMonth == 17) return "Night Festival";
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
                    return $"";
            }
            return $"";
        }

        public static T GetModApi<T>(IMonitor Monitor, IModHelper Helper, string name, string minVersion, string friendlyName="") where T : class
        {
            var modManifest = Helper.ModRegistry.Get(name);
            if (modManifest != null)
            {
                if (!modManifest.Manifest.Version.IsOlderThan(minVersion))
                {
                    T api = Helper.ModRegistry.GetApi<T>(name);
                    if (api == null)
                    {
                        Monitor.Log($"{(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)}'s API returned null. ", LogLevel.Info);
                    }

                    if (api != null)
                    {
                        Monitor.Log($"{(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)} {modManifest.Manifest.Version} integration feature enabled", LogLevel.Info);
                    }
                    return api;

                }
                else
                    Monitor.Log($"{(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)} detected, but not of a sufficient version. Req:{minVersion} Detected:{modManifest.Manifest.Version}. Update the other mod if you want to use the integration feature. Skipping..", LogLevel.Debug);
            }
            else
                Monitor.Log($"Didn't find mod {(String.IsNullOrEmpty(friendlyName) ? name : friendlyName)}; you can optionally install it for extra features!", LogLevel.Debug);
            return null;
        }

        public static void ShowMessage(string msg, int whatType)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true)
            {
                whatType = whatType
            };
            Game1.addHUDMessage(hudmsg);
        }

        public static Dictionary<int,int> GetFishListing(GameLocation loc, int timeOfDay, bool limitToValidAtTime = false)
        {
            var fish = new Dictionary<int, int>();
            Dictionary<string, string> locationListing = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            string key = loc.Name;
                     
            if (loc is Farm)
            {
                switch (Game1.whichFarm)
                {
                    case Farm.riverlands_layout:
                      fish = GetFishForLocation(locationListing, "Forest", timeOfDay, limitToValidAtTime);
                      var temp2 = GetFishForLocation(locationListing, "Town", timeOfDay, limitToValidAtTime);
                      foreach (var item in temp2)
                      {
                            if (!fish.ContainsKey(item.Key))
                                fish.Add(item.Key, item.Value);
                      }
                      break;
                    case Farm.beach_layout:
                       fish.Add(152,-1);
                       fish.Add(723, -1);
                       fish.Add(393, -1);
                       fish.Add(719, -1);
                       fish.Add(718, -1);
                       temp2 = GetFishForLocation(locationListing, "Beach", timeOfDay, limitToValidAtTime);
                       foreach (var item in temp2)
                       {
                            fish.Add(item.Key, item.Value);
                       }
                       break;
                    case Farm.fourCorners_layout:
                    case Farm.mountains_layout:
                       fish = GetFishForLocation(locationListing, "Forest", timeOfDay, limitToValidAtTime);
                       break;
                    case Farm.forest_layout:
                        fish.Add(734,1);
                        fish = GetFishForLocation(locationListing, "Forest", timeOfDay, limitToValidAtTime);
                        break;
                    case Farm.combat_layout:
                        fish = GetFishForLocation(locationListing, "Mountain", timeOfDay, limitToValidAtTime);
                        break;
                }
            }

            else if (locationListing.ContainsKey(key))
            {
                fish = GetFishForLocation(locationListing, key, timeOfDay, limitToValidAtTime);
            }
            return fish;
        }

        public static Dictionary<int, int> GetFishForLocation(Dictionary<string, string> locationListing, string key, int timeOfDay, bool limitToValidAtTime)
        {
            var fish = new Dictionary<int, int>();
            string[] rawData = locationListing[key].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');

            Dictionary<string, string> processedData = new();
            if (rawData.Length > 1)
            {
                for (int index = 0; index < rawData.Length; index += 2)
                    if (!(processedData.Keys.Contains(rawData[index])))
                        processedData.Add(rawData[index], rawData[index + 1]);
            }
            string[] locationFish = processedData.Keys.ToArray<string>();

            Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            Utility.Shuffle<string>(Game1.random, locationFish);
            for (int index1 = 0; index1 < locationFish.Length; ++index1)
            {
                //iterate through the fish
                bool isValid = true;
                //get the fish data
                string[] fishParsed = fishData[Convert.ToInt32(locationFish[index1])].Split('/');
                int zoneData = Convert.ToInt32(processedData[locationFish[index1]]);

                //log.Log($"Limit to Valid At Time is {limitToValidAtTime}");
                if (limitToValidAtTime)
                {
                    //check time requirements
                    string[] timeSpawned = fishParsed[5].Split(' ');
                    for (int index2 = 0; index2 < timeSpawned.Length; index2 += 2)
                    {
                        //log.Log($"Fish ID: {locationFish[index1]} being checked with time {timeOfDay} against {Convert.ToInt32(timeSpawned[index2])} and {Convert.ToInt32(timeSpawned[index2 + 1])}");
                        if (timeOfDay < Convert.ToInt32(timeSpawned[index2]) || timeOfDay >= Convert.ToInt32(timeSpawned[index2 + 1]))
                        {
                            //log.Log($"Fish ID:  {locationFish[index1]} is invalid");
                            isValid = false;
                            break;
                        }
                    }
                }

                //check weather requirements
                if (!fishParsed[7].Equals("both"))
                {
                    if (fishParsed[7].Equals("rainy") && !Game1.isRaining)
                        isValid = false;
                    else if (fishParsed[7].Equals("sunny") && Game1.isRaining)
                        isValid = false;
                }

                if (isValid)
                    fish.Add(Convert.ToInt32(locationFish[index1]), zoneData);

            }

            return fish;
        }

        /*
        public static StardewValley.Object GetRandomFish(GameLocation loc)
        {
            int parentSheetIndex = 372;
            Dictionary<string, string> locationListing = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            string key = loc.Name;

            if (locationListing.ContainsKey(key))
            {
                string[] locationData = locationListing[key].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                if (locationData.Length > 1)
                {
                    for (int index = 0; index < locationData.Length; index += 2)
                        dictionary2.Add(locationData[index], locationData[index + 1]);
                }

                string[] array = dictionary2.Keys.ToArray<string>();
                Utility.Shuffle<string>(Game1.random, array);
                Dictionary<int, string> dictionary3 = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
                for (int index1 = 0; index1 < array.Length; ++index1)
                {
                    bool flag2 = true;
                    string[] strArray2 = dictionary3[Convert.ToInt32(array[index1])].Split('/');
                    string[] strArray3 = strArray2[5].Split(' ');
                    int int32 = Convert.ToInt32(dictionary2[array[index1]]);
                    if (int32 == -1)
                    {
                        for (int index2 = 0; index2 < strArray3.Length; index2 += 2)
                        {
                            if (Game1.timeOfDay >= Convert.ToInt32(strArray3[index2]) && Game1.timeOfDay < Convert.ToInt32(strArray3[index2 + 1]))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }
                    if (!strArray2[7].Equals("both"))
                    {
                        if (strArray2[7].Equals("rainy") && !Game1.isRaining)
                            flag2 = true;
                        else if (strArray2[7].Equals("sunny") && Game1.isRaining)
                            flag2 = true;
                    }
                    
                    if (!flag2)
                    {
                        parentSheetIndex = Convert.ToInt32(array[index1]);
                    }
                }
                return new StardewValley.Object(parentSheetIndex, 1, false, -1, 0);
            }

            return new StardewValley.Object(parentSheetIndex, 1, false, -1, 0);
        }
 */
    }
}
