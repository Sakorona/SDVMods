namespace TwilightShards.LunarDisturbances
{
    public class MoonConfig
    {
        //required options
        public double BadMoonRising { get; set; }
        public bool EclipseOn { get; set; }
        public double EclipseChance { get; set; }
        public bool SpawnMonsters { get; set; }
        public bool SpawnMonstersAllFarms { get; set; }
        public bool HazardousMoonEvents { get; set; }
        public bool Verbose { get; set; }
        
        public MoonConfig()
        {
            // be able to deal with lightning strikes
            //general mod options
            Verbose = true;

            //eclipse stuff
            EclipseOn = true;
            EclipseChance = .015;
            SpawnMonsters = true;
            SpawnMonstersAllFarms = false;
            HazardousMoonEvents = false;
        }
    }
}
