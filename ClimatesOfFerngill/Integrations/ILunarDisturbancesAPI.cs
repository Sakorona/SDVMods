using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimatesOfFerngillRebuild.Integrations
{
    public interface ILunarDisturbancesAPI
    {
        string GetCurrentMoonPhase();
        bool IsSolarEclipse();
    }
}
