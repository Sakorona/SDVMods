using System.Collections.Generic;

namespace ClimateOfFerngill
{
    public class FerngillClimateTimeSpan
    {
        public string Season;
        public int BeginDay;
        public int EndDay;

        public bool FrozenPrecip;

        public double LowTempBase;
        public double LowTempChange;
        public double LowTempVariable;

        public double HighTempBase;
        public double HighTempChange;
        public double HighTempVariable;

        public double BaseRainChance;
        public double RainChange;
        public double RainVariability;
        public double BaseStormChance;
        public double StormChange;
        public double StormVariability;
        public double BaseDebrisChance;
        public double DebrisChange;

        public bool AllowDebris;

        public FerngillClimateTimeSpan()
        {

        }

        public FerngillClimateTimeSpan(bool FrozenPrecip, bool AllowDebris, string Season, int BeginDay, int EndDay, 
            double LowTempBase, double LowTempChange, double LowTempVariable, double HighTempBase, double HighTempChange,
            double HighTempVariable, double BaseRainChance, double RainChange, double RainVariability, double BaseStormChance,
            double StormChange, double StormVariability, double BaseDebrisChance, double DebrisChange)
        {
            this.AllowDebris = AllowDebris;
            this.FrozenPrecip = FrozenPrecip;

            this.Season = Season;
            this.BeginDay = BeginDay;
            this.EndDay = EndDay;

            this.LowTempBase = LowTempBase;
            this.LowTempChange = LowTempChange;
            this.LowTempVariable = LowTempVariable;

            this.HighTempBase = HighTempBase;
            this.HighTempChange = HighTempChange;
            this.HighTempVariable = HighTempVariable;

            this.BaseRainChance = BaseRainChance;
            this.RainChange = RainChange;
            this.RainVariability = RainVariability;

            this.BaseStormChance = BaseStormChance;
            this.StormChange = StormChange;
            this.StormVariability = StormVariability;

            this.BaseDebrisChance = BaseDebrisChance;
            this.DebrisChange = DebrisChange;
        }
    }

    public class FerngillClimate
    {
        public List<FerngillClimateTimeSpan> ClimateSequences;

        public FerngillClimate()
        {
            ClimateSequences = new List<FerngillClimateTimeSpan>();
        }
    }
}
