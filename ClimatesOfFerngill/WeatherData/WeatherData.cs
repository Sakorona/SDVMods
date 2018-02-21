namespace ClimatesOfFerngillRebuild
{
    public class WeatherData
    {
        WeatherIcon Icon { get; set; }
        WeatherIcon IconBasic { get; set; }
        string ConditionName { get; set; }
        string ConditionDescDay { get; set; }
        string ConditionDescNight { get; set; }
        bool IsSpecialWeather { get; set; }
        
        public WeatherData()
        {

        }

        public WeatherData(WeatherIcon Icon, WeatherIcon IconBasic, string CondName, string CondDesc, bool IsSpecial = false, string CondDescNight = null)
        {
            this.Icon = Icon;
            this.IconBasic = IconBasic;
            ConditionName = CondName;
            ConditionDescDay = CondDesc;
            IsSpecialWeather = IsSpecial;
            ConditionDescNight = CondDescNight;
        }

        public string GetConditionString(bool IsNight)
        {
            return IsNight && !string.IsNullOrEmpty(ConditionDescNight) ? ConditionDescNight : ConditionDescDay;
        }
    }
}
