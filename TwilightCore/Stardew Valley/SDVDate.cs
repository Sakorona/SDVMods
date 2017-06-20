using System;

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

        public static bool NotEquals(SDVDate s1, string s2, int d2)
        {
            return (s1 != new SDVDate(s2, d2));
        }

        public static bool Equals(SDVDate s1, string s2, int d2)
        {
            return (s1 == new SDVDate(s2, d2));
        }

        public static bool GreaterThan(SDVDate s1, string s2, int d2)
        {
            return (s1 > new SDVDate(s2, d2));
        }

        public static bool GreaterOrEqualsThan(SDVDate s1, string s2, int d2)
        {
            return (s1 >= new SDVDate(s2, d2));
        }

        public static bool LesserThan(SDVDate s1, string s2, int d2)
        {
            return (s1 < new SDVDate(s2, d2));
        }

        public static bool LesserOrEqualsThan(SDVDate s1, string s2, int d2)
        {
            return (s1 <= new SDVDate(s2, d2));
        }

        public static bool NotEquals(string s1, int d1, string s2, int d2)
        {
            return (new SDVDate(s1,d1) != new SDVDate(s2, d2));
        }

        public static bool Equals(string s1, int d1, string s2, int d2)
        {
            return (new SDVDate(s1, d1) == new SDVDate(s2, d2));
        }

        public static bool GreaterThan(string s1, int d1, string s2, int d2)
        {
            return (new SDVDate(s1,d1) > new SDVDate(s2, d2));
        }

        public static bool GreaterOrEqualsThan(string s1, int d1, string s2, int d2)
        {
            return (new SDVDate(s1,d1) >= new SDVDate(s2, d2));
        }

        public static bool LesserThan(string s1, int d1, string s2, int d2)
        {
            return (new SDVDate(s1,d1) < new SDVDate(s2, d2));
        }

        public static bool LesserOrEqualsThan(string s1, int d1, string s2, int d2)
        {
            return (new SDVDate(s1,d1) <= new SDVDate(s2, d2));
        }

        public static bool operator >(SDVDate s1, SDVDate s2)
        {
            //handle same season first.
            if (s1.Season == s2.Season && s1.Day > s2.Day)
                return true;
            else if (s1.Season == s2.Season && !(s1.Day > s2.Day))
                return false;

            //handle different season
            if (s1.Season == "winter" && s2.Season != "winter")
               return true;
            
            if (s1.Season == "fall" && (s2.Season == "summer" || s2.Season == "spring"))
                return true;
            
            if (s1.Season == "summer" && s2.Season == "spring")
               return true;
            
            return false;
        }

        public static bool operator <(SDVDate s1, SDVDate s2)
        {
            //handle same season first.
            if (s1.Season == s2.Season && s1.Day < s2.Day)
                return true;
            else if (s1.Season == s2.Season && !(s1.Day < s2.Day))
                return false;

            //handle different season
            if (s1.Season == "spring" && (s2.Season != "spring"))
                return true;

            if (s1.Season == "summer" && (s2.Season == "fall" || s2.Season == "winter"))
                return true;

            if (s1.Season == "fall" && s2.Season == "winter")
                return true;

            return false;
        }

        public static bool operator <=(SDVDate s1, SDVDate s2)
        {
            //handle same season first.
            if (s1.Season == s2.Season && s1.Day <= s2.Day)
                return true;
            else if (s1.Season == s2.Season && !(s1.Day <= s2.Day))
                return false;

            //handle different season
            if (s1.Season == "spring" && (s2.Season != "spring"))
                return true;

            if (s1.Season == "summer" && (s2.Season == "fall" || s2.Season == "winter"))
                return true;

            if (s1.Season == "fall" && s2.Season == "winter")
                return true;

            return false;
        }

        public static bool operator >=(SDVDate s1, SDVDate s2)
        {
            if (s1.Season == s2.Season && s1.Day >= s2.Day)
                return true;
            else if (s1.Season == s2.Season && !(s1.Day >= s2.Day))
                return false;

            //handle different season
            if (s1.Season == "winter" && s2.Season != "winter")
                return true;

            if (s1.Season == "fall" && (s2.Season == "summer" || s2.Season == "spring"))
                return true;

            if (s1.Season == "summer" && s2.Season == "spring")
                return true;

            return false;
        }

        public bool IsBetween(SDVDate lower, SDVDate higher)
        {
            if (this > lower && this < higher)
                return true;
            else
                return false;
        }

        public bool IsBetweenInc(SDVDate lower, SDVDate higher)
        {
            if (this >= lower && this <= higher)
                return true;
            else
                return false;
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
