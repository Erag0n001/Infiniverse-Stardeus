using Game.Systems.Space;
using HarmonyLib;

namespace Infiniverse.Patches;

public static class GenSpaceBiomePatch
{
    [HarmonyPatch(typeof(GenSpaceBiome), "RemapDifficulty")]
    public static class RemapDifficultyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, float diff)
        {
            __result = (diff + 1f) * 0.5f;
            return false;
        }
    }
}