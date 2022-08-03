namespace FerngillCustomWeathers
{
    public class WeatherConfig
    {
        public double FogChanceInEarlySpring { get; set; } = .40;
        public double FogChanceInLateSpring { get; set; }= .20;
        public double FogChanceInEarlySummer { get; set; }= .10;
        public double FogChanceInLateSummer { get; set; } = .15;
        public double FogChanceInEarlyFall { get; set; }= .60;
        public double FogChanceInLateFall { get; set; } = .50;
        public double FogChanceInEarlyWinter { get; set; } = .20;
        public double FogChanceInLateWinter { get; set; } = .20;

        public bool UseLighterFog { get; set; } = false;
        public bool DisplayFogInTheDesert { get; set; } = false;
        public double EveningWeatherFogChance { get; set; } = .35;
    }
}
