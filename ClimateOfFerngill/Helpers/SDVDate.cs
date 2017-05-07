namespace ClimateOfFerngill
{
    public struct SDVDate
    {
        public string Season { get; set; }
        public int Day { get; set; }

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

    }
}
