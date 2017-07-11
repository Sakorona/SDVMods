namespace ClimatesOfFerngillRebuild
{
    class WeatherConfig
    {
        //required options
        public string ClimateType { get; set; }
        public double ThundersnowOdds { get; set; }
        public bool Verbose { get; set; }
        
        public WeatherConfig()
        {
            //set climate type
            ClimateType = "normal";
            ThundersnowOdds = .10;
            Verbose = true;
        }
    }
}
