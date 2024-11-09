using System;
using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using System.Linq;
using StardewValley.TerrainFeatures;

namespace StardewNotification
{
    public class GeneralNotification
    {
        public static void DoNewDayNotifications(ITranslationHelper Trans)
        {
            CheckForBirthday(Trans);
            CheckForFestival(Trans);
            CheckForMaxLuck(Trans);
            CheckForTravelingMerchant(Trans);
            CheckForTVChannels(Trans);
            CheckForToolUpgrade(Trans);
            CheckForHayLevel(Trans);
            CheckForSpringOnions(Trans);
        }

        public static void DoBookSellerReminder(ITranslationHelper Trans)
        {
            if (!StardewNotification.Config.NotifyBookseller) return;
            
            if (StardewNotification.StartInGingerIslandLoaded && !Game1.player.hasOrWillReceiveMail("willyBoatFixed"))
                return;

            //!(Game1.player.locationsVisited.Contains("IslandSouth") || 

            if (Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
                Util.ShowMessage(Trans.Get("bookseller"));
        }

        public static void CheckForSpringOnions(ITranslationHelper Trans)
        {
            if (!StardewNotification.Config.ShowSpringOnionCount)
                return;

            if (StardewNotification.StartInGingerIslandLoaded && !Game1.player.hasOrWillReceiveMail("willyBoatFixed"))
                return;

            //they really only grow in the forest, thankfully.
            var loc = Game1.locations.Where(n => n is Forest).First();
            int count = 0;
            foreach (var l in loc.terrainFeatures.Values)
            {
                if (l is HoeDirt h && h.crop != null && h.crop.forageCrop.Value == true && h.crop.whichForageCrop.Value == "1")
                    count++;
            }

            if (count > 0)
            {
                Util.ShowMessage(Trans.Get("springOnion", new { count}));
            }
        }

        public static void DoWeatherReminder(ITranslationHelper trans)
        {
            //check for force days.
            string weather = Game1.netWorldState.Value.GetWeatherForLocation("Default").WeatherForTomorrow;
            var Tomorrow = WorldDate.ForDaysPlayed(++WorldDate.Now().TotalDays);
            //Console.WriteLine($"Tommorow is {Tomorrow.DayOfMonth} {Tomorrow.Season} {Tomorrow.Year}");
            weather = Game1.getWeatherModificationsForDate(Tomorrow, weather);

            //mod integration
            //if (StardewNotification.FallOnDay28Loaded && Game1.dayOfMonth == 27 && Game1.season == Season.Fall)
            //    weather = "Snow";

            switch (weather)
            {
                case "Rain":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-rain") }));
                    break;
                case "Wind":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-wind") }));
                    break;
                case "Storm":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-tstorm") }));
                    break;
                case "Snow":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-snow") }));
                    break;
                case "Wedding":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-wedding") }));
                    break;
                case "Festival":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-festival", new { festivalName = GetFestivalName(trans) }) }));
                    break;
                case "GreenRain":
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-greenrain") }));
                    break;
                case "Sun":
                default:
                    Util.ShowMessage(trans.Get("weather", new { weather = trans.Get("weather-sunny") }));
                    break;
            }            
        }

        public static void CheckForHayLevel(ITranslationHelper Trans)
        {
            if (!StardewNotification.Config.NotifyHay) 
                return;

            bool cont = false;

            //check if the silo exists and is not being built
            foreach(var building in Game1.getFarm().buildings)
            {
                if (building.buildingType.Value == "Silo" && building.daysOfConstructionLeft.Value <= 0)
                    cont = true;
            }

            if (!cont)
                return;

            int hayAmt = Game1.getFarm().piecesOfHay.Value;
            if (hayAmt > 0)
                Util.ShowMessage(Trans.Get("hayMessage", new { hayAmt = Game1.getFarm().piecesOfHay.Value}));
            else if (StardewNotification.Config.ShowEmptyhay)
                Util.ShowMessage(Trans.Get("noHayMessage"));
        }
        
        public static void DoBirthdayReminder(ITranslationHelper Trans)
        {
            var character = Utility.getTodaysBirthdayNPC();
            if (character is not null && Game1.player.friendshipData.Keys.Contains(character.Name) && Game1.player.friendshipData[character.Name].GiftsToday != 1)
            {
                Util.ShowMessage(Trans.Get("birthdayReminder", new { charName = character.displayName }));
            }
        }

        private static void CheckForBirthday(ITranslationHelper Trans)
        {
            if (StardewNotification.Config.NotifyBirthdays)
            {
                var character = Utility.getTodaysBirthdayNPC();
                if (character is null) return;

                if ((Game1.player.friendshipData.TryGetValue(character.Name, out var friendship) == false || friendship is null) && StardewNotification.Config.HideUnknownNPCBirthday == true)
                    return; 
                
                Util.ShowMessage(Trans.Get("birthday", new { charName = character.displayName }));
            }
        }

        private static void CheckForTravelingMerchant(ITranslationHelper Trans)
        {
            if (StardewNotification.StartInGingerIslandLoaded && !Game1.player.hasOrWillReceiveMail("willyBoatFixed"))
                return;


            Forest f = Game1.getLocationFromName("Forest") as Forest;
            if (!StardewNotification.Config.NotifyTravelingMerchant) return;

            
            if (f.ShouldTravelingMerchantVisitToday())
            {
                Util.ShowMessage(Trans.Get("travelingMerchant"));
            }
        }

        private static void CheckForToolUpgrade(ITranslationHelper Trans)
        {
            if (!StardewNotification.Config.NotifyToolUpgrade) return;
            if (Game1.player.toolBeingUpgraded.Value is not null && Game1.player.daysLeftForToolUpgrade.Value <= 0)
                Util.ShowMessage(Trans.Get("toolPickup", new {toolName = Game1.player.toolBeingUpgraded.Value.DisplayName }));
        }

        private static void CheckForMaxLuck(ITranslationHelper Trans)
        {
            if (StardewNotification.Config.NotifyMaxLuck && Game1.player.DailyLuck > 0.07)
                Util.ShowMessage(Trans.Get("luckyDay"));
            else if (StardewNotification.Config.NotifyMinLuck && Game1.player.DailyLuck < -.07)
                Util.ShowMessage(Trans.Get("unluckyDay"));
        }


        private static void CheckForTVChannels(ITranslationHelper Trans)
        {
            if (!StardewNotification.Config.NotifyTVChannels) return;

            if (StardewNotification.StartInGingerIslandLoaded && !Game1.player.hasOrWillReceiveMail("willyBoatFixed"))
                return;

            if (Game1.IsGreenRainingHere()) { 
                Util.ShowMessage(Trans.Get("noSignal"));
                return;
            }

            var dayName = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            switch(dayName)
            {
                case "Mon":
                case "Thu":
                    Util.ShowMessage(Trans.Get("checkLiving"));
                    break;
                case "Wed":
                    Util.ShowMessage(Trans.Get("checkRerun"));
                    break;
                case "Sun":
                    Util.ShowMessage(Trans.Get("queenSauce"));
                    break;
                default:
                    break;
            }
        }


        private static void CheckForFestival(ITranslationHelper Trans)
        {
            if (StardewNotification.StartInGingerIslandLoaded && !Game1.player.hasOrWillReceiveMail("willyBoatFixed"))
                return;

            if (!StardewNotification.Config.NotifyFestivals || !Utility.isFestivalDay() || !Utility.IsPassiveFestivalDay()) return;
            var festivalName = GetFestivalName(Trans);
            Util.ShowMessage(Trans.Get("fMsg", new { fest = festivalName }));

            if (!festivalName.Equals(Trans.Get("WinterStar"))) return;
            Random r = new((int)(Game1.uniqueIDForThisGame / 2uL) ^ Game1.year ^ (int)Game1.player.UniqueMultiplayerID);
            var santa = Utility.getRandomTownNPC(r).displayName;

            Util.ShowMessage(Trans.Get("SecretSantaReminder", new { charName = santa })); 
        }

        private static string GetFestivalName(ITranslationHelper Trans)
        {
            var season = Game1.currentSeason;
            var day = Game1.dayOfMonth;
            switch (season)
            {
                case "spring":
                    if (day == 13) return Trans.Get("EggFestival");
                    if (day == 15 || day == 16 || day == 17) return Trans.Get("DesertFestival");
                    if (day == 24) return Trans.Get("FlowerDance");
                    break;
                case "summer":
                    if (day == 11) return Trans.Get("Luau");
                    if (day == 20 || day == 21) return Trans.Get("TroutDerby");
                    if (day == 28) return Trans.Get("MoonlightJellies");
                    break;
                case "fall":
                    if (day == 16) return Trans.Get("ValleyFair");
                    if (day == 27) return Trans.Get("SpiritsEve");
                    break;
                case "winter":
                    if (day == 8) return Trans.Get("IceFestival");
                    if (day == 12 || day == 13) return Trans.Get("SquidFest");
                    if (day == 14 || day == 15 || day == 16) return Trans.Get("NightFestival");
                    if (day == 25) return Trans.Get("WinterStar");
                    break;
                default:
                    break;
            }
            return Trans.Get("festival");
        }

        internal static void DoGingerIslandWeatherReminder(ITranslationHelper trans)
        {
            if (!(Game1.player.locationsVisited.Contains("IslandSouth")))
                return;

            //check for force days.
            string weather = Game1.netWorldState.Value.GetWeatherForLocation("Island").WeatherForTomorrow;
            //weather = Game1.getWeatherModificationsForDate(WorldDate.ForDaysPlayed(WorldDate.Now().TotalDays++), weather);

            //mod integration
            if (StardewNotification.FallOnDay28Loaded && Game1.dayOfMonth == 27 && Game1.season == Season.Fall)
                weather = "Snow";

            switch (weather)
            {
                case "Rain":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-rain") }));
                    break;
                case "Wind":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-wind") }));
                    break;
                case "Storm":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-tstorm") }));
                    break;
                case "Snow":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-snow") }));
                    break;
                case "Wedding":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-wedding") }));
                    break;
                case "Festival":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-festival", new { festivalName = GetFestivalName(trans) }) }));
                    break;
                case "GreenRain":
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-greenrain") }));
                    break;
                case "Sun":
                default:
                    Util.ShowMessage(trans.Get("weatherGingerIsland", new { weather = trans.Get("weather-sunny") }));
                    break;
            }
        }
    }
}
