using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;

namespace TwilightShards.ClimatesOfFerngill.Components
{
    public class CurrentWeather
    {
        public double HighTemp { private set; get; }
        public double LowTemp { private set; get; }
        public BaseWeathers CurrentCondtions;
        public bool IsFoggy { private set; get; }
        public RainLevel CurrentRainLevel { private set; get; }

        

    }
}
