using TwilightShards.Stardew.Common;

namespace DynamicNightTime.Patches
{
    class GettingDarkPatch
    {
        public static void Postfix(ref int __result)
        {
            //SDVTime calcTime = DynamicNightTime.GetSunset();
            SDVTime calcTime = new SDVTime(18,00);
            calcTime.ClampToTenMinutes();

            __result = calcTime.ReturnIntTime();
        }
    }
}
