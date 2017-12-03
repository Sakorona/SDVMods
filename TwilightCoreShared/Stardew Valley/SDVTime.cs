using StardewValley;
using System;

namespace TwilightShards.Stardew.Common
{
    public enum SDVTimeOptions
    {
        TimeNormal,
        TimePlayable
    }

    public enum SDVTimePeriods
    {
        Morning,
        Afternoon,
        Evening,
        Night,
        LateNight
    }

    public class SDVTime
    {
        public static bool IsNight {
            get
            {
                if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime())
                    return true;
                else
                    return false;
            }
        }

        /* public SDVTimePeriods CurrentPeriod
        {
            get
            {

            }
        } */

        public static SDVTime CurrentTime => new SDVTime(Game1.timeOfDay);
        public static int CurrentIntTime => new SDVTime(Game1.timeOfDay).ReturnIntTime();

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
                if (hour >= 28)
                    throw new ArgumentOutOfRangeException("Invalid Time passed to the constructor");
                minute = t - 60;
            }
        }

        public SDVTime(int h, int m)
        {
            hour = h;
            if (hour > 28)
                throw new Exception("Invalid Time passed to the constructor");

            minute = m;

            if (m >= 60)
                throw new ArgumentOutOfRangeException("There are only 60 minutes in an hour.");
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

            if (hour > 28)
                hour = hour - 24;

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

            if (hour >= 28)
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

        public static bool operator ==(SDVTime s1, SDVTime s2)
        {
            if ((s1.hour == s2.hour) && (s1.minute == s2.minute))
                return true;
            else
                return false;
        }

        public static bool operator !=(SDVTime s1, SDVTime s2)
        {
            if ((s1.hour == s2.hour) && (s1.minute == s2.minute))
                return false;
            else
                return true;
        }

        public static bool operator ==(SDVTime s1, int s2)
        {
            int intHour = s2 / 100;
            int intMinute = s2 % 100;

            if ((s1.hour == (s2 / 100)) && (s1.minute == (s2 % 100)))
                return true;
            else
                return false;
        }
        
        public static bool operator !=(SDVTime s1, int s2)
        {
            return !(s1 == s2);
        }

        public static bool operator >(SDVTime s1, SDVTime s2)
        {
            if (s1.hour > s2.hour)
                return true;
            else if (s1.hour == s2.hour && s1.minute > s2.minute)
                return true;

            return false;
        }

        public static bool operator <(SDVTime s1, SDVTime s2)
        {
            if (s1.hour < s2.hour)
                return true;
            else if (s1.hour == s2.hour && s1.minute < s2.minute)
                return true;

            return false;
        }

        public static bool operator >=(SDVTime s1, SDVTime s2)
        {
            if (s1 == s2)
                return true;
            if (s1.hour > s2.hour)
                return true;
            else if (s1.hour == s2.hour && s1.minute > s2.minute)
                return true;

            return false;
        }

        public static bool operator <=(SDVTime s1, SDVTime s2)
        {
            if (s1 == s2)
                return true;
            if (s1.hour < s2.hour)
                return true;
            else if (s1.hour == s2.hour && s1.minute < s2.minute)
                return true;

            return false;
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

        public string Get12HourTime()
        {
            if (hour < 12)
                return $"{hour.ToString().PadLeft(2, '0')}:{minute.ToString().PadLeft(2, '0')} am";
            else if (hour >= 12 && hour < 24)
                return $"{(hour - 12).ToString().PadLeft(2, '0')}:{minute.ToString().PadLeft(2, '0')} pm";
            else if (hour > 24)
                return $"{(hour - 24).ToString().PadLeft(2, '0')}:{minute.ToString().PadLeft(2, '0')} am";

            return "99:99 99";
        }

        public override string ToString()
        {
            if (hour < 24)
                return $"{hour.ToString().PadLeft(2,'0')}:{minute.ToString().PadLeft(2, '0')}";
            else
                return $"{(hour - 24).ToString().PadLeft(2,'0')}:{minute.ToString().PadLeft(2, '0')}";
        }

        public override bool Equals(object obj)
        {
            SDVTime time = (SDVTime)obj;

            return time != null &&
                   hour == time.hour &&
                   minute == time.minute;
        }

        public bool Equals(SDVTime other)
        {
            return other != null &&
                   hour == other.hour &&
                   minute == other.minute;
        }

        public override int GetHashCode()
        {
            var hashCode = -1190848304;
            hashCode = hashCode * -1521134295 + hour.GetHashCode();
            hashCode = hashCode * -1521134295 + minute.GetHashCode();
            return hashCode;
        }
    }
}

