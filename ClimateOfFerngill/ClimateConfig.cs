using StardewModdingAPI;

namespace ClimatesOfFerngill
{
    public class ClimateConfig
    {
        public bool SuppressLog { get; set; }
        public bool AllowSnowOnFall28 { get; set; }
        public bool AllowStormsFirstSpring { get; set; }

        //Future time!
        public bool HarshWeather { get; set; }


        public ClimateConfig()
        {
            //set defaults for spring weather
 
            AllowSnowOnFall28 = true;
            AllowStormsFirstSpring = false;
            SuppressLog = false;

            HarshWeather = false; //by default, turn the harsh events off. 

            //should we cap and min cap these?
        }
    }
}
