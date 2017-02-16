using System;
using NPack;
using StardewValley;

namespace ClimateOfFerngill
{
    static class WeatherHelper
    {
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
                double tmpTemp = (temp + 273.15) * (9 / 5);
                return string.Format("{0:0.00}", tmpTemp) + " Ra";
            }

            if (tempGauge == "fahrenheit")
            {
                double tmpTemp = temp * (9 / 5) + 32;
                return string.Format("{0:0.00}", tmpTemp) + " F";
            }

            if (tempGauge == "romer")
            {
                return string.Format("{0:0.00}", (temp * (40 / 21) + 7.5)) + " Ro";
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


        public static string GetWeatherDesc(MersenneTwister dice, SDVWeather weather)
        {

            string[] springRainText = new string[] { "It'll be a rainy day outside! Make sure to bring your coat. ", "It'll be a wet day outside. ", "It'll be a misty, wet day - make sure to pause when you can and enjoy it! " };
            string[] springStormText = new string[] { "Early showers bring summer flowers! It'll be stormy outside. ", "Expect some lightning  outside -  be careful! ", "A storm front is blowing over the region, bringing rain and lightning. " };
            string[] springWindText = new string[] { "It'll be a blustery day outside. ", "A cold front is blowing through - if you have allergies, be careful. ", "The wind will be blowing through . " };
            string[] springClearWeather = new string[] { "A nice spring day, perfect for all those outside chores! ", "Clear and warm, it should be a perfect day. " };

            string[] summerRainText = new string[] { "A warm rain is expected. ", "There will be a warm refreshing rain as a front passes by. " };
            string[] summerStormText = new string[] { "Expect storms throughout the day. ", "A cold front is expected to pass through, bringing through a squall line. " };
            string[] summerClearWeather = new string[] { "It'll be a sweltering day. ", "Another perfect sunny day, perfect for hitting the beach.", "A hot and clear day dawns over the Valley. " };

            string[] fallRainText = new string[] { "Expect a cold rain as a low pressure goes overhead. ", "Moisture off the Gem Sea will make for a cold windy rain. " };
            string[] fallStormText = new string[] { "Expect storms throughout the day. ", "It'll be a cold and stormy day . " };
            string[] fallWindText = new string[] { "It'll be a blustry cold day outside . ", "Expect blowing leaves - a cold front will be passing through. " };
            string[] fallClearWeather = new string[] { "A cold day in the morning, with a warmer afternoon - clear. ", "Another autumn day in eastern Ferngill, expect a chilly and clear day. " };

            string[] winterSnowText = new string[] { "Winter continues it's relentless assualt - expect snow. ", "Moisture blowing off the Gem Sea - expecting snowfall for the Stardew Valley, more in the mountains. ", "A curtain of white will descend on the valley. " };
            string[] winterClearWeather = new string[] { "It'll be a clear cold day . ", "A cold winter day - keep warm!", "Another chilly clear day over the Valley as a High pressure moves overhead. " };
			
			string nextDayIsNextSeason = "It'll be a fine day for the first day of";
			string nextDayIsNewYear = "Another year has come to an end, and we will greet the next year with a sunny spring day!"

            string[] festivalWeather = new string[] { "It'll be good weather for the festival! Sunny and clear. " };
            string[] weddingWeather = new string[] { "It'll be good weather for a Pelican Town Wedding! Congratuatlions to the newlyweds. " };
            if ((int)weather == Game1.weather_festival)
                return festivalWeather.GetRandomItem(dice);

            if ((int)weather == Game1.weather_wedding)
                return festivalWeather.GetRandomItem(dice);
			
			if (Game1.dayofMonth == 28 && Game1.currentSeason != "winter") //some customization for next day is a new season
				return nextDayIsNextSeason + " " + Game1.currentSeason;
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
