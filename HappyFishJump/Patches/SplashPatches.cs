using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Buildings;

namespace HappyFishJump
{
    [HarmonyPatch(typeof(JumpingFish), nameof(JumpingFish.Splash))]
    class SplashPatches
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("playSound"))
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method( typeof( HappyFishJump ), nameof(HappyFishJump.PlaySound) );
                }             
            }
            return codes.AsEnumerable();
        }
    }
}