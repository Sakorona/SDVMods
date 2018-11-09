using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TwilightShards.LunarDisturbances;

namespace LunarDisturbances.Patches
{
    public static class ShippingMenuPatches
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            bool StopLoop = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Bne_Un)
                {
                    codes[i].opcode = OpCodes.Ble_Un_S;
                    for (int j = i; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Ldsfld && codes[j].operand.ToString().Contains("mouseCursors"))
                        {
                            codes[j].operand = AccessTools.Field(typeof(Sprites.Icons), "MoonSource");
                            var insertPoint = j + 8;
                            Console.WriteLine($"ip: {insertPoint}, codes is {codes}, MoonSource is {TwilightShards.LunarDisturbances.LunarDisturbances.OurIcons}");
                            codes[insertPoint].operand =
                                TwilightShards.LunarDisturbances.LunarDisturbances.OurIcons.GetNightMoonSprite(MoonPhase.WaxingGibbeous).X;
                           insertPoint++;
                            codes[insertPoint].operand =
                                TwilightShards.LunarDisturbances.LunarDisturbances.OurIcons.GetNightMoonSprite(MoonPhase.WaxingGibbeous).Y;
                            insertPoint++;
                            codes[insertPoint].operand =
                                TwilightShards.LunarDisturbances.LunarDisturbances.OurIcons.GetNightMoonSprite(MoonPhase.WaxingGibbeous).Width;
                            insertPoint++;
                            codes[insertPoint].operand =
                                TwilightShards.LunarDisturbances.LunarDisturbances.OurIcons.GetNightMoonSprite(MoonPhase.WaxingGibbeous).Height;
                            StopLoop = true;
                        }

                        if (StopLoop)
                            break;
                    }
                }
                if (StopLoop)
                    break;
            }
            return codes.AsEnumerable();
        }
    }
}
