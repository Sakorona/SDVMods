using StardewValley;
using System.Collections.Specialized;
using System.Linq;

namespace ClimatesOfFerngillRebuild
{
    internal static class WeatherUtilities
    {
        internal static OrderedDictionary RainCategories = new OrderedDictionary()
        {
            { "None", 0 },
            { "Sunshower", 7 },
            { "Light", 35 },
            { "Normal", 70 },
            { "Heavy", 225 },
            { "Torrential", 550 },
            { "Hurricane", 1100 }
        };

        internal static void SetWeatherRain()
        {
            Game1.isSnowing = Game1.isLightning = Game1.isDebrisWeather = false;
            Game1.isRaining = true;
            Game1.debrisWeather.Clear();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("ThunderFrenzy").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Sandstorm").First().EndWeather();
        }

        internal static void SetWeatherStorm()
        {
            Game1.isSnowing = Game1.isDebrisWeather = false;
            Game1.isLightning = Game1.isRaining = true;
            Game1.debrisWeather.Clear();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Sandstorm").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
        }

        internal static void SetWeatherSnow()
        {
            Game1.isRaining = Game1.isLightning = Game1.isDebrisWeather = false;
            Game1.isSnowing = true;
            Game1.debrisWeather.Clear();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("ThunderFrenzy").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Sandstorm").First().EndWeather();
        }

        internal static void SetWeatherDebris()
        {
            Game1.isSnowing = Game1.isLightning = Game1.isRaining = false;
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("ThunderFrenzy").First().EndWeather();
            Game1.isDebrisWeather = true;
            Game1.populateDebrisWeatherArray();
        }

        internal static void SetWeatherSunny()
        {
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Blizzard").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Fog").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("WhiteOut").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("ThunderFrenzy").First().EndWeather();
            ClimatesOfFerngill.Conditions.GetWeatherMatchingType("Sandstorm").First().EndWeather();
            Game1.debrisWeather.Clear();
            Game1.isSnowing = Game1.isLightning = Game1.isRaining = Game1.isDebrisWeather = false;
        }

        internal static int GetWeatherCode()
        {
            if (!Game1.isRaining)
            {
                if (Game1.isDebrisWeather) return Game1.weather_debris;
                else if (Game1.isSnowing) return Game1.weather_snow;
                else return Game1.weather_sunny;
            }
            else
            {
                if (Game1.isLightning) return Game1.weather_lightning;
                else
                    return Game1.weather_rain;
            }
        }

        internal static int GetNextHighestRainCategoryBeginning(int currentRain)
        {
            for(int i = 0; i < RainCategories.Count; i++)
            {
                if ((int)RainCategories[i] == currentRain && i+1 < RainCategories.Count)
                    return (int)RainCategories[i+1];
                else if ((int)RainCategories[i] == currentRain && i + 1 == RainCategories.Count)
                    return (int)RainCategories[i];
                else if ((int)RainCategories[i] > currentRain)
                    return (int)RainCategories[i];
            }
            
            return currentRain;      
        }      
    }
}
