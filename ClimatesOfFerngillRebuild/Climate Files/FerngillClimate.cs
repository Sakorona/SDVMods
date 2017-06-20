using System.Collections.Generic;
using TwilightCore;
using TwilightCore.PRNG;
using TwilightCore.StardewValley;

namespace ClimatesOfFerngillRebuild
{
        public class FerngillClimate
        {
            public bool AllowRainInWinter;
            public List<FerngillClimateTimeSpan> ClimateSequences;

            //constructor
            public FerngillClimate()
            {
                ClimateSequences = new List<FerngillClimateTimeSpan>();
            }

            //constructor
            public FerngillClimate(List<FerngillClimateTimeSpan> fCTS)
            {
                ClimateSequences = new List<FerngillClimateTimeSpan>();
                foreach (FerngillClimateTimeSpan CTS in fCTS)
                    this.ClimateSequences.Add(new FerngillClimateTimeSpan(CTS));
            }

            //climate access functions
            public FerngillClimateTimeSpan GetClimateForDate(SDVDate Target)
            {
                foreach (FerngillClimateTimeSpan s in ClimateSequences)
                {
                    SDVDate BeginDate = new SDVDate(s.BeginSeason, s.BeginDay);
                    SDVDate EndDate = new SDVDate(s.EndSeason, s.EndDay);

                    if (Target.IsBetweenInc(BeginDate, EndDate))
                        return s;
                }

                return default(FerngillClimateTimeSpan);
            }
        }
}

