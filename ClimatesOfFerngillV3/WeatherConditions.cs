namespace ClimatesOfFerngillV3
{
    internal class WeatherConditions
    {
        public double HighTemp, LowTemp, RainTotals;
        public int HighDayTime, LowDayTime, RainPerHour;
        
        public WeatherConditions()
        {

        }

        public void Reset()
        {
            HighTemp = LowTemp = 0;
            HighDayTime = LowDayTime = 0;
            RainTotals = RainPerHour = 0;
        }

    }
}
