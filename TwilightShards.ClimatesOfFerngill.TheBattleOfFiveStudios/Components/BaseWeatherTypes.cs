using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilightShards.ClimatesOfFerngill.Components
{
    /// <summary>
    /// This enum tracks the base weather
    /// </summary>
    [Flags]
    public enum BaseWeathers
    {
        Unset = 0,
        Sunny = 2,
        NormalRain = 4,
        Snow = 8,
        Wind = 16,
        Festival = 32,
        Wedding = 64,
        Lightning = 128,
        Blizzard = 256,
        WhiteOut = 512,
        ThunderFrenzy = 1024,
        Flooding = 2048
    }
}