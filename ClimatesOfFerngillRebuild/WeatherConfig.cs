namespace ClimatesOfFerngillRebuild
{
    class WeatherConfig
    {
        //required options
        public string ClimateType { get; set; }
        
        public WeatherConfig()
        {
            //set climate type
            ClimateType = "normal";
        }
    }
}
