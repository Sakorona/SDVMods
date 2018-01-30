using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
{
    class Descriptions
    {
        private ITranslationHelper Helper;
        private MersenneTwister OurDice;
        private WeatherConfig ModConfig;
        private IMonitor Output;

        public Descriptions(ITranslationHelper Translaton, MersenneTwister mDice, WeatherConfig wc, IMonitor log)
        {
            Helper = Translaton;
            OurDice = mDice;
            ModConfig = wc;
            Output = log;
        }

        internal string GetDescOfDay(SDate date)
        {
            return Helper.Get("date" + GeneralFunctions.FirstLetterToUpper(date.Season) + date.Day);
        }

        internal string DescribeInGameWeather(int weather)
        {
            if (weather == Game1.weather_debris)
                return Helper.Get("weather_wind");
            if (weather == Game1.weather_festival)
                return Helper.Get("weather_festival");
            if (weather == Game1.weather_lightning)
                return Helper.Get("weather_lightning");
            if (weather == Game1.weather_rain)
                return Helper.Get("weather_rainy");
            if (weather == Game1.weather_snow)
                return Helper.Get("weather_snow");
            if (weather == Game1.weather_sunny)
                return Helper.Get("weather_sunny");
            if (weather == Game1.weather_wedding)
                return Helper.Get("weather_wedding");

            return "ERROR";
        }

        private string GetTemperatureString(double temp)
        {
            if (ModConfig.ShowBothScales)
            {
                //Temp =  "34 C (100 F)"
                return $"{temp.ToString("N1")} C ({GeneralFunctions.ConvCtF(temp).ToString("N1")} F)";
            }
            else
            {
                return $"{temp.ToString("N1")}";
            }
        }

        internal string UpperSeason(string season)
        {
            if (season == "spring") return "Spring";
            if (season == "winter") return "Winter";
            if (season == "fall") return "Fall";
            if (season == "summer") return "Summer";

            return "error";
        }

        internal string GenerateMenuPopup(WeatherConditions Current, SDVMoon Moon)
        {

            string text = Helper.Get("weather-menu.opening", new { descDay = Helper.Get($"date{UpperSeason(SDate.Now().Season)}{SDate.Now().Day}") }) + Environment.NewLine + Environment.NewLine;

            if (Current.ContainsCondition(CurrentWeather.Heatwave))
            {
                text += Helper.Get("weather-menu.condition.heatwave") + Environment.NewLine;
            }

            if (Current.ContainsCondition(CurrentWeather.Frost))
            {
                text += Helper.Get("weather-menu.condition.frost") + Environment.NewLine;
            }

            //Current Conditions.
            text += Helper.Get("weather-menu.current", new
            {
                todayCondition = (Current.HasWeather(CurrentWeather.Fog) ? Helper.Get("weather-menu.fog", new { condition = GetBasicWeather(Current, Game1.currentSeason) }) : GetBasicWeather(Current, Game1.currentSeason)),
               todayHigh = GetTemperatureString(Current.TodayHigh),
               todayLow = GetTemperatureString(Current.TodayLow),
               fogString = (Current.HasEveningFog? Helper.Get("weather-menu.fogFuture", 
                        new {
                            fogTime = Current.GetWeatherMatchingType("Fog").First().WeatherBeginTime.ToString(),
                            endFog = Current.GetWeatherMatchingType("Fog").First().WeatherExpirationTime.ToString()
                        }) 
                    : (Current.GetWeatherMatchingType("Fog").First().IsWeatherVisible? 
                    Helper.Get("weather-menu.fog", 
                        new {
                            fogTime = Current.GetWeatherMatchingType("Fog").First().IsWeatherVisible ?  
                            Current.GetWeatherMatchingType("Fog").First().WeatherExpirationTime.ToString(): ""
                        }) : "")
                  )
            }) + Environment.NewLine;
            text += Environment.NewLine;

            //Tomorrow weather
            text += Helper.Get("weather-menu.tomorrow", 
                new {
                    tomorrowCondition = GetBasicWeather(Game1.weatherForTomorrow, Game1.currentSeason),
                    tomorrowLow = GetTemperatureString(Current.TomorrowLow),
                    tomorrowHigh = GetTemperatureString(Current.TomorrowHigh)
                }) + Environment.NewLine;

            return text;
        }

        internal string GenerateTVForecast(WeatherConditions Current, SDVMoon Moon)
        {            
            //assemble params
            var talkParams = new Dictionary<string, string>
            {
                { "location", GetRandomLocation() },
                { "descWeather", GetWeather(Current, Game1.currentSeason) },
                { "festival", SDVUtilities.GetFestivalName(SDate.Now()) },
                { "festivalTomorrow", SDVUtilities.GetFestivalName(SDate.Now().AddDays(1)) },
                { "fogTime", Current.GetFogTime().ToString() },
                { "todayHigh", GetTemperatureString(Current.TodayHigh) },
                { "todayLow", GetTemperatureString(Current.TodayLow) },
                { "tomorrowWeather", GetWeather(Game1.weatherForTomorrow, Game1.currentSeason) },
                { "tomorrowHigh", GetTemperatureString(Current.TomorrowHigh) },
                { "tomorrowLow", GetTemperatureString(Current.TomorrowLow) },
                { "condWarning", GetCondWarning(Current) },
                { "eveningFog", GetEveningFog(Current) }
            };

            //select the weather string for the TV.
            SDVTimePeriods CurrentPeriod = SDVTime.CurrentTimePeriod; //get the current time period
            int nRandom = OurDice.Next(2);

            //first, check for special conditions -fog, festival, wedding
            if (Current.HasWeather(CurrentWeather.Fog))
            {
                return Helper.Get($"weat-loc.fog.{nRandom}", talkParams);
            }

            //festival today
            else if (Current.HasWeather(CurrentWeather.Festival))
            {
                return Helper.Get("weat-fesTomorrow.0", talkParams);
            }

            //festival tomrrow
            else if (SDVUtilities.GetFestivalName(SDate.Now().AddDays(1)) != "")
            {
                return Helper.Get("weat-fesTomorrow.0", talkParams);
            }

            //wedding today
            else if (Current.HasWeather(CurrentWeather.Wedding))
            {
                return Helper.Get("weat-wedToday.0", talkParams);
            }

            //wedding tomrrow
            else if (Game1.countdownToWedding == 1)
            {
                talkParams["tomrrowWeather"] = Helper.Get($"weat-{Game1.currentSeason}.sunny.{nRandom}");
                return Helper.Get("weat-wedTomorrow.0", talkParams);
            }

            if (OurDice.NextDoublePositive() > .45)
            {
                if (CurrentPeriod == SDVTimePeriods.Morning)
                    return Helper.Get($"weat-morn.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Afternoon)
                    return Helper.Get($"weat-afternoon.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Evening)
                    return Helper.Get($"weat-evening.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Night)
                    return Helper.Get($"weat-night.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Midnight)
                    return Helper.Get($"weat-midnight.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.LateNight)
                    return Helper.Get($"weat-latenight.{nRandom}", talkParams);
            }
            else
            {
                //ye olde generic!
                return Helper.Get($"weat-loc.{nRandom}", talkParams);
            }


            return "";
        }

        private string GetEveningFog(WeatherConditions Current)
        {
            if (Current.HasEveningFog)
            {
                var fList = Current.GetWeatherMatchingType("Fog");
                foreach (ISDVWeather weat in fList)
                {
                   if (weat is FerngillFog fWeat)
                    {
                        return Helper.Get("weather-condition.evenFog", new { startTime = fWeat.WeatherBeginTime.ToString(), endTime = fWeat.WeatherExpirationTime.ToString() });
                    }
                }

                return "";
            }
            else
                return "";
        }

        private string GetCondWarning(WeatherConditions Current)
        {
            int rNumber = OurDice.Next(2);
            if (Current.ContainsCondition(CurrentWeather.Heatwave))
            {
                return Helper.Get($"weather-condition.heatwave.{rNumber}");
            }

            if (Current.ContainsCondition(CurrentWeather.Frost))
            {
                return Helper.Get($"weather-condition.frost.{rNumber}");
            }

            return "";            
        }

        private string GetWeather(int weather, string season)
        {
            int rNumber = OurDice.Next(2);

            if (weather == Game1.weather_debris)
                return Helper.Get($"weat-{season}.debris.{rNumber}");
            else if (weather == Game1.weather_festival || weather == Game1.weather_wedding)
                return Helper.Get($"weat-{season}.sunny.{rNumber}");
            else if (weather == Game1.weather_lightning)
                return Helper.Get($"weat-{season}.stormy.{rNumber}");
            else if (weather == Game1.weather_rain)
                return Helper.Get($"weat-{season}.rainy.{rNumber}");
            else if (weather == Game1.weather_snow)
                return Helper.Get($"weat-{season}.snow.{rNumber}");
            else if (weather == Game1.weather_sunny)
                return Helper.Get($"weat-{season}.sunny.{rNumber}");

            return "ERROR";
        }

        private string GetBasicWeather(int weather, string season)
        {
            int rNumber = OurDice.Next(2);

            if (weather == Game1.weather_debris)
                return Helper.Get($"weather_wind");
            else if (weather == Game1.weather_festival || weather == Game1.weather_wedding)
                return Helper.Get($"weather_sunny");
            else if (weather == Game1.weather_lightning)
                return Helper.Get($"weather_lightning");
            else if (weather == Game1.weather_rain)
                return Helper.Get($"weather_rainy");
            else if (weather == Game1.weather_snow)
                return Helper.Get($"weather_snow");
            else if (weather == Game1.weather_sunny)
                return Helper.Get($"weather_sunny");

            return "ERROR";
        }

        private string GetBasicWeather(WeatherConditions Weather, string season)
        {
            int rNumber = OurDice.Next(2);

            if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconBlizzard)
                return Helper.Get($"weather_blizzard");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSpringDebris || Weather.CurrentWeatherIconBasic == WeatherIcon.IconDebris)
                return Helper.Get($"weather_wind");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconDryLightning)
                return Helper.Get($"weather_drylightning");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSunny)
                return Helper.Get($"weather_sunny");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconStorm)
                return Helper.Get($"weather_lightning");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSnow)
                return Helper.Get($"weather_snow");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconRain)
                return Helper.Get($"weather_rainy");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconThunderSnow)
                return Helper.Get($"weather_thundersnow");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconWedding)
                return Helper.Get($"weather_wedding");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconFestival)
                return Helper.Get($"weather_festival");
            return "ERROR";
        }


        private string GetWeather(WeatherConditions Weather, string season)
        {
            int rNumber = OurDice.Next(2);

            if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconBlizzard)
                return Helper.Get($"weat-{season}.blizzard.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSpringDebris || Weather.CurrentWeatherIconBasic == WeatherIcon.IconDebris)
                return Helper.Get($"weat-{season}.wind.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconDryLightning)
                return Helper.Get($"weat-{season}.drylightning.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSunny)
                return Helper.Get($"weat-{season}.sunny.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconStorm)
                return Helper.Get($"weat-{season}.stormy.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSnow)
                return Helper.Get($"weat-{season}.snow.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconRain)
                return Helper.Get($"weat-{season}.rainy.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconThunderSnow)
                return Helper.Get($"weat-{season}.thundersnow.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconWedding || Weather.CurrentWeatherIconBasic == WeatherIcon.IconFestival)
                return Helper.Get($"weat-{season}.sunny.{rNumber}");

            return "ERROR";
        }

        private string GetRandomLocation()
        {
            return Helper.Get("fern-loc." + OurDice.Next(12));
        }

        internal TemporaryAnimatedSprite GetWeatherOverlay(WeatherConditions Current, TV tv)
        {
            Rectangle placement = new Rectangle(413, 333, 13, 33);

            switch (Current.CurrentWeatherIconBasic)
            {
                case WeatherIcon.IconSunny:
                case WeatherIcon.IconWedding:
                case WeatherIcon.IconDryLightning:
                    placement = new Rectangle(413, 333, 13, 13);
                    break;
                case WeatherIcon.IconRain:
                    placement = new Rectangle(465, 333, 13, 13);
                    break;
                case WeatherIcon.IconDebris:
                    placement = Game1.currentSeason.Equals("spring") ? new Rectangle(465, 359, 13, 13) : (Game1.currentSeason.Equals("fall") ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13));
                    break;
                case WeatherIcon.IconStorm:
                    placement = new Rectangle(413, 346, 13, 13);
                    break;
                case WeatherIcon.IconFestival:
                    placement = new Rectangle(413, 372, 13, 13);
                    break;
                case WeatherIcon.IconSnow:
                case WeatherIcon.IconBlizzard:
                    placement = new Rectangle(465, 346, 13, 13);
                    break;
            }

            return new TemporaryAnimatedSprite(Game1.mouseCursors, placement, 100f, 4, 999999, tv.getScreenPosition() + new Vector2(3f, 3f) * tv.getScreenSizeModifier(), false, false, (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 1.99999994947575E-05), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);
        }
    }
}
