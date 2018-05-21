namespace ClimatesOfFerngillRebuild.Integrations
{
    public interface ILunarDisturbancesAPI
    {
        string GetCurrentMoonPhase();
        bool IsSolarEclipse();
    }
}
