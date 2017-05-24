using System;
using StardewValley;

namespace TwilightCore.StardewValley
{
    public class SDVDate
    {
        public string Season { get; set; }
        public int Day { get; set; }

        public static string[] Seasons = new string[]
        {
            "spring",
            "summer",
            "fall",
            "winter"
        };

        public static SDVDate Today { get => new SDVDate(Game1.currentSeason, Game1.dayOfMonth); }
        public static SDVDate Tomorrow { get => GetTomorrowInGame(); }

        public SDVDate(string s, int d)
        {
            s = s.ToLower();

            if (!Seasons.Contains(s))
                throw new ArgumentOutOfRangeException("The season must be one of the four seasons");

            if (d > 28)
                throw new ArgumentOutOfRangeException("The day can be no larger than 28");

            Season = s;
            Day = d;
        }

        public override bool Equals(object obj)
        {
            if (obj is SDVDate i)
            {
                if (i.Season == Season && i.Day == Day)
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SDVDate s1, SDVDate s2)
        {
            if (s1.Season == s2.Season && s1.Day == s2.Day)
                return true;
            else
                return false;
        }

        public static SDVDate operator -(SDVDate s1, int s2)
        {
            int day = s1.Day - s2;
            string season = s1.Season;
            while (day < 1)
            {
                season = SDVDate.GetPrevSeason(season);
                day += 28;
            }

            return new SDVDate(season, day);
        }

        public static SDVDate operator +(SDVDate s1, int s2)
        {
            int day = s1.Day + s2;
            string season = s1.Season;
            while (day > 28)
            {
                season = SDVDate.GetNextSeason(season);
                day -= 28;
            }

            return new SDVDate(season, day);
        }

        public static bool operator !=(SDVDate s1, SDVDate s2)
        {
            if (s1.Season == s2.Season && s1.Day == s2.Day)
                return false;
            else
                return true;
        }

        public static SDVDate GetTomorrowInGame()
        {
            int day = 1;
            string season = "spring";

            if (Game1.dayOfMonth == 28)
            {
                day = 1;
                season = SDVDate.GetNextSeason(Game1.currentSeason);
            }
            else
            {
                season = Game1.currentSeason;
                day = Game1.dayOfMonth + 1;
            }

            return new SDVDate(season, day);
        }

        public static SDVDate GetNextDay(SDVDate current)
        {
            int day = 1;
            string season = "spring";

            if (current.Day == 28)
            {
                day = 1;
                season = SDVDate.GetNextSeason(current.Season);
            }
            else
            {
                season = current.Season;
                day = current.Day + 1;
            }

            return new SDVDate(season, day);
        }

        public static SDVDate GetNextDay(string season, int day)
        {
            if (day == 28)
            {
                day = 1;
                season = SDVDate.GetNextSeason(season);
            }
            else
            {
                day = day + 1;
            }

            return new SDVDate(season, day);
        }


        public static string GetNextSeason(string currentSeason)
        {
            for (int i = 0; i < Seasons.Length; i++)
            {
                if (Seasons[i] == currentSeason)
                {
                    if (i + 1 < Seasons.Length)
                        return Seasons[i + 1];
                    else
                        return Seasons[0];
                }
            }

            return "Error";
        }

        public static string GetPrevSeason(string currentSeason)
        {
            for (int i = 0; i < Seasons.Length; i++)
            {
                if (Seasons[i] == currentSeason)
                {
                    if (i - 1 >= 0)
                        return Seasons[i - 1];
                    else
                        return Seasons[Seasons.Length - 1];
                }
            }

            return "Error";
        }

        public override string ToString()
        {
            return $"{Season} {Day}";
        }

    }
}
