using System;

namespace TwilightShards.Common
{
    class GeneralFunctions
    {
        public static string FirstLetterToUpper(string str)
        {
            if (String.IsNullOrEmpty(str))
                throw new ArgumentException("ARGH!");

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static double ConvCtF(double temp) => ((temp * 1.8) + 32);
        public static double ConvFtC(double temp) => ((temp - 32) / 1.8);
        public static double ConvCtK(double temp) => (temp + 273.15);
        public static double ConvFtK(double temp) => ((temp + 459.67) * (5.0 / 9.0));
        public static double ConvKtC(double temp) => (temp - 273.15);
        public static double ConvKtF(double temp) => ((temp * 1.8) - 459.67);

        public static bool ContainsOnlyMatchingFlags(Enum enumType, int FlagVal)
        {
            int enumVal = Convert.ToInt32(enumType);
            if (enumVal == FlagVal)
            {
                return true;
            }

            return false;
        }

        public static double DegreeToRadians(double deg) => deg * (Math.PI / 180);
        public static double RadiansToDegree(double rad) => (rad * 180) / Math.PI;

        public static string ConvMinToHrMin(double val)
        {
            int hour = (int)Math.Floor(val / 60.0);
            double min = val - (hour * 60);

            return $"{hour}h{min.ToString("00")}m";
        }
    }
}
