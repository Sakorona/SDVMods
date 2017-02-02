namespace ClimateOfFerngill
{
    public class FerngillWeather
    {
        public int todayHigh { get; set; }
        public int status { get; set; }
        public int todayLow { get; set; }

        public static int BLIZZARD = 101;
        public static int HEATWAVE = 102;

        public void AlterTemps(int temp)
        {
            todayHigh = todayHigh + temp;
            todayLow = todayLow + temp;
        }

        public void GetLowFromHigh(int temp, int lowCap = 0)
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
    }
}
