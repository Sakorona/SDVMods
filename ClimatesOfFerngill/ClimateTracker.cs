namespace ClimatesOfFerngillRebuild
{
    public class ClimateTracker
    {
        public int DaysSinceRainedLast { get; set;} = 0;
        public int AmtOfRainInCurrentStreak { get; set;}
        public long AmtOfRainSinceDay1 { get; set;}

        public ClimateTracker()
        {
            DaysSinceRainedLast = 0;
            AmtOfRainInCurrentStreak = 0;
            AmtOfRainSinceDay1 = 0;
        }

        public ClimateTracker(int daysSinceLast, int amtInStreak, long TotalRain)
        {
            DaysSinceRainedLast = daysSinceLast;
            AmtOfRainInCurrentStreak = amtInStreak;
            AmtOfRainSinceDay1 = TotalRain;
        }

        public ClimateTracker(ClimateTracker c)
        {
            DaysSinceRainedLast = c.DaysSinceRainedLast;
            AmtOfRainInCurrentStreak = c.AmtOfRainInCurrentStreak;
            AmtOfRainSinceDay1 = c.AmtOfRainSinceDay1;
        }
    }
}
