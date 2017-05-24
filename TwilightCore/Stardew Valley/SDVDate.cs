using StardewValley;
namespace TwilightCore.StardewValley
{
    public class SDVDate
    {
        public string Season { get; set; }
        public int Day { get; set; }

        public static SDVDate Today { get => new SDVDate(Game1.currentSeason, Game1.dayOfMonth); }
        public static SDVDate Tomorrow { get => GetTomorrowInGame(); }

        public SDVDate(string s, int d)
        {
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
                season = SDVUtilities.GetNextSeason(Game1.currentSeason);
            }
            else
            {
                season = Game1.currentSeason;
                day = Game1.dayOfMonth + 1;
            }

            return new SDVDate(season, day);
        }

    }
}
