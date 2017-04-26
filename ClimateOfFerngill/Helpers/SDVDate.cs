namespace ClimateOfFerngill
{
    public class SDVDate
    {
        public string Season { get; set; }
        public int Day { get; set; }

        public SDVDate()
        {

        }

        public SDVDate(string s, int d)
        {
            Season = s;
            Day = d;
        }
    }
}
