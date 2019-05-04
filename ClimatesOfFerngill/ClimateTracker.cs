namespace ClimatesOfFerngillRebuild
{
    class ClimateTracker
    {
        public int DaysSinceRainedLast { get; set;} = 0;

        public ClimateTracker()
        {
            DaysSinceRainedLast = 0;
        }
    }
}
