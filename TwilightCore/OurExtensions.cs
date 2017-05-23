using System;
using NPack;

namespace TwilightCore
{
    public static class TwilightExtensions
    {
        public static string GetRandomItem(this string[] array, Random r)
        {
            int l = array.Length;

            return array[r.Next(l)];
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

        public static bool Contains(this int[] array, int val)
        {
            foreach (int i in array)
            {
                if (val == i)
                    return true;
            }

            return false;
        }

        public static double RollInRange(this RangePair s, MersenneTwister d)
        {
            if (s.HigherBound == s.LowerBound)
                return s.LowerBound;

            return (d.NextDoublePositive() * (s.HigherBound - s.LowerBound) + s.LowerBound);
        }
    }
}
