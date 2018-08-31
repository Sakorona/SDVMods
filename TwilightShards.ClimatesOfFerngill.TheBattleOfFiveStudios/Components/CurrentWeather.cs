using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilightShards.ClimatesOfFerngill.Components
{
    public class CurrentWeather
    {
        private double highTemp;
        private double lowTemp;
        public BaseWeathers CurrentCondtions;
        public bool IsFoggy { get; private set; }
        public RainLevel CurrentRainLevel { get; private set; }

        public void UpdateCurrentRainLevel(int rainAmt)
        {
            //default rain amount in vanilla is 70, which is considered normal
            if (rainAmt <= 0)
                CurrentRainLevel = RainLevel.None;
            


        }

    }
}
