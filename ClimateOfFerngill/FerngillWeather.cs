namespace ClimateOfFerngill
{
    public class FerngillWeather
    {
        private double TodayHigh { get; set; }
        private double TodayLow { get; set; }
        private SDVWeather CurrentWeather { get; set; }
        private ClimateConfig Config { get; set; }

        public bool IsBlizzard { get; private set; }
        public bool IsHeatwave { get; private set; }
        public bool IsFrost { get; private set; }

        public FerngillWeather(ClimateConfig config)
        {
            IsBlizzard = false;
            IsHeatwave = false;
            IsFrost = false;
            Config = config;
        }

        public bool IsDangerousWeather()
        {
            if (IsBlizzard || IsFrost || IsHeatwave)
                return true;
            else
                return false;
        }

        public string GetHazardMessage()
        {
            string areasAffected = " Areas affected include Zuzu City, Pelican Town...";
            if (IsBlizzard)
                return "FRWS Warning: There's a dangerous blizzard out today." + areasAffected;
            if (IsHeatwave)
                return "FRWS Warning: An unnatural heatwave is affecting the region." + areasAffected;
            if (IsFrost)
                return "FRAS Warning: Temepratures in your region will be dipping below the frost threshold. Your crops will be vulnerable.";

            return "";
        }

        public void MessageForDangerousWeather()
        {
            if (IsBlizzard) InternalUtility.ShowMessage("There's a dangerous blizzard out today. Be careful!");
            if (IsFrost) InternalUtility.ShowMessage("The temperature tonight will be dipping below freezing. Your crops may be vulnerable to frost!");
            if (IsHeatwave) InternalUtility.ShowMessage("A massive heatwave is sweeping the valley. Stay hydrated!");
        }

        public void SetTemperatures(double high, double low)
        {
            TodayHigh = high;
            TodayLow = low;
        }

        public void SetTodayHigh(double high)
        {
            TodayHigh = high;
        }

        public void SetTodayLow(double low)
        {
            TodayLow = low;
        }

        public double GetTodayHigh()
        {
            return this.TodayHigh;
        }

        public double GetTodayLow()
        {
            return this.TodayLow;
        }

        public bool SetBlizzard()
        {
            if (CurrentWeather == SDVWeather.Snow)
            {
                IsBlizzard = true;
                return true;
            }

            return false;
        }

        public bool SetHeatwave()
        {
            if (TodayHigh > Config.HeatwaveWarning)
            {
                IsHeatwave = true;
                return true;
            }

            return false;
        }

        public bool SetFrost()
        {
            if (TodayLow < Config.FrostWarning)
            {
                IsFrost = true;
                return true;
            }

            return false;
        }

        public void AlterTemps(double temp)
        {
            TodayHigh = TodayHigh + temp;
            TodayLow = TodayLow + temp;
        }

        public void GetLowFromHigh(double temp)
        {
            TodayLow = TodayHigh - temp;
        }

        public void UpdateForNewDay()
        {
            Reset();
        }

        public void Reset()
        {
            IsBlizzard = false;
            IsHeatwave = false;
            IsFrost = false;
            CurrentWeather = SDVWeather.None;
            TodayHigh = -1000;
            TodayLow = -1000;
        }

        public override string ToString()
        {
            string s = "High: " + TodayHigh + " C and Low: " + TodayLow + " C, with status " + CurrentWeather.ToString();

            if (IsBlizzard)
                s += " . There's a blizzard out";

            if (IsFrost)
                s += " . There's a high chance of a frost tonight";

            if (IsHeatwave)
                s += " . It's very hot outside, expect a good chance of a heatwave.";

            return s;
        }
    }
}
