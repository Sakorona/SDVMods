using NPack;
using System.Collections.Generic;
using System.Linq;

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

        public FerngillClimateTimeSpan(string BeginSeason, string EndSeason, int BeginDay, int EndDay, List<WeatherParameters> wp)
        {
            this.BeginSeason = BeginSeason;
            this.EndSeason = EndSeason;

            this.BeginDay = BeginDay;
            this.EndDay = EndDay;
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

        public double RetrieveOdds(MersenneTwister dice, string weather, int day)
        {
            double Odd = 0;

            List<WeatherParameters> wp = (List<WeatherParameters>)this.WeatherChances.Where(w => w.WeatherType == weather);

            if (wp.Count == 0)
                return 0;

            Odd = wp[0].BaseValue + (wp[0].ChangeRate * day);
            RangePair range = new RangePair(wp[0].VariableLowerBound, wp[0].VariableHigherBound);
            Odd = Odd + range.RollInRange(dice);

            //sanity check.
            if (Odd < 0) Odd = 0;
            if (Odd > 1) Odd = 1;

            return Odd;
        }

        public double RetrieveTemp(MersenneTwister dice, string temp, int day)
        {
            double Temp = 0;
            List<WeatherParameters> wp = (List<WeatherParameters>)this.WeatherChances.Where(w => w.WeatherType == temp);

            if (wp.Count == 0)
                return 0;

            Temp = wp[0].BaseValue + (wp[0].ChangeRate * day);
            RangePair range = new RangePair(wp[0].VariableLowerBound, wp[0].VariableHigherBound);
            Temp = Temp + range.RollInRange(dice);

            return Temp;
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
