using NPack;
using StardewModdingAPI;
using StardewValley;

namespace ClimateOfFerngill
{
    static class WeatherHelper
    {
        /// <summary>
        /// This function checks if we're allowed to storm.
        /// </summary>
        /// <param name="Config">Mod configuration variable</param>
        /// <returns>If we can storm or not</returns>
        public static bool CanWeStorm(ClimateConfig Config)
        {
            if (Game1.year == 1 && Game1.currentSeason == "spring") return Config.AllowStormsFirstSpring;
            else return true;
        }

        public static bool IsSeason(SDVSeasons season)
        {
            return (season.ToString()).ToLower() == Game1.currentSeason;
        }

        /// <summary>
        /// This function just attempts to fix the TV for tomorrow to match forced weather.
        /// </summary>
        /// <returns>True if it was fixed, false elsewise</returns>
        public static bool FixTV()
        {
            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 1)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return true;
            }

            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 2)
            {
                Game1.weatherForTomorrow = Game1.weather_rain;
                return true;
            }

            if (Game1.currentSeason == "spring" && Game1.year == 1 && Game1.dayOfMonth == 3)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return true;
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 12)
            {
                Game1.weatherForTomorrow = Game1.weather_lightning;
                return true;
            }

            if (Game1.currentSeason == "summer" && Game1.dayOfMonth == 25)
            {
                Game1.weatherForTomorrow = Game1.weather_lightning;
                return true;
            }

            if (Game1.dayOfMonth == 28)
            {
                Game1.weatherForTomorrow = Game1.weather_sunny;
                return true;
            }

            return false;
        }

        public static string DescWeather(int weather)
        {
            switch (weather)
            {
                case 0:
                    return "Sunny";
                case 1:
                    return "Rainy";
                case 2:
                    return "Debris";
                case 3:
                    return "Stormy";
                case 4:
                    return "Festival";
                case 5:
                    return "Snowy";
                case 6:
                    return "Wedding";
                default:
                    return "Weather not present in base game";
            }
        }

        public static string DescWeather(SDVWeather weather)
        {
            switch (weather)
            {
                case SDVWeather.Sunny:
                    return "Sunny";
                case SDVWeather.Rainy:
                    return "Rainy";
                case SDVWeather.Debris:
                    return "Debris";
                case SDVWeather.Stormy:
                    return "Stormy";
                case SDVWeather.Festival:
                    return "Festival";
                case SDVWeather.Snow:
                    return "Snowy";
                case SDVWeather.Wedding:
                    return "Wedding";
                default:
                    return "Weather not present in base game";
            }
        }

        public static string DisplayTemperature(double temp, string tempGauge)
        {
            //base temps are always in celsius
            if (tempGauge == "celsius")
            {
                return temp + " C";
            }

            if (tempGauge == "kelvin")
            {
                return (temp + 273.15) + " K";
            }

            if (tempGauge == "rankine")
            {
                double tmpTemp = (temp + 273.15) * 1.8;
                return string.Format("{0:0.00}", tmpTemp) + " Ra";
            }

            if (tempGauge == "fahrenheit")
            {
                double tmpTemp = (temp * 1.8) + 32;
                return string.Format("{0:0.00}", tmpTemp) + " F";
            }

            if (tempGauge == "romer")
            {
                return string.Format("{0:0.00}", (temp * 1.904761905) + 7.5) + " Ro";
            }

            if (tempGauge == "delisle")
            {
                return string.Format("{0:0.00}", ((100 - temp) * 1.5)) + " De";
            }

            if (tempGauge == "reaumur")
            {
                return string.Format("{0:0.00}", temp * .8) + " Re";
            }

            return "ERROR";
        }

        public static SDVWeather GetTodayWeather()
        {
            if (Game1.isRaining)
            {
                if (Game1.isLightning) return SDVWeather.Stormy;
                else return SDVWeather.Rainy;
            }

            if (Game1.isSnowing) return SDVWeather.Snow;
            if (Game1.isDebrisWeather) return SDVWeather.Debris;

            if (Game1.weddingToday == true)
                return SDVWeather.Wedding;

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
                return SDVWeather.Festival;

            return SDVWeather.Sunny;
        }

        public static string GetWeatherDesc(MersenneTwister dice, SDVWeather weather, bool today, IMonitor logger)
        {

            string[] springRainText = new string[] { "it'll be a rainy day outside! Make sure to bring your coat. ", "it'll be a wet day outside. ", "it'll be a misty, wet day - make sure to pause when you can and enjoy it! " };
            string[] springStormText = new string[] { "early showers bring summer flowers! It'll be stormy outside. ", "expect some lightning outside -  be careful! ", "a storm front is blowing over the region, bringing rain and lightning. " };
            string[] springWindText = new string[] { "it'll be a blustery day outside. ", "a cold front is blowing through - if you have allergies, be careful.  ", "the wind will be blowing through the valley today, bringing plenty of spring pollen. " };
            string[] springClearWeather = new string[] { "a nice spring day, perfect for all those outside chores! ", "clear and warm, it should be a perfect day. ", "it will be a nice clear spring day. " };

            string[] summerRainText = new string[] { "a warm rain is expected. ", "there will be a warm refreshing rain as a front passes by. ", "rain tommorow will bring a slightly cooler day. " };
            string[] summerStormText = new string[] { "expect storms throughout the day. ", "a cold front is expected to pass through, bringing through a squall line. ", "a warm front moving on from Calico Desert will cause storms as it nears the mountains. " };
            string[] summerClearWeather = new string[] { "it'll be a sweltering day. ", "another perfect sunny day, perfect for hitting the beach. ", "a hot and clear day dawns over Stardew Valley. " };

            string[] fallRainText = new string[] { "expect a cold rain as a low pressure front passes overhead. ", "moisture off the Gem Sea will make for a cold windy rain. " };
            string[] fallStormText = new string[] { "expect storms throughout the day. ", "it'll be a cold and stormy day. ", "a variable pressure front coming down from the north will bring storms with it. "};
            string[] fallWindText = new string[] { "it'll be a blustry cold day outside . ", "expect blowing leaves - a cold front will be passing through. " };
            string[] fallClearWeather = new string[] { "a cold day in the morning, with a warmer afternoon - clear. ", "another autumn day in eastern Ferngill, expect a chilly and clear day. " };

            string[] winterSnowText = new string[] { "winter continues it's relentless assualt - expect snow. ", "moisture blowing off the Gem Sea - expecting snowfall for the Stardew Valley, more in the mountains. ", "a curtain of white will descend on the valley starting at Point Drake. " };
            string[] winterClearWeather = new string[] { "it'll be a clear cold day. ", "a cold winter day - keep warm! ", "another chilly clear day over the Valley as a High pressure moves overhead. " };
			
			string nextDayIsNextSeason = "it'll be a fine day for the first day of";
            string nextDayIsNewYear = "another year has come to an end, and we will greet the next year with a sunny spring day!";

            string[] weddingWeather = new string[] { "it'll be good weather for a Pelican Town Wedding! Congratulations to the newlyweds. " };
            if ((int)weather == Game1.weather_festival)
                return "It'll be good weather for the " + InternalUtility.GetFestivalName(Game1.dayOfMonth + 1, Game1.currentSeason) + "! Sunny and clear.";

            if (today && weather == SDVWeather.Wedding)
                return weddingWeather.GetRandomItem(dice);

            if (Game1.countdownToWedding == 1 && !today)//fixes wedding forecast not properly stated. 
                return weddingWeather.GetRandomItem(dice);

            if (Game1.dayOfMonth == 28 && Game1.currentSeason != "winter") //some customization for next day is a new season
				return nextDayIsNextSeason + " " + InternalUtility.GetNewSeason(Game1.currentSeason);

			if (Game1.dayOfMonth == 28 && Game1.currentSeason == "winter") //end of year message.
				return nextDayIsNewYear;

            //spring
            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_debris)
                return springWindText.GetRandomItem(dice);

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_sunny)
                return springClearWeather.GetRandomItem(dice);

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_lightning)
                return springStormText.GetRandomItem(dice);

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_rain)
                return springRainText.GetRandomItem(dice);

            //summer
            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_sunny)
                return summerClearWeather.GetRandomItem(dice);

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_lightning)
                return summerStormText.GetRandomItem(dice);

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_rain)
                return summerRainText.GetRandomItem(dice);

            //fall
            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_debris)
                return fallWindText.GetRandomItem(dice);

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_sunny)
                return fallClearWeather.GetRandomItem(dice);

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_lightning)
                return fallStormText.GetRandomItem(dice);

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_rain)
                return fallRainText.GetRandomItem(dice);

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow && Game1.dayOfMonth == 27)
                return "Winter is just around the bend, with snow predicted for tommorow!";

            //winter
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                return winterClearWeather.GetRandomItem(dice);

            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_snow)
                return winterSnowText.GetRandomItem(dice);

            //error!
            logger.Log("This function has reached an error. It is being called for " + (today? "Today" : "Tommorow") + ". Current season is " + Game1.currentSeason + " and the internal weather is " + weather + " with the game weather flags being: Raining " + Game1.isRaining + " , Windy " + Game1.isDebrisWeather + " ,Stormy " + Game1.isLightning + " ,and Snowy " + Game1.isSnowing + " with tommorow's weather: " + DescWeather(Game1.weatherForTomorrow), LogLevel.Error);

            return "Angry suns descend on us! Run! (ERROR)";
        }

        internal static bool WeatherForceDay(string currentSeason, int dayOfMonth, int year)
        {
            if (dayOfMonth == 1) //all day 1 are forced
                return true;
            if (Utility.isFestivalDay(dayOfMonth, currentSeason))
                return true;
            if (year == 1 && currentSeason == "spring" && (dayOfMonth == 2 || dayOfMonth == 3 || dayOfMonth == 4))
                return true;
            if (currentSeason == "summer" && (dayOfMonth == 13 || dayOfMonth == 26))
                return true;

            return false;
        }
    }
}
