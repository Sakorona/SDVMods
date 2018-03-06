using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilightShards.LunarDisturbances
{
    public interface ILunarDisturbances
    {
        string GetCurrentMoonPhase();
    }

    public class LunarDisturbancesAPI : ILunarDisturbances
    {
    
    }
}

