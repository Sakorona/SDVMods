namespace FerngillDynamicRainAndWind
{
    public class FDRAWConfig
    {
        public double VariableRainChance { get; set; } = .25;
        public double RainChangeChance { get; set; } = .10;
        public double RainChangeAmt { get; set; } = .15;
        public double ChanceOfIncrease { get; set; } = .5;
        public int NumberOfTenMinutes { get; set; } = 3;
    }
}