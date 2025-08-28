using System.Collections.Generic;
using System.Reflection.Emit;
using Game.Systems.Space;
using HarmonyLib;

namespace Infiniverse.Patches;

public static class GenSpacePatches
{
    [HarmonyPatch(typeof(GenSpace), nameof(GenSpace.Generate))]
    public static class GeneratePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode == OpCodes.Ldloc_S)
                {
                    if (codes[i].operand is LocalBuilder builder && builder.LocalIndex == 5)
                    {
                        var labels = codes[i].labels;
                        codes.RemoveAt(i);
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_S, 0).WithLabels(labels));
                        break;
                    }
                }
            }
            return codes;
        }
    }
}