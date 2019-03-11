namespace ClimatesOfFerngillV3.ModelData
{
    public class ClimateLocations
    {
        public string LocationName;
        public double TemperatureChange;
        public string WeatherTypeChange;
        public double WeatherChanceChange;

        public ClimateLocations()
        {

        }

        public ClimateLocations(string loc, double tempC, string weatherT, double weatherC)
        {
            LocationName = loc;
            TemperatureChange = tempC;
            WeatherTypeChange = weatherT;
            WeatherChanceChange = weatherC;
        }
    }
}
