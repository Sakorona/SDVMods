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
        public double LowTempVariableLowerBound;
        public double LowTempVariableHigherBound;

        public double HighTempBase;
        public double HighTempChange;
        public double HighTempVariableLowerBound;
        public double HighTempVariableHigherBound;

        public double BaseRainChance;
        public double RainChange;
        public double RainVariabilityLowerBound;
        public double RainVariabilityHigherBound;

        public double BaseStormChance;
        public double StormChange;
        public double StormVariabilityLowerBound;
        public double StormVariabilityHigherBound;

        public double BaseDebrisChance;
        public double DebrisChange;
        public double DebrisVariabilityLowerBound;
        public double DebrisVariabilityHigherBound;

        public double BaseSnowChance;
        public double SnowChange;
        public double SnowVariabilityLowerBound;
        public double SnowVariabilityHigherBound;

        public double FogChance;
        public double FogVariabilityLowerBound;
        public double FogVariabilityHigherBound;

        public FerngillClimateTimeSpan()
        {

        }

        public FerngillClimateTimeSpan(bool FrozenPrecip, string Season, int BeginDay, int EndDay, 
            double LowTempBase, double LowTempChange, double LowTempVariableLowerBound, double HighTempBase, double HighTempChange,
            double HighTempVariableLowerBound, double BaseRainChance, double RainChange, double RainVariabilityLowerBound, 
            double BaseStormChance, double StormChange, double StormVariabilityLowerBound, double BaseDebrisChance, double DebrisChange, 
            double DebrisVariabilityLowerBound, double BaseSnowChance, double SnowChange, double SnowVariabilityLowerBound, double FogChance, 
            double FogVariabilityLowerBound, double FogVariabilityHigherBound, double LowTempVariableHigherBound, double HighTempVariableHigherBound,
            double RainVariabilityHigherBound, double SnowVariabilityHigherBound, double DebrisChanceHigherBound)
        {
            this.FrozenPrecip = FrozenPrecip;

            this.Season = Season;
            this.BeginDay = BeginDay;
            this.EndDay = EndDay;

            this.LowTempBase = LowTempBase;
            this.LowTempChange = LowTempChange;
            this.LowTempVariableLowerBound = LowTempVariableLowerBound;

            this.HighTempBase = HighTempBase;
            this.HighTempChange = HighTempChange;
            this.HighTempVariableLowerBound = HighTempVariableLowerBound;

            this.BaseRainChance = BaseRainChance;
            this.RainChange = RainChange;
            this.RainVariability = RainVariability;

            this.BaseStormChance = BaseStormChance;
            this.StormChange = StormChange;
            this.StormVariability = StormVariability;

            this.BaseDebrisChance = BaseDebrisChance;
            this.DebrisChange = DebrisChange;
            this.DebrisVariability = DebrisVariability;

            this.BaseSnowChance = BaseSnowChance;
            this.SnowChange = SnowChange;
            this.SnowVariability = SnowVariability;

            this.FogChance = FogChance;
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
