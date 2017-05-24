using System;

namespace TwilightCore.StardewValley
{
    public class SDVTime
    {
        int hour;
        int minute;

        public SDVTime(int t)
        {
            hour = t / 100;

            if (hour >= 26)
                throw new ArgumentOutOfRangeException("Invalid Time passed to the constructor");

            t = t - (hour * 100);

            if (t < 60)
                minute = t;
            else
            {
                hour++;
                if (hour >= 26)
                    throw new ArgumentOutOfRangeException("Invalid Time passed to the constructor");
                minute = t - 60;
            }
        }

        public SDVTime(int h, int m)
        {
            hour = h;
            if (hour > 24)
                throw new Exception("Invalid Time passed to the constructor");

            minute = m;
        }

        public SDVTime(SDVTime c)
        {
            hour = c.hour;
            minute = c.minute;
        }

        public void SubtractTime(int time)
        {
            int subhr = time / 60;
            int submin = time - (60 * subhr);

            hour = hour - subhr;
            minute = minute - submin;

            if (minute < 0)
            {
                hour--;
                minute = 60 + minute;
            }
        }

        public void SubtractTime(SDVTime sTime)
        {
            hour = hour - sTime.hour;
            minute = minute - sTime.minute;

            if ( minute < 0)
            {
                hour--;
                minute = 60 + minute;
            }

            if (hour < 0)
                hour = hour + 24;

        }

        public void AddTime(int time)
        {
            int addhr = time / 60;
            int addmin = time - (60 * addhr);

            hour = hour + addhr;
            minute = minute + addmin;
            while (minute > 59)
            {
                hour++;
                minute -= 60;
            }
        }

        public void AddTime(SDVTime sTime)
        {
            hour = hour + sTime.hour;
            minute = minute + sTime.minute;

            while (minute > 59)
            {
                hour++;
                minute -= 60;
            }

            if (hour >= 26)
            {
                hour = hour - 24;
            }
        }
        //operator functions
        public static SDVTime operator +(SDVTime s1, SDVTime s2)
        {
            SDVTime ret = new SDVTime(s1);
            ret.AddTime(s2);
            return ret;
        }

        public static SDVTime operator -(SDVTime s1, SDVTime s2)
        {
            SDVTime ret = new SDVTime(s1);
            ret.SubtractTime(s2);
            return ret;
        }

        public static SDVTime operator -(SDVTime s1, int time)
        {
            SDVTime ret = new SDVTime(s1);
            ret.SubtractTime(time);
            return ret;
        }

        public static SDVTime operator +(SDVTime s1, int time)
        {
            SDVTime ret = new SDVTime(s1);
            ret.AddTime(time);
            return ret;
        }

        //description and return functions
        public int ReturnIntTime()
        {
            return (hour * 100) + minute;
        }

        public static bool VerifyValidIntTime(int time)
        {
            //basic bounds first
            if (time < 0600 || time > 2600)
                return false;
            if ((time % 100) > 59)
                return false;

            return true;
        }

        public override string ToString()
        {
            if (hour < 24)
                return $"{hour.ToString().PadLeft(2,'0')}{minute.ToString().PadLeft(2, '0')}";
            else
                return $"{(hour - 24).ToString().PadLeft(2,'0')}{minute.ToString().PadLeft(2, '0')}";
        }
    }
}
