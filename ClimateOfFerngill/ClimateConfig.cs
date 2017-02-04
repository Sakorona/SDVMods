
namespace ClimateOfFerngill
{
    public class ClimateConfig
    {
        public bool SuppressLog { get; set; }
        public bool AllowSnowOnFall28 { get; set; }
        public bool AllowStormsFirstSpring { get; set; }
        public int NoLongerDisplayToday { get; set; }
        public string ClimateType { get; set; }
        public string TempGauge { get; set; }
        public bool DisplaySecondScale { get; set; }
        public string SecondScaleGauge { get; set; }
        public bool StormyPenalty { get; set; }
        public int StaminaPenalty { get; set; }
        public bool tooMuchInfo { get; set; }
        public bool HarshWeather { get; set; }
        public int HeatwaveWarning { get; set; }
        public bool SetLowCap { get; set; }
        public int LowCap { get; set; }

        public ClimateConfig()
        {
            //set defaults for mod specific stuff
            SuppressLog = false;
            tooMuchInfo = false; 

            //set overrides
            AllowSnowOnFall28 = true;
            AllowStormsFirstSpring = false;
            NoLongerDisplayToday = 1700;

            //set climate information
            ClimateType = "normal";

            //set tv information
            TempGauge = "celsius";
            DisplaySecondScale = false;
            SecondScaleGauge = "";

            //set storm penalty stuff
            StormyPenalty = true;
            StaminaPenalty = 1;

            //set harsh weather events - currently unused.
            HarshWeather = false;
            HeatwaveWarning = 37;

            //set fall temp caps, if the plyer wants
            SetLowCap = false;
            LowCap = 1;
        }
    }
}
