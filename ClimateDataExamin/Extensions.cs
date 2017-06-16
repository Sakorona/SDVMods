using System;

namespace ClimateDataExaminer
{
    public static class TwilightExtensions
    {
        public static string GetRandomItem(this string[] array, Random r)
        {
            int l = array.Length;

            return array[r.Next(l)];
        }

        public static bool Contains<T>(this T[] array, T val)
        {
            foreach (T i in array)
            {
                if (val.Equals(i))
                    return true;
            }

            return false;
        }

        public static bool IsBetween(this int val, int lowBound, int highBound)
        {
            if (val > lowBound && val < highBound)
                return true;

            return false;
        }

        public static bool IsBetweenInc(this int val, int lowBound, int highBound)
        {
            if (val >= lowBound && val <= highBound)
                return true;

            return false;
        }
    }
}
