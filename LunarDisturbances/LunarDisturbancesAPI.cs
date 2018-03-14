namespace TwilightShards.LunarDisturbances
{
    public interface ILunarDisturbancesAPI
    {
        string GetCurrentMoonPhase();
        bool IsSolarEclipse();
    }

    public class LunarDisturbancesAPI : ILunarDisturbancesAPI
    {
        private SDVMoon IntMoon;
        private bool IsEclipse;

        public LunarDisturbancesAPI(SDVMoon OurMoon, bool IsEcl)
        {
            IntMoon = OurMoon;
            IsEclipse = IsEcl;
        }

        public string GetCurrentMoonPhase()
        {
            return IntMoon.DescribeMoonPhase();
        }

        public bool IsSolarEclipse()
        {
            return IsEclipse;
        }
    }
}

