using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace ClimatesOfFerngillRebuild
{
    class FerngillThunderFrenzy : ISDVWeather
    {
        public string WeatherType => "ThunderFrenzy";

        private SDVTime BeginTime;
        private SDVTime ExpirTime;
        private bool IsThorAngry;

        public event EventHandler<WeatherNotificationArgs> OnUpdateStatus;
        public SDVTime WeatherExpirationTime => (ExpirTime ?? new SDVTime(0600));
        public SDVTime WeatherBeginTime => (BeginTime ?? new SDVTime(0600));
        public void SetWeatherExpirationTime(SDVTime t) => ExpirTime = t;
        public void SetWeatherBeginTime(SDVTime t) => BeginTime = t;
        public bool WeatherInProgress => (SDVTime.CurrentTime >= BeginTime && SDVTime.CurrentTime <= ExpirTime);
        public bool IsWeatherVisible => IsThorAngry;
        private MersenneTwister Dice;
        private WeatherConfig ModConfig;

        /// <summary> Default constructor. </summary>
        internal FerngillThunderFrenzy(MersenneTwister Dice, WeatherConfig config)
        {
            this.Dice = Dice;
            this.ModConfig = config;
        }

        public void SetWeatherTime(SDVTime begin, SDVTime end)
        {
            BeginTime = new SDVTime(begin);
            ExpirTime = new SDVTime(end);
        }

        public void OnNewDay()
        {
            IsThorAngry = false;
        }

        public void Reset()
        {
            IsThorAngry = false;
        }

        public void UpdateStatus(string weather, bool status)
        {
            if (OnUpdateStatus == null) return;

            WeatherNotificationArgs args = new WeatherNotificationArgs(weather, status);
            OnUpdateStatus(this, args);
        }

        public void MoveWeather()
        {
        }

        public void DrawWeather()
        {
        }

        public void CreateWeather()
        {
            //set the begin and end time
            SDVTime stormStart = new SDVTime(1150 + (Dice.Next(0, 230)));
            stormStart.ClampToTenMinutes();

            BeginTime = new SDVTime(stormStart);

            stormStart.AddTime(Dice.Next(30, 190));
            stormStart.ClampToTenMinutes();
            ExpirTime = new SDVTime(stormStart);
        }

        public void UpdateWeather()
        {
            if (WeatherBeginTime is null || WeatherExpirationTime is null)
                return;

            if (WeatherInProgress && !IsWeatherVisible)
            {
                IsThorAngry = true;
                UpdateStatus(WeatherType, true);
            }

            if (WeatherExpirationTime <= SDVTime.CurrentTime && IsWeatherVisible)
            {
                IsThorAngry = false;
                UpdateStatus(WeatherType, false);
            }

            if (IsWeatherVisible)
            {
                BREAKEVERYTHINGUNTILYOUWRITEME();
            }

        }

        public void EndWeather()
        {
            if (IsWeatherVisible)
            {
                ExpirTime = new SDVTime(SDVTime.CurrentTime - 10);
                UpdateStatus(WeatherType, false);
            }
        }
    }
}
