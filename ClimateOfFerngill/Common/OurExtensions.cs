using System;
using NPack;

namespace ClimateOfFerngill
{
    public static class OurExtensions
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
    }
}
