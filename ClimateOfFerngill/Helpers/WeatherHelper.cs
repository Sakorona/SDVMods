using NPack;
using StardewModdingAPI;
using StardewValley;

namespace ClimateOfFerngill
{
    static class WeatherHelper
    {
        public static string DescWeather(SDVWeather weather, string season)
        {
            switch (weather)
            {
                case SDVWeather.Sunny:
                    return "Sunny";
                case SDVWeather.Rainy:
                    return "Rainy";
                case SDVWeather.Debris:
                    return "Windy with" + (season == "spring" ? " pollen in the air" : (season == "fall" ? " leaves blowing in the wind" : "flurries of snow"));
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

        public static string GetWeatherDesc(TVStrings OurText, MersenneTwister dice, SDVWeather weather, FerngillWeather conditions, 
            bool today, IMonitor logger, bool debugFlag)
        {
            string ret = "";
            if (today)
                ret = "It is ";
            else
                ret = "it will be ";

            if (debugFlag)
                logger.Log($"[DESC] The weather Tomorrow at start is: {WeatherHelper.DescWeather(weather, Game1.currentSeason)}");

            if ((int)weather == Game1.weather_festival)
                return "It'll be good weather for the " + InternalUtility.GetTomorrowFestivalName() + "! Sunny and clear.";


            if (today && weather == SDVWeather.Wedding)
                ret += OurText.WeddingWeather.GetRandomItem(dice) + " ";

            if (Game1.countdownToWedding == 1 && !today)//fixes wedding forecast not properly stated. 
                ret += OurText.WeddingWeather.GetRandomItem(dice) + " ";

            if (Game1.dayOfMonth == 28 && Game1.currentSeason != "winter") //some customization for next day is a new season
                ret += OurText.NextDayIsNextSeason + " " + InternalUtility.GetNewSeason(Game1.currentSeason) + " ";

			if (Game1.dayOfMonth == 28 && Game1.currentSeason == "winter") //end of year message.
                ret += OurText.NextDayIsNewYear + " ";

            //spring
            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_debris)
                ret += OurText.SpringWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_sunny)
                ret += OurText.SpringClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_snow)
                ret += OurText.SpringSnowText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_lightning)
                ret += OurText.SpringStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "spring" && (int)weather == Game1.weather_rain)
                ret += OurText.SpringRainText.GetRandomItem(dice) + " ";

            //summer
            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_sunny)
                ret += OurText.SummerClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_lightning)
                ret += OurText.SummerStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_lightning)
                ret += OurText.SummerWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "summer" && (int)weather == Game1.weather_rain)
                ret += OurText.SummerRainText.GetRandomItem(dice) + " ";

            //fall
            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_debris)
                ret += OurText.FallWindText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_sunny)
                ret += OurText.FallClearText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_lightning)
                ret += OurText.FallStormText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_rain)
                ret += OurText.FallRainText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow)
                ret += OurText.FallSnowText.GetRandomItem(dice) + " ";

            if (Game1.currentSeason == "fall" && (int)weather == Game1.weather_snow && Game1.dayOfMonth == 27)
                ret += "Winter is just around the bend, with snow predicted for Tomorrow!";

            //winter
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                ret += OurText.WinterClearText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_sunny)
                ret += OurText.WinterWindText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_snow)
                ret += OurText.WinterSnowText.GetRandomItem(dice) + " ";
            if (Game1.currentSeason == "winter" && (int)weather == Game1.weather_rain)
                ret += OurText.WinterRainText.GetRandomItem(dice) + " ";

            //token replace
            ret.Replace("[high]", conditions.GetTodayHighInScale().ToString("{0:0.00}"));
            ret.Replace("[high_scale]", conditions.GetTempScale().ToString());
            ret.Replace("[low]", conditions.GetTodayLowInScale().ToString("{0:0.00}"));
            ret.Replace("[low_scale]", conditions.GetTempScale().ToString());

            return ret;
        } 
    }
}
