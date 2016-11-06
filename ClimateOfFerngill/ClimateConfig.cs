using StardewModdingAPI;

namespace ClimatesOfFerngill
{
    public class ClimateConfig
    {
        public double spgBaseRainChance { get; set; }
        public double smrBaseRainChance { get; set; }
        public double falBaseRainChance { get; set; }

        public double spgRainChanceIncrease { get; set; }
        public double smrRainChanceIncrease { get; set; }
        public double falRainChanceIncrease { get; set; }

        public double spgBaseWindChance { get; set; }
        public double smrBaseWindChance { get; set; }
        public double falBaseWindChance { get; set; }
        public double winBaseWindChance { get; set; }

        public double spgWindChanceIncrease { get; set; }
        public double smrWindChanceIncrease { get; set; }
        public double falWindChanceIncrease { get; set; }
        public double winWindChanceIncrease { get; set; }

        public double spgBaseStormChance { get; set; }
        public double smrBaseStormChance { get; set; }
        public double falBaseStormChance { get; set; }

        public double spgStormChanceIncrease { get; set; }
        public double smrStormChanceIncrease { get; set; }
        public double falStormChanceIncrease { get; set; }

        public double spgConvRainToStorm { get; set; }
        public double smrConvRainToStorm { get; set; }
        public double falConvRainToStorm { get; set; }

        public double winBaseSnowChance { get; set; }
        public double winSnowChanceIncrease { get; set; }

        public bool SuppressLog { get; set; }
        public bool AllowSnowOnFall28 { get; set; }
        public bool HUDDescription { get; set; }
        public bool AllowStormsFirstSpring { get; set; }

        //Future time!
        public bool HarshWeather { get; set; }
        public double baseFlashChance { get; set; }
        public int capFloodRadius { get; set; }
        public double cropWiltChance { get; set; }
        public double strickenFarmerChance { get; set; }
        public bool allowFlashFloodingOnlyWhenRaining { get; set; }
        public bool hardwoodStopFloods { get; set; }

        public ClimateConfig()
        {
            /* Spring Weather:
             * Starts wet and dries out. Storms should follow this, with the wind being fairly constant (but slacking towards the end)
             * 
             * Summer Weather:
             * Not that windy at start, but it gets progressively more windy. Same as rainy, but it will storm more often.
             * 
             * Fall Weather:
             * Halfway through, we should start monsoon season.  Windy. (Too bad we don't have windy rain!)
             * 
             * Winter Weather:
             * Wet (so snowy), not that windy. No storms in winter. */

            //set defaults for spring weather
            spgBaseRainChance = .50; 
            spgRainChanceIncrease = -.0135;

            spgBaseStormChance = .16;
            spgStormChanceIncrease = -.0055;

            spgBaseWindChance = .22;
            spgWindChanceIncrease = -.0025;

            spgConvRainToStorm = .0375; //We generally want very few conversions in spring. 

            //set defaults for summer weather
            smrBaseRainChance = .122;
            smrRainChanceIncrease = .0065;

            smrBaseStormChance = .006;
            smrStormChanceIncrease = .0099;

            smrConvRainToStorm = .5000; //half chance for rain to become storm.

            //set defaults for autumn weather
            falBaseRainChance = .304;
            falRainChanceIncrease = .0165;

            falBaseStormChance = .2832;
            falStormChanceIncrease = .0099;

            falBaseWindChance = .243;
            falWindChanceIncrease = .0175;

            falConvRainToStorm = .0095;

            //set default for winter weather
            winBaseWindChance = .65;
            winWindChanceIncrease = -.012;

            winBaseSnowChance = .766;
            winSnowChanceIncrease = .005;

            AllowSnowOnFall28 = true;
            AllowStormsFirstSpring = false;
            HUDDescription = true;
            SuppressLog = false;

            HarshWeather = false; //by default, turn the harsh events off. 
            baseFlashChance = .005; // it's .5% - luck, and should corrospondingly trigger rarely. Multiple raining days increase the chance
            allowFlashFloodingOnlyWhenRaining = true;
            cropWiltChance = .3; //every crop has a base of 30% - luck chance to wilt. This gives a variant of [.23,.37] by default
            capFloodRadius = 4; //stops the flood from totally wrecking riverlands. Theoretically.
            strickenFarmerChance = .15; //right now this applies to storms too. Do not make this a high chance. >_> Should I seperate these?
            hardwoodStopFloods = false; //it's not really a wall.
            //should we cap and min cap these?
        }
    }
}
