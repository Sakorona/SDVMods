namespace TwilightCore.PRNG
{
    public static class MTwisterExtensions
    {
        public static double RollInRange(this RangePair s, MersenneTwister d)
        {
            if (s.HigherBound == s.LowerBound)
                return s.LowerBound;

            return (d.NextDoublePositive() * (s.HigherBound - s.LowerBound) + s.LowerBound);
        }

        public static string GetRandomItem(this string[] array, MersenneTwister mt)
        {
            int l = array.Length;

            return array[mt.Next(l - 1)];
        }

        public static int GetRandomItem(this int[] array, MersenneTwister mt)
        {
            int l = array.Length;

            return array[mt.Next(l - 1)];
        }

    }
}
