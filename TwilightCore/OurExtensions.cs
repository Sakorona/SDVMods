using System;

namespace TwilightCore
{
    public static class TwilightExtensions
    {
        public static string GetRandomItem(this string[] array, Random r)
        {
            int l = array.Length;

            return array[r.Next(l)];
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
