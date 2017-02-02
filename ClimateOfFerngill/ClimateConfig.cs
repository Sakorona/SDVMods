using StardewModdingAPI;

namespace ClimatesOfFerngill
{
    public class ClimateConfig
    {
        public bool SuppressLog { get; set; }
        public bool AllowSnowOnFall28 { get; set; }
        public bool AllowStormsFirstSpring { get; set; }
        public int NoLongerDisplayToday { get; set; }
        public string ClimateType { get; set; }
        public string TempGauge { get; set; }
        public bool StormyPenalty { get; set; }
        public int StaminaPenalty { get; set; }
        public bool tooMuchInfo { get; set; }


        //Future time!
        public bool HarshWeather { get; set; }


        public ClimateConfig()
        {
            //set defaults for spring weather
 
            AllowSnowOnFall28 = true;
            AllowStormsFirstSpring = false;
            SuppressLog = false;
            NoLongerDisplayToday = 1700;
            ClimateType = "normal";
            TempGauge = "celsius";
            StormyPenalty = true;
            StaminaPenalty = 1;
            tooMuchInfo = false; //debug string
            HarshWeather = false; //by default, turn the harsh events off. 

            //should we cap and min cap these?
        }
    }
}
