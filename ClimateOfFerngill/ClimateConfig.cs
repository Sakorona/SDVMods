
namespace ClimateOfFerngill
{
    public class ClimateConfig
    {
        public int HeatwaveTime { get; set; }

        public bool SuppressLog { get; set; }
        public bool AllowSnowOnFall28 { get; set; }
        public bool AllowStormsFirstSpring { get; set; }
        public int NoLongerDisplayToday { get; set; }

        public string ClimateType { get; set; }
        public string TempGauge { get; set; }
        public bool DisplaySecondScale { get; set; }
        public string SecondScaleGauge { get; set; }

        public bool StormyPenalty { get; set; }
        public double DiseaseChance { get; set; }
        public int StaminaPenalty { get; set; }

        public bool tooMuchInfo { get; set; }

        public bool HarshWeather { get; set; }

        public int HeatwaveWarning { get; set; }
        public int DeathTemp { get; set; }
        public int TimeToDie { get; set; }
        public bool AllowCropHeatDeath { get; set; }

        public int FrostWarning { get; set; }
        public double FrostHardiness { get; set; }
        public bool SetLowCap { get; set; }
        public int LowCap { get; set; }        

        //remove before rc1.
        public bool ForceHeat { get; set; }
        public bool ForceFrost { get; set; }

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
            DiseaseChance = .475;
            StaminaPenalty = 2;

            //set harsh weather events - currently unused.
            HarshWeather = false;
            HeatwaveTime = 1600;
            HeatwaveWarning = 37; //98.6F 
            FrostWarning = 2; //35.6F
            FrostHardiness = .45; // 45% of all crops will survive the frost wave.
            DeathTemp = 41; //105.8F
            AllowCropHeatDeath = false;
            TimeToDie = 310; // gives by default 3 hrs and 10 mins.

            //set fall temp caps, if the player wants
            SetLowCap = false;
            LowCap = 1;

            //debug options
            ForceHeat = false;
            ForceFrost = false;
        }
    }
}
