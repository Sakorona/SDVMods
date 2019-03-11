using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimatesOfFerngillV3.ModelData
{
    class ClimatesSaveData
    {
        public double RainWithinLastWeek { get; set;}
        public int DaysSinceLastRain { get; set; }
        public bool WeatherSystemInProgress { get; set;}
        public int WeatherType { get ; set;}
        public int WeatherSystemDaysRemaining { get; set;}

    }
}
