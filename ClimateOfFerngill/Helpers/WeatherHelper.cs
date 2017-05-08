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
                case SDVWeather.Blizzard:
                    return "Blizzard";
                case SDVWeather.Thundersnow:
                    return "Thundersnow";
                default:
                    return "Weather not present in base game";
            }
        }



        public static string GetWeatherDesc(TVStrings ourText, MersenneTwister dice, SDVWeather weather, FerngillWeather conditions, 
            bool today, IMonitor logger, bool debugFlag)
        {
            string ret = "";
            if (today)
                ret = "It is ";
            else
                ret = "it will be ";

            if (debugFlag)
                logger.Log($"[DESC] The weather tommorow at start is: {WeatherHelper.DescWeather(weather, Game1.currentSeason)}");

            if ((int)weather == Game1.weather_festival)
                return "It'll be good weather for the " + InternalUtility.GetTommorowFestivalName() + "! Sunny and clear.";


            if (today && weather == SDVWeather.Wedding)
                ret += ourText.WeddingWeather.GetRandomItem(dice) + " ";

            if (Game1.countdownToWedding == 1 && !today)//fixes wedding forecast not properly stated. 
                ret += ourText.WeddingWeather.GetRandomItem(dice) + " ";

            if (Game1.dayOfMonth == 28 && Game1.currentSeason != "winter") //some customization for next day is a new season
                ret += ourText.NextDayIsNextSeason + " " + InternalUtility.GetNewSeason(Game1.currentSeason) + " ";

			if (Game1.dayOfMonth == 28 && Game1.currentSeason == "winter") //end of year message.
                ret += ourText.NextDayIsNewYear + " ";

            //spring
            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_debris)
                ret += ourText.SpringWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_sunny)
                ret += ourText.SpringClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_snow)
                ret += ourText.SpringSnowText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_lightning)
                ret += ourText.SpringStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_rain)
                ret += ourText.SpringRainText.GetRandomItem(dice) + " ";

            //summer
            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_sunny)
                ret += ourText.SummerClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_lightning)
                ret += ourText.SummerStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_lightning)
                ret += ourText.SummerWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_rain)
                ret += ourText.SummerRainText.GetRandomItem(dice) + " ";

            //fall
            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_debris)
                ret += ourText.FallWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_sunny)
                ret += ourText.FallClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_lightning)
                ret += ourText.FallStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_rain)
                ret += ourText.FallRainText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow)
                ret += ourText.FallSnowText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow && Game1.dayOfMonth == 27)
                ret += "Winter is just around the bend, with snow predicted for tommorow!";

            //winter
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                ret += ourText.WinterClearText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                ret += ourText.WinterWindText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_snow)
                ret += ourText.WinterSnowText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_rain)
                ret += ourText.WinterRainText.GetRandomItem(dice) + " ";

            //token replace
            ret.Replace("[high]", conditions.GetTodayHighInScale().ToString("{0:0.00}"));
            ret.Replace("[high_scale]", conditions.GetTempScale().ToString());
            ret.Replace("[low]", conditions.GetTodayLowInScale().ToString("{0:0.00}"));
            ret.Replace("[low_scale]", conditions.GetTempScale().ToString());

            return ret;
        }



 
    }
}
