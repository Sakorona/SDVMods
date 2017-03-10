namespace ClimateOfFerngill
{
    public class FerngillWeather
    {
        public double TodayHigh { get; set; }
        public int Status { get; set; }
        public double TodayLow { get; set; }

        public static int BLIZZARD = 101;
        public static int HEATWAVE = 102;
        public static int FROST = 103;

        public void AlterTemps(int temp)
        {
            TodayHigh = TodayHigh + temp;
            TodayLow = TodayLow + temp;
        }

        public void GetLowFromHigh(double temp, int lowCap = 0)
        {
            TodayLow = TodayHigh - temp;
            if (lowCap != 0 && TodayLow < lowCap)
                TodayLow = lowCap;
        }

        public void Reset()
        {
            Status = 0;
            TodayHigh = -1000;
            TodayLow = -1000;
        }

        public override string ToString()
        {
            return "High: " + TodayHigh + " C and Low: " + TodayLow + " C";
        }
    }
}
