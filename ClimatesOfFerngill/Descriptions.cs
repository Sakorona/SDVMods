using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
{
    class Descriptions
    {
        public Descriptions()
        {
        }

        internal string GetDescOfDay(SDate date)
        {
            return ClimatesOfFerngill.Translator.Get("date" + GeneralFunctions.FirstLetterToUpper(date.Season) + date.Day);
        }

        internal string DescribeInGameWeather(int weather)
        {
            if (weather == Game1.weather_debris)
                return ClimatesOfFerngill.Translator.Get("weather_wind");
            if (weather == Game1.weather_festival)
                return ClimatesOfFerngill.Translator.Get("weather_festival");
            if (weather == Game1.weather_lightning)
                return ClimatesOfFerngill.Translator.Get("weather_lightning");
            if (weather == Game1.weather_rain)
                return ClimatesOfFerngill.Translator.Get("weather_rainy");
            if (weather == Game1.weather_snow)
                return ClimatesOfFerngill.Translator.Get("weather_snow");
            if (weather == Game1.weather_sunny)
                return ClimatesOfFerngill.Translator.Get("weather_sunny");
            if (weather == Game1.weather_wedding)
                return ClimatesOfFerngill.Translator.Get("weather_wedding");

            return "ERROR";
        }

        private string GetNextSeason(string season)
        {
            switch (season)
            {
                case "spring":
                    return "summer";
                case "summer":
                    return "fall";
                case "fall":
                    return "winter";
                case "winter":
                    return "spring";
                default:
                    return "error";
            }
        }

        private string GetTemperatureString(double temp)
        {
            //return a string based on what mod options are selected

            if (ClimatesOfFerngill.WeatherOpt.ShowBothScales && ClimatesOfFerngill.WeatherOpt.DisplayCelsiusInsteadOfKraggs)
            {
                return ClimatesOfFerngill.Translator.Get("temp-bothScalesCelsius", new { temp=temp.ToString("N1"), tempFaren=GeneralFunctions.ConvCtF(temp).ToString("N1") });
            }
            else if (ClimatesOfFerngill.WeatherOpt.ShowBothScales)
            {
                return ClimatesOfFerngill.Translator.Get("temp-bothScales", new { temp = temp.ToString("N1"), tempFaren = GeneralFunctions.ConvCtF(temp).ToString("N1") });
            }
            else if (ClimatesOfFerngill.WeatherOpt.SetDefaultScaleToF)
            {
                return ClimatesOfFerngill.Translator.Get("temp-farenOnly", new { temp = temp.ToString("N1")});
            }
            else if (ClimatesOfFerngill.WeatherOpt.DisplayCelsiusInsteadOfKraggs)
            {
                return ClimatesOfFerngill.Translator.Get("temp-celsiusSet", new { temp = temp.ToString("N1")});
            }

            return ClimatesOfFerngill.Translator.Get("temp-normal", new { temp = temp.ToString("N1")});
        }

        internal string UpperSeason(string season)
        {
            if (season == "spring") return "Spring";
            if (season == "winter") return "Winter";
            if (season == "fall") return "Fall";
            if (season == "summer") return "Summer";

            return "error";
        }

        internal string GenerateMenuPopup(WeatherConditions Current, string MoonPhase = "", string NightTime = "")
        {
            string text;
            if (SDate.Now().Season == "spring" && SDate.Now().Day == 1)
                text = ClimatesOfFerngill.Translator.Get("weather-menu.openingS1D1", new { descDay = ClimatesOfFerngill.Translator.Get($"date{UpperSeason(SDate.Now().Season)}{SDate.Now().Day}") }) + Environment.NewLine + Environment.NewLine;
            else if (SDate.Now().Season == "winter" && SDate.Now().Day == 28)
                text = ClimatesOfFerngill.Translator.Get("weather-menu.openingS4D28", new { descDay = ClimatesOfFerngill.Translator.Get($"date{UpperSeason(SDate.Now().Season)}{SDate.Now().Day}") }) + Environment.NewLine + Environment.NewLine;
            else
                text = ClimatesOfFerngill.Translator.Get("weather-menu.opening", new { descDay = ClimatesOfFerngill.Translator.Get($"date{UpperSeason(SDate.Now().Season)}{SDate.Now().Day}") }) + Environment.NewLine + Environment.NewLine;

            if (Current.ContainsCondition(CurrentWeather.Sandstorm))
                text += ClimatesOfFerngill.Translator.Get("weather-menu.condition.sandstorm") + Environment.NewLine;

            if (Current.ContainsCondition(CurrentWeather.Heatwave))
            {
                text += ClimatesOfFerngill.Translator.Get("weather-menu.condition.heatwave") + Environment.NewLine;
            }

            if (Current.ContainsCondition(CurrentWeather.Frost))
            { 
                text += ClimatesOfFerngill.Translator.Get("weather-menu.condition.frost") + Environment.NewLine;
            }

            if (Current.ContainsCondition(CurrentWeather.WhiteOut))
            {
                text += ClimatesOfFerngill.Translator.Get("weather-menu.condition.whiteOut") + Environment.NewLine;
            }

            if (Current.ContainsCondition(CurrentWeather.ThunderFrenzy))
            {
                text += ClimatesOfFerngill.Translator.Get("weather-menu.condition.thunderFrenzy") + Environment.NewLine;
            }

            if (MoonPhase == "Blood Moon")
            {
                text += ClimatesOfFerngill.Translator.Get("weather-menu.condition.bloodmoon") + Environment.NewLine;
            }

            ISDVWeather CurrentFog = Current.GetWeatherMatchingType("Fog").First();
            string fogString = "";

            //  If the fog is visible, we don't need to display fog information. However, if it's in the morning, 
            //    and we know evening fog is likely, we should display the message it's expected
            // That said, if it's not, we need to pull the fog information down, assuming it's been reset. This checks that the fog end
            //    time is *before* now. To avoid nested trinary statements..
            if (SDVTime.CurrentTime < CurrentFog.WeatherExpirationTime && Current.GenerateEveningFog && CurrentFog.WeatherBeginTime < new SDVTime(1200))
                fogString = ClimatesOfFerngill.Translator.Get("weather-menu.expectedFog");
            if (CurrentFog.WeatherBeginTime > SDVTime.CurrentTime && Current.GenerateEveningFog)
                fogString = ClimatesOfFerngill.Translator.Get("weather-menu.fogFuture",
                    new
                    {
                        fogTime = CurrentFog.WeatherBeginTime.ToString(),
                        endFog = CurrentFog.WeatherExpirationTime.ToString()
                    });

            //Current Conditions.
            text += ClimatesOfFerngill.Translator.Get("weather-menu.current", new
            {
                todayCondition = Current.HasWeather(CurrentWeather.Fog) ? ClimatesOfFerngill.Translator.Get("weather-menu.fog", new { condition = GetBasicWeather(Current, Game1.currentSeason), fogTime = CurrentFog.IsWeatherVisible ? CurrentFog.WeatherExpirationTime.ToString() : "" }) : GetBasicWeather(Current, Game1.currentSeason),

                todayHigh = GetTemperatureString(Current.TodayHigh),
                todayLow = GetTemperatureString(Current.TodayLow),
                fogString               
            }) + Environment.NewLine;

            //Tomorrow weather
            text += ClimatesOfFerngill.Translator.Get("weather-menu.tomorrow", 
                        new {
                            tomorrowCondition = GetBasicWeather(Game1.weatherForTomorrow, Game1.currentSeason),
                            tomorrowLow = GetTemperatureString(Current.TomorrowLow),
                            tomorrowHigh = GetTemperatureString(Current.TomorrowHigh)
                        }) + Environment.NewLine;

            //now, night time
            if (NightTime != "")
            {
                text += Environment.NewLine;
                text += NightTime + Environment.NewLine;
            }

            return text;
        }

        internal string GenerateTVForecast(WeatherConditions Current, string MoonPhase = "")
        {            
            //assemble params
            var talkParams = new Dictionary<string, string>
            {
                { "location", GetRandomLocation() },
                { "descWeather", GetWeather(Current, Game1.dayOfMonth, Game1.currentSeason) },
                { "festival", SDVUtilities.GetFestivalName(SDate.Now()) },
                { "festivalTomorrow", SDVUtilities.GetFestivalName(SDate.Now().AddDays(1)) },
                { "fogTime", Current.GetFogTime().ToString() },
                { "todayHigh", GetTemperatureString(Current.TodayHigh) },
                { "todayLow", GetTemperatureString(Current.TodayLow) },
                { "tomorrowWeather", GetWeather(Game1.weatherForTomorrow, Game1.dayOfMonth, Game1.currentSeason, true) },
                { "tomorrowHigh", GetTemperatureString(Current.TomorrowHigh) },
                { "tomorrowLow", GetTemperatureString(Current.TomorrowLow) },
                { "condWarning", GetCondWarning(Current) },
                { "condString", GetCondWarning(Current) },
                { "eveningFog", GetEveningFog(Current) }
            };

            //select the weather string for the TV.
            SDVTimePeriods CurrentPeriod = SDVTime.CurrentTimePeriod; //get the current time period
            int nRandom = ClimatesOfFerngill.Dice.Next(2);

            //blood moon checks
            if ((Game1.player.spouse != null && Game1.player.isEngaged() && Game1.player.friendshipData[Game1.player.spouse].CountdownToWedding == 1) && MoonPhase == "Blood Moon")
            {
                talkParams["tomrrowWeather"] = ClimatesOfFerngill.Translator.Get($"weat-{Game1.currentSeason}.sunny.{nRandom}");
                return ClimatesOfFerngill.Translator.Get("weat-wedTomorrow.BM.0", talkParams);
            }

            //festival tomorrow
            else if (SDVUtilities.GetFestivalName(SDate.Now().AddDays(1)) != "" && MoonPhase == "Blood Moon")
            {
                return ClimatesOfFerngill.Translator.Get("weat-fesTomorrow.BM.0", talkParams);
            }

            else if (MoonPhase == "Blood Moon")
            {
                return ClimatesOfFerngill.Translator.Get("weat-gen.bloodmoon.0", talkParams);
            }

            //first, check for special conditions -fog, festival, wedding
            else if (Current.HasWeather(CurrentWeather.Fog))
            {
                return ClimatesOfFerngill.Translator.Get($"weat-loc.fog.{nRandom}", talkParams);
            }

            //festival today
            else if (Current.HasWeather(CurrentWeather.Festival))
            {
                return ClimatesOfFerngill.Translator.Get("weat-fesToday.0", talkParams);
            }

            //festival tomorrow
            else if (SDVUtilities.GetFestivalName(SDate.Now().AddDays(1)) != "")
            {
                return ClimatesOfFerngill.Translator.Get("weat-fesTomorrow.0", talkParams);
            }

            //wedding today
            else if (Current.HasWeather(CurrentWeather.Wedding))
            {
                return ClimatesOfFerngill.Translator.Get("weat-wedToday.0", talkParams);
            }

            //wedding tomrrow
            else if (Game1.player.spouse != null && Game1.player.isEngaged() && Game1.player.friendshipData[Game1.player.spouse].CountdownToWedding == 1)
            {
                talkParams["tomrrowWeather"] = ClimatesOfFerngill.Translator.Get($"weat-{Game1.currentSeason}.sunny.{nRandom}");
                return ClimatesOfFerngill.Translator.Get("weat-wedTomorrow.0", talkParams);
            }

            if (ClimatesOfFerngill.Dice.NextDoublePositive() > .45)
            {
                if (CurrentPeriod == SDVTimePeriods.Morning)
                    return ClimatesOfFerngill.Translator.Get($"weat-morn.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Afternoon)
                    return ClimatesOfFerngill.Translator.Get($"weat-afternoon.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Evening)
                    return ClimatesOfFerngill.Translator.Get($"weat-evening.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Night)
                    return ClimatesOfFerngill.Translator.Get($"weat-night.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.Midnight)
                    return ClimatesOfFerngill.Translator.Get($"weat-midnight.{nRandom}", talkParams);
                else if (CurrentPeriod == SDVTimePeriods.LateNight)
                    return ClimatesOfFerngill.Translator.Get($"weat-latenight.{nRandom}", talkParams);
            }
            else
            {
                //ye olde generic!
                return ClimatesOfFerngill.Translator.Get($"weat-loc.{nRandom}", talkParams);
            }

            return "";
        }

        private string GetEveningFog(WeatherConditions Current)
        {
            if (Current.GenerateEveningFog)
            {
                var fList = Current.GetWeatherMatchingType("Fog");
                foreach (ISDVWeather weat in fList)
                {
                   if (weat is FerngillFog fWeat)
                    {
                        if (Current.GetWeatherMatchingType("Fog").First().IsWeatherVisible && (SDVTime.CurrentTime > new SDVTime(1200)))
                            return ClimatesOfFerngill.Translator.Get("weather-condition.fog", new { fogTime = fWeat.WeatherExpirationTime.ToString() });
                        else 
                        {
                            if (fWeat.WeatherBeginTime != fWeat.WeatherExpirationTime && fWeat.WeatherBeginTime > new SDVTime(1500))
                                return ClimatesOfFerngill.Translator.Get("weather-condition.evenFog", new { startTime = fWeat.WeatherBeginTime.ToString(), endTime = fWeat.WeatherExpirationTime.ToString() });
                            else if (fWeat.WeatherBeginTime != fWeat.WeatherExpirationTime)
                                return ClimatesOfFerngill.Translator.Get("weather-condition.eveningFogNoTime");
                            else
                                return "";
                        }
                    }
                }
                return "";
            }
            else
                return "";
        }

        private string GetCondWarning(WeatherConditions Current)
        {
            int rNumber = ClimatesOfFerngill.Dice.Next(2);

            if (Current.ContainsCondition(CurrentWeather.Sandstorm))
                return ClimatesOfFerngill.Translator.Get($"weather-condition.sandstorm.{rNumber}");

            if (Current.ContainsCondition(CurrentWeather.WhiteOut))
                return ClimatesOfFerngill.Translator.Get($"weather-condition.whiteout.{rNumber}");

            if (Current.ContainsCondition(CurrentWeather.ThunderFrenzy))
                return ClimatesOfFerngill.Translator.Get($"weather-condition.thunderfrenzy.{rNumber}");

            if (Current.ContainsCondition(CurrentWeather.Heatwave))
                return ClimatesOfFerngill.Translator.Get($"weather-condition.heatwave.{rNumber}");

            if (Current.ContainsCondition(CurrentWeather.Frost))
                return ClimatesOfFerngill.Translator.Get($"weather-condition.frost.{rNumber}");


            return "";            
        }

        private string GetWeather(int weather, int day, string season, bool TomorrowWeather = false)
        {
            int rNumber = ClimatesOfFerngill.Dice.Next(2);

            if (day == 28)
                season = GetNextSeason(season);

            if (weather == Game1.weather_debris)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.debris.{rNumber}");
            else if (weather == Game1.weather_festival || weather == Game1.weather_wedding || weather == Game1.weather_sunny && SDVTime.CurrentIntTime < Game1.getModeratelyDarkTime() && !TomorrowWeather)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.sunny_daytime.{rNumber}");
            else if (weather == Game1.weather_festival || weather == Game1.weather_wedding || weather == Game1.weather_sunny && SDVTime.CurrentIntTime >= Game1.getModeratelyDarkTime() && !TomorrowWeather)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.sunny_nighttime.{rNumber}");
            else if (weather == Game1.weather_festival || weather == Game1.weather_wedding || weather == Game1.weather_sunny && TomorrowWeather)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.sunny_daytime.{rNumber}");
            else if (weather == Game1.weather_lightning)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.stormy.{rNumber}");
            else if (weather == Game1.weather_rain)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.rainy.{rNumber}");
            else if (weather == Game1.weather_snow)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.snow.{rNumber}");

            return "ERROR";
        }

        private string GetBasicWeather(int weather, string season)
        {
            if (weather == Game1.weather_debris)
                return ClimatesOfFerngill.Translator.Get($"weather_wind");
            else if (weather == Game1.weather_festival || weather == Game1.weather_wedding)
                return ClimatesOfFerngill.Translator.Get($"weather_sunny");
            else if (weather == Game1.weather_lightning)
                return ClimatesOfFerngill.Translator.Get($"weather_lightning");
            else if (weather == Game1.weather_rain)
                return ClimatesOfFerngill.Translator.Get($"weather_rainy");
            else if (weather == Game1.weather_snow)
                return ClimatesOfFerngill.Translator.Get($"weather_snow");
            else if (weather == Game1.weather_sunny)
                return ClimatesOfFerngill.Translator.Get($"weather_sunny");

            return "ERROR";
        }

        private string GetBasicWeather(WeatherConditions Weather, string season)
        {
            if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconBlizzard || Weather.CurrentWeatherIconBasic == WeatherIcon.IconWhiteOut)
                return ClimatesOfFerngill.Translator.Get($"weather_blizzard");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSandstorm)
                return ClimatesOfFerngill.Translator.Get($"weather_sandstorm");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSpringDebris || Weather.CurrentWeatherIconBasic == WeatherIcon.IconDebris)
                return ClimatesOfFerngill.Translator.Get($"weather_wind");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconDryLightning)
                return ClimatesOfFerngill.Translator.Get($"weather_drylightning");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSunny && SDVTime.CurrentIntTime < Game1.getModeratelyDarkTime())
                return ClimatesOfFerngill.Translator.Get($"weather_sunny_daytime");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSunny && SDVTime.CurrentIntTime >= Game1.getModeratelyDarkTime())
                return ClimatesOfFerngill.Translator.Get($"weather_sunny_nighttime");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconStorm)
                return ClimatesOfFerngill.Translator.Get($"weather_lightning");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSnow)
                return ClimatesOfFerngill.Translator.Get($"weather_snow");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconRain)
                return ClimatesOfFerngill.Translator.Get($"weather_rainy");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconThunderSnow)
                return ClimatesOfFerngill.Translator.Get($"weather_thundersnow");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconWedding)
                return ClimatesOfFerngill.Translator.Get($"weather_wedding");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconFestival)
                return ClimatesOfFerngill.Translator.Get($"weather_festival");
            return "ERROR";
        }

        private string GetWeather(WeatherConditions Weather, int day, string season)
        {
            if (day == 28)
                season = GetNextSeason(season);

            int rNumber = ClimatesOfFerngill.Dice.Next(2);

            if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconBlizzard || Weather.CurrentWeatherIconBasic == WeatherIcon.IconWhiteOut)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.blizzard.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSandstorm)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.sandstorm.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSpringDebris || Weather.CurrentWeatherIconBasic == WeatherIcon.IconDebris)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.debris.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconDryLightning)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.drylightning.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconWedding || Weather.CurrentWeatherIconBasic == WeatherIcon.IconFestival || Weather.CurrentWeatherIconBasic == WeatherIcon.IconSunny && SDVTime.CurrentIntTime < Game1.getModeratelyDarkTime())
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.sunny_daytime.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSunny || Weather.CurrentWeatherIconBasic == WeatherIcon.IconWedding || Weather.CurrentWeatherIconBasic == WeatherIcon.IconFestival && SDVTime.CurrentIntTime >= Game1.getModeratelyDarkTime())
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.sunny_nighttime.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconStorm)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.stormy.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconSnow)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.snow.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconRain)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.rainy.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconThunderSnow)
                return ClimatesOfFerngill.Translator.Get($"weat-{season}.thundersnow.{rNumber}");
            else if (Weather.CurrentWeatherIconBasic == WeatherIcon.IconBloodMoon)
                return ClimatesOfFerngill.Translator.Get($"weat-bloodmoon");
            return "ERROR";
        }

        private string GetRandomLocation()
        {
            return ClimatesOfFerngill.Translator.Get("fern-loc." + ClimatesOfFerngill.Dice.Next(12));
        }

        internal TemporaryAnimatedSprite GetWeatherOverlay(WeatherConditions Current, TV tv)
        {
            Rectangle placement = new Rectangle(413, 333, 13, 13);

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
                    placement = (Game1.currentSeason.Equals("fall") ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13));
                    break;
                case WeatherIcon.IconSpringDebris:
                    placement = new Rectangle(465, 359, 13, 13);
                    break;
                case WeatherIcon.IconStorm:
                    placement = new Rectangle(413, 346, 13, 13);
                    break;
                case WeatherIcon.IconFestival:
                    placement = new Rectangle(413, 372, 13, 13);
                    break;
                case WeatherIcon.IconSnow:
                case WeatherIcon.IconBlizzard:
                case WeatherIcon.IconWhiteOut:
                    placement = new Rectangle(465, 346, 13, 13);
                    break;
            }

            return new TemporaryAnimatedSprite("LooseSprites\\Cursors", placement, 100f, 4, 999999, tv.getScreenPosition() + new Vector2(3f, 3f) * tv.getScreenSizeModifier(), false, false, (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 1.99999994947575E-05), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);
        }
    }
}
