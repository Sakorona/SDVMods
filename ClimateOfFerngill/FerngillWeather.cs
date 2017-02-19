namespace ClimateOfFerngill
{
    public class FerngillWeather
    {
        public double todayHigh { get; set; }
        public int status { get; set; }
        public double todayLow { get; set; }

        public static int BLIZZARD = 101;
        public static int HEATWAVE = 102;
        public static int FROST = 103;

        public void AlterTemps(int temp)
        {
            todayHigh = todayHigh + temp;
            todayLow = todayLow + temp;
        }

        public void GetLowFromHigh(double temp, int lowCap = 0)
        {
            todayLow = todayHigh - temp;
            if (lowCap != 0 && todayLow < lowCap)
                todayLow = lowCap;
        }

        public void Reset()
        {
            status = 0;
            todayHigh = -1000;
            todayLow = -1000;
        }

        public override string ToString()
        {
            return "High: " + todayHigh + " C and Low: " + todayLow + " C";
        }
    }
}
