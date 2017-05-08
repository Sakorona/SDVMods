using NPack;
using StardewModdingAPI;
using StardewValley;

namespace ClimateOfFerngill
{
    static class WeatherHelper
    {
        public static string DescWeather(int weather, string season)
        {
            switch (weather)
            {
                case 0:
                    return "Sunny";
                case 1:
                    return "Rainy";
                case 2:
                    if (season != "winter")
                        return "Windy with" + (season == "spring" ? " pollen in the air" : " leaves blowing in the wind");
                    else 
                        return "Flurries";
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

        public static string DescWeather(SDVWeather weather, string season)
        {
            switch (weather)
            {
                case SDVWeather.Sunny:
                    return "Sunny";
                case SDVWeather.Rainy:
                    return "Rainy";
                case SDVWeather.Debris:
                    return "Windy with" + (season == "spring" ? " pollen in the air" : " leaves blowing in the wind");
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

        public static string GetWeatherDesc(TVStrings ourText, MersenneTwister dice, SDVWeather weather, bool today, IMonitor logger, bool debugFlag)
        {
            if (debugFlag)
                logger.Log($"[DESC] The weather tommorow at start is: {WeatherHelper.DescWeather(weather, Game1.currentSeason)}");

            if ((int)weather == Game1.weather_festival)
                return "It'll be good weather for the " + InternalUtility.GetTommorowFestivalName() + "! Sunny and clear.";

            if (today && weather == SDVWeather.Wedding)
                return ourText.weddingWeather.GetRandomItem(dice) + " ";

            if (Game1.countdownToWedding == 1 && !today)//fixes wedding forecast not properly stated. 
                return ourText.weddingWeather.GetRandomItem(dice) + " ";

            if (Game1.dayOfMonth == 28 && Game1.currentSeason != "winter") //some customization for next day is a new season
				return ourText.nextDayIsNextSeason + " " + InternalUtility.GetNewSeason(Game1.currentSeason) + " ";

			if (Game1.dayOfMonth == 28 && Game1.currentSeason == "winter") //end of year message.
				return ourText.nextDayIsNewYear + " ";

            //spring
            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_debris)
                return ourText.springWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_sunny)
                return ourText.springClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_snow)
                return ourText.springSnowText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_lightning)
                return ourText.springStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_rain)
                return ourText.springRainText.GetRandomItem(dice) + " ";

            //summer
            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_sunny)
                return ourText.summerClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_lightning)
                return ourText.summerStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_rain)
                return ourText.summerRainText.GetRandomItem(dice) + " ";

            //fall
            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_debris)
                return ourText.fallWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_sunny)
                return ourText.fallClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_lightning)
                return ourText.fallStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_rain)
                return ourText.fallRainText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow)
                return ourText.fallSnowText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow && Game1.dayOfMonth == 27)
                return "Winter is just around the bend, with snow predicted for tommorow!";

            //winter
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                return ourText.winterClearText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                return ourText.winterWindText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_snow)
                return ourText.winterSnowText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_rain)
                return ourText.winterRainText.GetRandomItem(dice) + " ";

            //error!
            logger.Log($"The weather desc has reached an error. It is being called for {(today? "Today" : "Tommorow")}." +
                       $"Current season is {Game1.currentSeason} and the internal weather is {weather}" +
                       $" with the game weather flags being: Raining: {Game1.isRaining}, Windy: {Game1.isDebrisWeather}," +
                       $" Stormy: {Game1.isLightning}, and Snowy: {Game1.isSnowing} with tommorow's weather: "
                       + DescWeather(Game1.weatherForTomorrow, Game1.currentSeason), LogLevel.Error);

            return "Angry suns descend on us! Run! (ERROR)";
        }



 
    }
}
