using System.Collections.Generic;

namespace ClimateOfFerngill
{
    public class WeatherParameters
    {
        public string WeatherType;
        public double BaseValue;
        public double ChangeRate;
        public double VariableLowerBound;
        public double VariableHigherBound;

        public WeatherParameters()
        {

        }

        public WeatherParameters(string wType, double bValue, double cRate, double vLowerBound, double vHigherBound)
        {
            this.WeatherType = wType;
            this.BaseValue = bValue;
            this.ChangeRate = cRate;
            this.VariableLowerBound = vLowerBound;
            this.VariableHigherBound = vHigherBound;
        }

        public WeatherParameters(WeatherParameters c)
        {
            this.WeatherType = c.WeatherType;
            this.BaseValue = c.BaseValue;
            this.ChangeRate = c.ChangeRate;
            this.VariableLowerBound = c.VariableLowerBound;
            this.VariableHigherBound = c.VariableHigherBound;
        }
    }

    public class FerngillClimateTimeSpan
    {
        public string BeginSeason;
        public string EndSeason;
        public int BeginDay;
        public int EndDay;
        public List<WeatherParameters> WeatherChances;
       
        public FerngillClimateTimeSpan()
        {

        }

        public FerngillClimateTimeSpan(List<WeatherParameters> wp)
        {
            foreach (WeatherParameters w in wp)
            {
                this.WeatherChances.Add(new WeatherParameters(w));
            }
        }

        public FerngillClimateTimeSpan(FerngillClimateTimeSpan CTS)
        {
            foreach (WeatherParameters w in CTS.WeatherChances)
                this.WeatherChances.Add(new WeatherParameters(w));
        }

        public void AddWeatherChances(WeatherParameters wp)
        {
            WeatherChances.Add(new WeatherParameters(wp));
        }
    }

    public class FerngillClimate
    {
        public List<FerngillClimateTimeSpan> ClimateSequences;

        public FerngillClimate()
        {
            ClimateSequences = new List<FerngillClimateTimeSpan>();
        }

        public FerngillClimate(List<FerngillClimateTimeSpan> fCTS)
        {
            foreach (FerngillClimateTimeSpan CTS in fCTS)
                this.ClimateSequences.Add(new FerngillClimateTimeSpan(CTS));
        }
    }
}
